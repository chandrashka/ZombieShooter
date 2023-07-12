using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> enemyPrefabs;
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private GameObject player;
    [SerializeField] private float timeToSpawnNewEnemy;
    private float m_CurrentTimeToSpawnNewEnemy;
    private System.Random m_Random;

    private void Start()
    {
        m_Random = new System.Random();
        m_CurrentTimeToSpawnNewEnemy = timeToSpawnNewEnemy;
        SpawnEnemy();
    }

    private void Update()
    {
        if (m_CurrentTimeToSpawnNewEnemy <= 0)
        {
            SpawnEnemy();
            m_CurrentTimeToSpawnNewEnemy = timeToSpawnNewEnemy;
        }
        else
        {
            m_CurrentTimeToSpawnNewEnemy -= Time.deltaTime;
        }
    }

    private void SpawnEnemy()
    {
        var enemyPrefab = enemyPrefabs[m_Random.Next(0, enemyPrefabs.Count)];
        var spawnPoint = spawnPoints[m_Random.Next(0, spawnPoints.Count)];

        var enemy = Instantiate(enemyPrefab, spawnPoint);
        enemy.GetComponent<EnemyAI>().player = player;
    }
}
