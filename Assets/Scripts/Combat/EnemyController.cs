using UnityEngine;
using Cardwin.Enemies;

namespace Cardwin.Combat
{
    public enum EnemyBehavior { Melee, Ranged }

    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyController : MonoBehaviour
    {
        [Header("Behavior")]
        public EnemyBehavior behavior = EnemyBehavior.Melee;
        public bool enablePatrol = true;

        [Header("Stats")]
        public float moveSpeed = 2f;
        public int contactDamage = 8;
        public float attackCooldown = 1f;
        public float aggroRange = 5f;

        [Header("Ranged")]
        public float shootRange = 10f;
        public float fireCooldown = 1.5f;
        public float projectileSpeed = 6f;
        public int projectileDamage = 6;

        [Header("Patrol")]
        public float patrolDistance = 2f;

        private Rigidbody2D _rb;
        private Health _health;
        private Transform _player;
        private float _attackTimer;
        private float _fireTimer;
        private float _startX;
        private int _patrolDir = 1;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _health = GetComponent<Health>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.freezeRotation = true;
            _startX = transform.position.x;

            EnsureVisual();
        }

        private void Start()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                _player = playerObj.transform;
        }

        private void Update()
        {
            if (_health != null && _health.IsDead())
                return;

            if (_attackTimer > 0f)
                _attackTimer -= Time.deltaTime;
            if (_fireTimer > 0f)
                _fireTimer -= Time.deltaTime;

            if (_player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                    _player = playerObj.transform;
                if (_player == null) return;
            }

            float dist = Vector2.Distance(transform.position, _player.position);

            if (behavior == EnemyBehavior.Melee)
            {
                UpdateMelee(dist);
            }
            else if (behavior == EnemyBehavior.Ranged)
            {
                UpdateRanged(dist);
            }
        }

        private void UpdateMelee(float dist)
        {
            if (dist <= aggroRange)
            {
                Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
                Vector2 targetPos = (Vector2)transform.position + dir * moveSpeed * Time.deltaTime;
                if (_rb != null)
                    _rb.MovePosition(targetPos);

                if (_spriteRenderer != null)
                    _spriteRenderer.flipX = dir.x < 0f;
            }
            else if (enablePatrol)
            {
                Patrol();
            }
        }

        private void UpdateRanged(float dist)
        {
            if (_spriteRenderer != null)
                _spriteRenderer.flipX = _player.position.x < transform.position.x;

            if (dist <= shootRange && _fireTimer <= 0f)
            {
                FireAtPlayer();
                _fireTimer = fireCooldown;
            }
        }

        private void Patrol()
        {
            float patrolTargetX = _startX + _patrolDir * patrolDistance;
            if (Mathf.Abs(transform.position.x - patrolTargetX) < 0.1f
                || Mathf.Abs(transform.position.x - _startX) > patrolDistance)
            {
                _patrolDir *= -1;
            }

            Vector2 patrolPos = new Vector2(
                transform.position.x + _patrolDir * moveSpeed * 0.4f * Time.deltaTime,
                transform.position.y);
            if (_rb != null)
                _rb.MovePosition(patrolPos);

            if (_spriteRenderer != null)
                _spriteRenderer.flipX = _patrolDir < 0;
        }

        private void FireAtPlayer()
        {
            if (_player == null) return;

            Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
            Vector3 spawnPos = transform.position + (Vector3)(dir * 0.5f);
            spawnPos.z = 0f;

            GameObject projObj = new GameObject("EnemyBullet");
            projObj.transform.position = spawnPos;
            projObj.layer = gameObject.layer;

            SpriteRenderer sr = projObj.AddComponent<SpriteRenderer>();
            Texture2D tex = new Texture2D(12, 12);
            Color[] pixels = new Color[144];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new Color(0.8f, 0.1f, 0.6f, 1f);
            tex.SetPixels(pixels);
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 12, 12), new Vector2(0.5f, 0.5f), 12f);

            CircleCollider2D col = projObj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.15f;

            Rigidbody2D rb = projObj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;

            EnemyProjectile ep = projObj.AddComponent<EnemyProjectile>();
            ep.Init(dir, projectileDamage, projectileSpeed);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (behavior != EnemyBehavior.Melee)
                return;

            if (!other.CompareTag("Player"))
                return;
            if (_attackTimer > 0f)
                return;

            Health playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage);
                _attackTimer = attackCooldown;
            }
        }

        private void EnsureVisual()
        {
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();

            if (_spriteRenderer != null && _spriteRenderer.sprite == null)
            {
                Color color = behavior == EnemyBehavior.Ranged
                    ? new Color(0.5f, 0.1f, 0.7f, 1f)
                    : new Color(0.9f, 0.2f, 0.1f, 1f);

                Texture2D tex = new Texture2D(48, 48);
                Color[] pixels = new Color[48 * 48];
                for (int i = 0; i < pixels.Length; i++)
                    pixels[i] = color;
                tex.SetPixels(pixels);
                tex.Apply();
                _spriteRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, 48, 48), new Vector2(0.5f, 0.5f), 48f);

                if (behavior == EnemyBehavior.Ranged)
                    _spriteRenderer.size = new Vector2(1.2f, 1.4f);
                else
                    _spriteRenderer.size = new Vector2(1.5f, 1.1f);
            }
        }
    }
}
