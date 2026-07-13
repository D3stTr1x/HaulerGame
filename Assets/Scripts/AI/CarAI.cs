using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class CarAI : BaseCarController
{
    public Transform[] waypoints;
    public float waitTime = 0f;
    public bool randomPatrol = false;

    private NavMeshPath path;
    private List<Vector3> smoothPath = new List<Vector3>();
    public Transform destination;
    //private NavMeshAgent agent;
    private float lookahead = 5f;
    private int curWaypoint = 0;
    private int curWaypointGlobal = 0;
    private float pointRad = 10f;

    private float pathUpdTime = 0f;
    private float pathUpdInterval = 0.5f;

    //replace with controller references
    private float maxSteer = 42f;
    private float maxTurn = 90f;
    private float lookAheadSeg = 3f;
    protected override void Start()
    {
        base.Start();
        //_maxSpeedForvard = 30;
        FindWheels();
        path = new NavMeshPath();
        waypoints = GetDeliveryWaypoints();
        if (waypoints.Length != 0)
            destination = waypoints[0];
        UpdatePath();
        //Debug.Log();
    }

    // Update is called once per frame
    protected override void Update()
    {
        pathUpdTime += Time.deltaTime;
        if (pathUpdTime >= pathUpdInterval)
        {
            UpdatePath();
            pathUpdTime = 0;
        }
        FollowPath();
    }

    void UpdatePath()
    {
        if (waypoints == null) waypoints = GetDeliveryWaypoints();
        if (waypoints.Length >= 0)
        {
            destination = waypoints[curWaypointGlobal];
        }
        int roadArea = NavMesh.GetAreaFromName("Road");
        int roadMask = 1 << roadArea;
        bool res = NavMesh.CalculatePath(transform.position, destination.position, NavMesh.AllAreas, path);
        //if (res)
        //    Debug.Log($"Path found, {path.corners.Length}");
        //else Debug.Log("Path not found");
        curWaypoint = 0;
    }
    void FollowPath()   
    {
        if (path.corners.Length == 0) return;
        Vector3 target = GetNextPoint();
        //Debug.Log($"got next point: {target}");

        Vector3 localTarget = transform.InverseTransformPoint(target);
        float steer = Mathf.Clamp(localTarget.x, -maxSteer, maxSteer);
        float desSpeed = CalcSpeed();

        float speedErr = desSpeed - _speed;
        float throttle, brake;
        if (speedErr > 0)
        {
            throttle = Mathf.Clamp(speedErr * 0.5f, 0f, 1f);
            brake = 0f;
        }
        else
        {
            throttle = 0f;
            brake = speedErr < -5f ? _brakeForce : 0f;
        }

        //brake = speedErr < -5f ? _brakeForce : 0f;

        SetInputs(throttle, steer, brake);
        KindaSteer();

        //KindaMove();
        //base.ApplyMotorTorque();
    }
    float CalcSpeed()
    {
        float desSpeed = _maxSpeedForvard;
        float steer = GetMaxTurnAngle();

        if (Mathf.Abs(steer) > 40f)
        {
            desSpeed *= 0.1f;
        }
        else if (Mathf.Abs(steer) > 20f)
        {
            desSpeed *= 0.2f;
        }
        else if (Mathf.Abs(steer) > 10f)
        {
            desSpeed *= 0.5f;
        }
        else if (Mathf.Abs(steer) > 5f)
        {
            desSpeed *= 0.7f;
        }
        return desSpeed;
    }
    //void FindSmoothPath()
    //{
    //    if (path.status != NavMeshPathStatus.PathComplete)
    //        return;
    //    if (HasSharpTurns())
    //    {
    //        return;
    //    }
    //}
    //bool HasSharpTurns()
    //{
    //    if (path.)
    //}
    float GetMaxTurnAngle(int segments = 3)
    {
        if (path.corners.Length < 3) return 0f;

        int start = Mathf.Min(curWaypoint, path.corners.Length - 1);
        int end = Mathf.Min(start + segments, path.corners.Length - 2);

        float maxAngle = 0f;

        for (int i = start; i < end; i++)
        {
            float angle = Vector3.Angle(
                (path.corners[i + 1] - path.corners[i]).normalized,
                (path.corners[i + 2] - path.corners[i + 1]).normalized
            );
            if (angle > maxAngle) maxAngle = angle;
        }

        return maxAngle;
    }
    Vector3 GetNextPoint()
    {
        Vector3 pos = transform.position;
        float distChecked = 0f;

        for (int i = curWaypoint; i < path.corners.Length; i++)
        {
            //distance check
            float seg = Vector3.Distance(pos, path.corners[i]);
            distChecked += seg;
            if (distChecked >= lookahead)
            {
                curWaypoint = i;
                return path.corners[i];
            }
            pos = path.corners[i];
            
        }
        //
        curWaypoint = path.corners.Length - 1;
        MoveToNextPointGlobal();
        UpdatePath();
        //return path.corners[path.corners.Length - 1];
        return GetNextPoint();
    }
    new private void FindWheels()
    {
        base.FindWheels();
        if (_wheels != null)
            base._wheels = _wheels;
    }
    Transform[] GetDeliveryWaypoints()
    {
        ZoneSpawner zoneSpawner = GameObject.FindFirstObjectByType<ZoneSpawner>();
        List<DeliveryZone> zones = zoneSpawner.deliveryZones;
        //Debug.Log($"{zones.Length} zones found");
        if (zones != null)
        {
            zones = zones.OrderBy(x => Random.value).ToList();
            waypoints = new Transform[zones.Count];
            for (int i = 0; i < zones.Count; i++)
            {
                waypoints[i] = zones[i].transform;
            }
        }
        Debug.Log($"Waypoints found: {waypoints.Length}");
        return waypoints;
    }
    void MoveToNextPointGlobal()
    {
        if (waypoints.Length == 0)
        {
            if (randomPatrol)
            {
                //Vector3 randDest = RandomNavMeshPoint(20f);
                return;
            }
            return;
        }
        curWaypointGlobal = (curWaypointGlobal + 1) % waypoints.Length;
        Debug.Log("Moving to next global point");
    }
}
