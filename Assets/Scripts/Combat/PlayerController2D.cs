using System.Collections.Generic;
using UnityEngine;
using Cardwin.Cards;
using Cardwin.Magazine;
using Cardwin.Inventory;
using Cardwin.UI;

namespace Cardwin.Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController2D : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 7f;

        [Header("Jump")]
        public float jumpForce = 13f;
        public int maxJumps = 2;

        [Header("Dash")]
        public float dashSpeed = 18f;
        public float dashDuration = 0.15f;
        public float dashCooldown = 0.6f;
        public bool invincibleDuringDash = true;

        [Header("Ground Check")]
        public Transform groundCheck;
        public float groundCheckRadius = 0.15f;
        public LayerMask groundLayer;

        [Header("Card System")]
        public CardData testCard;
        public PlayerCardContext cardContext;

        [Header("Magazine")]
        public MagazineSystem magazineSystem;

        [Header("Temp Shooting (fallback)")]
        public Transform firePoint;
        public GameObject projectilePrefab;
        public int testProjectileDamage = 10;

        [Header("Inventory & Bag")]
        public InventorySystem inventorySystem;
        public MagazineEditUI magazineEditUI;

        private Rigidbody2D _rb;
        private SpriteRenderer _spriteRenderer;
        private Health _health;
        private CardEffectExecutor _cardExecutor;

        private float _horizontalInput;
        private int _jumpsRemaining;
        private bool _isDashing;
        private float _dashTimer;
        private float _dashCooldownTimer;
        private bool _facingRight = true;
        private bool _warnedMissingGroundCheck;
        private bool _warnedUnsetLayer;
        private bool _inputLocked;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _health = GetComponent<Health>();

            _rb.freezeRotation = true;

            if (groundCheck == null)
                FindGroundCheckIfMissing();

            _cardExecutor = GetComponent<CardEffectExecutor>();
            if (_cardExecutor == null)
                Debug.LogError("[PlayerController2D] Missing CardEffectExecutor on Player. Please add it in the Inspector.");

            cardContext = new PlayerCardContext
            {
                player = gameObject,
                firePoint = firePoint,
                playerHealth = _health,
                defaultProjectilePrefab = projectilePrefab
            };

            if (_cardExecutor != null)
                _cardExecutor.Initialize(cardContext);

            if (magazineSystem == null)
                magazineSystem = GetComponent<MagazineSystem>();

            if (magazineSystem == null)
                Debug.LogError("[PlayerController2D] Missing MagazineSystem on Player. Please add it in the Inspector.");
            else
            {
                magazineSystem.context = cardContext;
                magazineSystem.cardExecutor = _cardExecutor;
            }

            if (inventorySystem == null)
                inventorySystem = GetComponent<InventorySystem>();

            if (inventorySystem == null)
                Debug.LogError("[PlayerController2D] Missing InventorySystem on Player. Please add it in the Inspector.");

            if (magazineEditUI == null)
                magazineEditUI = FindObjectOfType<MagazineEditUI>();

            if (magazineEditUI == null)
                Debug.LogError("[PlayerController2D] Missing MagazineEditUI on Canvas. Please add it in the Inspector.");
            else
            {
                magazineEditUI.inventorySystem = inventorySystem;
                magazineEditUI.magazineSystem = magazineSystem;
            }
        }

        private void Update()
        {
            _horizontalInput = Input.GetAxisRaw("Horizontal");

            if (_dashCooldownTimer > 0f)
                _dashCooldownTimer -= Time.deltaTime;

            if (_isDashing)
            {
                _dashTimer -= Time.deltaTime;
                if (_dashTimer <= 0f)
                {
                    _isDashing = false;
                    if (invincibleDuringDash && _health != null)
                        _health.SetInvincible(false);
                }
            }
            else if (!_inputLocked)
            {
                if (IsGrounded())
                    _jumpsRemaining = maxJumps;

                if (Input.GetKeyDown(KeyCode.Space))
                    Jump();

                if (Input.GetKeyDown(KeyCode.LeftShift))
                    StartDash();

                if (Input.GetMouseButtonDown(0))
                {
                    if (magazineSystem != null && magazineSystem.GetCurrentCard() != null)
                    {
                        magazineSystem.UseCurrentCardLeft();
                    }
                    else if (testCard != null && _cardExecutor != null)
                    {
                        _cardExecutor.ExecuteLeft(testCard, cardContext);
                    }
                    else
                    {
                        Shoot();
                    }
                }

                if (Input.GetMouseButtonDown(1))
                {
                    if (magazineSystem != null && magazineSystem.GetCurrentCard() != null)
                    {
                        magazineSystem.UseCurrentCardRight();
                    }
                    else if (testCard != null && _cardExecutor != null)
                    {
                        _cardExecutor.ExecuteRight(testCard, cardContext);
                    }
                }

                if (Input.GetKeyDown(KeyCode.R))
                {
                    if (magazineSystem != null)
                        magazineSystem.ManualReload();
                }
            }

            FlipSprite();
        }

        private void FixedUpdate()
        {
            if (_inputLocked)
            {
                _rb.velocity = new Vector2(0f, _rb.velocity.y);
                return;
            }

            if (_isDashing)
            {
                float dir = _facingRight ? 1f : -1f;
                _rb.velocity = new Vector2(dir * dashSpeed, 0f);
            }
            else
            {
                _rb.velocity = new Vector2(_horizontalInput * moveSpeed, _rb.velocity.y);
            }
        }

        public void Move(float horizontalInput)
        {
            _horizontalInput = horizontalInput;
        }

        public void Jump()
        {
            if (_jumpsRemaining <= 0)
                return;

            _rb.velocity = new Vector2(_rb.velocity.x, 0f);
            _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            _jumpsRemaining--;
        }

        public void StartDash()
        {
            if (_isDashing)
                return;
            if (_dashCooldownTimer > 0f)
                return;

            _isDashing = true;
            _dashTimer = dashDuration;
            _dashCooldownTimer = dashCooldown;

            if (invincibleDuringDash && _health != null)
                _health.SetInvincible(true);
        }

        public void SetInputLocked(bool locked)
        {
            _inputLocked = locked;
            if (locked && _rb != null)
            {
                _rb.velocity = Vector2.zero;
                _horizontalInput = 0f;
            }
        }

        public bool IsGrounded()
        {
            if (groundCheck == null)
            {
                FindGroundCheckIfMissing();
                if (groundCheck == null)
                    return false;
            }

            if (groundLayer.value == 0)
            {
                if (!_warnedUnsetLayer)
                {
                    Debug.LogWarning("[PlayerController2D] groundLayer not set. Ground detection disabled. Set groundLayer to 'Ground' in Inspector.");
                    _warnedUnsetLayer = true;
                }
                return false;
            }

            return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        private void FindGroundCheckIfMissing()
        {
            Transform found = transform.Find("GroundCheck");
            if (found != null)
            {
                groundCheck = found;
            }
            else if (!_warnedMissingGroundCheck)
            {
                Debug.LogWarning("[PlayerController2D] GroundCheck child not found. Create a child GameObject named 'GroundCheck' at the player's feet.");
                _warnedMissingGroundCheck = true;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck == null)
            {
                Transform found = transform.Find("GroundCheck");
                if (found != null)
                    groundCheck = found;
            }

            if (groundCheck != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
        }

        private void FlipSprite()
        {
            if (_horizontalInput > 0f && !_facingRight)
            {
                _facingRight = true;
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
            else if (_horizontalInput < 0f && _facingRight)
            {
                _facingRight = false;
                Vector3 scale = transform.localScale;
                scale.x = -Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
        }

        private void Shoot()
        {
            if (projectilePrefab == null)
            {
                Debug.LogError("[PlayerShoot] projectilePrefab is not assigned. Drag Projectile_Test.prefab into PlayerController2D.");
                return;
            }

            Transform fp = firePoint;
            if (fp == null)
            {
                fp = transform.Find("FirePoint");
                if (fp != null)
                    firePoint = fp;
            }

            Vector3 mouseWorld = UnityEngine.Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;

            Vector3 spawnBase = fp != null ? fp.position : transform.position;
            Vector2 shootDirection = ((Vector2)mouseWorld - (Vector2)spawnBase).normalized;

            if (shootDirection == Vector2.zero)
                shootDirection = _facingRight ? Vector2.right : Vector2.left;

            Vector3 spawnPos = spawnBase + (Vector3)(shootDirection * (fp != null ? 0.2f : 0.7f));
            spawnPos.z = 0f;

            Debug.Log($"[PlayerShoot] Fire projectile. Direction={shootDirection}, spawnPos={spawnPos}");

            Debug.DrawRay(spawnPos, shootDirection * 2f, Color.yellow, 0.5f);

            GameObject projObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            projObj.transform.position = new Vector3(spawnPos.x, spawnPos.y, 0f);
            projObj.transform.localScale = Vector3.one * 0.8f;

            Debug.Log($"[PlayerShoot] Spawned projectile instance={projObj.name}, active={projObj.activeSelf}, pos={projObj.transform.position}, scale={projObj.transform.localScale}");

            Projectile proj = projObj.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.Init(shootDirection, testProjectileDamage);
            }
            else
            {
                Debug.LogError("[PlayerShoot] Projectile component missing on projectilePrefab.");
                Destroy(projObj);
            }
        }
    }
}
