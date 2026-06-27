using UnityEngine;
using Cardwin.Combat;

namespace Cardwin.Characters
{
    public enum VisualActionType
    {
        None,
        FireRed,
        FireBlue,
        SelfActionBlue,
        SelfActionRed
    }

    [RequireComponent(typeof(Animator))]
    public class GothicNunAnimationBridge : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private Animator _animator;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int GroundedHash = Animator.StringToHash("Grounded");
        private static readonly int VerticalVelocityHash = Animator.StringToHash("VerticalVelocity");
        private static readonly int DeadHash = Animator.StringToHash("Dead");
        private static readonly int FireBlueHash = Animator.StringToHash("FireBlue");
        private static readonly int FireRedHash = Animator.StringToHash("FireRed");
        private static readonly int SelfActionBlueHash = Animator.StringToHash("SelfActionBlue");
        private static readonly int SelfActionRedHash = Animator.StringToHash("SelfActionRed");
        private static readonly int MoveRequestedHash = Animator.StringToHash("MoveRequested");

        private Health _health;
        private Transform _groundCheck;
        private LayerMask _groundLayer;
        private float _groundCheckRadius = 0.15f;
        private bool _wasDead;
        private bool _deadTriggered;

        private PlayerController2D _playerController;

        private void Awake()
        {
            if (_animator == null) _animator = GetComponent<Animator>();

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                if (_rb == null) _rb = player.GetComponent<Rigidbody2D>();
                if (_health == null) _health = player.GetComponent<Health>();

                _playerController = player.GetComponent<PlayerController2D>();
                if (_playerController != null)
                {
                    _groundCheck = _playerController.groundCheck;
                    _groundLayer = _playerController.groundLayer;
                    _groundCheckRadius = _playerController.groundCheckRadius;
                }
            }
        }

        private void Start()
        {
            if (_health != null)
                _health.OnDeath.AddListener(OnPlayerDeath);

            CardVisualEventBus.OnVisualAction += HandleVisualAction;
        }

        private void Update()
        {
            if (_animator == null || _rb == null || _wasDead) return;

            float speed = Mathf.Abs(_rb.velocity.x);
            bool grounded = CheckGrounded();
            bool moveRequested = false;
            if (_playerController != null)
                moveRequested = Mathf.Abs(_playerController.HorizontalInput) > 0.05f;

            _animator.SetFloat(SpeedHash, speed);
            _animator.SetBool(GroundedHash, grounded);
            _animator.SetFloat(VerticalVelocityHash, _rb.velocity.y);
            _animator.SetBool(MoveRequestedHash, moveRequested);

            if (!_wasDead)
            {
                _animator.SetBool(DeadHash, false);
            }
        }

        private bool CheckGrounded()
        {
            if (_groundCheck == null) return true;
            if (_groundLayer.value == 0) return false;
            return Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);
        }

        private void HandleVisualAction(VisualActionType action, float shotDirectionX)
        {
            if (_animator == null || _wasDead) return;
            if (_playerController == null) return;

            if (_playerController.IsActionRecoveryLocked)
                return;

            if (action == VisualActionType.FireRed || action == VisualActionType.FireBlue)
            {
                if (Mathf.Abs(shotDirectionX) > 0.001f)
                {
                    FlipVisualToFace(shotDirectionX);
                }
            }

            _playerController.StartActionRecovery();

            switch (action)
            {
                case VisualActionType.FireRed:
                    _animator.SetTrigger(FireRedHash);
                    break;
                case VisualActionType.FireBlue:
                    _animator.SetTrigger(FireBlueHash);
                    break;
                case VisualActionType.SelfActionBlue:
                    _animator.SetTrigger(SelfActionBlueHash);
                    break;
                case VisualActionType.SelfActionRed:
                    _animator.SetTrigger(SelfActionRedHash);
                    break;
            }
        }

        private void FlipVisualToFace(float shotDirectionX)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            float sign = shotDirectionX >= 0f ? 1f : -1f;
            Vector3 scale = player.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * sign;
            player.transform.localScale = scale;
        }

        private void OnPlayerDeath()
        {
            if (_deadTriggered) return;
            _deadTriggered = true;
            _wasDead = true;
            if (_animator != null)
                _animator.SetBool(DeadHash, true);
        }

        /// <summary>
        /// Exits the death animation on Retry. Clears the dead flags, rebinds the
        /// Animator back to its default (Idle) state and clears the Dead bool so the
        /// normal locomotion update resumes. Called by PlayerRuntimeReset.ResetForRetry.
        /// </summary>
        public void ResetDeathVisual()
        {
            _deadTriggered = false;
            _wasDead = false;

            if (_animator != null)
            {
                _animator.Rebind();
                _animator.SetBool(DeadHash, false);
                _animator.Update(0f);
            }

            Debug.Log("[Retry] Animator reset.");
        }

        private void OnDestroy()
        {
            if (_health != null)
                _health.OnDeath.RemoveListener(OnPlayerDeath);

            CardVisualEventBus.OnVisualAction -= HandleVisualAction;
        }
    }
}
