using UnityEngine;
using Cardwin.Combat;

namespace Cardwin.Lua
{
    /// <summary>
    /// Safe, narrow surface that bullet behaviours (Lua or the C# bridge) are allowed
    /// to call. Behaviours never touch Health / Boss internals directly; all damage
    /// still flows through Health.TakeDamage or IDamageable.TakeHit, so the existing
    /// combat / boss damage-receiver paths stay authoritative.
    /// </summary>
    public static class LuaBattleAPI
    {
        // ---- Queries -------------------------------------------------------

        public static GameObject FindNearestEnemy(Vector3 pos)
        {
            GameObject best = null;
            float bestSqr = float.MaxValue;

            Health[] healths = Object.FindObjectsOfType<Health>();
            foreach (Health h in healths)
            {
                if (h == null || h.IsDead())
                    continue;
                if (h.CompareTag("Player"))
                    continue;

                float sqr = (h.transform.position - pos).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = h.gameObject;
                }
            }

            if (best != null)
                return best;

            // Fall back to any IDamageable (e.g. boss body receiver) if no Health enemy.
            var damageables = Object.FindObjectsOfType<MonoBehaviour>();
            foreach (MonoBehaviour mb in damageables)
            {
                if (!(mb is IDamageable) || mb.CompareTag("Player"))
                    continue;
                float sqr = (mb.transform.position - pos).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = mb.gameObject;
                }
            }

            return best;
        }

        public static GameObject FindNearestBoss(Vector3 pos)
        {
            var boss = Object.FindObjectOfType<MirrorSaintessBossPack.MirrorSaintessBoss>();
            return boss != null ? boss.gameObject : null;
        }

        public static bool IsDead(GameObject target)
        {
            if (target == null)
                return true;
            Health h = target.GetComponent<Health>() ?? target.GetComponentInParent<Health>();
            return h != null && h.IsDead();
        }

        /// <summary>Returns the GameObject that owns a Health or IDamageable, or null.</summary>
        public static GameObject ResolveDamageableOwner(GameObject go)
        {
            if (go == null)
                return null;

            Health h = go.GetComponent<Health>() ?? go.GetComponentInParent<Health>();
            if (h != null)
                return h.gameObject;

            var dmg = go.GetComponent<IDamageable>() ?? go.GetComponentInParent<IDamageable>();
            if (dmg is MonoBehaviour mb)
                return mb.gameObject;

            return null;
        }

        // ---- Damage --------------------------------------------------------

        public static void Damage(GameObject target, int amount)
        {
            if (target == null || amount <= 0)
                return;

            Health h = target.GetComponent<Health>() ?? target.GetComponentInParent<Health>();
            if (h != null)
            {
                h.TakeDamage(amount);
                return;
            }

            var dmg = target.GetComponent<IDamageable>() ?? target.GetComponentInParent<IDamageable>();
            dmg?.TakeHit(amount, null);
        }

        public static void DamagePercentOfMaxHp(GameObject target, float percent)
        {
            if (target == null || percent <= 0f)
                return;

            Health h = target.GetComponent<Health>() ?? target.GetComponentInParent<Health>();
            if (h != null)
            {
                int amount = Mathf.Max(1, Mathf.RoundToInt(h.maxHealth * percent));
                h.TakeDamage(amount);
                return;
            }

            // IDamageable targets (boss) do not expose maxHP; use a documented flat
            // approximation so percent bullets still register.
            var dmg = target.GetComponent<IDamageable>() ?? target.GetComponentInParent<IDamageable>();
            if (dmg != null)
            {
                int amount = Mathf.Max(1, Mathf.RoundToInt(percent * 100f));
                dmg.TakeHit(amount, null);
            }
        }

        public static void HealPlayer(float percent)
        {
            if (percent <= 0f)
                return;
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
                return;
            Health h = player.GetComponent<Health>();
            if (h == null)
                return;
            h.Heal(Mathf.Max(1, Mathf.RoundToInt(h.maxHealth * percent)));
        }

        // ---- Movement ------------------------------------------------------

        public static void Move(LuaBulletHost host, Vector2 dir, float speed, float dt)
        {
            if (host == null || dir == Vector2.zero)
                return;
            Vector2 nd = dir.normalized;
            host.Direction = nd;
            host.transform.position += (Vector3)(nd * speed * dt);
            host.transform.right = nd;
        }

        public static void MoveToward(LuaBulletHost host, Vector3 targetPos, float speed, float dt)
        {
            if (host == null)
                return;
            Vector2 dir = ((Vector2)(targetPos - host.transform.position));
            if (dir.sqrMagnitude < 0.0001f)
                dir = host.Direction;
            Move(host, dir, speed, dt);
        }

        // ---- FX ------------------------------------------------------------

        public static void PlayEffect(string effectId, Vector3 pos)
        {
            // No FX system wired yet; stub keeps the API stable for behaviours / future hot-update.
            Debug.Log($"[LuaBattleAPI] PlayEffect '{effectId}' at {pos}");
        }

        // ---- Lifecycle -----------------------------------------------------

        public static void RecycleBullet(LuaBulletHost host)
        {
            host?.Recycle();
        }
    }
}
