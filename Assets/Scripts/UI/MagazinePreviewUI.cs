using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cardwin.Cards;
using Cardwin.Magazine;

namespace Cardwin.UI
{
    [ExecuteAlways]
    public class MagazinePreviewUI : MonoBehaviour
    {
        [Header("Magazine")]
        public MagazineSystem magazineSystem;
        public int previewCount = 3;

        [Header("Editor Preview")]
        public bool showEditorPreview = true;
        public int previewBulletCount = 3;
        public Sprite previewAttackSprite;
        public Sprite previewBufferSprite;
        public string previewSlot0Name = "ATTACK";
        public string previewSlot1Name = "FOCUS";
        public string previewSlot2Name = "GUARD";

        [Header("Editor Preview Types")]
        public BulletPreviewVisualType previewSlot0Type = BulletPreviewVisualType.Attack;
        public BulletPreviewVisualType previewSlot1Type = BulletPreviewVisualType.Buffer;
        public BulletPreviewVisualType previewSlot2Type = BulletPreviewVisualType.Empty;

        [Header("Editor Animation Preview")]
        public PreviewMode editorPreviewMode = PreviewMode.None;
        [Range(0f, 1f)]
        public float editorAnimProgress;

        public enum PreviewMode { None, Enemy, Self }

        [Header("Bullet Sprites")]
        public Sprite attackBulletSprite;
        public Sprite bufferBulletSprite;

        [Header("Background Sprites")]
        public Sprite attackBackgroundSprite;
        public Sprite bufferBackgroundSprite;
        public Sprite emptyBackgroundSprite;

        [Header("Icon Layout")]
        public Vector2 iconSize = new Vector2(64, 64);
        [Range(-180f, 180f)]
        public float iconRotationZ = 90f;

        [Header("Name Text")]
        public float nameFontSize = 14f;
        public Color nameColor = new Color(1f, 0.85f, 0.3f, 1f);

        [Header("Layout")]
        public bool driveChildLayoutFromScript = false;

        [Header("Animations")]
        [Range(0.1f, 2f)]
        public float attackAnimDuration = 0.5f;
        [Range(0.1f, 1f)]
        public float selfAnimSpeed = 800f;

        [Header("Self Target")]
        public RectTransform selfTargetRect;

        [Header("Impact Feedback")]
        [SerializeField] private PlayerStatusImpactFeedback impactFeedback;

        [Header("Slots")]
        public CardSlotUI[] previewSlots;
        public BulletPreviewItem[] bulletPreviewItems;

        private bool _isSubscribed;
        private bool _animationInProgress;
        private bool _pendingRefresh;
        private HorizontalLayoutGroup _layoutGroup;

        private void OnEnable()
        {
            if (!Application.isPlaying && showEditorPreview)
            {
                LoadBulletSprites();
                EnsureSlotsExist();
            }
        }

        private void Start()
        {
            if (magazineSystem != null)
                return;

            magazineSystem = FindObjectOfType<MagazineSystem>();
            if (magazineSystem != null)
                Bind(magazineSystem);
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                ApplyLayoutSettings();
                RefreshEditorPreview();
            }
        }

        public void RefreshEditorPreview()
        {
            if (Application.isPlaying)
                return;

            if (!showEditorPreview)
                return;

            if (bulletPreviewItems == null || bulletPreviewItems.Length == 0)
            {
                EnsureSlotsExist();
                if (bulletPreviewItems == null || bulletPreviewItems.Length == 0)
                    return;
            }

            LoadBulletSprites();

            string[] previewNames = { previewSlot0Name, previewSlot1Name, previewSlot2Name };
            Sprite[] previewSprites = { previewAttackSprite, previewBufferSprite, previewAttackSprite };
            BulletPreviewVisualType[] previewTypes = { previewSlot0Type, previewSlot1Type, previewSlot2Type };

            for (int i = 0; i < bulletPreviewItems.Length; i++)
            {
                var item = bulletPreviewItems[i];
                if (item == null)
                    continue;

                item.attackSprite = attackBulletSprite;
                item.bufferSprite = bufferBulletSprite;
                item.attackBackgroundSprite = attackBackgroundSprite;
                item.bufferBackgroundSprite = bufferBackgroundSprite;
                item.emptyBackgroundSprite = emptyBackgroundSprite;
                item.iconSize = iconSize;
                item.iconRotationZ = iconRotationZ;

                item.EnsureReferences();

                BulletPreviewVisualType visType = i < previewTypes.Length
                    ? previewTypes[i] : BulletPreviewVisualType.Empty;
                item.ApplyBackground(visType);

                if (i < previewBulletCount && i < previewNames.Length)
                {
                    item.gameObject.SetActive(true);

                    Sprite spr = i < previewSprites.Length ? previewSprites[i] : null;
                    if (spr == null)
                        spr = (i % 2 == 0) ? previewAttackSprite ?? attackBulletSprite : previewBufferSprite ?? bufferBulletSprite;

                    if (item.bulletIcon != null)
                    {
                        item.bulletIcon.sprite = spr;
                        item.bulletIcon.gameObject.SetActive(true);
                        item.bulletIcon.transform.localRotation = Quaternion.Euler(0f, 0f, iconRotationZ);
                        item.bulletIcon.color = new Color(1f, 1f, 1f, 1f);
                        var icoRt = item.bulletIcon.GetComponent<RectTransform>();
                        icoRt.sizeDelta = iconSize;
                    }

                    if (item.bulletName != null)
                    {
                        item.bulletName.text = previewNames[i];
                        item.bulletName.color = nameColor;
                        item.bulletName.fontSize = nameFontSize;
                        item.bulletName.gameObject.SetActive(true);
                    }

                    ApplyEditorAnimationPreview(item, i);
                }
                else
                {
                    item.gameObject.SetActive(true);
                    item.ApplyBackground(BulletPreviewVisualType.Empty);
                    if (item.bulletIcon != null)
                        item.bulletIcon.gameObject.SetActive(false);
                    if (item.bulletName != null)
                    {
                        item.bulletName.text = "---";
                        item.bulletName.color = new Color(0.3f, 0.3f, 0.3f, 1f);
                        item.bulletName.gameObject.SetActive(true);
                    }
                }
            }
        }

        private void ApplyEditorAnimationPreview(BulletPreviewItem item, int slotIndex)
        {
            if (editorPreviewMode == PreviewMode.None || slotIndex != 0)
                return;

            if (item == null)
                return;

            var ac = item.transform.Find("AnimatedContent");
            if (ac == null)
                return;

            var acRt = ac.GetComponent<RectTransform>();
            var cg = ac.GetComponent<CanvasGroup>();

            if (editorPreviewMode == PreviewMode.Enemy)
            {
                float alpha = 1f - editorAnimProgress;
                float scale = 1f - editorAnimProgress * 0.3f;
                acRt.localScale = Vector3.one * scale;
                if (cg != null) cg.alpha = alpha;

                if (item.bulletName != null)
                    item.bulletName.color = new Color(nameColor.r, nameColor.g, nameColor.b, alpha);
            }
            else if (editorPreviewMode == PreviewMode.Self)
            {
                Vector2 startPos = Vector2.zero;
                Vector2 targetPos = GetSelfTargetLocalPosition(item);
                Vector2 pos = Vector2.Lerp(startPos, targetPos, editorAnimProgress);
                acRt.anchoredPosition = pos;

                if (editorAnimProgress > 0.7f)
                {
                    float fadeT = (editorAnimProgress - 0.7f) / 0.3f;
                    float alpha = 1f - fadeT;
                    if (cg != null) cg.alpha = alpha;
                    if (item.bulletName != null)
                        item.bulletName.color = new Color(nameColor.r, nameColor.g, nameColor.b, alpha);
                }
                else
                {
                    if (cg != null) cg.alpha = 1f;
                }
            }
        }

        public void ResetEditorPreview()
        {
            editorAnimProgress = 0f;
            editorPreviewMode = PreviewMode.None;

            if (bulletPreviewItems != null)
            {
                foreach (var item in bulletPreviewItems)
                {
                    if (item != null)
                        item.ResetVisualState();
                }
            }

            // Also update background sprites
            LoadBulletSprites();
            if (bulletPreviewItems != null)
            {
                for (int i = 0; i < bulletPreviewItems.Length && i < 3; i++)
                {
                    var item = bulletPreviewItems[i];
                    if (item == null) continue;
                    item.attackBackgroundSprite = attackBackgroundSprite;
                    item.bufferBackgroundSprite = bufferBackgroundSprite;
                    item.emptyBackgroundSprite = emptyBackgroundSprite;
                }
            }

            RefreshEditorPreview();
        }

        public void LoadBulletSprites()
        {
            if (attackBulletSprite == null)
                attackBulletSprite = Resources.Load<Sprite>("BulletPreview/attackzidan");
            if (bufferBulletSprite == null)
                bufferBulletSprite = Resources.Load<Sprite>("BulletPreview/bufferzidan");

            if (attackBackgroundSprite == null)
                attackBackgroundSprite = Resources.Load<Sprite>("BulletPreview/bullet_preview_bg_attack");
            if (bufferBackgroundSprite == null)
                bufferBackgroundSprite = Resources.Load<Sprite>("BulletPreview/bullet_preview_bg_buffer");
            if (emptyBackgroundSprite == null)
                emptyBackgroundSprite = Resources.Load<Sprite>("BulletPreview/bullet_preview_bg_empty");

#if UNITY_EDITOR
            if (attackBulletSprite == null)
                attackBulletSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(
                    "Assets/Art/Gothic/UI/BulletPreview/Raw/attackzidan.png");
            if (bufferBulletSprite == null)
                bufferBulletSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(
                    "Assets/Art/Gothic/UI/BulletPreview/Raw/bufferzidan.png");

            if (attackBackgroundSprite == null)
                attackBackgroundSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(
                    "Assets/Art/Gothic/UI/BulletPreview/Backgrounds/bullet_preview_bg_attack.png");
            if (bufferBackgroundSprite == null)
                bufferBackgroundSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(
                    "Assets/Art/Gothic/UI/BulletPreview/Backgrounds/bullet_preview_bg_buffer.png");
            if (emptyBackgroundSprite == null)
                emptyBackgroundSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(
                    "Assets/Art/Gothic/UI/BulletPreview/Backgrounds/bullet_preview_bg_empty.png");
#endif
        }

        public void Bind(MagazineSystem system)
        {
            if (_isSubscribed && magazineSystem != null)
            {
                magazineSystem.OnMagazineChanged -= RequestRefresh;
                magazineSystem.OnReloadStarted -= OnReloadStarted;
                magazineSystem.OnReloadFinished -= OnReloadFinished;
                magazineSystem.OnCardConsumed -= PlayCurrentBulletConsumed;
            }

            magazineSystem = system;
            magazineSystem.OnMagazineChanged += RequestRefresh;
            magazineSystem.OnReloadStarted += OnReloadStarted;
            magazineSystem.OnReloadFinished += OnReloadFinished;
            magazineSystem.OnCardConsumed += PlayCurrentBulletConsumed;
            _isSubscribed = true;

            RefreshAllSlotsImmediately();
        }

        public void RequestRefresh()
        {
            if (_animationInProgress)
            {
                _pendingRefresh = true;
                return;
            }

            RefreshAllSlotsImmediately();
        }

        public void PlayCurrentBulletConsumed(CardData firedCardSnapshot, bool targetsSelf)
        {
            if (_animationInProgress)
            {
                Debug.LogWarning("[MagazinePreviewUI] Animation already in progress; ignoring duplicate fire notification.");
                return;
            }

            if (bulletPreviewItems == null || bulletPreviewItems.Length == 0)
                return;

            _animationInProgress = true;

            var slot0 = bulletPreviewItems[0];
            if (slot0 == null)
            {
                _animationInProgress = false;
                return;
            }

            slot0.Bind(firedCardSnapshot);

            if (targetsSelf)
            {
                SelfImpactVisualType impactType = firedCardSnapshot != null && firedCardSnapshot.IsOffensive
                    ? SelfImpactVisualType.Red
                    : SelfImpactVisualType.Blue;
                StartCoroutine(PlaySelfAndRefresh(slot0, impactType));
            }
            else
            {
                StartCoroutine(PlayEnemyAndRefresh(slot0));
            }
        }

        private IEnumerator PlayEnemyAndRefresh(BulletPreviewItem item)
        {
            yield return StartCoroutine(item.PlayEnemyConsumeAnimationCoroutine());
            CompleteConsumeAnimation();
        }

        private IEnumerator PlaySelfAndRefresh(BulletPreviewItem item, SelfImpactVisualType impactType)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera : null;

            Vector2 targetLocal = GetSelfTargetLocalPosition(item);
            yield return StartCoroutine(item.PlaySelfConsumeAnimationCoroutine(
                targetLocal, selfTargetRect, uiCamera));

            if (impactFeedback != null)
            {
                if (impactType == SelfImpactVisualType.Red)
                    impactFeedback.PlayRedImpact();
                else
                    impactFeedback.PlayBlueImpact();
            }

            CompleteConsumeAnimation();
        }

        private Vector2 GetSelfTargetLocalPosition(BulletPreviewItem item)
        {
            if (selfTargetRect == null || item == null || item.bulletIcon == null)
                return Vector2.left * 300f;

            var iconRt = item.bulletIcon.GetComponent<RectTransform>();
            if (iconRt == null)
                return Vector2.left * 300f;

            Canvas canvas = GetComponentInParent<Canvas>();
            Camera cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera : null;

            Vector2 targetScreenPoint = RectTransformUtility.WorldToScreenPoint(cam, selfTargetRect.position);

            RectTransform parentRect = iconRt.parent as RectTransform;
            if (parentRect == null)
                parentRect = transform as RectTransform;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, targetScreenPoint, cam, out Vector2 localPoint);

            return localPoint;
        }

        private void CompleteConsumeAnimation()
        {
            if (bulletPreviewItems != null && bulletPreviewItems.Length > 0 && bulletPreviewItems[0] != null)
                bulletPreviewItems[0].ResetVisualState();

            _animationInProgress = false;
            RefreshAllSlotsImmediately();

            if (_pendingRefresh)
            {
                _pendingRefresh = false;
                RefreshAllSlotsImmediately();
            }
        }

        private void RefreshAllSlotsImmediately()
        {
            if (magazineSystem == null)
                return;

            List<CardData> cards = magazineSystem.GetPreviewCards(previewCount);
            int currentIdx = magazineSystem.CurrentIndex;

            if (previewSlots == null || previewSlots.Length == 0)
            {
                EnsureSlotsExist();
                if (previewSlots == null)
                    return;
            }

            for (int i = 0; i < previewSlots.Length; i++)
            {
                bool isCurrentCard = (i == 0 && currentIdx < magazineSystem.LoadedCards.Count);

                if (i < cards.Count)
                {
                    previewSlots[i].SetCard(cards[i], isCurrentCard);

                    if (bulletPreviewItems != null && i < bulletPreviewItems.Length && bulletPreviewItems[i] != null)
                    {
                        bulletPreviewItems[i].SetupBullet(cards[i]);
                        bulletPreviewItems[i].gameObject.SetActive(true);
                    }
                }
                else
                {
                    previewSlots[i].SetEmpty();

                    if (bulletPreviewItems != null && i < bulletPreviewItems.Length && bulletPreviewItems[i] != null)
                    {
                        bulletPreviewItems[i].SetEmpty();
                        bulletPreviewItems[i].gameObject.SetActive(true);
                    }
                }
            }
        }

        private void OnReloadStarted()
        {
            if (_animationInProgress)
            {
                _pendingRefresh = true;
                return;
            }

            _animationInProgress = false;
            _pendingRefresh = false;
            if (previewSlots != null)
            {
                foreach (CardSlotUI slot in previewSlots)
                    if (slot != null)
                        slot.SetReloading();
            }
            if (bulletPreviewItems != null)
            {
                foreach (var item in bulletPreviewItems)
                    if (item != null)
                        item.SetEmpty();
            }
        }

        private void OnReloadFinished()
        {
            if (_animationInProgress)
            {
                _pendingRefresh = true;
                return;
            }

            RefreshAllSlotsImmediately();
        }

        public void ApplyLayoutSettings()
        {
            if (_layoutGroup == null)
                _layoutGroup = GetComponent<HorizontalLayoutGroup>();
            if (_layoutGroup != null)
            {
                _layoutGroup.childAlignment = TextAnchor.MiddleCenter;
                _layoutGroup.childForceExpandWidth = false;
                _layoutGroup.childForceExpandHeight = false;
                _layoutGroup.childControlWidth = false;
                _layoutGroup.childControlHeight = false;
            }
        }

        private void EnsureSlotsExist()
        {
            List<CardSlotUI> cardSlots = new List<CardSlotUI>();
            List<BulletPreviewItem> bulletItems = new List<BulletPreviewItem>();

            _layoutGroup = GetComponent<HorizontalLayoutGroup>();
            ApplyLayoutSettings();

            for (int i = 0; i < previewCount; i++)
            {
                string slotName = $"PreviewSlot_{i}";
                Transform existingSlot = transform.Find(slotName);
                GameObject slotObj;
                if (existingSlot != null)
                {
                    slotObj = existingSlot.gameObject;
                }
                else
                {
                    slotObj = CreateSlotObject(slotName, transform);
                }

                RectTransform slotRt = slotObj.GetComponent<RectTransform>();
                float slotHeight = iconSize.y + nameFontSize + 64f;
                float slotWidth = Mathf.Max(150f, iconSize.x + 20f);
                slotRt.sizeDelta = new Vector2(slotWidth, slotHeight);

                CardSlotUI cardSlot = slotObj.GetComponent<CardSlotUI>();
                if (cardSlot == null)
                    cardSlot = slotObj.AddComponent<CardSlotUI>();

                cardSlot.backgroundImage = slotObj.GetComponent<Image>();
                if (cardSlot.backgroundImage != null)
                {
                    cardSlot.backgroundImage.raycastTarget = false;
                    cardSlot.backgroundImage.enabled = false;
                }

                cardSlot.effectText = EnsureEffectText(slotObj);

                BulletPreviewItem bulletItem = slotObj.GetComponent<BulletPreviewItem>();
                if (bulletItem == null)
                    bulletItem = slotObj.AddComponent<BulletPreviewItem>();

                // Remove stale CanvasGroup from root — belongs on AnimatedContent
                CanvasGroup staleCg = slotObj.GetComponent<CanvasGroup>();
                if (staleCg != null && Application.isPlaying)
                    Destroy(staleCg);
                else if (staleCg != null)
                    DestroyImmediate(staleCg);

                bulletItem.attackSprite = attackBulletSprite;
                bulletItem.bufferSprite = bufferBulletSprite;
                bulletItem.attackBackgroundSprite = attackBackgroundSprite;
                bulletItem.bufferBackgroundSprite = bufferBackgroundSprite;
                bulletItem.emptyBackgroundSprite = emptyBackgroundSprite;
                bulletItem.iconSize = iconSize;
                bulletItem.iconRotationZ = iconRotationZ;
                bulletItem.nameFontSize = nameFontSize;
                bulletItem.nameColor = nameColor;
                bulletItem.attackAnimDuration = attackAnimDuration;
                bulletItem.selfAnimSpeed = selfAnimSpeed;

                EnsureSlotBackground(slotObj, bulletItem);
                GameObject animatedContent = EnsureAnimatedContent(slotObj);
                EnsureBulletIcon(animatedContent, bulletItem);
                EnsureBulletName(animatedContent, bulletItem);

                cardSlots.Add(cardSlot);
                bulletItems.Add(bulletItem);
            }

            previewSlots = cardSlots.ToArray();
            bulletPreviewItems = bulletItems.ToArray();

            if (!Application.isPlaying)
            {
                for (int i = 0; i < bulletPreviewItems.Length; i++)
                {
                    var item = bulletPreviewItems[i];
                    if (item != null)
                    {
                        item.attackSprite = attackBulletSprite;
                        item.bufferSprite = bufferBulletSprite;
                        item.EnsureReferences();
                        item.RecordBaseState();
                    }
                }
            }
            else
            {
                RefreshAllSlotsImmediately();
            }
        }

        private GameObject EnsureAnimatedContent(GameObject parent)
        {
            Transform existing = parent.transform.Find("AnimatedContent");
            if (existing != null)
                return existing.gameObject;

            GameObject ac = new GameObject("AnimatedContent");
            ac.transform.SetParent(parent.transform, false);

            RectTransform rt = ac.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // CanvasGroup on AnimatedContent, not on root, so SlotBackground is untouched
            if (ac.GetComponent<CanvasGroup>() == null)
                ac.AddComponent<CanvasGroup>();

            return ac;
        }

        private void EnsureSlotBackground(GameObject parent, BulletPreviewItem item)
        {
            Transform existing = parent.transform.Find("SlotBackground");
            GameObject bgObj;
            if (existing != null)
            {
                bgObj = existing.gameObject;
            }
            else
            {
                bgObj = new GameObject("SlotBackground");
                bgObj.transform.SetParent(parent.transform, false);
                bgObj.transform.SetAsFirstSibling();
            }

            Image img = bgObj.GetComponent<Image>();
            if (img == null)
                img = bgObj.AddComponent<Image>();
            img.raycastTarget = false;
            img.preserveAspect = true;

            RectTransform rt = bgObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;

            item.slotBackground = img;
        }

        private void EnsureBulletIcon(GameObject parent, BulletPreviewItem item)
        {
            Transform existing = parent.transform.Find("BulletIcon");
            GameObject iconObj;
            if (existing != null)
            {
                iconObj = existing.gameObject;
            }
            else
            {
                iconObj = new GameObject("BulletIcon");
                iconObj.transform.SetParent(parent.transform, false);
            }

            Image img = iconObj.GetComponent<Image>();
            if (img == null)
                img = iconObj.AddComponent<Image>();
            img.raycastTarget = false;
            img.preserveAspect = true;

            RectTransform rt = iconObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = iconSize;

            item.bulletIcon = img;
        }

        private void EnsureBulletName(GameObject parent, BulletPreviewItem item)
        {
            Transform existing = parent.transform.Find("BulletName");
            GameObject nameObj;
            if (existing != null)
            {
                nameObj = existing.gameObject;
            }
            else
            {
                nameObj = new GameObject("BulletName");
                nameObj.transform.SetParent(parent.transform, false);
            }

            TextMeshProUGUI tmp = nameObj.GetComponent<TextMeshProUGUI>();
            if (tmp == null)
            {
                Text legacyText = nameObj.GetComponent<Text>();
                if (legacyText != null)
                {
                    if (Application.isPlaying)
                        Destroy(legacyText);
                    else
                        DestroyImmediate(legacyText);
                }
                tmp = nameObj.AddComponent<TextMeshProUGUI>();
            }

            tmp.fontSize = nameFontSize;
            tmp.color = nameColor;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            tmp.text = "---";

            if (nameObj.GetComponent<UnityEngine.UI.Outline>() == null)
            {
                var ol = nameObj.AddComponent<UnityEngine.UI.Outline>();
                ol.effectColor = new Color(0.4f, 0.2f, 0f, 0.8f);
                ol.effectDistance = new Vector2(1.5f, -1.5f);
            }

            RectTransform rt = nameObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(iconSize.x + 40f, nameFontSize + 8f);

            item.bulletName = tmp;
        }

        private Text EnsureEffectText(GameObject parent)
        {
            Transform existing = parent.transform.Find("EffectText");
            GameObject effObj;
            if (existing != null)
            {
                effObj = existing.gameObject;
            }
            else
            {
                effObj = new GameObject("EffectText");
                effObj.transform.SetParent(parent.transform, false);
            }

            Text txt = effObj.GetComponent<Text>();
            if (txt == null)
                txt = effObj.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 12;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = new Color(0.6f, 0.6f, 0.6f);
            txt.raycastTarget = false;

            RectTransform effRt = effObj.GetComponent<RectTransform>();
            effRt.anchorMin = new Vector2(0f, 0f);
            effRt.anchorMax = new Vector2(1f, 0f);
            effRt.pivot = new Vector2(0.5f, 0f);
            effRt.anchoredPosition = new Vector2(0f, 4f);
            effRt.sizeDelta = new Vector2(0f, 18f);

            return txt;
        }

        private static GameObject CreateSlotObject(string name, Transform parent)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(150, 60);

            return obj;
        }

        private void OnDestroy()
        {
            if (magazineSystem != null)
            {
                magazineSystem.OnMagazineChanged -= RequestRefresh;
                magazineSystem.OnReloadStarted -= OnReloadStarted;
                magazineSystem.OnReloadFinished -= OnReloadFinished;
                magazineSystem.OnCardConsumed -= PlayCurrentBulletConsumed;
            }
        }
    }
}
