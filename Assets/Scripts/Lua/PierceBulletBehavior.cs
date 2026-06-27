using UnityEngine;

namespace Cardwin.Lua
{
    /// <summary>
    /// C# bridge for the "Bullets.PierceBullet" Lua behaviour. Flies straight and
    /// passes through up to <c>pierceCount</c> enemies, damaging each once.
    /// Stateless — per-bullet state (remaining pierce) lives on the host.
    /// </summary>
    public sealed class PierceBulletBehavior : ILuaBulletBehavior
    {
        public void OnSpawn(LuaBulletHost host)
        {
            // RemainingPierce is initialised from definition.PierceCount by the host.
        }

        public void OnUpdate(LuaBulletHost host, float dt)
        {
            LuaBulletDefinition def = host.Definition;
            LuaBattleAPI.Move(host, host.Direction, def.Speed, dt);
        }

        public void OnHit(LuaBulletHost host, GameObject target)
        {
            LuaBulletDamage.Apply(host, target);

            host.RemainingPierce--;
            if (host.RemainingPierce <= 0)
                host.Recycle();
        }

        public void OnRecycle(LuaBulletHost host) { }
    }
}
