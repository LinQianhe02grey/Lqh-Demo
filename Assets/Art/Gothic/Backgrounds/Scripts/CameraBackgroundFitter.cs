using UnityEngine;

namespace Cardwin.Visual
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class CameraBackgroundFitter : MonoBehaviour
    {
        [SerializeField] private Camera _targetCamera;

        private SpriteRenderer _sr;
        private int _lastScreenWidth;
        private int _lastScreenHeight;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_sr == null)
            {
                Debug.LogError("[CameraBackgroundFitter] Missing SpriteRenderer on " + gameObject.name);
                enabled = false;
                return;
            }

            if (_targetCamera == null)
                _targetCamera = Camera.main;

            if (_targetCamera == null)
            {
                Debug.LogError("[CameraBackgroundFitter] No Camera found.");
                enabled = false;
                return;
            }

            FitToCamera();
        }

        private void LateUpdate()
        {
            if (_sr == null || _targetCamera == null) return;

            if (Screen.width != _lastScreenWidth || Screen.height != _lastScreenHeight)
            {
                FitToCamera();
            }

            Vector3 camPos = _targetCamera.transform.position;
            transform.position = new Vector3(camPos.x, camPos.y, 0f);
        }

        private void FitToCamera()
        {
            if (_sr == null || _targetCamera == null) return;
            if (_sr.sprite == null) return;

            float camHeight = _targetCamera.orthographicSize * 2f;
            float camWidth = camHeight * _targetCamera.aspect;

            Bounds spriteBounds = _sr.sprite.bounds;
            float spriteWidth = spriteBounds.size.x;
            float spriteHeight = spriteBounds.size.y;

            if (spriteWidth <= 0f || spriteHeight <= 0f) return;

            float scaleX = camWidth / spriteWidth;
            float scaleY = camHeight / spriteHeight;
            float scale = Mathf.Max(scaleX, scaleY);

            transform.localScale = new Vector3(scale, scale, 1f);

            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;
        }
    }
}
