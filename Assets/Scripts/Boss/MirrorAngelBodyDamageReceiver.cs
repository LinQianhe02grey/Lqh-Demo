using UnityEngine;
using Cardwin.Combat;
using MirrorSaintessBossPack;

namespace Cardwin.Boss
{
    /// <summary>
    /// Stage 42: single-Body hurtbox for the simplified Mirror Angel boss. Implements
    /// <see cref="IDamageable"/> so the existing player projectile hits the Body trigger
    /// and forwards the damage to the boss root (<see cref="MirrorSaintessBoss"/>), which
    /// owns the total HP / Phase2 / Death logic. Replaces the removed destructible parts.
    /// No part-break, no firing, no mirror skills — just a stable hittable target.
    /// </summary>
    public sealed class MirrorAngelBodyDamageReceiver : MonoBehaviour, IDamageable
    {
        [SerializeField] private MirrorSaintessBoss owner;
        private MirrorAngelBossEffectReceiver _effectReceiver;

        private void Awake()
        {
            if (owner == null)
                owner = GetComponentInParent<MirrorSaintessBoss>();
            if (_effectReceiver == null)
                _effectReceiver = GetComponentInParent<MirrorAngelBossEffectReceiver>();
        }

        public void TakeHit(int amount, GameObject source)
        {
            if (amount <= 0)
                return;
            if (owner == null)
                owner = GetComponentInParent<MirrorSaintessBoss>();
            if (owner == null)
                return;

            // Route raw IDamageable damage through the effect receiver so the boss Shield
            // absorbs it too. (Card-effect bullets use IProjectileEffectReceiver directly.)
            if (_effectReceiver == null)
                _effectReceiver = GetComponentInParent<MirrorAngelBossEffectReceiver>();
            if (_effectReceiver != null)
                _effectReceiver.ApplyExternalDamage(amount);
            else
                owner.TakeHit(amount, source);

            Debug.Log($"[MirrorAngelBoss] Body hit, damage={amount}, hp={owner.CurrentTotalHp}/{owner.MaxTotalHp}", this);
        }
    }
}
