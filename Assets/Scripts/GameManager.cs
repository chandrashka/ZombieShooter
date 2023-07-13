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
    private bool m_GamePaused;

    private PlayerManager m_PlayerManager;

    private void Awake()
    {
        m_PlayerManager = player.GetComponent<PlayerManager>();
        Time.timeScale = 0;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (m_IsGameOn && m_PlayerDead && !m_GamePaused)
            {
                audioSource.PlayOneShot(buttonSound);
                RestartGame();
            }
            else if (m_GamePaused)
            {
                audioSource.PlayOneShot(buttonSound);
                uiManager.UnPauseGame();
                m_GamePaused = false;
                Time.timeScale = 1;
            }
            else
            {
                audioSource.PlayOneShot(buttonSound);
                StartGame();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            if (m_GamePaused || m_PlayerDead)
            {
                audioSource.PlayOneShot(buttonSound);
                Application.Quit();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!m_IsGameOn) return;
            audioSource.PlayOneShot(buttonSound);
            uiManager.PauseGame();
            m_GamePaused = true;
            Time.timeScale = 0;
        }
    }

    private void StartGame()
    {
        Time.timeScale = 1;
        m_IsGameOn = true;
        m_PlayerDead = false;

        m_PlayerManager.RespawnPlayer();
        enemyManager.StartGame(player);
        uiManager.StartGame();
    }

    private void RestartGame()
    {
        Time.timeScale = 1;
        m_IsGameOn = true;
        m_PlayerDead = false;

        m_PlayerManager.RespawnPlayer();
        uiManager.StartGame();
    }


    public void EndGame()
    {
        Time.timeScale = 0;
        m_IsGameOn = false;
        m_PlayerDead = true;

        enemyManager.ResetGame();
        uiManager.EndGame();
    }
}