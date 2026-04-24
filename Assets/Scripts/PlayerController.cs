using System.Collections; 
using UnityEngine;
using UnityEngine.InputSystem; 
using UnityEngine.SceneManagement;
using TMPro; 

public class PlayerController : MonoBehaviour
{
    private PlayerControls                      _playerControls;
    [SerializeField] private Vector2            _playerDirection;
    [SerializeField] private Vector2            _lastDirection = new Vector2(0, -1); 
    private Rigidbody2D                         _playerRB2D;
    public float                                _playerSpeed;
    [SerializeField] private Animator           _animator; 

    [Header("Sistema de Armas")]
    [SerializeField] private GameObject         _gunObject; 
    [SerializeField] private GameObject         _handsObject; 
    
    private Animator                            _gunAnimator; 
    private Animator                            _handsAnimator; 
    private bool                                _isWeaponEquipped = false; 

    [Header("Renderizadores (Profundidade)")]
    [SerializeField] private SpriteRenderer     _gunRenderer;
    [SerializeField] private SpriteRenderer     _handsRenderer;

    [Header("Sistema de Tiro")]
    [SerializeField] private GameObject         bulletPrefab;    
    [SerializeField] private Transform          firePoint; 
    [SerializeField] private Vector2            firePointOffset = new Vector2(0.6f, 0.4f);
    [SerializeField] private GameObject         muzzleFlash;     
    [SerializeField] private float              fireRate = 0.2f;      
    
    private Animator                            _muzzleAnimator; 
    private float                               nextFireTime = 0f;                     

    private bool                                _isDead = false;

    [Header("Status do Jogador")]
    public int vidasTotais = 3; 
    private int vidasAtuais;
    
    public float healthMaxima = 100f; 
    private float healthAtual;

    [Header("Interface")]
    public TextMeshProUGUI textoVida; 
    public UnityEngine.UI.Slider sliderVida; 

    [Header("Ação Especial")]
    public float raioDeExplosao = 4f;
    public GameObject explosaoPrefab;

    [Header("Áudio")]
    public AudioSource audioSource;
    public AudioClip somTiro;

    private void OnEnable() { _playerControls.Enable(); }
    private void OnDisable() { _playerControls.Disable(); }

    void Awake()
    {
        _playerControls = new PlayerControls();
    }

    void Start()
    {
        _playerRB2D = GetComponent<Rigidbody2D>();
        
        if (_animator == null) _animator = GetComponent<Animator>();
        
        if (_gunObject != null)
        {
            _gunAnimator = _gunObject.GetComponent<Animator>();
            _gunObject.SetActive(_isWeaponEquipped);
        }

        if (_handsObject != null)
        {
            _handsAnimator = _handsObject.GetComponent<Animator>();
            _handsObject.SetActive(!_isWeaponEquipped); 
        }

        if (muzzleFlash != null)
        {
            _muzzleAnimator = muzzleFlash.GetComponent<Animator>();
            muzzleFlash.SetActive(false);
        }
        
        vidasAtuais = vidasTotais;
        healthAtual = healthMaxima;
        
        if (sliderVida != null) sliderVida.maxValue = healthMaxima;
        
        AtualizarHUD();
    }

    void AtualizarHUD()
    {
        if (textoVida != null) textoVida.text = "Life: " + vidasAtuais;
        if (sliderVida != null) sliderVida.value = healthAtual;
    }

    void Update()
    {
        if (_isDead) return;

        PlayerInput();
        UpdateAnimation();
        CheckWeaponToggle(); 
        CheckCombatInput(); 
        CheckSpecialAttack(); 
    }

    void FixedUpdate()
    {
        if (_isDead) return;
        PlayerMove();
    }

    void PlayerInput()
    {
        _playerDirection = _playerControls.Player.Move.ReadValue<Vector2>();
    }

    void PlayerMove()
    {
        _playerRB2D.MovePosition(_playerRB2D.position + _playerDirection * (_playerSpeed * Time.deltaTime));
    }

    void UpdateAnimation()
    {
        if (_playerDirection != Vector2.zero)
        {
            _lastDirection = _playerDirection;
        }

        float magnitude = _playerDirection.magnitude;

        if (_animator != null)
        {
            _animator.SetFloat("X", _lastDirection.x);
            _animator.SetFloat("Y", _lastDirection.y);
            _animator.SetFloat("Speed", magnitude);
        }

        if (_gunObject != null && _gunObject.activeSelf && _gunAnimator != null)
        {
            _gunAnimator.SetFloat("X", _lastDirection.x);
            _gunAnimator.SetFloat("Y", _lastDirection.y);
        }

        if (_handsObject != null && _handsObject.activeSelf && _handsAnimator != null)
        {
            _handsAnimator.SetFloat("X", _lastDirection.x);
            _handsAnimator.SetFloat("Y", _lastDirection.y);
        }

        if (muzzleFlash != null && muzzleFlash.activeSelf && _muzzleAnimator != null)
        {
            _muzzleAnimator.SetFloat("X", _lastDirection.x);
            _muzzleAnimator.SetFloat("Y", _lastDirection.y);
        }

        if (firePoint != null)
        {
            float posX = _lastDirection.x * firePointOffset.x;
            float posY = (_lastDirection.y * firePointOffset.x) + firePointOffset.y;

            firePoint.localPosition = new Vector3(posX, posY, 0);

            float angle = Mathf.Atan2(_lastDirection.y, _lastDirection.x) * Mathf.Rad2Deg;
            firePoint.rotation = Quaternion.Euler(0, 0, angle);
        }

        if (_lastDirection.y > 0)
        {
            if (_gunRenderer != null) _gunRenderer.sortingOrder = -1;
            if (_handsRenderer != null) _handsRenderer.sortingOrder = -1;
            if (muzzleFlash != null) muzzleFlash.GetComponent<SpriteRenderer>().sortingOrder = -1;
        }
        else 
        {
            if (_gunRenderer != null) _gunRenderer.sortingOrder = 1;
            if (_handsRenderer != null) _handsRenderer.sortingOrder = 1;
            if (muzzleFlash != null) muzzleFlash.GetComponent<SpriteRenderer>().sortingOrder = 1;
        }
    }

    void CheckWeaponToggle()
    {
        if (_playerControls.Player.Get() != null && _playerControls.Player.SwitchWeapon.triggered)
        {
            _isWeaponEquipped = !_isWeaponEquipped;
            
            if (_gunObject != null) 
            {
                _gunObject.SetActive(_isWeaponEquipped);
                if (muzzleFlash != null) muzzleFlash.SetActive(false);
            }

            if (_handsObject != null) _handsObject.SetActive(!_isWeaponEquipped); 
        }
    }

    void CheckCombatInput()
    {
        if (_playerControls.Player.Get() != null && _playerControls.Player.Attack.triggered)
        {
            if (_isWeaponEquipped)
            {
                if (Time.time >= nextFireTime)
                {
                    Shoot();
                    nextFireTime = Time.time + fireRate; 
                }
            }
            else
            {
                if (_animator != null) _animator.SetTrigger("Attack");
                if (_handsAnimator != null) _handsAnimator.SetTrigger("Attack");
            }
        }
    }

    void Shoot()
    {
        if (_gunAnimator != null) _gunAnimator.SetTrigger("Shoot"); 

        if (muzzleFlash != null)
        {
            muzzleFlash.SetActive(true);
            Invoke("HideMuzzleFlash", 0.25f); 
        }

        // --- NOVA LINHA DE ÁUDIO AQUI ---
        if (audioSource != null && somTiro != null)
        {
            // O "0.4f" no final significa 40% do volume original
            audioSource.PlayOneShot(somTiro, 0.7f); 
        }

        if (bulletPrefab != null && firePoint != null)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            CameraShake.Instance.Shake(0.5f, 0.05f);
        }
    }

    void HideMuzzleFlash()
    {
        if (muzzleFlash != null) muzzleFlash.SetActive(false);
    }

    void CheckSpecialAttack()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (vidasAtuais > 1) 
            {
                UsarEspecial();
            }
            else
            {
                Debug.Log("Vida muito baixa! Não é possível usar o especial.");
            }
        }
    }

    void UsarEspecial()
    {
        if (vidasAtuais > 1)
        {
            vidasAtuais--; 
            healthAtual = healthMaxima; 
            AtualizarHUD();

            CameraShake.Instance.Shake(2f, 0.15f); 
            
            StartCoroutine(EfeitoVisualEspecial());

            if (explosaoPrefab != null)
            {
                Instantiate(explosaoPrefab, transform.position, Quaternion.identity);
            }

            EnemyAI[] todosOsZumbis = FindObjectsByType<EnemyAI>(FindObjectsInactive.Exclude);        
            foreach (EnemyAI zumbi in todosOsZumbis)
            {
                float distancia = Vector2.Distance(transform.position, zumbi.transform.position);
                if (distancia <= raioDeExplosao && !zumbi.isDead)
                {
                    zumbi.Die(); 
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, raioDeExplosao);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_isDead) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            healthAtual -= 20f;
            
            StartCoroutine(EfeitoDanoFlash());
            CameraShake.Instance.Shake(2f, 0.15f);

            if (healthAtual <= 0)
            {
                vidasAtuais--; 
                healthAtual = healthMaxima; 
                
                if (vidasAtuais <= 0)
                {
                    StartCoroutine(DeathSequence());
                }
            }

            AtualizarHUD();
        }
    }

    private IEnumerator EfeitoDanoFlash()
    {
        SpriteRenderer playerSprite = GetComponent<SpriteRenderer>();

        if (playerSprite != null)
        {
            Color corOriginal = Color.white; 
            Color corDano;
            ColorUtility.TryParseHtmlString("#801A1A", out corDano);

            for (int i = 0; i < 3; i++)
            {
                playerSprite.color = corDano;
                if (_gunRenderer != null) _gunRenderer.color = corDano;
                if (_handsRenderer != null) _handsRenderer.color = corDano;

                yield return new WaitForSeconds(0.1f);

                playerSprite.color = corOriginal;
                if (_gunRenderer != null) _gunRenderer.color = corOriginal;
                if (_handsRenderer != null) _handsRenderer.color = corOriginal;

                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    private IEnumerator EfeitoVisualEspecial()
    {
        SpriteRenderer playerSprite = GetComponent<SpriteRenderer>();

        if (playerSprite != null)
        {
            Color corOriginal = Color.white; 
            Color corNova;
            ColorUtility.TryParseHtmlString("#801A1A", out corNova); 

            playerSprite.color = corNova; 
            if (_gunRenderer != null) _gunRenderer.color = corNova;
            if (_handsRenderer != null) _handsRenderer.color = corNova;

            yield return new WaitForSeconds(0.15f);

            playerSprite.color = corOriginal;
            if (_gunRenderer != null) _gunRenderer.color = corOriginal;
            if (_handsRenderer != null) _handsRenderer.color = corOriginal;
        }
    }

    private IEnumerator DeathSequence()
    {
        _isDead = true;
        Debug.Log("Player morreu! Iniciando animação...");

        _playerRB2D.linearVelocity = Vector2.zero;

        if (_gunObject != null) _gunObject.SetActive(false);
        if (_handsObject != null) _handsObject.SetActive(false);

        if (_animator != null) _animator.SetTrigger("Die");

        EnemyAI[] todosOsZumbis = FindObjectsByType<EnemyAI>(FindObjectsInactive.Exclude);
        foreach (EnemyAI zumbi in todosOsZumbis)
        {
            zumbi.enabled = false; 
            Animator zumbiAnim = zumbi.GetComponent<Animator>();
            if (zumbiAnim != null) zumbiAnim.SetFloat("Speed", 0); 
        }

        // Aguarda a animação de morte terminar
        yield return new WaitForSeconds(2f);

        // --- AQUI ESTÁ A MUDANÇA: Chama o GameManager em vez de recarregar a cena direto ---
        if (GameManager.Instance != null)
        {
            GameManager.Instance.MostrarGameOver();
        }
        else
        {
            // Apenas como garantia, se você esquecer o GameManager na cena, ele recarrega normal
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}