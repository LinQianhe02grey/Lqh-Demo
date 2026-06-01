using UnityEngine;
using Cardwin.Combat;

namespace Cardwin.Enemies
{
    public class EnemyProjectile : MonoBehaviour
    {
        private Vector2 _direction;
        private float _speed;
        private int _damage;
        private float _lifetime = 5f;
        private float _timer;
        private Rigidbody2D _rb;
        private SpriteRenderer _spriteRenderer;
        private bool _initialized;
        private bool _hasHit;
        private const float HitRadius = 0.22f;

        private void Awake()
        {
            EnsureVisibleProjectile();

            _rb = GetComponent<Rigidbody2D>();
            if (_rb == null)
            {
                Debug.LogError($"[EnemyProjectile] Missing Rigidbody2D on {gameObject.name}.");
            }
            else
            {
                _rb.bodyType = RigidbodyType2D.Dynamic;
                _rb.gravityScale = 0f;
                _rb.freezeRotation = true;
                _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }

            Collider2D col = GetComponent<Collider2D>();
            if (col == null)
            {
                Debug.LogError($"[EnemyProjectile] Missing Collider2D on {gameObject.name}.");
                return;
            }

            col.isTrigger = true;

            CircleCollider2D circle = col as CircleCollider2D;
            if (circle != null && circle.radius < 0.12f)
                circle.radius = 0.18f;
        }

        public void Init(Vector2 direction, int damage, float speed)
        {
            _direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
            _damage = damage;
            _speed = speed;
            _timer = _lifetime;
            _initialized = true;

            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            if (_rb == null)
                _rb = GetComponent<Rigidbody2D>();

            if (_rb != null)
            {
                _rb.velocity = _direction * _speed;
            }

            Debug.Log($"[EnemyProjectile] Init direction={_direction} damage={_damage} speed={_speed}");
        }

        private void EnsureVisibleProjectile()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
            {
                Debug.LogError($"[EnemyProjectile] Missing SpriteRenderer on {gameObject.name}.");
                return;
            }

            if (_spriteRenderer.sprite == null)
                Debug.LogError($"[EnemyProjectile] Missing sprite on {gameObject.name}. Assign it in the prefab.");
        }

        private void Update()
        {
            if (!_initialized)
                return;

            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            if (_rb == null)
            {
                transform.position += (Vector3)(_direction * _speed * Time.deltaTime);
            }

            CheckManualHit();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            HandleHit(other);
        }

        private void CheckManualHit()
        {
            if (_hasHit)
                return;

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, HitRadius);
            foreach (Collider2D hit in hits)
            {
                if (HandleHit(hit))
                    return;
            }
        }

        private bool HandleHit(Collider2D other)
        {
            if (_hasHit || other == null || other.gameObject == gameObject)
                return false;

            if (other.CompareTag("Player"))
            {
                Health playerHealth = other.GetComponentInParent<Health>();
                if (playerHealth != null)
                {
                    _hasHit = true;
                    Debug.Log($"[EnemyProjectile] Hit Player damage={_damage}");
                    playerHealth.TakeDamage(_damage);
                }
                Destroy(gameObject);
                return true;
            }

            if (other.GetComponentInParent<MeleeEnemyController>() != null
                || other.GetComponentInParent<RangedEnemyController>() != null)
            {
                return false;
            }

            int groundLayer = LayerMask.NameToLayer("Ground");
            int defaultLayer = LayerMask.NameToLayer("Default");

            if (other.gameObject.layer == groundLayer
                || other.gameObject.layer == defaultLayer)
            {
                if (!other.isTrigger)
                {
                    _hasHit = true;
                    Debug.Log($"[EnemyProjectile] Hit Ground destroy. target={other.name}");
                    Destroy(gameObject);
                    return true;
                }
            }

            return false;
        }
    }
}
