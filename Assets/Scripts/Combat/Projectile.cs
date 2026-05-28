using UnityEngine;

namespace Cardwin.Combat
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        public float speed = 9f;
        public float lifetime = 3f;
        public int damage = 10;

        private Vector2 _direction;
        private Rigidbody2D _rb;
        private float _lifeTimer;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public void Init(Vector2 direction, int damageAmount)
        {
            _direction = direction.normalized;
            damage = damageAmount;
            _lifeTimer = lifetime;

            _rb.velocity = _direction * speed;

            if (_direction != Vector2.zero)
                transform.right = _direction;
        }

        private void Update()
        {
            _lifeTimer -= Time.deltaTime;
            if (_lifeTimer <= 0f)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}
