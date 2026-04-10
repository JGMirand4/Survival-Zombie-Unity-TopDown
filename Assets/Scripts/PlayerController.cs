using System.Collections; // NECESSÁRIO PARA A COROUTINE DE TEMPO
using UnityEngine;
using UnityEngine.InputSystem; 
using UnityEngine.SceneManagement;

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

    // --- NOVA VARIÁVEL DE MORTE ---
    private bool                                _isDead = false;

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
    }

    void Update()
    {
        // SE ESTIVER MORTO, NÃO DEIXA FAZER NADA E SAI DO UPDATE
        if (_isDead) return;

        PlayerInput();
        UpdateAnimation();
        CheckWeaponToggle(); 
        CheckCombatInput(); 
    }

    void FixedUpdate()
    {
        // SE ESTIVER MORTO, PARA DE SE MOVER FISICAMENTE
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

        if (muzzleFlash != null && _muzzleAnimator != null)
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

        if (bulletPrefab != null && firePoint != null)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }
    }

    void HideMuzzleFlash()
    {
        if (muzzleFlash != null) muzzleFlash.SetActive(false);
    }

    // --- NOVA LÓGICA DE COLISÃO E MORTE ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Se já estiver morto, ignora
        if (_isDead) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Inicia a sequência de morte
            StartCoroutine(DeathSequence());
        }
    }

    private IEnumerator DeathSequence()
    {
        _isDead = true;
        Debug.Log("Player morreu! Iniciando animação...");

        // 1. Zera a velocidade para o personagem não deslizar
        _playerRB2D.linearVelocity = Vector2.zero;

        // 2. Desliga a Arma e as Mãos
        if (_gunObject != null) _gunObject.SetActive(false);
        if (_handsObject != null) _handsObject.SetActive(false);

        // 3. Puxa o gatilho da animação do Player
        if (_animator != null) _animator.SetTrigger("Die");

        // --- A MÁGICA: PARANDO TODOS OS ZUMBIS DA TELA ---
        EnemyAI[] todosOsZumbis = FindObjectsOfType<EnemyAI>();
        foreach (EnemyAI zumbi in todosOsZumbis)
        {
            zumbi.enabled = false; // Desliga o script (o cérebro) do zumbi
            
            // Força ele a voltar para a animação de Idle (parado)
            Animator zumbiAnim = zumbi.GetComponent<Animator>();
            if (zumbiAnim != null) zumbiAnim.SetFloat("Speed", 0); 
        }
        // -------------------------------------------------

        // 4. ESPERA 2 SEGUNDOS 
        yield return new WaitForSeconds(2f);

        // 5. Reinicia a fase
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}