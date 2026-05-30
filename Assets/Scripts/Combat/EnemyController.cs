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
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.freezeRotation = true;
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

            Vector2 newPos = _rb.position + new Vector2(dir * moveSpeed * Time.deltaTime, 0f);
            _rb.MovePosition(newPos);

            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * dir;
            transform.localScale = scale;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryDamagePlayer(other.gameObject);
        }

        private void TryDamagePlayer(GameObject other)
        {
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
    }
}
