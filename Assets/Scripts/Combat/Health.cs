using UnityEngine;
using UnityEngine.Events;

namespace Cardwin.Combat
{
    public class Health : MonoBehaviour
    {
        public int maxHealth = 50;
        public int currentHealth;
        public int currentBlock;

        public bool IsInvincible { get; private set; }

        public UnityEvent<int> OnDamaged;
        public UnityEvent<int> OnHealed;
        public UnityEvent<int> OnBlockChanged;
        public UnityEvent OnDeath;

        private bool _isDead;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void SetInvincible(bool value)
        {
            IsInvincible = value;
        }

        public void TakeDamage(int damage)
        {
            if (_isDead)
                return;
            if (IsInvincible)
                return;

            int remaining = damage;
            if (currentBlock > 0)
            {
                int blockAbsorb = Mathf.Min(currentBlock, remaining);
                currentBlock -= blockAbsorb;
                remaining -= blockAbsorb;
                OnBlockChanged?.Invoke(currentBlock);
            }

            if (remaining > 0)
            {
                currentHealth -= remaining;
                OnDamaged?.Invoke(remaining);
            }

            if (currentHealth <= 0 && !_isDead)
                Die();
        }

        public void Heal(int amount)
        {
            if (_isDead)
                return;

            int before = currentHealth;
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            int healed = currentHealth - before;
            if (healed > 0)
                OnHealed?.Invoke(healed);
        }

        public void GainBlock(int amount)
        {
            if (_isDead)
                return;

            currentBlock += amount;
            OnBlockChanged?.Invoke(currentBlock);
        }

        public bool IsDead()
        {
            return _isDead;
        }

        private void Die()
        {
            _isDead = true;
            OnDeath?.Invoke();
        }
    }
}
