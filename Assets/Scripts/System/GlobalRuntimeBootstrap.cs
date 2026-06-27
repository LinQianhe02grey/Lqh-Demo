using UnityEngine;
using Cardwin.Cameras;
using Cardwin.Combat;

namespace Cardwin.Runtime
{
    [DefaultExecutionOrder(-1000)]
    public sealed class GlobalRuntimeBootstrap : MonoBehaviour
    {
        private static GlobalRuntimeBootstrap _instance;

        [Header("Core References")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Rigidbody2D playerRigidbody;
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private CameraFollow2D cameraFollow;

        public static GlobalRuntimeBootstrap Instance => _instance;
        public Transform PlayerTransform => playerTransform;
        public Rigidbody2D PlayerRigidbody => playerRigidbody;
        public Camera GameplayCamera => gameplayCamera;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("[GlobalRuntime] Duplicate GlobalRuntimeRoot detected. Destroying this instance.");
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            if (playerTransform == null)
                playerTransform = GetComponentInChildren<PlayerController2D>(true)?.transform;

            if (playerRigidbody == null && playerTransform != null)
                playerRigidbody = playerTransform.GetComponent<Rigidbody2D>();

            if (gameplayCamera == null)
            {
                foreach (var cam in GetComponentsInChildren<Camera>(true))
                {
                    if (cam.CompareTag("MainCamera"))
                    {
                        gameplayCamera = cam;
                        break;
                    }
                }
            }

            if (cameraFollow == null && gameplayCamera != null)
                cameraFollow = gameplayCamera.GetComponent<CameraFollow2D>();
        }

        public void TeleportPlayer(Vector3 worldPosition)
        {
            if (playerRigidbody != null)
            {
                playerRigidbody.velocity = Vector2.zero;
                playerRigidbody.angularVelocity = 0f;
                playerRigidbody.position = worldPosition;
            }
            else if (playerTransform != null)
            {
                playerTransform.position = worldPosition;
            }

            Physics2D.SyncTransforms();
        }

        public void SnapCameraToPlayer()
        {
            if (cameraFollow != null)
            {
                cameraFollow.SnapToTarget();
            }
            else if (gameplayCamera != null && playerTransform != null)
            {
                var pos = gameplayCamera.transform.position;
                pos.x = playerTransform.position.x;
                pos.y = playerTransform.position.y;
                gameplayCamera.transform.position = pos;
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }
    }
}
