using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Cardwin.Lua
{
    /// <summary>
    /// Reads and caches Lua-defined bullets from
    /// StreamingAssets/Lua/Bullets/BulletRegistry.lua and exposes CRUD-style queries.
    ///
    /// Create  : add a new bullet id table in BulletRegistry.lua, then Reload().
    /// Read     : Get / List* methods below.
    /// Update   : edit fields in BulletRegistry.lua, then ReloadLuaBullets().
    /// Delete   : soft-delete via enabled = false (filtered out of enabled/inventory/drop).
    ///
    /// Runtime-safe (no UnityEditor APIs), so it ships in a Windows build and reads
    /// the registry from the player's StreamingAssets folder.
    /// </summary>
    public class LuaBulletDatabase
    {
        public const string RegistryRelativePath = "Lua/Bullets/BulletRegistry.lua";

        private static LuaBulletDatabase _instance;
        public static LuaBulletDatabase Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LuaBulletDatabase();
                    _instance.EnsureLoaded();
                }
                return _instance;
            }
        }

        private readonly Dictionary<string, LuaBulletDefinition> _bullets =
            new Dictionary<string, LuaBulletDefinition>();
        private readonly List<LuaBulletDefinition> _ordered = new List<LuaBulletDefinition>();
        private bool _loaded;
        public int Version { get; private set; }

        public static string RegistryFullPath =>
            Path.Combine(Application.streamingAssetsPath, RegistryRelativePath);

        // ---- Lifecycle -----------------------------------------------------

        public void EnsureLoaded()
        {
            if (!_loaded)
                Reload();
        }

        /// <summary>Re-reads BulletRegistry.lua and rebuilds the cache.</summary>
        public void Reload()
        {
            _bullets.Clear();
            _ordered.Clear();
            Version = 0;
            _loaded = true;

            string path = RegistryFullPath;
            if (!File.Exists(path))
            {
                Debug.LogError($"[LuaBullet] Registry not found: {path}");
                return;
            }

            string source;
            try
            {
                source = File.ReadAllText(path);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LuaBullet] Failed reading registry: {e.Message}");
                return;
            }

            if (!SimpleLuaTableParser.TryParse(source, out LuaTable root, out string error))
            {
                Debug.LogError($"[LuaBullet] Parse error in BulletRegistry.lua: {error}");
                return;
            }

            Version = root.GetInt("version", 1);
            LuaTable bullets = root.GetTable("bullets");
            if (bullets == null)
            {
                Debug.LogError("[LuaBullet] BulletRegistry.lua has no 'bullets' table.");
                return;
            }

            foreach (var kvp in bullets.Map)
            {
                if (!(kvp.Value is LuaTable entry))
                    continue;

                LuaBulletDefinition def = BuildDefinition(kvp.Key, entry);
                _bullets[def.Id] = def;
                _ordered.Add(def);
            }

            Debug.Log($"Loaded Lua bullets: {_ordered.Count}");
        }

        public static void ReloadLuaBullets()
        {
            Instance.Reload();
        }

        private static LuaBulletDefinition BuildDefinition(string id, LuaTable entry)
        {
            var def = new LuaBulletDefinition
            {
                Id = id,
                Enabled = entry.GetBool("enabled", true)
            };

            LuaTable display = entry.GetTable("display");
            if (display != null)
            {
                def.DisplayName = display.GetString("name", id);
                def.Description = display.GetString("desc", string.Empty);
                def.Icon = display.GetString("icon", string.Empty);
                def.Sprite = display.GetString("sprite", string.Empty);
                def.Rarity = display.GetString("rarity", "Common");
            }
            else
            {
                def.DisplayName = id;
                def.Rarity = "Common";
            }

            LuaTable card = entry.GetTable("card");
            if (card != null)
            {
                def.CardType = card.GetString("cardType", "Attack");
                def.Tags = card.GetStringArray("tags");
                def.LeftClickEffect = card.GetString("leftClickEffect", "LuaBullet");
                def.RightClickEffect = card.GetString("rightClickEffect", "None");
            }
            else
            {
                def.CardType = "Attack";
                def.LeftClickEffect = "LuaBullet";
                def.RightClickEffect = "None";
            }

            LuaTable bullet = entry.GetTable("bullet");
            if (bullet != null)
            {
                def.Prefab = bullet.GetString("prefab", "LuaBulletHost");
                def.Behavior = bullet.GetString("behavior", string.Empty);
                def.Speed = bullet.GetFloat("speed", 10f);
                def.LifeTime = bullet.GetFloat("lifeTime", 4f);
                def.Damage = bullet.GetFloat("damage", 5f);
                def.DamageMode = bullet.GetString("damageMode", "Flat");
                def.PierceCount = bullet.GetInt("pierceCount", 0);
                def.TurnSpeed = bullet.GetFloat("turnSpeed", 0f);
                def.VisualScale = bullet.GetFloat("visualScale", 1f);
                def.HitRadius = bullet.GetFloat("hitRadius", 0.35f);
            }

            LuaTable inventory = entry.GetTable("inventory");
            if (inventory != null)
            {
                bool invEnabled = inventory.GetBool("enabled", true);
                def.AddToBackpack = invEnabled && inventory.GetBool("addToBackpack", true);
                def.DefaultCount = inventory.GetInt("defaultCount", 1);
            }

            LuaTable drop = entry.GetTable("drop");
            if (drop != null)
            {
                def.AddToDrop = drop.GetBool("enabled", false);
                def.DropWeight = drop.GetInt("weight", 0);
                def.DropEnemies = drop.GetStringArray("enemies");
                def.MinNight = drop.GetInt("minNight", 0);
            }

            return def;
        }

        // ---- Read / Query --------------------------------------------------

        public LuaBulletDefinition GetBullet(string id)
        {
            EnsureLoaded();
            if (string.IsNullOrEmpty(id))
                return null;
            return _bullets.TryGetValue(id, out LuaBulletDefinition def) ? def : null;
        }

        public IReadOnlyList<LuaBulletDefinition> ListAll()
        {
            EnsureLoaded();
            return _ordered;
        }

        public IReadOnlyList<LuaBulletDefinition> ListEnabled()
        {
            EnsureLoaded();
            var list = new List<LuaBulletDefinition>();
            foreach (var def in _ordered)
                if (def.Enabled)
                    list.Add(def);
            return list;
        }

        public IReadOnlyList<LuaBulletDefinition> ListInventoryBullets()
        {
            EnsureLoaded();
            var list = new List<LuaBulletDefinition>();
            foreach (var def in _ordered)
                if (def.Enabled && def.AddToBackpack)
                    list.Add(def);
            return list;
        }

        public IReadOnlyList<LuaBulletDefinition> ListDropBullets(string enemyType)
        {
            EnsureLoaded();
            var list = new List<LuaBulletDefinition>();
            foreach (var def in _ordered)
                if (def.CanDropFor(enemyType))
                    list.Add(def);
            return list;
        }
    }
}
