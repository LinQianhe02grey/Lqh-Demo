using UnityEngine;
using UnityEngine.UI;
using MirrorSaintessBossPack;

namespace Cardwin.Boss
{
    /// <summary>
    /// Minimal BossRoom-local boss HUD (V1). Builds its own ScreenSpaceOverlay canvas at
    /// runtime so it does NOT touch the GlobalRuntimeRoot canvas / EventSystem. Shows the
    /// boss name, a total-HP bar, the three part states and a "DEFEATED" line on death.
    /// </summary>
    public sealed class BossHUD : MonoBehaviour
    {
        [SerializeField] private MirrorSaintessBoss boss;
        [SerializeField] private float barWidth = 520f;
        [SerializeField] private float barHeight = 26f;

        private Canvas _canvas;
        private Text _nameText;
        private Text _hpText;
        private RectTransform _fillRect;
        private Text _blueText;
        private Text _redText;
        private Text _coreText;
        private Text _defeatedText;
        private MirrorAngelBossEffectReceiver _effect;
        private Font _font;
        private bool _built;
        private bool _subscribed;

        private void Start()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null)
                _font = Font.CreateDynamicFontFromOSFont("Arial", 16);

            ResolveBoss();
            BuildUI();
            TrySubscribe();
            RefreshAll();
        }

        private void LateUpdate()
        {
            if (boss == null)
            {
                ResolveBoss();
                TrySubscribe();
            }
            // Lightweight poll as a backup to the events.
            RefreshAll();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void ResolveBoss()
        {
            if (boss == null)
                boss = FindObjectOfType<MirrorSaintessBoss>();
            if (_effect == null && boss != null)
                _effect = boss.GetComponent<MirrorAngelBossEffectReceiver>();
        }

        private void TrySubscribe()
        {
            if (boss == null || _subscribed)
                return;
            boss.OnHealthChanged += HandleHealthChanged;
            boss.OnPartStateChanged += HandlePartChanged;
            boss.OnBossDefeated += HandleDefeated;
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (boss == null || !_subscribed)
                return;
            boss.OnHealthChanged -= HandleHealthChanged;
            boss.OnPartStateChanged -= HandlePartChanged;
            boss.OnBossDefeated -= HandleDefeated;
            _subscribed = false;
        }

        private void HandleHealthChanged(int current, int max) => RefreshBar();
        private void HandlePartChanged() => RefreshParts();
        private void HandleDefeated()
        {
            if (_defeatedText != null)
                _defeatedText.text = "MIRROR SAINTESS DEFEATED";
            Debug.Log("[BossHUD] Victory placeholder shown.");
        }

        private void BuildUI()
        {
            if (_built)
                return;
            _built = true;

            GameObject canvasObj = new GameObject("BossHUD_Canvas");
            canvasObj.transform.SetParent(transform, false);
            _canvas = canvasObj.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 50;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // Root panel near top-center.
            RectTransform root = NewRect("Root", canvasObj.transform);
            root.anchorMin = new Vector2(0.5f, 1f);
            root.anchorMax = new Vector2(0.5f, 1f);
            root.pivot = new Vector2(0.5f, 1f);
            root.anchoredPosition = new Vector2(0f, -24f);
            root.sizeDelta = new Vector2(barWidth + 40f, 150f);

            _nameText = MakeText("Name", root, boss != null ? boss.BossName : "Mirror Saintess", 26, TextAnchor.UpperCenter);
            SetRect(_nameText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -2f), new Vector2(barWidth, 32f));

            // HP bar background.
            GameObject bgObj = new GameObject("HPBarBG");
            bgObj.transform.SetParent(root, false);
            Image bg = bgObj.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
            RectTransform bgRect = bg.rectTransform;
            SetRect(bgRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(barWidth, barHeight));

            // HP bar fill (left-anchored, width scaled by ratio).
            GameObject fillObj = new GameObject("HPBarFill");
            fillObj.transform.SetParent(bgRect, false);
            Image fill = fillObj.AddComponent<Image>();
            fill.color = new Color(0.85f, 0.15f, 0.2f, 1f);
            _fillRect = fill.rectTransform;
            _fillRect.anchorMin = new Vector2(0f, 0f);
            _fillRect.anchorMax = new Vector2(0f, 1f);
            _fillRect.pivot = new Vector2(0f, 0.5f);
            _fillRect.anchoredPosition = Vector2.zero;
            _fillRect.sizeDelta = new Vector2(barWidth, 0f);

            _hpText = MakeText("HPText", bgRect, "", 16, TextAnchor.MiddleCenter);
            SetRect(_hpText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            // Stage 43: Shield | Body | Status row.
            RectTransform partsRow = NewRect("StatusRow", root);
            SetRect(partsRow, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -74f), new Vector2(barWidth, 28f));
            _blueText = MakeText("ShieldStatus", partsRow, "Shield: 0", 16, TextAnchor.MiddleLeft);
            SetRect(_blueText.rectTransform, new Vector2(0f, 0f), new Vector2(0.34f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            _coreText = MakeText("BodyStatus", partsRow, "Body: OK", 16, TextAnchor.MiddleCenter);
            SetRect(_coreText.rectTransform, new Vector2(0.33f, 0f), new Vector2(0.67f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            _redText = MakeText("BuffStatus", partsRow, "Status: None", 16, TextAnchor.MiddleRight);
            SetRect(_redText.rectTransform, new Vector2(0.66f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            _defeatedText = MakeText("Defeated", root, "", 22, TextAnchor.MiddleCenter);
            _defeatedText.color = new Color(1f, 0.85f, 0.2f, 1f);
            SetRect(_defeatedText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -108f), new Vector2(barWidth, 30f));
        }

        private void RefreshAll()
        {
            RefreshBar();
            RefreshParts();
        }

        private void RefreshBar()
        {
            if (boss == null || _fillRect == null)
                return;
            float ratio = Mathf.Clamp01(boss.HealthRatio);
            _fillRect.sizeDelta = new Vector2(barWidth * ratio, 0f);
            if (_hpText != null)
                _hpText.text = $"{boss.CurrentTotalHp} / {boss.MaxTotalHp}   (Phase {boss.CurrentPhase})";
        }

        private void RefreshParts()
        {
            if (boss == null)
                return;
            if (_effect == null)
                _effect = boss.GetComponent<MirrorAngelBossEffectReceiver>();
            // Stage 43: Shield | Body | Status.
            if (_blueText != null)
                _blueText.text = "Shield: " + (_effect != null ? _effect.CurrentShield : 0);
            if (_coreText != null)
                _coreText.text = "Body: " + (boss.IsDead ? "DEAD" : "OK");
            if (_redText != null)
            {
                bool buff = _effect != null && _effect.HasBuff;
                _redText.text = buff ? ("Status: " + _effect.BuffName) : "Status: None";
            }
        }

        private RectTransform NewRect(string name, Transform parent)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.AddComponent<RectTransform>();
        }

        private Text MakeText(string name, Transform parent, string content, int size, TextAnchor anchor)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Text t = go.AddComponent<Text>();
            t.font = _font;
            t.text = content;
            t.fontSize = size;
            t.alignment = anchor;
            t.color = Color.white;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        private void SetRect(RectTransform rt, Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
        }
    }
}
