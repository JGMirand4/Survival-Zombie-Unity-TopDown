using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    public float speed = 2.5f;
    
    [Header("Status do Zumbi")]
    public int maxHealth = 3;       // Quantos tiros o zumbi aguenta
    private int currentHealth;      // A vida atual dele

    [Header("Pontos de Referência")]
    [SerializeField] private Transform headPoint; // Arraste um objeto vazio na cabeça aqui

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    public bool isDead = false; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        // Inicializa a vida
        currentHealth = maxHealth;
    }

    void FixedUpdate()
    {
        if (isDead) return; 

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
            return; 
        }

        Vector2 direction = (player.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);

        if (animator != null)
        {
            animator.SetFloat("X", direction.x);
            animator.SetFloat("Y", direction.y);
            animator.SetFloat("Speed", direction.magnitude); 
        }
    }

    // --- NOVA FUNÇÃO: RECEBER DANO E POSIÇÃO DO IMPACTO ---
    public void TakeDamage(int damageAmount, Vector3 impactPoint)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        Debug.Log("Zumbi tomou tiro! Vida restante: " + currentHealth);

        // AQUI ESTÁ O SANGUE:
        // Se você configurou o headPoint, vamos usar a posição dele para o sangue
        // (Isso é melhor do que o ponto exato da bala, para sair 'da cabeça')
        if (headPoint != null)
        {
            // O headPoint já está 'na frente' visualmente, então usamos a posição dele
            SpawnBlood(headPoint.position);
        }
        else 
        {
            // Fallback: se não tiver headPoint, usa o ponto do impacto, mas sobe um pouco
            SpawnBlood(impactPoint + new Vector3(0, 0.5f, -0.1f));
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Função interna para criar o sangue
    private void SpawnBlood(Vector3 position)
    {
        // Precisamos de um GameManager ou um objeto que gerencie os efeitos
        // Para simplificar agora, vamos pedir para a própria Bala gerenciar o spawn
        // e apenas passar a posição aqui. *Vamos fazer isso no Bullet.cs*
    }

    // A função de morte que você já tinha e ajustou!
    public void Die()
    {
        if (isDead) return; 
        isDead = true;

        if (animator != null) animator.SetTrigger("Die");
        GetComponent<Collider2D>().enabled = false;
        rb.linearVelocity = Vector2.zero;

        // --- AS 3 LINHAS ADICIONADAS ---
        // Pede para o GameManager dar 10 pontos
        if (GameManager.Instance != null) {
            GameManager.Instance.AdicionarPontos(10); 
        }

        Destroy(gameObject, 1.5f); 
    }
}