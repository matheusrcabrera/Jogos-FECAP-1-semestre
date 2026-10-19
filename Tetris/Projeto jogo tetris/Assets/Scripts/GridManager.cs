using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GridManager : MonoBehaviour
{
    public static int larg = 10;
    public static int alt = 20;
    public static Transform[,] grid = new Transform[larg, alt];

    public static GridManager instance;
    public GameObject[] pecasPrefabs;
    
    [Header("Elementos de UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public GameObject gameOverPanel;

    private int pontuacao = 0;
    private float tempoRestante = 600f; 
    private bool jogoAcabou = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        AtualizarUIPontuacao();
        SpawnNovaPeca();
    }

    void Update()
    {
        if (jogoAcabou) return;

        // Gestão do Temporizador de 10 minutos
        if (tempoRestante > 0)
        {
            tempoRestante -= Time.deltaTime;
            AtualizarUITemporizador(tempoRestante);
        }
        else
        {
            tempoRestante = 0;
            AtualizarUITemporizador(tempoRestante);
            ExecutarGameOver("Tempo Esgotado!");
        }
    }

    public void SpawnNovaPeca()
    {
        if (jogoAcabou) return;

        int index = Random.Range(0, pecasPrefabs.Length);
        Vector3 posSpawn = new Vector3(5, 18, 0);

        GameObject novaPeca = Instantiate(pecasPrefabs[index], posSpawn, Quaternion.identity);

        if (!PecaPosicaoValida(novaPeca.transform))
        {
            ExecutarGameOver("Tabuleiro Cheio!");
            Destroy(novaPeca);
        }
    }

    bool PecaPosicaoValida(Transform peca)
    {
        foreach (Transform bloco in peca)
        {
            int x = Mathf.RoundToInt(bloco.position.x);
            int y = Mathf.RoundToInt(bloco.position.y);

            if (x >= 0 && x < larg && y >= 0 && y < alt)
            {
                if (grid[x, y] != null)
                    return false;
            }
        }
        return true;
    }

    public void AdicionarAoGrid(Transform peca)
    {
        foreach (Transform bloco in peca)
        {
            int arredondadoX = Mathf.RoundToInt(bloco.position.x);
            int arredondadoY = Mathf.RoundToInt(bloco.position.y);

            if (arredondadoX >= 0 && arredondadoX < larg && arredondadoY >= 0 && arredondadoY < alt)
            {
                grid[arredondadoX, arredondadoY] = bloco;
            }
        }
    }

    public void VerificarLinhas()
    {
        int linhasLimpas = 0;

        for (int y = 0; y < alt; y++)
        {
            if (LinhaEstaCheia(y))
            {
                DeletarLinha(y);
                BaixarLinhasSuperiores(y + 1);
                y--;
                linhasLimpas++;
            }
        }

        if (linhasLimpas > 0)
        {
            CalcularPontuacao(linhasLimpas);
        }
    }

    bool LinhaEstaCheia(int y)
    {
        for (int x = 0; x < larg; x++)
        {
            if (grid[x, y] == null)
                return false;
        }
        return true;
    }

    void DeletarLinha(int y)
    {
        for (int x = 0; x < larg; x++)
        {
            Destroy(grid[x, y].gameObject);
            grid[x, y] = null;
        }
    }

    void BaixarLinhasSuperiores(int yInicio)
    {
        for (int y = yInicio; y < alt; y++)
        {
            for (int x = 0; x < larg; x++)
            {
                if (grid[x, y] != null)
                {
                    grid[x, y - 1] = grid[x, y];
                    grid[x, y] = null;
                    grid[x, y - 1].position += new Vector3(0, -1, 0);
                }
            }
        }
    }

    void CalcularPontuacao(int quantidadeLinhas)
    {
        // Sistema clássico de pontuação Tetris
        switch (quantidadeLinhas)
        {
            case 1: pontuacao += 100; break;
            case 2: pontuacao += 300; break;
            case 3: pontuacao += 500; break;
            case 4: pontuacao += 800; break; // Tetris!
        }

        AtualizarUIPontuacao();
    }

    void AtualizarUIPontuacao()
    {
        if (scoreText != null)
            scoreText.text = "SCORE: " + pontuacao;
    }

    void AtualizarUITemporizador(float tempoEmSegundos)
    {
        if (timerText == null) return;

        int minutos = Mathf.FloorToInt(tempoEmSegundos / 60);
        int segundos = Mathf.FloorToInt(tempoEmSegundos % 60);
        timerText.text = string.Format("TIME: {0:00}:{1:00}", minutos, segundos);
    }

    public void ExecutarGameOver(string motivo)
    {
        jogoAcabou = true;
        Time.timeScale = 0f; // Congela o jogo

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Debug.Log("Game Over: " + motivo);
    }
    public void VoltarParaGame()
    {
        Time.timeScale = 1f; // Reativa a velocidade normal do jogo (caso tenha pausado no Game Over)
        grid = new Transform[larg, alt]; // Limpa a matriz do grid
        SceneManager.LoadScene("Game"); // Nome exato da tua cena de jogo
    }

    // Método para o Botão "Menu Principal"
    public void VoltarParaMainMenu()
    {
        Time.timeScale = 1f; // Reativa o tempo normal antes de mudar de cena
        grid = new Transform[larg, alt]; // Limpa a matriz do grid
        SceneManager.LoadScene("MainMenu"); // Nome exato da tua cena de menu
    }

    // Método para ser chamado por um botão na UI de Reiniciar
    public void ReiniciarJogo()
    {
        Time.timeScale = 1f;
        // Limpa o grid em memória
        grid = new Transform[larg, alt];
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}