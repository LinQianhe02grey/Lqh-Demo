using UnityEngine;

namespace Cardwin.Lua
{
    /// <summary>
    /// C# bridge for the "Bullets.HomingBullet" Lua behaviour. Steers toward the
    /// nearest enemy (turnSpeed deg/s) and recycles on first hit.
    /// </summary>
    public sealed class HomingBulletBehavior : ILuaBulletBehavior
    {
        public void OnSpawn(LuaBulletHost host) { }

        public void OnUpdate(LuaBulletHost host, float dt)
        {
            LuaBulletDefinition def = host.Definition;
            Vector3 pos = host.transform.position;

            if (host.CurrentTarget == null || LuaBattleAPI.IsDead(host.CurrentTarget))
                host.CurrentTarget = LuaBattleAPI.FindNearestEnemy(pos);

            if (host.CurrentTarget != null)
            {
                Vector2 desired = ((Vector2)(host.CurrentTarget.transform.position - pos));
                if (desired.sqrMagnitude > 0.0001f)
                {
                    float maxDeg = def.TurnSpeed * dt;
                    float cur = Mathf.Atan2(host.Direction.y, host.Direction.x) * Mathf.Rad2Deg;
                    float want = Mathf.Atan2(desired.y, desired.x) * Mathf.Rad2Deg;
                    float next = Mathf.MoveTowardsAngle(cur, want, maxDeg);
                    Vector2 steered = new Vector2(
                        Mathf.Cos(next * Mathf.Deg2Rad),
                        Mathf.Sin(next * Mathf.Deg2Rad));
                    LuaBattleAPI.Move(host, steered, def.Speed, dt);
                    return;
                }
            }

            LuaBattleAPI.Move(host, host.Direction, def.Speed, dt);
        }

        public void OnHit(LuaBulletHost host, GameObject target)
        {
            LuaBulletDamage.Apply(host, target);
            host.Recycle();
        }

        public void OnRecycle(LuaBulletHost host) { }
    }
}
