using UnityEngine;

namespace Cardwin.Combat
{
    public class EnemyController : MonoBehaviour
    {
        public int maxHealth = 30;
        public int currentHealth;
        public int currentBlock;

        public enum AttackMode { None, Melee, Ranged }
        public enum MoveMode { None, PatrolChase, KeepDistance }

        public AttackMode attackMode = AttackMode.Melee;
        public MoveMode moveMode = MoveMode.PatrolChase;

        public float attackThinkInterval = 0.25f;
        public float moveThinkInterval = 0.02f;

        private void Awake() { currentHealth = maxHealth; }

        public void TakeDamage(int damage) { }

        public void Heal(int amount) { }

        public void GainBlock(int amount) { }

        public bool IsDead() { return currentHealth <= 0; }

        private void AttackThink() { }

        private void MoveThink() { }
    }
}
