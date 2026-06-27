using System;
using UnityEngine;
using Cardwin.Combat;
using Cardwin.Cards;
using MirrorSaintessBossPack;

namespace Cardwin.Boss
{
    /// <summary>
    /// Stage 43: receives the FULL card effect carried by a player projectile and applies
    /// it to the Mirror Angel boss. Damage routes through a boss-local Shield (absorbed
    /// first) then the boss total HP; Block/Guard adds Shield; Heal raises boss HP; Focus
    /// applies a timed visible status. Only this boss implements
    /// <see cref="IProjectileEffectReceiver"/>; normal enemies are unaffected. Does not
    /// modify the player, projectile firing, card system or normal-enemy Health path.
    /// </summary>
    [RequireComponent(typeof(MirrorSaintessBoss))]
    public sealed class MirrorAngelBossEffectReceiver : MonoBehaviour, IProjectileEffectReceiver
    {
        [SerializeField] private MirrorSaintessBoss owner;
        [SerializeField] private int maxShield = 999;
        [SerializeField] private float focusBuffDuration = 5f;

        private int _currentShield;
        private string _buffName = "";
        private float _buffTimer;

        public int CurrentShield => _currentShield;
        public bool HasBuff => _buffTimer > 0f && !string.IsNullOrEmpty(_buffName);
        public string BuffName => HasBuff ? _buffName : "";
        public float BuffRemaining => Mathf.Max(0f, _buffTimer);

        public event Action<int> OnShieldChanged;
        public event Action<string, float> OnBuffChanged;

        private void Awake()
        {
            if (owner == null)
                owner = GetComponent<MirrorSaintessBoss>();
        }

        private void Update()
        {
            if (_buffTimer > 0f)
            {
                _buffTimer -= Time.deltaTime;
                if (_buffTimer <= 0f)
                {
                    _buffTimer = 0f;
                    Debug.Log($"[MirrorAngelBoss] Buff expired: {_buffName}");
                    _buffName = "";
                    OnBuffChanged?.Invoke("", 0f);
                }
            }
        }

        // ---- IProjectileEffectReceiver ----
        public void ReceiveProjectileEffect(Projectile projectile, Vector2 hitPoint)
        {
            if (projectile == null)
                return;
            if (owner == null)
                owner = GetComponent<MirrorSaintessBoss>();
            if (owner == null || owner.IsDead)
                return;

            // Raw bullet (no card effect): treat as plain damage.
            if (!projectile.UsesCardEffect)
            {
                ApplyDamage(projectile.damage);
                return;
            }

            CardData card = projectile.SourceCard;
            switch (projectile.EffectType)
            {
                case CardEffectType.Damage:
                    ApplyDamage(projectile.ResolveDamage());
                    break;
                case CardEffectType.Block:
                    AddShield(card != null ? card.block : 0);
                    break;
                case CardEffectType.Heal:
                    owner.Heal(card != null ? card.heal : 0); // logs in boss
                    break;
                case CardEffectType.Focus:
                    ApplyBuff("Focus", focusBuffDuration);
                    break;
                default:
                    Debug.Log($"[MirrorAngelBoss] Projectile effect ignored: {projectile.EffectType}");
                    break;
            }
        }

        // ---- Effect application ----
        private void ApplyDamage(int amount)
        {
            if (amount <= 0 || owner == null)
                return;

            if (_currentShield > 0)
            {
                int absorbed = Mathf.Min(_currentShield, amount);
                _currentShield -= absorbed;
                amount -= absorbed;
                OnShieldChanged?.Invoke(_currentShield);
                Debug.Log($"[MirrorAngelBoss] Shield absorbed {absorbed}, shield={_currentShield}, remainingDamage={amount}");
            }

            if (amount > 0)
                owner.TakeHit(amount, gameObject); // allowDirectBodyDamage=true -> deals + phase2/death
        }

        private void AddShield(int amount)
        {
            if (amount <= 0)
                return;
            _currentShield = Mathf.Min(maxShield, _currentShield + amount);
            OnShieldChanged?.Invoke(_currentShield);
            Debug.Log($"[MirrorAngelBoss] Shield applied: +{amount}, shield={_currentShield}");
        }

        private void ApplyBuff(string buffName, float duration)
        {
            _buffName = buffName;
            _buffTimer = duration;
            OnBuffChanged?.Invoke(_buffName, _buffTimer);
            Debug.Log($"[MirrorAngelBoss] Buff applied: {buffName}, duration={duration}s");
        }

        /// <summary>Called by the body hurtbox IDamageable path so raw hits are shield-aware too.</summary>
        public void ApplyExternalDamage(int amount) => ApplyDamage(amount);

#if UNITY_EDITOR
        [ContextMenu("Debug/Add Shield 30")] private void DbgShield() => AddShield(30);
        [ContextMenu("Debug/Apply Focus Buff")] private void DbgBuff() => ApplyBuff("Focus", focusBuffDuration);
        [ContextMenu("Debug/Heal 50")] private void DbgHeal() { if (owner != null) owner.Heal(50); }
#endif
    }
}
