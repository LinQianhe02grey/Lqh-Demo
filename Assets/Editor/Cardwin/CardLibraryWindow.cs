using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Cardwin.Cards;

namespace Cardwin.Editor
{
    public class CardLibraryWindow : EditorWindow
    {
        private const string CardsFolder = "Assets/Data/Cards";
        private const string LegacyFolder = "Assets/Data/Cards/Legacy";
        private const string DbPath = CardsFolder + "/CardDatabase.asset";

        private CardDatabase _database;
        private List<CardData> _allCards = new();
        private List<CardData> _legacyCards = new();
        private bool _showLegacy;
        private Vector2 _listScroll, _detailScroll;
        private int _selectedIndex = -1;

        private string _searchText = "";
        private int _filterTypeIdx = 0;
        private int _filterRarityIdx = 0;
        private int _filterTargetIdx = 0;
        private int _filterStatus = 0;

        private static readonly string[] TypeFilterNames = { "All", "Self", "Enemy" };
        private static readonly CardUseTarget?[] TypeFilterTargets = { null, CardUseTarget.Self, CardUseTarget.Enemy };
        private static readonly string[] RarityFilterNames = { "All", "Common", "Rare", "Epic" };
        private static readonly CardRarity[] RarityFilterValues = { (CardRarity)(-1), CardRarity.Common, CardRarity.Rare, CardRarity.Epic };
        private static readonly string[] TargetFilterNames = { "All", "Enemy", "Self", "Both" };
        private static readonly CardUseTarget[] TargetFilterValues = { (CardUseTarget)(-1), CardUseTarget.Enemy, CardUseTarget.Self, CardUseTarget.Both };

        private List<CardData> FilteredCards
        {
            get
            {
                var list = new List<CardData>(_allCards);
                if (_showLegacy)
                    list.AddRange(_legacyCards);

                return list
                    .Where(c => c != null)
                    .Where(c => string.IsNullOrEmpty(_searchText)
                        || c.cardId.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0
                        || c.cardName.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0
                        || (c.description != null && c.description.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0))
                    .Where(c => _filterTypeIdx <= 0 || c.useTarget == TypeFilterTargets[_filterTypeIdx])
                    .Where(c => _filterRarityIdx <= 0 || c.rarity == RarityFilterValues[_filterRarityIdx])
                    .Where(c => _filterTargetIdx <= 0 || c.useTarget == TargetFilterValues[_filterTargetIdx])
                    .Where(c => _filterStatus switch
                    {
                        1 => c.enabled,
                        2 => !c.enabled,
                        3 => c.implemented,
                        4 => !c.implemented,
                        _ => true
                    })
                    .ToList();
            }
        }

        [MenuItem("Tools/Cardwin/Card Library")]
        public static void ShowWindow()
        {
            var w = GetWindow<CardLibraryWindow>("Card Library");
            w.minSize = new Vector2(800, 500);
            w.Show();
        }

        private void OnEnable()
        {
            Refresh();
        }

        private void Refresh()
        {
            _database = AssetDatabase.LoadAssetAtPath<CardDatabase>(DbPath);
            _allCards.Clear();
            _legacyCards.Clear();

            string[] allGuids = AssetDatabase.FindAssets("t:CardData", new[] { CardsFolder });
            foreach (string guid in allGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                CardData cd = AssetDatabase.LoadAssetAtPath<CardData>(path);
                if (cd == null || string.IsNullOrEmpty(cd.cardId) || cd == _database)
                    continue;

                if (AssetDatabase.IsValidFolder(LegacyFolder) && path.StartsWith(LegacyFolder + "/"))
                    _legacyCards.Add(cd);
                else
                    _allCards.Add(cd);
            }

            _allCards.Sort((a, b) => string.Compare(a.cardId, b.cardId, StringComparison.OrdinalIgnoreCase));
            _legacyCards.Sort((a, b) => string.Compare(a.cardId, b.cardId, StringComparison.OrdinalIgnoreCase));

            var allForDupes = new List<CardData>(_allCards);
            allForDupes.AddRange(_legacyCards);
            var dupes = allForDupes.GroupBy(c => c.cardId).Where(g => g.Count() > 1).ToList();
            foreach (var dupe in dupes)
                Debug.LogWarning($"[CardLibrary] Duplicate CardID: {dupe.Key} ({dupe.Count()} assets)");

            Debug.Log($"[CardLibrary] Found CardData assets: formal={_allCards.Count} legacy={_legacyCards.Count}");
            foreach (var card in _allCards)
                Debug.Log($"[CardLibrary] Formal: {card.cardId} {card.cardName}");
            foreach (var card in _legacyCards)
                Debug.Log($"[CardLibrary] Legacy: {card.cardId} {card.cardName}");
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            DrawToolbar();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            DrawLeftPanel();
            DrawRightPanel();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            DrawBottomBar();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawToolbar()
        {
            if (GUILayout.Button("Refresh", GUILayout.Width(70)))
                Refresh();

            GUI.SetNextControlName("SearchField");
            _searchText = EditorGUILayout.TextField(_searchText, EditorStyles.toolbarTextField, GUILayout.Width(180));
            if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
            {
                _searchText = "";
                GUI.FocusControl(null);
            }

            GUILayout.Space(10);

            _filterTypeIdx = EditorGUILayout.Popup("Type", _filterTypeIdx, TypeFilterNames, GUILayout.Width(150));
            _filterRarityIdx = EditorGUILayout.Popup("Rarity", _filterRarityIdx, RarityFilterNames, GUILayout.Width(150));
            _filterTargetIdx = EditorGUILayout.Popup("Target", _filterTargetIdx, TargetFilterNames, GUILayout.Width(120));

            string[] statusOpts = { "All", "Enabled", "Disabled", "Implemented", "Not Impl" };
            _filterStatus = EditorGUILayout.Popup(_filterStatus, statusOpts, GUILayout.Width(100));

            GUILayout.Space(8);
            _showLegacy = EditorGUILayout.Toggle("Show Legacy", _showLegacy, GUILayout.Width(110));

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Sync Database", GUILayout.Width(110)))
                SyncCardDatabase();

            if (GUILayout.Button("Import CSV", GUILayout.Width(90)))
                CardCsvImporter.Import();

            if (GUILayout.Button("Create New", GUILayout.Width(80)))
                CreateNewCard();
        }

        private void DrawLeftPanel()
        {
            var filtered = FilteredCards;

            int totalCount = _allCards.Count + (_showLegacy ? _legacyCards.Count : 0);
            EditorGUILayout.BeginVertical(GUILayout.Width(360));
            EditorGUILayout.LabelField($"Cards ({filtered.Count}/{totalCount})", EditorStyles.boldLabel);

            _listScroll = EditorGUILayout.BeginScrollView(_listScroll, GUILayout.ExpandHeight(true));
            for (int i = 0; i < filtered.Count; i++)
            {
                var card = filtered[i];
                if (card == null) continue;

                bool isLegacy = _legacyCards.Contains(card);
                bool selected = _selectedIndex >= 0 && _selectedIndex < _allCards.Count && _allCards[_selectedIndex] == card;
                if (!selected && isLegacy)
                    selected = _selectedIndex >= 0 && _selectedIndex < _legacyCards.Count && _legacyCards[_selectedIndex] == card;

                Color bg = selected ? new Color(0.3f, 0.5f, 0.6f, 1f) :
                    isLegacy ? new Color(0.3f, 0.2f, 0.1f, 1f) :
                    card.enabled ? new Color(0.15f, 0.18f, 0.22f, 1f) : new Color(0.22f, 0.12f, 0.12f, 1f);

                var rect = EditorGUILayout.BeginHorizontal();
                EditorGUI.DrawRect(rect, bg);

                string prefix = isLegacy ? "[Legacy] " : "";
                string atk = card.IsOffensive ? "[A]" : "[S]";
                string status = card.enabled ? (card.implemented ? "" : " (!)") : " (X)";
                string typeLabel = card.useTarget == CardUseTarget.Self ? "Self" : "Enemy";
                string label = $"{prefix}{card.cardId}  {card.cardName}  [{typeLabel}]  {card.cardType}/{card.rarity}{status}";

                GUI.color = isLegacy ? new Color(0.9f, 0.7f, 0.4f) :
                    card.enabled ? Color.white : new Color(0.6f, 0.4f, 0.4f);
                if (GUILayout.Button(label, EditorStyles.label, GUILayout.Height(20)))
                {
                    if (isLegacy)
                        _selectedIndex = _legacyCards.IndexOf(card);
                    else
                        _selectedIndex = _allCards.IndexOf(card);
                }
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawRightPanel()
        {
            EditorGUILayout.BeginVertical();

            CardData selected = null;

            if (_selectedIndex >= 0 && _showLegacy && _selectedIndex < _legacyCards.Count)
                selected = _legacyCards[_selectedIndex];
            else if (_selectedIndex >= 0 && _selectedIndex < _allCards.Count)
                selected = _allCards[_selectedIndex];

            if (selected == null)
            {
                EditorGUILayout.LabelField("Select a card from the list.", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.EndVertical();
                return;
            }

            bool isLegacy = _legacyCards.Contains(selected);

            _detailScroll = EditorGUILayout.BeginScrollView(_detailScroll, GUILayout.ExpandHeight(true));

            if (isLegacy)
            {
                var guiStyle = new GUIStyle(EditorStyles.label);
                guiStyle.normal.textColor = new Color(0.9f, 0.7f, 0.4f);
                EditorGUILayout.LabelField("[Legacy Card - Archived]", guiStyle);
            }

            EditorGUILayout.LabelField(selected.cardName, EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            EditorGUILayout.LabelField("Card ID", selected.cardId);
            EditorGUILayout.LabelField("Type", selected.useTarget == CardUseTarget.Self ? "Self" : "Enemy");
            EditorGUILayout.LabelField("Card Type", selected.cardType.ToString());
            EditorGUILayout.LabelField("Rarity", selected.rarity.ToString());
            EditorGUILayout.LabelField("Offensive", selected.IsOffensive ? "Yes [A]" : "No [S]");
            EditorGUILayout.LabelField("Good Cost", selected.goodCost.ToString());
            EditorGUILayout.LabelField("Evil Cost", selected.evilCost.ToString());
            EditorGUILayout.LabelField("Damage", selected.damage.ToString());
            EditorGUILayout.LabelField("Block", selected.block.ToString());
            EditorGUILayout.LabelField("Heal", selected.heal.ToString());
            EditorGUILayout.LabelField("Focus Gain", selected.focusGain.ToString());
            EditorGUILayout.LabelField("L Click", selected.leftClickEffect.ToString());
            EditorGUILayout.LabelField("R Click", selected.rightClickEffect.ToString());

            if (!string.IsNullOrEmpty(selected.baseEffectDescription))
                EditorGUILayout.LabelField("Effect", selected.baseEffectDescription);
            if (!string.IsNullOrEmpty(selected.finalValueRaw))
                EditorGUILayout.LabelField("Final Value", $"{selected.finalValueRaw} ({selected.finalValue})");
            if (!string.IsNullOrEmpty(selected.valueUnit))
                EditorGUILayout.LabelField("Unit", selected.valueUnit);
            if (!string.IsNullOrEmpty(selected.cooldownLimit))
                EditorGUILayout.LabelField("Cooldown/Limit", selected.cooldownLimit);
            if (!string.IsNullOrEmpty(selected.role))
                EditorGUILayout.LabelField("Role", selected.role);
            if (!string.IsNullOrEmpty(selected.riskNotes))
                EditorGUILayout.LabelField("Risk Notes", selected.riskNotes);
            if (!string.IsNullOrEmpty(selected.description))
                EditorGUILayout.LabelField("Description", selected.description);

            EditorGUILayout.Space(8);

            if (!isLegacy)
            {
                EditorGUI.BeginChangeCheck();
                bool enable = EditorGUILayout.Toggle("Enabled", selected.enabled);
                bool impl = EditorGUILayout.Toggle("Implemented", selected.implemented);
                if (EditorGUI.EndChangeCheck())
                {
                    selected.enabled = enable;
                    selected.implemented = impl;
                    EditorUtility.SetDirty(selected);
                    AssetDatabase.SaveAssets();
                }

                EditorGUILayout.Space(12);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Ping Asset", GUILayout.Width(100)))
                {
                    EditorGUIUtility.PingObject(selected);
                }
                if (GUILayout.Button("Select Asset", GUILayout.Width(100)))
                {
                    Selection.activeObject = selected;
                    EditorGUIUtility.PingObject(selected);
                }

                GUI.backgroundColor = new Color(0.8f, 0.4f, 0.2f);
                if (GUILayout.Button("Disable Card", GUILayout.Width(100)))
                {
                    selected.enabled = false;
                    EditorUtility.SetDirty(selected);
                    AssetDatabase.SaveAssets();
                }

                GUI.backgroundColor = new Color(0.7f, 0.5f, 0.3f);
                if (GUILayout.Button("Remove From DB", GUILayout.Width(120)))
                {
                    if (EditorUtility.DisplayDialog("Remove From Database",
                        $"Remove '{selected.cardName}' from CardDatabase only? Asset file will be kept.",
                        "Remove", "Cancel"))
                    {
                        if (_database != null)
                        {
                            _database.allCards.Remove(selected);
                            EditorUtility.SetDirty(_database);
                            AssetDatabase.SaveAssets();
                            Refresh();
                            _selectedIndex = -1;
                        }
                    }
                }

                GUI.backgroundColor = new Color(0.9f, 0.3f, 0.2f);
                if (GUILayout.Button("Delete Asset", GUILayout.Width(100)))
                {
                    if (EditorUtility.DisplayDialog("Delete Card Asset",
                        $"PERMANENTLY delete '{selected.cardName}' asset file?\nThis cannot be undone.",
                        "Delete", "Cancel"))
                    {
                        string path = AssetDatabase.GetAssetPath(selected);
                        if (_database != null)
                            _database.allCards.Remove(selected);
                        AssetDatabase.DeleteAsset(path);
                        AssetDatabase.SaveAssets();
                        Refresh();
                        _selectedIndex = -1;
                    }
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.LabelField("(Editing disabled for legacy archived cards)", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.Space(8);
                if (GUILayout.Button("Ping Asset", GUILayout.Width(100)))
                {
                    EditorGUIUtility.PingObject(selected);
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawBottomBar()
        {
            EditorGUILayout.LabelField($"Database: {(_database != null ? _database.allCards.Count + " cards" : "MISSING")}" +
                $"  Legacy: {_legacyCards.Count}",
                GUILayout.Width(300));

            if (_database == null)
            {
                if (GUILayout.Button("Create CardDatabase", GUILayout.Width(160)))
                {
                    if (!System.IO.Directory.Exists(CardsFolder))
                        System.IO.Directory.CreateDirectory(CardsFolder);
                    _database = ScriptableObject.CreateInstance<CardDatabase>();
                    AssetDatabase.CreateAsset(_database, DbPath);
                    AssetDatabase.SaveAssets();
                    Refresh();
                }
            }
        }

        private void SyncCardDatabase()
        {
            if (_database == null)
            {
                Debug.LogError("[CardLibrary] CardDatabase not found. Create one first.");
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:CardData", new[] { CardsFolder });
            _database.allCards.Clear();
            var cards = new List<CardData>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.IsValidFolder(LegacyFolder) && path.StartsWith(LegacyFolder + "/"))
                    continue;
                CardData cd = AssetDatabase.LoadAssetAtPath<CardData>(path);
                if (cd != null && !string.IsNullOrEmpty(cd.cardId))
                    cards.Add(cd);
            }
            cards.Sort((a, b) => string.Compare(a.cardId, b.cardId, StringComparison.OrdinalIgnoreCase));
            _database.allCards.AddRange(cards);
            EditorUtility.SetDirty(_database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Refresh();
            Debug.Log($"[CardLibrary] Synced CardDatabase. Count={_database.allCards.Count}");
        }

        private void CreateNewCard()
        {
            if (!System.IO.Directory.Exists(CardsFolder))
                System.IO.Directory.CreateDirectory(CardsFolder);

            string id = "NEW_" + System.DateTime.Now.ToString("HHmmss");
            string path = $"{CardsFolder}/{id}_NewCard.asset";

            var card = ScriptableObject.CreateInstance<CardData>();
            card.cardId = id;
            card.cardName = "New Card";
            card.cardType = CardType.Attack;
            card.rarity = CardRarity.Common;
            card.enabled = true;
            card.implemented = true;

            AssetDatabase.CreateAsset(card, path);
            AssetDatabase.SaveAssets();
            Refresh();

            if (_database != null)
            {
                _database.allCards.Add(card);
                EditorUtility.SetDirty(_database);
                AssetDatabase.SaveAssets();
            }

            _selectedIndex = _allCards.IndexOf(card);
            Selection.activeObject = card;
            EditorGUIUtility.PingObject(card);
            Debug.Log($"[CardLibrary] Created new card: {id}");
        }
    }
}
