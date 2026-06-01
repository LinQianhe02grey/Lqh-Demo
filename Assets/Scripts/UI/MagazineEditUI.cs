using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Cardwin.Cards;
using Cardwin.Magazine;
using Cardwin.Inventory;
using Cardwin.Combat;

namespace Cardwin.UI
{
    public enum BagTab
    {
        Magazine,
        Inventory,
        Fusion,
        Equipment,
        Preview
    }

    public class MagazineEditUI : MonoBehaviour
    {
        [Header("References")]
        public InventorySystem inventorySystem;
        public MagazineSystem magazineSystem;
        public CardDatabase cardDatabase;

        private GameObject _rootPanel;
        private Transform _ownedGridRoot;
        private Transform _loadoutGridRoot;
        private CanvasGroup _canvasGroup;
        private PlayerController2D _playerController;
        private bool _isOpen;

        public bool IsOpen => _isOpen;
        private Font _font;

        private List<CardData> _editingLoadout = new();
        private Dictionary<CardData, int> _editingOwnedCounts = new();
        private bool _hasPendingChanges;
        private Text _loadoutCountText;
        private PlayerAlignment _playerAlignment;
        private Text _alignmentText;

        private BagTab _currentTab = BagTab.Magazine;
        private Transform _contentRoot;
        private GameObject _magazinePage;
        private GameObject _inventoryPage;
        private GameObject _fusionPage;
        private GameObject _equipmentPage;
        private GameObject _previewPage;
        private GameObject _bottomButtonRow;
        private Dictionary<BagTab, Image> _tabButtons = new();

        private void Awake()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _playerController = FindObjectOfType<PlayerController2D>();
        }

        private void Start()
        {
            if (inventorySystem == null)
                inventorySystem = FindObjectOfType<InventorySystem>();
            if (magazineSystem == null)
                magazineSystem = FindObjectOfType<MagazineSystem>();

            EnsureEventSystem();
            EnsureUI();
        }

        private void Update()
        {
            if (_isOpen)
            {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.B))
                    CloseQuick();
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.B))
                    Toggle();
            }
        }

        private void CloseQuick()
        {
            if (!_isOpen)
                return;
            CancelEdit();
        }

        public void Toggle()
        {
            if (_isOpen)
                Close();
            else
                Open();
        }

        public void Open()
        {
            EnsureUI();

            if (inventorySystem != null)
            {
                _editingOwnedCounts.Clear();
                foreach (var entry in inventorySystem.GetCardCounts())
                {
                    if (entry.card != null && entry.count > 0)
                        _editingOwnedCounts[entry.card] = entry.count;
                }
            }

            if (magazineSystem != null)
            {
                _editingLoadout = new List<CardData>(magazineSystem.GetLoadoutCards());
            }
            else
            {
                _editingLoadout = new List<CardData>();
            }

            if (_playerAlignment == null)
            {
                _playerAlignment = _playerController != null
                    ? _playerController.GetComponent<PlayerAlignment>()
                    : FindObjectOfType<PlayerAlignment>();
                if (_playerAlignment == null)
                    Debug.LogError("[MagazineEditUI] Missing PlayerAlignment on Player.");
            }

            _hasPendingChanges = false;
            _currentTab = BagTab.Magazine;

            _isOpen = true;

            if (_rootPanel != null)
                _rootPanel.SetActive(true);
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }

            if (_playerController != null)
                _playerController.SetInputLocked(true);

            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            SwitchTab(BagTab.Magazine);

            Debug.Log($"[MagazineEditUI] Open edit panel. EditingLoadout={_editingLoadout.Count}");
        }

        public void Apply()
        {
            if (!_isOpen)
                return;

            int offensive = CountOffensiveCards(_editingLoadout);
            int requiredEvil = _playerAlignment != null ? _playerAlignment.Evil : 0;

            if (offensive != requiredEvil)
            {
                Debug.Log($"[MagazineEditUI] Apply blocked. Offensive={offensive}, RequiredEvil={requiredEvil}");
                if (_alignmentText != null)
                {
                    _alignmentText.text = $"Good: {(_playerAlignment != null ? _playerAlignment.Good : 0)}  Evil: {requiredEvil}\nAttack Bullets: {offensive} / {requiredEvil}  — Need exactly {requiredEvil} attack bullets!";
                    _alignmentText.color = Color.red;
                }
                return;
            }

            if (magazineSystem != null)
            {
                magazineSystem.SetLoadoutCards(_editingLoadout, rebuildImmediately: true);
                Debug.Log($"[MagazineEditUI] Apply loadout. Count={_editingLoadout.Count}");
            }

            if (inventorySystem != null)
            {
                inventorySystem.SetOwnedCardsFromCounts(_editingOwnedCounts);
            }

            _hasPendingChanges = false;

            Debug.Log("[MagazineEditUI] Apply complete.");
            Close();
        }

        private CardDatabase FindCardDatabaseInternal()
        {
            if (cardDatabase != null)
                return cardDatabase;
            if (magazineSystem != null && magazineSystem.cardDatabase != null)
                return magazineSystem.cardDatabase;

            CardDatabase db = FindObjectOfType<CardDatabase>();
            if (db != null)
                return db;

            CardDatabase[] all = Resources.FindObjectsOfTypeAll<CardDatabase>();
            if (all.Length > 0)
                return all[0];

            return null;
        }

        public void CancelEdit()
        {
            if (!_isOpen)
                return;

            Debug.Log("[MagazineEditUI] Cancel edit. Changes discarded.");
            Close();
        }

        public void ClearLoadout()
        {
            if (!_isOpen)
                return;

            foreach (CardData card in _editingLoadout)
            {
                if (card == null)
                    continue;

                if (!_editingOwnedCounts.ContainsKey(card))
                    _editingOwnedCounts[card] = 0;
                _editingOwnedCounts[card]++;
            }

            _editingLoadout.Clear();
            _hasPendingChanges = true;

            Debug.Log("[MagazineEditUI] Clear editing loadout.");
            Refresh();
        }

        public void AutoFill()
        {
            if (!_isOpen)
                return;

            if (_editingLoadout.Count >= 8)
            {
                Debug.Log("[MagazineEditUI] AutoFill: Loadout already full.");
                return;
            }

            int requiredEvil = _playerAlignment != null ? _playerAlignment.Evil : 0;

            List<CardData> offensivePool = new List<CardData>();
            List<CardData> nonOffensivePool = new List<CardData>();

            foreach (var kvp in _editingOwnedCounts)
            {
                if (kvp.Key == null || kvp.Value <= 0) continue;
                for (int i = 0; i < kvp.Value; i++)
                {
                    if (kvp.Key.IsOffensive)
                        offensivePool.Add(kvp.Key);
                    else
                        nonOffensivePool.Add(kvp.Key);
                }
            }

            if (offensivePool.Count == 0 && nonOffensivePool.Count == 0)
            {
                Debug.Log("[MagazineEditUI] AutoFill: No cards in inventory.");
                return;
            }

            int capacity = magazineSystem != null ? magazineSystem.Capacity : 8;
            int currentOffensive = CountOffensiveCards(_editingLoadout);

            while (_editingLoadout.Count < capacity)
            {
                List<CardData> pickFrom = null;
                if (currentOffensive < requiredEvil && offensivePool.Count > 0)
                    pickFrom = offensivePool;
                else if (nonOffensivePool.Count > 0)
                    pickFrom = nonOffensivePool;
                else if (offensivePool.Count > 0)
                    pickFrom = offensivePool;
                else
                    break;

                int r = UnityEngine.Random.Range(0, pickFrom.Count);
                CardData picked = pickFrom[r];
                pickFrom.RemoveAt(r);
                _editingLoadout.Add(picked);
                if (picked.IsOffensive) currentOffensive++;

                if (_editingOwnedCounts.ContainsKey(picked))
                {
                    _editingOwnedCounts[picked]--;
                    if (_editingOwnedCounts[picked] <= 0)
                        _editingOwnedCounts.Remove(picked);
                }
            }

            _hasPendingChanges = true;

            int finalOffensive = CountOffensiveCards(_editingLoadout);
            Debug.Log($"[MagazineEditUI] AutoFill by alignment. Offensive={finalOffensive}/{requiredEvil}, Total={_editingLoadout.Count}/{capacity}");
            Refresh();
        }

        private int CountOffensiveCards(List<CardData> cards)
        {
            if (cards == null) return 0;
            int count = 0;
            foreach (CardData c in cards)
                if (c != null && c.IsOffensive)
                    count++;
            return count;
        }

        public void Close()
        {
            if (_hasPendingChanges)
            {
                Debug.Log("[MagazineEditUI] Close without Apply. Changes discarded.");
            }

            _isOpen = false;
            _hasPendingChanges = false;
            _editingLoadout.Clear();
            _editingOwnedCounts.Clear();

            if (_rootPanel != null)
                _rootPanel.SetActive(false);
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }

            if (_playerController != null)
                _playerController.SetInputLocked(false);

            Time.timeScale = 1f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            Debug.Log("[MagazineEditUI] Close bag panel.");
        }

        private void Refresh()
        {
            if (!_isOpen)
                return;

            RefreshCurrentTab();

            int ownedTotal = 0;
            foreach (var kvp in _editingOwnedCounts)
                ownedTotal += kvp.Value;

            Debug.Log($"[MagazineEditUI] Refresh. Tab={_currentTab}, OwnedTotal={ownedTotal}, Loadout={_editingLoadout.Count}, Pending={_hasPendingChanges}");
        }

        private void RefreshOwnedCards()
        {
            if (_ownedGridRoot == null)
                return;

            for (int i = _ownedGridRoot.childCount - 1; i >= 0; i--)
                Destroy(_ownedGridRoot.GetChild(i).gameObject);

            if (_editingOwnedCounts.Count == 0)
            {
                CreateTextSlot(_ownedGridRoot, "[No cards owned]", Color.grey);
                return;
            }

            int idx = 0;
            foreach (var kvp in _editingOwnedCounts)
            {
                CardData card = kvp.Key;
                int cnt = kvp.Value;
                if (card == null || cnt <= 0)
                    continue;

                string label = $"{card.cardName} x{cnt}";
                string typeLabel = card.useTarget == CardUseTarget.Self ? "Self" : "Enemy";
                string costLabel = $"G{card.goodCost} / E{card.evilCost}";

                GameObject slot = new GameObject("OwnedCardSlot_" + idx, typeof(RectTransform));
                slot.transform.SetParent(_ownedGridRoot, false);
                Image bg = slot.AddComponent<Image>();
                bg.color = new Color(0.18f, 0.22f, 0.28f, 0.85f);
                bg.raycastTarget = true;

                Text nameText = CreateTextChild(slot.transform, "Name", label, 14, TextAnchor.MiddleCenter, Color.white);
                nameText.rectTransform.anchorMin = new Vector2(0f, 0.55f);
                nameText.rectTransform.anchorMax = new Vector2(1f, 1f);
                nameText.rectTransform.sizeDelta = new Vector2(0f, 26f);

                Text typeText = CreateTextChild(slot.transform, "Type", $"{typeLabel}  {costLabel}", 11, TextAnchor.MiddleCenter, new Color(0.6f, 0.7f, 0.8f));
                typeText.rectTransform.anchorMin = new Vector2(0f, 0f);
                typeText.rectTransform.anchorMax = new Vector2(1f, 0.45f);
                typeText.rectTransform.sizeDelta = new Vector2(0f, 20f);

                Button btn = slot.AddComponent<Button>();
                btn.targetGraphic = bg;
                ColorBlock cb = btn.colors;
                cb.normalColor = new Color(0.18f, 0.22f, 0.28f, 0.85f);
                cb.highlightedColor = new Color(0.25f, 0.35f, 0.45f, 0.9f);
                cb.pressedColor = new Color(0.12f, 0.16f, 0.22f, 0.95f);
                btn.colors = cb;

                CardData capturedCard = card;
                btn.onClick.AddListener(() => OnOwnedCardClicked(capturedCard));

                Debug.Log($"[MagazineEditUI] Created owned slot: {label}");
                idx++;
            }
        }

        private void RefreshLoadoutSlots()
        {
            if (_loadoutGridRoot == null)
                return;

            for (int i = _loadoutGridRoot.childCount - 1; i >= 0; i--)
                Destroy(_loadoutGridRoot.GetChild(i).gameObject);

            for (int i = 0; i < 8; i++)
            {
                GameObject slot = new GameObject("LoadoutSlot_" + i, typeof(RectTransform));
                slot.transform.SetParent(_loadoutGridRoot, false);
                Image bg = slot.AddComponent<Image>();

                if (i < _editingLoadout.Count && _editingLoadout[i] != null)
                {
                    CardData card = _editingLoadout[i];
                    bg.color = new Color(0.15f, 0.35f, 0.2f, 0.8f);
                    bg.raycastTarget = true;

                    Text nameText = CreateTextChild(slot.transform, "Name", card.cardName, 13, TextAnchor.MiddleCenter, new Color(0.85f, 0.95f, 0.85f));
                    nameText.rectTransform.anchorMin = new Vector2(0f, 0.5f);
                    nameText.rectTransform.anchorMax = new Vector2(1f, 0.5f);
                    nameText.rectTransform.sizeDelta = new Vector2(0f, 22f);

                    Text idxText = CreateTextChild(slot.transform, "Index", "[" + i + "]", 10, TextAnchor.MiddleCenter, new Color(0.4f, 0.6f, 0.4f));
                    idxText.rectTransform.anchorMin = new Vector2(0f, 0f);
                    idxText.rectTransform.anchorMax = new Vector2(1f, 0.35f);
                    idxText.rectTransform.anchoredPosition = new Vector2(0f, 2f);
                    idxText.rectTransform.sizeDelta = new Vector2(0f, 16f);

                    Button btn = slot.AddComponent<Button>();
                    btn.targetGraphic = bg;
                    ColorBlock cb = btn.colors;
                    cb.normalColor = new Color(0.15f, 0.35f, 0.2f, 0.8f);
                    cb.highlightedColor = new Color(0.25f, 0.5f, 0.3f, 0.9f);
                    cb.pressedColor = new Color(0.1f, 0.25f, 0.15f, 0.95f);
                    btn.colors = cb;

                    int capturedIndex = i;
                    btn.onClick.AddListener(() => OnLoadoutSlotClicked(capturedIndex));
                }
                else
                {
                    bg.color = new Color(0.12f, 0.12f, 0.15f, 0.5f);
                    bg.raycastTarget = true;

                    Text emptyText = CreateTextChild(slot.transform, "Empty", "-", 16, TextAnchor.MiddleCenter, new Color(0.25f, 0.25f, 0.3f));
                    emptyText.rectTransform.anchorMin = new Vector2(0f, 0.5f);
                    emptyText.rectTransform.anchorMax = new Vector2(1f, 0.5f);
                    emptyText.rectTransform.sizeDelta = new Vector2(0f, 24f);
                }
            }
        }

        private void OnOwnedCardClicked(CardData card)
        {
            if (card == null)
                return;

            if (_editingLoadout.Count >= 8)
            {
                Debug.Log("[MagazineEditUI] Loadout full.");
                return;
            }

            if (!_editingOwnedCounts.ContainsKey(card) || _editingOwnedCounts[card] <= 0)
            {
                Debug.Log($"[MagazineEditUI] No stock: {card.cardName}");
                return;
            }

            _editingOwnedCounts[card]--;
            if (_editingOwnedCounts[card] <= 0)
                _editingOwnedCounts.Remove(card);

            _editingLoadout.Add(card);
            _hasPendingChanges = true;

            Debug.Log($"[MagazineEditUI] Add {card.cardName} to editing loadout. EditingLoadout={_editingLoadout.Count}");
            Refresh();
        }

        private void OnLoadoutSlotClicked(int index)
        {
            if (index < 0 || index >= _editingLoadout.Count)
            {
                Debug.Log("[MagazineEditUI] Slot is empty.");
                return;
            }

            CardData removed = _editingLoadout[index];
            _editingLoadout.RemoveAt(index);

            if (!_editingOwnedCounts.ContainsKey(removed))
                _editingOwnedCounts[removed] = 0;
            _editingOwnedCounts[removed]++;

            _hasPendingChanges = true;

            Debug.Log($"[MagazineEditUI] Remove {removed.cardName} from editing loadout. EditingLoadout={_editingLoadout.Count}");
            Refresh();
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
                return;

            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
            Debug.Log("[MagazineEditUI] EventSystem created.");
        }

        private void EnsureUI()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
                canvas = FindObjectOfType<Canvas>();

            if (canvas == null)
            {
                Debug.LogError("[MagazineEditUI] No Canvas found in scene.");
                return;
            }

            if (_rootPanel != null)
            {
                for (int i = _rootPanel.transform.childCount - 1; i >= 0; i--)
                    DestroyImmediate(_rootPanel.transform.GetChild(i).gameObject);
                _ownedGridRoot = null;
                _loadoutGridRoot = null;
                _loadoutCountText = null;
                _tabButtons.Clear();
                _contentRoot = null;
                _magazinePage = null;
                _inventoryPage = null;
                _fusionPage = null;
                _equipmentPage = null;
                _previewPage = null;
                _bottomButtonRow = null;
            }
            else
            {
                _rootPanel = new GameObject("BagPanel", typeof(RectTransform));
                _rootPanel.transform.SetParent(canvas.transform, false);
                _rootPanel.SetActive(false);

                RectTransform rootRt = _rootPanel.GetComponent<RectTransform>();
                rootRt.anchorMin = new Vector2(0.5f, 0.5f);
                rootRt.anchorMax = new Vector2(0.5f, 0.5f);
                rootRt.pivot = new Vector2(0.5f, 0.5f);
                rootRt.anchoredPosition = Vector2.zero;
                rootRt.sizeDelta = new Vector2(1380f, 820f);

                Image panelBg = _rootPanel.AddComponent<Image>();
                panelBg.color = new Color(0.02f, 0.02f, 0.04f, 0.96f);

                _canvasGroup = _rootPanel.AddComponent<CanvasGroup>();
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }

            CreateBagPanelBackground();
            CreateTitleText();
            CreateTabRow();
            CreateContentRoot();
            CreateMagazinePage();
            CreateInventoryPage();
            CreateFusionPage();
            CreateEquipmentPage();
            CreatePreviewPage();
            CreateBottomButtonRow();
            CreateHintText();

            SwitchTab(_currentTab);

            Debug.Log("[MagazineEditUI] BagPanel created. Size=1380x820. ButtonRow ready. Buttons=4");
        }

        private void CreateBagPanelBackground()
        {
            GameObject bg = new GameObject("Background", typeof(RectTransform));
            bg.transform.SetParent(_rootPanel.transform, false);
            bg.transform.SetAsFirstSibling();
            RectTransform bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.02f, 0.02f, 0.04f, 1f);
            bgImg.raycastTarget = false;
        }

        private void CreateTitleText()
        {
            GameObject titleObj = new GameObject("TitleText", typeof(RectTransform));
            titleObj.transform.SetParent(_rootPanel.transform, false);
            RectTransform titleRt = titleObj.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 1f);
            titleRt.anchorMax = new Vector2(0.5f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -24f);
            titleRt.sizeDelta = new Vector2(800f, 40f);
            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "Cardwin Inventory";
            titleText.font = _font;
            titleText.fontSize = 24;
            titleText.color = Color.white;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.raycastTarget = false;
        }

        private void CreateTabRow()
        {
            GameObject tabRow = new GameObject("TabRow", typeof(RectTransform));
            tabRow.transform.SetParent(_rootPanel.transform, false);
            RectTransform tabRowRt = tabRow.GetComponent<RectTransform>();
            tabRowRt.anchorMin = new Vector2(0.5f, 1f);
            tabRowRt.anchorMax = new Vector2(0.5f, 1f);
            tabRowRt.pivot = new Vector2(0.5f, 1f);
            tabRowRt.anchoredPosition = new Vector2(0f, -76f);
            tabRowRt.sizeDelta = new Vector2(1100f, 48f);

            HorizontalLayoutGroup hlg = tabRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 14f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            string[] tabNames = { "Magazine", "Inventory", "Fusion", "Equipment", "Preview" };
            BagTab[] tabValues = { BagTab.Magazine, BagTab.Inventory, BagTab.Fusion, BagTab.Equipment, BagTab.Preview };

            for (int i = 0; i < tabNames.Length; i++)
            {
                string name = tabNames[i];
                BagTab tab = tabValues[i];
                GameObject tabBtn = CreateTabButton(tabRow.transform, name, tab);
            }
        }

        private GameObject CreateTabButton(Transform parent, string label, BagTab tab)
        {
            GameObject btnObj = new GameObject("Tab_" + label, typeof(RectTransform));
            btnObj.transform.SetParent(parent, false);
            RectTransform btnRt = btnObj.GetComponent<RectTransform>();
            btnRt.sizeDelta = new Vector2(180f, 42f);

            Image btnBg = btnObj.AddComponent<Image>();
            btnBg.color = new Color(0.2f, 0.22f, 0.28f, 1f);

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnBg;
            ColorBlock cb = btn.colors;
            cb.normalColor = new Color(0.2f, 0.22f, 0.28f, 1f);
            cb.highlightedColor = new Color(0.3f, 0.35f, 0.45f, 1f);
            cb.pressedColor = new Color(0.15f, 0.18f, 0.22f, 1f);
            btn.colors = cb;

            BagTab capturedTab = tab;
            btn.onClick.AddListener(() => SwitchTab(capturedTab));

            Text txt = CreateTextChild(btnObj.transform, "Text", label, 17, TextAnchor.MiddleCenter, Color.white);
            txt.rectTransform.anchorMin = Vector2.zero;
            txt.rectTransform.anchorMax = Vector2.one;
            txt.rectTransform.sizeDelta = Vector2.zero;

            _tabButtons[tab] = btnBg;
            return btnObj;
        }

        private void CreateContentRoot()
        {
            GameObject crObj = new GameObject("ContentRoot", typeof(RectTransform));
            crObj.transform.SetParent(_rootPanel.transform, false);
            _contentRoot = crObj.transform;
            RectTransform crRt = crObj.GetComponent<RectTransform>();
            crRt.anchorMin = new Vector2(0.5f, 0.5f);
            crRt.anchorMax = new Vector2(0.5f, 0.5f);
            crRt.pivot = new Vector2(0.5f, 0.5f);
            crRt.anchoredPosition = new Vector2(0f, -10f);
            crRt.sizeDelta = new Vector2(1260f, 610f);
        }

        private void CreateMagazinePage()
        {
            Transform contentRoot = _contentRoot;
            if (contentRoot == null) return;

            _magazinePage = new GameObject("MagazinePage", typeof(RectTransform));
            _magazinePage.transform.SetParent(contentRoot, false);
            RectTransform pageRt = _magazinePage.GetComponent<RectTransform>();
            pageRt.anchorMin = Vector2.zero;
            pageRt.anchorMax = Vector2.one;
            pageRt.offsetMin = Vector2.zero;
            pageRt.offsetMax = Vector2.zero;

            // Left: OwnedCardsPanel
            Text ownedTitle = CreateTextChild(_magazinePage.transform, "OwnedTitle", "Owned Cards", 18, TextAnchor.MiddleLeft, new Color(0.6f, 0.85f, 1f));
            RectTransform ownedTitleRt = ownedTitle.rectTransform;
            ownedTitleRt.anchorMin = new Vector2(0.05f, 1f);
            ownedTitleRt.anchorMax = new Vector2(0.45f, 1f);
            ownedTitleRt.pivot = new Vector2(0f, 1f);
            ownedTitleRt.anchoredPosition = new Vector2(5f, -5f);
            ownedTitleRt.sizeDelta = new Vector2(200f, 24f);

            GameObject ownedPanel = new GameObject("OwnedCardsPanel", typeof(RectTransform));
            ownedPanel.transform.SetParent(_magazinePage.transform, false);
            RectTransform ownedRt = ownedPanel.GetComponent<RectTransform>();
            ownedRt.anchorMin = new Vector2(0.5f, 0.5f);
            ownedRt.anchorMax = new Vector2(0.5f, 0.5f);
            ownedRt.pivot = new Vector2(0.5f, 0.5f);
            ownedRt.anchoredPosition = new Vector2(-360f, 0f);
            ownedRt.sizeDelta = new Vector2(540f, 500f);
            Image ownedBg = ownedPanel.AddComponent<Image>();
            ownedBg.color = new Color(0.06f, 0.06f, 0.08f, 0.8f);
            ownedBg.raycastTarget = false;

            ScrollRect scrollRect = ownedPanel.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 30f;

            GameObject viewport = new GameObject("Viewport", typeof(RectTransform));
            viewport.transform.SetParent(ownedPanel.transform, false);
            RectTransform vpRt = viewport.GetComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero;
            vpRt.anchorMax = Vector2.one;
            vpRt.offsetMin = Vector2.zero;
            vpRt.offsetMax = Vector2.zero;
            Image vpMask = viewport.AddComponent<Image>();
            vpMask.color = new Color(0.05f, 0.05f, 0.07f, 0.3f);
            viewport.AddComponent<Mask>();

            GameObject content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = new Vector2(0f, 100f);

            GridLayoutGroup ownedGrid = content.AddComponent<GridLayoutGroup>();
            ownedGrid.cellSize = new Vector2(230f, 70f);
            ownedGrid.spacing = new Vector2(10f, 8f);
            ownedGrid.padding = new RectOffset(10, 10, 10, 10);
            ownedGrid.childAlignment = TextAnchor.UpperLeft;
            ownedGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            ownedGrid.constraintCount = 2;

            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = vpRt;
            scrollRect.content = contentRt;
            _ownedGridRoot = content.transform;

            // Right: LoadoutPanel
            Text loadoutTitle = CreateTextChild(_magazinePage.transform, "LoadoutTitle", "Loadout", 18, TextAnchor.MiddleLeft, new Color(0.6f, 1f, 0.6f));
            RectTransform loadoutTitleRt = loadoutTitle.rectTransform;
            loadoutTitleRt.anchorMin = new Vector2(0.55f, 1f);
            loadoutTitleRt.anchorMax = new Vector2(0.95f, 1f);
            loadoutTitleRt.pivot = new Vector2(0f, 1f);
            loadoutTitleRt.anchoredPosition = new Vector2(5f, -5f);
            loadoutTitleRt.sizeDelta = new Vector2(200f, 24f);

            GameObject loadoutPanel = new GameObject("LoadoutPanel", typeof(RectTransform));
            loadoutPanel.transform.SetParent(_magazinePage.transform, false);
            RectTransform loadoutRt = loadoutPanel.GetComponent<RectTransform>();
            loadoutRt.anchorMin = new Vector2(0.5f, 0.5f);
            loadoutRt.anchorMax = new Vector2(0.5f, 0.5f);
            loadoutRt.pivot = new Vector2(0.5f, 0.5f);
            loadoutRt.anchoredPosition = new Vector2(360f, 0f);
            loadoutRt.sizeDelta = new Vector2(540f, 500f);
            Image loadoutBg = loadoutPanel.AddComponent<Image>();
            loadoutBg.color = new Color(0.06f, 0.06f, 0.08f, 0.8f);
            loadoutBg.raycastTarget = false;

            GridLayoutGroup loadoutGrid = loadoutPanel.AddComponent<GridLayoutGroup>();
            loadoutGrid.cellSize = new Vector2(190f, 80f);
            loadoutGrid.spacing = new Vector2(14f, 14f);
            loadoutGrid.padding = new RectOffset(15, 15, 15, 15);
            loadoutGrid.childAlignment = TextAnchor.UpperCenter;
            loadoutGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            loadoutGrid.constraintCount = 2;
            _loadoutGridRoot = loadoutPanel.transform;

            // Alignment display
            GameObject alignObj = new GameObject("AlignmentText", typeof(RectTransform));
            alignObj.transform.SetParent(_magazinePage.transform, false);
            RectTransform alignRt = alignObj.GetComponent<RectTransform>();
            alignRt.anchorMin = new Vector2(0.5f, 1f);
            alignRt.anchorMax = new Vector2(0.5f, 1f);
            alignRt.pivot = new Vector2(0.5f, 1f);
            alignRt.anchoredPosition = new Vector2(0f, -35f);
            alignRt.sizeDelta = new Vector2(500f, 40f);
            _alignmentText = alignObj.AddComponent<Text>();
            _alignmentText.text = "Good: 4  Evil: 4\nAttack Bullets: 0 / 4";
            _alignmentText.font = _font;
            _alignmentText.fontSize = 14;
            _alignmentText.color = Color.white;
            _alignmentText.alignment = TextAnchor.MiddleCenter;
            _alignmentText.raycastTarget = false;

            // Loadout count text
            GameObject countObj = new GameObject("LoadoutCount", typeof(RectTransform));
            countObj.transform.SetParent(_magazinePage.transform, false);
            RectTransform countRt = countObj.GetComponent<RectTransform>();
            countRt.anchorMin = new Vector2(0.5f, 0f);
            countRt.anchorMax = new Vector2(0.5f, 0f);
            countRt.pivot = new Vector2(0.5f, 0f);
            countRt.anchoredPosition = new Vector2(290f, 6f);
            countRt.sizeDelta = new Vector2(500f, 24f);
            _loadoutCountText = countObj.AddComponent<Text>();
            _loadoutCountText.text = "Loadout 0/8";
            _loadoutCountText.font = _font;
            _loadoutCountText.fontSize = 16;
            _loadoutCountText.color = new Color(0.55f, 0.55f, 0.55f);
            _loadoutCountText.alignment = TextAnchor.MiddleCenter;
            _loadoutCountText.raycastTarget = false;
        }

        private void CreateInventoryPage()
        {
            Transform contentRoot = _contentRoot;
            if (contentRoot == null) return;

            _inventoryPage = new GameObject("InventoryPage", typeof(RectTransform));
            _inventoryPage.transform.SetParent(contentRoot, false);
            RectTransform pageRt = _inventoryPage.GetComponent<RectTransform>();
            pageRt.anchorMin = Vector2.zero;
            pageRt.anchorMax = Vector2.one;
            pageRt.offsetMin = Vector2.zero;
            pageRt.offsetMax = Vector2.zero;

            Text title = CreateTextChild(_inventoryPage.transform, "Title", "Inventory — Owned Cards", 22, TextAnchor.UpperCenter, new Color(0.6f, 0.85f, 1f));
            RectTransform titleRt = title.rectTransform;
            titleRt.anchorMin = new Vector2(0.5f, 1f);
            titleRt.anchorMax = new Vector2(0.5f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -10f);
            titleRt.sizeDelta = new Vector2(600f, 30f);

            GameObject invGrid = new GameObject("InvGrid", typeof(RectTransform));
            invGrid.transform.SetParent(_inventoryPage.transform, false);
            RectTransform invGridRt = invGrid.GetComponent<RectTransform>();
            invGridRt.anchorMin = new Vector2(0.5f, 0.5f);
            invGridRt.anchorMax = new Vector2(0.5f, 0.5f);
            invGridRt.pivot = new Vector2(0.5f, 0.5f);
            invGridRt.anchoredPosition = Vector2.zero;
            invGridRt.sizeDelta = new Vector2(900f, 520f);

            GridLayoutGroup grid = invGrid.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(240f, 90f);
            grid.spacing = new Vector2(24f, 16f);
            grid.padding = new RectOffset(20, 20, 20, 20);
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;

            Transform invGridRoot = invGrid.transform;
            var entries = inventorySystem != null ? inventorySystem.GetCardCounts() : new List<InventoryEntry>();
            foreach (var entry in entries)
            {
                if (entry.card == null || entry.count <= 0) continue;
                CreateReadOnlyCardSlot(invGridRoot, entry.card, entry.count);
            }
        }

        private void CreateReadOnlyCardSlot(Transform parent, CardData card, int count)
        {
            GameObject slot = new GameObject("CardSlot_" + card.cardName, typeof(RectTransform));
            slot.transform.SetParent(parent, false);

            Image bg = slot.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.18f, 0.22f, 0.85f);
            bg.raycastTarget = false;

            Text nameText = CreateTextChild(slot.transform, "Name", card.cardName + " x" + count, 16, TextAnchor.MiddleCenter, Color.white);
            nameText.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            nameText.rectTransform.anchorMax = new Vector2(1f, 0.5f);
            nameText.rectTransform.sizeDelta = new Vector2(0f, 28f);

            string eff = CardSlotUI.EffectToShortPublic(card.leftClickEffect) + "/" + CardSlotUI.EffectToShortPublic(card.rightClickEffect);
            Text effText = CreateTextChild(slot.transform, "Effect", eff, 11, TextAnchor.MiddleCenter, new Color(0.5f, 0.55f, 0.6f));
            effText.rectTransform.anchorMin = new Vector2(0f, 0f);
            effText.rectTransform.anchorMax = new Vector2(1f, 0.4f);
            effText.rectTransform.anchoredPosition = new Vector2(0f, 4f);
            effText.rectTransform.sizeDelta = new Vector2(0f, 20f);
        }

        private void CreateFusionPage()
        {
            Transform contentRoot = _contentRoot;
            if (contentRoot == null) return;

            _fusionPage = new GameObject("FusionPage", typeof(RectTransform));
            _fusionPage.transform.SetParent(contentRoot, false);
            RectTransform pageRt = _fusionPage.GetComponent<RectTransform>();
            pageRt.anchorMin = Vector2.zero;
            pageRt.anchorMax = Vector2.one;
            pageRt.offsetMin = Vector2.zero;
            pageRt.offsetMax = Vector2.zero;

            CreatePagePlaceholder(_fusionPage.transform, "Fusion system coming later.", "Future: combine duplicate cards into upgraded bullets.");
        }

        private void CreateEquipmentPage()
        {
            Transform contentRoot = _contentRoot;
            if (contentRoot == null) return;

            _equipmentPage = new GameObject("EquipmentPage", typeof(RectTransform));
            _equipmentPage.transform.SetParent(contentRoot, false);
            RectTransform pageRt = _equipmentPage.GetComponent<RectTransform>();
            pageRt.anchorMin = Vector2.zero;
            pageRt.anchorMax = Vector2.one;
            pageRt.offsetMin = Vector2.zero;
            pageRt.offsetMax = Vector2.zero;

            CreatePagePlaceholder(_equipmentPage.transform, "Equipment system coming later.", "Future: equip passive modules or relic-like items.");
        }

        private void CreatePreviewPage()
        {
            Transform contentRoot = _contentRoot;
            if (contentRoot == null) return;

            _previewPage = new GameObject("PreviewPage", typeof(RectTransform));
            _previewPage.transform.SetParent(contentRoot, false);
            RectTransform pageRt = _previewPage.GetComponent<RectTransform>();
            pageRt.anchorMin = Vector2.zero;
            pageRt.anchorMax = Vector2.one;
            pageRt.offsetMin = Vector2.zero;
            pageRt.offsetMax = Vector2.zero;

            Text title = CreateTextChild(_previewPage.transform, "Title", "Current Loadout & Magazine Preview", 20, TextAnchor.UpperCenter, new Color(0.8f, 0.8f, 0.4f));
            RectTransform titleRt = title.rectTransform;
            titleRt.anchorMin = new Vector2(0.5f, 1f);
            titleRt.anchorMax = new Vector2(0.5f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -10f);
            titleRt.sizeDelta = new Vector2(600f, 28f);

            // Info will be refreshed in RefreshCurrentTab
            Text infoText = CreateTextChild(_previewPage.transform, "Info", "", 16, TextAnchor.UpperLeft, new Color(0.7f, 0.7f, 0.7f));
            infoText.rectTransform.anchorMin = new Vector2(0.1f, 0.15f);
            infoText.rectTransform.anchorMax = new Vector2(0.9f, 0.85f);
            infoText.rectTransform.offsetMin = Vector2.zero;
            infoText.rectTransform.offsetMax = Vector2.zero;
        }

        private void CreatePagePlaceholder(Transform parent, string title, string subtitle)
        {
            Text titleText = CreateTextChild(parent, "Title", title, 26, TextAnchor.MiddleCenter, new Color(0.5f, 0.5f, 0.55f));
            RectTransform titleRt = titleText.rectTransform;
            titleRt.anchorMin = new Vector2(0.5f, 0.6f);
            titleRt.anchorMax = new Vector2(0.5f, 0.6f);
            titleRt.pivot = new Vector2(0.5f, 0.5f);
            titleRt.sizeDelta = new Vector2(800f, 40f);

            Text subText = CreateTextChild(parent, "Subtitle", subtitle, 16, TextAnchor.MiddleCenter, new Color(0.35f, 0.35f, 0.4f));
            RectTransform subRt = subText.rectTransform;
            subRt.anchorMin = new Vector2(0.5f, 0.45f);
            subRt.anchorMax = new Vector2(0.5f, 0.45f);
            subRt.pivot = new Vector2(0.5f, 0.5f);
            subRt.sizeDelta = new Vector2(800f, 24f);
        }

        private void CreateBottomButtonRow()
        {
            _bottomButtonRow = new GameObject("BottomButtonRow", typeof(RectTransform));
            _bottomButtonRow.transform.SetParent(_rootPanel.transform, false);
            RectTransform rowRt = _bottomButtonRow.GetComponent<RectTransform>();
            rowRt.anchorMin = new Vector2(0.5f, 0f);
            rowRt.anchorMax = new Vector2(0.5f, 0f);
            rowRt.pivot = new Vector2(0.5f, 0f);
            rowRt.anchoredPosition = new Vector2(0f, 66f);
            rowRt.sizeDelta = new Vector2(820f, 52f);

            HorizontalLayoutGroup hlg = _bottomButtonRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            CreateActionButton(_bottomButtonRow.transform, "Clear", () => { Debug.Log("[MagazineEditUI] Clear clicked."); ClearLoadout(); });
            CreateActionButton(_bottomButtonRow.transform, "Auto Fill", () => { Debug.Log("[MagazineEditUI] Auto Fill clicked."); AutoFill(); });
            CreateActionButton(_bottomButtonRow.transform, "Apply", () => { Debug.Log("[MagazineEditUI] Apply clicked."); Apply(); });
            CreateActionButton(_bottomButtonRow.transform, "Cancel", () => { Debug.Log("[MagazineEditUI] Cancel clicked."); CancelEdit(); });
        }

        private void CreateHintText()
        {
            GameObject hintObj = new GameObject("HintText", typeof(RectTransform));
            hintObj.transform.SetParent(_rootPanel.transform, false);
            RectTransform hintRt = hintObj.GetComponent<RectTransform>();
            hintRt.anchorMin = new Vector2(0.5f, 0f);
            hintRt.anchorMax = new Vector2(0.5f, 0f);
            hintRt.pivot = new Vector2(0.5f, 0f);
            hintRt.anchoredPosition = new Vector2(0f, 28f);
            hintRt.sizeDelta = new Vector2(1000f, 24f);
            Text hintText = hintObj.AddComponent<Text>();
            hintText.text = "Magazine: click owned card to edit loadout. Apply saves. Cancel/B/Esc discards.";
            hintText.font = _font;
            hintText.fontSize = 12;
            hintText.color = new Color(0.35f, 0.35f, 0.4f);
            hintText.alignment = TextAnchor.MiddleCenter;
            hintText.raycastTarget = false;
        }

        private void SwitchTab(BagTab tab)
        {
            _currentTab = tab;

            if (_magazinePage != null) _magazinePage.SetActive(tab == BagTab.Magazine);
            if (_inventoryPage != null) _inventoryPage.SetActive(tab == BagTab.Inventory);
            if (_fusionPage != null) _fusionPage.SetActive(tab == BagTab.Fusion);
            if (_equipmentPage != null) _equipmentPage.SetActive(tab == BagTab.Equipment);
            if (_previewPage != null) _previewPage.SetActive(tab == BagTab.Preview);

            if (_bottomButtonRow != null)
            {
                bool showEditButtons = tab == BagTab.Magazine;
                _bottomButtonRow.SetActive(showEditButtons);
            }

            RefreshTabButtons();

            if (_isOpen)
                RefreshCurrentTab();

            Debug.Log($"[MagazineEditUI] Switch tab: {tab}");
        }

        private void RefreshCurrentTab()
        {
            switch (_currentTab)
            {
                case BagTab.Magazine:
                    RefreshOwnedCards();
                    RefreshLoadoutSlots();
                    if (_loadoutCountText != null)
                    {
                        int cap = magazineSystem != null ? magazineSystem.Capacity : 8;
                        string pending = _hasPendingChanges ? " *" : "";
                        _loadoutCountText.text = $"Loadout {_editingLoadout.Count}/{cap}{pending}";
                    }
                    if (_alignmentText != null)
                    {
                        int offensive = CountOffensiveCards(_editingLoadout);
                        int good = _playerAlignment != null ? _playerAlignment.Good : 0;
                        int evil = _playerAlignment != null ? _playerAlignment.Evil : 0;
                        bool valid = offensive == evil;
                        _alignmentText.text = $"Good: {good}  Evil: {evil}\nAttack Bullets: {offensive} / {evil}" + (valid ? "" : $"  — Need exactly {evil} attack bullets!");
                        _alignmentText.color = valid ? Color.white : new Color(1f, 0.6f, 0.3f);
                    }
                    break;
                case BagTab.Preview:
                    RefreshPreviewPage();
                    break;
            }
        }

        private void RefreshPreviewPage()
        {
            if (_previewPage == null) return;
            Transform infoTransform = _previewPage.transform.Find("Info");
            if (infoTransform == null) return;
            Text infoText = infoTransform.GetComponent<Text>();
            if (infoText == null) return;

            string info = "";
            if (magazineSystem != null)
            {
                var loadout = magazineSystem.GetLoadoutCards();
                var loaded = magazineSystem.GetLoadedCards();

                info += $"Loadout Count: {loadout.Count}/{magazineSystem.Capacity}\n\n";
                info += "Loadout:\n";
                for (int i = 0; i < loadout.Count; i++)
                    info += $"  [{i}] {loadout[i]?.cardName ?? "null"}\n";

                info += "\nLoaded Preview (first 3):\n";
                for (int i = 0; i < 3; i++)
                {
                    if (i < loaded.Count && loaded[i] != null)
                    {
                        string mark = i == magazineSystem.CurrentIndex ? " > " : "   ";
                        info += $"{mark}[{i}] {loaded[i].cardName}\n";
                    }
                    else
                    {
                        info += $"   [{i}] Empty\n";
                    }
                }

                info += "\nSource: loadoutCards";
            }
            else
            {
                info = "MagazineSystem not found.";
            }

            infoText.text = info;
        }

        private void RefreshTabButtons()
        {
            Color activeColor = new Color(0.2f, 0.45f, 0.55f, 1f);
            Color inactiveColor = new Color(0.2f, 0.22f, 0.28f, 1f);

            foreach (var kvp in _tabButtons)
            {
                kvp.Value.color = kvp.Key == _currentTab ? activeColor : inactiveColor;
            }
        }

        private void CreateTextSlot(Transform parent, string text, Color color)
        {
            GameObject go = new GameObject("EmptyHint", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            Text txt = go.AddComponent<Text>();
            txt.text = text;
            txt.font = _font;
            txt.fontSize = 14;
            txt.color = color;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.raycastTarget = false;
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(200f, 40f);
        }

        private Text CreateTextChild(Transform parent, string name, string content, int fontSize, TextAnchor align, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            Text txt = go.AddComponent<Text>();
            txt.text = content;
            txt.font = _font;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = align;
            txt.raycastTarget = false;
            return txt;
        }

        private void CreateActionButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject("Btn_" + label.Replace(" ", ""), typeof(RectTransform));
            btnObj.transform.SetParent(parent, false);

            RectTransform btnRt = btnObj.GetComponent<RectTransform>();
            btnRt.sizeDelta = new Vector2(170f, 42f);

            Image btnBg = btnObj.AddComponent<Image>();
            Color bgColor;
            switch (label)
            {
                case "Apply":
                    bgColor = new Color(0.15f, 0.35f, 0.2f, 0.9f);
                    break;
                case "Cancel":
                    bgColor = new Color(0.35f, 0.15f, 0.15f, 0.9f);
                    break;
                case "Clear":
                    bgColor = new Color(0.3f, 0.2f, 0.1f, 0.9f);
                    break;
                default:
                    bgColor = new Color(0.2f, 0.25f, 0.35f, 0.9f);
                    break;
            }
            btnBg.color = bgColor;

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnBg;
            ColorBlock cb = btn.colors;
            cb.normalColor = bgColor;
            cb.highlightedColor = bgColor * 1.3f;
            cb.pressedColor = bgColor * 0.7f;
            btn.colors = cb;
            btn.onClick.AddListener(onClick);

            Text btnText = CreateTextChild(btnObj.transform, "Text", label, 17, TextAnchor.MiddleCenter, Color.white);
            btnText.rectTransform.anchorMin = Vector2.zero;
            btnText.rectTransform.anchorMax = Vector2.one;
            btnText.rectTransform.sizeDelta = Vector2.zero;
        }
    }
}
