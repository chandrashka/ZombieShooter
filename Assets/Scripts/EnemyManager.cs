using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private KilledEnemyCounter m_KilledEnemyCounter;
    private EnemySpawner m_EnemySpawner;
    
    [SerializeField] private UIManager uiManager;
    [SerializeField] private float timeToSpawnNewEnemy;
    
    private float m_CurrentTimeToSpawnNewEnemy;

    private readonly List<GameObject> m_Enemies = new ();

    private bool m_GameOn;

    private void Start()
    {
        m_KilledEnemyCounter = GetComponent<KilledEnemyCounter>();
        m_EnemySpawner = GetComponent<EnemySpawner>();
        m_CurrentTimeToSpawnNewEnemy = timeToSpawnNewEnemy;
    }

    public void StartGame(GameObject player)
    {
        m_EnemySpawner.SpawnEnemy();
        m_EnemySpawner.player = player;
        m_GameOn = true;
    }
    
    private void Update()
    {
        if(!m_GameOn) return;
        if (m_CurrentTimeToSpawnNewEnemy <= 0)
        {
            m_Enemies.Add(m_EnemySpawner.SpawnEnemy());
            m_CurrentTimeToSpawnNewEnemy = timeToSpawnNewEnemy;
        }
        else
        {
            m_CurrentTimeToSpawnNewEnemy -= Time.deltaTime;
        }
    }

    public void UpdateEnemyState(bool isAlive, GameObject enemy, bool killed)
    {
        if (isAlive) return;
        if(killed) return;

        enemy.GetComponent<EnemyHealthManager>().killed = true;
        
        KillEnemy(enemy);
        m_KilledEnemyCounter.EnemyKilled();
        
        uiManager.UpdateEnemyCounter(m_KilledEnemyCounter.GetCounter());
    }

    private void KillEnemy(GameObject enemy)
    {
        var enemyRagdoll = enemy.GetComponent<RagdollController>();
        enemyRagdoll.MakePhysical();
        Destroy(enemy, 10f);
    }

    public void ResetGame()
    {
        m_KilledEnemyCounter.ResetCounter();
        m_Enemies.Clear();
    }
}
