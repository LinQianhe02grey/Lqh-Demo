using UnityEngine;

namespace Cardwin.Combat
{
    /// <summary>
    /// Stage 43: optional receiver for the FULL card effect carried by a player
    /// <see cref="Projectile"/> (Damage / Block / Heal / Focus). Only the Mirror Angel
    /// boss implements this so non-damage card bullets (heal / guard / focus) can affect
    /// the boss too. Normal enemies do NOT implement it and keep their unchanged
    /// <see cref="Health"/> damage path.
    /// </summary>
    public interface IProjectileEffectReceiver
    {
        void ReceiveProjectileEffect(Projectile projectile, Vector2 hitPoint);
    }
}
