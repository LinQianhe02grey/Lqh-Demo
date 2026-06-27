using System.Collections.Generic;
using UnityEngine;
using Cardwin.Cards;
using Cardwin.Combat;

namespace Cardwin.Lua
{
    /// <summary>
    /// Generic Unity host for every Lua-defined bullet. Lua (or the C# behaviour
    /// bridge while no Lua VM is integrated) only supplies behaviour + config; this
    /// component owns the GameObject, components and lifecycle:
    ///   OnSpawn  -> behaviour.OnSpawn
    ///   Update   -> behaviour.OnUpdate (+ lifetime countdown)
    ///   Trigger  -> behaviour.OnHit
    ///   Recycle  -> behaviour.OnRecycle + Destroy
    ///
    /// Self-builds SpriteRenderer + Kinematic Rigidbody2D + trigger CircleCollider2D
    /// at runtime, so no prefab asset is required (Instantiate/Destroy for now; an
    /// object pool can be added later). Runtime-safe, packable.
    /// </summary>
    [DisallowMultipleComponent]
    public class LuaBulletHost : MonoBehaviour
    {
        public LuaBulletDefinition Definition { get; private set; }
        public Vector2 Direction { get; set; }
        public PlayerCardContext Context { get; private set; }

        public int RemainingPierce { get; set; }
        public GameObject CurrentTarget { get; set; }
        public bool IsRecycled { get; private set; }

        private ILuaBulletBehavior _behavior;
        private float _lifeTimer;
        private readonly HashSet<GameObject> _alreadyHit = new HashSet<GameObject>();

        /// <summary>Spawn helper: builds a host GameObject and starts the bullet.</summary>
        public static LuaBulletHost Spawn(LuaBulletDefinition def, Vector3 position,
            Vector2 direction, PlayerCardContext context)
        {
            if (def == null)
            {
                Debug.LogError("[LuaBullet] Spawn called with null definition.");
                return null;
            }

            var go = new GameObject($"LuaBulletHost_{def.Id}");
            go.transform.position = new Vector3(position.x, position.y, 0f);
            var host = go.AddComponent<LuaBulletHost>();
            host.Setup(def, direction, context);
            return host;
        }

        public void Setup(LuaBulletDefinition def, Vector2 direction, PlayerCardContext context)
        {
            Definition = def;
            Context = context;
            Direction = direction == Vector2.zero ? Vector2.right : direction.normalized;
            RemainingPierce = Mathf.Max(0, def.PierceCount);
            _lifeTimer = def.LifeTime > 0f ? def.LifeTime : 4f;
            IsRecycled = false;

            BuildVisualAndPhysics();

            if (Direction != Vector2.zero)
                transform.right = Direction;

            _behavior = LuaBulletBehaviorRegistry.Resolve(def.Behavior);
            if (_behavior == null)
            {
                Debug.LogWarning($"[LuaBullet] No behaviour for '{def.Behavior}' (id={def.Id}); bullet will fly straight.");
                _behavior = LuaBulletBehaviorRegistry.Fallback;
            }

            Debug.Log($"[LuaBullet] Spawn id={def.Id} behavior={def.Behavior} dir={Direction} dmg={def.Damage}/{def.DamageMode}");
            _behavior.OnSpawn(this);
        }

        private void BuildVisualAndPhysics()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            if (sr.sprite == null) sr.sprite = LuaBulletVisual.GetCircleSprite(RarityColor(Definition.Rarity));
            sr.color = Color.white;
            sr.sortingOrder = 100;

            float scale = Definition.VisualScale > 0f ? Definition.VisualScale : 1f;
            transform.localScale = Vector3.one * 0.25f * scale;

            var rb = GetComponent<Rigidbody2D>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            var col = GetComponent<CircleCollider2D>();
            if (col == null) col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = Definition.HitRadius > 0f ? Definition.HitRadius : 0.35f;
        }

        private void Update()
        {
            if (IsRecycled)
                return;

            _lifeTimer -= Time.deltaTime;
            if (_lifeTimer <= 0f)
            {
                Recycle();
                return;
            }

            _behavior?.OnUpdate(this, Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsRecycled || other == null)
                return;

            if (other.CompareTag("Player"))
                return;
            if (other.GetComponent<LuaBulletHost>() != null)
                return;
            if (other.GetComponent<Projectile>() != null)
                return;
            if (LayerMask.LayerToName(other.gameObject.layer) == "Trigger")
                return;

            string n = other.name.ToLower();
            if (n.Contains("bossdoor") || n.Contains("spawnpoint") || n.Contains("camerabounds"))
                return;

            if (LayerMask.LayerToName(other.gameObject.layer) == "Ground")
            {
                Recycle();
                return;
            }

            GameObject hitOwner = LuaBattleAPI.ResolveDamageableOwner(other.gameObject);
            if (hitOwner == null)
                return; // not a combat target

            if (!_alreadyHit.Add(hitOwner))
                return; // already hit this target

            _behavior?.OnHit(this, hitOwner);
        }

        public void Recycle()
        {
            if (IsRecycled)
                return;
            IsRecycled = true;
            _behavior?.OnRecycle(this);
            Destroy(gameObject);
        }

        private static Color RarityColor(string rarity)
        {
            switch (rarity)
            {
                case "Epic": return new Color(0.65f, 0.35f, 1f);
                case "Rare": return new Color(0.3f, 0.6f, 1f);
                case "Legendary": return new Color(1f, 0.6f, 0.1f);
                default: return new Color(0.9f, 0.9f, 0.9f);
            }
        }
    }

    /// <summary>Runtime sprite cache so each bullet doesn't allocate a new texture.</summary>
    internal static class LuaBulletVisual
    {
        private static readonly Dictionary<Color, Sprite> _cache = new Dictionary<Color, Sprite>();

        public static Sprite GetCircleSprite(Color color)
        {
            if (_cache.TryGetValue(color, out Sprite cached) && cached != null)
                return cached;

            const int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            float center = size * 0.5f;
            float radius = size * 0.45f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center + 0.5f;
                    float dy = y - center + 0.5f;
                    pixels[y * size + x] = (dx * dx + dy * dy <= radius * radius) ? color : Color.clear;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            _cache[color] = sprite;
            return sprite;
        }
    }
}
