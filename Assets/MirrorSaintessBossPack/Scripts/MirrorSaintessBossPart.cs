using System.Collections;
using UnityEngine;
using Cardwin.Combat;

namespace MirrorSaintessBossPack
{
    public enum MirrorSaintessPartType
    {
        ChestCore,
        BlueGun,
        RedGun
    }

    /// <summary>
    /// Destructible boss part. Implements <see cref="IDamageable"/> so the existing player
    /// projectile hits it the same way it hits normal enemies. Each hit forwards damage to
    /// the boss total HP (always, even after break, so the boss is always killable) and
    /// reduces this part's own HP until it breaks once.
    /// </summary>
    public sealed class MirrorSaintessBossPart : MonoBehaviour, IDamageable
    {
        [Header("Identity")]
        [SerializeField] private string partId = "Part";
        [SerializeField] private MirrorSaintessPartType partType;

        [Header("Health")]
        [SerializeField] private int maxHp = 80;
        [SerializeField] private int currentHp;
        [SerializeField] private bool isBroken;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer visualRenderer;
        [SerializeField] private Sprite intactSprite;
        [SerializeField] private Sprite brokenSprite;

        [Header("Collider")]
        [SerializeField] private Collider2D hitCollider;
        [Tooltip("V1/V2: keep false so the part stays hittable after breaking and post-break hits still damage boss total HP.")]
        [SerializeField] private bool disableColliderWhenBroken = false;

        [Header("Hit Feedback")]
        [SerializeField] private Color hitFlashColor = Color.white;
        [SerializeField] private Color breakFlashColor = new Color(1f, 0.35f, 0.35f, 1f);
        [SerializeField] private float flashDuration = 0.08f;
        [SerializeField] private float breakShakeMagnitude = 0.12f;
        [SerializeField] private float breakShakeDuration = 0.2f;

        [Header("Debug Hitbox")]
        [Tooltip("Draw a translucent hitbox quad in the Game View at runtime.")]
        [SerializeField] private bool showRuntimeHitbox = false;

        private MirrorSaintessBoss _owner;
        private Color _baseColor = Color.white;
        private Coroutine _flashRoutine;
        private Coroutine _shakeRoutine;
        private Vector3 _visualBasePos;
        private SpriteRenderer _runtimeHitbox;

        public string PartId => partId;
        public MirrorSaintessPartType PartType => partType;
        public bool IsBroken => isBroken;
        public int CurrentHp => currentHp;
        public int MaxHp => maxHp;

        private void Awake()
        {
            if (visualRenderer == null)
                visualRenderer = GetComponentInChildren<SpriteRenderer>();
            if (hitCollider == null)
                hitCollider = GetComponent<Collider2D>();
            if (visualRenderer != null)
            {
                _baseColor = visualRenderer.color;
                _visualBasePos = visualRenderer.transform.localPosition;
            }

            currentHp = maxHp;
            isBroken = false;
            ApplyIntactVisual();

            if (showRuntimeHitbox)
                EnsureRuntimeHitbox();
        }

        public void Initialize(MirrorSaintessBoss owner)
        {
            _owner = owner;
        }

        // ---- IDamageable (player projectile entry point) ----
        public void TakeHit(int amount, GameObject source)
        {
            if (amount <= 0)
                return;

            EnsureOwner();

            // Always feed the boss total HP so the boss can always be damaged/killed.
            if (_owner != null)
                _owner.DealBossDamageFromPart(amount, this);

            if (!isBroken)
            {
                currentHp = Mathf.Max(0, currentHp - amount);
                FlashOnce(hitFlashColor);
                Debug.Log($"[BossPart] {partId} hit -{amount} -> hp {currentHp}/{maxHp}");
                if (currentHp <= 0)
                    BreakPart();
            }
            else
            {
                FlashOnce(hitFlashColor);
            }
        }

        public void TakeDamage(float damage)
        {
            if (damage <= 0f)
                return;
            TakeHit(Mathf.Max(1, Mathf.RoundToInt(damage)), null);
        }

        public void ResetPart()
        {
            isBroken = false;
            currentHp = maxHp;
            if (hitCollider != null)
                hitCollider.enabled = true;
            ApplyIntactVisual();
            if (visualRenderer != null)
            {
                visualRenderer.color = _baseColor;
                visualRenderer.transform.localPosition = _visualBasePos;
            }
            UpdateRuntimeHitboxColor();
        }

        private void BreakPart()
        {
            if (isBroken)
                return;

            isBroken = true;

            if (visualRenderer != null && brokenSprite != null)
                visualRenderer.sprite = brokenSprite;

            if (hitCollider != null && disableColliderWhenBroken)
                hitCollider.enabled = false;

            FlashOnce(breakFlashColor);
            StartShake();
            UpdateRuntimeHitboxColor();

            EnsureOwner();
            if (_owner != null)
                _owner.NotifyPartBroken(this);

            Debug.Log($"[BossPart] {partId} broken.");
        }

        private void EnsureOwner()
        {
            if (_owner == null)
                _owner = GetComponentInParent<MirrorSaintessBoss>();
        }

        private void ApplyIntactVisual()
        {
            if (visualRenderer != null && intactSprite != null)
                visualRenderer.sprite = intactSprite;
        }

        private void FlashOnce(Color color)
        {
            if (visualRenderer == null || !isActiveAndEnabled)
                return;
            if (_flashRoutine != null)
                StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(FlashRoutine(color));
        }

        private IEnumerator FlashRoutine(Color color)
        {
            visualRenderer.color = color;
            yield return new WaitForSeconds(flashDuration);
            if (visualRenderer != null)
                visualRenderer.color = _baseColor;
            _flashRoutine = null;
        }

        private void StartShake()
        {
            if (visualRenderer == null || !isActiveAndEnabled)
                return;
            if (_shakeRoutine != null)
                StopCoroutine(_shakeRoutine);
            _shakeRoutine = StartCoroutine(ShakeRoutine());
        }

        private IEnumerator ShakeRoutine()
        {
            float t = 0f;
            Transform vt = visualRenderer.transform;
            while (t < breakShakeDuration)
            {
                t += Time.deltaTime;
                Vector2 off = Random.insideUnitCircle * breakShakeMagnitude;
                vt.localPosition = _visualBasePos + (Vector3)off;
                yield return null;
            }
            vt.localPosition = _visualBasePos;
            _shakeRoutine = null;
        }

        private void EnsureRuntimeHitbox()
        {
            if (_runtimeHitbox != null || hitCollider == null)
                return;
            GameObject go = new GameObject("RuntimeHitbox");
            go.transform.SetParent(transform, false);
            _runtimeHitbox = go.AddComponent<SpriteRenderer>();
            _runtimeHitbox.sprite = BuildWhiteSprite();
            _runtimeHitbox.sortingOrder = 500;
            // Match the collider's local AABB.
            Bounds lb = hitCollider is BoxCollider2D box
                ? new Bounds(box.offset, box.size)
                : new Bounds(Vector2.zero, Vector2.one);
            go.transform.localPosition = lb.center;
            go.transform.localScale = lb.size;
            UpdateRuntimeHitboxColor();
        }

        private void UpdateRuntimeHitboxColor()
        {
            if (_runtimeHitbox == null)
                return;
            Color c = isBroken ? new Color(0.5f, 0.5f, 0.5f, 0.25f) : new Color(1f, 0f, 1f, 0.25f);
            _runtimeHitbox.color = c;
        }

        private static Sprite _whiteSprite;
        private static Sprite BuildWhiteSprite()
        {
            if (_whiteSprite != null)
                return _whiteSprite;
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            Color[] px = { Color.white, Color.white, Color.white, Color.white };
            tex.SetPixels(px);
            tex.Apply();
            _whiteSprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);
            return _whiteSprite;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Collider2D col = hitCollider != null ? hitCollider : GetComponent<Collider2D>();
            if (col == null)
                return;
            Gizmos.color = isBroken ? new Color(0.5f, 0.5f, 0.5f, 0.9f) : new Color(1f, 0f, 1f, 0.9f);
            Bounds b = col.bounds;
            Gizmos.DrawWireCube(b.center, b.size);
        }

        [ContextMenu("Debug/Damage Part 25")]
        private void DebugDamagePart25() => TakeHit(25, null);

        [ContextMenu("Debug/Break Part")]
        private void DebugBreakPart() => TakeHit(Mathf.Max(1, currentHp), null);

        [ContextMenu("Debug/Reset Part")]
        private void DebugResetPart() => ResetPart();
#endif
    }
}
