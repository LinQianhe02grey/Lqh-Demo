using UnityEngine;
using MirrorSaintessBossPack;

namespace Cardwin.Boss
{
    /// <summary>
    /// Gravity-based locomotion for the Mirror Angel boss (Stage 40). Uses a Dynamic
    /// Rigidbody2D so the boss falls and rests on the BossRoom ground via its own body
    /// collider (the part hit-colliders stay triggers). Implements a minimal state set:
    /// ground Walk between two bounds, periodic short Dash, periodic short Fly
    /// (gravityScale 0), and stops when dead or during the boss Phase2 transition
    /// (MirrorSaintessBoss.CanMove == false). No complex attacks / pathfinding.
    /// Stage 46.3 — when a MirrorAngelBossBrain is present and active, the mover
    /// defers movement direction decisions to the brain (DesiredMoveX) and skips
    /// its own patrol/chase/dash/fly logic.
    /// Exposes read-only state for the animator bridge.
    /// </summary>
    [RequireComponent(typeof(MirrorSaintessBoss))]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class MirrorAngelBossGravityMover : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private MirrorSaintessBoss boss;
        [Tooltip("Stage 46: unified facing / beam-origin source (defaults to component on this object).")]
        [SerializeField] private MirrorAngelFacingController facing;
        [Tooltip("Stage 46.3: optional boss brain (auto-resolved). When active, the mover uses brain.DesiredMoveX for walk direction and skips patrol/dash/fly.")]
        [SerializeField] private MirrorAngelBossBrain brain;
        [Tooltip("Stage 50.1: action controller ref for AirLaserMode detection.")]
        [SerializeField] private MirrorAngelBossActionController actionController;

        [Header("Bounds (Transforms; do not hardcode)")]
        [SerializeField] private Transform leftBound;
        [SerializeField] private Transform rightBound;

        [Header("Ground Check")]
        [SerializeField] private LayerMask groundMask = 1 << 8; // Ground
        [SerializeField] private float groundCheckDistance = 0.65f;
        [SerializeField] private float groundCheckWidth = 0.9f;
        [Tooltip("Vertical offset from root to the foot ray origin.")]
        [SerializeField] private float footLocalY = -0.4f;

        [Header("Walk")]
        [SerializeField] private float walkSpeed = 1.2f;
        [SerializeField] private float stopDistanceToPlayer = 3.5f;
        [SerializeField] private float playerDetectRange = 16f;

        [Header("Dash")]
        [SerializeField] private float dashCooldown = 4f;
        [SerializeField] private float dashDuration = 0.35f;
        [SerializeField] private float dashSpeed = 4.5f;

        [Header("Fly")]
        [SerializeField] private float flyCooldown = 6f;
        [SerializeField] private float flyDuration = 1.2f;
        [SerializeField] private float flyHeight = 2f;
        [SerializeField] private float flyBobAmplitude = 0.3f;
        [SerializeField] private float flyBobFrequency = 2f;
        [SerializeField] private float normalGravityScale = 3f;

        [Header("Cast (Phase2 / debug)")]
        [SerializeField] private float debugCastDuration = 0.6f;

        [Header("Facing")]
        [SerializeField] private bool facePlayer = true;

        private Transform _player;
        private float _minX, _maxX;
        private int _patrolDir = 1;

        private bool _isGrounded;
        private bool _isDashing;
        private bool _isFlying;

        private float _dashTimer;
        private float _dashEnd;
        private int _dashDir = 1;

        private float _flyTimer;
        private float _flyEnd;
        private float _flyBaseY;

        private float _castEnd;
        private bool _movementLocked;
        private bool _externalCasting;

        public bool IsGrounded => _isGrounded;
        public bool IsDashing => _isDashing;
        public bool IsFlying => _isFlying;
        public bool IsCasting => Time.time < _castEnd || _externalCasting;
        public float CurrentMoveSpeed => rb != null ? Mathf.Abs(rb.velocity.x) : 0f;
        public Rigidbody2D Rigidbody => rb;
        public bool IsMovementLocked => _movementLocked;

        private Vector2 _externalVelocity = new Vector2(-1f, -1f);
        private bool HasExternalVelocity => _externalVelocity.x > -0.5f;

        private bool BrainActive => brain != null && brain.isActiveAndEnabled;

        /// <summary>
        /// Stage 44: external lock so an active skill (e.g. MirrorAngelTripleBeamSkill)
        /// can stop horizontal locomotion while casting. Gravity / Y velocity are left
        /// untouched so the boss can cast in mid-air (NO grounded requirement).
        /// </summary>
        public void SetMovementLocked(bool locked) => _movementLocked = locked;

        /// <summary>Stage 44: force the CastMirror visual on/off via the animator bridge.</summary>
        public void SetCasting(bool casting) => _externalCasting = casting;

        /// <summary>Stage 48.1: allow a skill to set a temporary velocity that the BrainActive block will NOT overwrite.</summary>
        public void SetExternalVelocity(Vector2 velocity) => _externalVelocity = velocity;

        /// <summary>Stage 48.1: clear temporary external velocity, restoring normal brain movement control.</summary>
        public void ClearExternalVelocity() => _externalVelocity = new Vector2(-1f, -1f);

        private void Awake()
        {
            if (boss == null) boss = GetComponent<MirrorSaintessBoss>();
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (facing == null)
                facing = GetComponent<MirrorAngelFacingController>();
            if (brain == null)
                brain = GetComponent<MirrorAngelBossBrain>();
            if (actionController == null)
                actionController = GetComponent<MirrorAngelBossActionController>();
            if (rb != null)
                normalGravityScale = rb.gravityScale > 0f ? rb.gravityScale : normalGravityScale;
        }

        private void Start()
        {
            ResolveBounds();
            _dashTimer = dashCooldown;
            _flyTimer = flyCooldown;
        }

        private void ResolveBounds()
        {
            float lx = leftBound != null ? leftBound.position.x : transform.position.x - 5f;
            float rx = rightBound != null ? rightBound.position.x : transform.position.x + 5f;
            _minX = Mathf.Min(lx, rx);
            _maxX = Mathf.Max(lx, rx);
        }

        private void FixedUpdate()
        {
            if (boss == null || rb == null)
                return;

            UpdateGroundCheck();
            FindPlayer();

            // Death: stop everything, let gravity keep it resting on the ground.
            if (boss.IsDead)
            {
                _isDashing = false;
                _isFlying = false;
                if (rb.gravityScale <= 0f) rb.gravityScale = normalGravityScale;
                rb.velocity = new Vector2(0f, Mathf.Min(rb.velocity.y, 0f));
                return;
            }

            // Phase2 transition (CanMove == false but not dead): stop and let bridge show CastMirror.
            if (!boss.CanMove)
            {
                _isDashing = false;
                if (!_isFlying) rb.velocity = new Vector2(0f, rb.velocity.y);
                return;
            }

            // Stage 44: skill movement lock. Freeze horizontal locomotion but keep the
            // current Y velocity / gravity so the boss can cast while airborne. No
            // grounded check is performed here.
            if (_movementLocked)
            {
                _isDashing = false;
                if (!_isFlying) rb.velocity = new Vector2(0f, rb.velocity.y);
                return;
            }

            // Stage 50.1: AirLaserMode has full control of Rigidbody2D — don't interfere.
            if (actionController != null && actionController.IsActionLocked &&
                actionController.CurrentAction == MirrorAngelActionType.AirLaserMode)
            {
                return;
            }

            // Stage 46.3: when brain is active, the mover defers all walk direction
            // decisions to the brain. Dash, fly, and patrol are skipped — the brain
            // handles all movement timing through DesiredMoveX.
            // Stage 48.1: if a skill has set external velocity (e.g. dash), use that
            // instead of brain.DesiredMoveX to avoid zeroing skill-driven movement.
            if (BrainActive)
            {
                _isDashing = false;
                if (_isFlying)
                {
                    _isFlying = false;
                    rb.gravityScale = normalGravityScale;
                }
                if (HasExternalVelocity)
                {
                    rb.velocity = new Vector2(_externalVelocity.x, _externalVelocity.y < -0.5f ? rb.velocity.y : _externalVelocity.y);
                }
                else
                {
                    float brainDir = brain.DesiredMoveX;
                    rb.velocity = new Vector2(brainDir * walkSpeed, rb.velocity.y);
                }
                ClampX();
                UpdateFacing();
                return;
            }

            ResolveBounds();

            // ---- Fly ----
            if (_isFlying)
            {
                TickFly();
                ClampX();
                UpdateFacing();
                return;
            }

            // ---- Dash ----
            if (_isDashing)
            {
                if (Time.time >= _dashEnd) { _isDashing = false; }
                else
                {
                    rb.velocity = new Vector2(_dashDir * dashSpeed, rb.velocity.y);
                    ClampX();
                    UpdateFacing();
                    return;
                }
            }

            // Try to start fly / dash when grounded.
            if (_isGrounded && IsCasting == false)
            {
                _flyTimer -= Time.fixedDeltaTime;
                _dashTimer -= Time.fixedDeltaTime;
                if (_flyTimer <= 0f) { StartFly(); ClampX(); UpdateFacing(); return; }
                if (_dashTimer <= 0f) { StartDash(); ClampX(); UpdateFacing(); return; }
            }

            // ---- Ground Walk ----
            float dir = ComputeWalkDir();
            rb.velocity = new Vector2(dir * walkSpeed, rb.velocity.y);
            ClampX();
            UpdateFacing();
        }

        private float ComputeWalkDir()
        {
            float curX = transform.position.x;
            if (_player != null && Mathf.Abs(_player.position.x - curX) <= playerDetectRange)
            {
                float dx = _player.position.x - curX;
                return Mathf.Abs(dx) > stopDistanceToPlayer ? Mathf.Sign(dx) : 0f;
            }
            return _patrolDir;
        }

        private void StartDash()
        {
            _isDashing = true;
            _dashEnd = Time.time + dashDuration;
            _dashTimer = dashCooldown;
            float curX = transform.position.x;
            _dashDir = (_player != null) ? (int)Mathf.Sign(_player.position.x - curX) : _patrolDir;
            if (_dashDir == 0) _dashDir = _patrolDir;
        }

        private void StartFly()
        {
            _isFlying = true;
            _flyEnd = Time.time + flyDuration;
            _flyTimer = flyCooldown;
            _flyBaseY = transform.position.y + flyHeight;
            rb.gravityScale = 0f;
        }

        private void TickFly()
        {
            if (Time.time >= _flyEnd)
            {
                _isFlying = false;
                rb.gravityScale = normalGravityScale; // fall and land
                return;
            }
            float targetY = _flyBaseY + Mathf.Sin(Time.time * flyBobFrequency) * flyBobAmplitude;
            float vy = (targetY - transform.position.y) * 4f;
            float dir = ComputeWalkDir();
            rb.velocity = new Vector2(dir * walkSpeed * 0.6f, Mathf.Clamp(vy, -3f, 3f));
        }

        private void ClampX()
        {
            float x = rb.position.x;
            if (x <= _minX) { x = _minX; _patrolDir = 1; rb.velocity = new Vector2(Mathf.Max(0f, rb.velocity.x), rb.velocity.y); }
            else if (x >= _maxX) { x = _maxX; _patrolDir = -1; rb.velocity = new Vector2(Mathf.Min(0f, rb.velocity.x), rb.velocity.y); }
            if (!Mathf.Approximately(x, rb.position.x))
                rb.position = new Vector2(x, rb.position.y);
        }

        private void UpdateGroundCheck()
        {
            Vector2 origin = new Vector2(transform.position.x, transform.position.y + footLocalY);
            RaycastHit2D hitC = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundMask);
            RaycastHit2D hitL = Physics2D.Raycast(origin + new Vector2(-groundCheckWidth * 0.5f, 0f), Vector2.down, groundCheckDistance, groundMask);
            RaycastHit2D hitR = Physics2D.Raycast(origin + new Vector2(groundCheckWidth * 0.5f, 0f), Vector2.down, groundCheckDistance, groundMask);
            _isGrounded = hitC.collider != null || hitL.collider != null || hitR.collider != null;
        }

        /// <summary>
        /// Stage 46: facing is delegated to the single MirrorAngelFacingController. While
        /// not locked (skill cast), the boss faces the way it actually MOVES (so it can
        /// never walk backwards); when standing still it faces the player. The controller
        /// is the only writer of Body flip + BeamOrigin mirror.
        /// </summary>
        private void UpdateFacing()
        {
            if (!facePlayer || facing == null || facing.IsFacingLocked)
                return;
            float vx = rb != null ? rb.velocity.x : 0f;
            if (Mathf.Abs(vx) > 0.05f)
                facing.FaceMoveDirection(vx);
            else if (_player != null)
                facing.FaceTarget(_player);
        }

        private void FindPlayer()
        {
            if (_player != null) return;
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) _player = p.transform;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            if (leftBound != null) Gizmos.DrawWireSphere(leftBound.position, 0.4f);
            if (rightBound != null) Gizmos.DrawWireSphere(rightBound.position, 0.4f);
            if (leftBound != null && rightBound != null)
                Gizmos.DrawLine(leftBound.position, rightBound.position);
            Gizmos.color = Color.green;
            Vector3 origin = new Vector3(transform.position.x, transform.position.y + footLocalY, 0f);
            Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckDistance);
        }

#if UNITY_EDITOR
        [ContextMenu("Debug/Force Dash")]
        private void DebugForceDash() { _dashTimer = 0f; }

        [ContextMenu("Debug/Force Fly")]
        private void DebugForceFly() { if (Application.isPlaying) StartFly(); }

        [ContextMenu("Debug/Force CastMirror")]
        private void DebugForceCast() { _castEnd = Time.time + debugCastDuration; }
#endif
    }
}
