using UnityEngine;

namespace OrbitBreaker
{
    [DisallowMultipleComponent, RequireComponent(typeof(BoxCollider))]
    public sealed class OrbitBreakerHazard : MonoBehaviour
    {
        public const float ContactCooldown = 1.2f;
        private float nextHitAt;
        public float CooldownRemaining => Mathf.Max(0f, nextHitAt - Time.time);

        private void OnCollisionEnter(Collision collision)
        {
            if (!isActiveAndEnabled || Time.time < nextHitAt || collision.rigidbody == null) return;
            var player = collision.rigidbody.GetComponent<OrbitBreakerPlayerMotor>();
            // Hazards cannot be killed: even a dash into them is a hit, subject to protection.
            if (player != null && player.TryReceiveHit()) nextHitAt = Time.time + ContactCooldown;
        }

        private void OnDisable() => nextHitAt = 0f;
    }
}
