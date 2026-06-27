using System.Collections.Generic;
using UnityEngine;
using Cardwin.Cards;
using Cardwin.Inventory;

namespace Cardwin.Lua
{
    /// <summary>
    /// Builds runtime <see cref="CardData"/> instances for Lua-defined bullets and
    /// adds the inventory ones to the player's backpack. Lua bullet cards exist only
    /// at runtime (CreateInstance) — no CardData asset is created or modified, so the
    /// existing card assets and the basic cards stay untouched.
    /// </summary>
    public static class LuaBulletCardBridge
    {
        // Cache one runtime CardData per lua bullet id (shared between backpack & drops).
        private static readonly Dictionary<string, CardData> _cards = new Dictionary<string, CardData>();

        public static CardData GetOrCreateCard(LuaBulletDefinition def)
        {
            if (def == null)
                return null;

            if (_cards.TryGetValue(def.Id, out CardData existing) && existing != null)
                return existing;

            var card = ScriptableObject.CreateInstance<CardData>();
            card.name = $"LuaCard_{def.Id}";
            card.cardId = def.Id;
            card.cardName = string.IsNullOrEmpty(def.DisplayName) ? def.Id : def.DisplayName;
            card.cardType = CardType.Attack;
            card.rarity = ParseRarity(def.Rarity);
            card.description = def.Description;

            // Display values only; actual flight/damage is driven by the Lua definition.
            card.damage = Mathf.Max(0, Mathf.RoundToInt(def.Damage));
            card.leftClickEffect = CardEffectType.Damage;
            card.rightClickEffect = CardEffectType.None;

            card.enabled = true;
            card.implemented = true;

            // The marker that makes CardEffectExecutor route this through the Lua channel.
            card.isLuaBullet = true;
            card.luaBulletId = def.Id;

            _cards[def.Id] = card;
            return card;
        }

        public static CardData GetCardById(string luaBulletId)
        {
            LuaBulletDefinition def = LuaBulletDatabase.Instance.GetBullet(luaBulletId);
            return def != null ? GetOrCreateCard(def) : null;
        }

        /// <summary>
        /// Adds every enabled, addToBackpack Lua bullet to the inventory using its
        /// defaultCount. Idempotent: skips bullets already present in the backpack.
        /// </summary>
        public static int AddInventoryBulletsToBackpack(InventorySystem inventory)
        {
            if (inventory == null)
                return 0;

            int added = 0;
            foreach (LuaBulletDefinition def in LuaBulletDatabase.Instance.ListInventoryBullets())
            {
                if (InventoryHasLuaBullet(inventory, def.Id))
                    continue;

                CardData card = GetOrCreateCard(def);
                int count = Mathf.Max(1, def.DefaultCount);
                inventory.AddRuntimeCard(card, count);
                added++;
                Debug.Log($"[LuaBullet] Added to backpack: {def.DisplayName} (id={def.Id}) x{count}");
            }

            return added;
        }

        private static bool InventoryHasLuaBullet(InventorySystem inventory, string luaBulletId)
        {
            foreach (CardData c in inventory.ownedCards)
            {
                if (c != null && c.isLuaBullet && c.luaBulletId == luaBulletId)
                    return true;
            }
            return false;
        }

        private static CardRarity ParseRarity(string rarity)
        {
            if (string.IsNullOrEmpty(rarity))
                return CardRarity.Common;
            switch (rarity.ToLowerInvariant())
            {
                case "rare": return CardRarity.Rare;
                case "epic":
                case "legendary": return CardRarity.Epic;
                default: return CardRarity.Common;
            }
        }
    }
}
