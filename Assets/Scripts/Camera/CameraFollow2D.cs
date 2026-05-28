using UnityEngine;

namespace Cardwin.Camera
{
    public class CameraFollow2D : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0f, 1.5f, -10f);
        public float smoothTime = 0.15f;
        public bool useBounds = true;
        public Vector2 minBounds = new Vector2(-10f, -3f);
        public Vector2 maxBounds = new Vector2(30f, 8f);

        private Vector3 _velocity = Vector3.zero;
        private UnityEngine.Camera _cam;
        private bool _warnedMissingPlayer;

        private void Awake()
        {
            _cam = GetComponent<UnityEngine.Camera>();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                FindTargetIfMissing();
                if (target == null)
                    return;
            }

            Vector3 desired = target.position + offset;
            desired.z = -10f;

            Vector3 smoothed = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime);

            if (useBounds && _cam != null)
            {
                float halfH = _cam.orthographicSize;
                float halfW = halfH * _cam.aspect;

                smoothed.x = Mathf.Clamp(smoothed.x, minBounds.x + halfW, maxBounds.x - halfW);
                smoothed.y = Mathf.Clamp(smoothed.y, minBounds.y + halfH, maxBounds.y - halfH);
            }

            transform.position = smoothed;
        }

        private void FindTargetIfMissing()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                _warnedMissingPlayer = false;
            }
            else if (!_warnedMissingPlayer)
            {
                Debug.LogWarning("[CameraFollow2D] Player not found. Set Player tag or assign target manually.");
                _warnedMissingPlayer = true;
            }
        }
    }
}
