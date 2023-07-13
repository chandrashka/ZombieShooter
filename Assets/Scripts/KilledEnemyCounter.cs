using UnityEngine;

public class KilledEnemyCounter : MonoBehaviour
{
    private int m_KilledEnemies;

    public void EnemyKilled()
    {
        m_KilledEnemies++;
    }

    public void ResetCounter()
    {
        m_KilledEnemies = 0;
    }

    public int GetCounter()
    {
        return m_KilledEnemies;
    }
}