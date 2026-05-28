using UnityEngine;

namespace Cardwin.Combat
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        public float speed = 9f;
        public float lifetime = 3f;

        private string _cardId;
        private Vector2 _direction;
        private Rigidbody2D _rb;

        public void Init(string cardId, Vector2 direction) { }

        private void OnTriggerEnter2D(Collider2D other) { }

        private void ApplyCardEffects(GameObject target) { }
    }
}
