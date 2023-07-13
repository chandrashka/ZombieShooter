using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI enemyCounter;
    [SerializeField] private TextMeshProUGUI resultText;
    
    [SerializeField] private GameObject startScreenCanvas;
    [SerializeField] private GameObject endGameCanvas;
    [SerializeField] private List<GameObject> gameCanvasObjects;

    private void Start()
    {
        startScreenCanvas.SetActive(true);
        endGameCanvas.SetActive(false);
        foreach (var obj in gameCanvasObjects) obj.SetActive(false);
    }

    public void UpdateEnemyCounter(int counter)
    {
        enemyCounter.text = counter.ToString();
    }

    public void StartGame()
    {
        startScreenCanvas.SetActive(false);
        endGameCanvas.SetActive(false);
        foreach (var obj in gameCanvasObjects) obj.SetActive(true);
    }

    public void EndGame()
    {
        startScreenCanvas.SetActive(false);
        endGameCanvas.SetActive(true);
        foreach (var obj in gameCanvasObjects) obj.SetActive(false);

        resultText.text = "Killed enemies: " + enemyCounter.text;
    }
}