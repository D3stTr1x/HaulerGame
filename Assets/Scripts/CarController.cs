using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class CarController : MonoBehaviour
{
    [SerializeField] private Transform _centreOfMass;
    [SerializeField] private Wheel[] _wheels;
    [SerializeField] private GameObject _lights;
    [SerializeField] private GameObject _lightsBackMove;


    private Rigidbody _rb;
    [SerializeField] private int _motorForce;
    [SerializeField] private int _brakeForce;
    [SerializeField] private float _engineBrakeForce;
    [SerializeField] private float _maxSpeedForvard;
    [SerializeField] private float _maxSpeedRevers;

    [SerializeField] private float _brakeInput;
    private float _verticalInput;
    private float _horizontalInput;

    [SerializeField] public float _speed;

    [SerializeField] private AnimationCurve _sterlingCurve;

    private JointSpring[] initialSprings;
    private WheelFrictionCurve[] initialForwardFrictions;
    private WheelFrictionCurve[] initialSidewaysFrictions;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.centerOfMass = _centreOfMass != null ? _centreOfMass.localPosition : new Vector3(0, -0.5f, 0);
        _rb.mass = 1500f;
        _rb.linearDamping = 0.5f;
        _rb.angularDamping = 0.5f;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

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
            // c.radius = 0.45f;           // ~35-37 дюймовые колёса
            // c.mass = 45f;

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

        Debug.Log($"Wheels count: {_wheels.Length}");
    }

    private void Update()
    {
        CheckInput();
        Move();
        Steer();
        Brake();
        
        
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

        // Логика фар
        if (_verticalInput < 0)
            _lightsBackMove.SetActive(true);
        else
            _lightsBackMove.SetActive(false);

        if (_verticalInput > 0)
            _lights.SetActive(true);
        else
            _lights.SetActive(false);
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
        float steeringAngle = _horizontalInput * _sterlingCurve.Evaluate(_speed);
        steeringAngle = Mathf.Clamp(steeringAngle, -48f, 48f);
        Debug.Log($"Horizontal: {_horizontalInput:F2}, Speed: {_speed:F1}, SteeringAngle: {steeringAngle:F1}");
        foreach (Wheel wheel in _wheels)
        {
            if (wheel.IsForwardWheels && wheel.WheelCollider != null)
                wheel.WheelCollider.steerAngle = steeringAngle;
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