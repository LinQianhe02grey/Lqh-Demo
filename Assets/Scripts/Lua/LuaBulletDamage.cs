using UnityEngine;

namespace Cardwin.Lua
{
    /// <summary>
    /// Shared damage application keyed by the bullet definition's damageMode.
    /// All paths still route through <see cref="LuaBattleAPI"/> so combat / boss
    /// damage receivers remain authoritative.
    /// </summary>
    internal static class LuaBulletDamage
    {
        public static void Apply(LuaBulletHost host, GameObject target)
        {
            if (host == null || target == null)
                return;

            LuaBulletDefinition def = host.Definition;
            if (string.Equals(def.DamageMode, "PercentTargetMaxHp", System.StringComparison.OrdinalIgnoreCase))
            {
                LuaBattleAPI.DamagePercentOfMaxHp(target, def.Damage);
            }
            else // "Flat" (default)
            {
                int amount = Mathf.Max(1, Mathf.RoundToInt(def.Damage));
                LuaBattleAPI.Damage(target, amount);
            }
        }
    }
}
