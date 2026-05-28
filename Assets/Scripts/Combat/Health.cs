using UnityEngine;
using UnityEngine.Events;

namespace Cardwin.Combat
{
    public class Health : MonoBehaviour
    {
        public int maxHealth = 50;
        public int currentHealth;
        public int currentBlock;

        public UnityEvent<int> OnDamaged;
        public UnityEvent<int> OnHealed;
        public UnityEvent<int> OnBlockChanged;
        public UnityEvent OnDeath;

        private bool _isDead;

        private void Awake() { currentHealth = maxHealth; }

        public void TakeDamage(int damage) { }

        public void Heal(int amount) { }

        public void GainBlock(int amount) { }

        public bool IsDead() { return _isDead; }

        private void Die() { }
    }
}
