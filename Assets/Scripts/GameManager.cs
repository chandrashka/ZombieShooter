using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private GameObject player;

    public void StartGame()
    {
        enemyManager.StartGame(player);
        uiManager.StartGame();
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void RestartGame()
    {
        enemyManager.ResetGame();
        uiManager.StartGame();
    }
    

    public void EndGame()
    {
        uiManager.EndGame();
    }
}
