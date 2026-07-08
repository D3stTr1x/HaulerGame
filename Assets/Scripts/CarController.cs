using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [SerializeField] private Transform _centreOfMass;
    [SerializeField] private Wheel[] _wheels;
    [SerializeField] private GameObject _lights;
    [SerializeField] private GameObject _lightsBackMove;
    public CarAudioController carAudio;

    // Центр массы
    [SerializeField] private Vector3 _emptyCenterOfMass = new Vector3(0, -0.65f, 0f);
    [SerializeField] private Vector3 _loadedCoMOffset = new Vector3(0.0f, 0.35f, -0.75f); // исправлено: вниз + назад

    private Vector3 baseCenterOfMass;
    private Rigidbody _rb;

    [SerializeField] private int _motorForce = 2600;
    [SerializeField] private AnimationCurve _powerCurve = AnimationCurve.EaseInOut(0, 1, 1, 0.35f);
    [SerializeField] private int _brakeForce = 4200;
    private float _currentBrakeForce;
    [SerializeField] private float _engineBrakeForce = 950f;

    [SerializeField] private float _maxSpeedForvard = 48f;
    [SerializeField] private float _maxSpeedRevers = 14f;

    private float loadFactor = 0f;
    [SerializeField] private float _brakeInput;
    private float _verticalInput;
    private float _horizontalInput;
    private float massCargo = 0f;
    [SerializeField] public float _speed;

    [SerializeField] private AnimationCurve _emptySterlingCurve;
    [SerializeField] private AnimationCurve _loadedSterlingCurve;

    private TruckCargoSystem truckCargoSystem;

    // Для оптимизации обновления настроек колёс
    private float lastLoadFactor = -1f;
    private const float loadThreshold = 0.03f;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        _rb.mass = 2600f;
        _rb.linearDamping = 0.45f;
        _rb.angularDamping = 0.8f;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        _rb.centerOfMass = _emptyCenterOfMass;
        baseCenterOfMass = _emptyCenterOfMass;

        truckCargoSystem = GetComponent<TruckCargoSystem>();

        InitializeWheels();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void InitializeWheels()
    {
        foreach (Wheel wheel in _wheels)
        {
            var c = wheel.WheelCollider;

            //Подвеска
            JointSpring spring = c.suspensionSpring;
            spring.spring = 38000f;
            spring.damper = 2400f;
            spring.targetPosition = 0.5f;
            c.suspensionSpring = spring;

            c.suspensionDistance = 0.68f;
            c.forceAppPointDistance = -0.04f;

            c.radius = 0.45f;
            c.mass = 42f;

            // Forward Friction
            WheelFrictionCurve fwd = c.forwardFriction;
            fwd.extremumSlip = 0.32f;
            fwd.extremumValue = 1.12f;
            fwd.asymptoteSlip = 0.72f;
            fwd.asymptoteValue = 0.78f;
            c.forwardFriction = fwd;

            // Sideways Friction
            WheelFrictionCurve side = c.sidewaysFriction;
            side.extremumSlip = 0.24f;
            side.extremumValue = 1.08f;
            side.asymptoteSlip = 0.52f;
            side.asymptoteValue = 0.82f;
            c.sidewaysFriction = side;
        }
    }

    private void Update()
    {
        CheckInput();
        Move();
        Steer();
        bool isReverseInput = _verticalInput < -0.1f;
        float throttle = Mathf.Max(0f, _verticalInput);           // газ только вперёд
        float brake = _brakeInput;                                // уже обработано в CheckInput

        carAudio.UpdateCarAudio(throttle, brake, isReverseInput, _rb.linearVelocity.magnitude);
    }

    private void FixedUpdate()
    {
        CalculateDynamicCenterOfMass();
        UpdateWheelSettingsOptimized();
        ApplyMotorTorque();
        _currentBrakeForce =Mathf.MoveTowards(_currentBrakeForce, _brakeForce * _brakeInput, Time.fixedDeltaTime * 8000f);
        ApplyBrakes();
    }


    private void CalculateDynamicCenterOfMass()
    {
        // Проверяем, есть ли вообще грузы в списке
        if (truckCargoSystem == null || truckCargoSystem.loadedCargos == null || truckCargoSystem.loadedCargos.Count == 0)
        {
            _rb.centerOfMass = baseCenterOfMass;
            loadFactor = 0f;
            return;
        }

        Vector3 totalCargoWeightedPosition = Vector3.zero;
        float accumulatedCargoMass = 0f;

        // Итерируемся по всем загруженным трансформам из списка
        foreach (Transform cargoTransform in truckCargoSystem.loadedCargos)
        {
            if (cargoTransform == null) continue;

            Rigidbody cargoRb = cargoTransform.GetComponent<Rigidbody>();
            if (cargoRb != null)
            {
                // Переводим мировой центр масс конкретной коробки в локальные координаты пикапа
                Vector3 localCargoPos = transform.InverseTransformPoint(cargoRb.worldCenterOfMass);

                // Взвешиваем позицию (умножаем на массу объекта)
                totalCargoWeightedPosition += localCargoPos * cargoRb.mass;

                // Суммируем массу
                accumulatedCargoMass += cargoRb.mass;
            }
        }

        // Если у объектов в кузове нет Rigidbody или масса равна 0, сбрасываем CoM
        if (accumulatedCargoMass <= 0f)
        {
            _rb.centerOfMass = baseCenterOfMass;
            loadFactor = 0f;
            return;
        }

        // Вычисляем финальный совокупный центр масс машины и всех её грузов
        float totalMass = _rb.mass + accumulatedCargoMass;
        Vector3 dynamicCom = ((baseCenterOfMass * _rb.mass) + totalCargoWeightedPosition) / totalMass;

        // Небольшой зажим по высоте (Y), чтобы гора коробок до небес не переворачивала пикап на ровном месте
        dynamicCom.y = Mathf.Clamp(dynamicCom.y, baseCenterOfMass.y - 0.1f, baseCenterOfMass.y + 0.4f);

        // Применяем центр масс к Rigidbody машины
        _rb.centerOfMass = dynamicCom;

        // Обновляем loadFactor для адаптивной подвески колес
        float maxCargoMass = 1100f; // Максимальная грузоподъемность пикапа
        loadFactor = Mathf.Clamp01(accumulatedCargoMass / maxCargoMass);
    }

    private void CheckInput()
    {
        _verticalInput = Input.GetAxis("Vertical");
        _horizontalInput = Input.GetAxis("Horizontal");

        float movingDirectional = Vector3.Dot(transform.forward, _rb.linearVelocity);

        if ((movingDirectional < -0.5f && _verticalInput > 0) ||
            (movingDirectional > 0.5f && _verticalInput < 0))
            _brakeInput = Mathf.Abs(_verticalInput);
        else
            _brakeInput = 0;
    }

    private void ApplyMotorTorque()
    {
        _speed = _rb.linearVelocity.magnitude;
        float effectiveMotorForce = _motorForce * _powerCurve.Evaluate(loadFactor);

        foreach (Wheel wheel in _wheels)
        {
            float motorTorque = 0f;
            float currentMaxSpeed = _verticalInput > 0 ? _maxSpeedForvard : _maxSpeedRevers;

            if (Mathf.Abs(_verticalInput) > 0.01f && _speed < currentMaxSpeed)
            {
                float speedLimit = Mathf.Clamp01(1f - (_speed / currentMaxSpeed));
                motorTorque = effectiveMotorForce * _verticalInput * speedLimit;
            }
            else if (_speed > 0.6f && Mathf.Abs(_verticalInput) < 0.01f)
            {
                motorTorque = -Mathf.Sign(Vector3.Dot(_rb.linearVelocity, transform.forward)) * _engineBrakeForce;
            }

            wheel.WheelCollider.motorTorque = motorTorque;
            wheel.UpdateMeshPosition();
        }
    }

    private void ApplyBrakes()
    {
        foreach (Wheel wheel in _wheels)
        {
            if (wheel.IsForwardWheels)
                wheel.WheelCollider.brakeTorque = _brakeForce * _brakeInput * 0.72f;
            else
                wheel.WheelCollider.brakeTorque = _brakeForce * _brakeInput * 0.28f;
        }
    }

    private void Steer()
    {
        AnimationCurve _sterlingCurve = (loadFactor > 0.3f) ? _loadedSterlingCurve : _emptySterlingCurve;
        float steeringAngle = _horizontalInput * _sterlingCurve.Evaluate(_speed);
        steeringAngle = Mathf.Clamp(steeringAngle, -42f, 42f);

        foreach (Wheel wheel in _wheels)
        {
            if (wheel.IsForwardWheels)
                wheel.WheelCollider.steerAngle = steeringAngle;
        }
    }

    // Оптимизированное обновление настроек колёс
    private void UpdateWheelSettingsOptimized()
    {
        if (Mathf.Abs(loadFactor - lastLoadFactor) < loadThreshold) return;

        lastLoadFactor = loadFactor;
        float t = loadFactor;

        foreach (Wheel wheel in _wheels)
        {
            if (!wheel.IsForwardWheels)
            {
                var c = wheel.WheelCollider;

                JointSpring spring = c.suspensionSpring;
                spring.spring = Mathf.Lerp(36000f, 65000f, t);     // мягче в пустом состоянии
                spring.damper = Mathf.Lerp(2200f, 3800f, t);
                spring.targetPosition = 0.48f;
                c.suspensionSpring = spring;

                c.suspensionDistance = Mathf.Lerp(0.70f, 0.56f, t);
                c.forceAppPointDistance = Mathf.Lerp(-0.03f, -0.07f, t);

                // Friction
                WheelFrictionCurve fwd = c.forwardFriction;
                fwd.extremumValue = Mathf.Lerp(1.08f, 1.22f, t);
                fwd.asymptoteValue = Mathf.Lerp(0.76f, 0.81f, t);
                c.forwardFriction = fwd;

                WheelFrictionCurve side = c.sidewaysFriction;
                side.extremumValue = Mathf.Lerp(1.12f, 1.02f, t);
                side.asymptoteValue = Mathf.Lerp(0.84f, 0.73f, t);
                c.sidewaysFriction = side;
            }
        }
    }

    private void Move()
    {
        // Логика огней оставлена в Update (как было)
        bool isReversingInput = _verticalInput < 0;
        bool isMovingForward = Vector3.Dot(transform.forward, _rb.linearVelocity) > 0.5f;

        if (isReversingInput && isMovingForward)
        {
            _lights.SetActive(true);
            _lightsBackMove.SetActive(false);
        }
        else if (isReversingInput && !isMovingForward)
        {
            _lights.SetActive(false);
            _lightsBackMove.SetActive(true);
        }
        else
        {
            _lights.SetActive(false);
            _lightsBackMove.SetActive(false);
        }
    }

    private void OnDrawGizmos()
    {
        // Получаем Rigidbody, если он еще не назначен (нужно для работы в редакторе)
        Rigidbody debugRb = _rb;
        if (debugRb == null)
            debugRb = GetComponent<Rigidbody>();

        if (debugRb != null)
        {
            // Переводим локальный центр масс в мировые координаты
            Vector3 worldCoM = transform.TransformPoint(debugRb.centerOfMass);

            // Рисуем красную сферу размером 0.3 метра
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(worldCoM, 0.3f);
        }
    }
}

// Wheel struct без изменений
[System.Serializable]
public struct Wheel
{
    public Transform WheelMesh;
    public WheelCollider WheelCollider;
    public bool IsForwardWheels;

    public void UpdateMeshPosition()
    {
        Vector3 position;
        Quaternion rotation;
        WheelCollider.GetWorldPose(out position, out rotation);
        WheelMesh.position = position;
        WheelMesh.rotation = rotation;
    }
}