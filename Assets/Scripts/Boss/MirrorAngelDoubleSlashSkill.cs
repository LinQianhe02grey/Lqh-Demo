using System.Collections;
using UnityEngine;
using MirrorSaintessBossPack;
using Cardwin.Combat;

namespace Cardwin.Boss
{
    public sealed class MirrorAngelDoubleSlashSkill : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private MirrorSaintessBoss boss;
        [SerializeField] private MirrorAngelBossGravityMover mover;
        [SerializeField] private MirrorAngelFacingController facing;
        [SerializeField] private MirrorAngelBossAnimatorBridge animBridge;

        [Header("Animator")]
        [SerializeField] private string stateName = "Attack2_DoubleSlash";
        [SerializeField] private string attackTypeParam = "AttackType";
        [SerializeField] private int attackTypeValue = 3;

        [Header("Timing")]
        [SerializeField] private float slash1HitTime = 0.30f;
        [SerializeField] private float slash2HitTime = 0.55f;
        [SerializeField] private float totalDuration = 0.90f;
        [SerializeField] private float cooldown = 3f;

        [Header("Hitbox")]
        [SerializeField] private float slashRangeX = 2.5f;
        [SerializeField] private float slashRangeY = 1.8f;
        [SerializeField] private float slashOffsetX = 1.4f;
        [SerializeField] private float slashOffsetY = 1.0f;

        [Header("Damage")]
        [SerializeField] private int slashDamage = 12;
        [SerializeField] private LayerMask playerLayer;

        private Transform _player;
        private bool _isCasting;
        private int _castFacingSign;
        private bool _slash1Hit;
        private bool _slash2Hit;

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
            _slash1Hit = false;
            _slash2Hit = false;
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
            while (elapsed < totalDuration)
            {
                if (Aborted()) yield break;
                elapsed += Time.deltaTime;

                if (!_slash1Hit && elapsed >= slash1HitTime)
                {
                    _slash1Hit = true;
                    TryDealDamage(slashDamage);
                }
                if (!_slash2Hit && elapsed >= slash2HitTime)
                {
                    _slash2Hit = true;
                    TryDealDamage(slashDamage);
                }
                yield return null;
            }

            EndCast();
        }

        private void TryDealDamage(int dmg)
        {
            Vector3 origin = transform.position;
            float cx = origin.x + _castFacingSign * slashOffsetX;
            float cy = origin.y + slashOffsetY;
            Vector2 center = new Vector2(cx, cy);
            Vector2 size = new Vector2(slashRangeX, slashRangeY);

            Collider2D hit = Physics2D.OverlapBox(center, size, 0f, playerLayer);
            if (hit != null)
            {
                ResolvePlayer();
                if (_player != null)
                {
                    var hp = _player.GetComponentInParent<Health>();
                    if (hp != null && !hp.IsDead())
                        hp.TakeDamage(dmg);
                }
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
        [ContextMenu("Debug/Play Double Slash")]
        private void DebugPlay() { if (Application.isPlaying) TryCast(); }

        private void OnDrawGizmosSelected()
        {
            int sign = 1;
            if (facing != null && Application.isPlaying) sign = facing.CurrentFacingSign;
            Vector3 o = transform.position;
            Gizmos.color = new Color(1f, 0.4f, 0.2f, 0.4f);
            Gizmos.DrawCube(new Vector3(o.x + sign * slashOffsetX, o.y + slashOffsetY, 0f), new Vector3(slashRangeX, slashRangeY, 0f));
            Gizmos.color = new Color(1f, 0.4f, 0.2f, 0.8f);
            Gizmos.DrawWireCube(new Vector3(o.x + sign * slashOffsetX, o.y + slashOffsetY, 0f), new Vector3(slashRangeX, slashRangeY, 0f));
        }
#endif
    }
}
