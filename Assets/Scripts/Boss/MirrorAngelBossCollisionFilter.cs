using System.Collections.Generic;
using UnityEngine;
using MirrorSaintessBossPack;

namespace Cardwin.Boss
{
    /// <summary>
    /// Stage 41: makes the Mirror Angel boss BODY collider physically collide ONLY with
    /// MainGround. Every other solid Collider2D in the loaded scenes (round0 / walls /
    /// platforms / decor) is ignored via Physics2D.IgnoreCollision so the boss is never
    /// blocked or stuck by anything except the main floor. Only the boss's own non-trigger
    /// body collider is filtered; the part hit-trigger colliders are left untouched so
    /// player projectiles can still hit them. Does NOT delete or disable any scene
    /// collider (other actors still use them); it only ignores them for THIS boss.
    /// </summary>
    [RequireComponent(typeof(MirrorSaintessBoss))]
    public sealed class MirrorAngelBossCollisionFilter : MonoBehaviour
    {
        [Tooltip("Name of the only collider the boss body is allowed to collide with.")]
        [SerializeField] private string mainGroundName = "MainGround";
        [SerializeField] private bool verboseLog = false;

        private readonly List<Collider2D> _bodyColliders = new List<Collider2D>();

        private void Start()
        {
            // Collect the boss's OWN solid (non-trigger) body colliders. Part hit colliders
            // are triggers and must keep receiving projectile overlaps, so we skip them.
            _bodyColliders.Clear();
            foreach (var col in GetComponentsInChildren<Collider2D>(true))
            {
                if (col == null) continue;
                if (col.isTrigger) continue;                 // skip part hit triggers
                if (col.GetComponentInParent<MirrorSaintessBossPart>() != null) continue; // safety
                _bodyColliders.Add(col);
            }

            if (_bodyColliders.Count == 0)
            {
                Debug.LogWarning("[BossCollisionFilter] No solid body collider found on boss; nothing to filter.");
                return;
            }

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            int ignored = 0, kept = 0;
            var all = FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
            foreach (var other in all)
            {
                if (other == null) continue;
                // Never touch the boss's own colliders (body or part triggers).
                if (other.transform.IsChildOf(transform)) continue;

                bool isMainGround = other.name == mainGroundName;
                foreach (var body in _bodyColliders)
                {
                    if (body == null) continue;
                    // Ignore collision with everything that is NOT MainGround.
                    Physics2D.IgnoreCollision(body, other, !isMainGround);
                }

                if (isMainGround) kept++;
                else ignored++;
            }

            Debug.Log($"[BossCollisionFilter] Boss body collides ONLY with '{mainGroundName}'. kept={kept}, ignored={ignored} other colliders.");
            if (verboseLog)
            {
                foreach (var other in all)
                {
                    if (other == null || other.transform.IsChildOf(transform)) continue;
                    Debug.Log($"[BossCollisionFilter]   {(other.name == mainGroundName ? "KEEP" : "IGNORE")} {other.name} (layer={LayerMask.LayerToName(other.gameObject.layer)})", other);
                }
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Debug/Reapply Collision Filter")]
        private void DebugReapply()
        {
            if (Application.isPlaying) { Start(); }
        }
#endif
    }
}
