using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    public Transform player;

    public bool waitForPlayer = false;
    public float detectionRange = 8f;

    private NavMeshAgent navMeshAgent;
    private bool chaseStarted;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        chaseStarted = !waitForPlayer;
    }

    void Update()
    {
        if (!navMeshAgent.isOnNavMesh)
        {
            return;
        }

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