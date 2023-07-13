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

    private PlayerManager m_PlayerManager;

    private void Awake()
    {
        m_PlayerManager = player.GetComponent<PlayerManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
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
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!m_PlayerDead && !m_IsGameOn) return;
            audioSource.PlayOneShot(buttonSound);
            Application.Quit();
        }
    }

    private void StartGame()
    {
        m_IsGameOn = true;
        m_PlayerDead = false;

        m_PlayerManager.RespawnPlayer();
        enemyManager.StartGame(player);
        uiManager.StartGame();
    }

    private void RestartGame()
    {
        m_IsGameOn = true;
        m_PlayerDead = false;

        m_PlayerManager.RespawnPlayer();
        uiManager.StartGame();
    }


    public void EndGame()
    {
        m_IsGameOn = false;
        m_PlayerDead = true;

        enemyManager.ResetGame();
        uiManager.EndGame();
    }
}