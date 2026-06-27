namespace Cardwin.Lua
{
    /// <summary>
    /// Plain runtime data object describing one Lua-defined bullet, built from a
    /// BulletRegistry.lua entry. Decoupled from Unity assets so it can be reloaded
    /// (hot-updated) at runtime without touching ScriptableObjects.
    /// </summary>
    public class LuaBulletDefinition
    {
        public string Id;
        public bool Enabled;

        // display
        public string DisplayName;
        public string Description;
        public string Icon;
        public string Sprite;
        public string Rarity;

        // card
        public string CardType;
        public string[] Tags = System.Array.Empty<string>();
        public string LeftClickEffect;
        public string RightClickEffect;

        // bullet
        public string Prefab;
        public string Behavior;

        public float Speed;
        public float LifeTime;
        public float Damage;
        public string DamageMode;

        public int PierceCount;
        public float TurnSpeed;
        public float VisualScale = 1f;
        public float HitRadius = 0.35f;

        // inventory
        public bool AddToBackpack;
        public int DefaultCount;

        // drop
        public bool AddToDrop;
        public int DropWeight;
        public string[] DropEnemies = System.Array.Empty<string>();
        public int MinNight;

        public bool CanDropFor(string enemyType)
        {
            if (!Enabled || !AddToDrop)
                return false;
            if (DropEnemies == null || DropEnemies.Length == 0)
                return true; // no enemy filter = drops from anyone
            if (string.IsNullOrEmpty(enemyType))
                return false;
            foreach (string e in DropEnemies)
            {
                if (string.Equals(e, enemyType, System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public override string ToString()
        {
            return $"LuaBullet({Id}, enabled={Enabled}, behavior={Behavior}, dmg={Damage}/{DamageMode})";
        }
    }
}
