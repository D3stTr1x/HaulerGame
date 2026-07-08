using UnityEngine;

public class CargoPickup : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private float pickupDistance = 6f;
    [SerializeField] private string pickupKey = "e";
    [SerializeField] private Transform cargoHoldPoint;
    [SerializeField] private float fallDetectionHeight = 0.1f;
    [SerializeField] private float fallCheckInterval = 0.5f;
    private TruckCargoSystem truckSystem;

    [Header("Ограничение груза")]
    [SerializeField] private bool allowOnlyOneCargo = true;

    [Header("Визуал")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseIntensity = 0.4f;

    private Renderer[] renderers;
    private Rigidbody rb;
    private bool isPlayerNearby = false;
    private bool isHold = false;
    private float fallCheckTimer = 0f;
    private static Transform currentCargoInTruck = null;
    public float massCargo = 0f;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        rb = GetComponentInParent<Rigidbody>(); // находим Rigidbody на родителе
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        cargoHoldPoint = player.GetComponent<TruckCargoSystem>().cargoHoldPoint;
    }

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(pickupKey))
        {
            TryPickup();
        }

        if (isPlayerNearby)
            PulseEffect();

        // Проверка на выпадение груза
        if (isHold)
        {
            fallCheckTimer += Time.deltaTime;
            if (fallCheckTimer >= fallCheckInterval)
            {
                CheckIfFallen();
                fallCheckTimer = 0f;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            ShowPickupPrompt(true);
            if (truckSystem == null)
                truckSystem = other.GetComponentInParent<TruckCargoSystem>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            ShowPickupPrompt(false);
            ResetHighlight();
        }
    }

    private void PickupCargo()
    {
        if (isHold) return;

        ShowPickupPrompt(false);
        ResetHighlight();

        StartCoroutine(MoveToCargoHold());
    }
    private void TryPickup()
    {
        if (truckSystem == null) return;

        if (!truckSystem.CanPickupCargo())
        {
            Debug.Log("Кузов уже занят!");
            return;
        }
        PickupCargo();
        truckSystem.LoadCargo(transform);
    }

    private System.Collections.IEnumerator MoveToCargoHold()
    {
        if (cargoHoldPoint == null)
        {
            Debug.LogError("CargoHoldPoint не назначен!");
            yield break;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        Debug.Log("Начинаем мгновенное перемещение");

        // Мгновенно ставим в точку
        transform.SetParent(null);
        if (truckSystem != null)
        {
            truckSystem.currentCargo = transform;  // важно!
            truckSystem.massCargo = rb.mass; // если TruckCargoSystem имеет это поле
        }
        transform.position = cargoHoldPoint.position + Vector3.up * 0.5f;
        transform.rotation = cargoHoldPoint.rotation;

        yield return new WaitForSeconds(0.3f); // небольшая пауза
        // Финально фиксируем
        transform.SetParent(cargoHoldPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        rb.isKinematic = false;
        isHold = true;
        currentCargoInTruck = transform;
        Debug.Log("Груз поставлен в кузов");
    }

    private void CheckIfFallen()
    {
        if (cargoHoldPoint == null || !isHold) return;

        // Получаем мировые координаты
        Vector3 cargoPos = transform.position;
        Vector3 holdPos = cargoHoldPoint.position;

        // Размер зоны кузова (подбери под свой кузов)
        float maxDistanceX = 3.3f;   // длина кузова / 2
        float maxDistanceZ = 1.3f;   // ширина кузова / 2
        float maxDropY = 0.5f;       // максимальное падение по высоте

        // Вычисляем расстояние по горизонтали (X и Z)
        float distX = Mathf.Abs(cargoPos.x - holdPos.x);
        float distZ = Mathf.Abs(cargoPos.z - holdPos.z);

        // Проверка выпадения
        if (distX > maxDistanceX || distZ > maxDistanceZ ||
            cargoPos.y < holdPos.y - maxDropY)
        {
            OnCargoFallen();
            truckSystem.UnloadCurrentCargo();
        }
    }
    private void OnCargoFallen()
    {
        Debug.LogWarning("Груз вывалился из кузова!");

        isHold = false;
        currentCargoInTruck = null;
        // Включаем физику обратно
        if (rb != null)
        {
            rb.detectCollisions = true;
        }

        // Отсоединяем от кузова
        transform.SetParent(null);

        // Можно добавить эффект/звук/штраф
    }

    private void PulseEffect()
    {
        float intensity = Mathf.PingPong(Time.time * pulseSpeed, pulseIntensity) + 0.6f;
        foreach (var r in renderers)
        {
            if (r.material.HasProperty("_EmissionColor"))
                r.material.SetColor("_EmissionColor", Color.white * intensity);
        }
    }

    private void ResetHighlight()
    {
        foreach (var r in renderers)
        {
            if (r.material.HasProperty("_EmissionColor"))
                r.material.SetColor("_EmissionColor", Color.black);
        }
    }

    private void ShowPickupPrompt(bool show)
    {
        if (show)
            Debug.Log("<color=yellow>Нажмите [E] чтобы подобрать груз</color>");
        // Здесь позже подключишь UI
    }
}