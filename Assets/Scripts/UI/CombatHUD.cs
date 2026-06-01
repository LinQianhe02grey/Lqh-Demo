using UnityEngine;
using UnityEngine.UI;
using Cardwin.Combat;
using Cardwin.Cards;
using Cardwin.Magazine;

namespace Cardwin.UI
{
    public class CombatHUD : MonoBehaviour
    {
        private Health _playerHealth;
        private PlayerCardContext _cardContext;
        private MagazineSystem _magazineSystem;

        private Text _hpText;
        private Text _shieldText;
        private Text _focusText;
        private Text _reloadText;
        private Text _comboText;
        private MagazinePreviewUI _magazinePreviewUI;
        private ComboRatingSystem _comboRating;

        private Transform _hudRoot;
        private bool _bound;
        private bool _warnedMagazineMissing;
        private bool _loggedFirstRefresh;

        private void Awake()
        {
            EnsureCanvas();
            DisableLegacyPlaceholders();
            EnsureHUDRoot();
            EnsureTopLeftStats();
            EnsurePreviewPanel();
            DisableFullBarIfExists();
            EnsureReloadText();
            EnsureComboText();
        }

        private void Start()
        {
            BindSystems();
            RefreshHUD();
        }

        private void Update()
        {
            if (!_bound)
            {
                BindSystems();
                return;
            }

            RefreshHUD();
            RefreshReloadProgress();
        }

        private void EnsureCanvas()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
                return;

            canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                transform.SetParent(canvas.transform, false);
                Debug.Log("[CombatHUD] Re-parented to existing Canvas.");
            }
            else
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                canvasObj.AddComponent<GraphicRaycaster>();
                transform.SetParent(canvasObj.transform, false);
                Debug.Log("[CombatHUD] Canvas created and attached.");
            }
        }

        private void DisableLegacyPlaceholders()
        {
            string[] legacyNames = { "HP_Text", "MagazinePreview_Placeholder", "State_Text" };
            foreach (string name in legacyNames)
            {
                Transform legacy = transform.Find(name);
                if (legacy != null && legacy.gameObject.activeSelf)
                {
                    legacy.gameObject.SetActive(false);
                    Debug.Log($"[CombatHUD] Legacy placeholder disabled: {name}");
                }
            }
        }

        private void EnsureHUDRoot()
        {
            Transform existing = transform.Find("CardwinHUDRoot");
            if (existing != null)
            {
                for (int i = existing.childCount - 1; i >= 0; i--)
                    Destroy(existing.GetChild(i).gameObject);

                _hudRoot = existing;
                Debug.Log("[CombatHUD] CardwinHUDRoot found, children cleaned");
                return;
            }

            GameObject rootObj = new GameObject("CardwinHUDRoot");
            rootObj.transform.SetParent(transform, false);
            RectTransform rt = rootObj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            _hudRoot = rootObj.transform;
            Debug.Log("[CombatHUD] CardwinHUDRoot created");
        }

        private void EnsureTopLeftStats()
        {
            Transform existing = _hudRoot.Find("TopLeftStats");
            if (existing != null)
                Destroy(existing.gameObject);

            GameObject container = new GameObject("TopLeftStats");
            container.transform.SetParent(_hudRoot, false);
            RectTransform rt = container.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(20f, -20f);
            rt.sizeDelta = new Vector2(320f, 120f);

            VerticalLayoutGroup vlg = container.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 2f;

            _hpText = EnsureTextInParent(container.transform, "HP_Text_Runtime", 24, TextAnchor.MiddleLeft, Color.white);
            _shieldText = EnsureTextInParent(container.transform, "Shield_Text_Runtime", 24, TextAnchor.MiddleLeft, new Color(0.5f, 0.8f, 1f));
            _focusText = EnsureTextInParent(container.transform, "Focus_Text_Runtime", 24, TextAnchor.MiddleLeft, new Color(1f, 0.8f, 0.3f));
        }

        private void EnsurePreviewPanel()
        {
            Transform existing = _hudRoot.Find("PreviewPanel");
            if (existing != null)
                Destroy(existing.gameObject);

            GameObject container = new GameObject("PreviewPanel");
            container.transform.SetParent(_hudRoot, false);
            RectTransform rt = container.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 35f);
            rt.sizeDelta = new Vector2(520f, 80f);

            HorizontalLayoutGroup hlg = container.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 12f;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            _magazinePreviewUI = container.AddComponent<MagazinePreviewUI>();
            Debug.Log("[CombatHUD] PreviewPanel created (3 slots only)");
        }

        private void DisableFullBarIfExists()
        {
            string[] barNames = { "FullMagazinePanel", "MagazineFullBar", "BottomFullMagazine" };
            foreach (string name in barNames)
            {
                Transform t = _hudRoot.Find(name);
                if (t == null)
                    t = transform.Find(name);
                if (t != null && t.gameObject.activeSelf)
                {
                    t.gameObject.SetActive(false);
                    Debug.Log($"[CombatHUD] Full bar disabled: {name}");
                }
            }
        }

        private void EnsureReloadText()
        {
            Transform existing = _hudRoot.Find("ReloadText");
            if (existing != null)
            {
                _reloadText = existing.GetComponent<Text>();
                return;
            }

            GameObject go = new GameObject("ReloadText");
            go.transform.SetParent(_hudRoot, false);
            _reloadText = go.AddComponent<Text>();
            _reloadText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _reloadText.fontSize = 24;
            _reloadText.color = new Color(1f, 0.7f, 0.3f);
            _reloadText.alignment = TextAnchor.MiddleCenter;
            _reloadText.raycastTarget = false;

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, 120f);
            rt.sizeDelta = new Vector2(300f, 50f);
        }

        private Text EnsureTextInParent(Transform parent, string name, int fontSize, TextAnchor align, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Text txt = go.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = align;
            txt.raycastTarget = false;

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(300f, fontSize + 4f);

            return txt;
        }

        private void BindSystems()
        {
            if (_bound)
                return;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                if (_magazineSystem == null)
                    _magazineSystem = FindObjectOfType<MagazineSystem>();

                if (_magazineSystem != null && !_bound)
                {
                    Debug.Log($"[CombatHUD] Bound MagazineSystem (fallback). Cards={_magazineSystem.LoadedCards.Count}");
                    if (_magazinePreviewUI != null)
                        _magazinePreviewUI.Bind(_magazineSystem);
                }
                return;
            }

            _playerHealth = player.GetComponent<Health>();
            var pc = player.GetComponent<PlayerController2D>();
            if (pc != null)
                _cardContext = pc.cardContext;

            _magazineSystem = player.GetComponent<MagazineSystem>();
            _comboRating = player.GetComponent<ComboRatingSystem>();
            Debug.Log("[CombatHUD] Bound Player");

            if (_magazineSystem != null)
            {
                int cardCount = _magazineSystem.LoadedCards.Count;
                if (cardCount == 0)
                {
                    Debug.LogWarning("[CombatHUD] MagazineSystem has no cards. Check Player initialCards.");
                }
                else
                {
                    Debug.Log($"[CombatHUD] Bound MagazineSystem. Cards={cardCount}");
                }

                if (_magazinePreviewUI != null)
                    _magazinePreviewUI.Bind(_magazineSystem);
            }
            else
            {
                if (!_warnedMagazineMissing)
                {
                    Debug.LogError("[CombatHUD] MagazineSystem not found on Player.");
                    _warnedMagazineMissing = true;
                }
            }

            _bound = _playerHealth != null;
            if (!_bound)
                Debug.LogWarning("[CombatHUD] Could not bind player Health. HUD stats won't update.");
        }

        private void RefreshHUD()
        {
            if (_playerHealth != null)
            {
                if (_hpText != null)
                    _hpText.text = $"HP: {_playerHealth.currentHealth}/{_playerHealth.maxHealth}";

                if (_shieldText != null)
                    _shieldText.text = _playerHealth.currentBlock > 0 ? $"Shield: {_playerHealth.currentBlock}" : "";
            }

            if (_cardContext != null && _focusText != null)
                _focusText.text = _cardContext.focusStacks > 0 ? $"Focus: {_cardContext.focusStacks}" : "";

            if (_comboText != null && _comboRating != null)
            {
                if (_comboRating.IsActive && _comboRating.ComboCount > 0)
                    _comboText.text = $"Combo: {_comboRating.ComboCount}\nRank: {_comboRating.CurrentRank}\nTime: {_comboRating.ComboTimer:F1}s";
                else
                    _comboText.text = "Combo: 0\nRank: -";
            }

            if (_bound && !_loggedFirstRefresh)
            {
                Debug.Log("[CombatHUD] First refresh done - verify HP/Shield/Focus visible in Game view top-left");
                _loggedFirstRefresh = true;
            }
        }

        private void EnsureComboText()
        {
            Transform existing = _hudRoot.Find("ComboText");
            if (existing != null)
            {
                _comboText = existing.GetComponent<Text>();
                return;
            }

            GameObject go = new GameObject("ComboText");
            go.transform.SetParent(_hudRoot, false);
            _comboText = go.AddComponent<Text>();
            _comboText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _comboText.fontSize = 22;
            _comboText.color = Color.white;
            _comboText.alignment = TextAnchor.UpperRight;
            _comboText.raycastTarget = false;

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-30f, -30f);
            rt.sizeDelta = new Vector2(260f, 80f);
        }

        private void RefreshReloadProgress()
        {
            if (_reloadText == null || _magazineSystem == null)
                return;

            if (_magazineSystem.IsReloading)
            {
                _reloadText.text = $"Reloading... {Mathf.RoundToInt(_magazineSystem.ReloadProgress * 100)}%";
                _reloadText.gameObject.SetActive(true);
            }
            else
            {
                _reloadText.gameObject.SetActive(false);
            }
        }
    }
}
