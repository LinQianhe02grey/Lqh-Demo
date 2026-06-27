using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cardwin.Level;
using Cardwin.Combat;

namespace Cardwin.Runtime
{
    /// <summary>
    /// Single authority for placing the global (DontDestroyOnLoad) player across scenes.
    ///
    /// Responsibilities:
    ///  - On gameplay scene load: enable player physics/visual/input, place at the
    ///    scene's SceneRespawnPoint, and snap the camera.
    ///  - On non-gameplay scene load (e.g. MainMenu): disable player simulation and
    ///    hide visuals so it does not free-fall. Gravity and Y-axis are NOT touched.
    ///  - Fall recovery: when the player drops below the active SceneRespawnPoint's
    ///    FallLimitY, teleport it back (position + velocity only; HP / magazine /
    ///    buffs / inventory are preserved).
    ///
    /// This component never resets combat state. It only changes position, velocity,
    /// physics-simulation flag, visual activeness and input-lock.
    /// </summary>
    [DefaultExecutionOrder(-500)]
    public sealed class SceneRespawnService : MonoBehaviour
    {
        [Header("Player References (auto-resolved from GlobalRuntimeBootstrap if empty)")]
        [SerializeField] private Transform playerRoot;
        [SerializeField] private Rigidbody2D playerRigidbody;
        [SerializeField] private PlayerController2D playerController;
        [SerializeField] private GameObject playerVisualRoot;

        [Header("Fall Recovery")]
        [SerializeField] private bool respawnOnFall = true;
        [SerializeField] private float respawnCooldown = 0.5f;

        private SceneRespawnPoint _currentRespawnPoint;
        private float _lastRespawnTime;
        private bool _gameplayActive;

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Start()
        {
            // Handle the scene that was already active when this service came online.
            EvaluateScene(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EvaluateScene(scene, mode);
        }

        private void EvaluateScene(Scene scene, LoadSceneMode mode)
        {
            ResolvePlayerReferences();

            SceneRespawnPoint point = FindRespawnPointInScene(scene);
            SceneGameplayMarker marker = FindMarkerInScene(scene);
            bool isGameplay = point != null || (marker != null && marker.IsGameplayScene);

            if (isGameplay)
            {
                if (point != null)
                    _currentRespawnPoint = point;

                EnterGameplayScene();
            }
            else if (mode == LoadSceneMode.Single)
            {
                _currentRespawnPoint = null;
                EnterNonGameplayScene();
            }
        }

        private void EnterGameplayScene()
        {
            _gameplayActive = true;

            if (playerRigidbody != null)
                playerRigidbody.simulated = true;

            SetVisualActive(true);
            SetInputLocked(false);

            if (_currentRespawnPoint != null)
            {
                PlacePlayer(_currentRespawnPoint.Position);
                StartCoroutine(SnapCameraNextFixedUpdate());
            }
            else
            {
                Debug.LogError("[Respawn] Gameplay scene has no SceneRespawnPoint. Player position not set.");
            }
        }

        private void EnterNonGameplayScene()
        {
            _gameplayActive = false;

            if (playerRigidbody != null)
            {
                playerRigidbody.velocity = Vector2.zero;
                playerRigidbody.angularVelocity = 0f;
                playerRigidbody.simulated = false;
            }

            SetInputLocked(true);
            SetVisualActive(false);
        }

        private void Update()
        {
            if (!respawnOnFall || !_gameplayActive)
                return;

            if (_currentRespawnPoint == null || playerRoot == null)
                return;

            if (playerRigidbody != null && !playerRigidbody.simulated)
                return;

            if (Time.unscaledTime - _lastRespawnTime < respawnCooldown)
                return;

            if (playerRoot.position.y < _currentRespawnPoint.FallLimitY)
            {
                _lastRespawnTime = Time.unscaledTime;
                RespawnPlayerAtCurrentPoint();
            }
        }

        /// <summary>
        /// Teleports the player to the active respawn point. Only position and
        /// instantaneous velocity are modified; combat state is preserved.
        /// </summary>
        public void RespawnPlayerAtCurrentPoint()
        {
            if (_currentRespawnPoint == null)
            {
                Debug.LogError("[Respawn] No SceneRespawnPoint found. Cannot respawn.");
                return;
            }

            PlacePlayer(_currentRespawnPoint.Position);
            StartCoroutine(SnapCameraNextFixedUpdate());
            Debug.Log("[Respawn] Player recovered to respawn point: " + _currentRespawnPoint.Position);
        }

        private void PlacePlayer(Vector3 worldPosition)
        {
            var runtime = GlobalRuntimeBootstrap.Instance;
            if (runtime != null)
            {
                runtime.TeleportPlayer(worldPosition);
                return;
            }

            if (playerRigidbody != null)
            {
                playerRigidbody.velocity = Vector2.zero;
                playerRigidbody.angularVelocity = 0f;
                playerRigidbody.position = worldPosition;
            }
            else if (playerRoot != null)
            {
                playerRoot.position = worldPosition;
            }

            Physics2D.SyncTransforms();
        }

        private IEnumerator SnapCameraNextFixedUpdate()
        {
            yield return new WaitForFixedUpdate();

            var runtime = GlobalRuntimeBootstrap.Instance;
            if (runtime != null)
                runtime.SnapCameraToPlayer();
        }

        private void SetInputLocked(bool locked)
        {
            if (playerController != null)
                playerController.SetInputLocked(locked);
        }

        private void SetVisualActive(bool visible)
        {
            if (playerVisualRoot != null)
            {
                if (playerVisualRoot.activeSelf != visible)
                    playerVisualRoot.SetActive(visible);
                return;
            }

            if (playerRoot == null)
                return;

            foreach (var sr in playerRoot.GetComponentsInChildren<SpriteRenderer>(true))
                sr.enabled = visible;
        }

        private void ResolvePlayerReferences()
        {
            var runtime = GlobalRuntimeBootstrap.Instance;

            if (playerRoot == null && runtime != null)
                playerRoot = runtime.PlayerTransform;

            if (playerRigidbody == null && runtime != null)
                playerRigidbody = runtime.PlayerRigidbody;

            if (playerRoot == null && playerRigidbody != null)
                playerRoot = playerRigidbody.transform;

            if (playerController == null && playerRoot != null)
                playerController = playerRoot.GetComponent<PlayerController2D>();
        }

        private static SceneRespawnPoint FindRespawnPointInScene(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
                return null;

            foreach (var root in scene.GetRootGameObjects())
            {
                var point = root.GetComponentInChildren<SceneRespawnPoint>(true);
                if (point != null)
                    return point;
            }
            return null;
        }

        private static SceneGameplayMarker FindMarkerInScene(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
                return null;

            foreach (var root in scene.GetRootGameObjects())
            {
                var marker = root.GetComponentInChildren<SceneGameplayMarker>(true);
                if (marker != null)
                    return marker;
            }
            return null;
        }
    }
}
