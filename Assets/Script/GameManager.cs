using UnityEngine;
using System.Collections;

/// <summary>
/// Simple Game Over manager - shows panel, waits, then auto-respawns player.
/// No buttons needed.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public PlayerMovement player;

    public GameObject gameOverPanel;

    public float gameOverDuration = 2f;

    public bool resetCoinsOnRespawn = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.GetComponent<PlayerMovement>();
            }
        }
    }

   
    public void ShowGameOver()
    {
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        yield return new WaitForSecondsRealtime(gameOverDuration);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (resetCoinsOnRespawn && CoinManager.Instance != null)
        {
            CoinManager.Instance.ResetCoins();
        }

        if (player != null)
        {
            player.Respawn();
        }
    }
}