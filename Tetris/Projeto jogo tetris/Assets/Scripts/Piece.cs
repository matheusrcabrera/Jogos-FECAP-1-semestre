using UnityEngine;

// Este script fica em cada peça que cai. Ele lê o teclado, move/rotaciona a peça
// e pede ao GridManager correto para verificar colisões e guardar os blocos.
/// <summary>
/// Controla uma peça que está caindo. O GridManager informa a qual tabuleiro ela
/// pertence e quais teclas devem controlá-la.
/// </summary>
public class Piece : MonoBehaviour
{
    // Guarda quando a peça desceu pela última vez; serve para controlar a velocidade.
    private float tempoAnterior;
    // Tempo normal entre movimentos para baixo, em segundos. Menor = cai mais rápido.
    public float tempoQueda = 0.8f;
    // Referência ao controlador do tabuleiro dono desta peça.
    // Assim, uma peça do P2 não consulta sem querer a matriz do P1.
    private GridManager gridManager;

    // O GridManager chama este método logo que cria a peça e informa quem é seu dono.
    public void Configurar(GridManager manager)
    {
        gridManager = manager;
    }

    // O Unity chama Update repetidamente durante o jogo enquanto a peça está ativa.
    private void Update()
    {
        if (gridManager == null || gridManager.JogoAcabou)
            return;

        // Lê o modo guardado pelo menu: 1 = solo; 2 = versus.
        bool jogadorUnico = PlayerPrefs.GetInt("GameMode", 1) != 2;
        bool jogador1 = gridManager.PlayerNumber == 1;

        // Em solo aceitamos os dois conjuntos de teclas, preservando o controle antigo.
        // No versus, P1 usa setas e P2 usa A/D/W/S para não disputarem o mesmo comando.
        bool esquerda = jogador1
            ? (Input.GetKeyDown(KeyCode.LeftArrow) || (jogadorUnico && Input.GetKeyDown(KeyCode.A)))
            : Input.GetKeyDown(KeyCode.A);
        bool direita = jogador1
            ? (Input.GetKeyDown(KeyCode.RightArrow) || (jogadorUnico && Input.GetKeyDown(KeyCode.D)))
            : Input.GetKeyDown(KeyCode.D);
        bool rotacao = jogador1
            ? (Input.GetKeyDown(KeyCode.UpArrow) || (jogadorUnico && Input.GetKeyDown(KeyCode.W)))
            : Input.GetKeyDown(KeyCode.W);

        // Jogador 1: setas. Jogador 2: WASD. No modo solo, setas e WASD
        // continuam funcionando para preservar os controles anteriores.
        if (esquerda)
            MoverHorizontal(-1);
        else if (direita)
            MoverHorizontal(1);
        else if (rotacao)
            Rotacionar();

        bool quedaRapida = jogador1
            ? (Input.GetKey(KeyCode.DownArrow) || (jogadorUnico && Input.GetKey(KeyCode.S)))
            : Input.GetKey(KeyCode.S);

        // O operador ?: escolhe um dos dois valores: se a tecla de baixo estiver pressionada,
        // a queda fica 10 vezes mais rápida; se não, usa o intervalo normal.
        float intervaloQueda = quedaRapida ? tempoQueda / 10f : tempoQueda;
        if (Time.time - tempoAnterior >= intervaloQueda)
        {
            transform.position += Vector3.down;

            if (!gridManager.MovimentoValido(transform))
            {
                transform.position += Vector3.up;
                gridManager.TravarPeca(transform);
                enabled = false;
            }

            tempoAnterior = Time.time;
        }
    }

    // direcao vale -1 para esquerda e +1 para direita.
    private void MoverHorizontal(int direcao)
    {
        Vector3 movimento = new Vector3(direcao, 0f, 0f);
        transform.position += movimento;

        if (!gridManager.MovimentoValido(transform))
            transform.position -= movimento;
    }

    // Gira a peça 90 graus. Se depois de girar ela bater em algo, desfaz a rotação.
    private void Rotacionar()
    {
        transform.Rotate(0f, 0f, -90f);
        if (!gridManager.MovimentoValido(transform))
            transform.Rotate(0f, 0f, 90f);
    }
}
