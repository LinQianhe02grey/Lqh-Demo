using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cardwin.Cards;

namespace Cardwin.Editor
{
    public static class CardConfigValidator
    {
        private static readonly HashSet<CardEffectType> ImplementedEffects = new()
        {
            CardEffectType.None,
            CardEffectType.Damage,
            CardEffectType.Block,
            CardEffectType.Heal,
            CardEffectType.Focus,
        };

        private static readonly HashSet<string> SelfCardNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "Guard", "Heal", "Focus", "Mercy Shield", "Mercy Shield",
            "Combo Spark", "Quick Reload",
        };

        private static readonly HashSet<string> EnemyCardNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "Strike", "Pierce", "Burst", "Evil Shot",
            "Weakness Mark", "Aerial Mark",
        };

        private static readonly HashSet<string> FormalCardPrefixes = new(StringComparer.OrdinalIgnoreCase)
        {
            "C001", "C002", "C003", "C004", "C005", "C006",
            "C007", "C008", "C009", "C010", "C011", "C012",
        };

        private static readonly HashSet<string> LegacyAssetNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "Strike.asset", "Guard.asset", "Heal.asset", "Focus.asset",
        };

        private static StringBuilder _report;
        private static List<string> _errors;
        private static List<string> _warnings;
        private static List<string> _infos;
        private static List<string> _cardListEntries;
        private static HashSet<string> _seenCardIds;
        private static HashSet<string> _seenCardNames;
        private static Dictionary<string, List<string>> _idToAssetNames;

        public static void Validate()
        {
            _report = new StringBuilder();
            _errors = new List<string>();
            _warnings = new List<string>();
            _infos = new List<string>();
            _cardListEntries = new List<string>();
            _seenCardIds = new HashSet<string>();
            _seenCardNames = new HashSet<string>();
            _idToAssetNames = new Dictionary<string, List<string>>();

            Debug.Log("[CardValidator] Validation started.");

            List<CardData> scannedAssets = ScanCardDataAssets();
            Debug.Log($"[CardValidator] Scanned CardData assets: {scannedAssets.Count}");

            List<CardData> legacyAssets = ScanLegacyCardAssets();
            Debug.Log($"[CardValidator] Legacy card assets archived: {legacyAssets.Count}");

            CardDatabase database = AssetDatabase.LoadAssetAtPath<CardDatabase>(
                "Assets/Data/Cards/CardDatabase.asset");

            List<CardData> dbCards = database != null
                ? database.allCards.Where(c => c != null).ToList()
                : new List<CardData>();

            int dbTotal = database?.allCards.Count ?? 0;
            int dbNullCount = database != null
                ? database.allCards.Count(c => c == null)
                : 0;
            Debug.Log($"[CardValidator] CardDatabase cards: {dbCards.Count} (total entries: {dbTotal}, null: {dbNullCount})");

            CheckBasicFields(scannedAssets);
            CheckTypeAndUseTarget(scannedAssets);
            CheckGoodEvilCost(scannedAssets);
            CheckIsOffensive(scannedAssets);
            CheckEffectImplementation(scannedAssets);
            CheckNumericValues(scannedAssets);
            CheckCardDatabase(database, dbCards, dbTotal, dbNullCount, scannedAssets, legacyAssets);
            CheckRewardPool(database, dbCards);
            CheckInventoryTestStock(database, dbCards);
            CheckLegacyAssets(legacyAssets, database);

            GenerateReport(scannedAssets.Count, dbCards.Count);
            SaveReport();

            Debug.Log($"[CardValidator] Errors: {_errors.Count} Warnings: {_warnings.Count} Infos: {_infos.Count}");
            Debug.Log("[CardValidator] Report saved to Assets/Data/CardImport/CardValidationReport.txt");
        }

        private static List<CardData> ScanCardDataAssets()
        {
            List<CardData> results = new();
            string[] guids = AssetDatabase.FindAssets("t:CardData", new[] { "Assets/Data/Cards" });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.StartsWith("Assets/Data/Cards/Legacy/"))
                    continue;
                CardData card = AssetDatabase.LoadAssetAtPath<CardData>(path);
                if (card != null)
                    results.Add(card);
            }

            return results;
        }

        private static List<CardData> ScanLegacyCardAssets()
        {
            List<CardData> results = new();
            string path = "Assets/Data/Cards/Legacy";
            if (!AssetDatabase.IsValidFolder(path))
                return results;

            string[] guids = AssetDatabase.FindAssets("t:CardData", new[] { path });
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                CardData card = AssetDatabase.LoadAssetAtPath<CardData>(assetPath);
                if (card != null)
                    results.Add(card);
            }

            return results;
        }


        private static void CheckBasicFields(List<CardData> cards)
        {
            foreach (CardData card in cards)
            {
                string path = AssetDatabase.GetAssetPath(card);
                string assetName = Path.GetFileName(path);
                string display = GetCardDisplay(card);

                if (string.IsNullOrEmpty(card.cardId))
                {
                    LogError($"Empty CardID asset={assetName}");
                }
                else
                {
                    if (_seenCardIds.Contains(card.cardId))
                    {
                        string others = _idToAssetNames.ContainsKey(card.cardId)
                            ? string.Join(" / ", _idToAssetNames[card.cardId])
                            : "unknown";
                        LogError($"Duplicate CardID {card.cardId}: {others} / {assetName}");
                    }
                    else
                    {
                        _seenCardIds.Add(card.cardId);
                        _idToAssetNames[card.cardId] = new List<string> { assetName };
                    }
                }

                if (string.IsNullOrEmpty(card.cardName))
                {
                    LogError($"Empty CardName asset={assetName}");
                }
                else
                {
                    if (_seenCardNames.Contains(card.cardName))
                    {
                        LogWarning($"Duplicate CardName '{card.cardName}' asset={assetName}");
                    }
                    else
                    {
                        _seenCardNames.Add(card.cardName);
                    }
                }

                if (card.useTarget == CardUseTarget.Both)
                {
                    LogWarning($"Card {display} has useTarget=Both (only Self/Enemy supported)");
                }

                if (string.IsNullOrEmpty(card.description))
                {
                    LogWarning($"Empty description card={display}");
                }

                if (!card.enabled)
                {
                    LogInfo($"Card {display} is disabled");
                }

                if (!card.implemented)
                {
                    LogInfo($"Card {display} is marked implemented=false");
                }

                if (card.icon == null)
                {
                    LogWarning($"Missing icon for card={display}");
                }

                if (card.goodCost < 0)
                {
                    LogError($"Card {display} goodCost={card.goodCost} is negative");
                }

                if (card.evilCost < 0)
                {
                    LogError($"Card {display} evilCost={card.evilCost} is negative");
                }

                if (!string.IsNullOrEmpty(card.cooldownLimit))
                {
                    string raw = card.cooldownLimit.Trim();
                    if (raw.Equals("None", StringComparison.OrdinalIgnoreCase)
                        || raw.Equals("N/A", StringComparison.OrdinalIgnoreCase)
                        || raw.Equals("-", StringComparison.OrdinalIgnoreCase))
                    {
                    }
                    else if (int.TryParse(raw, out int cdVal) && cdVal < 0)
                    {
                        LogWarning($"Card {display} cooldownLimit={card.cooldownLimit} is negative");
                    }
                }

                _cardListEntries.Add(FormatCardListEntry(card));
            }
        }

        private static void CheckTypeAndUseTarget(List<CardData> cards)
        {
            foreach (CardData card in cards)
            {
                string display = GetCardDisplay(card);

                if (SelfCardNames.Contains(card.cardName))
                {
                    if (card.useTarget != CardUseTarget.Self)
                    {
                        LogError($"Card {display} expected UseTarget=Self but got {card.useTarget}");
                    }
                }

                if (EnemyCardNames.Contains(card.cardName))
                {
                    if (card.useTarget != CardUseTarget.Enemy)
                    {
                        LogError($"Card {display} expected UseTarget=Enemy but got {card.useTarget}");
                    }
                }
            }
        }

        private static void CheckGoodEvilCost(List<CardData> cards)
        {
            foreach (CardData card in cards)
            {
                string display = GetCardDisplay(card);

                if (card.useTarget == CardUseTarget.Self)
                {
                    if (card.goodCost <= 0)
                    {
                        LogWarning($"Self card {display} has goodCost={card.goodCost} (expected >0)");
                    }
                    if (card.evilCost > 0)
                    {
                        LogWarning($"Self card {display} has evilCost={card.evilCost} (expected 0)");
                    }
                }

                if (card.useTarget == CardUseTarget.Enemy)
                {
                    if (card.evilCost <= 0)
                    {
                        LogWarning($"Enemy card {display} has evilCost={card.evilCost} (expected >0)");
                    }
                    if (card.goodCost > 0)
                    {
                        LogWarning($"Enemy card {display} has goodCost={card.goodCost} (expected 0)");
                    }
                }

                if (card.goodCost > 0 && card.evilCost > 0)
                {
                    LogWarning($"Card {display} has both goodCost={card.goodCost} and evilCost={card.evilCost} > 0");
                }

                if (card.goodCost == 0 && card.evilCost == 0)
                {
                    LogWarning($"Card {display} has goodCost=0 and evilCost=0");
                }
            }
        }

        private static void CheckIsOffensive(List<CardData> cards)
        {
            foreach (CardData card in cards)
            {
                string display = GetCardDisplay(card);

                bool hasDamageEffect = card.leftClickEffect == CardEffectType.Damage
                    || card.rightClickEffect == CardEffectType.Damage;

                if (hasDamageEffect && !card.IsOffensive)
                {
                    LogError($"Card {display} has Damage effect but IsOffensive=false");
                }

                bool hasNonOffensiveEffect =
                    card.leftClickEffect == CardEffectType.Heal
                    || card.rightClickEffect == CardEffectType.Heal
                    || card.leftClickEffect == CardEffectType.Block
                    || card.rightClickEffect == CardEffectType.Block
                    || card.leftClickEffect == CardEffectType.Focus
                    || card.rightClickEffect == CardEffectType.Focus;

                if (hasNonOffensiveEffect && card.IsOffensive && !hasDamageEffect)
                {
                    LogWarning($"Card {display} has Heal/Block/Focus effect but IsOffensive=true");
                }

                if (card.IsOffensive && card.useTarget == CardUseTarget.Self)
                {
                    LogWarning($"Card {display} is Self-target but IsOffensive=true (affects Good/Evil loading)");
                }
            }
        }

        private static void CheckEffectImplementation(List<CardData> cards)
        {
            foreach (CardData card in cards)
            {
                string display = GetCardDisplay(card);

                CheckSingleEffect(card, display, card.leftClickEffect, "leftClickEffect");
                CheckSingleEffect(card, display, card.rightClickEffect, "rightClickEffect");
            }
        }

        private static void CheckSingleEffect(CardData card, string display, CardEffectType effect, string fieldName)
        {
            if (effect == CardEffectType.None)
                return;

            if (!ImplementedEffects.Contains(effect))
            {
                if (card.implemented)
                {
                    LogWarning($"Card {display} uses unimplemented effect {effect} ({fieldName}) but implemented=true");
                }
                else
                {
                    LogInfo($"Card {display} has unimplemented effect {effect} ({fieldName}), marked implemented=false");
                }
            }
        }

        private static void CheckNumericValues(List<CardData> cards)
        {
            foreach (CardData card in cards)
            {
                string display = GetCardDisplay(card);

                if (card.leftClickEffect == CardEffectType.Damage || card.rightClickEffect == CardEffectType.Damage)
                {
                    if (card.damage < 0)
                        LogError($"Card {display} damage={card.damage} is negative");
                    else if (card.damage > 50)
                        LogWarning($"High damage card={display} value={card.damage}");
                }

                if (card.leftClickEffect == CardEffectType.Heal || card.rightClickEffect == CardEffectType.Heal)
                {
                    if (card.heal < 0)
                        LogError($"Card {display} heal={card.heal} is negative");
                    else if (card.heal > 50)
                        LogWarning($"High heal card={display} value={card.heal}");
                }

                if (card.leftClickEffect == CardEffectType.Block || card.rightClickEffect == CardEffectType.Block)
                {
                    if (card.block < 0)
                        LogError($"Card {display} block={card.block} is negative");
                    else if (card.block > 80)
                        LogWarning($"Suspicious high block value card={display} raw={card.block}");
                }

                if (card.leftClickEffect == CardEffectType.Focus || card.rightClickEffect == CardEffectType.Focus)
                {
                    if (card.focusGain < 0)
                        LogError($"Card {display} focusGain={card.focusGain} is negative");
                }

                if (card.finalValue < 0)
                {
                    LogError($"Card {display} finalValue={card.finalValue} is negative");
                }

                if (card.finalValue > 3.0f)
                {
                    bool hasPercentRaw = !string.IsNullOrEmpty(card.finalValueRaw) && card.finalValueRaw.Contains("%");
                    if (hasPercentRaw || card.finalValue > 200f)
                        LogWarning($"Card {display} finalValue={card.finalValue} > 3.0 (possible percent value)");
                }

                if (!string.IsNullOrEmpty(card.finalValueRaw) && card.finalValueRaw.Contains("%"))
                {
                    LogWarning($"Card {display} has percent value raw={card.finalValueRaw} — verify not accidentally converted");
                    if (card.block > 500)
                        LogWarning($"Suspicious high block for percent card={display} block={card.block}");
                }

                if (card.damage < 0)
                    LogError($"Card {display} damage={card.damage} is negative");
                if (card.heal < 0)
                    LogError($"Card {display} heal={card.heal} is negative");
                if (card.block < 0)
                    LogError($"Card {display} block={card.block} is negative");
            }
        }

        private static void CheckCardDatabase(
            CardDatabase database,
            List<CardData> dbCards,
            int dbTotal,
            int dbNullCount,
            List<CardData> scannedAssets,
            List<CardData> legacyAssets)
        {
            if (database == null)
            {
                LogError("CardDatabase.asset not found at Assets/Data/Cards/CardDatabase.asset");
                return;
            }

            if (dbCards.Count == 0 && dbTotal == 0)
            {
                LogError("CardDatabase.cards list is empty");
            }

            for (int i = 0; i < (database?.allCards.Count ?? 0); i++)
            {
                if (database.allCards[i] == null)
                {
                    LogError($"CardDatabase.allCards[{i}] is null (missing reference)");
                }
            }

            HashSet<string> dbIds = new();
            foreach (CardData card in dbCards)
            {
                if (string.IsNullOrEmpty(card.cardId))
                    continue;

                if (dbIds.Contains(card.cardId))
                {
                    LogError($"CardDatabase duplicate CardID '{card.cardId}' — card={card.cardName}");
                }
                else
                {
                    dbIds.Add(card.cardId);
                }
            }

            foreach (CardData card in dbCards)
            {
                if (!card.enabled)
                {
                    LogWarning($"CardDatabase contains disabled card: {GetCardDisplay(card)}");
                }
            }

            HashSet<string> dbAssetPaths = new();
            foreach (CardData card in dbCards)
            {
                string path = AssetDatabase.GetAssetPath(card);
                if (!string.IsNullOrEmpty(path))
                    dbAssetPaths.Add(path);
            }

            HashSet<string> legacyPaths = new();
            if (legacyAssets != null)
            {
                foreach (CardData card in legacyAssets)
                {
                    string path = AssetDatabase.GetAssetPath(card);
                    if (!string.IsNullOrEmpty(path))
                        legacyPaths.Add(path);
                }
            }

            foreach (CardData card in scannedAssets)
            {
                string path = AssetDatabase.GetAssetPath(card);
                string fileName = Path.GetFileName(path);
                if (!dbAssetPaths.Contains(path))
                {
                    LogWarning($"CardData at {path} is NOT in CardDatabase");
                }
            }

            HashSet<string> dbCardNames = new(StringComparer.OrdinalIgnoreCase);
            foreach (CardData card in dbCards)
            {
                if (!string.IsNullOrEmpty(card.cardName))
                    dbCardNames.Add(card.cardName);
            }

            string[] formalNames = { "Strike", "Pierce", "Burst", "Guard", "Heal", "Focus",
                "Evil Shot", "Mercy Shield", "Combo Spark", "Quick Reload", "Weakness Mark", "Aerial Mark" };

            foreach (string name in formalNames)
            {
                if (!dbCardNames.Contains(name))
                {
                    LogWarning($"CardDatabase missing formal card: '{name}'");
                }
            }
        }

        private static string FindFormalEquivalent(CardData legacyCard)
        {
            if (string.IsNullOrEmpty(legacyCard?.cardName))
                return string.Empty;

            string name = legacyCard.cardName.Trim();
            var mapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Strike", "C001_Strike.asset" },
                { "Guard", "C004_Guard.asset" },
                { "Heal", "C005_Heal.asset" },
                { "Focus", "C006_Focus.asset" },
            };

            if (mapping.TryGetValue(name, out string formal))
                return formal;

            return string.Empty;
        }

        private static void CheckRewardPool(CardDatabase database, List<CardData> dbCards)
        {
            if (database == null || dbCards.Count == 0)
            {
                LogError("Reward pool: CardDatabase is null or empty — no reward pool available");
                return;
            }

            int disabledInPool = 0;
            int unimplementedEnabled = 0;
            int nullInPool = 0;

            for (int i = 0; i < database.allCards.Count; i++)
            {
                CardData card = database.allCards[i];
                if (card == null)
                {
                    nullInPool++;
                    continue;
                }

                if (!card.enabled)
                    disabledInPool++;

                if (!card.implemented && card.enabled)
                    unimplementedEnabled++;
            }

            if (nullInPool > 0)
                LogError($"Reward pool contains {nullInPool} null entries");

            if (disabledInPool > 0)
                LogWarning($"Reward pool contains {disabledInPool} disabled cards — should not enter reward pool");

            if (unimplementedEnabled > 0)
                LogWarning($"Reward pool contains {unimplementedEnabled} unimplemented but enabled cards — may appear in rewards");
        }

        private static void CheckInventoryTestStock(CardDatabase database, List<CardData> dbCards)
        {
            if (database == null || dbCards.Count == 0)
            {
                LogError("Inventory test stock: CardDatabase is null or empty");
                return;
            }

            int formalCount = dbCards.Count(c =>
                c != null && !string.IsNullOrEmpty(c.cardId) && FormalCardPrefixes.Any(p =>
                    c.cardId.StartsWith(p, StringComparison.OrdinalIgnoreCase)));

            if (formalCount == 0)
            {
                int enabledCount = dbCards.Count(c => c != null && c.enabled);
                formalCount = enabledCount;
            }

            int expectedTotal = formalCount * 20;

            LogInfo($"Inventory test stock: {formalCount} formal cards in DB × 20 = {expectedTotal} expected");

            if (formalCount == 12)
            {
                LogInfo("Inventory test stock: 12 formal cards, expected 240 total");
            }
            else if (formalCount < 12)
            {
                LogWarning($"Inventory test stock: only {formalCount} formal cards (expected 12)");
            }
        }

        private static void CheckLegacyAssets(List<CardData> legacyAssets, CardDatabase database)
        {
            if (legacyAssets == null || legacyAssets.Count == 0)
            {
                LogInfo("No legacy card assets found in Assets/Data/Cards/Legacy/");
                return;
            }

            LogInfo($"Legacy card assets archived: {legacyAssets.Count}");

            HashSet<string> dbAssetPaths = new();
            if (database != null)
            {
                foreach (CardData card in database.allCards)
                {
                    if (card == null)
                        continue;
                    string path = AssetDatabase.GetAssetPath(card);
                    if (!string.IsNullOrEmpty(path))
                        dbAssetPaths.Add(path);
                }
            }

            foreach (CardData legacy in legacyAssets)
            {
                string path = AssetDatabase.GetAssetPath(legacy);
                string fileName = Path.GetFileName(path);

                if (dbAssetPaths.Contains(path))
                {
                    LogError($"Legacy card still referenced by CardDatabase: {fileName}");
                }
                else
                {
                    LogInfo($"Legacy card archived (not in DB): {fileName}");
                }
            }
        }


        private static void GenerateReport(int assetCount, int dbCardCount)
        {
            _report.AppendLine("Cardwin Card Config Validation Report");
            _report.AppendLine($"Generated At: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            _report.AppendLine();
            _report.AppendLine("Summary:");
            _report.AppendLine();
            _report.AppendLine($"* Assets Scanned: {assetCount}");
            _report.AppendLine($"* Database Cards: {dbCardCount}");
            _report.AppendLine($"* Errors: {_errors.Count}");
            _report.AppendLine($"* Warnings: {_warnings.Count}");
            _report.AppendLine($"* Infos: {_infos.Count}");
            _report.AppendLine();

            _report.AppendLine("Errors:");
            _report.AppendLine();
            if (_errors.Count == 0)
                _report.AppendLine("  (none)");
            else
                for (int i = 0; i < _errors.Count; i++)
                    _report.AppendLine($"  {i + 1}. {_errors[i]}");
            _report.AppendLine();

            _report.AppendLine("Warnings:");
            _report.AppendLine();
            if (_warnings.Count == 0)
                _report.AppendLine("  (none)");
            else
                for (int i = 0; i < _warnings.Count; i++)
                    _report.AppendLine($"  {i + 1}. {_warnings[i]}");
            _report.AppendLine();

            _report.AppendLine("Infos:");
            _report.AppendLine();
            if (_infos.Count == 0)
                _report.AppendLine("  (none)");
            else
                for (int i = 0; i < _infos.Count; i++)
                    _report.AppendLine($"  {i + 1}. {_infos[i]}");
            _report.AppendLine();

            _report.AppendLine("Card List:");
            _report.AppendLine();
            foreach (string entry in _cardListEntries)
                _report.AppendLine($"  {entry}");
        }

        private static void SaveReport()
        {
            string dir = "Assets/Data/CardImport";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string path = Path.Combine(dir, "CardValidationReport.txt");
            File.WriteAllText(path, _report.ToString(), Encoding.UTF8);
            AssetDatabase.Refresh();
        }

        private static void LogError(string msg)
        {
            _errors.Add(msg);
            Debug.LogError($"[CardValidator][Error] {msg}");
        }

        private static void LogWarning(string msg)
        {
            _warnings.Add(msg);
            Debug.LogWarning($"[CardValidator][Warning] {msg}");
        }

        private static void LogInfo(string msg)
        {
            _infos.Add(msg);
            Debug.Log($"[CardValidator][Info] {msg}");
        }

        private static string GetCardDisplay(CardData card)
        {
            if (card == null)
                return "null";

            string id = string.IsNullOrEmpty(card.cardId) ? "?" : card.cardId;
            string name = string.IsNullOrEmpty(card.cardName) ? "Unnamed" : card.cardName;
            return $"{id} {name}";
        }

        private static string FormatCardListEntry(CardData card)
        {
            string id = string.IsNullOrEmpty(card.cardId) ? "?" : card.cardId;
            string name = string.IsNullOrEmpty(card.cardName) ? "Unnamed" : card.cardName;
            string target = card.useTarget.ToString();
            string rarity = card.rarity.ToString();
            string enabled = card.enabled ? "Enabled" : "Disabled";
            string implemented = card.implemented ? "Implemented" : "Unimplemented";
            return $"{id} | {name} | {target} | {rarity} | {enabled} | {implemented}";
        }
    }
}
