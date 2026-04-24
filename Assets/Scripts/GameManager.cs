using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // --- ADICIONADO: Necessário para trocar de cena ---

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Interface")]
    public int pontuacaoAtual = 0;
    public TextMeshProUGUI textoPontuacao;
    
    // --- NOVO: Referência para a tela de Game Over ---
    [Header("Game Over")]
    public GameObject painelGameOver; 
    private bool _isGameOver = false;

    [Header("Configurações dos Inimigos")]
    public GameObject[] enemyPrefabs;    
    public Transform[] spawnPoints;   

    [Header("Dificuldade (Tempo de Spawn)")]
    public float tempoInicial = 3.0f;
    public float tempoMinimo = 0.5f;  
    public float taxaDeReducao = 0.1f;
    private float tempoAtual;

    private void Awake()
    {
        Instance = this;
        // Garante que o tempo do jogo comece normal
        Time.timeScale = 1f; 
    }

    void Start()
    {
        tempoAtual = tempoInicial;
        StartCoroutine(RotinaDeSpawn());
        
        // Garante que o painel comece escondido
        if (painelGameOver != null) painelGameOver.SetActive(false);
    }

    public void AdicionarPontos(int pontosGanhos)
    {
        if (_isGameOver) return; // Não ganha pontos depois de morto
        pontuacaoAtual += pontosGanhos;
        if (textoPontuacao != null) textoPontuacao.text = "Pontos: " + pontuacaoAtual;
    }

    // --- NOVA FUNÇÃO: Ativa a tela de derrota ---
    public void MostrarGameOver()
    {
        _isGameOver = true;
        if (painelGameOver != null) painelGameOver.SetActive(true);
        
        // Opcional: Pausa o jogo (zumbis e player param)
        // Time.timeScale = 0f; 
    }

    // --- FUNÇÕES DOS BOTÕES ---
    public void ReiniciarJogo()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void VoltarAoMenu()
    {
        SceneManager.LoadScene("MainMenu"); // Verifique se o nome da cena está correto
    }

    private IEnumerator RotinaDeSpawn()
    {
        // Só spawna zumbis se o jogo NÃO acabou
        while (!_isGameOver)
        {
            yield return new WaitForSeconds(tempoAtual);

            if (spawnPoints.Length > 0 && enemyPrefabs.Length > 0)
            {
                int puntoSorteado = Random.Range(0, spawnPoints.Length);
                Transform pontoEscolhido = spawnPoints[puntoSorteado];
                int inimigoSorteado = Random.Range(0, enemyPrefabs.Length);
                GameObject zumbiEscolhido = enemyPrefabs[inimigoSorteado];

                Instantiate(zumbiEscolhido, pontoEscolhido.position, Quaternion.identity);
            }

            if (tempoAtual > tempoMinimo)
            {
                tempoAtual -= taxaDeReducao;
            }
        }
    }
}