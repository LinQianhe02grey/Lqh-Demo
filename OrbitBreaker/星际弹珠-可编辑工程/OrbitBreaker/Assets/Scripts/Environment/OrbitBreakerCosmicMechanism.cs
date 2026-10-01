using UnityEngine;

namespace OrbitBreaker
{
    // Kinematic machine motion; all player impulses go through the motor.
    [RequireComponent(typeof(Rigidbody))]
    public sealed class OrbitBreakerCosmicMechanism : MonoBehaviour
    {
        public bool pendulum;
        public float speed = 70f;
        private Rigidbody body;
        private OrbitBreakerGameManager manager;
        private float elapsed, nextHit;
        private Quaternion rest;
        public float MotionAngle { get; private set; }
        private void Awake()
        {
            body = GetComponent<Rigidbody>(); body.isKinematic = true; body.useGravity = false;
            rest = body.rotation; manager = FindObjectOfType<OrbitBreakerGameManager>();
        }
        private void FixedUpdate()
        {
            if (manager == null || !manager.IsPlaying) return;
            elapsed += Time.fixedDeltaTime;
            MotionAngle = pendulum ? Mathf.Sin(elapsed * 2.8f) * 80f : elapsed * speed * 1.8f;
            body.MoveRotation(rest * Quaternion.Euler(0, 0, MotionAngle));
        }
        private void OnCollisionEnter(Collision hit)
        {
            if (Time.time < nextHit || hit.rigidbody == null) return;
            var player = hit.rigidbody.GetComponent<OrbitBreakerPlayerMotor>();
            if (player == null) return;
            Vector2 away = (Vector2)(player.Position - transform.position);
            if (away.sqrMagnitude < 0.01f) away = Vector2.up;
            if (player.TryRequestBoardImpulse(away.normalized + new Vector2(-away.y,away.x).normalized*.45f + Vector2.up * 0.2f, 23f)) nextHit = Time.time + 0.2f;
        }
    }
}
