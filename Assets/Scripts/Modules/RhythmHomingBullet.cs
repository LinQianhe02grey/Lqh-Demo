using UnityEngine;
using UnityEngine.SceneManagement;
using Cardwin.Combat;
using Cardwin.Boss;
using MirrorSaintessBossPack;

namespace Cardwin.Modules
{
    /// <summary>
    /// World-space homing projectile spawned when the player correctly hits a RED
    /// rhythm note. It prefers the nearest NORMAL enemy in the current scene (never
    /// the Player); if there is no normal enemy AND the active scene is the BossRoom
    /// it homes onto the Boss instead. On contact it deals 3% of the target's max
    /// health: normal enemies via Health.TakeDamage, the Boss via the project's real
    /// IDamageable entry (MirrorAngelBodyDamageReceiver -> MirrorSaintessBoss total HP
    /// -> BossHUD / Phase2 / Death). It re-acquires a new target if the current one
    /// dies/disappears.
    ///
    /// The sprite lives on a child "Visual" so it can be scaled up 5x WITHOUT
    /// affecting hit detection (distance-based via hitDistance; no Collider2D). Purely
    /// additive and only used by the rhythm module — it never touches the normal
    /// player projectile, Boss AI/skills, or the Cursed / Blessed modules.
    /// </summary>
    public sealed class RhythmHomingBullet : MonoBehaviour
    {
        [Header("Homing")]
        [SerializeField] private float homingSpeed = 12f;
        [SerializeField] private float lifeTime = 4f;
        [SerializeField] private float hitDistance = 0.45f;
        [Tooltip("How often (s) to re-acquire the nearest enemy when the current target is lost/dead.")]
        [SerializeField] private float retargetInterval = 0.2f;

        [Header("Damage")]
        [SerializeField] private float damagePercentOfTargetMaxHp = 0.03f;

        [Header("Visual (scaled child only; hit range is unaffected)")]
        [Tooltip("Base visual scale of the bullet sprite (the previous size).")]
        [SerializeField] private float baseVisualScale = 0.35f;
        [Tooltip("Multiplier applied to the visual only. 5 = five times bigger than before.")]
        [SerializeField] private float visualScaleMultiplier = 5f;

        [Header("Targeting")]
        [Tooltip("When no normal enemy exists in the current scene and the scene is the BossRoom, allow the bullet to home onto and damage the Boss.")]
        [SerializeField] private bool allowBossTargetInBossRoom = true;

        // Unified target: either a normal enemy (Health) or the Boss (IDamageable +
        // MirrorSaintessBoss for MaxTotalHp / IsDead). Only one is set at a time.
        private bool _targetIsBoss;
        private Transform _targetTransform;
        private Health _targetHealth;          // normal enemy
        private IDamageable _bossDamageable;   // boss damage entry (Body receiver or root)
        private MirrorSaintessBoss _boss;      // boss root (max HP / death)

        private Vector2 _lastDirection = Vector2.right;
        private float _age;
        private float _retargetTimer;
        private bool _damaged;

        private Transform _visualRoot;
        private SpriteRenderer _renderer;

        private static Sprite _sharedSprite;

        public void Init(Health target, Vector2 fallbackDirection, float speed, float life, float damagePercent)
        {
            _lastDirection = fallbackDirection.sqrMagnitude > 0.0001f ? fallbackDirection.normalized : Vector2.right;
            homingSpeed = speed;
            lifeTime = life;
            damagePercentOfTargetMaxHp = damagePercent;
            EnsureVisual();

            if (target != null)
                SetEnemyTarget(target);
            else
                Retarget();   // no normal enemy seeded (e.g. BossRoom) -> try enemy then Boss now
        }

        private void Awake()
        {
            EnsureVisual();
        }

        private void EnsureVisual()
        {
            if (_visualRoot != null) return;

            // Defensive: if anything added a SpriteRenderer to the root, disable it so
            // we only render the (scaled) child sprite.
            var rootSr = GetComponent<SpriteRenderer>();
            if (rootSr != null) rootSr.enabled = false;

            transform.localScale = Vector3.one;   // root never scales (keeps hit logic stable)

            var existing = transform.Find("Visual");
            if (existing != null)
            {
                _visualRoot = existing;
                _renderer = existing.GetComponent<SpriteRenderer>();
                if (_renderer == null) _renderer = existing.gameObject.AddComponent<SpriteRenderer>();
            }
            else
            {
                var go = new GameObject("Visual", typeof(SpriteRenderer));
                go.transform.SetParent(transform, false);
                _visualRoot = go.transform;
                _renderer = go.GetComponent<SpriteRenderer>();
            }

            _renderer.sprite = GetSharedSprite();
            _renderer.color = new Color(1f, 0.25f, 0.32f, 0.95f);
            _renderer.sortingOrder = 400;

            // 5x bigger visual lives entirely on the child; the root stays at scale 1.
            _visualRoot.localPosition = Vector3.zero;
            _visualRoot.localScale = Vector3.one * (baseVisualScale * visualScaleMultiplier);
        }

        private void Update()
        {
            _age += Time.deltaTime;
            if (_age >= lifeTime)
            {
                Destroy(gameObject);
                return;
            }

            // Re-acquire a target (nearest normal enemy, then Boss in BossRoom) when
            // the current one is lost/dead.
            if (!HasValidTarget())
            {
                _retargetTimer -= Time.deltaTime;
                if (_retargetTimer <= 0f)
                {
                    Retarget();
                    _retargetTimer = retargetInterval;
                }
            }

            if (HasValidTarget())
            {
                Vector2 toTarget = (Vector2)_targetTransform.position - (Vector2)transform.position;
                if (toTarget.sqrMagnitude > 0.0001f)
                    _lastDirection = toTarget.normalized;

                if (toTarget.magnitude <= hitDistance)
                {
                    DealDamage();
                    return;
                }
            }

            // Whether homing or coasting (no target found yet), keep moving forward.
            transform.position += (Vector3)(_lastDirection * homingSpeed * Time.deltaTime);
        }

        private void DealDamage()
        {
            if (_damaged)
            {
                Destroy(gameObject);
                return;
            }
            _damaged = true;

            if (_targetIsBoss)
            {
                if (_boss != null && !_boss.IsDead && _bossDamageable != null)
                {
                    int dmg = Mathf.Max(1, Mathf.CeilToInt(_boss.MaxTotalHp * damagePercentOfTargetMaxHp));
                    Debug.Log($"[RhythmHomingBullet] Boss hit {_boss.gameObject.name}, dmg={dmg} ({damagePercentOfTargetMaxHp:P0} of {_boss.MaxTotalHp}), hpBefore={_boss.CurrentTotalHp}/{_boss.MaxTotalHp}.");
                    _bossDamageable.TakeHit(dmg, gameObject);   // real Boss entry: shield -> total HP -> BossHUD/Phase2/Death
                }
            }
            else if (_targetHealth != null && !_targetHealth.IsDead())
            {
                int dmg = Mathf.Max(1, Mathf.CeilToInt(_targetHealth.maxHealth * damagePercentOfTargetMaxHp));
                Debug.Log($"[RhythmHomingBullet] Hit {_targetHealth.gameObject.name}, dmg={dmg} ({damagePercentOfTargetMaxHp:P0} of {_targetHealth.maxHealth}).");
                _targetHealth.TakeDamage(dmg);
            }

            Destroy(gameObject);
        }

        // ---------------- Target management ----------------

        private bool HasValidTarget()
        {
            if (_targetTransform == null) return false;
            if (_targetIsBoss)
                return _boss != null && !_boss.IsDead && _bossDamageable != null;
            return _targetHealth != null && !_targetHealth.IsDead();
        }

        private void Retarget()
        {
            // 1) Always prefer the nearest current-scene NORMAL enemy.
            Health enemy = FindNearestEnemyInCurrentScene();
            if (enemy != null)
            {
                SetEnemyTarget(enemy);
                return;
            }

            // 2) No normal enemy: in the BossRoom, allow targeting the Boss.
            ClearTarget();
            if (allowBossTargetInBossRoom && IsInBossRoom())
                TryAcquireBossTarget();
        }

        private void SetEnemyTarget(Health h)
        {
            _targetIsBoss = false;
            _targetHealth = h;
            _targetTransform = h != null ? h.transform : null;
            _bossDamageable = null;
            _boss = null;
        }

        private void ClearTarget()
        {
            _targetIsBoss = false;
            _targetHealth = null;
            _targetTransform = null;
            _bossDamageable = null;
            _boss = null;
        }

        /// <summary>
        /// Acquire the Boss using the project's REAL damage entry: the Body's
        /// <see cref="MirrorAngelBodyDamageReceiver"/> (IDamageable -> shield ->
        /// <see cref="MirrorSaintessBoss"/> total HP -> BossHUD / Phase2 / Death),
        /// falling back to the boss root. Current active scene only; skips a dead boss.
        /// </summary>
        private void TryAcquireBossTarget()
        {
            Scene activeScene = SceneManager.GetActiveScene();

            var receivers = FindObjectsOfType<MirrorAngelBodyDamageReceiver>();
            for (int i = 0; i < receivers.Length; i++)
            {
                var r = receivers[i];
                if (r == null) continue;
                var s = r.gameObject.scene;
                if (!s.IsValid() || !s.isLoaded || s != activeScene) continue;

                var owner = r.GetComponentInParent<MirrorSaintessBoss>();
                if (owner == null || owner.IsDead) continue;

                _targetIsBoss = true;
                _targetTransform = r.transform;
                _bossDamageable = r;     // MirrorAngelBodyDamageReceiver : IDamageable
                _boss = owner;
                _targetHealth = null;
                Debug.Log($"[RhythmHomingBullet] Boss target acquired via Body receiver: {owner.gameObject.name} (hp={owner.CurrentTotalHp}/{owner.MaxTotalHp}).");
                return;
            }

            // Fallback: the boss root implements IDamageable directly.
            var bosses = FindObjectsOfType<MirrorSaintessBoss>();
            for (int i = 0; i < bosses.Length; i++)
            {
                var b = bosses[i];
                if (b == null || b.IsDead) continue;
                var s = b.gameObject.scene;
                if (!s.IsValid() || !s.isLoaded || s != activeScene) continue;

                _targetIsBoss = true;
                _targetTransform = b.transform;
                _bossDamageable = b;     // MirrorSaintessBoss : IDamageable
                _boss = b;
                _targetHealth = null;
                Debug.Log($"[RhythmHomingBullet] Boss target acquired via root: {b.gameObject.name} (hp={b.CurrentTotalHp}/{b.MaxTotalHp}).");
                return;
            }
        }

        private static bool IsInBossRoom()
        {
            return SceneManager.GetActiveScene().name.IndexOf("Boss", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Nearest living NORMAL enemy: any Health that is not the Player and not the
        /// Boss. (The Boss uses its own damage-receiver, not a plain Health, so it is
        /// naturally excluded; the name/component guard is extra safety.)
        /// </summary>
        private Health FindNearestEnemyInCurrentScene()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            Health best = null;
            float bestDist = float.MaxValue;
            Vector2 pos = transform.position;

            var all = FindObjectsOfType<Health>();
            for (int i = 0; i < all.Length; i++)
            {
                var h = all[i];
                if (h == null || h.IsDead()) continue;
                if (h.currentHealth <= 0) continue;

                // Current active scene only: never a stale/unloading Demo_Combat enemy,
                // and never the DontDestroyOnLoad player (it lives in the DDOL scene).
                var s = h.gameObject.scene;
                if (!s.IsValid() || !s.isLoaded || s != activeScene) continue;

                if (IsPlayer(h.gameObject)) continue;
                if (IsBoss(h.gameObject)) continue;
                if (!IsEnemy(h.gameObject)) continue;

                float d = Vector2.Distance(pos, h.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = h;
                }
            }
            return best;
        }

        private static bool IsPlayer(GameObject go)
        {
            if (go == null) return false;
            if (go.CompareTag("Player")) return true;
            return go.GetComponentInParent<PlayerController2D>() != null;
        }

        private static bool IsEnemy(GameObject go)
        {
            if (go == null) return false;
            if (go.CompareTag("Enemy")) return true;

            // Project enemies are NOT tagged; they carry MeleeEnemyController /
            // RangedEnemyController / (legacy) EnemyController — all contain "Enemy"
            // (and never "Boss") in the type name. Match by component name so no
            // normal enemy is missed and the Boss is never treated as a normal enemy.
            var comps = go.GetComponentsInParent<MonoBehaviour>(true);
            for (int i = 0; i < comps.Length; i++)
            {
                if (comps[i] == null) continue;
                string n = comps[i].GetType().Name;
                if (n.IndexOf("Enemy", System.StringComparison.OrdinalIgnoreCase) >= 0
                    && n.IndexOf("Boss", System.StringComparison.OrdinalIgnoreCase) < 0)
                    return true;
            }
            return false;
        }

        private static bool IsBoss(GameObject go)
        {
            var comps = go.GetComponentsInParent<MonoBehaviour>(true);
            for (int i = 0; i < comps.Length; i++)
            {
                if (comps[i] == null) continue;
                if (comps[i].GetType().Name.IndexOf("Boss", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return go.name.IndexOf("Boss", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static Sprite GetSharedSprite()
        {
            if (_sharedSprite != null)
                return _sharedSprite;

            const int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f - 1f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    float a = Mathf.Clamp01(1f - (d - radius * 0.55f) / (radius * 0.45f));
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            tex.Apply();
            _sharedSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            return _sharedSprite;
        }
    }
}
