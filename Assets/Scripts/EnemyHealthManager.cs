using Opsive.UltimateCharacterController.Traits.Damage;
using UnityEngine;

public class EnemyHealthManager : MonoBehaviour, IDamageTarget
{
    [SerializeField] private float health;

    public bool killed = false;

    public EnemyManager enemyManager;

    public GameObject Owner { get; }
    public GameObject HitGameObject { get; }
    public void Damage(DamageData damageData)
    {
        health -= damageData.Amount;
        enemyManager.UpdateEnemyState(IsAlive(), gameObject, killed);
    }

    public bool IsAlive()
    {
        return health > 0;
    }
}
