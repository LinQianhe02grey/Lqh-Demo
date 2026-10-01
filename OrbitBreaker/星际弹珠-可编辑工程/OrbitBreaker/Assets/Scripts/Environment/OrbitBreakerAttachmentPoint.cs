using System.Collections.Generic;
using UnityEngine;

namespace OrbitBreaker
{
    [DisallowMultipleComponent]
    public sealed class OrbitBreakerAttachmentPoint : MonoBehaviour
    {
        public const float CaptureRadius = 2.35f;
        public const float OrbitRadius = 1.08f;
        private static readonly HashSet<OrbitBreakerAttachmentPoint> active = new HashSet<OrbitBreakerAttachmentPoint>();
        public Vector2 Center => transform.position;
        public bool Spent { get; private set; }
        public void Consume(){if(Spent)return;Spent=true;OrbitBreakerSpentProp.Consume(gameObject);}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry() => active.Clear();
        private void OnEnable() => active.Add(this);
        private void OnDisable() => active.Remove(this);
        public static OrbitBreakerAttachmentPoint Nearest(Vector2 position)
        {
            OrbitBreakerAttachmentPoint nearest = null;
            float best = CaptureRadius * CaptureRadius;
            foreach (var point in active)
            {
                if (point == null || !point.isActiveAndEnabled) continue;
                float distance = (point.Center - position).sqrMagnitude;
                if (distance <= best) { best = distance; nearest = point; }
            }
            return nearest;
        }
    }
}
