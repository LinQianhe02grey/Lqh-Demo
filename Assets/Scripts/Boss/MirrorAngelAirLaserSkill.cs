using System.Collections;
using UnityEngine;
using MirrorSaintessBossPack;
using Cardwin.Combat;

namespace Cardwin.Boss
{
    public sealed class MirrorAngelAirLaserSkill : MonoBehaviour
    {
        private enum AirSubState { None, Rise, Hover, Move, Dash, Laser, Exit }

        [Header("Refs")]
        [SerializeField] private MirrorSaintessBoss boss;
        [SerializeField] private MirrorAngelBossGravityMover mover;
        [SerializeField] private MirrorAngelBossActionController actionController;
        [SerializeField] private MirrorAngelBossAnimatorBridge animBridge;
        [SerializeField] private MirrorAngelFacingController facing;

        [Header("Height")]
        [SerializeField] private float riseHeight = 3.5f;
        [SerializeField] private float riseTime = 0.45f;
        [SerializeField] private float airDurationMin = 6f;
        [SerializeField] private float airDurationMax = 10f;
        [SerializeField] private float exitTime = 0.4f;

        [Header("Air Movement")]
        [SerializeField] private float airMoveSpeed = 2.5f;
        [SerializeField] private float airMoveIntervalMin = 1.5f;
        [SerializeField] private float airMoveIntervalMax = 3.5f;
        [SerializeField] private float airMoveChance = 0.6f;

        [Header("Air Dash")]
        [SerializeField] private float airDashDistance = 4.0f;
        [SerializeField] private float airDashDuration = 0.35f;
        [SerializeField] private float airDashChance = 0.45f;

        [Header("Laser")]
        [SerializeField] private float airLaserIntervalMin = 0.8f;
        [SerializeField] private float airLaserIntervalMax = 1.5f;
        [SerializeField] private float airLaserChance = 0.75f;
        [SerializeField] private float laserRange = 16f;
        [SerializeField] private float laserWarningTime = 0.85f;
        [SerializeField] private float laserVisibleTime = 0.18f;
        [SerializeField] private int laserDamage = 10;
        [SerializeField] private LayerMask playerLayer;

        [Header("LineRenderer Prefabs")]
        [SerializeField] private LineRenderer warningLinePrefab;
        [SerializeField] private LineRenderer beamLinePrefab;
        [SerializeField] private Material warningMaterial;
        [SerializeField] private Material beamMaterial;
        [SerializeField] private Color warningColor = new Color(1f, 0.05f, 0.05f, 0.85f);
        [SerializeField] private Color beamColor = new Color(0.85f, 0.75f, 1f, 1f);
        [SerializeField] private float warningWidth = 0.08f;
        [SerializeField] private float beamWidth = 0.24f;
        [SerializeField] private int sortingOrder = 120;

        [Header("Debug")]
        [SerializeField] private AirSubState currentAirSubState;

        private Transform _player;
        private Rigidbody2D _rb;
        private float _originalGravityScale;
        private RigidbodyConstraints2D _originalConstraints;
        private bool _isCasting;

        public bool IsCasting => _isCasting;

        private void Awake()
        {
            if (boss == null) boss = GetComponent<MirrorSaintessBoss>();
            if (mover == null) mover = GetComponent<MirrorAngelBossGravityMover>();
            if (actionController == null) actionController = GetComponent<MirrorAngelBossActionController>();
            if (animBridge == null) animBridge = GetComponent<MirrorAngelBossAnimatorBridge>();
            if (facing == null) facing = GetComponent<MirrorAngelFacingController>();
            if (playerLayer.value == 0) { int l = LayerMask.NameToLayer("Player"); playerLayer = l >= 0 ? (1 << l) : playerLayer; }
            _rb = GetComponent<Rigidbody2D>();
        }

        private void SetAirSubType(AirSubState st)
        {
            currentAirSubState = st;
            int val = 1; // default Hover
            switch (st)
            {
                case AirSubState.Rise:
                case AirSubState.Hover:
                case AirSubState.Move: val = 1; break;
                case AirSubState.Dash: val = 2; break;
                case AirSubState.Laser: val = 3; break;
                case AirSubState.Exit:
                case AirSubState.None: val = 0; break;
            }
            var anim = animBridge != null ? animBridge.Animator : null;
            if (anim != null) anim.SetInteger("AirSubType", val);
        }

        public bool TryCast()
        {
            if (_isCasting || boss == null || boss.IsDead) return false;
            ResolvePlayer();
            _isCasting = true;
            StartCoroutine(CastRoutine());
            return true;
        }

        private IEnumerator CastRoutine()
        {
            if (_rb != null)
            {
                _originalGravityScale = _rb.gravityScale;
                _originalConstraints = _rb.constraints;
                _rb.gravityScale = 0f;
                _rb.velocity = Vector2.zero;
            }

            float startY = _rb != null ? _rb.position.y : transform.position.y;
            float hoverY = startY + riseHeight;

            // ---- Rise ----
            SetAirSubType(AirSubState.Rise);
            float riseElapsed = 0f;
            while (riseElapsed < riseTime)
            {
                if (Aborted()) yield break;
                riseElapsed += Time.fixedDeltaTime;
                float t = Mathf.Clamp01(riseElapsed / riseTime);
                float y = Mathf.Lerp(startY, hoverY, t * t * (3f - 2f * t));
                SetY(y);
                yield return new WaitForFixedUpdate();
            }
            SetY(hoverY);
            if (Aborted()) yield break;

            SetAirSubType(AirSubState.Hover);

            // ---- Air Loop ----
            float airDuration = Random.Range(airDurationMin, airDurationMax);
            float minRequired = riseTime + laserWarningTime + laserVisibleTime + exitTime + 0.3f;
            airDuration = Mathf.Max(airDuration, minRequired);
            float airEndTime = Time.time + airDuration;

            bool hasFiredAtLeastOneLaser = false;
            float nextLaserTime = Time.time + 0.3f;
            float nextDashTime = Time.time + Random.Range(1.5f, 3.0f);
            float nextMoveTime = Time.time + Random.Range(0.5f, 1.5f);

            while (Time.time < airEndTime)
            {
                if (Aborted()) yield break;

                // --- Must fire first laser if none yet ---
                if (!hasFiredAtLeastOneLaser && Time.time >= nextLaserTime)
                {
                    yield return FireAirLaser();
                    hasFiredAtLeastOneLaser = true;
                    nextLaserTime = Time.time + Random.Range(airLaserIntervalMin, airLaserIntervalMax);
                    nextMoveTime = Time.time + 0.5f;
                    continue;
                }

                // --- After first laser fired, mix actions ---
                // Air dash
                if (hasFiredAtLeastOneLaser && Time.time >= nextDashTime && Random.value < airDashChance)
                {
                    SetAirSubType(AirSubState.Dash);
                    yield return DoAirDash();
                    nextDashTime = Time.time + Random.Range(1.5f, 3.0f);
                    SetAirSubType(AirSubState.Hover);
                    continue;
                }

                // Air lateral move
                if (hasFiredAtLeastOneLaser && Time.time >= nextMoveTime && Random.value < airMoveChance)
                {
                    SetAirSubType(AirSubState.Move);
                    DoAirMove();
                    nextMoveTime = Time.time + Random.Range(airMoveIntervalMin, airMoveIntervalMax);
                }

                // Subsequent laser
                if (hasFiredAtLeastOneLaser && Time.time >= nextLaserTime && Random.value < airLaserChance)
                {
                    yield return FireAirLaser();
                    nextLaserTime = Time.time + Random.Range(airLaserIntervalMin, airLaserIntervalMax);
                    SetAirSubType(AirSubState.Hover);
                }

                // Keep hover Y
                float currentY = _rb != null ? _rb.position.y : transform.position.y;
                if (Mathf.Abs(currentY - hoverY) > 0.5f)
                {
                    SetY(Mathf.Lerp(currentY, hoverY, 0.15f));
                }

                yield return null;
            }

            if (Aborted()) yield break;

            // ---- Exit ----
            SetAirSubType(AirSubState.Exit);
            float exitElapsed = 0f;
            float exitStartY = _rb != null ? _rb.position.y : transform.position.y;
            while (exitElapsed < exitTime)
            {
                if (Aborted()) yield break;
                exitElapsed += Time.fixedDeltaTime;
                float t = Mathf.Clamp01(exitElapsed / exitTime);
                float y = Mathf.Lerp(exitStartY, startY, t * t * (3f - 2f * t));
                SetY(y);
                yield return new WaitForFixedUpdate();
            }
            SetY(startY);

            SetAirSubType(AirSubState.None);
            EndCastNormal();
        }

        private void SetY(float y)
        {
            if (_rb != null) { var p = _rb.position; p.y = y; _rb.MovePosition(p); }
            else { var p = transform.position; p.y = y; transform.position = p; }
        }

        private void DoAirMove()
        {
            ResolvePlayer();
            float dir = _player != null ? Mathf.Sign(_player.position.x - transform.position.x) : 1f;
            float moveX = dir * airMoveSpeed * Time.fixedDeltaTime * 3f;
            if (_rb != null) { var p = _rb.position; p.x += moveX; _rb.MovePosition(p); }
            else { var p = transform.position; p.x += moveX; transform.position = p; }
        }

        private IEnumerator DoAirDash()
        {
            ResolvePlayer();
            float dashDir = _player != null ? Mathf.Sign(_player.position.x - transform.position.x) : 1f;
            float startX = _rb != null ? _rb.position.x : transform.position.x;
            float targetX = startX + dashDir * airDashDistance;
            float elapsed = 0f;
            while (elapsed < airDashDuration)
            {
                if (Aborted()) yield break;
                elapsed += Time.fixedDeltaTime;
                float t = Mathf.Clamp01(elapsed / airDashDuration);
                float x = Mathf.Lerp(startX, targetX, t * t * (3f - 2f * t));
                if (_rb != null) { var p = _rb.position; p.x = x; _rb.MovePosition(p); }
                else { var p = transform.position; p.x = x; transform.position = p; }
                yield return new WaitForFixedUpdate();
            }
            if (_rb != null) { var fp = _rb.position; fp.x = targetX; _rb.MovePosition(fp); }
            else { var tp = transform.position; tp.x = targetX; transform.position = tp; }
        }

        private void EndCastNormal()
        {
            SetAirSubType(AirSubState.None);
            if (_rb != null)
            {
                _rb.gravityScale = _originalGravityScale;
                _rb.constraints = _originalConstraints;
                _rb.velocity = Vector2.zero;
            }
            _isCasting = false;
        }

        private void EndCastDeath()
        {
            SetAirSubType(AirSubState.None);
            if (_rb != null)
            {
                _rb.gravityScale = 0f;
                _rb.constraints = RigidbodyConstraints2D.FreezeAll;
                _rb.velocity = Vector2.zero;
                _rb.angularVelocity = 0f;
            }
            _isCasting = false;
        }

        private bool Aborted()
        {
            if (boss != null && boss.IsDead)
            {
                EndCastDeath();
                return true;
            }
            return false;
        }

        private void EndCast()
        {
            EndCastNormal();
        }

        private void ResolvePlayer()
        {
            if (_player != null) return;
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) _player = p.transform;
        }

        private void OnDisable() { if (boss == null || !boss.IsDead) EndCastNormal(); else EndCastDeath(); }

#if UNITY_EDITOR
        [ContextMenu("Debug/Force AirLaser Once")]
        private void DebugForceAirLaserOnce()
        {
            if (!Application.isPlaying) return;
            ResolvePlayer();
            StartCoroutine(FireAirLaser());
        }

        [ContextMenu("Debug/Force AirLaserMode")]
        private void DebugForceAirLaserMode()
        {
            if (!Application.isPlaying) return;
            TryCast();
        }
#endif

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
                var go = new GameObject("AirLaserRuntime");
                go.transform.SetParent(null);
                lr = go.AddComponent<LineRenderer>();
                var mat = fallbackMat != null ? new Material(fallbackMat) : new Material(Shader.Find("Sprites/Default"));
                mat.color = color;
                lr.material = mat;
                lr.numCapVertices = 4;
                lr.numCornerVertices = 4;
                lr.sortingLayerName = "Default";
            }
            lr.positionCount = 2;
            lr.useWorldSpace = true;
            lr.startWidth = width;
            lr.endWidth = width;
            lr.startColor = color;
            lr.endColor = color;
            lr.sortingOrder = 200;
            lr.enabled = true;
            return lr;
        }

        private static void UpdateLine(LineRenderer lr, Vector2 a, Vector2 b)
        {
            if (lr == null) return;
            lr.SetPosition(0, a);
            lr.SetPosition(1, b);
            lr.enabled = true;
        }

        private static void DestroyLine(LineRenderer lr)
        {
            if (lr != null) Destroy(lr.gameObject);
        }

        private Vector2 GetAirLaserOrigin()
        {
            var pos = _rb != null ? _rb.position : (Vector2)transform.position;
            pos.y += 0.8f;
            return pos;
        }

        private IEnumerator FireAirLaser()
        {
            ResolvePlayer();
            if (_player == null) { Debug.LogWarning("[AirLaser] No player target, aborting"); yield break; }

            SetAirSubType(AirSubState.Laser);

            if (facing != null)
                facing.FaceTarget(_player);

            Vector2 origin = GetAirLaserOrigin();
            Vector2 target = _player.position;
            Vector2 dir = (target - origin).normalized;
            if (dir.sqrMagnitude < 0.001f) dir = Vector2.right;
            Vector2 end = origin + dir * laserRange;

            Debug.Log($"[AirLaser] Firing: origin={origin:F1}, target={target:F1}, dir={dir:F2}, range={laserRange}");

            // 1. Warning line
            LineRenderer warn = SpawnLine(warningLinePrefab, warningMaterial, warningWidth, warningColor);
            UpdateLine(warn, origin, end);
            Debug.Log("[AirLaser] Warning line shown");

            float wElapsed = 0f;
            while (wElapsed < laserWarningTime)
            {
                if (boss != null && boss.IsDead) { DestroyLine(warn); yield break; }
                wElapsed += Time.deltaTime;
                yield return null;
            }
            DestroyLine(warn);

            // 2. Beam
            LineRenderer beam = SpawnLine(beamLinePrefab, beamMaterial, beamWidth, beamColor);
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, laserRange, playerLayer);
            if (hit.collider != null)
            {
                end = hit.point;
                var hp = hit.collider.GetComponentInParent<Health>();
                if (hp != null && !hp.IsDead()) hp.TakeDamage(laserDamage);
                Debug.Log($"[AirLaser] Hit player at {end:F1}, damage={laserDamage}");
            }
            UpdateLine(beam, origin, end);
            Debug.Log("[AirLaser] Beam shown");

            float bElapsed = 0f;
            while (bElapsed < laserVisibleTime)
            {
                if (boss != null && boss.IsDead) { DestroyLine(beam); yield break; }
                bElapsed += Time.deltaTime;
                yield return null;
            }
            DestroyLine(beam);

            Debug.Log("[AirLaser] Cleanup done");
        }
    }
}
