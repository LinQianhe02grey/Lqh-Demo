using System.Collections.Generic;
using UnityEngine;
using Cardwin.Cards;
using Cardwin.Inventory;

namespace Cardwin.Lua
{
    /// <summary>
    /// Minimal Lua-bullet drop provider. Builds weighted drop candidates per enemy
    /// type from the registry and rolls a drop. Drops are routed into the player's
    /// backpack (InventorySystem), never directly into the magazine.
    ///
    /// Filtering rules (enforced via <see cref="LuaBulletDefinition.CanDropFor"/>):
    ///   - enabled = false bullets never drop.
    ///   - drop.enabled = false bullets never drop.
    ///   - enemyType not in drop.enemies never drops (empty list = any enemy).
    /// </summary>
    public static class LuaBulletDropBridge
    {
        public static List<CardData> GetDropCandidates(string enemyType)
        {
            var list = new List<CardData>();
            foreach (LuaBulletDefinition def in LuaBulletDatabase.Instance.ListDropBullets(enemyType))
            {
                CardData card = LuaBulletCardBridge.GetOrCreateCard(def);
                if (card != null)
                    list.Add(card);
            }
            return list;
        }

        public static CardData RollDrop(string enemyType)
        {
            IReadOnlyList<LuaBulletDefinition> candidates =
                LuaBulletDatabase.Instance.ListDropBullets(enemyType);
            if (candidates.Count == 0)
                return null;

            int totalWeight = 0;
            foreach (LuaBulletDefinition def in candidates)
                totalWeight += Mathf.Max(0, def.DropWeight);

            if (totalWeight <= 0)
                return null;

            int roll = Random.Range(0, totalWeight);
            int acc = 0;
            foreach (LuaBulletDefinition def in candidates)
            {
                acc += Mathf.Max(0, def.DropWeight);
                if (roll < acc)
                    return LuaBulletCardBridge.GetOrCreateCard(def);
            }

            return LuaBulletCardBridge.GetOrCreateCard(candidates[candidates.Count - 1]);
        }

        /// <summary>Rolls a drop for the given enemy type into the backpack.</summary>
        public static bool TryDropToInventory(string enemyType, InventorySystem inventory, float chance = 1f)
        {
            if (inventory == null)
                return false;
            if (chance < 1f && Random.value > chance)
                return false;

            CardData card = RollDrop(enemyType);
            if (card == null)
                return false;

            inventory.AddRuntimeCard(card, 1);
            Debug.Log($"[LuaBullet] Drop from {enemyType}: {card.cardName} (id={card.luaBulletId}) -> backpack");
            return true;
        }
    }
}
