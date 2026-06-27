using UnityEngine;
using Cardwin.Cards;

namespace Cardwin.Combat
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        public float speed = 4f;
        public float lifetime = 5f;
        public int damage = 10;

        [Header("Player bullet visuals (Stage 38, visual only)")]
        [SerializeField] private Sprite redSprite;   // Damage cards
        [SerializeField] private Sprite blueSprite;  // Block/Heal/Focus cards
        [SerializeField] private float bulletScale = 0.25f;

        private Vector2 _direction;
        private float _lifeTimer;
        private bool _usesCardEffect;

        private CardData _sourceCard;
        private CardEffectType _effectType;
        private PlayerCardContext _cardContext;

        // Stage 43: read-only accessors so a boss effect receiver can read the carried
        // card effect. Additive only — does not change firing or damage logic.
        public CardData SourceCard => _sourceCard;
        public CardEffectType EffectType => _effectType;
        public bool UsesCardEffect => _usesCardEffect;
        public PlayerCardContext CardContext => _cardContext;
        public int ResolveDamage() => ResolveGenericDamage();

        private void Awake()
        {
            EnsureVisibleDebugSprite();
        }

        private void EnsureVisibleDebugSprite()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null)
                sr = gameObject.AddComponent<SpriteRenderer>();

            // Fallback only if no art is assigned at all (red/blue set in Init).
            if (sr.sprite == null && redSprite == null && blueSprite == null)
                sr.sprite = CreateRuntimeSprite();

            sr.color = Color.white;
            sr.sortingOrder = 100;

            Vector3 pos = transform.position;
            pos.z = 0f;
            transform.position = pos;
        }

        // Stage 38: choose red/blue bullet art by card effect (Damage = red, else blue).
        // Visual only — does not touch damage, hit, card, magazine or part-break logic.
        private void ApplyBulletVisual(bool isRed)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null)
                sr = gameObject.AddComponent<SpriteRenderer>();

            Sprite chosen = isRed ? redSprite : blueSprite;
            if (chosen != null)
                sr.sprite = chosen;
            else if (sr.sprite == null)
                sr.sprite = CreateRuntimeSprite();

            sr.color = Color.white;
            sr.sortingOrder = 100;
            transform.localScale = Vector3.one * bulletScale;
        }

        private static Sprite CreateRuntimeSprite()
        {
            const int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            float center = size * 0.5f;
            float radius = size * 0.45f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center + 0.5f;
                    float dy = y - center + 0.5f;
                    pixels[y * size + x] = (dx * dx + dy * dy <= radius * radius)
                        ? Color.yellow
                        : Color.clear;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        public void Init(Vector2 direction, int damageAmount)
        {
            _direction = direction.normalized;
            damage = damageAmount;
            _lifeTimer = lifetime;
            _usesCardEffect = false;

            if (_direction != Vector2.zero)
                transform.right = _direction;

            ApplyBulletVisual(true);
            Debug.Log($"[Projectile] Init direction={_direction}, damage={damage}");
        }

        public void Init(Vector2 direction, CardData card, CardEffectType effectType, PlayerCardContext context)
        {
            _direction = direction.normalized;
            _lifeTimer = lifetime;
            _usesCardEffect = true;
            _sourceCard = card;
            _effectType = effectType;
            _cardContext = context;

            if (_direction != Vector2.zero)
                transform.right = _direction;

            ApplyBulletVisual(effectType == CardEffectType.Damage);
            Debug.Log($"[Projectile] Init card={card.cardName} effect={effectType} direction={_direction}");
        }

        private void Update()
        {
            _lifeTimer -= Time.deltaTime;
            if (_lifeTimer <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            transform.position += (Vector3)(_direction * speed * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            HandleHit(other);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.collider != null)
                HandleHit(collision.collider);
        }

        private void HandleHit(Collider2D other)
        {
            if (other.CompareTag("Player"))
                return;

            if (other.GetComponent<Projectile>() != null)
                return;

            string otherName = other.name.ToLower();
            if (otherName.Contains("bossdoor") ||
                otherName.Contains("spawnpoint") ||
                otherName.Contains("camerabounds"))
                return;

            if (LayerMask.LayerToName(other.gameObject.layer) == "Trigger")
                return;

            // Stage 43: full card-effect receiver (Mirror Angel boss only) takes priority so
            // Heal / Guard / Focus bullets also affect the boss. Normal enemies do NOT implement
            // IProjectileEffectReceiver, so they fall through to the unchanged paths below.
            IProjectileEffectReceiver effectReceiver = other.GetComponent<IProjectileEffectReceiver>();
            if (effectReceiver == null)
                effectReceiver = other.GetComponentInParent<IProjectileEffectReceiver>();
            if (effectReceiver != null)
            {
                effectReceiver.ReceiveProjectileEffect(this, transform.position);
                Destroy(gameObject);
                return;
            }

            // Stage 36: Boss parts have TOP hit priority so the body/root can never
            // steal a part hit. Then generic IDamageable, then the unchanged enemy Health path.
            // Normal enemies have neither a BossPart nor IDamageable, so they fall through.
            var bossPart = other.GetComponent<MirrorSaintessBossPack.MirrorSaintessBossPart>();
            if (bossPart == null)
                bossPart = other.GetComponentInParent<MirrorSaintessBossPack.MirrorSaintessBossPart>();

            IDamageable damageable = bossPart as IDamageable;
            if (damageable == null)
            {
                damageable = other.GetComponent<IDamageable>();
                if (damageable == null)
                    damageable = other.GetComponentInParent<IDamageable>();
            }

            if (damageable != null)
            {
                int genericDamage = ResolveGenericDamage();
                string partName = bossPart != null ? bossPart.PartId : "-";
                Debug.Log($"[ProjectileHit] other={other.name}, root={other.transform.root.name}, part={partName}, damageable={damageable.GetType().Name}, dmg={genericDamage}", other);
                if (genericDamage > 0)
                    damageable.TakeHit(genericDamage, gameObject);
                Destroy(gameObject);
                return;
            }

            Health health = other.GetComponent<Health>() ?? other.GetComponentInParent<Health>();
            if (health != null)
            {
                if (_usesCardEffect)
                {
                    Cardwin.Cards.CardEffectExecutor executor = FindObjectOfType<Cardwin.Cards.CardEffectExecutor>();
                    if (executor != null)
                        executor.ApplyEffectToTarget(_sourceCard, _effectType, other.gameObject, _cardContext);
                }
                else
                {
                    health.TakeDamage(damage);
                }

                Debug.Log($"[Projectile] Hit target={other.name} effect={(_usesCardEffect ? _effectType.ToString() : "Damage")}");
                Destroy(gameObject);
                return;
            }

            Debug.Log($"[Projectile] Hit object but no Health: {other.name} (layer={LayerMask.LayerToName(other.gameObject.layer)})");

            if (LayerMask.LayerToName(other.gameObject.layer) == "Ground")
            {
                Debug.Log($"[Projectile] Hit ground: {other.name}");
                Destroy(gameObject);
            }
        }

        private int ResolveGenericDamage()
        {
            if (!_usesCardEffect)
                return damage;
            if (_sourceCard == null)
                return damage;
            if (_effectType == CardEffectType.Damage)
            {
                float mult = _cardContext != null ? _cardContext.ConsumeFocusMultiplier() : 1f;
                return Mathf.Max(1, Mathf.RoundToInt(_sourceCard.damage * mult));
            }
            // Non-damage card effects (Block/Heal/Focus) deal no boss damage.
            return 0;
        }
    }
}
