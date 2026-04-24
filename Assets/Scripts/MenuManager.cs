using UnityEngine;
using UnityEngine.SceneManagement; // Biblioteca para trocar de tela

public class MenuManager : MonoBehaviour
{
    // Escreva aqui o nome EXATO da sua cena de jogo (ex: "SampleScene")
    public string nomeDaFase = "SampleScene"; 

    public void BotaoJogar()
    {
        // Carrega a cena do jogo
        SceneManager.LoadScene(nomeDaFase);
    }

    public void BotaoSair()
    {
        // Fecha o jogo (Só funciona quando o jogo for exportado/buildado)
        Debug.Log("O jogo foi fechado!"); 
        Application.Quit();
    }
}
