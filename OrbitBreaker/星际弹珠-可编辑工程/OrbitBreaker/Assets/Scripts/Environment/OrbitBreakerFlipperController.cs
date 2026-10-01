using System;
using UnityEngine;

namespace OrbitBreaker
{
    public enum FlipperSide { Left, Right }

    [DisallowMultipleComponent, RequireComponent(typeof(BoxCollider))]
    public sealed class OrbitBreakerFlipperController : MonoBehaviour
    {
        public const float LaunchSpeed = 18f;
        public const float Cooldown = 0.25f;
        public const float StrokeDuration = 0.16f;
        [SerializeField] private OrbitBreakerPlayerMotor player;
        [SerializeField] private FlipperSide side;
        [SerializeField] private Transform paddle;
        private BoxCollider zone;
        private Quaternion restingRotation;
        private float nextActivation;
        private float strokeUntil;
        public FlipperSide Side => side;
        public float CooldownRemaining => Mathf.Max(0f, nextActivation - Time.time);
        public event Action Activated;

        private void Awake()
        {
            zone = GetComponent<BoxCollider>();
            if (paddle != null) restingRotation = paddle.localRotation;
        }

        public bool ContainsPlayer()
        {
            if (player == null || zone == null || !zone.enabled) return false;
            var local = transform.InverseTransformPoint(player.Position) - zone.center;
            var half = zone.size * 0.5f;
            return Mathf.Abs(local.x) <= half.x && Mathf.Abs(local.y) <= half.y && Mathf.Abs(local.z) <= half.z;
        }

        public bool TryActivate()
        {
            if (!isActiveAndEnabled || player == null || player.OrbitBreakerGameManager == null ||
                !player.OrbitBreakerGameManager.IsPlaying || CooldownRemaining > 0f || !ContainsPlayer()) return false;
            var direction = new Vector2(side == FlipperSide.Left ? 0.25f : -0.25f, 1f);
            if (!player.TryRequestBoardImpulse(direction, LaunchSpeed)) return false;
            player.RegisterDashContact(zone, LaunchSpeed);
            nextActivation = Time.time + Cooldown;
            strokeUntil = Time.time + StrokeDuration;
            Activated?.Invoke();
            return true;
        }

        private void Update()
        {
            if (player == null || player.OrbitBreakerGameManager == null || !player.OrbitBreakerGameManager.IsPlaying)
            {
                nextActivation = strokeUntil = 0f;
                RestorePaddle();
                return;
            }
            if (Input.GetKey(side == FlipperSide.Left ? KeyCode.J : KeyCode.K)) TryActivate();
            if (paddle != null)
            {
                float remaining = Mathf.Clamp01((strokeUntil - Time.time) / StrokeDuration);
                float angle = Mathf.Sin(remaining * Mathf.PI) * (side == FlipperSide.Left ? 40f : -40f);
                paddle.localRotation = restingRotation * Quaternion.Euler(0f, 0f, angle);
            }
        }

        private void OnDisable()
        {
            nextActivation = strokeUntil = 0f;
            RestorePaddle();
        }
        private void RestorePaddle() { if (paddle != null) paddle.localRotation = restingRotation; }
    }
}
