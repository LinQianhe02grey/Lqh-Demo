using UnityEngine;
using UnityEngine.UI;

namespace Cardwin.Modules
{
    /// <summary>
    /// Note color / required input. Red = left click, Blue = right click.
    /// </summary>
    public enum RhythmNoteType
    {
        Red,
        Blue
    }

    /// <summary>
    /// Serializable chart entry. hitTime is in seconds relative to song start
    /// (audioSource.time). type decides which mouse button is required.
    /// </summary>
    [System.Serializable]
    public class RhythmNoteData
    {
        public float hitTime;
        public RhythmNoteType type;

        public RhythmNoteData() { }

        public RhythmNoteData(float hitTime, RhythmNoteType type)
        {
            this.hitTime = hitTime;
            this.type = type;
        }
    }

    /// <summary>
    /// Lightweight visual note. The RhythmGameController owns all movement and
    /// judgement logic; this component only stores per-note state and exposes the
    /// RectTransform / Image so the controller can position and recolor it.
    /// Lives under RhythmGameCanvas/NoteTrack (Screen Space Overlay, pixel space).
    /// </summary>
    public sealed class RhythmNote : MonoBehaviour
    {
        public RhythmNoteType type;
        public float hitTime;
        public bool judged;

        private RectTransform _rect;
        private Image _image;

        public RectTransform Rect => _rect;
        public Image Image => _image;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _image = GetComponent<Image>();
        }

        public void Setup(RhythmNoteType noteType, float noteHitTime, Color color, float diameter)
        {
            if (_rect == null) _rect = GetComponent<RectTransform>();
            if (_image == null) _image = GetComponent<Image>();

            type = noteType;
            hitTime = noteHitTime;
            judged = false;

            _rect.anchorMin = Vector2.zero;
            _rect.anchorMax = Vector2.zero;
            _rect.pivot = new Vector2(0.5f, 0.5f);
            _rect.sizeDelta = new Vector2(diameter, diameter);

            if (_image != null)
                _image.color = color;
        }

        public void SetAnchoredPosition(float x, float y)
        {
            if (_rect == null) _rect = GetComponent<RectTransform>();
            _rect.anchoredPosition = new Vector2(x, y);
        }

        public float CurrentX
        {
            get
            {
                if (_rect == null) _rect = GetComponent<RectTransform>();
                return _rect.anchoredPosition.x;
            }
        }

        public void SetAlpha(float a)
        {
            if (_image == null) _image = GetComponent<Image>();
            if (_image == null) return;
            Color c = _image.color;
            c.a = a;
            _image.color = c;
        }
    }
}
