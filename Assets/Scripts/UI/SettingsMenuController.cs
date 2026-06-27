using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cardwin.Core;
using Cardwin.Settings;

namespace Cardwin.UI
{
    public enum SettingsSource
    {
        MainMenu,
        PauseMenu
    }

    public class SettingsMenuController : MonoBehaviour
    {
        [Header("Parent Panels (for back navigation)")]
        public GameObject mainPanel;
        public GameObject pausePanel;

        private SettingsSource currentSource;
        private List<(int width, int height)> _resolutionList;
        private bool _uiReady;

        private GameObject _settingsPanelRuntime;
        private Slider _volumeSlider;
        private Text _volumeValueText;
        private Toggle _fullscreenToggle;
        private Dropdown _fullscreenModeDropdown;
        private Dropdown _resolutionDropdown;
        private Button _applyButton;
        private Button _backButton;
        private Button _resumeButton;
        private Button _mainMenuButton;
        private Text _hintText;

        public bool IsOpen
        {
            get
            {
                return _settingsPanelRuntime != null && _settingsPanelRuntime.activeSelf;
            }
        }

        private void Awake()
        {
            SettingsSystem.Load();
        }

        private void Update()
        {
            if (_settingsPanelRuntime != null && _settingsPanelRuntime.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("[Settings] Esc pressed - closing.");
                Close();
            }
        }

        public bool OpenFromMainMenu()
        {
            Debug.Log("[Settings] Open from MainMenu.");

            currentSource = SettingsSource.MainMenu;

            if (!EnsureUI())
            {
                Debug.LogError("[Settings] EnsureUI failed.");
                return false;
            }

            if (mainPanel != null) mainPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);

            SetContextualButtons(false);
            ShowPanel();
            return true;
        }

        public bool OpenFromPauseMenu()
        {
            Debug.Log("[Settings] Open from PauseMenu.");

            currentSource = SettingsSource.PauseMenu;

            if (!EnsureUI())
            {
                Debug.LogError("[Settings] EnsureUI failed.");
                return false;
            }

            if (pausePanel != null) pausePanel.SetActive(false);
            if (mainPanel != null) mainPanel.SetActive(false);

            SetContextualButtons(true);
            ShowPanel();
            return true;
        }

        private void SetContextualButtons(bool visible)
        {
            if (_resumeButton != null) _resumeButton.gameObject.SetActive(visible);
            if (_mainMenuButton != null) _mainMenuButton.gameObject.SetActive(visible);
        }

        private void ShowPanel()
        {
            if (_settingsPanelRuntime == null)
            {
                Debug.LogError("[Settings] _settingsPanelRuntime is null, cannot show.");
                return;
            }

            _settingsPanelRuntime.SetActive(true);
            _settingsPanelRuntime.transform.SetAsLastSibling();

            LoadCurrentSettingsToUI();
        }

        public void Close()
        {
            if (_settingsPanelRuntime != null)
                _settingsPanelRuntime.SetActive(false);

            if (_hintText != null)
                _hintText.text = "";

            if (currentSource == SettingsSource.MainMenu && mainPanel != null)
            {
                mainPanel.SetActive(true);
            }
            else if (currentSource == SettingsSource.PauseMenu && pausePanel != null)
            {
                pausePanel.SetActive(true);
                Time.timeScale = 0f;
            }

            Debug.Log($"[Settings] Closed. Source was {currentSource}.");
        }

        private bool EnsureUI()
        {
            if (_uiReady && _settingsPanelRuntime != null) return true;

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[Settings] No Canvas found in parent hierarchy. UI cannot render.");
                return false;
            }

            if (_settingsPanelRuntime == null)
            {
                _settingsPanelRuntime = new GameObject("SettingsPanel_Runtime", typeof(RectTransform));
                _settingsPanelRuntime.transform.SetParent(canvas.transform, false);
                _settingsPanelRuntime.layer = canvas.gameObject.layer;
            }

            var rt = _settingsPanelRuntime.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(620, 560);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.one;

            var panelImg = _settingsPanelRuntime.GetComponent<Image>();
            if (panelImg == null) panelImg = _settingsPanelRuntime.AddComponent<Image>();
            panelImg.color = new Color(0f, 0f, 0f, 0.92f);
            panelImg.raycastTarget = true;

            SafeDestroyChildren(_settingsPanelRuntime.transform);

            Font font = GetFont();
            if (font == null)
            {
                Debug.LogError("[Settings] Cannot load font. UI text will be empty.");
            }

            CreateBackground(_settingsPanelRuntime.transform);
            CreateTitle(_settingsPanelRuntime.transform, font);
            CreateVolumeSection(_settingsPanelRuntime.transform, font);
            CreateFullscreenSection(_settingsPanelRuntime.transform, font);
            CreateResolutionSection(_settingsPanelRuntime.transform, font);
            CreateButtons(_settingsPanelRuntime.transform, font);
            CreateHint(_settingsPanelRuntime.transform, font);

            BuildResolutionDropdown();
            _settingsPanelRuntime.SetActive(false);
            _uiReady = true;

            Debug.Log("[Settings] EnsureUI complete - all controls created on SettingsPanel_Runtime.");
            return true;
        }

        private void SafeDestroyChildren(Transform parent)
        {
            if (parent == null) return;
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                var child = parent.GetChild(i);
                if (child != null)
                    Destroy(child.gameObject);
            }
        }

        private GameObject CreateChild(string name, float x, float y, float w, float h, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.layer = parent.gameObject.layer;

            var rt2 = go.GetComponent<RectTransform>();
            if (rt2 == null) rt2 = go.AddComponent<RectTransform>();
            rt2.anchorMin = new Vector2(0.5f, 0.5f);
            rt2.anchorMax = new Vector2(0.5f, 0.5f);
            rt2.anchoredPosition = new Vector2(x, y);
            rt2.sizeDelta = new Vector2(w, h);
            rt2.localScale = Vector3.one;

            return go;
        }

        private void CreateBackground(Transform parent)
        {
            if (parent == null) return;
            var bg = CreateChild("Background", 0, 0, 620, 560, parent);
            if (bg == null) return;
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0f, 0f, 0f, 0.92f);
            bgImg.raycastTarget = true;
                bg.transform.SetAsFirstSibling();
        }

        private Font GetFont()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Font.CreateDynamicFontFromOSFont("Arial", 14);
            }
            return font;
        }

        private void CreateTitle(Transform parent, Font font)
        {
            if (parent == null) return;
            var go = CreateChild("TitleText", 0, 220, 400, 50, parent);
            if (go == null) return;
            var txt = go.AddComponent<Text>();
            txt.text = "Settings";
            if (font != null) txt.font = font;
            txt.fontSize = 34;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.raycastTarget = false;
        }

        private void CreateVolumeSection(Transform parent, Font font)
        {
            if (parent == null) return;

            var label = CreateChild("VolumeLabel", -180, 150, 200, 30, parent);
            if (label != null)
            {
                var lbl = label.AddComponent<Text>();
                lbl.text = "Volume";
                if (font != null) lbl.font = font;
                lbl.fontSize = 20;
                lbl.alignment = TextAnchor.MiddleCenter;
                lbl.color = Color.white;
                lbl.raycastTarget = false;
            }

            var sliderGo = CreateChild("VolumeSlider", 0, 115, 320, 24, parent);
            if (sliderGo != null)
            {
                var sliderBg = sliderGo.AddComponent<Image>();
                sliderBg.color = new Color(0.3f, 0.3f, 0.3f);
                _volumeSlider = sliderGo.AddComponent<Slider>();
                _volumeSlider.targetGraphic = sliderBg;

                var fillArea = new GameObject("Fill Area", typeof(RectTransform));
                fillArea.transform.SetParent(sliderGo.transform, false);
                fillArea.layer = sliderGo.layer;
                var faRt = fillArea.GetComponent<RectTransform>();
                faRt.anchorMin = new Vector2(0, 0.25f);
                faRt.anchorMax = new Vector2(1, 0.75f);
                faRt.sizeDelta = new Vector2(-20, 0);
                faRt.anchoredPosition = Vector2.zero;

                var fillGo = new GameObject("Fill", typeof(RectTransform));
                fillGo.transform.SetParent(fillArea.transform, false);
                fillGo.layer = sliderGo.layer;
                var fillRt = fillGo.GetComponent<RectTransform>();
                fillRt.anchorMin = Vector2.zero;
                fillRt.anchorMax = Vector2.one;
                fillRt.sizeDelta = Vector2.zero;
                var fillImg = fillGo.AddComponent<Image>();
                fillImg.color = new Color(1f, 1f, 1f, 0.5f);
                _volumeSlider.fillRect = fillRt;
                _volumeSlider.targetGraphic = fillImg;

                var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
                handleArea.transform.SetParent(sliderGo.transform, false);
                handleArea.layer = sliderGo.layer;
                var haRt = handleArea.GetComponent<RectTransform>();
                haRt.anchorMin = new Vector2(0, 0);
                haRt.anchorMax = new Vector2(1, 1);
                haRt.sizeDelta = new Vector2(-10, 0);
                haRt.anchoredPosition = Vector2.zero;

                var handleGo = new GameObject("Handle", typeof(RectTransform));
                handleGo.transform.SetParent(handleArea.transform, false);
                handleGo.layer = sliderGo.layer;
                var handleRt = handleGo.GetComponent<RectTransform>();
                handleRt.anchorMin = Vector2.zero;
                handleRt.anchorMax = Vector2.one;
                handleRt.sizeDelta = new Vector2(20, 0);
                var handleImg = handleGo.AddComponent<Image>();
                handleImg.color = new Color(0.8f, 0.8f, 0.8f);
                _volumeSlider.handleRect = handleRt;

                _volumeSlider.minValue = 0f;
                _volumeSlider.maxValue = 1f;
                _volumeSlider.wholeNumbers = false;
                _volumeSlider.onValueChanged.RemoveAllListeners();
                _volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            }

            var valGo = CreateChild("VolumeValueText", 220, 115, 60, 30, parent);
            if (valGo != null)
            {
                _volumeValueText = valGo.AddComponent<Text>();
                _volumeValueText.text = "100%";
                if (font != null) _volumeValueText.font = font;
                _volumeValueText.fontSize = 18;
                _volumeValueText.alignment = TextAnchor.MiddleCenter;
                _volumeValueText.color = Color.white;
                _volumeValueText.raycastTarget = false;
            }
        }

        private void CreateFullscreenSection(Transform parent, Font font)
        {
            if (parent == null) return;

            var labelGo = CreateChild("FullscreenLabel", 20, 45, 180, 30, parent);
            if (labelGo != null)
            {
                var lbl = labelGo.AddComponent<Text>();
                lbl.text = "Display Mode";
                if (font != null) lbl.font = font;
                lbl.fontSize = 20;
                lbl.alignment = TextAnchor.MiddleLeft;
                lbl.color = Color.white;
                lbl.raycastTarget = false;
            }

            var ddGo = CreateChild("FullscreenModeDropdown", -120, 45, 260, 36, parent);
            if (ddGo != null)
            {
                var ddImg = ddGo.AddComponent<Image>();
                ddImg.color = new Color(0.3f, 0.3f, 0.3f, 1f);
                _fullscreenModeDropdown = ddGo.AddComponent<Dropdown>();

                var templateGo = new GameObject("Template", typeof(RectTransform));
                templateGo.transform.SetParent(ddGo.transform, false);
                templateGo.layer = ddGo.layer;
                var tRt = templateGo.GetComponent<RectTransform>();
                tRt.anchorMin = Vector2.zero;
                tRt.anchorMax = new Vector2(1, 0);
                var tImg = templateGo.AddComponent<Image>();
                tImg.color = new Color(0.25f, 0.25f, 0.25f);
                _fullscreenModeDropdown.template = tRt;

                var viewport = new GameObject("Viewport", typeof(RectTransform));
                viewport.transform.SetParent(templateGo.transform, false);
                viewport.layer = ddGo.layer;
                var vpRt = viewport.GetComponent<RectTransform>();
                vpRt.anchorMin = Vector2.zero;
                vpRt.anchorMax = Vector2.one;
                vpRt.sizeDelta = Vector2.zero;
                var vpImg = viewport.AddComponent<Image>();
                vpImg.color = new Color(0.2f, 0.2f, 0.2f);
                var vpMask = viewport.AddComponent<Mask>();
                vpMask.showMaskGraphic = false;

                var contentGo = new GameObject("Content", typeof(RectTransform));
                contentGo.transform.SetParent(viewport.transform, false);
                contentGo.layer = ddGo.layer;
                var cRt = contentGo.GetComponent<RectTransform>();
                cRt.anchorMin = Vector2.zero;
                cRt.anchorMax = Vector2.one;
                cRt.sizeDelta = Vector2.zero;

                var labelDrop = CreateChild("Label", 0, 0, 0, 0, ddGo.transform);
                if (labelDrop != null)
                {
                    var lrt = labelDrop.GetComponent<RectTransform>();
                    lrt.anchorMin = Vector2.zero;
                    lrt.anchorMax = Vector2.one;
                    lrt.offsetMin = new Vector2(10, 2);
                    lrt.offsetMax = new Vector2(-30, -2);
                    var ddlbl = labelDrop.AddComponent<Text>();
                    if (font != null) ddlbl.font = font;
                    ddlbl.fontSize = 16;
                    ddlbl.alignment = TextAnchor.MiddleLeft;
                    ddlbl.color = Color.white;
                    ddlbl.raycastTarget = false;
                    _fullscreenModeDropdown.captionText = ddlbl;
                }

                _fullscreenModeDropdown.ClearOptions();
                _fullscreenModeDropdown.AddOptions(new System.Collections.Generic.List<string> {
                    "Fullscreen",
                    "Borderless Fullscreen",
                    "Windowed"
                });
            }
        }

        private void CreateResolutionSection(Transform parent, Font font)
        {
            if (parent == null) return;

            var labelGo = CreateChild("ResolutionLabel", -120, -25, 150, 30, parent);
            if (labelGo != null)
            {
                var lbl = labelGo.AddComponent<Text>();
                lbl.text = "Resolution";
                if (font != null) lbl.font = font;
                lbl.fontSize = 20;
                lbl.alignment = TextAnchor.MiddleRight;
                lbl.color = Color.white;
                lbl.raycastTarget = false;
            }

            var ddGo = CreateChild("ResolutionDropdown", 80, -65, 260, 36, parent);
            if (ddGo != null)
            {
                var ddImg = ddGo.AddComponent<Image>();
                ddImg.color = new Color(0.3f, 0.3f, 0.3f, 1f);
                _resolutionDropdown = ddGo.AddComponent<Dropdown>();

                var templateGo = new GameObject("Template", typeof(RectTransform));
                templateGo.transform.SetParent(ddGo.transform, false);
                templateGo.layer = ddGo.layer;
                var tRt = templateGo.GetComponent<RectTransform>();
                tRt.anchorMin = Vector2.zero;
                tRt.anchorMax = new Vector2(1, 0);
                var tImg = templateGo.AddComponent<Image>();
                tImg.color = new Color(0.25f, 0.25f, 0.25f);
                _resolutionDropdown.template = tRt;

                var viewport = new GameObject("Viewport", typeof(RectTransform));
                viewport.transform.SetParent(templateGo.transform, false);
                viewport.layer = ddGo.layer;
                var vpRt = viewport.GetComponent<RectTransform>();
                vpRt.anchorMin = Vector2.zero;
                vpRt.anchorMax = Vector2.one;
                vpRt.sizeDelta = Vector2.zero;
                var vpImg = viewport.AddComponent<Image>();
                vpImg.color = new Color(0.2f, 0.2f, 0.2f);
                var vpMask = viewport.AddComponent<Mask>();
                vpMask.showMaskGraphic = false;

                var contentGo = new GameObject("Content", typeof(RectTransform));
                contentGo.transform.SetParent(viewport.transform, false);
                contentGo.layer = ddGo.layer;
                var cRt = contentGo.GetComponent<RectTransform>();
                cRt.anchorMin = Vector2.zero;
                cRt.anchorMax = Vector2.one;
                cRt.sizeDelta = Vector2.zero;
                _resolutionDropdown.itemText = null;

                var labelDrop = CreateChild("Label", 0, 0, 0, 0, ddGo.transform);
                if (labelDrop != null)
                {
                    var lrt = labelDrop.GetComponent<RectTransform>();
                    lrt.anchorMin = Vector2.zero;
                    lrt.anchorMax = Vector2.one;
                    lrt.offsetMin = new Vector2(10, 2);
                    lrt.offsetMax = new Vector2(-30, -2);
                    var ddlbl = labelDrop.AddComponent<Text>();
                    if (font != null) ddlbl.font = font;
                    ddlbl.fontSize = 16;
                    ddlbl.alignment = TextAnchor.MiddleLeft;
                    ddlbl.color = Color.white;
                    ddlbl.raycastTarget = false;
                    _resolutionDropdown.captionText = ddlbl;
                }
            }
        }

        private void CreateButtons(Transform parent, Font font)
        {
            if (parent == null) return;

            _applyButton = CreateButton(parent, "Apply", -170, -210, 150, 40,
                new Color(0.2f, 0.6f, 0.3f), font, () =>
                {
                    Debug.Log("[Settings] Apply clicked.");
                    OnApplyClicked();
                });

            _backButton = CreateButton(parent, "Back", 0, -210, 150, 40,
                new Color(0.5f, 0.5f, 0.5f), font, () =>
                {
                    Debug.Log("[Settings] Back clicked.");
                    Close();
                });

            _resumeButton = CreateButton(parent, "Resume", 170, -210, 150, 40,
                new Color(0.2f, 0.5f, 0.25f), font, () =>
                {
                    Debug.Log("[Settings] Resume clicked.");
                    OnResumeClicked();
                });

            _mainMenuButton = CreateButton(parent, "Main Menu", 0, -265, 220, 38,
                new Color(0.5f, 0.2f, 0.2f), font, () =>
                {
                    Debug.Log("[Settings] Return Main Menu clicked.");
                    OnMainMenuClicked();
                });
        }

        private Button CreateButton(Transform parent, string label, float x, float y,
            float w, float h, Color color, Font font, UnityEngine.Events.UnityAction onClick)
        {
            var go = CreateChild("Btn_" + label.Replace(" ", ""), x, y, w, h, parent);
            if (go == null) return null;

            var img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = true;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.interactable = true;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(onClick);

            var lblGo = CreateChild("Label", 0, 0, 0, 0, go.transform);
            if (lblGo != null)
            {
                var lblRt = lblGo.GetComponent<RectTransform>();
                lblRt.anchorMin = Vector2.zero;
                lblRt.anchorMax = Vector2.one;
                lblRt.sizeDelta = Vector2.zero;
                var lblTxt = lblGo.AddComponent<Text>();
                lblTxt.text = label;
                if (font != null) lblTxt.font = font;
                lblTxt.fontSize = 17;
                lblTxt.alignment = TextAnchor.MiddleCenter;
                lblTxt.color = Color.white;
                lblTxt.raycastTarget = false;
            }

            return btn;
        }

        private void CreateHint(Transform parent, Font font)
        {
            if (parent == null) return;
            var go = CreateChild("HintText", 0, -310, 500, 24, parent);
            if (go == null) return;
            _hintText = go.AddComponent<Text>();
            _hintText.text = "";
            if (font != null) _hintText.font = font;
            _hintText.fontSize = 14;
            _hintText.alignment = TextAnchor.MiddleCenter;
            _hintText.color = new Color(0.7f, 0.7f, 0.7f);
            _hintText.raycastTarget = false;
        }

        private void LoadCurrentSettingsToUI()
        {
            var s = SettingsSystem.Current;

            if (_volumeSlider != null)
                _volumeSlider.SetValueWithoutNotify(s.masterVolume);
            UpdateVolumeText(s.masterVolume);

            if (_fullscreenModeDropdown != null)
            {
                int modeIdx = Mathf.Clamp(s.fullscreenMode, 0, 2);
                _fullscreenModeDropdown.SetValueWithoutNotify(modeIdx);
            }

            SetResolutionDropdownValue(s.resolutionWidth, s.resolutionHeight);
        }

        private void OnVolumeChanged(float value)
        {
            value = Mathf.Clamp01(value);
            AudioListener.volume = value;
            SettingsSystem.Current.masterVolume = value;
            UpdateVolumeText(value);
            Debug.Log($"[Settings] Volume changed: {value:F2}");
        }

        private void UpdateVolumeText(float value)
        {
            if (_volumeValueText != null)
                _volumeValueText.text = $"{Mathf.RoundToInt(value * 100f)}%";
        }

        private void BuildResolutionDropdown()
        {
            if (_resolutionDropdown == null) return;

            _resolutionList = new List<(int, int)>();
            var options = new List<string>();
            var available = SettingsSystem.GetAvailableResolutions();
            foreach (var res in available)
            {
                _resolutionList.Add(res);
                options.Add($"{res.width} x {res.height}");
            }
            _resolutionDropdown.ClearOptions();
            _resolutionDropdown.AddOptions(options);
        }

        private void SetResolutionDropdownValue(int width, int height)
        {
            if (_resolutionDropdown == null || _resolutionList == null) return;
            for (int i = 0; i < _resolutionList.Count; i++)
            {
                if (_resolutionList[i].width == width && _resolutionList[i].height == height)
                {
                    _resolutionDropdown.SetValueWithoutNotify(i);
                    return;
                }
            }
            _resolutionDropdown.SetValueWithoutNotify(0);
        }

        private void OnApplyClicked()
        {
            var s = SettingsSystem.Current;

            if (_volumeSlider != null) s.masterVolume = _volumeSlider.value;

            if (_fullscreenModeDropdown != null)
                s.fullscreenMode = _fullscreenModeDropdown.value;

            if (_resolutionDropdown != null && _resolutionList != null)
            {
                int idx = _resolutionDropdown.value;
                if (idx >= 0 && idx < _resolutionList.Count)
                {
                    s.resolutionWidth = _resolutionList[idx].width;
                    s.resolutionHeight = _resolutionList[idx].height;
                }
            }

            SettingsSystem.Apply();
            SettingsSystem.Save();

            if (_hintText != null) _hintText.text = "Settings Applied.";
        }

        private void OnBackClicked() { /* handled inline */ }
        private void OnResumeClicked()
        {
            if (_settingsPanelRuntime != null) _settingsPanelRuntime.SetActive(false);
            if (_hintText != null) _hintText.text = "";
            if (pausePanel != null) pausePanel.SetActive(false);
            Time.timeScale = 1f;
            var player = FindObjectOfType<Combat.PlayerController2D>();
            if (player != null) player.SetInputLocked(false);
        }

        private void OnMainMenuClicked()
        {
            Time.timeScale = 1f;
            GameFlowManager.Instance.ReturnToMainMenu();
        }
    }
}
