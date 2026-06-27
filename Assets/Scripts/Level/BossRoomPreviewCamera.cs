using UnityEngine;

namespace Cardwin.Level
{
    [ExecuteAlways]
    public sealed class BossRoomPreviewCamera : MonoBehaviour
    {
        [SerializeField]
        private Camera previewCamera;

        private void OnEnable()
        {
            RefreshState();
        }

        private void Update()
        {
            RefreshState();
        }

        private void RefreshState()
        {
            if (previewCamera == null)
                previewCamera = GetComponent<Camera>();

            if (previewCamera == null)
                return;

            bool shouldBeActive = !Application.isPlaying;
            if (previewCamera.enabled != shouldBeActive)
            {
                previewCamera.enabled = shouldBeActive;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (previewCamera == null)
                previewCamera = GetComponent<Camera>();
        }
#endif
    }
}
