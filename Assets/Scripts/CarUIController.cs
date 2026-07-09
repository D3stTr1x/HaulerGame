using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CarUIController : MonoBehaviour
{
    [Header("UI Элементы")]
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI gearText;
    [SerializeField] private Slider rpmSlider;

    [Header("Настройки")]
    [SerializeField] private float maxRPM = 6500f;
    [SerializeField] private Color highRpmColor = new Color(1f, 0.3f, 0.3f);

    private PlayerCarController playerCar;
    private Rigidbody rb;

    private void Awake()
    {
        // Ищем машину по тегу или через FindObjectOfType
        playerCar = Object.FindFirstObjectByType<PlayerCarController>();

        if (playerCar == null)
        {
            Debug.LogError("CarUIController: PlayerCarController не найден в сцене!");
            enabled = false;
            return;
        }

        rb = playerCar.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (playerCar == null) return;

        UpdateSpeed();
        UpdateRPM();
        UpdateGear();
    }

    private void UpdateSpeed()
    {
        if (speedText == null) return;
        float speedKmh = rb.linearVelocity.magnitude * 3.6f;
        speedText.text = Mathf.RoundToInt(speedKmh).ToString() + " км/ч";
    }

    private void UpdateRPM()
    {
        if (rpmSlider == null) return;

        float speedKmh = rb.linearVelocity.magnitude * 3.6f;
        float maxSpeedGear = playerCar.CurrentMaxSpeedThisGear;

        float baseRpm = 800f; // Холостые обороты
        float rpm = baseRpm;
        float speedPercent = 0f;

        if (maxSpeedGear > 0.1f)
        {
            // Оставляем небольшой запас (1 км/ч), чтобы компенсировать сопротивление физики
            float effectiveMaxSpeed = Mathf.Max(1f, maxSpeedGear - 1f);
            speedPercent = Mathf.Clamp01(speedKmh / effectiveMaxSpeed);
            rpm = Mathf.Lerp(baseRpm, maxRPM, speedPercent);
        }

        float throttle = Mathf.Max(0f, Input.GetAxis("Vertical"));

        if (throttle > 0.1f)
        {
            // ИСПРАВЛЕНИЕ 1: Умный газ.
            // На низких скоростях педаль дает скачок в 600 оборотов, 
            // но по мере приближения к максимуму этот скачок плавно сходит на нет (до 0).
            // Это не даст газу закинуть стрелку в отсечку раньше времени.
            float throttleBump = Mathf.Lerp(600f, 0f, speedPercent);
            rpm += throttle * throttleBump;
        }

        // ИСПРАВЛЕНИЕ 2: Привязываем отсечку к РЕАЛЬНОЙ СКОРОСТИ, а не к цифрам RPM.
        bool isLimiting = false;

        // Дергаем слайдер ТОЛЬКО если машина физически разогналась до 98% от лимита передачи.
        if (speedPercent >= 0.98f && throttle > 0.1f)
        {
            isLimiting = true;
            rpm = maxRPM - Random.Range(150f, 500f); // Эффект прыгающей стрелки
        }

        // Жестко зажимаем в рамках
        rpm = Mathf.Clamp(rpm, baseRpm, maxRPM);
        rpmSlider.value = rpm / maxRPM;

        // ИСПРАВЛЕНИЕ 3: Цвет тоже привязываем к реальной скорости (с 95% разгона)
        Image fillImage = rpmSlider.fillRect.GetComponent<Image>();
        if (fillImage != null)
        {
            if (isLimiting || speedPercent > 0.95f)
            {
                fillImage.color = highRpmColor;
            }
            else
            {
                fillImage.color = Color.white; // Укажи здесь свой базовый цвет
            }
        }
    }

    private void UpdateGear()
    {
        if (gearText == null) return;
        gearText.text = playerCar.GetGearText();

        // Цвет задней передачи
        gearText.color = playerCar.IsReverse ? Color.red : Color.white;
    }
}