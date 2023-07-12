using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private GameObject player;

    [SerializeField] private AudioClip buttonSound;
    [SerializeField] private AudioSource audioSource;

    private bool m_IsGameOn;
    private bool m_PlayerDead;
    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.F)) return;
        if (m_IsGameOn && m_PlayerDead)
        {
            RestartGame();
            audioSource.PlayOneShot(buttonSound);
        }
        else
        {
            StartGame();
            audioSource.PlayOneShot(buttonSound);
        }
    }

    private void StartGame()
    {
        m_IsGameOn = true;
        m_PlayerDead = false;
        
        enemyManager.StartGame(player);
        uiManager.StartGame();
    }

    private void RestartGame()
    {
        m_IsGameOn = true;
        m_PlayerDead = false;
        
        enemyManager.ResetGame();
        uiManager.StartGame();
    }
    

    public void EndGame()
    {
        m_IsGameOn = false;
        m_PlayerDead = true;
        uiManager.EndGame();
    }
}
