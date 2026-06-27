using UnityEngine;

namespace MirrorSaintessBossPack
{
    public sealed class MirrorSaintessProjectile : MonoBehaviour
    {
        [SerializeField] private float damage = 10f;
        [SerializeField] private float lifeTime = 4f;
        [SerializeField] private string playerTag = "Player";

        private Rigidbody2D _body;
        private Vector2 _direction = Vector2.left;
        private float _speed = 6f;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            Destroy(gameObject, lifeTime);
        }

        public void Fire(Vector2 direction, float speed)
        {
            _direction = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector2.left;
            _speed = speed;
            if (_body == null)
            {
                _body = GetComponent<Rigidbody2D>();
            }
            if (_body != null)
            {
                _body.velocity = _direction * _speed;
            }
        }

        private void Update()
        {
            if (_body == null)
            {
                transform.position += (Vector3)(_direction * _speed * Time.deltaTime);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag))
            {
                return;
            }

            // Project-independent damage handoff. Your project can implement TakeDamage(float)
            // on player Health or a relay component.
            other.SendMessageUpwards("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject);
        }
    }
}
