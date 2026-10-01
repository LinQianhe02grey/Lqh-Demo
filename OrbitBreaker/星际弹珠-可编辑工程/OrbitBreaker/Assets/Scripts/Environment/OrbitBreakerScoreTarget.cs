using System;
using System.Collections.Generic;
using UnityEngine;

namespace OrbitBreaker
{
    [DisallowMultipleComponent, RequireComponent(typeof(BoxCollider))]
    public sealed class OrbitBreakerScoreTarget : MonoBehaviour
    {
        public const int NormalPoints = 50;
        public const int HighPoints = 100;
        public const float MinimumUpwardSpeed = 6f;
        public const float HitCooldown = 0.5f;
        public const float FlashDuration = 0.18f;
        [SerializeField] private OrbitBreakerScoreManager scoreManager;
        [SerializeField] private bool highValue;
        [SerializeField] private Renderer targetRenderer;
        private readonly HashSet<Collider> contacts = new HashSet<Collider>();
        private MaterialPropertyBlock originalBlock, flashBlock;
        private bool originalWasEmpty;
        private int interactionId;
        private float nextHit, flashUntil;
        public int Points => highValue ? HighPoints : NormalPoints;
        public float CooldownRemaining => Mathf.Max(0f, nextHit - Time.time);
        public Vector3 LastIncomingVelocity { get; private set; }
        public int ContactCount => contacts.Count;
        public int InteractionId => interactionId;
        public event Action Scored;

        private void Awake()
        {
            originalBlock = new MaterialPropertyBlock();
            flashBlock = new MaterialPropertyBlock();
            if (targetRenderer != null) targetRenderer.GetPropertyBlock(originalBlock);
            originalWasEmpty = originalBlock.isEmpty;
            flashBlock.SetColor("_Color", new Color(0.4f, 1f, 0.55f));
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!isActiveAndEnabled || collision.rigidbody == null) return;
            var player = collision.rigidbody.GetComponent<OrbitBreakerPlayerMotor>();
            if (player == null) return;
            bool firstContact = contacts.Count == 0;
            if (!contacts.Add(collision.collider) || !firstContact) return;
            interactionId++;
            // On this stationary target, relativeVelocity is the ball's incoming velocity,
            // before the contact solver stops it. Never read the post-contact Rigidbody velocity.
            LastIncomingVelocity = collision.relativeVelocity;
            if (scoreManager == null || player.OrbitBreakerGameManager != scoreManager.OrbitBreakerGameManager ||
                CooldownRemaining > 0f || LastIncomingVelocity.y < MinimumUpwardSpeed) return;
            if (!scoreManager.TryAward(Points, this, interactionId)) return;
            nextHit = Time.time + HitCooldown;
            flashUntil = Time.time + FlashDuration;
            if (targetRenderer != null) targetRenderer.SetPropertyBlock(flashBlock);
            Scored?.Invoke();
        }

        private void OnCollisionExit(Collision collision) => contacts.Remove(collision.collider);
        private void Update()
        {
            if (flashUntil <= 0f || Time.time < flashUntil) return;
            RestoreAppearance();
        }
        private void RestoreAppearance()
        {
            flashUntil = 0f;
            if (targetRenderer != null && originalBlock != null)
                targetRenderer.SetPropertyBlock(originalWasEmpty ? null : originalBlock);
        }
        private void OnDisable()
        {
            contacts.Clear();
            RestoreAppearance();
            // Preserve cooldown and event sequence across disable/enable within the same round.
        }
    }
}
