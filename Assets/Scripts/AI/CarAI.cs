using UnityEngine;
using UnityEngine.AI;

public class CarAI : MonoBehaviour
{
    public Transform[] waypoints;
    public float waitTime = 0f;
    public bool randomPatrol = true;

    private NavMeshPath path;
    public Transform destination;
    //private NavMeshAgent agent;
    private int curWaypoint = 0;
    private float pathUpdTime = 0f;
    private float pathUpdInterval = 0.5f;
    private BaseCarController controller;
    //private float waitTimer = 0f;
    //private bool waiting = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<BaseCarController>();
        path = new NavMeshPath();
    }

    // Update is called once per frame
    void Update()
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
        NavMesh.CalculatePath(transform.position, destination.position, NavMesh.AllAreas, path);
        curWaypoint = 0;
    }
    void FollowPath()
    {

    }
}
