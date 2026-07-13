
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class NavigationSystem : MonoBehaviour
{
    [Header("Ссылки на объекты")]
    [Tooltip("Трансформ автомобиля (игрока)")]
    public Transform player;

    [Tooltip("UI элемент стрелки (Image/RectTransform)")]
    public RawImage uiArrow;

    [Tooltip("Список всех доступных точек погрузки/доставки")]
    public Transform[] destinationPoints;

    [Header("Настройки")]
    [Tooltip("Частота обновления пути в секундах (для оптимизации)")]
    public float pathUpdateInterval = 0.5f;

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

        // Обновляем путь не каждый кадр, а по таймеру (оптимизация нагрузки на CPU)
        _timer += Time.deltaTime;
        if (_timer >= pathUpdateInterval)
        {
            CalculateRoute();
            _timer = 0f;
        }

        UpdateArrowDirection();
    }

    /// <summary>
    /// Ищет ближайшую точку из массива destinationPoints
    /// </summary>
    public void FindNearestTarget()
    {
        if (destinationPoints == null || destinationPoints.Length == 0) return;

        float minDistance = Mathf.Infinity;
        Transform nearestPoint = null;

        foreach (Transform point in destinationPoints)
        {
            if (!point.gameObject.activeInHierarchy) continue;
            // Считаем дистанцию по прямой (для первоначального поиска этого достаточно)
            float distance = Vector3.Distance(player.position, point.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestPoint = point;
            }
        }

        _currentTarget = nearestPoint;
    }

    /// <summary>
    /// Просчитывает путь от игрока до выбранной цели по NavMesh
    /// </summary>

    private void CalculateRoute()
    {
        if (_currentTarget != null)
        {
            // Берем координаты центра зоны, но опускаем их на уровень машины/дороги
            Vector3 groundDestination = new Vector3(_currentTarget.position.x, player.position.y, _currentTarget.position.z);

            // Создаем фильтр, чтобы использовать настройки вашего агента "Car", 
            // которого мы видим на 5-м скриншоте!
            NavMeshQueryFilter filter = new NavMeshQueryFilter();
            filter.areaMask = NavMesh.AllAreas;

            // Если "Car" стоит вторым в списке агентов (после Humanoid), его индекс 1
            filter.agentTypeID = NavMesh.GetSettingsByIndex(1).agentTypeID;

            // Считаем путь до точки на земле
            NavMesh.CalculatePath(player.position, groundDestination, filter, _path);
        }
    }

    /// <summary>
    /// Поворачивает UI стрелку в сторону следующего узла пути
    /// </summary>
    private void UpdateArrowDirection()
    {
        // ВРЕМЕННЫЙ ДЕБАГ:
        if (_path.status != NavMeshPathStatus.PathComplete)
        {
            Debug.LogWarning("Стрелка скрыта: Путь не полный! Текущий статус: " + _path.status);
        }
        else if (_path.corners.Length < 2)
        {
            Debug.Log("Стрелка скрыта: Мы уже достигли цели (осталась 1 точка).");
        }

        if (_path.status != NavMeshPathStatus.PathComplete || _path.corners.Length < 2)
        {
            uiArrow.gameObject.SetActive(false);
            return;
        }
        // Если путь не просчитан или мы уже достигли цели (осталась 1 точка - мы сами)
        if (_path.status != NavMeshPathStatus.PathComplete || _path.corners.Length < 2)
        {
            uiArrow.gameObject.SetActive(false);
            return;
        }

        uiArrow.gameObject.SetActive(true);

        // corners[0] - позиция начала пути (игрок). 
        // corners[1] - ближайший следующий поворот на маршруте.
        Vector3 nextWaypoint = _path.corners[1];

        // Находим вектор направления от машины к следующему повороту
        Vector3 directionToWaypoint = (nextWaypoint - player.position).normalized;
        directionToWaypoint.y = 0; // Игнорируем разницу высот (оставляем только 2D плоскость XZ)

        // Получаем вектор направления переда машины
        Vector3 playerForward = player.forward;
        playerForward.y = 0;

        // Вычисляем угол между направлением капота машины и нужным поворотом
        float angle = Vector3.SignedAngle(playerForward, directionToWaypoint, Vector3.up);

        // Вращаем UI элемент. 
        // Ось Z отвечает за вращение 2D объектов. Минус нужен для корректного отображения (лево/право).
        uiArrow.rectTransform.localEulerAngles = new Vector3(0, 0, -angle);
    }

    // --- Метод для вызова извне ---
    // Вызывайте его, когда игрок забрал груз и нужно сменить точки назначения на точки доставки
    public void UpdateDestinationPoints(Transform[] newPoints)
    {
        destinationPoints = newPoints;
        FindNearestTarget();
        CalculateRoute();
    }
}