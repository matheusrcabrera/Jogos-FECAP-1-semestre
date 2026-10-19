using UnityEngine;
using TMPro; // Permite alterar textos da interface (placar, relógio e Game Over).
using UnityEngine.SceneManagement; // Permite trocar ou recarregar cenas do Unity.

// ESTA CLASSE COORDENA UM TABULEIRO: sabe onde cada bloco está, cria peças,
// calcula pontos e encerra a partida. No modo versus existe um GridManager por jogador.

/// <summary>
/// Controla um tabuleiro de Tetris. No modo versus, cada jogador tem seu próprio
/// GridManager, matriz de blocos e pontuação.
/// </summary>
public class GridManager : MonoBehaviour
{
    // Cada tabuleiro tem 10 colunas (largura) e 20 linhas (altura).
    // const significa que esses números são fixos: não mudam durante a partida.
    public const int larg = 10;
    public const int alt = 20;

    // Estes campos aparecem no Inspector do Unity.
    // playerNumber identifica o dono deste tabuleiro: 1 ou 2 nesta versão.
    // boardOffsetX desloca o tabuleiro 2 para a direita sem alterar a matriz interna.
    [Header("Configuração do jogador")]
    [Tooltip("1 = jogador da esquerda; 2 = jogador da direita.")]
    public int playerNumber = 1;
    [Tooltip("Deslocamento horizontal do tabuleiro no mundo. O jogador 2 usa 12.")]
    public int boardOffsetX = 0;

    // Prefabs são modelos prontos de peças que o Unity duplica quando uma nova peça nasce.
    [Header("Peças")]
    public GameObject[] pecasPrefabs;

    // Referências aos elementos visuais. Elas são ligadas no Inspector da cena.
    [Header("Elementos de UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public GameObject gameOverPanel;

    // A matriz é a memória do tabuleiro. Cada posição guarda um bloco ocupado ou null (vazio).
    // Ela NÃO é static: cada GridManager tem sua própria matriz independente.
    private readonly Transform[,] grid = new Transform[larg, alt];
    private int pontuacao;
    private float tempoRestante = 600f;
    private bool jogoAcabou;
    private bool iniciado;

    // Dados compartilhados entre os dois tabuleiros. static significa que existe uma única
    // cópia desses dados para toda a partida, não uma cópia por jogador.
    // ATENÇÃO PARA 3 JOGADORES: este array e vários usos abaixo ainda assumem 2 jogadores.
    private static GridManager[] jogadores = new GridManager[2];
    private static bool partidaEncerrada;
    private static int modoAtual = 1;
    private const int deslocamentoEntreTabuleiros = 12;

    public int PlayerNumber => playerNumber;
    public int Pontuacao => pontuacao;
    public bool JogoAcabou => jogoAcabou;

    // Start é chamado automaticamente pelo Unity quando o objeto começa a jogar.
    private void Start()
    {
        // Alguns painéis da cena têm componentes GridManager antigos/vazios.
        // Sem prefabs de peça, este objeto não é um tabuleiro ativo e deve parar aqui.
        if (pecasPrefabs == null || pecasPrefabs.Length == 0)
            return;

        // Só o jogador 1 inicializa os dados compartilhados uma vez por partida.
        if (playerNumber == 1)
        {
            Time.timeScale = 1f; // Garante que um Game Over anterior não deixe o jogo pausado.
            jogadores = new GridManager[2]; // Dois espaços: índice 0 = P1, índice 1 = P2.
            partidaEncerrada = false;
            // PlayerPrefs é uma pequena memória do Unity: o menu salva 1 ou 2 antes de abrir esta cena.
            modoAtual = PlayerPrefs.GetInt("GameMode", 1) == 2 ? 2 : 1;
        }

        // Arrays começam no índice 0. Por isso P1 vai em 0 e P2 vai em 1.
        jogadores[playerNumber - 1] = this;
        iniciado = true;

        if (gameOverPanel != null && playerNumber == 1)
            gameOverPanel.SetActive(false);

        AtualizarUIPontuacao();
        AtualizarUITemporizador(tempoRestante);

        // Quando o modo escolhido é versus, o P1 cria o objeto responsável pelo P2.
        // Para 3 jogadores, esta lógica precisa virar um método que crie P2 e P3 em um loop.
        if (playerNumber == 1 && modoAtual == 2)
            CriarSegundoJogador();

        // Cada tabuleiro cria a sua própria primeira peça.
        SpawnNovaPeca();
    }

    // Update roda muitas vezes por segundo. Aqui o P1 atualiza o relógio compartilhado.
    private void Update()
    {
        if (!iniciado || jogoAcabou || partidaEncerrada || playerNumber != 1)
            return;

        // O relógio é compartilhado no modo versus para que os dois jogadores
        // tenham exatamente o mesmo limite de tempo.
        if (tempoRestante > 0f)
        {
            tempoRestante -= Time.deltaTime;
            if (tempoRestante < 0f)
                tempoRestante = 0f;

            AtualizarUITemporizador(tempoRestante);
        }
        else
        {
            EncerrarPorTempo();
        }
    }

    // Cria tudo que o segundo jogador precisa: fundo visual, posição da câmera,
    // placar e um novo GridManager com matriz de blocos independente.
    // Este método é específico para P2; ainda não cria uma quantidade variável de jogadores.
    private void CriarSegundoJogador()
    {
        // Duplica somente o objeto visual de fundo do tabuleiro. As peças e a
        // lógica de jogo do segundo jogador são controladas por outro manager.
        GameObject tabuleiroVisual = GameObject.Find("GridLines");
        if (tabuleiroVisual != null)
        {
            Instantiate(
                tabuleiroVisual,
                tabuleiroVisual.transform.position + Vector3.right * deslocamentoEntreTabuleiros,
                tabuleiroVisual.transform.rotation,
                tabuleiroVisual.transform.parent);
        }
        else
        {
            Debug.LogWarning("Não encontrei o objeto 'GridLines'. O segundo tabuleiro funcionará, mas pode não ter fundo visual.");
        }

        // Move a câmera para centralizar os dois tabuleiros lado a lado.
        Camera cameraPrincipal = Camera.main;
        if (cameraPrincipal != null)
        {
            Vector3 posicaoCamera = cameraPrincipal.transform.position;
            posicaoCamera.x = 4.5f + deslocamentoEntreTabuleiros / 2f;
            cameraPrincipal.transform.position = posicaoCamera;
        }

        TextMeshProUGUI placarJogador2 = CriarTextoPlacarJogador2();

        // Um GameObject é um objeto na cena. AddComponent acrescenta a lógica GridManager.
        // O novo manager recebe os mesmos prefabs, mas tem outra matriz e outra pontuação.
        GameObject objetoJogador2 = new GameObject("GridManager_Player2");
        GridManager jogador2 = objetoJogador2.AddComponent<GridManager>();
        jogador2.playerNumber = 2;
        jogador2.boardOffsetX = deslocamentoEntreTabuleiros;
        jogador2.pecasPrefabs = pecasPrefabs;
        jogador2.scoreText = placarJogador2;
        jogador2.timerText = timerText;
        jogador2.gameOverPanel = null;
    }

    // Duplica o texto do placar já existente e muda sua posição para o P2.
    // A posição é fixa; para P3 é preciso reservar e posicionar um terceiro placar.
    private TextMeshProUGUI CriarTextoPlacarJogador2()
    {
        if (scoreText == null)
            return null;

        TextMeshProUGUI novoPlacar = Instantiate(scoreText, scoreText.transform.parent);
        novoPlacar.name = "Player2ScoreText";
        novoPlacar.raycastTarget = false;

        RectTransform rect = novoPlacar.rectTransform;
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        // O temporizador existente fica no canto superior direito; o placar do
        // jogador 2 fica um pouco à esquerda dele.
        rect.anchoredPosition = new Vector2(-250f, 0f);
        rect.sizeDelta = new Vector2(230f, 50f);
        novoPlacar.text = "P2 SCORE: 0";
        return novoPlacar;
    }

    // Escolhe uma peça aleatória e a cria no topo do tabuleiro deste jogador.
    public void SpawnNovaPeca()
    {
        if (jogoAcabou || partidaEncerrada || pecasPrefabs == null || pecasPrefabs.Length == 0)
            return;

        int index = Random.Range(0, pecasPrefabs.Length);
        // O +5 leva a peça para o meio das 10 colunas; o deslocamento move o tabuleiro P2.
        Vector3 posSpawn = new Vector3(boardOffsetX + 5, 18, 0);
        GameObject novaPeca = Instantiate(pecasPrefabs[index], posSpawn, Quaternion.identity);

        Piece scriptPeca = novaPeca.GetComponent<Piece>();
        if (scriptPeca != null)
            scriptPeca.Configurar(this);
        else
            Debug.LogError("A peça do prefab não possui o script Piece: " + novaPeca.name);

        if (!MovimentoValido(novaPeca.transform))
        {
            Destroy(novaPeca);
            EncerrarPorTabuleiroCheio();
        }
    }

    /// <summary>Valida a posição de uma peça no tabuleiro associado a este manager.</summary>
    // Pergunta: esta peça pode ficar nesta posição?
    // Retorna true se não saiu pelas laterais/embaixo e não bateu em blocos já fixos.
    public bool MovimentoValido(Transform peca)
    {
        foreach (Transform bloco in peca)
        {
            // A posição real do P2 inclui seu deslocamento horizontal. Subtraímos esse
            // deslocamento para consultar a coluna equivalente na matriz local 0..9.
            int x = Mathf.RoundToInt(bloco.position.x) - boardOffsetX;
            int y = Mathf.RoundToInt(bloco.position.y);

            if (x < 0 || x >= larg || y < 0)
                return false;

            // Permite que a parte superior da peça fique acima do tabuleiro ao
            // nascer. Assim que o bloco entra na área visível, a colisão é checada.
            if (y < alt && grid[x, y] != null)
                return false;
        }
        return true;
    }

    // Quando a peça não pode mais descer, registra cada um dos seus blocos na matriz.
    private void AdicionarAoGrid(Transform peca)
    {
        foreach (Transform bloco in peca)
        {
            int x = Mathf.RoundToInt(bloco.position.x) - boardOffsetX;
            int y = Mathf.RoundToInt(bloco.position.y);

            if (x >= 0 && x < larg && y >= 0 && y < alt)
                grid[x, y] = bloco;
        }
    }

    /// <summary>Trava uma peça e decide se o jogo continua ou termina.</summary>
    // "Travar" significa transformar a peça móvel em parte fixa do tabuleiro.
    // Depois disso, procura linhas completas e cria a peça seguinte.
    public void TravarPeca(Transform peca)
    {
        if (jogoAcabou || partidaEncerrada)
            return;

        foreach (Transform bloco in peca)
        {
            int y = Mathf.RoundToInt(bloco.position.y);
            if (y >= alt)
            {
                EncerrarPorTabuleiroCheio();
                return;
            }
        }

        AdicionarAoGrid(peca);
        VerificarLinhas();
        SpawnNovaPeca();
    }

    // Percorre todas as linhas. Se uma estiver cheia, apaga-a e desce as que estão acima.
    private void VerificarLinhas()
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
            CalcularPontuacao(linhasLimpas);
    }

    // Olha todas as colunas de uma linha. Se encontrar um espaço vazio, retorna false.
    private bool LinhaEstaCheia(int y)
    {
        for (int x = 0; x < larg; x++)
        {
            if (grid[x, y] == null)
                return false;
        }
        return true;
    }

    // Apaga os blocos visuais e marca cada posição da matriz como vazia (null).
    private void DeletarLinha(int y)
    {
        for (int x = 0; x < larg; x++)
        {
            if (grid[x, y] != null)
                Destroy(grid[x, y].gameObject);
            grid[x, y] = null;
        }
    }

    // Depois de apagar uma linha, move todos os blocos acima uma posição para baixo.
    private void BaixarLinhasSuperiores(int yInicio)
    {
        for (int y = yInicio; y < alt; y++)
        {
            for (int x = 0; x < larg; x++)
            {
                if (grid[x, y] != null)
                {
                    grid[x, y - 1] = grid[x, y];
                    grid[x, y] = null;
                    grid[x, y - 1].position += Vector3.down;
                }
            }
        }
    }

    // Converte a quantidade de linhas removidas em pontos e atualiza o placar.
    private void CalcularPontuacao(int quantidadeLinhas)
    {
        switch (quantidadeLinhas)
        {
            case 1: pontuacao += 100; break;
            case 2: pontuacao += 300; break;
            case 3: pontuacao += 500; break;
            case 4: pontuacao += 800; break;
            default: pontuacao += quantidadeLinhas * 200; break;
        }

        AtualizarUIPontuacao();
    }

    // Atualiza o texto do placar. O texto pode não existir em objetos auxiliares.
    private void AtualizarUIPontuacao()
    {
        if (scoreText == null)
            return;

        if (modoAtual == 2)
            scoreText.text = (playerNumber == 1 ? "P1 SCORE: " : "P2 SCORE: ") + pontuacao;
        else
            scoreText.text = "Placar: " + pontuacao;
    }

    // Transforma segundos em um texto MM:SS, por exemplo 09:35.
    private void AtualizarUITemporizador(float tempoEmSegundos)
    {
        if (timerText == null)
            return;

        int minutos = Mathf.FloorToInt(tempoEmSegundos / 60f);
        int segundos = Mathf.FloorToInt(tempoEmSegundos % 60f);
        timerText.text = string.Format("TIME: {0:00}:{1:00}", minutos, segundos);
    }

    // Decide como termina a partida quando uma nova peça não cabe ou passa do topo.
    // A regra atual funciona para 1 ou 2 jogadores; com 3, a regra precisa ser redefinida.
    private void EncerrarPorTabuleiroCheio()
    {
        if (modoAtual == 2)
        {
            int vencedor = playerNumber == 1 ? 2 : 1;
            FinalizarPartida("JOGADOR " + vencedor + " VENCEU!", "O tabuleiro do jogador " + playerNumber + " ficou cheio.");
        }
        else
        {
            FinalizarPartida("FIM DE JOGO", "Tabuleiro cheio!");
        }
    }

    // Quando o relógio chega a zero, compara os pontos e escolhe o resultado.
    // A comparação atual cita P1 e P2 pelo nome; não basta só acrescentar P3 ao array.
    private void EncerrarPorTempo()
    {
        if (modoAtual != 2)
        {
            FinalizarPartida("TEMPO ESGOTADO!", "O limite de 10 minutos terminou.");
            return;
        }

        GridManager jogador2 = jogadores[1];
        if (jogador2 == null)
        {
            FinalizarPartida("TEMPO ESGOTADO!", "O limite de 10 minutos terminou.");
            return;
        }

        if (pontuacao > jogador2.pontuacao)
            FinalizarPartida("JOGADOR 1 VENCEU PELOS PONTOS!", "O tempo terminou.");
        else if (jogador2.pontuacao > pontuacao)
            FinalizarPartida("JOGADOR 2 VENCEU PELOS PONTOS!", "O tempo terminou.");
        else
            FinalizarPartida("EMPATE!", "O tempo terminou com a mesma pontuação.");
    }

    // Para a partida toda, congela o tempo e exibe o painel final.
    // static permite chamar este método sem depender de um tabuleiro específico.
    private static void FinalizarPartida(string mensagem, string motivo)
    {
        if (partidaEncerrada)
            return;

        partidaEncerrada = true;
        Time.timeScale = 0f;

        foreach (GridManager jogador in jogadores)
        {
            if (jogador != null)
                jogador.jogoAcabou = true;
        }

        GridManager controladorUI = jogadores[0];
        if (controladorUI != null && controladorUI.gameOverPanel != null)
        {
            TextMeshProUGUI textoFim = null;
            TextMeshProUGUI[] textos = controladorUI.gameOverPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (TextMeshProUGUI texto in textos)
            {
                if (texto.gameObject.name.Contains("GameOverText"))
                {
                    textoFim = texto;
                    break;
                }
            }

            if (textoFim != null)
                textoFim.text = mensagem;

            controladorUI.gameOverPanel.SetActive(true);
        }

        Debug.Log("Fim de jogo: " + mensagem + " Motivo: " + motivo);
    }

    // Métodos públicos abaixo podem ser ligados aos botões da interface pelo Inspector.
    public void VoltarParaGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }

    public void VoltarParaMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void ReiniciarJogo()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
