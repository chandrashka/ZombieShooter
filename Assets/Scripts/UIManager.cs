using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI enemyCounter;
    [SerializeField] private TextMeshProUGUI resultText;
    
    [SerializeField] private GameObject startGameCanvas;
    [SerializeField] private GameObject endGameCanvas;
    [SerializeField] private List<GameObject> gameCanvasObjects;
    [SerializeField] private GameObject pausedGameCanvas;

    private void Start()
    {
        startGameCanvas.SetActive(true);
        endGameCanvas.SetActive(false);
        pausedGameCanvas.SetActive(false);
        foreach (var obj in gameCanvasObjects) obj.SetActive(false);
    }

    public void UpdateEnemyCounter(int counter)
    {
        enemyCounter.text = counter.ToString();
    }

    public void StartGame()
    {
        startGameCanvas.SetActive(false);
        endGameCanvas.SetActive(false);
        pausedGameCanvas.SetActive(false);
        foreach (var obj in gameCanvasObjects) obj.SetActive(true);
    }

    public void PauseGame()
    {
        startGameCanvas.SetActive(false);
        endGameCanvas.SetActive(false);
        pausedGameCanvas.SetActive(true);
        foreach (var obj in gameCanvasObjects) obj.SetActive(false);
    }

    public void UnPauseGame()
    {
        startGameCanvas.SetActive(false);
        endGameCanvas.SetActive(false);
        pausedGameCanvas.SetActive(false);
        foreach (var obj in gameCanvasObjects) obj.SetActive(true);
    }

    public void EndGame()
    {
        startGameCanvas.SetActive(false);
        endGameCanvas.SetActive(true);
        pausedGameCanvas.SetActive(false);
        foreach (var obj in gameCanvasObjects) obj.SetActive(false);

        resultText.text = "Killed enemies: " + enemyCounter.text;
        enemyCounter.text = "0";
    }
}