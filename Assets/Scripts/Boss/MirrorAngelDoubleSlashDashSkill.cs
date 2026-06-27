using System.Collections;
using UnityEngine;
using MirrorSaintessBossPack;
using Cardwin.Combat;

namespace Cardwin.Boss
{
    public sealed class MirrorAngelDoubleSlashDashSkill : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private MirrorSaintessBoss boss;
        [SerializeField] private MirrorAngelBossGravityMover mover;
        [SerializeField] private MirrorAngelFacingController facing;
        [SerializeField] private MirrorAngelBossAnimatorBridge animBridge;

        [Header("Animator")]
        [SerializeField] private string stateName = "Attack2_DoubleSlashDash";
        [SerializeField] private string attackTypeParam = "AttackType";
        [SerializeField] private int attackTypeValue = 4;

        [Header("Timing")]
        [SerializeField] private float slash1HitTime = 0.30f;
        [SerializeField] private float slash2HitTime = 0.55f;
        [SerializeField] private float dashStartTime = 0.60f;
        [SerializeField] private float dashHitTime = 0.85f;
        [SerializeField] private float dashEndTime = 1.00f;
        [SerializeField] private float totalDuration = 1.40f;
        [SerializeField] private float cooldown = 5f;

        [Header("Slash Hitbox")]
        [SerializeField] private float slashRangeX = 2.5f;
        [SerializeField] private float slashRangeY = 1.8f;
        [SerializeField] private float slashOffsetX = 1.4f;
        [SerializeField] private float slashOffsetY = 1.0f;
        [SerializeField] private int slashDamage = 12;

        [Header("Dash")]
        [SerializeField] private float dashDistance = 3.2f;
        [SerializeField] private float dashDuration = 0.25f;
        [SerializeField] private AnimationCurve dashCurve;

        [Header("Dash Hitbox")]
        [SerializeField] private float dashHitboxWidth = 3.5f;
        [SerializeField] private float dashHitboxHeight = 2.0f;
        [SerializeField] private float dashHitboxOffsetX = 2.0f;
        [SerializeField] private float dashHitboxOffsetY = 1.0f;
        [SerializeField] private int dashDamage = 20;

        [Header("Damage")]
        [SerializeField] private LayerMask playerLayer;

        private Transform _player;
        private bool _isCasting;
        private int _castFacingSign;
        private bool _slash1Hit, _slash2Hit, _dashHit;

        public bool IsCasting => _isCasting;

        private void Awake()
        {
            if (boss == null) boss = GetComponent<MirrorSaintessBoss>();
            if (mover == null) mover = GetComponent<MirrorAngelBossGravityMover>();
            if (facing == null) facing = GetComponent<MirrorAngelFacingController>();
            if (animBridge == null) animBridge = GetComponent<MirrorAngelBossAnimatorBridge>();
            if (playerLayer.value == 0) { int l = LayerMask.NameToLayer("Player"); playerLayer = l >= 0 ? (1 << l) : playerLayer; }
        }

        public bool TryCast()
        {
            if (_isCasting || boss == null || boss.IsDead) return false;
            ResolvePlayer();
            _isCasting = true;
            _slash1Hit = false; _slash2Hit = false; _dashHit = false;
            StartCoroutine(CastRoutine());
            return true;
        }

        private IEnumerator CastRoutine()
        {
            _castFacingSign = facing != null ? facing.CurrentFacingSign : 1;
            if (facing != null) facing.LockFacing(_castFacingSign);
            if (animBridge != null && animBridge.Animator != null)
            {
                animBridge.Animator.SetInteger(attackTypeParam, attackTypeValue);
                animBridge.Animator.Play(stateName, 0, 0f);
            }

            float elapsed = 0f;
            while (elapsed < dashStartTime)
            {
                if (Aborted()) yield break;
                elapsed += Time.deltaTime;

                if (!_slash1Hit && elapsed >= slash1HitTime) { _slash1Hit = true; DealSlashHit(); }
                if (!_slash2Hit && elapsed >= slash2HitTime) { _slash2Hit = true; DealSlashHit(); }
                yield return null;
            }

            if (Aborted()) yield break;

            var rb = mover != null ? mover.Rigidbody : null;
            float startX = rb != null ? rb.position.x : transform.position.x;
            float targetX = startX + _castFacingSign * dashDistance;
            float dashElapsed = 0f;

            while (dashElapsed < dashDuration)
            {
                if (Aborted()) yield break;
                dashElapsed += Time.fixedDeltaTime;

                float t = Mathf.Clamp01(dashElapsed / dashDuration);
                float eased = dashCurve != null && dashCurve.keys.Length > 0
                    ? dashCurve.Evaluate(t) : t * t * (3f - 2f * t);

                if (rb != null)
                {
                    Vector2 pos = rb.position;
                    pos.x = Mathf.Lerp(startX, targetX, eased);
                    rb.MovePosition(pos);
                }

                if (!_dashHit)
                {
                    _dashHit = true;
                    DealDashHit();
                }

                yield return new WaitForFixedUpdate();
            }

            if (rb != null)
            {
                Vector2 finalPos = rb.position;
                finalPos.x = targetX;
                rb.MovePosition(finalPos);
            }

            if (mover != null)
                mover.ClearExternalVelocity();

            if (Aborted()) yield break;

            float remaining = totalDuration - (dashStartTime + dashDuration);
            if (remaining > 0f)
            {
                float recElapsed = 0f;
                while (recElapsed < remaining)
                {
                    if (Aborted()) yield break;
                    recElapsed += Time.deltaTime;
                    yield return null;
                }
            }

            EndCast();
        }

        private void DealSlashHit()
        {
            Vector3 origin = transform.position;
            float cx = origin.x + _castFacingSign * slashOffsetX;
            float cy = origin.y + slashOffsetY;
            Collider2D hit = Physics2D.OverlapBox(new Vector2(cx, cy), new Vector2(slashRangeX, slashRangeY), 0f, playerLayer);
            if (hit != null) ApplyDamage(slashDamage);
        }

        private void DealDashHit()
        {
            Vector3 origin = transform.position;
            float cx = origin.x + _castFacingSign * dashHitboxOffsetX;
            float cy = origin.y + dashHitboxOffsetY;
            Collider2D hit = Physics2D.OverlapBox(new Vector2(cx, cy), new Vector2(dashHitboxWidth, dashHitboxHeight), 0f, playerLayer);
            if (hit != null) ApplyDamage(dashDamage);
        }

        private void ApplyDamage(int dmg)
        {
            ResolvePlayer();
            if (_player != null)
            {
                var hp = _player.GetComponentInParent<Health>();
                if (hp != null && !hp.IsDead()) hp.TakeDamage(dmg);
            }
        }

        private bool Aborted()
        {
            if (boss != null && boss.IsDead) { EndCast(); return true; }
            return false;
        }

        private void EndCast()
        {
            if (facing != null) facing.UnlockFacing();
            _isCasting = false;
        }

        private void ResolvePlayer()
        {
            if (_player != null) return;
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) _player = p.transform;
        }

        private void OnDisable() { EndCast(); }

#if UNITY_EDITOR
        [ContextMenu("Debug/Play Double Slash Dash")]
        private void DebugPlay() { if (Application.isPlaying) TryCast(); }

        private void OnDrawGizmosSelected()
        {
            int sign = 1;
            if (facing != null && Application.isPlaying) sign = facing.CurrentFacingSign;
            Vector3 o = transform.position;
            // Slash box
            Gizmos.color = new Color(1f, 0.4f, 0.2f, 0.4f);
            Gizmos.DrawCube(new Vector3(o.x + sign * slashOffsetX, o.y + slashOffsetY, 0f), new Vector3(slashRangeX, slashRangeY, 0f));
            Gizmos.color = new Color(1f, 0.4f, 0.2f, 0.8f);
            Gizmos.DrawWireCube(new Vector3(o.x + sign * slashOffsetX, o.y + slashOffsetY, 0f), new Vector3(slashRangeX, slashRangeY, 0f));
            // Dash hitbox at current position
            Gizmos.color = new Color(0.8f, 0.2f, 0.2f, 0.35f);
            Gizmos.DrawCube(new Vector3(o.x + sign * dashHitboxOffsetX, o.y + dashHitboxOffsetY, 0f), new Vector3(dashHitboxWidth, dashHitboxHeight, 0f));
            Gizmos.color = new Color(0.8f, 0.2f, 0.2f, 0.8f);
            Gizmos.DrawWireCube(new Vector3(o.x + sign * dashHitboxOffsetX, o.y + dashHitboxOffsetY, 0f), new Vector3(dashHitboxWidth, dashHitboxHeight, 0f));
            // Dash start
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(o, 0.2f);
            // Dash endpoint
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(new Vector3(o.x + sign * dashDistance, o.y, 0f), 0.3f);
            Gizmos.DrawLine(o, new Vector3(o.x + sign * dashDistance, o.y, 0f));
        }
#endif
    }
}
