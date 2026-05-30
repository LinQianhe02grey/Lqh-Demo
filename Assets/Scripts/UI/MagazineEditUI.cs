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
        private Font _font;

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
            Close();
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

            CardDatabase db = FindCardDatabase();

            if (inventorySystem != null)
                inventorySystem.ResetToTestStock(db);

            if (magazineSystem != null)
                magazineSystem.InitializeDefaultLoadoutIfEmpty(db);

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

            Refresh();

            Debug.Log("[MagazineEditUI] Open bag panel.");
        }

        private CardDatabase FindCardDatabase()
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

            Debug.LogError("[MagazineEditUI] CardDatabase not found anywhere.");
            return null;
        }

        public void Close()
        {
            _isOpen = false;

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

            RefreshOwnedCards();
            RefreshLoadoutSlots();

            int ownedTotal = inventorySystem != null ? inventorySystem.GetOwnedCards().Count : 0;
            int ownedEntries = inventorySystem != null ? inventorySystem.GetCardCounts().Count : 0;
            int loadoutCount = magazineSystem != null ? magazineSystem.GetLoadoutCards().Count : 0;
            Debug.Log($"[MagazineEditUI] Refresh. OwnedTotal={ownedTotal}, OwnedEntries={ownedEntries}, Loadout={loadoutCount}");
        }

        private void RefreshOwnedCards()
        {
            if (_ownedGridRoot == null)
                return;

            for (int i = _ownedGridRoot.childCount - 1; i >= 0; i--)
                Destroy(_ownedGridRoot.GetChild(i).gameObject);

            if (inventorySystem == null)
            {
                Debug.LogWarning("[MagazineEditUI] InventorySystem is null.");
                return;
            }

            List<InventoryEntry> entries = inventorySystem.GetCardCounts();

            if (entries.Count == 0)
            {
                CreateTextSlot(_ownedGridRoot, "[No cards owned]", Color.grey);
                return;
            }

            Debug.Log($"[MagazineEditUI] OwnedCards count entries={entries.Count}");

            int idx = 0;
            foreach (InventoryEntry entry in entries)
            {
                CardData card = entry.card;
                int cnt = entry.count;
                string label = card.cardName + " x" + cnt;

                GameObject slot = new GameObject("OwnedCardSlot_" + idx, typeof(RectTransform));
                slot.transform.SetParent(_ownedGridRoot, false);
                Image bg = slot.AddComponent<Image>();
                bg.color = new Color(0.18f, 0.22f, 0.28f, 0.85f);
                bg.raycastTarget = true;

                Text nameText = CreateTextChild(slot.transform, "Name", label, 14, TextAnchor.MiddleCenter, Color.white);
                nameText.rectTransform.anchorMin = new Vector2(0f, 0.5f);
                nameText.rectTransform.anchorMax = new Vector2(1f, 0.5f);
                nameText.rectTransform.sizeDelta = new Vector2(0f, 24f);

                string eff = CardSlotUI.EffectToShortPublic(card.leftClickEffect) + "/" + CardSlotUI.EffectToShortPublic(card.rightClickEffect);
                Text effText = CreateTextChild(slot.transform, "Effect", eff, 10, TextAnchor.MiddleCenter, new Color(0.55f, 0.6f, 0.7f));
                effText.rectTransform.anchorMin = new Vector2(0f, 0f);
                effText.rectTransform.anchorMax = new Vector2(1f, 0.4f);
                effText.rectTransform.anchoredPosition = new Vector2(0f, 2f);
                effText.rectTransform.sizeDelta = new Vector2(0f, 18f);

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

            if (magazineSystem == null)
            {
                Debug.LogWarning("[MagazineEditUI] MagazineSystem is null.");
                return;
            }

            List<CardData> loadout = magazineSystem.GetLoadoutCards();

            for (int i = 0; i < 8; i++)
            {
                GameObject slot = new GameObject("LoadoutSlot_" + i, typeof(RectTransform));
                slot.transform.SetParent(_loadoutGridRoot, false);
                Image bg = slot.AddComponent<Image>();

                if (i < loadout.Count && loadout[i] != null)
                {
                    CardData card = loadout[i];
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
            if (magazineSystem == null)
            {
                Debug.LogError("[MagazineEditUI] MagazineSystem is null.");
                return;
            }
            if (inventorySystem == null)
            {
                Debug.LogError("[MagazineEditUI] InventorySystem is null.");
                return;
            }

            if (inventorySystem.GetCount(card) <= 0)
            {
                Debug.Log($"[MagazineEditUI] No {card.cardName} remaining in inventory.");
                return;
            }

            List<CardData> currentLoadout = new List<CardData>(magazineSystem.GetLoadoutCards());
            if (currentLoadout.Count >= 8)
            {
                Debug.Log("[MagazineEditUI] Loadout full.");
                return;
            }

            inventorySystem.RemoveCard(card);
            currentLoadout.Add(card);
            magazineSystem.SetLoadoutCards(currentLoadout);

            int left = inventorySystem.GetCount(card);
            Debug.Log($"[MagazineEditUI] Add {card.cardName} to loadout. Inventory left={left}");
            Refresh();
        }

        private void OnLoadoutSlotClicked(int index)
        {
            if (magazineSystem == null)
            {
                Debug.LogError("[MagazineEditUI] MagazineSystem is null.");
                return;
            }
            if (inventorySystem == null)
            {
                Debug.LogError("[MagazineEditUI] InventorySystem is null.");
                return;
            }

            List<CardData> currentLoadout = new List<CardData>(magazineSystem.GetLoadoutCards());
            if (index < 0 || index >= currentLoadout.Count)
            {
                Debug.Log("[MagazineEditUI] Slot is empty.");
                return;
            }

            CardData removed = currentLoadout[index];
            currentLoadout.RemoveAt(index);
            magazineSystem.SetLoadoutCards(currentLoadout);

            inventorySystem.AddCard(removed);
            int now = inventorySystem.GetCount(removed);
            Debug.Log($"[MagazineEditUI] Remove {removed?.cardName ?? "null"} from loadout. Inventory now={now}");
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
                return;

            _rootPanel = new GameObject("BagPanel", typeof(RectTransform));
            _rootPanel.transform.SetParent(canvas.transform, false);
            _rootPanel.SetActive(false);

            RectTransform rootRt = _rootPanel.GetComponent<RectTransform>();
            rootRt.anchorMin = new Vector2(0.5f, 0.5f);
            rootRt.anchorMax = new Vector2(0.5f, 0.5f);
            rootRt.pivot = new Vector2(0.5f, 0.5f);
            rootRt.anchoredPosition = Vector2.zero;
            rootRt.sizeDelta = new Vector2(900f, 520f);

            Image panelBg = _rootPanel.AddComponent<Image>();
            panelBg.color = new Color(0.04f, 0.04f, 0.06f, 0.95f);

            _canvasGroup = _rootPanel.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            GameObject titleObj = new GameObject("TitleText", typeof(RectTransform));
            titleObj.transform.SetParent(_rootPanel.transform, false);
            RectTransform titleRt = titleObj.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 1f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -10f);
            titleRt.sizeDelta = new Vector2(0f, 36f);
            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "Inventory / Magazine Edit";
            titleText.font = _font;
            titleText.fontSize = 24;
            titleText.color = Color.white;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.raycastTarget = false;

            GameObject ownedLabel = new GameObject("OwnedTitle", typeof(RectTransform));
            ownedLabel.transform.SetParent(_rootPanel.transform, false);
            RectTransform ownedLabelRt = ownedLabel.GetComponent<RectTransform>();
            ownedLabelRt.anchorMin = new Vector2(0.025f, 0.88f);
            ownedLabelRt.anchorMax = new Vector2(0.46f, 0.93f);
            ownedLabelRt.offsetMin = Vector2.zero;
            ownedLabelRt.offsetMax = Vector2.zero;
            Text ownedLabelText = ownedLabel.AddComponent<Text>();
            ownedLabelText.text = "Owned Cards";
            ownedLabelText.font = _font;
            ownedLabelText.fontSize = 16;
            ownedLabelText.color = new Color(0.6f, 0.85f, 1f);
            ownedLabelText.alignment = TextAnchor.MiddleLeft;
            ownedLabelText.raycastTarget = false;

            GameObject ownedPanel = new GameObject("OwnedCardsPanel", typeof(RectTransform));
            ownedPanel.transform.SetParent(_rootPanel.transform, false);
            RectTransform ownedPanelRt = ownedPanel.GetComponent<RectTransform>();
            ownedPanelRt.anchorMin = new Vector2(0f, 0.5f);
            ownedPanelRt.anchorMax = new Vector2(0f, 0.5f);
            ownedPanelRt.pivot = new Vector2(0f, 0.5f);
            ownedPanelRt.anchoredPosition = new Vector2(40f, 0f);
            ownedPanelRt.sizeDelta = new Vector2(360f, 360f);
            Image ownedBg = ownedPanel.AddComponent<Image>();
            ownedBg.color = new Color(0.08f, 0.08f, 0.1f, 0.7f);
            ownedBg.raycastTarget = false;

            GridLayoutGroup ownedGrid = ownedPanel.AddComponent<GridLayoutGroup>();
            ownedGrid.cellSize = new Vector2(150f, 60f);
            ownedGrid.spacing = new Vector2(10f, 10f);
            ownedGrid.padding = new RectOffset(10, 10, 10, 10);
            ownedGrid.childAlignment = TextAnchor.UpperLeft;
            ownedGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            ownedGrid.constraintCount = 2;
            _ownedGridRoot = ownedPanel.transform;

            GameObject loadoutLabel = new GameObject("LoadoutTitle", typeof(RectTransform));
            loadoutLabel.transform.SetParent(_rootPanel.transform, false);
            RectTransform loadoutLabelRt = loadoutLabel.GetComponent<RectTransform>();
            loadoutLabelRt.anchorMin = new Vector2(0.54f, 0.88f);
            loadoutLabelRt.anchorMax = new Vector2(0.975f, 0.93f);
            loadoutLabelRt.offsetMin = Vector2.zero;
            loadoutLabelRt.offsetMax = Vector2.zero;
            Text loadoutLabelText = loadoutLabel.AddComponent<Text>();
            loadoutLabelText.text = "Loadout (8 Slots)";
            loadoutLabelText.font = _font;
            loadoutLabelText.fontSize = 16;
            loadoutLabelText.color = new Color(0.6f, 1f, 0.6f);
            loadoutLabelText.alignment = TextAnchor.MiddleLeft;
            loadoutLabelText.raycastTarget = false;

            GameObject loadoutPanel = new GameObject("LoadoutPanel", typeof(RectTransform));
            loadoutPanel.transform.SetParent(_rootPanel.transform, false);
            RectTransform loadoutPanelRt = loadoutPanel.GetComponent<RectTransform>();
            loadoutPanelRt.anchorMin = new Vector2(0.54f, 0.08f);
            loadoutPanelRt.anchorMax = new Vector2(0.975f, 0.87f);
            loadoutPanelRt.offsetMin = Vector2.zero;
            loadoutPanelRt.offsetMax = Vector2.zero;
            Image loadoutBg = loadoutPanel.AddComponent<Image>();
            loadoutBg.color = new Color(0.08f, 0.08f, 0.1f, 0.7f);
            loadoutBg.raycastTarget = false;

            GridLayoutGroup loadoutGrid = loadoutPanel.AddComponent<GridLayoutGroup>();
            loadoutGrid.cellSize = new Vector2(110f, 60f);
            loadoutGrid.spacing = new Vector2(12f, 12f);
            loadoutGrid.padding = new RectOffset(12, 12, 12, 12);
            loadoutGrid.childAlignment = TextAnchor.MiddleCenter;
            loadoutGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            loadoutGrid.constraintCount = 2;
            _loadoutGridRoot = loadoutPanel.transform;

            GameObject hintObj = new GameObject("HintText", typeof(RectTransform));
            hintObj.transform.SetParent(_rootPanel.transform, false);
            RectTransform hintRt = hintObj.GetComponent<RectTransform>();
            hintRt.anchorMin = new Vector2(0.025f, 0f);
            hintRt.anchorMax = new Vector2(0.975f, 0.055f);
            hintRt.offsetMin = Vector2.zero;
            hintRt.offsetMax = Vector2.zero;
            Text hintText = hintObj.AddComponent<Text>();
            hintText.text = "Click owned card to add. Click loadout slot to remove. B / Esc to close.";
            hintText.font = _font;
            hintText.fontSize = 12;
            hintText.color = new Color(0.4f, 0.4f, 0.45f);
            hintText.alignment = TextAnchor.MiddleCenter;
            hintText.raycastTarget = false;

            GameObject closeBtnObj = new GameObject("CloseButton", typeof(RectTransform));
            closeBtnObj.transform.SetParent(_rootPanel.transform, false);
            RectTransform closeBtnRt = closeBtnObj.GetComponent<RectTransform>();
            closeBtnRt.anchorMin = new Vector2(0.92f, 0.955f);
            closeBtnRt.anchorMax = new Vector2(0.975f, 0.995f);
            closeBtnRt.offsetMin = Vector2.zero;
            closeBtnRt.offsetMax = Vector2.zero;
            Image closeBtnBg = closeBtnObj.AddComponent<Image>();
            closeBtnBg.color = new Color(0.4f, 0.15f, 0.15f, 0.8f);
            Button closeBtn = closeBtnObj.AddComponent<Button>();
            closeBtn.targetGraphic = closeBtnBg;
            closeBtn.onClick.AddListener(() => CloseQuick());

            Text closeBtnText = CreateTextChild(closeBtnObj.transform, "Text", "Close", 14, TextAnchor.MiddleCenter, Color.white);
            closeBtnText.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            closeBtnText.rectTransform.anchorMax = new Vector2(1f, 0.5f);
            closeBtnText.rectTransform.sizeDelta = new Vector2(0f, 20f);

            Debug.Log("[MagazineEditUI] BagPanel created. Size=900x520");
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
    }
}
