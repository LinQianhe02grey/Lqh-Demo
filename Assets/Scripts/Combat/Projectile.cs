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

        private Vector2 _direction;
        private float _lifeTimer;
        private bool _usesCardEffect;

        private CardData _sourceCard;
        private CardEffectType _effectType;
        private PlayerCardContext _cardContext;

        private void Awake()
        {
            EnsureVisibleDebugSprite();
        }

        private void EnsureVisibleDebugSprite()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null)
                sr = gameObject.AddComponent<SpriteRenderer>();

            if (sr.sprite == null)
                sr.sprite = CreateRuntimeSprite();

            sr.color = Color.yellow;
            sr.sortingOrder = 100;

            Vector3 pos = transform.position;
            pos.z = 0f;
            transform.position = pos;

            transform.localScale = new Vector3(0.8f, 0.8f, 1f);

            Debug.Log($"[ProjectileVisual] SpriteRenderer ready. spriteNull={sr.sprite == null}, scale={transform.localScale}, sorting={sr.sortingOrder}");
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
    }
}
