using System;
using UnityEngine;

namespace OrbitBreaker
{
    [DisallowMultipleComponent, RequireComponent(typeof(CapsuleCollider))]
    public sealed class OrbitBreakerBumper : MonoBehaviour
    {
        public const float ExitSpeed = 16f;
        public bool liftAssist;
        public int Variant {get;private set;}
        public float LaunchSpeed => Variant==0?14f:Variant==1?17f:21f;
        public void Configure(int variant){Variant=Mathf.Clamp(variant,0,2);if(Variant==2)liftAssist=true;}
        public const float ContactCooldown = 0.12f;
        [SerializeField] private Transform visual;
        private Vector3 restingScale;
        private float nextContact;
        private float pulseUntil;
        public event Action Bounced;

        private void Awake() { if (visual != null) restingScale = visual.localScale; }

        private void OnCollisionEnter(Collision collision)
        {
            if (!isActiveAndEnabled || Time.time < nextContact || collision.rigidbody == null) return;
            var player = collision.rigidbody.GetComponent<OrbitBreakerPlayerMotor>();
            if (player == null) return;
            var direction = (Vector2)(player.Position - transform.position);
            if (direction.sqrMagnitude < 0.001f) return;
            direction.Normalize();
            if (liftAssist) direction = new Vector2(direction.x, direction.y + .25f); // Bias upward without reversing an underside contact into the pin.
            if(Variant==1)direction+=new Vector2(-direction.y,direction.x)*.28f;
            if (!player.TryRequestBoardImpulse(direction, LaunchSpeed)) return;
            nextContact = pulseUntil = Time.time + ContactCooldown;
            Bounced?.Invoke();
        }

        private void Update()
        {
            if (visual == null) return;
            float remaining = Mathf.Clamp01((pulseUntil - Time.time) / ContactCooldown);
            visual.localScale = restingScale * (1f + Mathf.Sin(remaining * Mathf.PI) * 0.12f);
        }

        private void OnDisable()
        {
            nextContact = pulseUntil = 0f;
            if (visual != null) visual.localScale = restingScale;
        }
    }
}
