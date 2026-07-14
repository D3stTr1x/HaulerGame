using System.Drawing;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class NavArrow : MonoBehaviour
{
    [Header("Ссылки на объекты")]
    public Transform player;
    public RawImage uiArrow;

    [Header("Массивы точек")]
    [Tooltip("Точки, где спавнятся коробки")]
    public Transform[] pickupPoints;

    [Tooltip("Зоны, куда нужно отвезти груз")]
    public Transform[] deliveryPoints;

    [Header("Настройки")]
    public float pathUpdateInterval = 0.5f;

    [Tooltip("Текущий режим: true - едем разгружаться, false - едем за коробками")]
    public bool isDelivering = false;

    private NavMeshPath _path;
    private Transform _currentTarget;
    //private float _timer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        _path = new NavMeshPath();
        GetCargoSpawnZones();
        isDelivering = false;

        //UnsetTarget();
    }
    void Update()
    {
        if (!isDelivering)
        { 
            FindNearestCargoZone();
            CalculateRoute();
            UpdateArrowDirection();
        }
        else
        {
            if (_currentTarget != null)
            {
                CalculateRoute();
                UpdateArrowDirection();
            }
        }
    }
    void GetDeliveryPoints()
    {
        ZoneSpawner zoneSpawner = FindFirstObjectByType<ZoneSpawner>();
        zoneSpawner.GetDeliveryZones(out deliveryPoints);
        if (deliveryPoints != null) Debug.Log($"delPoints: {deliveryPoints.Length} (arrow nav)");
    }
    void GetCargoSpawnZones()
    {
        CargoSpawner zoneSpawner = FindFirstObjectByType<CargoSpawner>();
        //zoneSpawner.GetActiveCargoZones(out pickupPoints);
        zoneSpawner.GetCargoZones(out pickupPoints);
        if (pickupPoints != null) Debug.Log($"pickPoints: {pickupPoints.Length} (arrow nav)");
    }
    public void SetAsTarget(GameObject target)
    {
        _currentTarget = target.transform;
        Debug.Log($"curTarget: {_currentTarget}");
    }
    public void UnsetTarget()
    {
        _currentTarget = null;
        uiArrow.gameObject.SetActive(false);
    }
    public void SetDeliveryMode(bool del)
    {
        if (del != isDelivering) UnsetTarget();
        isDelivering = del;
    }
    public Transform GetTarget()
    {
        return _currentTarget;
    }
    public void FindNearestCargoZone()
    {
        if (isDelivering) return;
        
        //GetCargoSpawnZones(); //?here?
        if (pickupPoints.Length == 0 || pickupPoints == null) return;
      

        float minDistance = Mathf.Infinity;
        Transform nearestPoint = null;

        foreach (Transform pt in pickupPoints)
        {
            float distance = Vector3.Distance(player.position, pt.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestPoint = pt;
            }
        }

        if (nearestPoint != null && _currentTarget != nearestPoint)
        {
            UnsetTarget();
            _currentTarget = nearestPoint;
            Debug.Log($"{_currentTarget}   (curTar, FindNearestCargo)");
        }
    }
    private void CalculateRoute()
    {
        //FindNearestTarget();
        if (_currentTarget != null)
        {
            //Debug.Log($"curTar: {_currentTarget}  (CalculateRoute)");

            NavMeshQueryFilter filter = new NavMeshQueryFilter();
            filter.areaMask = NavMesh.AllAreas;

            // Твой индекс 1 для агента "Car"
            filter.agentTypeID = NavMesh.GetSettingsByIndex(1).agentTypeID;

            // --- НАДЕЖНЫЙ СПОСОБ ПОИСКА ТОЧКИ ---
            NavMeshHit hit;
            // Ищем легальную точку на NavMesh в радиусе 10 метров от нашего спавнера
            if (NavMesh.SamplePosition(_currentTarget.position, out hit, 100.0f, filter))
            {
                // Если нашли - строим путь до неё (hit.position - это идеальные координаты на сетке)
                bool res = NavMesh.CalculatePath(player.position, hit.position, filter, _path);
                if (res) Debug.Log($"path to nearest point isDelivering = {isDelivering} calculated (arrow nav), curTar: {_currentTarget}");
            }
            else
            {
                // Дебаг, если спавнер вообще висит в космосе далеко от дорог
                Debug.LogWarning("Точка " + _currentTarget.name + " находится слишком далеко от NavMesh! Увеличьте радиус поиска или подвиньте спавнер ближе к дороге.");
            }
        }
    }
    private void UpdateArrowDirection()
    {
        //if (_path.status != NavMeshPathStatus.PathComplete || _path.corners.Length < 2) // !=
        //{
        //    Debug.Log("if entered");
        //    uiArrow.gameObject.SetActive(false);
        //    return;
        //}
        if (_path == null || _path.corners.Length == 0)
        {
            //Debug.Log("if entered");
            uiArrow.gameObject.SetActive(false);
            return;
        }

        uiArrow.gameObject.SetActive(true);
        Vector3 nextWaypoint = _path.corners[1];

        Vector3 directionToWaypoint = (nextWaypoint - player.position).normalized;
        directionToWaypoint.y = 0;
        Vector3 playerForward = player.forward;
        playerForward.y = 0;

        float angle = Vector3.SignedAngle(playerForward, directionToWaypoint, Vector3.up);
        uiArrow.rectTransform.localEulerAngles = new Vector3(0, 0, -angle);
    }
}
