using UnityEngine;
using UnityEngine.AI;

public class SimpleAIMover : MonoBehaviour
{
    public Transform[] waypoints;
    public float waitTime = 1f;
    public bool randomPatrol = true;

    private NavMeshAgent agent;
    private int curWaypoint = 0;
    private float waitTimer = 0f;
    private bool waiting = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[0].position);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (agent == null) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!waiting)
            {
                waiting = true;
                waitTimer = waitTime;
            }
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                waiting = false;
                MoveToNextPoint();
            }
        }
    }
    void MoveToNextPoint()
    {
        if (waypoints.Length == 0)
        {
            if (randomPatrol)
            {
                Vector3 randDest = RandomNavMeshPoint(20f);
                agent.SetDestination(randDest);
            }
            return;
        }
        curWaypoint = (curWaypoint + 1) % waypoints.Length;
        agent.SetDestination(waypoints[curWaypoint].position);
    }
    Vector3 RandomNavMeshPoint(float range)
    {
        Vector3 randPt = transform.position + Random.insideUnitSphere * range;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randPt, out hit, range, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return transform.position; //stay in place
    }
}
