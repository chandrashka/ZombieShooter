using Opsive.UltimateCharacterController.Traits;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float damage;
    
    private static readonly int Moving = Animator.StringToHash("moving");
    private static readonly int Punch = Animator.StringToHash("punch");
    
    private float m_TimeToAttack;
    private const float TimeBetweenAttacks = 1f;
    
    public bool isKilled;

    public GameObject player;
    private LayerMask m_Masks;
    private NavMeshAgent m_NavMeshAgent;
    
    private void Start()
    {
        m_Masks = LayerMask.GetMask("Character", "SubCharacter");
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void FixedUpdate()
    {
        if (isKilled) return;
        if (Physics.Raycast(transform.position, transform.forward, 1f,
                m_Masks))
        {
            if (m_TimeToAttack <= 0)
            {
                Attack();
                m_TimeToAttack = TimeBetweenAttacks;
            }
            else
            {
                m_TimeToAttack -= Time.deltaTime;
            }
        }
        else
        {
            animator.SetBool(Moving, true);
            MoveToPlayer();
        }
    }

    private void Attack()
    {
        m_NavMeshAgent.isStopped = true;
        
        animator.SetTrigger(Punch);
        var health = player.GetComponent<CharacterHealth>();
        health.Damage(damage);
    }

    private void MoveToPlayer()
    {
        m_NavMeshAgent.isStopped = false;
        m_NavMeshAgent.SetDestination(player.transform.position);
    }
}