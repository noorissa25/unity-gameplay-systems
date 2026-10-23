using UnityEngine;
using UnityEngine.AI;

// My AI script for AI_Minion (the clone I made of Minion_NoRootMotion)
// I took out CharacterInputController, MinionBasicControlScript and MinionScript from the clone,
// so nothing reads the keyboard anymore and this script is what drives him now
// The NavMeshAgent does the actual path finding and steering, I just tell it where to go
//
// The AI is a procedural state machine (an enum + a switch in Update), which is separate from
// the Animator's state machine. It has 2 states and loops between them forever:
//   PatrolStaticWaypoints   -> walk to my 5 static waypoints in order (0 -> 1 -> 2 -> 3 -> 4)
//   InterceptMovingWaypoint -> head off the moving waypoint by predicting where it will be
//   then back to PatrolStaticWaypoints starting at waypoint 0 again, and so on
[RequireComponent(typeof(NavMeshAgent))]
public class MinionAI : MonoBehaviour
{
    // My two AI states
    public enum AIState
    {
        PatrolStaticWaypoints,
        InterceptMovingWaypoint
    };

    // Which state the minion is in right now, public so I can watch it change in the Inspector
    public AIState aiState;

    [Header("Static Waypoints (state 0)")]

    // The 5 static waypoints I placed around the arena (the yellow spheres with no colliders)
    // I drag them into this array in the Inspector, and the order I put them in is the
    // order the minion visits them
    public GameObject[] waypoints;

    // Which waypoint the minion is walking to right now
    // I start it at -1 on purpose: setNextWaypoint() adds 1 before using it, so the very first
    // waypoint he goes to is 0 and not 1 (the TA FAQ says starting anywhere but 0 loses points)
    public int currWaypoint = -1;

    // How close he needs to get (in meters) before I count the waypoint as reached
    // The agent usually stops a little short of the exact point, so if this was 0 he would
    // just stand next to the waypoint forever and never move on
    public float arriveDistance = 0.5f;

    [Header("Moving Waypoint (state 1)")]

    // The red sphere that my Mecanim animation slides back and forth along the right wall
    // This has to be the sphere that actually moves (it has the VelocityReporter on it)
    public GameObject movingWaypoint;

    // The tall skinny box I use to show where the minion is predicting he'll catch the
    // moving waypoint, it gets moved to the predicted spot every frame
    public GameObject destinationTracker;

    // How close the minion has to get to the ACTUAL moving waypoint (not the predicted spot)
    // to count it as caught, small enough that it looks like he really touches it
    public float captureDistance = 1.0f;

    // The most seconds into the future I'm allowed to predict
    // Without a limit, when he's far away the prediction could shoot way past the end of the
    // waypoint's track, so I clamp the look ahead time with Mathf.Clamp
    public float maxLookAheadTime = 3.0f;

    // How high to lift the tracker box so it stands on the floor instead of half inside it
    // (the box is 3 tall and its pivot is in the middle, so half of 3)
    public float trackerHeightOffset = 1.5f;

    // The NavMeshAgent I added to AI_Minion, this is what actually moves him around the NavMesh
    private NavMeshAgent agent;

    // The minion's Animator, I use it to send the walking speed so he hops instead of sliding
    private Animator anim;

    // The VelocityReporter on the moving waypoint, this is where I get its velocity from
    private VelocityReporter movingWaypointVelocity;

    // The time when the current loop started (heading to waypoint 0), so I can print how long
    // each full loop takes and keep it under 45 seconds
    private float loopStartTime;

    void Awake()
    {
        // RequireComponent makes sure the NavMeshAgent is always there, so I can just grab it
        agent = GetComponent<NavMeshAgent>();

        // The Animator isn't required by the script, so I check it and print an error if it's
        // missing instead of letting it break later with a confusing null reference
        anim = GetComponent<Animator>();

        if (anim == null)
            Debug.LogError("MinionAI: No Animator found on " + gameObject.name);

        // Grab the VelocityReporter off the moving waypoint so I can read how fast it's going
        // If this is missing the prediction can't work (velocity would always be zero)
        if (movingWaypoint == null)
        {
            Debug.LogError("MinionAI: movingWaypoint isn't assigned on " + gameObject.name);
        }
        else
        {
            movingWaypointVelocity = movingWaypoint.GetComponent<VelocityReporter>();

            if (movingWaypointVelocity == null)
                Debug.LogError("MinionAI: " + movingWaypoint.name + " is missing its VelocityReporter");
        }

        if (destinationTracker == null)
            Debug.LogError("MinionAI: destinationTracker isn't assigned on " + gameObject.name);
    }

    void Start()
    {
        // The minion always starts out patrolling the static waypoints
        aiState = AIState.PatrolStaticWaypoints;

        // Send him to his first waypoint as soon as the game starts
        // Because currWaypoint starts at -1, this call bumps it to 0
        setNextWaypoint();

        // Start timing the first loop
        loopStartTime = Time.time;
    }

    void Update()
    {
        // Check which state I'm in and do that state's job, switching states when it's done
        switch (aiState)
        {
            case AIState.PatrolStaticWaypoints:

                // I only pick the next waypoint once he has actually arrived at the current one
                // pathPending is true for a moment right after SetDestination() while the agent
                // is still working out the path, and during that time remainingDistance can read 0
                // Without checking pathPending first, he would think he arrived instantly and skip
                // through a bunch of waypoints in a row
                if (!agent.pathPending && agent.remainingDistance <= arriveDistance)
                {
                    // Once he reaches the last static waypoint (waypoint 4) the patrol part of
                    // the loop is done, so I switch over to chasing the moving waypoint
                    if (currWaypoint >= waypoints.Length - 1)
                        aiState = AIState.InterceptMovingWaypoint;
                    else
                        setNextWaypoint();
                }

                // During patrol I just park the tracker where he's heading.
                // agent.destination is the waypoint's spot snapped down
                // onto the NavMesh, so the box stands on the floor instead of floating
                moveTracker(agent.destination);

                break;

            case AIState.InterceptMovingWaypoint:

                // Safety check so the game doesn't crash if I forget to assign the moving waypoint
                // in the Inspector, he just goes back to patrolling instead
                if (movingWaypoint == null)
                {
                    aiState = AIState.PatrolStaticWaypoints;
                    currWaypoint = -1;
                    setNextWaypoint();
                    break;
                }

                // Recalculate the predicted catch point EVERY frame, not just once when the state
                // starts, otherwise he would run to an old guess and never actually catch it
                Vector3 predictedPos = predictInterceptPosition();

                // Steer toward the predicted spot, never straight at the moving waypoint itself,
                // so he intercepts it instead of trailing behind it
                agent.SetDestination(predictedPos);

                // Show my prediction in the game with the tall skinny box
                moveTracker(predictedPos);

                // Success is measured to the ACTUAL moving waypoint, not the predicted spot
                // Reaching the guess isn't the same as reaching the waypoint, so I only count it
                // when he's really within captureDistance of the sphere
                if (flatDistance(transform.position, movingWaypoint.transform.position) <= captureDistance)
                {
                    // Print how long this whole loop took (5 static waypoints + catching the
                    // moving one) so I can check it in the Console while I tune the speeds
                    Debug.Log("MinionAI: loop finished in " + (Time.time - loopStartTime).ToString("F1") + " seconds");
                    loopStartTime = Time.time;

                    // Caught it, so go back to patrolling and start over at waypoint 0
                    // Resetting to -1 means the next setNextWaypoint() call goes to 0
                    aiState = AIState.PatrolStaticWaypoints;
                    currWaypoint = -1;
                    setNextWaypoint();
                }

                break;

            default:
                break;
        }

        // Without this he just slides around sunk into the floor with no step animation
        // The minion's animator (MinionNoRootAnimController) uses the float "vely" for forward
        // speed, 0 = standing still and 1 = full speed hopping forward
        // agent.speed is the max speed I set on the NavMeshAgent, so dividing his current
        // velocity by it turns his real speed into that 0 to 1 range the blend tree expects
        // That way he hops faster when he's moving fast and slows down as he brakes at a waypoint
        if (anim != null)
            anim.SetFloat("vely", agent.velocity.magnitude / agent.speed);
    }

    // Moves currWaypoint up by one, loops back to 0 after the last waypoint,
    // and gives the NavMeshAgent the new destination
    private void setNextWaypoint()
    {
        // If I forgot to fill in the waypoints array in the Inspector, there's nowhere to go
        // and waypoints[currWaypoint] would crash, so I log an error and stop here instead
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("MinionAI: waypoints array is empty on " + gameObject.name);
            return;
        }

        currWaypoint++;

        // After waypoint 4 (the last spot in my array of 5) this would go out of bounds,
        // so I wrap it back around to 0 and the loop starts over
        if (currWaypoint >= waypoints.Length)
            currWaypoint = 0;

        // Tell the agent to path find to the waypoint's position, the NavMeshAgent takes
        // care of steering around the crates and walls on the way there
        agent.SetDestination(waypoints[currWaypoint].transform.position);
    }

    // Figures out where the moving waypoint is going to be when the minion gets there
    // This is the simple distance based look ahead from the AI steering lecture:
    //   look ahead time = distance to the target / my max speed
    //   predicted spot  = target position + target velocity * look ahead time
    // Then I make sure the predicted spot is still on the NavMesh before I use it
    private Vector3 predictInterceptPosition()
    {
        Vector3 targetPos = movingWaypoint.transform.position;

        // If the velocity reporter is missing I can't predict, so just aim at the waypoint
        // (Awake already printed an error about this)
        if (movingWaypointVelocity == null)
            return targetPos;

        Vector3 targetVel = movingWaypointVelocity.velocity;

        // How long it would take me to cover the distance to the waypoint at full speed
        // The farther away he is, the further ahead in time I need to predict
        float dist = flatDistance(transform.position, targetPos);
        float lookAheadTime = dist / agent.speed;

        // Clamp it so a really far away minion doesn't predict a spot way off the track
        lookAheadTime = Mathf.Clamp(lookAheadTime, 0f, maxLookAheadTime);

        // Extrapolate along the waypoint's current direction of travel
        Vector3 predictedPos = targetPos + targetVel * lookAheadTime;

        // Keep the prediction on the NavMesh (the TA FAQ takes points off if it isn't)
        // NavMesh.Raycast needs to start from a point that's actually on the NavMesh, and my
        // waypoint hovers 0.5 above the floor, so first I snap its position down onto the mesh
        NavMeshHit startHit;
        if (!NavMesh.SamplePosition(targetPos, out startHit, 2f, NavMesh.AllAreas))
            return targetPos;

        Vector3 start = startHit.position;
        Vector3 end = new Vector3(predictedPos.x, start.y, predictedPos.z);

        // Raycast along the NavMesh from the waypoint toward my prediction
        // If it hits the edge of the NavMesh (like a wall or crate) the predicted spot would be
        // off the mesh, so I pull it back to just before the spot where it hit the edge
        NavMeshHit edgeHit;
        if (NavMesh.Raycast(start, end, out edgeHit, NavMesh.AllAreas))
            end = Vector3.Lerp(start, edgeHit.position, 0.9f);

        // Last safety check: snap the final point onto the NavMesh so SetDestination always
        // gets a valid spot the agent can actually walk to
        NavMeshHit finalHit;
        if (NavMesh.SamplePosition(end, out finalHit, 1f, NavMesh.AllAreas))
            return finalHit.position;

        return start;
    }

    // Puts the tall skinny tracker box at a spot on the floor so I can see the prediction
    private void moveTracker(Vector3 floorPos)
    {
        if (destinationTracker == null)
            return;

        destinationTracker.transform.position = new Vector3(floorPos.x, floorPos.y + trackerHeightOffset, floorPos.z);
    }

    // Distance measured only across the floor (x and z), ignoring height
    // The minion and the waypoint sit at different heights, so a normal 3D distance would
    // never get as small as it should even when he's standing right on top of it
    private float flatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }
}
