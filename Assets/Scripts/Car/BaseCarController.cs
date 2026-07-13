using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BaseCarController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _centreOfMass;
    [SerializeField] public Wheel[] _wheels;
    [SerializeField] public CarAudioController carAudio; // можно оставить, но отключать у ботов

    [Header("Center of Mass")]
    [SerializeField] private Vector3 _emptyCenterOfMass = new Vector3(0, 0.3f, 0.1f);
    [SerializeField] private Vector3 _loadedCoMOffset = new Vector3(0.0f, 2.35f, 2.05f);

    [Header("Vehicle Settings")]
    [SerializeField] protected int _motorForce = 2600;
    [SerializeField] private AnimationCurve _powerCurve = AnimationCurve.EaseInOut(0, 1, 1, 0.35f);
    [SerializeField] protected int _brakeForce = 4200;
    [SerializeField] protected float _engineBrakeForce = 950f;
    [SerializeField] protected float _maxSpeedForvard = 48f;
    [SerializeField] protected float _maxSpeedRevers = 14f;

    [Header("Steering")]
    [SerializeField] private AnimationCurve _emptySterlingCurve;
    [SerializeField] private AnimationCurve _loadedSterlingCurve;

    protected Rigidbody _rb;
    protected TruckCargoSystem truckCargoSystem;

    // Inputs (будут задаваться из наследника)
    protected float _verticalInput;
    protected float _horizontalInput;
    protected float _brakeInput;

    protected float _speed;
    protected float loadFactor = 0f;
    private Vector3 baseCenterOfMass;
    private float lastLoadFactor = -1f;
    private const float loadThreshold = 0.03f;

    protected virtual void Start()
    {
        _rb = GetComponent<Rigidbody>();
        SetupRigidbody();
        baseCenterOfMass = _emptyCenterOfMass;
        truckCargoSystem = GetComponent<TruckCargoSystem>();
        InitializeWheels();
    }

    private void SetupRigidbody()
    {
        _rb.mass = 2200f;
        _rb.linearDamping = 0.45f;
        _rb.angularDamping = 0.8f;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.centerOfMass = _emptyCenterOfMass;
    }

    private void InitializeWheels()
    {
        foreach (Wheel wheel in _wheels)
        {
            var c = wheel.WheelCollider;
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

    protected void FixedUpdate()
    {
        CalculateDynamicCenterOfMass();
        UpdateWheelSettingsOptimized();
        ApplyMotorTorque();
        ApplyBrakes();
    }
    //protected void KindaMove()
    //{
    //    CalculateDynamicCenterOfMass();
    //    UpdateWheelSettingsOptimized();
    //    ApplyMotorTorque();
    //    //ApplyBrakes();
    //}
    protected virtual void Update()
    {
        // Переопределяется в PlayerCarController
        Move(); // логика огней и т.д.
    }

    protected void CalculateDynamicCenterOfMass()
    {
        if (truckCargoSystem == null || truckCargoSystem.loadedCargos == null || truckCargoSystem.loadedCargos.Count == 0)
        {
            _rb.centerOfMass = baseCenterOfMass;
            loadFactor = 0f;
            return;
        }

        Vector3 totalCargoWeightedPosition = Vector3.zero;
        float accumulatedCargoMass = 0f;

        foreach (Transform cargoTransform in truckCargoSystem.loadedCargos)
        {
            if (cargoTransform == null) continue;
            Rigidbody cargoRb = cargoTransform.GetComponent<Rigidbody>();
            if (cargoRb != null)
            {
                Vector3 localCargoPos = transform.InverseTransformPoint(cargoRb.worldCenterOfMass);
                totalCargoWeightedPosition += localCargoPos * cargoRb.mass;
                accumulatedCargoMass += cargoRb.mass;
            }
        }

        if (accumulatedCargoMass <= 0f)
        {
            _rb.centerOfMass = baseCenterOfMass;
            loadFactor = 0f;
            return;
        }

        float totalMass = _rb.mass + accumulatedCargoMass;
        Vector3 dynamicCom = ((baseCenterOfMass * _rb.mass) + totalCargoWeightedPosition) / totalMass;
        dynamicCom.y = Mathf.Clamp(dynamicCom.y, baseCenterOfMass.y - 0.1f, baseCenterOfMass.y + 0.4f);

        _rb.centerOfMass = dynamicCom;
        loadFactor = Mathf.Clamp01(accumulatedCargoMass / 1100f);
    }

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
                spring.spring = Mathf.Lerp(36000f, 65000f, t);
                spring.damper = Mathf.Lerp(2200f, 3800f, t);
                spring.targetPosition = 0.48f;
                c.suspensionSpring = spring;
                c.suspensionDistance = Mathf.Lerp(0.70f, 0.56f, t);
                c.forceAppPointDistance = Mathf.Lerp(-0.03f, -0.07f, t);

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

    protected virtual void ApplyMotorTorque()
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
        //Debug.Log("applying torque");
    }

    protected void ApplyBrakes()
    {
        foreach (Wheel wheel in _wheels)
        {
            float brakeMultiplier = wheel.IsForwardWheels ? 0.72f : 0.28f;
            wheel.WheelCollider.brakeTorque = _brakeForce * _brakeInput * brakeMultiplier;
        }
        //Debug.Log("applying brakes");
    }

    protected void Steer()
    {
        AnimationCurve steeringCurve = (loadFactor > 0.3f) ? _loadedSterlingCurve : _emptySterlingCurve;
        float steeringAngle = _horizontalInput * steeringCurve.Evaluate(_speed);
        steeringAngle = Mathf.Clamp(steeringAngle, -42f, 42f);

        foreach (Wheel wheel in _wheels)
        {
            if (wheel.IsForwardWheels)
            {
                wheel.WheelCollider.steerAngle = steeringAngle;
                //Debug.Log($"Steering: {steeringAngle}, h_input: {_horizontalInput}");
            }
            //else Debug.Log("no forward wheel found :(");
        }
    }
    protected void KindaSteer()  //this is the kinda one
    {
        float steeringAngle = _horizontalInput;
        steeringAngle = Mathf.Clamp(steeringAngle, -42f, 42f);

        foreach (Wheel wheel in _wheels)
        {
            if (wheel.IsForwardWheels)
            {
                wheel.WheelCollider.steerAngle = steeringAngle;
                //Debug.Log($"Steering: {steeringAngle}, h_input: {_horizontalInput}");
            }
            //else Debug.Log("no forward wheel found :(");
        }
    }
    //protected void Steer(float h_input)
    //{

    //}
    protected virtual void Move()
    {
        // Логика огней — можно переопределить в PlayerCarController
    }

    protected void SetInputs(float vertical, float horizontal, float brake = 0f)
    {
        _verticalInput = vertical;
        _horizontalInput = horizontal;
        _brakeInput = brake;
    }

    private void OnDrawGizmos()
    {
        if (_rb == null) _rb = GetComponent<Rigidbody>();
        if (_rb != null)
        {
            Vector3 worldCoM = transform.TransformPoint(_rb.centerOfMass);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(worldCoM, 0.3f);
        }
    }
    protected void FindWheels()
    {
        Transform tireParent = transform.Find("Tire");
        Transform colliderParent = transform.Find("Collider");
        //if (tireParent != null)
        //{
        //    GameObject[] wheels = new GameObject[tireParent.childCount];
        //    for (int i = 0; i < tireParent.childCount; i++)
        //    {
        //        wheels[i] = tireParent.GetChild(i).gameObject;
        //    }
        //}
        //if (colliderParent != null)
        //{
        //    WheelCollider[] wheelColliders = colliderParent.GetComponentsInChildren<WheelCollider>();
        //}
        Wheel[] wheels = {
            new Wheel(tireParent.Find("Pick Up_7 BL Tire"), colliderParent.Find("WheelCollider_BL").GetComponent<WheelCollider>(), false),
            new Wheel(tireParent.Find("Pick Up_7 BR Tire"), colliderParent.Find("WheelCollider_BR").GetComponent<WheelCollider>(), false),
            new Wheel(tireParent.Find("Pick Up_7 FR Tire"), colliderParent.Find("WheelCollider_FR").GetComponent<WheelCollider>(), true),
            new Wheel(tireParent.Find("Pick Up_7 FL Tire"), colliderParent.Find("WheelCollider_FL").GetComponent<WheelCollider>(), true)
        };
        _wheels = wheels;
        //GameObject[] wheelColliders = GameObject.FindGameObjectsWithTag("WheelCollider");
    }
}
// Wheel struct без изменений
//[System.Serializable]
//public struct Wheel
//{
//    public Transform WheelMesh;
//    public WheelCollider WheelCollider;
//    public bool IsForwardWheels;

//    public void UpdateMeshPosition()
//    {
//        Vector3 position;
//        Quaternion rotation;
//        WheelCollider.GetWorldPose(out position, out rotation);
//        WheelMesh.position = position;
//        WheelMesh.rotation = rotation;
//    }
//}