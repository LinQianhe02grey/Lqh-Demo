using UnityEngine;
using UnityEngine.Events;
using Cardwin.Combat;

namespace Cardwin.Level
{
    public class BossPortal : MonoBehaviour
    {
        [Header("Portal Components")]
        [SerializeField]
        private SpriteRenderer portalVisualRenderer;

        [SerializeField]
        private SpriteRenderer portalGlowRenderer;

        [SerializeField]
        private GameObject interactionHint;

        [SerializeField]
        private Collider2D portalTrigger;

        [Header("Events")]
        [SerializeField]
        private UnityEvent onPlayerEnterPortal;

        [Header("Temporary Debug")]
        [Tooltip(
            "Temporary debug option. " +
            "When enabled, the portal is available immediately " +
            "without clearing enemies.")]
        [SerializeField]
        private bool forceOpenForTesting = true;

        [Header("Editor")]
        [SerializeField]
        private bool showPreviewInEditor = true;

        private bool _isAvailable;
        private bool _transitionStarted;

        public bool IsAvailable => _isAvailable;

        private void Awake()
        {
            if (portalTrigger == null)
            {
                portalTrigger = GetComponentInChildren<Collider2D>();
            }

            if (portalTrigger != null && !portalTrigger.isTrigger)
            {
                portalTrigger.isTrigger = true;
            }

#if UNITY_EDITOR
            // Edit-mode preview only. ApplyEditorPreview lives in the UNITY_EDITOR block,
            // so this branch must be compiled out of Player builds (it never runs there:
            // Application.isPlaying is always true in a build).
            if (!Application.isPlaying)
            {
                ApplyEditorPreview();
                return;
            }
#endif

            SetPortalAvailable(false);
        }

        private void Start()
        {
            if (!Application.isPlaying) return;

            if (forceOpenForTesting)
            {
                Debug.LogWarning(
                    "[BossPortal] Force Open For Testing is enabled. " +
                    "Enemy-clear requirement is temporarily bypassed.");
                ActivatePortal();
            }
        }

        public void ActivatePortal()
        {
            if (_isAvailable)
            {
                Debug.LogWarning("[BossPortal] Portal is already available.");
                return;
            }

            SetPortalAvailable(true);
            Debug.Log(
                $"[BossPortal] Portal activated. Trigger enabled: {portalTrigger != null && portalTrigger.enabled}",
                this);
        }

        public void SetPortalAvailable(bool available)
        {
            _isAvailable = available;

            if (portalVisualRenderer != null)
                portalVisualRenderer.enabled = available;

            if (portalGlowRenderer != null)
                portalGlowRenderer.enabled = available;

            if (portalTrigger != null)
                portalTrigger.enabled = available;

            if (interactionHint != null)
                interactionHint.SetActive(available);
        }

        public void TryEnterPortal(Collider2D other)
        {
            Debug.Log(
                $"[BossPortal] Trigger entered by: {other.name}, " +
                $"layer={LayerMask.LayerToName(other.gameObject.layer)}, " +
                $"tag={other.tag}",
                this);

            if (_transitionStarted)
            {
                Debug.Log("[BossPortal] Transition already started. Ignoring.");
                return;
            }

            if (!_isAvailable)
            {
                Debug.Log("[BossPortal] Portal is not yet available. Ignoring.");
                return;
            }

            var player = other.GetComponentInParent<PlayerController2D>();
            if (player == null)
            {
                Debug.Log("[BossPortal] Not a player. Ignoring.");
                return;
            }

            _transitionStarted = true;
            Debug.Log("[BossPortal] Player confirmed. Starting BossRoom transition.", this);

            onPlayerEnterPortal?.Invoke();

            if (portalTrigger != null)
                portalTrigger.enabled = false;

            SetPortalAvailable(false);
        }

#if UNITY_EDITOR
        [ContextMenu("Debug/Force Activate Portal")]
        private void ForceActivatePortal()
        {
            ActivatePortal();
        }

        private void OnValidate()
        {
            if (Application.isPlaying) return;
            ApplyEditorPreview();
        }

        private void ApplyEditorPreview()
        {
            if (portalVisualRenderer != null)
                portalVisualRenderer.enabled = showPreviewInEditor;

            if (portalGlowRenderer != null)
                portalGlowRenderer.enabled = showPreviewInEditor;

            if (portalTrigger != null)
                portalTrigger.enabled = showPreviewInEditor;

            if (interactionHint != null)
                interactionHint.SetActive(showPreviewInEditor);
        }

        private void OnDrawGizmos()
        {
            if (portalTrigger != null)
            {
                Gizmos.color = new Color(0.65f, 0.15f, 1f, 0.25f);
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = portalTrigger.transform.localToWorldMatrix;

                if (portalTrigger is BoxCollider2D box)
                {
                    Gizmos.DrawWireCube(box.offset, box.size);
                }
                else if (portalTrigger is CircleCollider2D circle)
                {
                    Gizmos.DrawWireSphere(circle.offset, circle.radius);
                }

                Gizmos.matrix = oldMatrix;
            }

            Gizmos.color = new Color(0.65f, 0.15f, 1f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, 0.6f);

            Vector3 upPos = transform.position + Vector3.up * 1.2f;
            Gizmos.DrawLine(transform.position + Vector3.left * 0.3f, upPos);
            Gizmos.DrawLine(transform.position + Vector3.right * 0.3f, upPos);
        }
#endif
    }
}
