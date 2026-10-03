using UnityEngine;

public class Piece : MonoBehaviour
{
    private float tempoAnterior;
    public float tempoQueda = 0.8f;

void Update()
{
    // Movimento para a Esquerda (A ou Seta Esquerda)
    if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
    {
        transform.position += new Vector3(-1, 0, 0);
        if (!MovimentoValido())
            transform.position -= new Vector3(-1, 0, 0);
    }
    // Movimento para a Direita (D ou Seta Direita)
    else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
    {
        transform.position += new Vector3(1, 0, 0);
        if (!MovimentoValido())
            transform.position -= new Vector3(1, 0, 0);
    }
    // Rotação (W ou Seta Cima)
    else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
    {
        transform.Rotate(0, 0, -90);
        if (!MovimentoValido())
            transform.Rotate(0, 0, 90);
    }

    // Controlo de Queda (Acelera ao pressionar S / Seta Baixo)
    float tempoAtual = (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) ? tempoQueda / 10f : tempoQueda;

    if (Time.time - tempoAnterior >= tempoAtual)
    {
        transform.position += new Vector3(0, -1, 0);
        
        if (!MovimentoValido())
        {
            transform.position -= new Vector3(0, -1, 0);
            
            // Procura o GridManager e executa as funções de travamento e spawn
            GridManager grid = FindFirstObjectByType<GridManager>();
            if (grid != null)
            {
                // ATENÇÃO: Verifica se o nome no teu GridManager.cs é AdicionarAoGrid ou AdicionarAoTabuleiro
                grid.AdicionarAoGrid(transform); 
                grid.VerificarLinhas();
                grid.SpawnNovaPeca();
            }
            
            enabled = false; // Desativa este script para a peça atual
        }
        
        tempoAnterior = Time.time;
    }
}
    bool MovimentoValido()
    {
        foreach (Transform filho in transform)
        {
            int arredondadoX = Mathf.RoundToInt(filho.transform.position.x);
            int arredondadoY = Mathf.RoundToInt(filho.transform.position.y);

            // Verifica limites do tabuleiro
            if (arredondadoX < 0 || arredondadoX >= GridManager.larg || arredondadoY < 0)
                return false;

            // Verifica se já existe outro bloco no mesmo local
            if (GridManager.grid[arredondadoX, arredondadoY] != null)
                return false;
        }
        return true;
    }

    void AdicionarAoGrid()
    {
        foreach (Transform filho in transform)
        {
            int arredondadoX = Mathf.RoundToInt(filho.transform.position.x);
            int arredondadoY = Mathf.RoundToInt(filho.transform.position.y);

            GridManager.grid[arredondadoX, arredondadoY] = filho;
        }
    }
}