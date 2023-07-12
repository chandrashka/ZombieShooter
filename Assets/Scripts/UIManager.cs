using System;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI enemyCounter;
    [SerializeField] private GameObject startScreenCanvas;
    [SerializeField] private GameObject gameCanvas;
    [SerializeField] private GameObject endGameCanvas;
    
    public void UpdateEnemyCounter(int counter)
    {
        enemyCounter.text = counter.ToString();
    }

    private void Start()
    {
        startScreenCanvas.SetActive(true);
        gameCanvas.SetActive(false);
        endGameCanvas.SetActive(false);
    }

    public void StartGame()
    {
        startScreenCanvas.SetActive(false);
        gameCanvas.SetActive(true);
        endGameCanvas.SetActive(false);
    }

    public void EndGame()
    {
        startScreenCanvas.SetActive(false);
        gameCanvas.SetActive(false);
        endGameCanvas.SetActive(true);
    }
}
