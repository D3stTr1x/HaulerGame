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
    //private float maxSpeed = 70f;
    //private BaseCarController controller;
    //private float waitTimer = 0f;
    //private bool waiting = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        FindWheels();
        //base._wheels = _wheels;
        path = new NavMeshPath();
        waypoints = GetDeliveryWaypoints();
        //if (waypoints.Length != 0)
        //    destination = waypoints[0];
        //UpdatePath();
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
        //if (transform.position) //check if in radius
        //if (waypoints == null) waypoints = GetDeliveryWaypoints();
        //if (waypoints.Length >= 0)
        //{
        //    destination = waypoints[curWaypointGlobal];
        //}
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

        float desSpeed = _maxSpeedForvard;
        if (Mathf.Abs(steer) > 40f)
        {
            desSpeed *= 0.1f;
        }
        else if (Mathf.Abs(steer) > 20f)
        {
            desSpeed *= 0.3f;
        }
        else if (Mathf.Abs(steer) > 10f)
        {
            desSpeed *= 0.7f;
        }
        
        
        float speedErr = desSpeed - _speed;
        float throttle = Mathf.Clamp(speedErr * 0.5f, 0f, 1f); //??
        float brake = speedErr < -5f ? _brakeForce : 0f;

        
        SetInputs(throttle, steer, brake);
        //Debug.Log($"h_input: {_horizontalInput}");
        KindaSteer();
        //Debug.Log("inputs set");

        //KindaMove();
        //base.ApplyMotorTorque();
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
        return path.corners[path.corners.Length - 1];
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
        //GameObject[] zones = GameObject.FindGameObjectsWithTag("DeliveryZone");
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
                //agent.SetDestination(randDest);
            }
            return;
        }
        curWaypointGlobal = (curWaypointGlobal + 1) % waypoints.Length;
        //agent.SetDestination(waypoints[curWaypoint].position);
    }
}
