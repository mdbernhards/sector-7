using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NpcPatrolBehaviour : MonoBehaviour
{
    [Header("Route")]
    [Tooltip("Empty objects placed on sidewalks. NPC walks between these in order.")]
    public Transform[] waypoints;
    public bool loopRoute = true;
    public bool pickRandomWaypoint = false;

    [Header("Timing")]
    public float waitMinSeconds = 2f;
    public float waitMaxSeconds = 5f;
    public bool startPatrollingOnAwake = true;

    [Header("Movement")]
    public float minWalkSpeed = 1.2f;
    public float maxWalkSpeed = 3f;
    [Tooltip("Higher values bias random speeds towards slower NPCs.")]
    public float slowerSpeedBias = 2.3f;

    [Header("Spawn")]
    [Tooltip("Teleport to a random waypoint when this NPC starts.")]
    public bool spawnAtRandomWaypointOnStart = true;

    [Header("Visual Quality")]
    [Tooltip("Locks child LODGroups to LOD0 (highest quality). Keep OFF for dynamic LOD.")]
    public bool forceHighestLod = false;

    private NavMeshAgent agent;
    private Animator animator;
    private int currentWaypointIndex;
    private bool patrolEnabled;
    private bool isWaiting;
    private Coroutine waitRoutine;

    private void Awake()
    {
        DisablePlaceholderBodyIfLodModelExists();
        ApplyLodQualityPolicy();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        agent.speed = GetRandomizedWalkSpeed();
        patrolEnabled = startPatrollingOnAwake;
    }

    private void DisablePlaceholderBodyIfLodModelExists()
    {
        // Some RandomNPC prefabs still include a starter placeholder body under
        // Geometry/Armature_Mesh. If the prefab also contains a real LOD-based body,
        // disable the placeholder so close-range visuals use the detailed model.
        LODGroup[] lodGroups = GetComponentsInChildren<LODGroup>(true);
        if (lodGroups == null || lodGroups.Length == 0)
        {
            return;
        }

        Transform geometry = transform.Find("Geometry");
        if (geometry == null)
        {
            return;
        }

        Transform placeholderMesh = geometry.Find("Armature_Mesh");
        if (placeholderMesh == null)
        {
            return;
        }

        placeholderMesh.gameObject.SetActive(false);
    }

    private void ApplyLodQualityPolicy()
    {
        if (!forceHighestLod)
        {
            return;
        }

        LODGroup[] lodGroups = GetComponentsInChildren<LODGroup>(true);
        for (int i = 0; i < lodGroups.Length; i++)
        {
            if (lodGroups[i] == null)
            {
                continue;
            }

            lodGroups[i].ForceLOD(0);
        }
    }

    private void Start()
    {
        if (patrolEnabled && HasWaypoints())
        {
            if (spawnAtRandomWaypointOnStart)
            {
                int randomStartIndex = Random.Range(0, waypoints.Length);
                WarpToWaypoint(randomStartIndex);
                currentWaypointIndex = randomStartIndex;
            }

            int initialTargetIndex = GetNextWaypointIndex();
            MoveToWaypoint(initialTargetIndex);
        }
    }

    private void Update()
    {
        if (!patrolEnabled || !HasWaypoints() || isWaiting)
        {
            UpdateAnimator(0f);
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.05f)
        {
            BeginWaitAtWaypoint();
            return;
        }

        UpdateAnimator(agent.velocity.magnitude);
    }

    public void SetPatrolEnabled(bool enabled)
    {
        patrolEnabled = enabled;

        if (!enabled)
        {
            agent.ResetPath();
            UpdateAnimator(0f);
            return;
        }

        if (HasWaypoints())
        {
            MoveToWaypoint(currentWaypointIndex);
        }
    }

    private void BeginWaitAtWaypoint()
    {
        if (isWaiting)
        {
            return;
        }

        if (waitRoutine != null)
        {
            StopCoroutine(waitRoutine);
        }

        waitRoutine = StartCoroutine(WaitThenAdvance());
    }

    private IEnumerator WaitThenAdvance()
    {
        isWaiting = true;
        agent.ResetPath();
        UpdateAnimator(0f);

        float waitTime = Random.Range(waitMinSeconds, waitMaxSeconds);
        yield return new WaitForSeconds(waitTime);

        currentWaypointIndex = GetNextWaypointIndex();
        MoveToWaypoint(currentWaypointIndex);
        isWaiting = false;
        waitRoutine = null;
    }

    private int GetNextWaypointIndex()
    {
        if (waypoints.Length <= 1)
        {
            return 0;
        }

        if (pickRandomWaypoint)
        {
            int nextIndex = currentWaypointIndex;
            while (nextIndex == currentWaypointIndex)
            {
                nextIndex = Random.Range(0, waypoints.Length);
            }

            return nextIndex;
        }

        int forwardIndex = currentWaypointIndex + 1;
        if (forwardIndex >= waypoints.Length)
        {
            return loopRoute ? 0 : currentWaypointIndex;
        }

        return forwardIndex;
    }

    private void MoveToWaypoint(int index)
    {
        if (!HasWaypoints() || waypoints[index] == null)
        {
            return;
        }

        currentWaypointIndex = index;
        agent.SetDestination(waypoints[index].position);
    }

    private void WarpToWaypoint(int index)
    {
        if (!HasWaypoints() || waypoints[index] == null)
        {
            return;
        }

        Vector3 desiredPosition = waypoints[index].position;
        if (NavMesh.SamplePosition(desiredPosition, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
        {
            desiredPosition = navHit.position;
        }

        if (!agent.Warp(desiredPosition))
        {
            transform.position = desiredPosition;
        }

        agent.ResetPath();
    }

    private bool HasWaypoints()
    {
        return waypoints != null && waypoints.Length > 0;
    }

    private void UpdateAnimator(float speed)
    {
        if (animator == null)
        {
            return;
        }

        animator.SetFloat("Speed", speed);
        float normalizedSpeed = speed / Mathf.Max(agent.speed, 0.01f);
        animator.SetFloat("MotionSpeed", normalizedSpeed);
    }

    private float GetRandomizedWalkSpeed()
    {
        if (maxWalkSpeed <= minWalkSpeed)
        {
            return Mathf.Max(0.1f, minWalkSpeed);
        }

        float bias = Mathf.Max(0.01f, slowerSpeedBias);
        float weightedRandom = Mathf.Pow(Random.value, bias);
        return Mathf.Lerp(minWalkSpeed, maxWalkSpeed, weightedRandom);
    }
}
