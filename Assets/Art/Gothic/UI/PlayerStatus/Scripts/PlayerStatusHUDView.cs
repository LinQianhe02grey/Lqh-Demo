using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cardwin.UI
{
    public class PlayerStatusHUDView : MonoBehaviour
    {
        [Header("HP Bar")]
        [SerializeField] private Image _hpFill;
        [SerializeField] private TMP_Text _hpText;

        [Header("MP Bar")]
        [SerializeField] private Image _mpFill;
        [SerializeField] private TMP_Text _mpText;

        [Header("Shield Bar")]
        [SerializeField] private Image _shieldFill;
        [SerializeField] private TMP_Text _shieldText;

        [Header("Status Effect Icons")]
        [SerializeField] private StatusEffectIconStripView _statusEffectStrip;

        [Header("Editor Preview")]
        [SerializeField, Range(0f, 1f)] private float previewHealth = 1f;
        [SerializeField, Range(0f, 1f)] private float previewMana = 0.75f;
        [SerializeField, Range(0f, 1f)] private float previewShield = 0.45f;

        private const int PreviewMaxHp = 100;
        private const int PreviewMaxMp = 100;
        private const int PreviewMaxShield = 100;

        public StatusEffectIconStripView StatusEffectStrip => _statusEffectStrip;

        private void OnValidate()
        {
            if (!Application.isPlaying)
                RefreshPreview();
        }

        private void RefreshPreview()
        {
            SetHPBar(previewHealth, PreviewMaxHp, PreviewMaxHp);
            SetMPBar(previewMana, PreviewMaxMp, PreviewMaxMp);
            SetShieldBar(previewShield, PreviewMaxShield, PreviewMaxShield);
        }

        public void SetHPBar(float current, float max, float displayMax)
        {
            if (_hpFill != null)
                _hpFill.fillAmount = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            if (_hpText != null)
                _hpText.text = Mathf.RoundToInt(current) + " / " + Mathf.RoundToInt(displayMax);
        }

        public void SetMPBar(float current, float max, float displayMax)
        {
            if (_mpFill != null)
                _mpFill.fillAmount = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            if (_mpText != null)
                _mpText.text = Mathf.RoundToInt(current) + " / " + Mathf.RoundToInt(displayMax);
        }

        public void SetShieldBar(float current, float max, float displayMax)
        {
            if (_shieldFill != null)
                _shieldFill.fillAmount = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            if (_shieldText != null)
                _shieldText.text = Mathf.RoundToInt(current) + " / " + Mathf.RoundToInt(displayMax);
        }
    }
}
