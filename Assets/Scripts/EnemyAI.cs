using Opsive.UltimateCharacterController.Traits;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private float speed;
    public GameObject player;
    private Rigidbody m_Rigidbody;
    [SerializeField] private Animator animator;
    private static readonly int Moving = Animator.StringToHash("moving");
    private static readonly int Punch = Animator.StringToHash("punch");
    private const float Damage = 0.1f;

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }
    
    void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, transform.forward, 1f,  
                LayerMask.GetMask("Character", "SubCharacter")))
        {
            animator.SetTrigger(Punch);
            var health = player.GetComponent<CharacterHealth>();
            health.Damage(Damage);
        }
        else
        {
            animator.SetBool(Moving, true);
            MoveToPlayer();
        }
    }

    private void MoveToPlayer()
    {
        var position = Vector3.MoveTowards(transform.position, player.transform.position,
            speed * Time.deltaTime);
        m_Rigidbody.MovePosition(position);
        transform.LookAt(player.transform);
    }
}
