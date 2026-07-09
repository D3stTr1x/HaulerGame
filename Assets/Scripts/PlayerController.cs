
using UnityEngine;

public class PlayerCarController : BaseCarController
{
    [Header("Player Settings")]
    [SerializeField] private GameObject _lights;
    [SerializeField] private GameObject _lightsBackMove;

    [Header("МКПП - Старый пикап (R, 1, 2, 3, 4)")]
    [SerializeField] private int maxForwardGear = 4;

    [Tooltip("Передаточные числа для тяжелого грузовичка: 0=R, 1=1st, 2=2nd, 3=3rd, 4=4th")]
    [SerializeField] private float[] gearRatios = { 4.0f, 4.2f, 2.5f, 1.5f, 1.0f };

    [Tooltip("Максимальная скорость на каждой передаче старого пикапа (в км/ч)")]
    [SerializeField] private float[] maxSpeedPerGear = { 12f, 15f, 32f, 55f, 82f };

    [SerializeField] private AnimationCurve gearEfficiencyCurve = AnimationCurve.EaseInOut(0, 1.0f, 1.0f, 0.2f);

    private int currentGear = 1;
    private bool isReverse = false;

    protected override void Start()
    {
        base.Start();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        currentGear = 1;

        // ИСПРАВЛЕНИЕ: Отменяем "кисель", прописанный в базовом классе.
        // Ставим нормальное сопротивление для автомобилей (0.02 - 0.05)
        if (_rb != null)
        {
            _rb.linearDamping = 0.02f;
        }
    }

    protected override void Update()
    {
        HandleGearShift();
        CheckPlayerInput();
        Steer();
        Move();

        float throttle = Mathf.Max(0f, _verticalInput);

        if (carAudio != null)
            carAudio.UpdateCarAudio(throttle, _brakeInput, isReverse, _rb.linearVelocity.magnitude);
    }

    private void HandleGearShift()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            if (isReverse)
            {
                isReverse = false;
                currentGear = 1;
            }
            else if (currentGear < maxForwardGear)
            {
                currentGear++;
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
        {
            if (currentGear > 1)
            {
                currentGear--;
            }
            else if (!isReverse)
            {
                isReverse = true;
            }
        }
    }

    private void CheckPlayerInput()
    {
        _verticalInput = Input.GetAxis("Vertical");
        _horizontalInput = Input.GetAxis("Horizontal");

        if (_verticalInput < 0)
        {
            _brakeInput = Mathf.Abs(_verticalInput);
        }
        else
        {
            _brakeInput = 0f;
        }
    }

    protected override void ApplyMotorTorque()
    {
        _speed = _rb.linearVelocity.magnitude;
        float speedKmh = _speed * 3.6f;

        float throttle = Mathf.Max(0f, _verticalInput);

        if (throttle < 0.01f)
        {
            ApplyEngineBraking();
            return;
        }

        int gearIndex = isReverse ? 0 : currentGear;
        float gearRatio = gearRatios[gearIndex];
        float currentMaxSpeed = maxSpeedPerGear[gearIndex];

        float slopeFactor = CalculateSlopeFactor();
        float speedNorm = currentMaxSpeed > 0 ? Mathf.Clamp01(speedKmh / currentMaxSpeed) : 0f;
        float efficiency = gearEfficiencyCurve.Evaluate(speedNorm);

        // УМНЫЙ ОГРАНИЧИТЕЛЬ: Плавно "отпускаем педаль газа" за 4 км/ч до лимита передачи.
        // Это предотвратит любые скачки скорости даже с очень мощным мотором.
        float speedLimitFactor = 1f;
        if (currentMaxSpeed > 0f)
        {
            float limitStart = Mathf.Max(0f, currentMaxSpeed - 4f);
            if (speedKmh > limitStart)
            {
                // Чем ближе к максимуму, тем меньше множитель (от 1.0 до 0.0)
                speedLimitFactor = Mathf.Clamp01((currentMaxSpeed - speedKmh) / 4f);
            }
        }

        // Общая сила без деления. Машина поедет бодро!
        float
effectiveForce = _motorForce * Mathf.Abs(gearRatio) * efficiency * (1.0f + slopeFactor * 0.5f);
        float directionMultiplier = isReverse ? -1f : 1f;

        foreach (Wheel wheel in _wheels)
        {
            // Применяем тягу с учетом умного ограничителя
            float motorTorque = effectiveForce * throttle * directionMultiplier * speedLimitFactor;

            // Жесткая отсечка на крайний случай
            if (speedKmh >= currentMaxSpeed)
            {
                motorTorque = 0f;
            }

            wheel.WheelCollider.motorTorque = motorTorque;
            wheel.UpdateMeshPosition();
        }
    }

    private void ApplyEngineBraking()
    {
        float speedKmh = _rb.linearVelocity.magnitude * 3.6f;

        if (speedKmh < 1f)
        {
            foreach (Wheel wheel in _wheels)
            {
                wheel.WheelCollider.motorTorque = 0f;
            }
            return;
        }

        float movingDirection = Vector3.Dot(_rb.linearVelocity, transform.forward);
        float brakeTorque = -Mathf.Sign(movingDirection) * _engineBrakeForce * 0.8f;

        foreach (Wheel wheel in _wheels)
        {
            wheel.WheelCollider.motorTorque = brakeTorque;
            wheel.UpdateMeshPosition();
        }
    }

    private float CalculateSlopeFactor()
    {
        float slopeAngle = Vector3.Angle(transform.up, Vector3.up);
        return Mathf.Clamp01(slopeAngle / 45f);
    }

    protected override void Move()
    {
        if (_lightsBackMove != null)
        {
            _lightsBackMove.SetActive(isReverse);
        }

        if (_lights != null)
        {
            bool isBraking = _brakeInput > 0.01f;
            _lights.SetActive(isBraking);
        }
    }

    public string GetGearText() => isReverse ? "R" : currentGear.ToString();
    public bool IsReverse => isReverse;

    // Добавлено для корректной работы UI Тахометра
    public float CurrentMaxSpeedThisGear => isReverse ? maxSpeedPerGear[0] : maxSpeedPerGear[currentGear];
}

