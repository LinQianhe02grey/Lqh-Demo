using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Cardwin.UI
{
    public enum SelfImpactVisualType
    {
        Red,
        Blue
    }

    [ExecuteAlways]
    public class PlayerStatusImpactFeedback : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private RectTransform feedbackTarget;

        [Header("Glow")]
        [SerializeField] private Image impactGlow;
        [SerializeField] private Color redGlowColor = new Color(1f, 0.15f, 0.1f, 0.9f);
        [SerializeField] private Color blueGlowColor = new Color(0.15f, 0.3f, 1f, 0.7f);
        [SerializeField] private AnimationCurve glowCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        [Header("Red Shake")]
        [SerializeField] private float redShakeAmplitude = 5f;
        [SerializeField] private float redShakeFrequency = 26f;

        [Header("Duration")]
        [SerializeField] private float feedbackDuration = 0.4f;

        private Coroutine _activeFeedback;
        private Vector2 _baseAnchoredPosition;
        private bool _baseRecorded;

        [ContextMenu("Preview Red Impact")]
        private void PreviewRedImpact()
        {
            if (Application.isPlaying)
            {
                PlayRedImpact();
                return;
            }
#if UNITY_EDITOR
            PlayFeedbackEditor(SelfImpactVisualType.Red);
#endif
        }

        [ContextMenu("Preview Blue Impact")]
        private void PreviewBlueImpact()
        {
            if (Application.isPlaying)
            {
                PlayBlueImpact();
                return;
            }
#if UNITY_EDITOR
            PlayFeedbackEditor(SelfImpactVisualType.Blue);
#endif
        }

        [ContextMenu("Reset Feedback")]
        private void ResetFeedback()
        {
            if (_activeFeedback != null)
            {
                StopCoroutine(_activeFeedback);
                _activeFeedback = null;
            }
            ResetVisual();
        }

#if UNITY_EDITOR
        private void PlayFeedbackEditor(SelfImpactVisualType type)
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                ResetVisual();
                if (impactGlow != null)
                {
                    impactGlow.color = type == SelfImpactVisualType.Red
                        ? redGlowColor : blueGlowColor;
                }

                UnityEditor.EditorApplication.delayCall += () =>
                {
                    ResetVisual();
                };
            };
        }
#endif

        private void OnEnable()
        {
            RecordBasePosition();
        }

        private void OnDisable()
        {
            ResetVisual();
        }

        private void RecordBasePosition()
        {
            if (feedbackTarget == null)
                feedbackTarget = GetComponent<RectTransform>();

            if (feedbackTarget != null)
            {
                _baseAnchoredPosition = feedbackTarget.anchoredPosition;
                _baseRecorded = true;
            }
        }

        public void PlayRedImpact()
        {
            Play(SelfImpactVisualType.Red);
        }

        public void PlayBlueImpact()
        {
            Play(SelfImpactVisualType.Blue);
        }

        public void Play(SelfImpactVisualType type)
        {
            if (!_baseRecorded)
                RecordBasePosition();

            if (_activeFeedback != null)
            {
                StopCoroutine(_activeFeedback);
                _activeFeedback = null;
            }

            ResetVisual();
            _activeFeedback = StartCoroutine(FeedbackCoroutine(type));
        }

        private IEnumerator FeedbackCoroutine(SelfImpactVisualType type)
        {
            bool isRed = type == SelfImpactVisualType.Red;
            Color glowColor = isRed ? redGlowColor : blueGlowColor;
            float elapsed = 0f;

            while (elapsed < feedbackDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / feedbackDuration);
                float curve = glowCurve.Evaluate(t);

                if (impactGlow != null)
                    impactGlow.color = new Color(glowColor.r, glowColor.g, glowColor.b, glowColor.a * curve);

                if (isRed && feedbackTarget != null)
                {
                    float shakeStrength = redShakeAmplitude * curve;
                    float wave = Mathf.Sin(elapsed * redShakeFrequency * Mathf.PI * 2f);
                    Vector2 shakeOffset = Random.insideUnitCircle * shakeStrength * (1f + wave * 0.3f);
                    feedbackTarget.anchoredPosition = _baseAnchoredPosition + shakeOffset;
                }

                yield return null;
            }

            ResetVisual();
            _activeFeedback = null;
        }

        private void ResetVisual()
        {
            if (impactGlow != null)
                impactGlow.color = new Color(1f, 1f, 1f, 0f);

            if (_baseRecorded && feedbackTarget != null)
                feedbackTarget.anchoredPosition = _baseAnchoredPosition;
        }
    }
}
