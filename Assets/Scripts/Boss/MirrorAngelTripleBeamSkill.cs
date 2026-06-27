using System.Collections;
using UnityEngine;
using Cardwin.Combat;
using MirrorSaintessBossPack;

namespace Cardwin.Boss
{
    /// <summary>
    /// Stage 44 — Mirror Angel boss first active attack: "Mirror Triple Beam".
    /// Flow: stop moving + play CastMirror, lock-aim at the player and show a red
    /// warning line for 1s, then fire 3 beams (LineRenderer) toward the player's
    /// CURRENT position (re-aim each beam), each beam Raycasts/CircleCasts the player
    /// layer and deals damage once. The boss may cast in the air (NO grounded check,
    /// gravity untouched). Stops immediately if the boss dies.
    ///
    /// Stage 46.3 — autoCast disabled; skill is now exclusively triggered by
    /// MirrorAngelBossBrain via TryCast(). The brain owns all decision logic
    /// (distance / timing / scoring / candidate pool). This class only executes the
    /// cast sequence.
    ///
    /// Does NOT modify the player, player bullets, Projectile, cards, magazine,
    /// inventory, boss hit/HP/HUD logic, ground or portal. Player damage reuses the
    /// existing Cardwin.Combat.Health.TakeDamage(int).
    /// </summary>
    [RequireComponent(typeof(MirrorSaintessBoss))]
    public sealed class MirrorAngelTripleBeamSkill : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private MirrorSaintessBoss boss;
        [SerializeField] private MirrorAngelBossGravityMover mover;
        [Tooltip("Stage 46: unified facing controller (locked during the cast).")]
        [SerializeField] private MirrorAngelFacingController facing;
        [Tooltip("Beam spawn point (near the mirror). Falls back to root + originFallbackOffset.")]
        [SerializeField] private Transform beamOrigin;
        [SerializeField] private Vector2 originFallbackOffset = new Vector2(-0.8f, 0.8f);

        [Header("Beam FX (LineRenderer). Prefabs optional; runtime fallback if null)")]
        [SerializeField] private LineRenderer warningLinePrefab;
        [SerializeField] private LineRenderer beamLinePrefab;
        [Tooltip("Material used when building a runtime fallback warning line (no prefab).")]
        [SerializeField] private Material warningMaterial;
        [Tooltip("Material used when building a runtime fallback beam line (no prefab).")]
        [SerializeField] private Material beamMaterial;

        [Header("Targeting / Damage")]
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private float beamRange = 14f;
        [SerializeField] private int beamDamage = 10;
        [Tooltip("CircleCast radius for hit detection (more forgiving than a thin ray).")]
        [SerializeField] private float beamHitRadius = 0.18f;

        [Header("Timing")]
        [SerializeField] private float firstWarningTime = 1f;
        [SerializeField] private float beamVisibleTime = 0.15f;
        [SerializeField] private float intervalBetweenBeams = 0.25f;
        [SerializeField] private float recoveryTime = 0.5f;
        [SerializeField] private int beamCount = 3;
        [Tooltip("Stage 45: fixed rotation (deg) for beams 2/3 off the locked beam-1 direction.")]
        [SerializeField] private float beamSpreadAngle = 15f;
        [Tooltip("Optional short warning flash before beams 2..N (0 = no extra warning, faithful to spec).")]
        [SerializeField] private float shortWarningTimeLaterBeams = 0f;

        [Header("Visual")]
        [SerializeField] private float warningWidth = 0.06f;
        [SerializeField] private float beamWidth = 0.22f;
        [SerializeField] private Color warningColor = new Color(1f, 0.15f, 0.15f, 0.9f);
        [SerializeField] private Color beamColor = new Color(0.85f, 0.75f, 1f, 1f);
        [Tooltip("Sorting order for beams/warning (above boss=50 and terrain).")]
        [SerializeField] private int sortingOrder = 120;

        [Header("Auto Trigger (Stage 46.3 — disabled; Brain now drives all decisions)")]
        [SerializeField] private bool autoCast = false;
        [SerializeField] private float initialDelay = 1.5f;
        [SerializeField] private float cooldown = 4.5f;
        [SerializeField] private float minCastDistance = 2.5f;
        [SerializeField] private float maxCastDistance = 12f;
        [SerializeField, Range(0f, 1f)] private float attackChance = 0.65f;
        [Tooltip("Retry delay after a failed distance/random roll (avoids per-frame spam).")]
        [SerializeField] private float retryDelay = 0.75f;

        private Transform _player;
        private Health _playerHealth;
        private bool _isCasting;
        private float _nextCastTime;

        public bool IsCasting => _isCasting;

        private void Awake()
        {
            if (boss == null) boss = GetComponent<MirrorSaintessBoss>();
            if (mover == null) mover = GetComponent<MirrorAngelBossGravityMover>();
            if (facing == null) facing = GetComponent<MirrorAngelFacingController>();
            if (playerLayer.value == 0)
            {
                int layer = LayerMask.NameToLayer("Player");
                playerLayer = layer >= 0 ? (1 << layer) : playerLayer;
            }
        }

        private void Start()
        {
            _nextCastTime = Time.time + initialDelay;
        }

        private void Update()
        {
            if (!autoCast)
                return;
            if (boss != null && boss.IsDead)
                return;
            if (_isCasting)
                return;
            if (Time.time < _nextCastTime)
                return;

            if (!TryCast())
            {
                // Not cast this attempt (distance / random / not ready) — retry shortly.
                _nextCastTime = Time.time + retryDelay;
            }
        }

        /// <summary>
        /// Entry point (also usable by a future Brain). Returns true if a cast started.
        /// Re-entrancy / death / missing-player guarded.
        /// </summary>
        public bool TryCast()
        {
            if (_isCasting)
                return false;
            if (boss == null || boss.IsDead)
                return false;

            ResolvePlayer();
            if (_player == null)
                return false;

            float dist = Vector2.Distance(GetOrigin(), _player.position);
            if (dist < minCastDistance || dist > maxCastDistance)
                return false;

            if (Random.value > attackChance)
                return false;

            StartCoroutine(CastRoutine());
            return true;
        }

        public IEnumerator CastRoutine()
        {
            _isCasting = true;
            _nextCastTime = Time.time + cooldown;

            ResolvePlayer();

            if (facing != null && _player != null)
                facing.LockFacing(facing.GetFacingToTarget(_player));
            if (mover != null)
            {
                mover.SetMovementLocked(true);
                mover.SetCasting(true);
            }

            try
            {

            // ---- Lock the base direction (beam 1) toward the player, ONCE ----
            Vector2 baseDir = AimDirection();
            // Clockwise / counter order chosen by the player's vertical side of the origin:
            // player above origin -> beam2 to the upper side first (+), beam3 lower (-);
            // player below -> beam2 lower first (-), beam3 upper (+). Rotate(+deg) is CCW.
            float spreadSign = (_player != null && _player.position.y >= GetOrigin().y) ? 1f : -1f;

            // ---- Beam 1: locked direction with a full 1s red warning ----
            LineRenderer warn = SpawnLine(warningLinePrefab, warningMaterial, warningWidth, warningColor);
            UpdateLine(warn, GetOrigin(), GetOrigin() + baseDir * beamRange);

            float t = 0f;
            while (t < firstWarningTime)
            {
                if (Aborted()) { DestroyLine(warn); yield break; }
                // Keep the warning anchored to the boss origin (direction stays locked).
                UpdateLine(warn, GetOrigin(), GetOrigin() + baseDir * beamRange);
                t += Time.deltaTime;
                yield return null;
            }
            DestroyLine(warn);

            if (Aborted()) yield break;
            yield return FireBeam(baseDir);

            // ---- Beams 2..N: FIXED rotation off baseDir (do NOT re-aim the player) ----
            for (int i = 1; i < beamCount; i++)
            {
                float wait = intervalBetweenBeams;
                while (wait > 0f)
                {
                    if (Aborted()) yield break;
                    wait -= Time.deltaTime;
                    yield return null;
                }
                if (Aborted()) yield break;

                // Symmetric fan around baseDir: ±15°, ±30°, ... (exactly ±15° for 3 beams).
                float step = Mathf.Ceil(i / 2f) * beamSpreadAngle;
                float dirSign = (i % 2 == 1) ? spreadSign : -spreadSign;
                Vector2 dir = Rotate(baseDir, dirSign * step);

                if (shortWarningTimeLaterBeams > 0f)
                {
                    LineRenderer flash = SpawnLine(warningLinePrefab, warningMaterial, warningWidth, warningColor);
                    float ft = 0f;
                    while (ft < shortWarningTimeLaterBeams)
                    {
                        if (Aborted()) { DestroyLine(flash); yield break; }
                        UpdateLine(flash, GetOrigin(), GetOrigin() + dir * beamRange);
                        ft += Time.deltaTime;
                        yield return null;
                    }
                    DestroyLine(flash);
                    if (Aborted()) yield break;
                }

                yield return FireBeam(dir);
            }

            // ---- Recovery: stop the cast pose, keep movement locked briefly ----
            if (mover != null)
                mover.SetCasting(false);

            float rec = recoveryTime;
            while (rec > 0f)
            {
                if (Aborted()) yield break;
                rec -= Time.deltaTime;
                yield return null;
            }

            }
            finally
            {
                EndCast();
            }
        }

        // ---- Internal helpers ----

        private IEnumerator FireBeam(Vector2 dir)
        {
            Vector2 origin = GetOrigin();
            Vector2 end = origin + dir * beamRange;

            RaycastHit2D hit = Physics2D.CircleCast(origin, beamHitRadius, dir, beamRange, playerLayer);
            if (hit.collider != null)
            {
                end = hit.point;
                Health hp = hit.collider.GetComponentInParent<Health>();
                if (hp != null && !hp.IsDead())
                    hp.TakeDamage(beamDamage); // one hit per beam (single cast)
            }

            LineRenderer beam = SpawnLine(beamLinePrefab, beamMaterial, beamWidth, beamColor);
            UpdateLine(beam, origin, end);

            float v = beamVisibleTime;
            while (v > 0f)
            {
                v -= Time.deltaTime;
                yield return null;
            }
            DestroyLine(beam);
        }

        private bool Aborted()
        {
            if (boss != null && boss.IsDead)
            {
                EndCast();
                return true;
            }
            return false;
        }

        private void EndCast()
        {
            // Always release the facing lock first so death / interrupt / disable can never
            // leave the boss stuck facing the cast direction.
            if (facing != null)
                facing.UnlockFacing();
            if (mover != null)
            {
                mover.SetCasting(false);
                mover.SetMovementLocked(false);
            }
            _isCasting = false;
        }

        private Vector2 AimDirection()
        {
            ResolvePlayer();
            if (_player == null)
                return Vector2.right;
            Vector2 d = (Vector2)_player.position - GetOrigin();
            return d.sqrMagnitude > 0.0001f ? d.normalized : Vector2.right;
        }

        private Vector2 GetOrigin()
        {
            if (beamOrigin != null)
                return beamOrigin.position;
            return (Vector2)transform.position + originFallbackOffset;
        }

        private void ResolvePlayer()
        {
            if (_player != null)
                return;
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                _player = p.transform;
                _playerHealth = p.GetComponentInParent<Health>();
            }
        }

        private LineRenderer SpawnLine(LineRenderer prefab, Material fallbackMat, float width, Color color)
        {
            LineRenderer lr;
            if (prefab != null)
            {
                lr = Instantiate(prefab);
                lr.gameObject.SetActive(true);
            }
            else
            {
                var go = new GameObject("BossBeamRuntime");
                lr = go.AddComponent<LineRenderer>();
                lr.material = fallbackMat != null ? fallbackMat : new Material(Shader.Find("Sprites/Default"));
                lr.numCapVertices = 4;
            }
            lr.useWorldSpace = true;
            lr.positionCount = 2;
            lr.startWidth = width;
            lr.endWidth = width;
            lr.startColor = color;
            lr.endColor = color;
            lr.sortingOrder = sortingOrder;
            return lr;
        }

        /// <summary>Rotate a 2D vector by degrees (positive = counter-clockwise). Returns normalized.</summary>
        private static Vector2 Rotate(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos).normalized;
        }

        private static void UpdateLine(LineRenderer lr, Vector2 a, Vector2 b)
        {
            if (lr == null)
                return;
            lr.SetPosition(0, a);
            lr.SetPosition(1, b);
        }

        private static void DestroyLine(LineRenderer lr)
        {
            if (lr != null)
                Destroy(lr.gameObject);
        }

        private void OnDisable()
        {
            // Safety: never leave the boss locked/casting if disabled mid-routine.
            EndCast();
        }
    }
}
