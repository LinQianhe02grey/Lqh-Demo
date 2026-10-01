using System;
using System.Collections.Generic;
using UnityEngine;

namespace OrbitBreaker
{
    [DisallowMultipleComponent, RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
    public sealed class OrbitBreakerEnemyController : MonoBehaviour
    {
        public static readonly HashSet<OrbitBreakerEnemyController> Active=new HashSet<OrbitBreakerEnemyController>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetActive()=>Active.Clear();
        public const float MoveSpeed = 2.2f;
        public const int KillPoints = 80;
        public const int MaximumHealth = 100;
        public int Health { get; private set; } = MaximumHealth;
        public int HealthCapacity { get; private set; } = MaximumHealth;
        private float nextDamageAt;
        public bool TryDamage(OrbitBreakerPlayerMotor source, bool qualifiedImpact = false)
        {
            if (source != player || IsDead || !isActiveAndEnabled || manager == null || !manager.IsPlaying ||
                (!qualifiedImpact && !source.IsAttackActive) || Time.time < nextDamageAt) return false;
            nextDamageAt = Time.time + 0.35f;
            int damage=qualifiedImpact?source.ImpactDamage:source.AttackDamage;
            if(damage<=0)return false;
            Vector3 center=Position;
            ApplyDamage(source,damage,source.Position);
            source.TryDashBurst(center,damage);
            return true;
        }
        public bool TryExplosionDamage(OrbitBreakerPlayerMotor source,int damage,Vector3 center)
        {
            if(source!=player || IsDead || !isActiveAndEnabled || manager==null || !manager.IsPlaying || damage<=0)return false;
            ApplyDamage(source,damage,center);return true;
        }
        private void ApplyDamage(OrbitBreakerPlayerMotor source,int damage,Vector3 center)
        {
            Health = Mathf.Max(0, Health - damage);
            repelDirection = ((Vector2)(Position - center)).normalized;
            if (repelDirection.sqrMagnitude < 0.01f) repelDirection = Vector2.up;
            repelledUntil = Time.time + OrbitRepelDuration;
            if (Health > 0) return;
            IsDead = true; hitbox.enabled = false; Freeze();
            source.RewardEnemyKill(this);
            float dropChance=Mathf.Clamp01(manager.Config.shieldDropChance);
            if(dropChance>=1f || (dropChance>0f && UnityEngine.Random.value<dropChance))OrbitBreakerShieldPickup.Spawn(Position,player);
            OrbitBreakerCombatAudio.Play(CombatSound.Kill);
            score.TryAward(KillPoints, this, 1); Killed?.Invoke(this); Destroy(gameObject);
        }
        private void OnGUI()
        {
            if (IsDead || manager == null || !manager.IsPlaying || Camera.main == null) return;
            var point = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.8f);
            if (point.z <= 0f) return;
            var saved = GUI.color; float w = Mathf.Clamp(Screen.width / 35f, 28f, 55f);
            var rect = new Rect(point.x - w / 2f, Screen.height - point.y - 7f, w, 6f);
            GUI.color = new Color(0.08f,0.015f,0.04f); GUI.DrawTexture(rect, Texture2D.whiteTexture);
            rect.width *= (float)Health / HealthCapacity; GUI.color = new Color(1f,0.25f,0.4f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = saved;
        }
        public const float AttackCooldown = 1.2f;
        private float nextAttackAt;
        public float AttackCooldownRemaining => Mathf.Max(0f, nextAttackAt - Time.time);
        private Rigidbody body;
        private BoxCollider hitbox;
        private OrbitBreakerPlayerMotor player;
        private OrbitBreakerScoreManager score;
        private OrbitBreakerGameManager manager;
        private bool subscribed;
        public const float OrbitRepelSpeed = 12f;
        public const float OrbitRepelDuration = 0.4f;
        private float repelledUntil;
        private Vector2 repelDirection;
        private Collider playerCollider;
        private bool collisionIgnored;
        private Renderer visual;
        private MaterialPropertyBlock original, flash;
        private bool showingRepel;
        private float nextPhaseScan;
        // Pair filtering keeps player contact and orbit queries intact, without changing global layers.
        private void RefreshPhaseCollisions()
        {
            if (hitbox == null || player == null) return;
            foreach (var other in FindObjectsOfType<Collider>())
            {
                if (other == hitbox || other.isTrigger || other.GetComponentInParent<OrbitBreakerPlayerMotor>() != null) continue;
                Physics.IgnoreCollision(hitbox, other, true);
            }
            nextPhaseScan = Time.time + .5f;
        }
        public bool IsRepelled => !IsDead && manager != null && manager.IsPlaying && Time.time < repelledUntil;
        public bool IsDead { get; private set; }
        public Vector3 Position => body == null ? transform.position : body.position;
        public event Action<OrbitBreakerEnemyController> Killed;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            hitbox = GetComponent<BoxCollider>();
            visual = GetComponent<Renderer>();
            original = new MaterialPropertyBlock();
            flash = new MaterialPropertyBlock();
            if (visual != null) visual.GetPropertyBlock(original);
            body.useGravity = false;
            body.constraints = OrbitBreakerPlayerMotor.PlaneConstraints;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            Freeze();
        }
        public void Initialize(OrbitBreakerPlayerMotor target, OrbitBreakerScoreManager scoring)
        {
            if (player != null || target == null || scoring == null || target.OrbitBreakerGameManager != scoring.OrbitBreakerGameManager)
                throw new InvalidOperationException("OrbitBreakerEnemy requires one initialization with matching player and score manager.");
            player = target;
            playerCollider = player.GetComponent<SphereCollider>();
            score = scoring;
            manager = target.OrbitBreakerGameManager;
            Health = HealthCapacity = Mathf.Clamp(manager.Config.enemyHealth,20,500);
            if (isActiveAndEnabled) Subscribe();
        }
        private void OnEnable() { Active.Add(this);if (manager != null) Subscribe(); }
        private void Subscribe()
        {
            if (subscribed) return;
            subscribed = true;
            player.Impact += OnPlayerImpact;
            player.AttachmentChanged += OnAttachmentChanged;
            manager.StateChanged += OnStateChanged;
            OnStateChanged(manager.State);
            OnAttachmentChanged(player.IsAttached);
            RefreshPhaseCollisions();
        }
        private void OnDisable()
        {
            Active.Remove(this);
            if (subscribed)
            {
                if (player != null) player.Impact -= OnPlayerImpact;
                if (player != null) player.AttachmentChanged -= OnAttachmentChanged;
                if (manager != null) manager.StateChanged -= OnStateChanged;
                subscribed = false;
            }
            if (body != null) Freeze();
            SetPlayerPassThrough(false);
            RestoreVisual();
        }
        private void FixedUpdate()
        {
            if (IsDead || manager == null || !manager.IsPlaying || player == null || body.isKinematic) return;
            if (Time.time >= nextPhaseScan) RefreshPhaseCollisions();
            // Only this component writes the enemy body. Never write the player's Rigidbody.
            Vector2 delta = player.Position - body.position;
            body.velocity = IsRepelled ? (Vector3)(repelDirection * Mathf.Lerp(4f, OrbitRepelSpeed,
                Mathf.Clamp01((repelledUntil - Time.time) / OrbitRepelDuration))) : (Vector3)(delta.normalized * MoveSpeed);
            body.angularVelocity = Vector3.zero;
            body.rotation = Quaternion.identity;
            if (player != null && (body.position-player.Position).sqrMagnitude > 40f*40f)
            {
                IsDead = true;
                hitbox.enabled = false;
                Freeze();
                Destroy(gameObject); // Out-of-board cleanup is not a kill and never awards points.
            }
        }
        private void OnPlayerImpact(Collision collision, bool wasDashing)
        {
            if (IsDead || !isActiveAndEnabled || manager == null || !manager.IsPlaying || collision.rigidbody != body) return;
            if (!wasDashing)
            {
                if (player.IsAttached || IsRepelled) return;
                // Enter only: staying in contact never refreshes stun. Re-entry needs both cooldowns.
                if (Time.time >= nextAttackAt && player.TryReceiveHit()) nextAttackAt = Time.time + AttackCooldown;
                return;
            }
            TryDamage(player, true);
        }
        private void OnStateChanged(GameState state)
        {
            if (state == GameState.Playing && !IsDead)
            {
                body.isKinematic = false;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                body.WakeUp();
            }
            else Freeze();
        }
        private void Freeze()
        {
            repelledUntil = 0f;
            if (!body.isKinematic) { body.velocity = Vector3.zero; body.angularVelocity = Vector3.zero; }
            body.collisionDetectionMode = CollisionDetectionMode.Discrete;
            body.isKinematic = true;
        }

        private void OnAttachmentChanged(bool protect)=>SetPlayerPassThrough(protect || (player!=null && player.IsPiercing));
        public void SetPlayerPassThrough(bool protect)
        {
            if (hitbox == null || playerCollider == null) return;
            bool ignore = protect && isActiveAndEnabled && !IsDead;
            if (collisionIgnored == ignore) return;
            Physics.IgnoreCollision(hitbox, playerCollider, ignore);
            collisionIgnored = ignore;
        }

        public bool TryOrbitRepel(OrbitBreakerPlayerMotor source)
        {
            if (source != player || player == null || !player.IsAttached || !isActiveAndEnabled || IsDead ||
                manager == null || !manager.IsPlaying || body.isKinematic || IsRepelled) return false;
            Vector2 direction = body.position - player.Position;
            if (direction.sqrMagnitude < 0.001f) direction = (Vector2)body.position - player.CurrentAttachment.Center;
            repelDirection = direction.sqrMagnitude < 0.001f ? Vector2.up : direction.normalized;
            repelledUntil = Time.time + OrbitRepelDuration;
            if (player.GetComponent<Rigidbody>().velocity.sqrMagnitude > 2.25f) TryDamage(player);
            return true; // Only the enemy controller writes the enemy body.
        }

        private void LateUpdate()
        {
            if (!IsRepelled) { RestoreVisual(); return; }
            if (visual == null) return;
            visual.GetPropertyBlock(flash);
            flash.SetColor("_Color", new Color(0.35f, 1f, 0.9f));
            visual.SetPropertyBlock(flash);
            showingRepel = true;
        }
        private void RestoreVisual()
        {
            if (!showingRepel || visual == null) return;
            visual.SetPropertyBlock(original.isEmpty ? null : original);
            showingRepel = false;
        }
    }
}
