using UnityEngine;
using UnityEngine.AI;

public class PetFollow : MonoBehaviour
{
    public Transform player;
    public Animator petAnimator;

    private NavMeshAgent agent;
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (petAnimator == null)
        {
            petAnimator = GetComponentInChildren<Animator>();
        }
    }

    void Update()
    {
        if (agent == null || !agent.isOnNavMesh)
        {
            SetMovingAnimation(false);
            return;
        }

        if (player == null)
        {
            agent.isStopped = true;
            SetMovingAnimation(false);
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(player.position);

        // Animate from the pet's actual movement, including when its path is blocked.
        SetMovingAnimation(agent.velocity.sqrMagnitude > 0.01f);
    }

    void SetMovingAnimation(bool moving)
    {
        if (petAnimator != null)
        {
            petAnimator.SetBool(IsMoving, moving);
        }
    }
}