using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class CarController : MonoBehaviour
{
    [SerializeField] private Transform _centreOfMass;
    [SerializeField] private Wheel[] _wheels;
    [SerializeField] private GameObject _lights;
    [SerializeField] private GameObject _lightsBackMove;

    //смещениие центра массы
    [SerializeField] private Vector3 _emptyCenterOfMass = new Vector3(0, -0.5f, 0);
    [SerializeField] private Vector3 _loadedCenterOfMassOffset = new Vector3(0, -0.8f, -0.6f); // X, Y, Z смещение при полной загрузке

    private Vector3 originalCenterOfMass;


    private Rigidbody _rb;
    [SerializeField] private int _motorForce;
    [SerializeField] private int _brakeForce;
    [SerializeField] private float _engineBrakeForce;
    [SerializeField] private float _maxSpeedForvard;
    [SerializeField] private float _maxSpeedRevers;

    private float loadFactor = 0f;

    [SerializeField] private float _brakeInput;
    private float _verticalInput;
    private float _horizontalInput;
    private float massCargo = 0f;

    [SerializeField] public float _speed;

    [SerializeField] private AnimationCurve _emptySterlingCurve;
    [SerializeField] private AnimationCurve _loadedSterlingCurve;

    private JointSpring[] initialSprings;
    private WheelFrictionCurve[] initialForwardFrictions;
    private WheelFrictionCurve[] initialSidewaysFrictions;

    private TruckCargoSystem truckCargoSystem;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.centerOfMass = _centreOfMass != null ? _centreOfMass.localPosition : new Vector3(0, -0.5f, 0);
        originalCenterOfMass = _rb.centerOfMass;
        _rb.mass = 2800f;
        _rb.linearDamping = 0.5f;
        _rb.angularDamping = 0.5f;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        truckCargoSystem = _rb.GetComponent<TruckCargoSystem>();

        foreach (Wheel wheel in _wheels)
        {
            var c = wheel.WheelCollider;

            // === Подвеска (Suspension) ===
            JointSpring spring = c.suspensionSpring;
            spring.spring = 45000f;        // Жёстче чем у легковушки
            spring.damper = 2800f;         // Хорошее демпфирование
            spring.targetPosition = 0.5f;
            c.suspensionSpring = spring;

            c.suspensionDistance = 0.65f;  // Ход подвески
            c.forceAppPointDistance = -0.05f; // Точка приложения силы (чуть ниже)

            // === Радиус и масса ===
            c.radius = 0.45f;           // ~35-37 дюймовые колёса
            c.mass = 45f;

            // === Трение (Friction) ===
            // Forward Friction (разгон и торможение)
            WheelFrictionCurve fwd = c.forwardFriction;
            fwd.extremumSlip = 0.35f;
            fwd.extremumValue = 1.15f;
            fwd.asymptoteSlip = 0.75f;
            fwd.asymptoteValue = 0.75f;
            c.forwardFriction = fwd;

            // Sideways Friction (боковое сцепление)
            WheelFrictionCurve side = c.sidewaysFriction;
            side.extremumSlip = 0.25f;
            side.extremumValue = 1.1f;
            side.asymptoteSlip = 0.55f;
            side.asymptoteValue = 0.85f;
            c.sidewaysFriction = side;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    private void Update()
    {
        CheckInput();
        Move();
        Steer();
        Brake();
        CheckLoad();
        UpdateWheelSettings();
        UpdateCenterOfMass();
    }

    private void FixedUpdate()
    {
        foreach (Wheel wheel in _wheels)
        {
            if (wheel.IsForwardWheels)
            {
                float slipAngel = Vector3.Angle(transform.forward, _rb.linearVelocity - transform.forward);
            }
        }

    }
    private void UpdateCenterOfMass()
    {
        if (loadFactor <= 0.01f)
        {
            _rb.centerOfMass = originalCenterOfMass;
            return;
        }

        // Плавная интерполяция центра масс
        Vector3 targetCoM = originalCenterOfMass + _loadedCenterOfMassOffset * loadFactor;

        // Дополнительно немного опускаем центр масс при сильной загрузке (очень важно!)
        targetCoM.y = Mathf.Lerp(originalCenterOfMass.y, originalCenterOfMass.y - 0.7f, loadFactor);

        _rb.centerOfMass = targetCoM;
        Debug.Log("centerOfMass" +  _rb.centerOfMass);
    }

    private void CheckLoad()
    {
        if (truckCargoSystem.currentCargo != null)
        {
            // Берём массу напрямую из Rigidbody груза
            Rigidbody cargoRb = truckCargoSystem.currentCargo.GetComponent<Rigidbody>();
            if (cargoRb != null)
            {
                massCargo = cargoRb.mass;
            }
            else
            {
                // Fallback — если есть публичное поле massCargo на скрипте CargoPickup
                CargoPickup cargoScript = truckCargoSystem.currentCargo.GetComponent<CargoPickup>();
                if (cargoScript != null)
                    massCargo = cargoScript.massCargo;
            }
        }
        else
        {
            massCargo = 0f;
        }

        float emptyMass = 2800f;           // пустая масса грузовика
        float maxCargoMass = 300f;        // максимальная масса груза (подстрой)

        loadFactor = Mathf.Clamp01((massCargo) / maxCargoMass); // теперь правильно
    }


    private void Move()
    {
        _speed = _rb.linearVelocity.magnitude;

        foreach (Wheel wheel in _wheels)
        {
            float motorTorque = 0f;
            float currentMaxSpeed = _verticalInput > 0 ? _maxSpeedForvard : _maxSpeedRevers;

            if (Mathf.Abs(_verticalInput) > 0.01f && _speed < currentMaxSpeed)
            {
                float speedLimit = Mathf.Clamp01(1f - (_speed / currentMaxSpeed));
                motorTorque = _motorForce * _verticalInput * speedLimit;
            }
            else if (_speed > 0.5f && Mathf.Abs(_verticalInput) < 0.01f)
            {
                // Двигательное торможение (engine brake)
                motorTorque = -Mathf.Sign(Vector3.Dot(_rb.linearVelocity, transform.forward)) * _engineBrakeForce;
                if (Vector3.Dot(transform.forward * motorTorque, _rb.linearVelocity.normalized) > 0)
                    motorTorque = 0;
            }

            wheel.WheelCollider.motorTorque = motorTorque;
            wheel.UpdateMeshPosition();
        }

        bool isReversingInput = _verticalInput < 0;

        if (isReversingInput && IsMovingForward)
        {
            _lightsBackMove.gameObject.SetActive(false);
            _lights.gameObject.SetActive(true);
        }
        else if (isReversingInput && !IsMovingForward)
        {
            _lights.gameObject.SetActive(false);
            _lightsBackMove.gameObject.SetActive(true);
        }
        else
        {
            _lights.gameObject.SetActive(false);
            _lightsBackMove.gameObject.SetActive(false);
        }
    }

    public bool IsMovingForward
    {
        get
        {
            return Vector3.Dot(transform.forward, _rb.linearVelocity) > 0.5f;
        }
    }

    private void CheckInput()
    {
        _verticalInput = Input.GetAxis("Vertical");
        _horizontalInput = Input.GetAxis("Horizontal");

        float movingDirectional = Vector3.Dot(transform.forward, _rb.linearVelocity);

        if ((movingDirectional < -0.5f && _verticalInput > 0) || (movingDirectional > 0.5f && _verticalInput < 0))
            _brakeInput = Mathf.Abs(_verticalInput);
        else
            _brakeInput = 0;

    }

    private void Brake()
    {
        foreach (Wheel wheel in _wheels)
        {
            if (wheel.IsForwardWheels)
            {
                wheel.WheelCollider.brakeTorque = _brakeForce * _brakeInput * 0.7f;
            }
            else
            {
                wheel.WheelCollider.brakeTorque = _brakeForce * _brakeInput * 0.3f;
            }
        }

    }

    private void Steer()
    {
        AnimationCurve _sterlingCurve = (loadFactor > 0.3f) ? _loadedSterlingCurve : _emptySterlingCurve;

        float steeringAngle = _horizontalInput * _sterlingCurve.Evaluate(_speed);
        steeringAngle = Mathf.Clamp(steeringAngle, -42f, 42f);

        foreach (Wheel wheel in _wheels)
        {
            if (wheel.IsForwardWheels && wheel.WheelCollider != null)
                wheel.WheelCollider.steerAngle = steeringAngle;
        }
    }

    void UpdateWheelSettings()
    {
        float t = loadFactor;

        foreach (Wheel wheel in _wheels)
        {
            var c = wheel.WheelCollider;

            // === Подвеска ===
            JointSpring spring = c.suspensionSpring;
            spring.spring = Mathf.Lerp(42000f, 68000f, t);
            spring.damper = Mathf.Lerp(2600f, 4200f, t);
            spring.targetPosition = 0.48f;
            c.suspensionSpring = spring;

            c.suspensionDistance = Mathf.Lerp(0.68f, 0.55f, t);
            c.forceAppPointDistance = Mathf.Lerp(-0.03f, -0.08f, t);

            // === Трение ===
            WheelFrictionCurve fwd = c.forwardFriction;
            fwd.extremumValue = Mathf.Lerp(1.1f, 1.25f, t);
            fwd.asymptoteValue = Mathf.Lerp(0.75f, 0.8f, t);
            c.forwardFriction = fwd;

            WheelFrictionCurve side = c.sidewaysFriction;
            side.extremumValue = Mathf.Lerp(1.15f, 1.05f, t); // хуже боковое сцепление при загрузке
            side.asymptoteValue = Mathf.Lerp(0.85f, 0.75f, t);
            c.sidewaysFriction = side;
        }
    }

}

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