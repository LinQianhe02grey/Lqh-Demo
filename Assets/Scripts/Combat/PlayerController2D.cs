using UnityEngine;

namespace Cardwin.Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController2D : MonoBehaviour
    {
        public float moveSpeed = 5f;
        public float jumpForce = 10f;
        public float dashStrength = 12f;
        public float dashDuration = 0.18f;
        public float dashCooldown = 0.8f;

        public int maxJumps = 2;

        private Rigidbody2D _rb;
        private int _jumpsRemaining;
        private bool _isDashing;
        private float _dashTimer;
        private float _dashCooldownTimer;
        private void Awake() { _rb = GetComponent<Rigidbody2D>(); }

        public void Move(float horizontalInput) { }

        public void Jump() { }

        public void StartDash() { }

        public void Fire() { }

        public void UseSelfCard() { }

        public void Reload() { }

        private void FlipSprite() { }
    }
}
