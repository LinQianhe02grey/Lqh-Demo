using Cardwin.Combat;
using UnityEngine;

namespace Cardwin.Characters
{
    [RequireComponent(typeof(Animator))]
    public class SamplePlayerAnimationBridge : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private Animator _animator;
        [SerializeField] private Health _health;
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float _groundCheckRadius = 0.15f;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int GroundedHash = Animator.StringToHash("Grounded");
        private static readonly int VerticalVelocityHash = Animator.StringToHash("VerticalVelocity");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int DeadHash = Animator.StringToHash("Dead");

        private bool _wasDead;

        void Awake()
        {
            if (_animator == null) _animator = GetComponent<Animator>();

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                if (_rb == null) _rb = player.GetComponent<Rigidbody2D>();
                if (_health == null) _health = player.GetComponent<Health>();
                if (_groundCheck == null)
                {
                    var pc = player.GetComponent<Cardwin.Combat.PlayerController2D>();
                    if (pc != null)
                    {
                        _groundCheck = pc.groundCheck;
                        _groundLayer = pc.groundLayer;
                        _groundCheckRadius = pc.groundCheckRadius;
                    }
                }
            }
        }

        void Start()
        {
            if (_health != null)
                _health.OnDeath.AddListener(OnPlayerDeath);
        }

        void Update()
        {
            if (_animator == null || _rb == null || _wasDead) return;

            float speed = Mathf.Abs(_rb.velocity.x);
            _animator.SetFloat(SpeedHash, speed);

            bool grounded = CheckGrounded();
            _animator.SetBool(GroundedHash, grounded);

            _animator.SetFloat(VerticalVelocityHash, _rb.velocity.y);
        }

        bool CheckGrounded()
        {
            if (_groundCheck == null) return true;
            return Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);
        }

        void OnDestroy()
        {
            if (_health != null)
                _health.OnDeath.RemoveListener(OnPlayerDeath);
        }

        public void TriggerAttack()
        {
            if (_animator != null && !_wasDead)
                _animator.SetTrigger(AttackHash);
        }

        bool _deadTriggered;
        void OnPlayerDeath()
        {
            if (_deadTriggered) return;
            _deadTriggered = true;
            _wasDead = true;
            if (_animator != null)
                _animator.SetBool(DeadHash, true);
        }
    }
}
