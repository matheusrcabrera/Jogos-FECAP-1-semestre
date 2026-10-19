using UnityEngine;
using UnityEngine.SceneManagement;

// Este script controla os botões do menu: abrir telas e escolher quantos jogadores.
// Ele não cria tabuleiros; apenas guarda a escolha e abre a cena chamada Game.
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

    // Método ligado ao botão "1 Jogador" pelo Inspector do Unity.
    public void IniciarUmJogador()
    {
        Time.timeScale = 1f; // Se voltamos de um Game Over, devolve o tempo à velocidade normal.
        // PlayerPrefs guarda um pequeno dado para a próxima cena conseguir ler a escolha.
        // O número 1 identifica o modo solo.
        PlayerPrefs.SetInt("GameMode", 1);
        PlayerPrefs.Save();

        // Carrega a cena do jogo (garante que o nome corresponda exatamente)
        SceneManager.LoadScene("Game");
    }

    // Método ligado ao botão "2 Jogadores". O número 2 é entendido por GridManager.
    public void IniciarDoisJogadores()
    {
        Time.timeScale = 1f;
        // Guarda o modo 2 (versus). GridManager lê esse valor quando a cena Game começa.
        PlayerPrefs.SetInt("GameMode", 2);
        PlayerPrefs.Save();

        // Carrega a cena do jogo
        SceneManager.LoadScene("Game");
    }

    // Os métodos abaixo apenas mostram ou escondem painéis de menu.
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

    // Fecha o jogo construído. No Editor, também para o modo Play para facilitar testes.
    public void Sair()
    {
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}