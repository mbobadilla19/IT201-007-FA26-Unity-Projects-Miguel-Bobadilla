using UnityEngine;
using UnityEngine.AI;

public class PetFollow : MonoBehaviour
{
    public Transform player;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (agent == null || !agent.isOnNavMesh)
        {
            return;
        }

        if (player == null)
        {
            agent.isStopped = true;
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(player.position);
    }
}