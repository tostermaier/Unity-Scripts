using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AnimalAi : MonoBehaviour
{
    [SerializeField]
    private NavMeshAgent agent;

    [Header("Movement")]
    private float walkSpeed = 1; //not implemented yet
    private float runSpeed = 2; //not implemented yet

    [Header("Pathfinding")]
    public bool randomMovement;
    public float randomMovementRadius = 15f;
    public float randomMovementTimer = 5f;
    public float timer;

    public List<Transform> waypoints;
    private Transform currentWaypoint;
    private readonly float waypointStoppingDistance = 2;
    private int curWaypointIndex = 0; //used for selecting current waypoint

    [Header("Fleeing")]
    [SerializeField]
    private bool isFleeing = false;
    public float fleeTimer = 5f;
    private bool fleeTimerIsStarted = false;

    // Start is called before the first frame update
    void Start()
    {
        //set timer to initalize random movement point on init
        timer = randomMovementTimer;

        //load navmesh agent when agent is not set per editor
        if (!agent) agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!randomMovement && !isFleeing) DoWaypointMovement();
        else if(!isFleeing) DoRandomMovement();
        else if (isFleeing) DoFlee(); //only for test purpose
    }

    private void DoRandomMovement()
    {
        timer += Time.deltaTime;

        if (timer >= randomMovementTimer)
        {
            Vector3 newPos = GenerateRandomMovementPoint(transform.position, randomMovementRadius, -1);
            agent.SetDestination(newPos);
            timer = 0;
        }
    }

    /**
     * generate random movement position in layermask
     * 
     * See: 
     * https://forum.unity.com/threads/solved-random-wander-ai-using-navmesh.327950/
     */ 
    private Vector3 GenerateRandomMovementPoint(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;

        randDirection += origin;

        NavMeshHit navHit;

        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);

        return navHit.position;
    }
  
    private void DoWaypointMovement()
    {
        Debug.Log("set waypoint" + curWaypointIndex);
        if (agent.remainingDistance > waypointStoppingDistance)
        {
            agent.SetDestination(currentWaypoint.position);
        }
        else
        {
            SelectNextWaypoint();
        }
    }

    private void SelectNextWaypoint()
    {
        if (curWaypointIndex >= waypoints.Count - 1) curWaypointIndex = 0;
        else curWaypointIndex++;
        
        currentWaypoint = waypoints[curWaypointIndex];
        agent.SetDestination(currentWaypoint.position);
    }

    public void DoFlee()
    {
        isFleeing = true;
       if (!fleeTimerIsStarted) StartCoroutine(FleeTimer());
    }

    private IEnumerator FleeTimer()
    {

        fleeTimerIsStarted = true;

        //generate Random Waypoint for fleeing
        Vector3 newPos = GenerateRandomMovementPoint(transform.position, randomMovementRadius, -1);
        agent.SetDestination(newPos);

        yield return new WaitForSeconds(fleeTimer);
        isFleeing = false;
        fleeTimerIsStarted = false;
    }
}
