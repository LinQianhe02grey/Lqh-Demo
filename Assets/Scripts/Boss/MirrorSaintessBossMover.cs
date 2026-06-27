using UnityEngine;
using MirrorSaintessBossPack;

namespace Cardwin.Boss
{
    /// <summary>
    /// Minimal Boss movement AI (V2): slow left/right patrol between two bounds, gentle
    /// approach toward the player with a stop distance, faster in Phase 2, faces the player.
    /// Uses the boss Kinematic Rigidbody2D + MovePosition with a locked Y so it never falls,
    /// never leaves the bounds, and never blocks the player (part colliders are triggers).
    /// No jumping, no pathfinding. Movement is gated by MirrorSaintessBoss.CanMove.
    /// </summary>
    [RequireComponent(typeof(MirrorSaintessBoss))]
    public sealed class MirrorSaintessBossMover : MonoBehaviour
    {
        [Header("Bounds (Transforms; do not hardcode)")]
        [SerializeField] private Transform leftBound;
        [SerializeField] private Transform rightBound;

        [Header("Visual")]
        [Tooltip("Only this visual is flipped to face the player (defaults to child 'Body'). Parts/colliders are NOT flipped.")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private bool facePlayer = true;
        [Tooltip("Set true if the body art faces RIGHT by default.")]
        [SerializeField] private bool artFacesRight = false;

        [Header("Speeds")]
        [SerializeField] private float phase1MoveSpeed = 1.2f;
        [SerializeField] private float phase2MoveSpeed = 1.8f;
        [SerializeField] private float stopDistanceToPlayer = 4f;
        [SerializeField] private float playerDetectRange = 14f;

        [Header("Refs")]
        [SerializeField] private Rigidbody2D bossRigidbody;

        private MirrorSaintessBoss _boss;
        private Transform _player;
        private float _fixedY;
        private float _minX, _maxX;
        private int _patrolDir = 1;
        private Vector3 _visualBaseScale = Vector3.one;
        private bool _initialized;

        private void Awake()
        {
            _boss = GetComponent<MirrorSaintessBoss>();
            if (bossRigidbody == null)
                bossRigidbody = GetComponent<Rigidbody2D>();
            if (visualRoot == null)
            {
                Transform body = transform.Find("Body");
                visualRoot = body != null ? body : transform;
            }
            if (visualRoot != null)
                _visualBaseScale = visualRoot.localScale;
        }

        private void Start()
        {
            _fixedY = transform.position.y;
            ResolveBounds();
            _initialized = true;
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
            if (!_initialized || _boss == null || bossRigidbody == null)
                return;

            // Dead / Phase2 transition -> stop.
            if (!_boss.CanMove)
                return;

            ResolveBounds();
            FindPlayer();

            float speed = _boss.CurrentPhase >= 2 ? phase2MoveSpeed : phase1MoveSpeed;
            float curX = transform.position.x;
            float targetDir;

            if (_player != null && Mathf.Abs(_player.position.x - curX) <= playerDetectRange)
            {
                float dx = _player.position.x - curX;
                float dist = Mathf.Abs(dx);
                targetDir = dist > stopDistanceToPlayer ? Mathf.Sign(dx) : 0f; // approach, stop at range
            }
            else
            {
                // Patrol: bounce between bounds.
                targetDir = _patrolDir;
            }

            float newX = curX + targetDir * speed * Time.fixedDeltaTime;

            // Clamp to bounds; reverse patrol direction at edges.
            if (newX <= _minX) { newX = _minX; _patrolDir = 1; }
            else if (newX >= _maxX) { newX = _maxX; _patrolDir = -1; }

            bossRigidbody.MovePosition(new Vector2(newX, _fixedY));

            UpdateFacing(curX);
        }

        private void UpdateFacing(float curX)
        {
            if (!facePlayer || visualRoot == null || _player == null)
                return;
            bool playerOnRight = _player.position.x >= curX;
            // facingSign: which way the art should point.
            float sign = playerOnRight ? 1f : -1f;
            if (!artFacesRight)
                sign = -sign;
            Vector3 s = _visualBaseScale;
            s.x = Mathf.Abs(_visualBaseScale.x) * sign;
            visualRoot.localScale = s;
        }

        private void FindPlayer()
        {
            if (_player != null)
                return;
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                _player = p.transform;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            float y = Application.isPlaying ? _fixedY : transform.position.y;
            if (leftBound != null)
                Gizmos.DrawWireSphere(leftBound.position, 0.4f);
            if (rightBound != null)
                Gizmos.DrawWireSphere(rightBound.position, 0.4f);
            if (leftBound != null && rightBound != null)
                Gizmos.DrawLine(new Vector3(leftBound.position.x, y, 0f), new Vector3(rightBound.position.x, y, 0f));
        }
    }
}
