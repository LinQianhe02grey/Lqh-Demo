using UnityEngine;
using Cardwin.Combat;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Cardwin.Enemies
{
    public class RangedEnemyController : MonoBehaviour
    {
        [Header("Stats")]
        public int maxHealth = 20;
        public float patrolSpeed = 1.2f;
        public float patrolDistance = 2.5f;
        public float shootRange = 16f;
        public float fireCooldown = 1.5f;
        public float projectileSpeed = 6f;
        public int projectileDamage = 6;

        [Header("Movement")]
        public bool isFlying = true;
        public float gravityScale = 0f;

        [Header("Projectile")]
        public GameObject enemyProjectilePrefab;

        private Rigidbody2D _rb;
        private Health _health;
        private Transform _player;
        private float _fireTimer;
        private float _startX;
        private float _startY;
        private int _patrolDir = 1;
        private bool _loggedTargetInRange;
        private float _patrolLogTimer;
        private const string EnemyProjectilePrefabPath = "Assets/Prefabs/Enemies/EnemyProjectile.prefab";

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _health = GetComponent<Health>();
            if (_rb != null)
            {
                _rb.bodyType = RigidbodyType2D.Kinematic;
                _rb.freezeRotation = true;
                _rb.gravityScale = isFlying ? gravityScale : 1f;
            }

            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
                col.isTrigger = true;

            _startX = transform.position.x;
            _startY = transform.position.y;

            ResolveProjectilePrefab();
        }

        private void Start()
        {
            FindPlayer();
        }

        private void Update()
        {
            if (_health != null && _health.IsDead())
                return;

            if (_fireTimer > 0f)
                _fireTimer -= Time.deltaTime;

            if (_player == null)
            {
                FindPlayer();
                if (_player == null)
                    return;
            }

            float dist = Vector2.Distance(transform.position, _player.position);

            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
                spriteRenderer.flipX = _player.position.x < transform.position.x;

            if (dist <= shootRange)
            {
                if (!_loggedTargetInRange)
                {
                    Debug.Log($"[RangedEnemy] Target in range. enemy={gameObject.name} distance={dist:F2}");
                    _loggedTargetInRange = true;
                }

                if (_fireTimer <= 0f)
                {
                    FireAtPlayer();
                    _fireTimer = fireCooldown;
                }
            }
            else
            {
                _loggedTargetInRange = false;
            }

            HorizontalPatrol();
        }

        private void HorizontalPatrol()
        {
            float currentX = transform.position.x;
            float patrolTargetX = _startX + _patrolDir * patrolDistance;

            if (Mathf.Abs(currentX - patrolTargetX) < 0.05f
                || Mathf.Abs(currentX - _startX) > patrolDistance + 0.1f)
            {
                _patrolDir *= -1;
            }

            float nextY = isFlying ? _startY + Mathf.Sin(Time.time * 1.5f) * 0.15f : transform.position.y;
            Vector2 targetPos = new Vector2(
                transform.position.x + _patrolDir * patrolSpeed * Time.deltaTime,
                nextY);

            transform.position = targetPos;

            if (_patrolLogTimer <= 0f)
            {
                Debug.Log($"[RangedEnemy] Patrol floating. enemy={gameObject.name}");
                _patrolLogTimer = 2f;
            }
            else
            {
                _patrolLogTimer -= Time.deltaTime;
            }
        }

        private void FireAtPlayer()
        {
            if (_player == null)
                return;

            if (enemyProjectilePrefab == null)
                ResolveProjectilePrefab();

            if (enemyProjectilePrefab == null)
            {
                Debug.LogError($"[RangedEnemy] Missing enemyProjectilePrefab on {gameObject.name}. Expected {EnemyProjectilePrefabPath}.");
                return;
            }

            Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
            Vector3 spawnPos = transform.position + (Vector3)(dir * 0.6f);
            spawnPos.z = 0f;

            GameObject projObj = Instantiate(enemyProjectilePrefab, spawnPos, Quaternion.identity);

            EnemyProjectile ep = projObj.GetComponent<EnemyProjectile>();
            if (ep != null)
            {
                Debug.Log($"[RangedEnemy] Fire projectile damage={projectileDamage} direction={dir} enemy={gameObject.name}");
                ep.Init(dir, projectileDamage, projectileSpeed);
            }
            else
            {
                Debug.LogError($"[RangedEnemy] EnemyProjectile component missing on prefab. enemy={gameObject.name}");
                Destroy(projObj);
            }
        }

        private void ResolveProjectilePrefab()
        {
            if (enemyProjectilePrefab != null)
                return;

#if UNITY_EDITOR
            enemyProjectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyProjectilePrefabPath);
            if (enemyProjectilePrefab != null)
            {
                EditorUtility.SetDirty(this);
                Debug.Log($"[RangedEnemy] Auto-bound enemyProjectilePrefab: {EnemyProjectilePrefabPath}");
                return;
            }
#endif

            Debug.LogError($"[RangedEnemy] enemyProjectilePrefab is not assigned and could not be found at {EnemyProjectilePrefabPath}.");
        }

        private void FindPlayer()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                _player = playerObj.transform;
                Debug.Log($"[RangedEnemy] Player found. enemy={gameObject.name}");
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, shootRange);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(new Vector3(_startX - patrolDistance, transform.position.y, 0f),
                new Vector3(_startX + patrolDistance, transform.position.y, 0f));
        }
    }
}
