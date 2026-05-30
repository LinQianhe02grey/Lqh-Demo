using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Cardwin.Cards
{
    [CreateAssetMenu(menuName = "Cardwin/Card Database", fileName = "CardDatabase")]
    public class CardDatabase : ScriptableObject
    {
        public List<CardData> allCards = new();

        private Dictionary<string, CardData> _cardById;
        private Dictionary<string, CardData> _cardByName;

        private void OnEnable()
        {
            Initialize();
        }

        public void Initialize()
        {
            _cardById = new Dictionary<string, CardData>();
            _cardByName = new Dictionary<string, CardData>();
            int warnings = 0;

            for (int i = 0; i < allCards.Count; i++)
            {
                CardData card = allCards[i];
                if (card == null)
                {
                    warnings++;
                    continue;
                }

                if (string.IsNullOrEmpty(card.cardId))
                {
                    Debug.LogWarning($"[CardDatabase] Card at index {i} has empty cardId. Skipping.");
                    warnings++;
                    continue;
                }

                if (_cardById.ContainsKey(card.cardId))
                {
                    Debug.LogWarning($"[CardDatabase] Duplicate cardId '{card.cardId}' at index {i}. Skipping.");
                    warnings++;
                    continue;
                }

                _cardById[card.cardId] = card;

                if (!string.IsNullOrEmpty(card.cardName))
                {
                    if (_cardByName.ContainsKey(card.cardName))
                    {
                        Debug.LogWarning($"[CardDatabase] Duplicate cardName '{card.cardName}'. Only first entry kept.");
                    }
                    else
                    {
                        _cardByName[card.cardName] = card;
                    }
                }
            }

            if (warnings > 0)
                Debug.Log($"[CardDatabase] Initialized. Total={allCards.Count}, Warnings={warnings}");
        }

        public CardData GetById(string cardId)
        {
            if (_cardById == null)
                Initialize();
            if (string.IsNullOrEmpty(cardId))
                return null;
            _cardById.TryGetValue(cardId, out CardData result);
            return result;
        }

        public CardData GetByName(string cardName)
        {
            if (_cardByName == null)
                Initialize();
            if (string.IsNullOrEmpty(cardName))
                return null;
            _cardByName.TryGetValue(cardName, out CardData result);
            return result;
        }

        public List<CardData> GetByType(CardType type)
        {
            List<CardData> results = new();
            foreach (CardData card in allCards)
            {
                if (card != null && card.cardType == type)
                    results.Add(card);
            }
            return results;
        }

        public List<CardData> GetByRarity(CardRarity rarity)
        {
            List<CardData> results = new();
            foreach (CardData card in allCards)
            {
                if (card != null && card.rarity == rarity)
                    results.Add(card);
            }
            return results;
        }

        public List<CardData> GetByEffect(CardEffectType effect)
        {
            List<CardData> results = new();
            foreach (CardData card in allCards)
            {
                if (card == null)
                    continue;
                if (card.leftClickEffect == effect || card.rightClickEffect == effect)
                    results.Add(card);
            }
            return results;
        }

        public CardData GetRandomCard()
        {
            List<CardData> valid = allCards.Where(c => c != null).ToList();
            if (valid.Count == 0)
                return null;
            return valid[UnityEngine.Random.Range(0, valid.Count)];
        }

        public List<CardData> GetRandomCards(int count, bool allowDuplicate)
        {
            List<CardData> valid = allCards.Where(c => c != null).ToList();
            if (valid.Count == 0)
                return new List<CardData>();

            List<CardData> results = new();
            if (allowDuplicate)
            {
                for (int i = 0; i < count; i++)
                    results.Add(valid[UnityEngine.Random.Range(0, valid.Count)]);
            }
            else
            {
                List<CardData> pool = new List<CardData>(valid);
                int actualCount = Mathf.Min(count, pool.Count);
                for (int i = 0; i < actualCount; i++)
                {
                    int idx = UnityEngine.Random.Range(0, pool.Count);
                    results.Add(pool[idx]);
                    pool.RemoveAt(idx);
                }
            }
            return results;
        }

        public void ValidateDatabase()
        {
            int errors = 0;
            int warnings = 0;
            int total = 0;

            HashSet<string> ids = new();
            HashSet<string> names = new();

            for (int i = 0; i < allCards.Count; i++)
            {
                CardData card = allCards[i];
                if (card == null)
                {
                    Debug.LogError($"[CardDatabase] Validation Error: allCards[{i}] is null.");
                    errors++;
                    continue;
                }

                total++;

                if (string.IsNullOrEmpty(card.cardId))
                {
                    Debug.LogError($"[CardDatabase] Validation Error: index {i} has empty cardId.");
                    errors++;
                }
                else if (ids.Contains(card.cardId))
                {
                    Debug.LogError($"[CardDatabase] Validation Error: duplicate cardId '{card.cardId}' at index {i}.");
                    errors++;
                }
                else
                {
                    ids.Add(card.cardId);
                }

                if (string.IsNullOrEmpty(card.cardName))
                {
                    Debug.LogWarning($"[CardDatabase] Validation Warning: index {i} has empty cardName.");
                    warnings++;
                }
                else if (names.Contains(card.cardName))
                {
                    Debug.LogWarning($"[CardDatabase] Validation Warning: duplicate cardName '{card.cardName}'.");
                    warnings++;
                }
                else
                {
                    names.Add(card.cardName);
                }

                if (string.IsNullOrEmpty(card.description))
                {
                    Debug.LogWarning($"[CardDatabase] Validation Warning: '{card.cardName}' has no description.");
                    warnings++;
                }

                if (card.leftClickEffect == CardEffectType.Damage && card.damage <= 0)
                {
                    Debug.LogWarning($"[CardDatabase] Validation Warning: '{card.cardName}' has Damage leftClickEffect but damage<=0.");
                    warnings++;
                }
                if (card.leftClickEffect == CardEffectType.Block && card.block <= 0)
                {
                    Debug.LogWarning($"[CardDatabase] Validation Warning: '{card.cardName}' has Block leftClickEffect but block<=0.");
                    warnings++;
                }
                if (card.leftClickEffect == CardEffectType.Heal && card.heal <= 0)
                {
                    Debug.LogWarning($"[CardDatabase] Validation Warning: '{card.cardName}' has Heal leftClickEffect but heal<=0.");
                    warnings++;
                }
                if ((card.leftClickEffect == CardEffectType.Focus || card.rightClickEffect == CardEffectType.Focus) && card.focusGain <= 0)
                {
                    Debug.LogWarning($"[CardDatabase] Validation Warning: '{card.cardName}' has Focus effect but focusGain<=0.");
                    warnings++;
                }
            }

            Debug.Log($"[CardDatabase] Validated. Total={total}, Errors={errors}, Warnings={warnings}");
        }
    }
}
