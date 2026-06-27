using System.Collections.Generic;
using UnityEngine;

namespace Cardwin.Lua
{
    /// <summary>
    /// Maps a bullet definition's `behavior` string (e.g. "Bullets.PierceBullet")
    /// to an <see cref="ILuaBulletBehavior"/>. While no Lua VM is integrated, the
    /// built-in C# behaviours are registered here. When xLua/tolua is added later,
    /// a single Lua-backed behaviour can be registered for any behavior id without
    /// touching the host or callers.
    /// </summary>
    public static class LuaBulletBehaviorRegistry
    {
        private static readonly Dictionary<string, ILuaBulletBehavior> _map =
            new Dictionary<string, ILuaBulletBehavior>();

        public static readonly ILuaBulletBehavior Fallback = new StraightBehavior();

        static LuaBulletBehaviorRegistry()
        {
            Register("Bullets.PierceBullet", new PierceBulletBehavior());
            Register("Bullets.HomingBullet", new HomingBulletBehavior());
        }

        public static void Register(string behaviorId, ILuaBulletBehavior behavior)
        {
            if (string.IsNullOrEmpty(behaviorId) || behavior == null)
                return;
            _map[behaviorId] = behavior;
        }

        public static ILuaBulletBehavior Resolve(string behaviorId)
        {
            if (string.IsNullOrEmpty(behaviorId))
                return null;
            return _map.TryGetValue(behaviorId, out ILuaBulletBehavior b) ? b : null;
        }

        /// <summary>Default behaviour: flies straight, damages and recycles on first hit.</summary>
        private sealed class StraightBehavior : ILuaBulletBehavior
        {
            public void OnSpawn(LuaBulletHost host) { }

            public void OnUpdate(LuaBulletHost host, float dt)
            {
                LuaBattleAPI.Move(host, host.Direction, host.Definition.Speed, dt);
            }

            public void OnHit(LuaBulletHost host, GameObject target)
            {
                LuaBulletDamage.Apply(host, target);
                host.Recycle();
            }

            public void OnRecycle(LuaBulletHost host) { }
        }
    }
}
