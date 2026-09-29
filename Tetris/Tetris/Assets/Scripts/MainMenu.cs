using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject HowToPlayPanel;
    public GameObject MainMenuPanel; // Crie esta variável para o menu principal

    public void jogar()
    {
        SceneManager.LoadScene("Game");
    }

    public void ComoJogar()
    {
        HowToPlayPanel.SetActive(true);
        if (MainMenuPanel != null)
        {
            MainMenuPanel.SetActive(false); // Esconde o menu principal
        }
    }

    public void Voltar()
    {
        HowToPlayPanel.SetActive(true);
        if (MainMenuPanel != null)
        {
            MainMenuPanel.SetActive(true); // Exibe o menu principal de volta
        }
    }

    public void Sair()
    {
        Application.Quit();
    }
}