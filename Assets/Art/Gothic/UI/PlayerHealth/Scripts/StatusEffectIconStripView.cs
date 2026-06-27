using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cardwin.UI
{
    [ExecuteAlways]
    public class StatusEffectIconStripView : MonoBehaviour
    {
        [Header("Editor Preview")]
        [SerializeField]
        private bool enableEditorPreview = true;

        [Range(0, 8)]
        [SerializeField]
        private int previewIconCount = 1;

        [SerializeField]
        private Sprite previewFocusSprite;

        [Header("Layout")]
        [SerializeField]
        private Vector2 iconSize = new Vector2(44f, 44f);

        [SerializeField]
        private float spacing = 6f;

        private HorizontalLayoutGroup _layoutGroup;
        private RectTransform _rowRect;
        private List<StatusSlot> _slots;
        private List<ActiveStatus> _activeStatuses = new List<ActiveStatus>();
        private bool _capacityWarningShown;

        private struct ActiveStatus
        {
            public string statusId;
            public Sprite sprite;
        }

        private class StatusSlot
        {
            public GameObject gameObject;
            public Image iconImage;
            public RectTransform rectTransform;

            public bool IsActive => gameObject != null && gameObject.activeSelf;
        }

        private void Awake()
        {
            CacheSlots();
        }

        private void OnEnable()
        {
            CacheSlots();

            if (!Application.isPlaying && enableEditorPreview)
                RefreshPreview();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (!Application.isPlaying && enableEditorPreview)
                RefreshPreview();
            else if (Application.isPlaying)
                RefreshSlots();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying && enableEditorPreview)
                RefreshPreview();
        }

        private void CacheSlots()
        {
            if (_slots != null && _slots.Count > 0)
                return;

            _slots = new List<StatusSlot>();
            _layoutGroup = GetComponent<HorizontalLayoutGroup>();
            _rowRect = GetComponent<RectTransform>();

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (!child.name.StartsWith("StatusSlot_"))
                    continue;

                Image iconImage = null;
                Transform iconTransform = child.Find("Icon");
                if (iconTransform != null)
                    iconImage = iconTransform.GetComponent<Image>();

                var slot = new StatusSlot
                {
                    gameObject = child.gameObject,
                    iconImage = iconImage,
                    rectTransform = child.GetComponent<RectTransform>()
                };

                _slots.Add(slot);
                child.gameObject.SetActive(false);
            }
        }

        public int CalculateVisibleCapacity()
        {
            if (_rowRect == null)
                _rowRect = GetComponent<RectTransform>();

            if (_rowRect == null)
                return 0;

            float availableWidth = _rowRect.rect.width;

            if (_layoutGroup == null)
                _layoutGroup = GetComponent<HorizontalLayoutGroup>();

            if (_layoutGroup != null)
            {
                availableWidth -= _layoutGroup.padding.left + _layoutGroup.padding.right;
            }

            float slotWidth = iconSize.x;

            if (availableWidth <= 0f || slotWidth <= 0f)
                return 0;

            int capacity = Mathf.FloorToInt((availableWidth + spacing) / (slotWidth + spacing));

            return Mathf.Clamp(capacity, 0, _slots != null ? _slots.Count : 0);
        }

        private void RefreshPreview()
        {
            CacheSlots();
            if (_slots == null || _slots.Count == 0)
                return;

            ApplyLayoutSettings();

            int capacity = CalculateVisibleCapacity();
            int visibleCount = Mathf.Min(previewIconCount, capacity, _slots.Count);

            for (int i = 0; i < _slots.Count; i++)
            {
                if (i < visibleCount)
                {
                    _slots[i].gameObject.SetActive(true);
                    if (_slots[i].iconImage != null && previewFocusSprite != null)
                        _slots[i].iconImage.sprite = previewFocusSprite;
                }
                else
                {
                    _slots[i].gameObject.SetActive(false);
                }
            }
        }

        private void ApplyLayoutSettings()
        {
            if (_layoutGroup == null)
                _layoutGroup = GetComponent<HorizontalLayoutGroup>();

            if (_layoutGroup != null)
            {
                _layoutGroup.spacing = spacing;
                _layoutGroup.childAlignment = TextAnchor.MiddleLeft;
                _layoutGroup.childForceExpandWidth = false;
                _layoutGroup.childForceExpandHeight = false;
                _layoutGroup.childControlWidth = false;
                _layoutGroup.childControlHeight = false;
            }

            foreach (var slot in _slots)
            {
                if (slot.rectTransform != null)
                {
                    slot.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, iconSize.x);
                    slot.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, iconSize.y);
                }
            }
        }

        public void ShowStatusIcon(string statusId, Sprite sprite)
        {
            CacheSlots();
            if (_slots == null || _slots.Count == 0)
                return;

            for (int i = 0; i < _activeStatuses.Count; i++)
            {
                if (_activeStatuses[i].statusId == statusId)
                {
                    _activeStatuses[i] = new ActiveStatus { statusId = statusId, sprite = sprite };
                    RefreshSlots();
                    return;
                }
            }

            _activeStatuses.Add(new ActiveStatus { statusId = statusId, sprite = sprite });
            RefreshSlots();
        }

        public void HideStatusIcon(string statusId)
        {
            CacheSlots();
            if (_slots == null || _slots.Count == 0)
                return;

            for (int i = 0; i < _activeStatuses.Count; i++)
            {
                if (_activeStatuses[i].statusId == statusId)
                {
                    _activeStatuses.RemoveAt(i);
                    RefreshSlots();
                    return;
                }
            }
        }

        public void ClearAllStatusIcons()
        {
            CacheSlots();
            if (_slots == null)
                return;

            _activeStatuses.Clear();

            foreach (var slot in _slots)
            {
                if (slot.gameObject != null)
                    slot.gameObject.SetActive(false);
                if (slot.iconImage != null)
                    slot.iconImage.sprite = null;
            }
        }

        public int ActiveIconCount
        {
            get
            {
                CacheSlots();
                if (_slots == null)
                    return 0;
                int c = 0;
                foreach (var s in _slots)
                    if (s.IsActive) c++;
                return c;
            }
        }

        public int MaxSlots => _slots != null ? _slots.Count : 0;

        private void RefreshSlots()
        {
            CacheSlots();
            if (_slots == null || _slots.Count == 0)
                return;

            int capacity = CalculateVisibleCapacity();
            int visibleCount = Mathf.Min(_activeStatuses.Count, capacity, _slots.Count);

            if (_activeStatuses.Count > capacity && !_capacityWarningShown)
            {
                Debug.LogWarning("[StatusEffectIconStrip] StatusEffectRow capacity reached. Additional status icons are hidden.");
                _capacityWarningShown = true;
            }
            else if (_activeStatuses.Count <= capacity)
            {
                _capacityWarningShown = false;
            }

            for (int i = 0; i < _slots.Count; i++)
            {
                if (i < visibleCount)
                {
                    _slots[i].gameObject.SetActive(true);
                    if (_slots[i].iconImage != null && _activeStatuses[i].sprite != null)
                        _slots[i].iconImage.sprite = _activeStatuses[i].sprite;
                }
                else
                {
                    _slots[i].gameObject.SetActive(false);
                }
            }
        }

        private void CompactSlots()
        {
            RefreshSlots();
        }
    }
}
