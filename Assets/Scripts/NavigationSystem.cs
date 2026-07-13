using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class NavigationSystem : MonoBehaviour
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
    private float _timer;

    void Start()
    {
        _path = new NavMeshPath();
        FindNearestTarget();
    }

    void Update()
    {
        if (_currentTarget == null) return;

        _timer += Time.deltaTime;
        if (_timer >= pathUpdateInterval)
        {
            CalculateRoute();
            _timer = 0f;
        }
        UpdateArrowDirection();
    }

    // --- НОВЫЙ МЕТОД: Переключатель режимов ---
    public void SetDeliveryMode(bool deliveryMode)
    {
        isDelivering = deliveryMode;
        FindNearestTarget(); // Сразу ищем новую цель при смене режима
        CalculateRoute();
    }

    public void FindNearestTarget()
    {
        // Выбираем активный массив в зависимости от того, едем мы с грузом или пустые
        Transform[] currentPoints = isDelivering ? deliveryPoints : pickupPoints;

        if (currentPoints == null || currentPoints.Length == 0) return;

        float minDistance = Mathf.Infinity;
        Transform nearestPoint = null;

        foreach (Transform point in currentPoints)
        {
            // Проверка для зон доставки: если объект выключен, пропускаем его
            if (isDelivering && !point.gameObject.activeInHierarchy)
            {
                continue;
            }
            // Проверка для точек спавна коробок (если они тоже могут быть выключены)
            if (!isDelivering && !point.gameObject.activeInHierarchy)
            {
                continue;
            }

            float distance = Vector3.Distance(player.position, point.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestPoint = point;
            }
        }

        _currentTarget = nearestPoint;
    }

    private void CalculateRoute()
    {
        if (_currentTarget != null)
        {
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
                NavMesh.CalculatePath(player.position, hit.position, filter, _path);
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
        if (_path.status != NavMeshPathStatus.PathComplete || _path.corners.Length < 2)
        {
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