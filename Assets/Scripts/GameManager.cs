using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Configurações dos Inimigos")]
    [Tooltip("Arraste os Prefabs dos seus Zumbis (Big, Axe, etc) para esta lista")]
    // NOVA VARIÁVEL: Agora é um Array (Lista) de GameObjects
    public GameObject[] enemyPrefabs;    
    
    [Tooltip("Arraste os pontos vazios de fora da tela para esta lista")]
    public Transform[] spawnPoints;   

    [Header("Dificuldade (Tempo de Spawn)")]
    public float tempoInicial = 3.0f;
    public float tempoMinimo = 0.5f;  
    public float taxaDeReducao = 0.1f;

    private float tempoAtual;

    void Start()
    {
        tempoAtual = tempoInicial;
        StartCoroutine(RotinaDeSpawn());
    }

    private IEnumerator RotinaDeSpawn()
    {
        while (true)
        {
            yield return new WaitForSeconds(tempoAtual);

            // Garante que existem portas e inimigos cadastrados no Inspector
            if (spawnPoints.Length > 0 && enemyPrefabs.Length > 0)
            {
                // 1. SORTEIA A PORTA DE ENTRADA
                int pontoSorteado = Random.Range(0, spawnPoints.Length);
                Transform pontoEscolhido = spawnPoints[pontoSorteado];

                // 2. SORTEIA O TIPO DE ZUMBI
                int inimigoSorteado = Random.Range(0, enemyPrefabs.Length);
                GameObject zumbiEscolhido = enemyPrefabs[inimigoSorteado];

                // 3. Cria o Zumbi escolhido na porta escolhida
                Instantiate(zumbiEscolhido, pontoEscolhido.position, Quaternion.identity);
            }

            // AUMENTA A DIFICULDADE
            if (tempoAtual > tempoMinimo)
            {
                tempoAtual -= taxaDeReducao;
                Debug.Log("Dificuldade Aumentou! Próximo zumbi em: " + tempoAtual + " segundos.");
            }
        }
    }
}