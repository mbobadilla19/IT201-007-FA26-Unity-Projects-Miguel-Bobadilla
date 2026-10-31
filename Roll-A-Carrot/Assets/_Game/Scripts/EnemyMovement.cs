using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    public Transform player;
    public bool waitForPlayer = false;
    public float detectionRange = 8f;

    [Header("Navigation Check — Read During Play")]
    public bool onNavMesh;
    public NavMeshPathStatus pathStatus;
    public float remainingDistance;

    private NavMeshAgent navMeshAgent;
    private bool chaseStarted;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        chaseStarted = !waitForPlayer;
    }

    void Update()
    {
        onNavMesh = navMeshAgent.isOnNavMesh;

        if (!onNavMesh)
        {
            return;
        }

        pathStatus = navMeshAgent.pathStatus;
        remainingDistance = navMeshAgent.remainingDistance;

        if (player == null)
        {
            navMeshAgent.isStopped = true;
            return;
        }

        if (!chaseStarted)
        {
            float distance = Vector3.Distance(
                transform.position, player.position
            );

            if (distance <= detectionRange)
            {
                chaseStarted = true;
            }
        }

        navMeshAgent.isStopped = !chaseStarted;

        if (chaseStarted)
        {
            navMeshAgent.SetDestination(player.position);
        }
    }
}