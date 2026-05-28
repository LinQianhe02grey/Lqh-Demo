using UnityEngine;

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

        private Rigidbody2D _rb;
        private SpriteRenderer _spriteRenderer;
        private Health _health;

        private float _horizontalInput;
        private int _jumpsRemaining;
        private bool _isDashing;
        private float _dashTimer;
        private float _dashCooldownTimer;
        private bool _facingRight = true;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _health = GetComponent<Health>();
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
            else
            {
                if (IsGrounded())
                    _jumpsRemaining = maxJumps;

                if (Input.GetKeyDown(KeyCode.Space))
                    Jump();

                if (Input.GetKeyDown(KeyCode.LeftShift))
                    StartDash();
            }

            FlipSprite();
        }

        private void FixedUpdate()
        {
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

        public bool IsGrounded()
        {
            if (groundCheck == null)
                return false;
            return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
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

        public void Fire() { }
        public void UseSelfCard() { }
        public void Reload() { }
    }
}
