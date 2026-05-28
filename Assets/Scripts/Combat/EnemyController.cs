using UnityEngine;

namespace Cardwin.Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyController : MonoBehaviour
    {
        public float moveSpeed = 3f;
        public int contactDamage = 8;
        public float attackCooldown = 1f;

        private Rigidbody2D _rb;
        private Health _health;
        private Transform _player;
        private float _attackTimer;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _health = GetComponent<Health>();
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
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

            if (_player == null)
                return;

            Vector2 toPlayer = _player.position - transform.position;
            float dir = toPlayer.x > 0f ? 1f : -1f;
            _rb.velocity = new Vector2(dir * moveSpeed, _rb.velocity.y);

            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * dir;
            transform.localScale = scale;
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                if (_attackTimer <= 0f)
                {
                    Health playerHealth = collision.gameObject.GetComponent<Health>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(contactDamage);
                        _attackTimer = attackCooldown;
                    }
                }
            }
        }
    }
}
