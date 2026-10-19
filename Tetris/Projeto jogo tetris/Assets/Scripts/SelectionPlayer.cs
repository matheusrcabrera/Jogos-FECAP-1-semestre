using UnityEngine;

public class SelectionPlayer : MonoBehaviour
{
    public GameObject OptionPlayer;
    public GameObject MainMenuPanel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   public void Voltar()
    {
        OptionPlayer.SetActive(false);
        if (MainMenuPanel != null)
        {
            MainMenuPanel.SetActive(true); // Exibe o menu principal de volta
        }
    }
}
