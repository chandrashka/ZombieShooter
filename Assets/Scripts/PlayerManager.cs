using Opsive.UltimateCharacterController.Traits;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    private Health m_Health;
    private Respawner m_Respawner;

    private void Awake()
    {
        m_Respawner = GetComponent<Respawner>();
        m_Health = GetComponent<Health>();
    }

    private void FixedUpdate()
    {
        if (m_Health.HealthValue <= 0) gameManager.EndGame();
    }

    public void RespawnPlayer()
    {
        m_Respawner.Respawn();
    }
}