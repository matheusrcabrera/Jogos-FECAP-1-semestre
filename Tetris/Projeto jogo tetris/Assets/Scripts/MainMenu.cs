using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Painéis do Menu")]
    public GameObject HowToPlayPanel;
    public GameObject PlayerSelectionPanel;
    public GameObject MainMenuPanel;

    // Método que o botão "Jogar" principal do menu chama
    public void AbrirSelecaoJogadores()
    {
        if (PlayerSelectionPanel != null)
        {
            PlayerSelectionPanel.SetActive(true);
            MainMenuPanel.SetActive(false);
        }
    }

    // Método para fechar a tela de seleção e voltar ao menu
    public void FecharSelecaoJogadores()
    {
        if (PlayerSelectionPanel != null)
        {
            MainMenuPanel.SetActive(true);
            PlayerSelectionPanel.SetActive(false);
        }
    }

    // Método ao clicar em "1 Jogador"
    public void IniciarUmJogador()
    {
        // Salva a preferência (1 jogador) para ser lida na cena do jogo
        PlayerPrefs.SetInt("GameMode", 1);
        PlayerPrefs.Save();

        // Carrega a cena do jogo (garante que o nome corresponda exatamente)
        SceneManager.LoadScene("Game");
    }

    // Método ao clicar em "2 Jogadores"
    public void IniciarDoisJogadores()
    {
        // Salva a preferência (2 jogadores) para ser lida na cena do jogo
        PlayerPrefs.SetInt("GameMode", 2);
        PlayerPrefs.Save();

        // Carrega a cena do jogo
        SceneManager.LoadScene("Game");
    }

    public void ComoJogar()
    {
        if (HowToPlayPanel != null)
        {
            MainMenuPanel.SetActive(false);
            HowToPlayPanel.SetActive(true);
        }
    }

    public void Voltar()
    {
        if (HowToPlayPanel != null)
        {
            MainMenuPanel.SetActive(true);
            HowToPlayPanel.SetActive(false);
        }
    }

    public void Sair()
    {
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}