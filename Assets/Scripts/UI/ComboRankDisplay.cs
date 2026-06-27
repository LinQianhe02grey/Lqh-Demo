using UnityEngine;
using UnityEngine.UI;

namespace Cardwin.UI
{
    public enum ComboRankPreview { D, C, B, A, S }

    [ExecuteAlways]
    public class ComboRankDisplay : MonoBehaviour
    {
        [Header("Rank Sprites")]
        [SerializeField] private Sprite rankDSprite;
        [SerializeField] private Sprite rankCSprite;
        [SerializeField] private Sprite rankBSprite;
        [SerializeField] private Sprite rankASprite;
        [SerializeField] private Sprite rankSSprite;

        [Header("Rank Image")]
        [SerializeField] private Image rankImage;

        [Header("Editor Preview")]
        [SerializeField] private bool enableEditorPreview = true;
        [SerializeField] private ComboRankPreview previewRank = ComboRankPreview.S;

        private string _currentRank = "-";

        private void OnValidate()
        {
            if (!Application.isPlaying && enableEditorPreview)
                ApplyEditorPreview();
        }

        private void OnEnable()
        {
            if (!Application.isPlaying && enableEditorPreview)
                ApplyEditorPreview();
        }

        private void ApplyEditorPreview()
        {
            string rank = previewRank.ToString();
            _currentRank = rank;
            SetSpriteOnly(rank);
        }

        public void ApplyRankVisual(string rank)
        {
            if (rank == _currentRank)
                return;

            _currentRank = rank;
            SetSpriteOnly(rank);
        }

        private void SetSpriteOnly(string rank)
        {
            if (rankImage == null)
                return;

            Sprite spr = GetRankSprite(rank);
            rankImage.sprite = spr;
            rankImage.gameObject.SetActive(spr != null);
        }

        private Sprite GetRankSprite(string rank)
        {
            switch (rank)
            {
                case "S": return rankSSprite;
                case "A": return rankASprite;
                case "B": return rankBSprite;
                case "C": return rankCSprite;
                case "D": return rankDSprite;
                default: return null;
            }
        }

        public void ClearRank()
        {
            _currentRank = "-";
            if (rankImage != null)
            {
                rankImage.sprite = null;
                rankImage.gameObject.SetActive(false);
            }
        }
    }
}
