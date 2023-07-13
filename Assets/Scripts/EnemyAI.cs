using Opsive.UltimateCharacterController.Traits;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Animator animator;
    [SerializeField] private float damage;
    
    private static readonly int Moving = Animator.StringToHash("moving");
    private static readonly int Punch = Animator.StringToHash("punch");
    
    private float m_TimeToAttack;
    private const float TimeBetweenAttacks = 1f;
    
    public bool isKilled;
    private Rigidbody m_Rigidbody;
    
    public GameObject player;
    
    private void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (isKilled) return;
        if (Physics.Raycast(transform.position, transform.forward, 1f,
                LayerMask.GetMask("Character", "SubCharacter")))
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
        animator.SetTrigger(Punch);
        var health = player.GetComponent<CharacterHealth>();
        health.Damage(damage);
    }

    private void MoveToPlayer()
    {
        var position = Vector3.MoveTowards(transform.position, player.transform.position,
            speed * Time.deltaTime);
        m_Rigidbody.MovePosition(position);
        transform.LookAt(player.transform);
    }
}