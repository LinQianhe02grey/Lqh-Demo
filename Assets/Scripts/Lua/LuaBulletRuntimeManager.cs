using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cardwin.Combat;
using Cardwin.Enemies;
using Cardwin.Inventory;

namespace Cardwin.Lua
{
    /// <summary>
    /// Self-bootstrapping runtime glue for the Lua bullet system. Created automatically
    /// via RuntimeInitializeOnLoadMethod (no scene or prefab edits required):
    ///   - ensures the registry is loaded once at startup,
    ///   - on every scene load, subscribes a Lua-bullet drop roll to enemy deaths
    ///     (MeleeEnemy / RangedEnemy) so kills can drop Lua bullets into the backpack.
    /// Does NOT modify enemy AI, prefabs or RewardManager. Runtime-safe, packable.
    /// </summary>
    public class LuaBulletRuntimeManager : MonoBehaviour
    {
        [Tooltip("Chance (0..1) for an enemy death to roll a Lua bullet drop into the backpack.")]
        public float dropChance = 1f;

        private static LuaBulletRuntimeManager _instance;
        private readonly HashSet<Health> _hooked = new HashSet<Health>();
        private InventorySystem _inventory;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null)
                return;

            var go = new GameObject("LuaBulletRuntimeManager");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<LuaBulletRuntimeManager>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            LuaBulletDatabase.Instance.EnsureLoaded();
            SceneManager.sceneLoaded += OnSceneLoaded;
            HookEnemiesInScene();
        }

        private void OnDestroy()
        {
            if (_instance == this)
                SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            HookEnemiesInScene();
        }

        private void HookEnemiesInScene()
        {
            Health[] healths = FindObjectsOfType<Health>();
            foreach (Health h in healths)
            {
                if (h == null || _hooked.Contains(h))
                    continue;

                string enemyType = ResolveEnemyType(h);
                if (enemyType == null)
                    continue;

                _hooked.Add(h);
                h.OnDeath.AddListener(() => OnEnemyDeath(enemyType));
            }
        }

        private static string ResolveEnemyType(Health h)
        {
            if (h.GetComponent<MeleeEnemyController>() != null)
                return "MeleeEnemy";
            if (h.GetComponent<RangedEnemyController>() != null)
                return "RangedEnemy";
            return null;
        }

        private void OnEnemyDeath(string enemyType)
        {
            if (_inventory == null)
                _inventory = FindObjectOfType<InventorySystem>();
            if (_inventory == null)
                return;

            LuaBulletDropBridge.TryDropToInventory(enemyType, _inventory, dropChance);
        }
    }
}
