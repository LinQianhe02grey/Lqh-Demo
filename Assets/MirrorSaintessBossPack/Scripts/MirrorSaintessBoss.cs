using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cardwin.Combat;

namespace MirrorSaintessBossPack
{
    public enum MirrorSaintessPhase
    {
        Phase1,
        Phase2,
        Dead
    }

    /// <summary>
    /// Boss combat root for the Boss-fight V1 closed loop.
    /// Owns the total HP and the phase/death state machine. Damage arrives either
    /// directly on the root (via <see cref="IDamageable"/> fallback) or, more commonly,
    /// forwarded from a <see cref="MirrorSaintessBossPart"/> the player shot.
    /// State machine (minimal V1): Idle / Hurt / Phase2 / Dead.
    /// </summary>
    public sealed class MirrorSaintessBoss : MonoBehaviour, IDamageable
    {
        [Header("Identity")]
        [SerializeField] private string bossName = "Mirror Saintess";

        [Header("Total Health")]
        [SerializeField] private int maxTotalHp = 400;
        [SerializeField] private int currentTotalHp;
        [SerializeField, Range(0f, 1f)] private float phase2HealthRatio = 0.5f;

        [Header("Runtime References")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private Transform target;
        [SerializeField] private Transform firePointBlue;
        [SerializeField] private Transform firePointRed;
        [SerializeField] private GameObject blueProjectilePrefab;
        [SerializeField] private GameObject redProjectilePrefab;

        [Header("Parts")]
        [SerializeField] private List<MirrorSaintessBossPart> destructibleParts = new List<MirrorSaintessBossPart>();
        [SerializeField] private bool blueGunBroken;
        [SerializeField] private bool redGunBroken;
        [SerializeField] private bool chestCoreBroken;

        [Header("Behaviour")]
        [Tooltip("V1: keep false. The prototype auto-attack loop is disabled for the combat closed-loop test.")]
        [SerializeField] private bool startAttackLoop = false;
        [SerializeField] private bool autoFindPlayerByTag = true;
        [Tooltip("If false (default), bullets that hit the body/root deal NO total-HP damage until the ChestCore is broken. Forces the player to destroy parts first.")]
        [SerializeField] private bool allowDirectBodyDamage = false;

        [Header("Timing")]
        [SerializeField] private float hurtToIdleDelay = 0.45f;
        [SerializeField] private float phase2ToIdleDelay = 0.8f;
        [SerializeField] private float coreBreakStun = 2.0f;

        [Header("Feedback")]
        [SerializeField] private Color hurtFlashColor = new Color(1f, 0.5f, 0.5f, 1f);
        [SerializeField] private float hurtFlashDuration = 0.08f;

        private MirrorSaintessPhase _phase = MirrorSaintessPhase.Phase1;
        private int _currentPhaseNumber = 1;
        private bool _dead;
        private bool _inPhase2Transition;
        private bool _attackLoopStarted;
        private Coroutine _attackRoutine;
        private Coroutine _stateRoutine;
        private Coroutine _flashRoutine;
        private Color _bodyBaseColor = Color.white;

        /// <summary>Mover gate: boss may move when not dead and not playing the Phase2 transition.</summary>
        public bool CanMove => !_dead && !_inPhase2Transition;

        // ---- Public state (read by HUD / future AI) ----
        public string BossName => bossName;
        public int CurrentTotalHp => currentTotalHp;
        public int MaxTotalHp => maxTotalHp;
        public float HealthRatio => maxTotalHp > 0 ? (float)currentTotalHp / maxTotalHp : 0f;
        public MirrorSaintessPhase Phase => _phase;
        public int CurrentPhase => _currentPhaseNumber;
        public bool IsDead => _dead;
        public bool IsBlueGunBroken => blueGunBroken;
        public bool IsRedGunBroken => redGunBroken;
        public bool IsCoreBroken => chestCoreBroken;

        // ---- Events (HUD / Victory / future systems) ----
        public event Action<int, int> OnHealthChanged;   // (current, max)
        public event Action OnPartStateChanged;
        public event Action<int> OnPhaseChanged;          // new phase number
        public event Action OnBossDefeated;

        private void Awake()
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
            if (bodyRenderer == null)
            {
                Transform body = transform.Find("Body");
                if (body != null)
                    bodyRenderer = body.GetComponent<SpriteRenderer>();
            }
            if (bodyRenderer != null)
                _bodyBaseColor = bodyRenderer.color;

            if (destructibleParts.Count == 0)
                destructibleParts.AddRange(GetComponentsInChildren<MirrorSaintessBossPart>(true));

            for (int i = 0; i < destructibleParts.Count; i++)
            {
                if (destructibleParts[i] != null)
                    destructibleParts[i].Initialize(this);
            }

            currentTotalHp = maxTotalHp;
            _phase = MirrorSaintessPhase.Phase1;
            _currentPhaseNumber = 1;
            _dead = false;
        }

        private void Start()
        {
            if (autoFindPlayerByTag && target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    target = player.transform;
            }

            ForcePlayState("Idle");
            OnHealthChanged?.Invoke(currentTotalHp, maxTotalHp);

            if (startAttackLoop && !_attackLoopStarted)
                _attackRoutine = StartCoroutine(AttackLoop());
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        // ---- Damage entry points ----

        // IDamageable: bullet hit the boss root/body collider directly (fallback hit area).
        public void TakeHit(int amount, GameObject source)
        {
            // By default the player must destroy parts; direct body damage is ignored until
            // the core is broken (or unless explicitly allowed). Parts forward their own hits
            // via DealBossDamageFromPart, so the boss is still always killable through parts.
            if (!allowDirectBodyDamage && !chestCoreBroken)
            {
                FlashBody(hurtFlashColor);
                Debug.Log("[Boss] Direct body hit had no effect (hit a part instead; allowDirectBodyDamage=false, core intact).");
                return;
            }
            DealBossDamage(amount, null);
        }

        // Called by a part the player shot.
        public void DealBossDamageFromPart(int amount, MirrorSaintessBossPart sourcePart)
        {
            DealBossDamage(amount, sourcePart);
        }

        // Backward-compatible float entry (prototype / external).
        public void TakeDamage(float damage)
        {
            DealBossDamage(Mathf.Max(0, Mathf.RoundToInt(damage)), null);
        }

        private void DealBossDamage(int amount, MirrorSaintessBossPart sourcePart)
        {
            if (_dead || amount <= 0)
                return;

            currentTotalHp = Mathf.Max(0, currentTotalHp - amount);
            OnHealthChanged?.Invoke(currentTotalHp, maxTotalHp);
            FlashBody(hurtFlashColor);

            // Death takes priority to avoid state confusion (per spec).
            if (currentTotalHp <= 0)
            {
                Die();
                return;
            }

            // Phase 2 once at <= 50%.
            if (_currentPhaseNumber < 2 && currentTotalHp <= Mathf.RoundToInt(maxTotalHp * phase2HealthRatio))
            {
                EnterPhase2();
                return;
            }

            PlayStateThenIdle("Hurt", hurtToIdleDelay);
        }

        /// <summary>Stage 43: external heal (e.g. player Heal-card bullet). Additive; caps at max; no phase/death change.</summary>
        public void Heal(int amount)
        {
            if (_dead || amount <= 0)
                return;
            currentTotalHp = Mathf.Min(maxTotalHp, currentTotalHp + amount);
            OnHealthChanged?.Invoke(currentTotalHp, maxTotalHp);
            Debug.Log($"[MirrorAngelBoss] Heal applied: +{amount}, hp={currentTotalHp}/{maxTotalHp}");
        }

        public void NotifyPartBroken(MirrorSaintessBossPart part)
        {
            if (part == null)
                return;

            switch (part.PartType)
            {
                case MirrorSaintessPartType.BlueGun:
                    blueGunBroken = true;
                    break;
                case MirrorSaintessPartType.RedGun:
                    redGunBroken = true;
                    break;
                case MirrorSaintessPartType.ChestCore:
                    chestCoreBroken = true;
                    break;
            }

            OnPartStateChanged?.Invoke();
            Debug.Log($"[Boss] Part broken: {part.PartType} (blue={blueGunBroken} red={redGunBroken} core={chestCoreBroken})");

            if (_dead)
                return;

            if (part.PartType == MirrorSaintessPartType.ChestCore)
                PlayStateThenIdle("Hurt", coreBreakStun);
            else
                FlashBody(hurtFlashColor);
        }

        private void EnterPhase2()
        {
            if (_currentPhaseNumber >= 2 || _dead)
                return;

            _phase = MirrorSaintessPhase.Phase2;
            _currentPhaseNumber = 2;
            OnPhaseChanged?.Invoke(_currentPhaseNumber);
            Debug.Log("[Boss] Entering Phase 2.");

            if (_stateRoutine != null)
                StopCoroutine(_stateRoutine);
            _stateRoutine = StartCoroutine(Phase2Routine());
        }

        private IEnumerator Phase2Routine()
        {
            _inPhase2Transition = true;   // mover stops during the Phase2 animation
            ForcePlayState("Phase2");
            yield return new WaitForSeconds(phase2ToIdleDelay);
            _inPhase2Transition = false;
            if (!_dead)
                ForcePlayState("Idle");
            _stateRoutine = null;
        }

        private void Die()
        {
            if (_dead)
                return;

            _dead = true;
            _phase = MirrorSaintessPhase.Dead;

            if (_attackRoutine != null)
                StopCoroutine(_attackRoutine);
            if (_stateRoutine != null)
                StopCoroutine(_stateRoutine);

            ForcePlayState("Death");

            var rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.gravityScale = 0f;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }

            SetAllCollidersEnabled(false);

            Debug.Log("[Boss] Mirror Saintess defeated.");
            OnBossDefeated?.Invoke();
        }

        /// <summary>Full reset so the boss can be re-fought (Retry / debug).</summary>
        public void ResetBoss()
        {
            if (_stateRoutine != null) { StopCoroutine(_stateRoutine); _stateRoutine = null; }
            if (_attackRoutine != null) { StopCoroutine(_attackRoutine); _attackRoutine = null; }

            _dead = false;
            _phase = MirrorSaintessPhase.Phase1;
            _currentPhaseNumber = 1;
            _inPhase2Transition = false;
            _attackLoopStarted = false;
            blueGunBroken = false;
            redGunBroken = false;
            chestCoreBroken = false;
            currentTotalHp = maxTotalHp;

            for (int i = 0; i < destructibleParts.Count; i++)
            {
                if (destructibleParts[i] != null)
                    destructibleParts[i].ResetPart();
            }

            SetAllCollidersEnabled(true);
            ForcePlayState("Idle");
            if (bodyRenderer != null)
                bodyRenderer.color = _bodyBaseColor;

            OnHealthChanged?.Invoke(currentTotalHp, maxTotalHp);
            OnPartStateChanged?.Invoke();
            OnPhaseChanged?.Invoke(_currentPhaseNumber);
            Debug.Log("[Boss] Reset to full.");
        }

        private void SetAllCollidersEnabled(bool enabled)
        {
            Collider2D[] cols = GetComponentsInChildren<Collider2D>(true);
            for (int i = 0; i < cols.Length; i++)
                cols[i].enabled = enabled;
        }

        // ---- Animation helpers (prototype controller has states but no transitions) ----
        public void ForcePlayState(string stateName)
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
            if (animator != null && !string.IsNullOrEmpty(stateName))
                animator.Play(stateName, 0, 0f);
        }

        private void PlayStateThenIdle(string stateName, float delay)
        {
            if (_dead)
                return;
            if (_stateRoutine != null)
                StopCoroutine(_stateRoutine);
            _stateRoutine = StartCoroutine(StateThenIdleRoutine(stateName, delay));
        }

        private IEnumerator StateThenIdleRoutine(string stateName, float delay)
        {
            ForcePlayState(stateName);
            yield return new WaitForSeconds(delay);
            if (!_dead)
                ForcePlayState("Idle");
            _stateRoutine = null;
        }

        private void FlashBody(Color color)
        {
            if (bodyRenderer == null || !isActiveAndEnabled)
                return;
            if (_flashRoutine != null)
                StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(FlashRoutine(color));
        }

        private IEnumerator FlashRoutine(Color color)
        {
            bodyRenderer.color = color;
            yield return new WaitForSeconds(hurtFlashDuration);
            bodyRenderer.color = _bodyBaseColor;
            _flashRoutine = null;
        }

        // ---- Prototype auto-attack loop (disabled by default in V1) ----
        private IEnumerator AttackLoop()
        {
            _attackLoopStarted = true;
            yield return new WaitForSeconds(1.0f);
            while (!_dead)
            {
                yield return new WaitForSeconds(2.0f);
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Debug/Damage Boss 50")]
        private void DebugDamageBoss50() => DealBossDamage(50, null);

        [ContextMenu("Debug/Force Phase2")]
        private void DebugForcePhase2() => EnterPhase2();

        [ContextMenu("Debug/Kill Boss")]
        private void DebugKillBoss() => DealBossDamage(currentTotalHp, null);

        [ContextMenu("Debug/Reset Boss")]
        private void DebugResetBoss() => ResetBoss();

        [ContextMenu("Debug/Play Idle")]
        private void DebugPlayIdle() => ForcePlayState("Idle");

        [ContextMenu("Debug/Play CastBlue")]
        private void DebugPlayCastBlue() => ForcePlayState("CastBlue");

        [ContextMenu("Debug/Play CastRed")]
        private void DebugPlayCastRed() => ForcePlayState("CastRed");
#endif
    }
}
