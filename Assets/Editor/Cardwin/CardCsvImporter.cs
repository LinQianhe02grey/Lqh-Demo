using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Cardwin.Cards;

namespace Cardwin.Editor
{
    public class CardCsvImporter : EditorWindow
    {
        private const string CsvPath = "Assets/Data/CardImport/bullets.csv";
        private const string CardsFolder = "Assets/Data/Cards";

        [MenuItem("Tools/Cardwin/Import Cards From CSV")]
        public static void Import()
        {
            if (!File.Exists(CsvPath))
            {
                Debug.LogError($"[CardCsvImporter] CSV not found: {CsvPath}");
                return;
            }

            string csvText = File.ReadAllText(CsvPath, Encoding.UTF8);
            string[] lines = csvText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2)
            {
                Debug.LogError("[CardCsvImporter] CSV has no data rows.");
                return;
            }

            var headers = ParseCsvLine(lines[0]);
            Dictionary<string, int> colMap = new();
            for (int i = 0; i < headers.Count; i++)
                colMap[headers[i].Trim()] = i;

            if (!Directory.Exists(CardsFolder))
                Directory.CreateDirectory(CardsFolder);

            AssetDatabase.StartAssetEditing();

            int created = 0, updated = 0, warnings = 0;
            HashSet<string> importedIds = new();

            for (int row = 1; row < lines.Length; row++)
            {
                var fields = ParseCsvLine(lines[row]);
                if (fields.Count == 0 || string.IsNullOrWhiteSpace(fields[0]))
                    continue;

                string cardId = GetField(fields, colMap, "CardID").Trim();
                if (string.IsNullOrEmpty(cardId))
                {
                    Debug.LogWarning($"[CardCsvImporter] Row {row}: empty CardID, skipping.");
                    warnings++;
                    continue;
                }

                if (!importedIds.Add(cardId))
                {
                    Debug.LogWarning($"[CardCsvImporter] Duplicate CardID={cardId} at row {row}.");
                    warnings++;
                    continue;
                }

                string cardName = GetField(fields, colMap, "卡牌名").Trim();
                string typeStr = GetField(fields, colMap, "类型").Trim();
                string rarityStr = GetField(fields, colMap, "稀有度").Trim();
                string finalValueRaw = GetField(fields, colMap, "FinalValue").Trim();
                string unit = GetField(fields, colMap, "单位").Trim();
                string goodStr = GetField(fields, colMap, "善消耗").Trim();
                string evilStr = GetField(fields, colMap, "恶消耗").Trim();
                string limit = GetField(fields, colMap, "冷却/限制").Trim();
                string role = GetField(fields, colMap, "定位").Trim();
                string risk = GetField(fields, colMap, "风险备注").Trim();
                string input = GetField(fields, colMap, "输入").Trim();
                string baseEffect = GetField(fields, colMap, "基础效果").Trim();

                CardType cardType = ParseCardType(typeStr, ref warnings);
                CardRarity rarity = ParseRarity(rarityStr, ref warnings);

                float finalValue = 0f;
                bool isPercent = finalValueRaw.Contains("%");
                string cleanVal = finalValueRaw.Replace("%", "").Trim();
                if (!float.TryParse(cleanVal, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out finalValue))
                {
                    Debug.LogWarning($"[CardCsvImporter] Cannot parse FinalValue '{finalValueRaw}' for {cardId}. Using 0.");
                    warnings++;
                }

                int goodCost = int.TryParse(goodStr, out int g) ? g : 0;
                int evilCost = int.TryParse(evilStr, out int e) ? e : 0;

                CardEffectType effect = ResolveEffect(cardType, unit, baseEffect, ref warnings);

                string safeName = SanitizeName(cardName);
                string assetPath = $"{CardsFolder}/{cardId}_{safeName}.asset";

                CardData card = AssetDatabase.LoadAssetAtPath<CardData>(assetPath);
                bool isNew = card == null;

                if (isNew)
                {
                    card = ScriptableObject.CreateInstance<CardData>();
                    AssetDatabase.CreateAsset(card, assetPath);
                    created++;
                }
                else
                {
                    updated++;
                }

                card.cardId = cardId;
                card.cardName = cardName;
                card.cardType = cardType;
                card.rarity = rarity;
                card.useTarget = input.Contains("左键") ? CardUseTarget.Enemy : (input.Contains("右键") ? CardUseTarget.Self : CardUseTarget.Enemy);
                card.leftClickEffect = effect;
                card.rightClickEffect = effect;
                card.goodCost = goodCost;
                card.evilCost = evilCost;
                card.finalValueRaw = finalValueRaw;
                card.finalValue = finalValue;
                card.valueUnit = unit;
                card.baseEffectDescription = baseEffect;
                card.cooldownLimit = limit;
                card.role = role;
                card.riskNotes = risk;
                card.isImportedFromCsv = true;

                card.damage = unit.Contains("伤害") ? Mathf.RoundToInt(finalValue) : 0;
                card.block = unit.Contains("护盾") && !isPercent ? Mathf.RoundToInt(finalValue) : 0;
                card.heal = unit.Contains("治疗") ? Mathf.RoundToInt(finalValue) : 0;

                if (unit.Contains("增伤%") || unit.Contains("易伤%") || unit.Contains("对空增伤%"))
                    card.focusGain = Mathf.RoundToInt(finalValue * 100f);

                if (unit.Contains("护盾") && isPercent)
                {
                    card.block = 25;
                    Debug.LogWarning($"[CardCsvImporter] {cardId}: shield percentage {finalValueRaw} mapped to block=25 (review needed).");
                }

                if (card.projectilePrefab == null && !isNew)
                {
                    // keep existing prefab reference
                }

                EditorUtility.SetDirty(card);
                Debug.Log($"[CardCsvImporter] {(isNew ? "Created" : "Updated")}: {cardId} {cardName}");
            }

            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            UpdateCardDatabase();

            Debug.Log($"[CardCsvImporter] Done. Created={created} Updated={updated} Warnings={warnings} Errors=0");
        }

        private static List<string> ParseCsvLine(string line)
        {
            List<string> result = new();
            bool inQuotes = false;
            StringBuilder current = new();
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            result.Add(current.ToString());
            return result;
        }

        private static string GetField(List<string> fields, Dictionary<string, int> colMap, string key)
        {
            if (colMap.TryGetValue(key, out int idx) && idx < fields.Count)
                return fields[idx];
            return "";
        }

        private static CardType ParseCardType(string s, ref int warnings)
        {
            switch (s.ToLower())
            {
                case "attack": return CardType.Attack;
                case "defense": return CardType.Defense;
                case "support": return CardType.Support;
                case "debuff": return CardType.Debuff;
                case "heal": return CardType.Heal;
                default:
                    Debug.LogWarning($"[CardCsvImporter] Unknown type '{s}', defaulting to Attack.");
                    warnings++;
                    return CardType.Attack;
            }
        }

        private static CardRarity ParseRarity(string s, ref int warnings)
        {
            switch (s.ToLower())
            {
                case "common": return CardRarity.Common;
                case "rare": return CardRarity.Rare;
                case "epic": return CardRarity.Epic;
                default:
                    Debug.LogWarning($"[CardCsvImporter] Unknown rarity '{s}', defaulting to Common.");
                    warnings++;
                    return CardRarity.Common;
            }
        }

        private static CardEffectType ResolveEffect(CardType type, string unit, string baseEffect, ref int warnings)
        {
            if (baseEffect.Contains("穿透") || baseEffect.Contains("Pierce"))
                return CardEffectType.Damage;
            if (baseEffect.Contains("易伤") || baseEffect.Contains("Weakness"))
                return CardEffectType.WeaknessMark;
            if (baseEffect.Contains("装填") || baseEffect.Contains("Reload"))
                return CardEffectType.QuickReload;
            if (baseEffect.Contains("连击") || baseEffect.Contains("Combo"))
                return CardEffectType.ComboSpark;
            if (baseEffect.Contains("对空"))
                return CardEffectType.AerialMark;

            if (type == CardType.Attack || unit.Contains("伤害"))
                return CardEffectType.Damage;
            if (type == CardType.Defense || unit.Contains("护盾"))
                return CardEffectType.Block;
            if (unit.Contains("治疗"))
                return CardEffectType.Heal;
            if (unit.Contains("增伤%") || unit.Contains("Focus"))
                return CardEffectType.Focus;

            return CardEffectType.Damage;
        }

        private static string SanitizeName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name.Replace(" ", "_");
        }

        private static void UpdateCardDatabase()
        {
            string dbPath = $"{CardsFolder}/CardDatabase.asset";
            CardDatabase db = AssetDatabase.LoadAssetAtPath<CardDatabase>(dbPath);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<CardDatabase>();
                AssetDatabase.CreateAsset(db, dbPath);
            }

            string[] guids = AssetDatabase.FindAssets("t:CardData", new[] { CardsFolder });
            db.allCards.Clear();
            var cards = new List<CardData>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                CardData cd = AssetDatabase.LoadAssetAtPath<CardData>(path);
                if (cd != null && cd != db && !string.IsNullOrEmpty(cd.cardId))
                    cards.Add(cd);
            }

            cards.Sort((a, b) => string.Compare(a.cardId, b.cardId, StringComparison.OrdinalIgnoreCase));
            db.allCards.AddRange(cards);

            EditorUtility.SetDirty(db);
            AssetDatabase.SaveAssets();
            Debug.Log($"[CardCsvImporter] CardDatabase updated. Count={db.allCards.Count}");
        }
    }
}
