using UnityEngine;

namespace Cardwin.Combat
{
    /// <summary>
    /// Generic damage receiver implemented by targets that do NOT use the standard
    /// <see cref="Health"/> component (e.g. the Mirror Saintess boss parts/root, which
    /// own their own HP and break logic). Normal enemies keep using <see cref="Health"/>
    /// and are not affected by this interface.
    /// </summary>
    public interface IDamageable
    {
        void TakeHit(int amount, GameObject source);
    }
}
