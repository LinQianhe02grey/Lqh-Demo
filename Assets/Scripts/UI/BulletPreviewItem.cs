using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cardwin.Cards;

namespace Cardwin.UI
{
    public enum BulletPreviewVisualType
    {
        Empty,
        Attack,
        Buffer
    }

    [ExecuteAlways]
    public class BulletPreviewItem : MonoBehaviour
    {
        [Header("Bullet Sprites")]
        public Sprite attackSprite;
        public Sprite bufferSprite;

        [Header("Background Sprites")]
        public Sprite attackBackgroundSprite;
        public Sprite bufferBackgroundSprite;
        public Sprite emptyBackgroundSprite;

        [Header("Bullet Icon")]
        public Vector2 iconSize = new Vector2(64, 64);
        [Range(-180f, 180f)]
        public float iconRotationZ = 90f;

        [Header("Name Text")]
        public float nameFontSize = 14f;
        public Color nameColor = new Color(1f, 0.85f, 0.3f, 1f);

        [Header("Animation")]
        [Range(0.1f, 2f)]
        public float attackAnimDuration = 0.5f;
        [Range(0.1f, 1f)]
        public float selfAnimSpeed = 800f;

        [Header("References (auto-setup)")]
        public Image slotBackground;
        public Image bulletIcon;
        public TextMeshProUGUI bulletName;

        private CardData _currentCard;
        private CanvasGroup _canvasGroup;
        private GameObject _animatedContent;

        private Vector2 _animatedContentBasePosition;
        private Vector3 _animatedContentBaseScale;
        private float _animatedContentBaseAlpha;
        private Color _baseIconColor;
        private Color _baseNameColor;

        public CardData CurrentCard => _currentCard;
        public bool IsAnimating { get; private set; }

        public BulletPreviewVisualType ResolveVisualType(CardData card)
        {
            if (card == null)
                return BulletPreviewVisualType.Empty;
            return card.IsOffensive
                ? BulletPreviewVisualType.Attack
                : BulletPreviewVisualType.Buffer;
        }

        public void ApplyBackground(BulletPreviewVisualType visualType)
        {
            if (slotBackground == null)
                return;

            switch (visualType)
            {
                case BulletPreviewVisualType.Attack:
                    slotBackground.sprite = attackBackgroundSprite;
                    break;
                case BulletPreviewVisualType.Buffer:
                    slotBackground.sprite = bufferBackgroundSprite;
                    break;
                default:
                    slotBackground.sprite = emptyBackgroundSprite;
                    break;
            }
            slotBackground.color = Color.white;
            slotBackground.gameObject.SetActive(slotBackground.sprite != null);
        }

        private void OnEnable()
        {
            EnsureReferences();
            RecordBaseState();
        }

        public void EnsureReferences()
        {
            if (_animatedContent == null)
            {
                Transform acTrans = transform.Find("AnimatedContent");
                if (acTrans != null)
                    _animatedContent = acTrans.gameObject;
            }

            if (slotBackground == null)
            {
                Transform bgTrans = transform.Find("SlotBackground");
                if (bgTrans != null)
                    slotBackground = bgTrans.GetComponent<Image>();
            }

            if (bulletIcon == null)
            {
                Transform iconTrans = _animatedContent != null
                    ? _animatedContent.transform.Find("BulletIcon")
                    : transform.Find("BulletIcon");
                if (iconTrans != null)
                    bulletIcon = iconTrans.GetComponent<Image>();
            }

            if (bulletName == null)
            {
                Transform nameTrans = _animatedContent != null
                    ? _animatedContent.transform.Find("BulletName")
                    : transform.Find("BulletName");
                if (nameTrans != null)
                    bulletName = nameTrans.GetComponent<TextMeshProUGUI>();
            }

            if (_canvasGroup == null)
            {
                if (_animatedContent != null)
                    _canvasGroup = _animatedContent.GetComponent<CanvasGroup>();
                if (_canvasGroup == null && _animatedContent != null)
                    _canvasGroup = _animatedContent.AddComponent<CanvasGroup>();
            }
        }

        public void RecordBaseState()
        {
            EnsureReferences();

            if (_animatedContent != null)
            {
                var acRt = _animatedContent.GetComponent<RectTransform>();
                if (acRt != null)
                {
                    _animatedContentBasePosition = acRt.anchoredPosition;
                    _animatedContentBaseScale = acRt.localScale;
                }
            }

            if (_canvasGroup != null)
                _animatedContentBaseAlpha = _canvasGroup.alpha;

            if (bulletIcon != null)
                _baseIconColor = bulletIcon.color;

            if (bulletName != null)
                _baseNameColor = bulletName.color;
        }

        public void ResetVisualState()
        {
            if (_animatedContent != null)
            {
                var acRt = _animatedContent.GetComponent<RectTransform>();
                if (acRt != null)
                {
                    acRt.anchoredPosition = _animatedContentBasePosition;
                    acRt.localScale = _animatedContentBaseScale;
                }
            }

            if (_canvasGroup != null)
                _canvasGroup.alpha = _animatedContentBaseAlpha;

            if (bulletIcon != null)
            {
                bulletIcon.color = _baseIconColor;
                bulletIcon.gameObject.SetActive(true);
            }

            if (bulletName != null)
            {
                bulletName.color = _baseNameColor;
                bulletName.gameObject.SetActive(true);
            }
        }

        public void Bind(CardData card)
        {
            if (!Application.isPlaying)
                return;

            _currentCard = card;
            EnsureReferences();

            if (card == null)
            {
                SetEmpty();
                return;
            }

            bool isOffensive = card.IsOffensive;
            Sprite sprite = isOffensive ? attackSprite : bufferSprite;
            string cardName = card.cardName;
            BulletPreviewVisualType visualType = ResolveVisualType(card);

            ResetVisualState();
            ApplyBackground(visualType);

            if (bulletIcon != null)
            {
                bulletIcon.sprite = sprite;
                bulletIcon.gameObject.SetActive(true);
                bulletIcon.transform.localRotation = Quaternion.Euler(0f, 0f, iconRotationZ);
                var rt = bulletIcon.GetComponent<RectTransform>();
                rt.sizeDelta = iconSize;
            }

            if (bulletName != null)
            {
                bulletName.text = cardName;
                bulletName.color = nameColor;
                bulletName.fontSize = nameFontSize;
                bulletName.gameObject.SetActive(true);
            }

            RecordBaseState();
        }

        public void SetEmpty()
        {
            _currentCard = null;
            ApplyBackground(BulletPreviewVisualType.Empty);

            if (bulletIcon != null)
                bulletIcon.gameObject.SetActive(false);

            if (bulletName != null)
            {
                bulletName.text = "---";
                bulletName.color = new Color(0.3f, 0.3f, 0.3f, 1f);
                bulletName.gameObject.SetActive(true);
            }
        }

        public void SetupBullet(CardData card)
        {
            EnsureReferences();

            if (card == null)
            {
                SetEmpty();
                return;
            }

            _currentCard = card;
            bool isOffensive = card.IsOffensive;
            Sprite sprite = isOffensive ? attackSprite : bufferSprite;
            string cardName = card.cardName;
            BulletPreviewVisualType visualType = ResolveVisualType(card);

            ApplyBackground(visualType);

            if (bulletIcon != null)
            {
                bulletIcon.sprite = sprite;
                bulletIcon.gameObject.SetActive(true);
                bulletIcon.transform.localRotation = Quaternion.Euler(0f, 0f, iconRotationZ);
                var rt = bulletIcon.GetComponent<RectTransform>();
                rt.sizeDelta = iconSize;
            }

            if (bulletName != null)
            {
                bulletName.text = cardName;
                bulletName.color = nameColor;
                bulletName.fontSize = nameFontSize;
                bulletName.gameObject.SetActive(true);
            }

            RecordBaseState();
        }

        public IEnumerator PlayEnemyConsumeAnimationCoroutine()
        {
            IsAnimating = true;
            EnsureReferences();

            if (_animatedContent == null)
            {
                IsAnimating = false;
                yield break;
            }

            var acRt = _animatedContent.GetComponent<RectTransform>();
            CanvasGroup cg = _canvasGroup;
            Color originalIconColor = bulletIcon != null ? bulletIcon.color : Color.white;
            Color originalNameColor = bulletName != null ? bulletName.color : Color.white;

            float elapsed = 0f;
            while (elapsed < attackAnimDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / attackAnimDuration;
                float scale = 1f - t * 0.3f;
                float alpha = 1f - t;

                acRt.localScale = _animatedContentBaseScale * scale;
                if (cg != null) cg.alpha = alpha;

                if (bulletName != null)
                    bulletName.color = new Color(originalNameColor.r, originalNameColor.g, originalNameColor.b, alpha);

                yield return null;
            }

            if (cg != null) cg.alpha = 0f;
            if (bulletName != null)
                bulletName.color = new Color(originalNameColor.r, originalNameColor.g, originalNameColor.b, 0f);

            IsAnimating = false;
        }

        private static bool IsOverlappingTarget(RectTransform movingRt, RectTransform targetRt, Camera uiCamera)
        {
            if (movingRt == null || targetRt == null)
                return false;

            Rect movingRect = GetScreenRect(movingRt, uiCamera);
            Rect targetRect = GetScreenRect(targetRt, uiCamera);
            return movingRect.Overlaps(targetRect);
        }

        private static Rect GetScreenRect(RectTransform rt, Camera uiCamera)
        {
            var corners = new Vector3[4];
            rt.GetWorldCorners(corners);

            Vector2 min = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[0]);
            Vector2 max = min;

            for (int i = 1; i < corners.Length; i++)
            {
                Vector2 point = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[i]);
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        public IEnumerator PlaySelfConsumeAnimationCoroutine(
            Vector2 targetLocalPosition,
            RectTransform targetRect,
            Camera uiCamera)
        {
            IsAnimating = true;
            EnsureReferences();

            if (_animatedContent == null)
            {
                IsAnimating = false;
                yield break;
            }

            var acRt = _animatedContent.GetComponent<RectTransform>();
            CanvasGroup cg = _canvasGroup;
            Color originalNameColor = bulletName != null ? bulletName.color : Color.white;
            Vector2 startLocal = acRt.anchoredPosition;

            float distance = Vector2.Distance(startLocal, targetLocalPosition);
            float totalDuration = distance / selfAnimSpeed;
            totalDuration = Mathf.Clamp(totalDuration, 0.2f, 0.8f);

            int subSteps = Mathf.Max(1, Mathf.CeilToInt(distance / 5f));

            for (int step = 1; step <= subSteps; step++)
            {
                float t = (float)step / subSteps;
                float easedT = t * t * (3f - 2f * t);
                acRt.anchoredPosition = Vector2.Lerp(startLocal, targetLocalPosition, easedT);

                bool overlapping = IsOverlappingTarget(acRt, targetRect, uiCamera);
                if (overlapping)
                {
                    if (cg != null) cg.alpha = 0f;
                    if (bulletName != null)
                        bulletName.color = new Color(originalNameColor.r, originalNameColor.g, originalNameColor.b, 0f);
                    IsAnimating = false;
                    yield break;
                }

                if (t > 0.7f)
                {
                    float fadeT = (t - 0.7f) / 0.3f;
                    float alpha = 1f - fadeT;
                    if (cg != null) cg.alpha = alpha;
                    if (bulletName != null)
                        bulletName.color = new Color(originalNameColor.r, originalNameColor.g, originalNameColor.b, alpha);
                }

                yield return null;
            }

            if (cg != null) cg.alpha = 0f;
            if (bulletName != null)
                bulletName.color = new Color(originalNameColor.r, originalNameColor.g, originalNameColor.b, 0f);

            IsAnimating = false;
        }
    }
}
