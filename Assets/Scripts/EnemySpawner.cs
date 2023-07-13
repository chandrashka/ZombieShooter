using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> enemyPrefabs;
    [SerializeField] private List<Transform> spawnPoints;
    
    public GameObject player;
    
    private EnemyManager m_EnemyManager;
    
    private Random m_Random;

    private void Start()
    {
        m_Random = new Random();
        m_EnemyManager = GetComponent<EnemyManager>();
    }

    public GameObject SpawnEnemy()
    {
        var enemyPrefab = enemyPrefabs[m_Random.Next(0, enemyPrefabs.Count)];
        var spawnPoint = spawnPoints[m_Random.Next(0, spawnPoints.Count)];

        var enemy = Instantiate(enemyPrefab, spawnPoint);
        
        enemy.GetComponent<EnemyAI>().player = player;
        enemy.GetComponent<EnemyHealthManager>().enemyManager = m_EnemyManager;

        return enemy;
    }
}