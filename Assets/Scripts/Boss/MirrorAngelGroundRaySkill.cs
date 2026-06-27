using System.Collections;
using UnityEngine;
using MirrorSaintessBossPack;
using Cardwin.Combat;

namespace Cardwin.Boss
{
    public sealed class MirrorAngelGroundRaySkill : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private MirrorSaintessBoss boss;
        [SerializeField] private MirrorAngelBossGravityMover mover;
        [SerializeField] private MirrorAngelFacingController facing;
        [SerializeField] private MirrorAngelBossAnimatorBridge animBridge;

        [Header("Animator")]
        [SerializeField] private string groundRayStateName = "Attack1_GroundRay";
        [SerializeField] private string attackTypeParam = "AttackType";
        [SerializeField] private int groundRayAttackTypeValue = 2;

        [Header("Timing")]
        [SerializeField] private float windupTime = 0.9f;
        [SerializeField] private float activeTime = 0.8f;
        [SerializeField] private float recoveryTime = 0.5f;
        [SerializeField] private float cooldown = 8f;

        [Header("Attack Area")]
        [SerializeField] private float attackLengthX = 100f;
        [SerializeField] private float attackHeight = 8f;
        [SerializeField] private float attackYOffset = 0f;

        [Header("Damage")]
        [SerializeField] private int damage = 18;
        [SerializeField] private LayerMask playerLayer;

        [Header("FX")]
        [SerializeField] private GameObject activeFxPrefab;
        [SerializeField] private Transform visualFxRoot;
        [SerializeField] private bool spawnFxAsChild = true;
        [SerializeField] private Color fxColor = new Color(0.7f, 0.5f, 1f, 0.35f);

        private Transform _player;
        private bool _isCasting;
        private bool _hasHitPlayerThisCast;
        private int _castFacingSign;
        private GameObject _spawnedFx;

        public bool IsCasting => _isCasting;

        private void Awake()
        {
            if (boss == null) boss = GetComponent<MirrorSaintessBoss>();
            if (mover == null) mover = GetComponent<MirrorAngelBossGravityMover>();
            if (facing == null) facing = GetComponent<MirrorAngelFacingController>();
            if (animBridge == null) animBridge = GetComponent<MirrorAngelBossAnimatorBridge>();
            if (playerLayer.value == 0)
            {
                int layer = LayerMask.NameToLayer("Player");
                playerLayer = layer >= 0 ? (1 << layer) : playerLayer;
            }
        }

        public bool TryCast()
        {
            if (_isCasting)
                return false;
            if (boss == null || boss.IsDead)
                return false;

            ResolvePlayer();

            _isCasting = true;
            _hasHitPlayerThisCast = false;
            StartCoroutine(CastRoutine());
            return true;
        }

        private IEnumerator CastRoutine()
        {
            _castFacingSign = facing != null ? facing.CurrentFacingSign : 1;

            if (facing != null)
                facing.LockFacing(_castFacingSign);

            if (animBridge != null && animBridge.Animator != null)
            {
                animBridge.Animator.SetInteger(attackTypeParam, groundRayAttackTypeValue);
                animBridge.Animator.Play(groundRayStateName, 0, 0f);
            }

            yield return new WaitForSeconds(windupTime);

            if (Aborted()) yield break;

            SpawnActiveFx();
            DealDamageOnce();

            float elapsed = 0f;
            while (elapsed < activeTime)
            {
                if (Aborted())
                {
                    DespawnFx();
                    yield break;
                }
                elapsed += Time.deltaTime;
                yield return null;
            }

            DespawnFx();

            if (Aborted()) yield break;

            float rec = recoveryTime;
            while (rec > 0f)
            {
                if (Aborted()) yield break;
                rec -= Time.deltaTime;
                yield return null;
            }

            EndCast();
        }

        private void DealDamageOnce()
        {
            if (_hasHitPlayerThisCast)
                return;
            _hasHitPlayerThisCast = true;

            Vector3 origin = transform.position;
            float centerX = origin.x + _castFacingSign * attackLengthX * 0.5f;
            float centerY = origin.y + attackYOffset;
            Vector2 center = new Vector2(centerX, centerY);
            Vector2 size = new Vector2(attackLengthX, attackHeight);

            Collider2D hit = Physics2D.OverlapBox(center, size, 0f, playerLayer);
            if (hit != null)
            {
                ResolvePlayer();
                if (_player != null)
                {
                    var hp = _player.GetComponentInParent<Health>();
                    if (hp != null && !hp.IsDead())
                    {
                        hp.TakeDamage(damage);
                        Debug.Log($"[MirrorAngelGroundRay] Hit player for {damage} damage. Facing={_castFacingSign}");
                    }
                }
            }
        }

        private void SpawnActiveFx()
        {
            if (activeFxPrefab != null)
            {
                Transform parent = spawnFxAsChild ? (visualFxRoot != null ? visualFxRoot : transform) : null;
                _spawnedFx = Instantiate(activeFxPrefab, transform.position, Quaternion.identity, parent);
                _spawnedFx.SetActive(true);
                PositionFx();
            }
            else
            {
                _spawnedFx = CreateRuntimeFx();
                PositionFx();
            }
        }

        private void PositionFx()
        {
            if (_spawnedFx == null)
                return;

            float centerX = transform.position.x + _castFacingSign * attackLengthX * 0.5f;
            float centerY = transform.position.y + attackYOffset;
            _spawnedFx.transform.position = new Vector3(centerX, centerY, 0f);
            _spawnedFx.transform.localScale = new Vector3(attackLengthX, attackHeight, 1f);
        }

        private GameObject CreateRuntimeFx()
        {
            var go = new GameObject("GroundRayAreaFX_Runtime");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateWhiteSquareSprite();
            sr.color = fxColor;
            sr.sortingOrder = 60;
            sr.drawMode = SpriteDrawMode.Sliced;
            var parent = spawnFxAsChild ? (visualFxRoot != null ? visualFxRoot : transform) : null;
            if (parent != null)
                go.transform.SetParent(parent);
            return go;
        }

        private static Sprite CreateWhiteSquareSprite()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            var colors = new Color[16];
            for (int i = 0; i < 16; i++)
                colors[i] = Color.white;
            tex.SetPixels(colors);
            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;
            var rect = new Rect(0f, 0f, 4f, 4f);
            var pivot = new Vector2(0.5f, 0.5f);
            return Sprite.Create(tex, rect, pivot, 4f, 0u, SpriteMeshType.FullRect, Vector4.zero);
        }

        private void DespawnFx()
        {
            if (_spawnedFx != null)
            {
                Destroy(_spawnedFx);
                _spawnedFx = null;
            }
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
            DespawnFx();
            if (animBridge != null && animBridge.Animator != null)
                animBridge.Animator.SetInteger(attackTypeParam, 0);
            if (facing != null)
                facing.UnlockFacing();
            _isCasting = false;
        }

        private void ResolvePlayer()
        {
            if (_player != null)
                return;
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                _player = p.transform;
        }

        private void OnDisable()
        {
            EndCast();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            int facingSign = 1;
            if (facing != null)
                facingSign = facing.CurrentFacingSign;
            else if (Application.isPlaying)
                return;

            Vector3 origin = transform.position;
            float cx = origin.x + facingSign * attackLengthX * 0.5f;
            float cy = origin.y + attackYOffset;
            Vector3 center = new Vector3(cx, cy, 0f);
            Vector3 size = new Vector3(attackLengthX, attackHeight, 0f);

            Gizmos.color = new Color(0.7f, 0.3f, 1f, 0.4f);
            Gizmos.DrawCube(center, size);
            Gizmos.color = new Color(0.7f, 0.3f, 1f, 0.8f);
            Gizmos.DrawWireCube(center, size);

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(origin, 0.3f);
        }
#endif
    }
}
