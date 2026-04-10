using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;       // Velocidade da bala
    public float lifetime = 2f;    // Quanto tempo ela vive antes de sumir
    private Rigidbody2D rb;
    [Header("Efeitos")]
    [SerializeField] private GameObject bloodParticlePrefab;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Dá o impulso inicial para a frente da bala (eixo X local)
        rb.linearVelocity = transform.right * speed;
        
        // Destrói a bala após 'lifetime' segundos para não pesar o jogo
        Destroy(gameObject, lifetime);
    }

    // O que acontece quando bate em algo
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) return;

        if (collision.CompareTag("Enemy"))
        {
            EnemyAI zombieScript = collision.GetComponent<EnemyAI>();
            
            if (zombieScript != null)
            {
                // DÁ 1 DE DANO E PASSA O PONTO DE IMPACTO
                zombieScript.TakeDamage(1, transform.position);

                // CRIA O SANGUE IMEDIATAMENTE NO PONTO DE IMPACTO
                if (bloodParticlePrefab != null)
                {
                    // Instancia a partícula ligeiramente na frente (Z menor) para aparecer
                    Vector3 bloodPos = transform.position + new Vector3(0, 0, -0.1f);
                    Instantiate(bloodParticlePrefab, bloodPos, Quaternion.identity);
                }
            }
            
            // A bala se destrói logo após o impacto
            Destroy(gameObject);
            return; 
        }

        // Se bater em paredes, etc. (se houver, adicione essa lógica)
        Destroy(gameObject); 
    }
}