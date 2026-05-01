using UnityEngine;
using UnityEngine.SceneManagement;
public class Main_Menu : MonoBehaviour
{
    [SerializeField] private GameObject creditPanel;

    public void OpenCredit()
    {
        creditPanel.SetActive(true);
    }

    public void CloseCredit()
    {
        creditPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Level Testing_2");
    }
}