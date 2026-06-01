using System.Collections.Generic;
using UnityEngine;
using Cardwin.Cards;

namespace Cardwin.Inventory
{
    [System.Serializable]
    public class InventoryEntry
    {
        public CardData card;
        public int count;
    }

    public class InventorySystem : MonoBehaviour
    {
        [Header("Owned Cards")]
        public List<CardData> ownedCards = new();

        public CardDatabase defaultDatabase;
        public bool useTestStock = true;
        public bool resetTestStockOnStart = true;
        private bool _hasInitializedThisRun;

        public void InitializeForRun(CardDatabase database)
        {
            if (_hasInitializedThisRun)
                return;

            if (useTestStock && resetTestStockOnStart)
            {
                ResetToTestStock(database);
            }

            _hasInitializedThisRun = true;
            Debug.Log("[Inventory] InitializeForRun.");
        }

        public int GetOwnedTotalCount()
        {
            return ownedCards.Count;
        }

        public void ResetToTestStock(CardDatabase database)
        {
            ownedCards.Clear();

            if (database == null)
            {
                database = FindObjectOfType<CardDatabase>();
                if (database == null)
                {
                    CardDatabase[] all = Resources.FindObjectsOfTypeAll<CardDatabase>();
                    if (all.Length > 0)
                        database = all[0];
                }
            }

            if (database == null)
            {
                Debug.LogError("[Inventory] Cannot find CardDatabase. Test stock not initialized.");
                return;
            }

            int cardCount = 0;
            var seenIds = new HashSet<string>();
            foreach (CardData card in database.allCards)
            {
                if (card == null || !card.enabled) continue;
                if (!seenIds.Add(card.cardId)) continue;
                AddCards(card, 20);
                cardCount++;
            }

            Debug.Log($"[Inventory] Test stock reset all cards. Count={cardCount}, Each=20, Total={ownedCards.Count}");
        }

        public void AddCard(CardData card)
        {
            if (card == null)
            {
                Debug.LogWarning("[Inventory] Cannot add null card.");
                return;
            }

            ownedCards.Add(card);
        }

        public void AddCards(CardData card, int count)
        {
            if (card == null || count <= 0)
                return;

            for (int i = 0; i < count; i++)
                ownedCards.Add(card);
        }

        public bool RemoveCard(CardData card)
        {
            if (card == null)
                return false;

            for (int i = 0; i < ownedCards.Count; i++)
            {
                if (ownedCards[i] == card)
                {
                    ownedCards.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public List<CardData> GetOwnedCards()
        {
            return ownedCards;
        }

        public int GetCount(CardData card)
        {
            if (card == null)
                return 0;

            int count = 0;
            foreach (CardData c in ownedCards)
            {
                if (c == card)
                    count++;
            }
            return count;
        }

        public List<InventoryEntry> GetCardCounts()
        {
            List<InventoryEntry> entries = new List<InventoryEntry>();
            Dictionary<string, int> seen = new Dictionary<string, int>();

            foreach (CardData card in ownedCards)
            {
                if (card == null)
                    continue;

                if (seen.ContainsKey(card.cardId))
                {
                    seen[card.cardId]++;
                }
                else
                {
                    seen[card.cardId] = 1;
                    entries.Add(new InventoryEntry { card = card, count = 1 });
                    continue;
                }

                for (int i = 0; i < entries.Count; i++)
                {
                    if (entries[i].card.cardId == card.cardId)
                    {
                        entries[i].count = seen[card.cardId];
                        break;
                    }
                }
            }

            return entries;
        }

        public bool HasCard(CardData card)
        {
            return ownedCards.Contains(card);
        }

        public void EnsureTestStockIfEmpty(CardDatabase database)
        {
            if (ownedCards.Count == 0)
                ResetToTestStock(database);
        }

        public void SetOwnedCardsFromCounts(Dictionary<CardData, int> counts)
        {
            ownedCards.Clear();
            if (counts == null)
                return;

            foreach (var kvp in counts)
            {
                if (kvp.Key != null && kvp.Value > 0)
                {
                    for (int i = 0; i < kvp.Value; i++)
                        ownedCards.Add(kvp.Key);
                }
            }

            _hasInitializedThisRun = true;
        }
    }
}
