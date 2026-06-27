using UnityEngine;

namespace Cardwin.Characters
{
    public class GothicNunAssemblyDebug : MonoBehaviour
    {
        [Header("Reference Image Debug")]
        [SerializeField] private SpriteRenderer _referenceRenderer;
        [SerializeField] private bool _showReference;
        [SerializeField] [Range(0f, 1f)] private float _referenceAlpha = 0.35f;

        [Header("Actions")]
        [SerializeField] private bool _resetAllTransforms;

        public bool ShowReference
        {
            get => _showReference;
            set
            {
                _showReference = value;
                ApplyReferenceState();
            }
        }

        public float ReferenceAlpha
        {
            get => _referenceAlpha;
            set
            {
                _referenceAlpha = Mathf.Clamp01(value);
                ApplyReferenceState();
            }
        }

        void OnValidate()
        {
            if (_resetAllTransforms)
            {
                _resetAllTransforms = false;
                ResetAllToZero();
            }
            ApplyReferenceState();
        }

        void Start()
        {
            ApplyReferenceState();
        }

        void ApplyReferenceState()
        {
            if (_referenceRenderer != null)
            {
                _referenceRenderer.enabled = _showReference;
                var c = _referenceRenderer.color;
                c.a = _referenceAlpha;
                _referenceRenderer.color = c;
            }
        }

        public void ResetAllToZero()
        {
            ResetChildTransforms(transform, true);
        }

        void ResetChildTransforms(Transform parent, bool isRoot)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                child.localPosition = Vector3.zero;
                child.localRotation = Quaternion.identity;
                child.localScale = Vector3.one;
                ResetChildTransforms(child, false);
            }
        }
    }
}
