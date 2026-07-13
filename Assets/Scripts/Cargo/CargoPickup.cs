
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class CargoPickup : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private float pickupDistance = 6f;
    [SerializeField] private string pickupKey = "e";
    [SerializeField] private float fallDetectionHeight = 0.1f;
    [SerializeField] private float fallCheckInterval = 0.5f;
    [SerializeField] private float pickupDuration = 1.2f; // Время плавного подъема
    [SerializeField] private TMP_Text HelpText;
    //[SerializeField] private AudioClip boxSound;
    /// <summary>
    /// [SerializeField] private AudioClip barrelSound;
    /// </summary>

    [Header("Визуал")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseIntensity = 0.4f;

    public float massCargo = 10f; // Установите базовый вес в инспекторе

    private TruckCargoSystem truckSystem;
    private Renderer[] renderers;
    private Rigidbody rb;

    private static int lastPickupFrame = -1;
    private bool isPlayerNearby = false;
    private bool isHold = false;
    private float fallCheckTimer = 0f;

    // Убрали static, так как грузов теперь может быть много
    private Transform currentCargoInTruck = null;

    public UnityEvent onCargoPickedUp; //events (for ZoneSpawner)
    public bool isPickedUp;

    public bool IsPickedUp
    {
        get => isPickedUp;
        set
        {
            if (isPickedUp != value)
            {
                isPickedUp = value;
                if (isPickedUp)
                {
                    onCargoPickedUp?.Invoke();
                    Debug.Log($"Package picked up, event fired");
                }
            }
        }
    }

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        rb = GetComponentInParent<Rigidbody>();

        //adding event listeners
        WarningText taskText = GameObject.FindFirstObjectByType<WarningText>();
        if (taskText != null)
        {
            onCargoPickedUp.AddListener(taskText.DeliverMessage);
        }
        ZoneSpawner zoneSpawner = GameObject.FindFirstObjectByType<ZoneSpawner>();
        if (zoneSpawner != null)
        {
            onCargoPickedUp.AddListener(zoneSpawner.ActivateRandom);
        }     
    }
    private void Start()
    {
        SphereCollider sphereCollider = GetComponentInChildren<SphereCollider>();
        if (sphereCollider)
        {
            float maxScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
            sphereCollider.radius = pickupDistance / maxScale;
        }
    }


    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(pickupKey))
        {
            // Проверяем, чтобы в один кадр брался только один груз
            if (lastPickupFrame != Time.frameCount)
            {
                lastPickupFrame = Time.frameCount;
                TryPickup();
            }
        }

        if (isPlayerNearby && !isHold)
            PulseEffect();

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
        if (other.CompareTag("Player") && !isHold)
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
            if (HelpText != null) HelpText.gameObject.SetActive(false);
        }
    }

    private void TryPickup()
    {
        if (truckSystem == null) return;

        if (!truckSystem.CanPickupCargo())
        {
            Debug.Log("Кузов уже заполнен!");
            return;
        }

        if (isHold) return;

        ShowPickupPrompt(false);
        ResetHighlight();

        // Сначала регистрируем груз, чтобы занять место
        truckSystem.LoadCargo(transform);
        StartCoroutine(MoveToCargoHold());
        IsPickedUp = true;
    }


    private System.Collections.IEnumerator MoveToCargoHold()
    {
        Transform holdPoint = truckSystem.GetCargoHoldPoint();
        if (holdPoint == null) yield break;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;

            // ИСПРАВЛЕНИЕ 1: Отключаем физические столкновения на время полета!
            // Теперь ящик - "призрак" и не сможет прогнуть машину своим коллайдером.
            rb.detectCollisions = false;
        }

        transform.SetParent(null);

        // Запрашиваем идеальное свободное место
        Vector3 localGridOffset = truckSystem.GetDynamicCargoPosition(transform);
        Vector3 targetPos = holdPoint.TransformPoint(localGridOffset);
        Quaternion targetRot = holdPoint.rotation;

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        float elapsed = 0f;

        // Плавное перемещение
        while (elapsed < pickupDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / pickupDuration;

            // ИСПРАВЛЕНИЕ 2: Двигаем ящик по красивой дуге
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);
            // Добавляем высоту (прыжок), чтобы груз перелетал через борт
            currentPos.y += Mathf.Sin(t * Mathf.PI) * 1.5f;

            transform.position = currentPos;
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null; // Ждем следующий кадр
        }

        // Жестко фиксируем в конце пути
        transform.position = targetPos;
        transform.rotation = targetRot;
        transform.SetParent(holdPoint);

        // Включаем физику обратно
        if (rb != null)
        {
            rb.isKinematic = false;
            // ИСПРАВЛЕНИЕ 3: Возвращаем столкновения, когда груз уже в кузове
            rb.detectCollisions = true;
        }

        // Применяем массу груза к грузовику
        truckSystem.totalMassCargo += massCargo;

        isHold = true;
        currentCargoInTruck = transform;
        if (HelpText != null) HelpText.gameObject.SetActive(false);
    }

    private void CheckIfFallen()
    {
        Transform holdPoint = truckSystem.GetCargoHoldPoint();
        if (holdPoint == null || !isHold) return;

        Vector3 cargoPos = transform.position;
        Vector3 holdPos = holdPoint.position;

        float maxDistanceX = 3.5f;
        float maxDistanceZ = 1.3f;
        float maxDropY = 0.5f;

        float distX = Mathf.Abs(cargoPos.x - holdPos.x);
        float distZ = Mathf.Abs(cargoPos.z - holdPos.z);

        if (distX > maxDistanceX || distZ > maxDistanceZ || cargoPos.y < holdPos.y - maxDropY)
        {
            OnCargoFallen();
        }
    }

    private void OnCargoFallen()
    {
        isHold = false;
        currentCargoInTruck = null;

        if (rb != null)
        {
            rb.detectCollisions = true;
        }

        transform.SetParent(null);
        truckSystem.UnloadCargo(transform); // Выгружаем конкретный груз из системы

        if (HelpText != null)
        {
            HelpText.gameObject.SetActive(true);
            HelpText.text = "Груз выпал из кузова!";
            StartCoroutine(HideHelpTextAfterDelay(3f));
        }
    }

    private System.Collections.IEnumerator HideHelpTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (HelpText != null) HelpText.gameObject.SetActive(false);
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
        if (HelpText == null) return;

        if (show)
        {
            HelpText.text = $"Нажмите [{pickupKey.ToUpper()}] чтобы погрузить";
            HelpText.gameObject.SetActive(true);
        }
        else
        {
            HelpText.gameObject.SetActive(false);
        }
    }
}