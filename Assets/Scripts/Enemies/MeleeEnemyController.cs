using UnityEngine;
using Cardwin.Combat;

namespace Cardwin.Enemies
{
    public enum EnemyState { Patrol, Chase, Attack, Return }

    public class MeleeEnemyController : MonoBehaviour
    {
        [Header("Stats")]
        public int maxHealth = 30;
        public float patrolSpeed = 1.4f;
        public float chaseSpeed = 2f;
        public float aggroRange = 5.5f;
        public float loseAggroRange = 8f;
        public float attackRange = 1.3f;
        public float leaveAttackRange = 1.7f;
        public float returnArriveThreshold = 0.3f;
        public int contactDamage = 8;
        public float attackCooldown = 1f;

        [Header("Patrol")]
        public bool enablePatrol = true;
        public float patrolDistance = 2.5f;

        [Header("Debug")]
        public bool showGizmos = true;

        private Rigidbody2D _rb;
        private Health _health;
        private Transform _player;
        private float _attackTimer;
        private float _startX;
        private int _patrolDir = 1;
        private EnemyState _state = EnemyState.Patrol;
        private EnemyState _previousState = EnemyState.Patrol;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _health = GetComponent<Health>();
            if (_rb != null)
            {
                _rb.bodyType = RigidbodyType2D.Kinematic;
                _rb.freezeRotation = true;
                _rb.gravityScale = 0f;
            }
            _startX = transform.position.x;
        }

        private void Start()
        {
            FindPlayer();
        }

        private void Update()
        {
            if (_health != null && _health.IsDead())
                return;

            if (_attackTimer > 0f)
                _attackTimer -= Time.deltaTime;

            if (_player == null)
            {
                FindPlayer();
                if (_player == null) return;
            }

            float dist = Vector2.Distance(transform.position, _player.position);

            switch (_state)
            {
                case EnemyState.Patrol:
                    UpdatePatrol(dist);
                    break;
                case EnemyState.Chase:
                    UpdateChase(dist);
                    break;
                case EnemyState.Attack:
                    UpdateAttack(dist);
                    break;
                case EnemyState.Return:
                    UpdateReturn(dist);
                    break;
            }
        }

        private void FixedUpdate()
        {
            if (_rb == null) return;
            UpdateMovement();
        }

        private void UpdatePatrol(float dist)
        {
            if (dist <= aggroRange)
            {
                TransitionTo(EnemyState.Chase);
                return;
            }

            float currentX = transform.position.x;
            float patrolTargetX = _startX + _patrolDir * patrolDistance;

            if (_patrolDir > 0 && currentX >= patrolTargetX - 0.1f)
                _patrolDir = -1;
            else if (_patrolDir < 0 && currentX <= patrolTargetX + 0.1f)
                _patrolDir = 1;
        }

        private void UpdateChase(float dist)
        {
            if (dist >= loseAggroRange)
            {
                TransitionTo(EnemyState.Return);
                return;
            }

            if (dist <= attackRange)
            {
                TransitionTo(EnemyState.Attack);
                return;
            }
        }

        private void UpdateAttack(float dist)
        {
            if (dist >= loseAggroRange)
            {
                TransitionTo(EnemyState.Return);
                return;
            }

            if (dist >= leaveAttackRange)
            {
                TransitionTo(EnemyState.Chase);
                return;
            }

            if (_attackTimer <= 0f)
            {
                TryDamagePlayer();
                _attackTimer = attackCooldown;
            }
        }

        private void UpdateReturn(float dist)
        {
            if (dist <= aggroRange)
            {
                TransitionTo(EnemyState.Chase);
                return;
            }

            if (Mathf.Abs(transform.position.x - _startX) <= returnArriveThreshold)
            {
                TransitionTo(EnemyState.Patrol);
                return;
            }
        }

        private void UpdateMovement()
        {
            float moveDir = 0f;
            float speed = 0f;

            switch (_state)
            {
                case EnemyState.Patrol:
                    moveDir = _patrolDir;
                    speed = patrolSpeed;
                    break;
                case EnemyState.Chase:
                    if (_player != null)
                        moveDir = Mathf.Sign(_player.position.x - transform.position.x);
                    speed = chaseSpeed;
                    break;
                case EnemyState.Attack:
                    return;
                case EnemyState.Return:
                    moveDir = Mathf.Sign(_startX - transform.position.x);
                    speed = patrolSpeed * 1.5f;
                    break;
            }

            if (Mathf.Abs(moveDir) < 0.01f || speed < 0.01f)
                return;

            Vector2 nextPos = _rb.position + new Vector2(moveDir * speed * Time.fixedDeltaTime, 0f);
            _rb.MovePosition(nextPos);

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.flipX = moveDir < 0f;
        }

        private void TryDamagePlayer()
        {
            if (_player == null) return;
            Health playerHealth = _player.GetComponent<Health>();
            if (playerHealth != null)
            {
                Debug.Log($"[MeleeEnemy] Attack player damage={contactDamage} enemy={gameObject.name}");
                playerHealth.TakeDamage(contactDamage);
            }
        }

        private void TransitionTo(EnemyState newState)
        {
            if (_state == newState) return;
            _previousState = _state;
            Debug.Log($"[MeleeEnemy] State {_previousState} -> {newState} enemy={gameObject.name}");
            _state = newState;
        }

        private void FindPlayer()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                _player = playerObj.transform;
        }

        private void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;
            Vector3 pos = transform.position;
            Vector3 startPos = new Vector3(_startX, pos.y, pos.z);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(pos, aggroRange);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(pos, loseAggroRange);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(pos, attackRange);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(new Vector3(_startX - patrolDistance, pos.y, 0f), new Vector3(_startX + patrolDistance, pos.y, 0f));
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(startPos, new Vector3(returnArriveThreshold * 2f, 0.2f, 0f));
        }
    }
}
