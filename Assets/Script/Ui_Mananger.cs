using UnityEngine;

public class Ui_Mananger : MonoBehaviour
{
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject panel;
    private PlayerMovement playerMovement;

    void Start()
    {
        playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement == null)
        {
            if(gameOverUI != null)
                gameOverUI.SetActive(false);
                
        }
    }

    void Update()
    {
        if (playerMovement != null && !playerMovement.CanMove)
        {
            if(gameOverUI != null)
                gameOverUI.SetActive(true);
                panel.SetActive(true);
        }
    }
}
