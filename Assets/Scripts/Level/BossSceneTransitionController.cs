using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cardwin.Runtime;

namespace Cardwin.Level
{
    public class BossSceneTransitionController : MonoBehaviour
    {
        [Header("Scene")]
        [SerializeField]
        private string bossSceneName = "BossRoom";

        private bool _transitionInProgress;
        private Scene _previousGameplayScene;

        public void TransitionToBossRoom()
        {
            if (_transitionInProgress)
            {
                Debug.LogWarning("[BossTransition] Transition already in progress.");
                return;
            }

            _previousGameplayScene = SceneManager.GetActiveScene();
            StartCoroutine(TransitionRoutine());
        }

        private IEnumerator TransitionRoutine()
        {
            _transitionInProgress = true;
            Debug.Log("[BossTransition] Starting transition to '" + bossSceneName + "'...");

            var runtime = GlobalRuntimeBootstrap.Instance;
            if (runtime == null)
            {
                Debug.LogError("[BossTransition] GlobalRuntimeBootstrap not found. Cannot transition.");
                _transitionInProgress = false;
                yield break;
            }

            if (runtime.GameplayCamera == null)
            {
                Debug.LogError("[BossTransition] Gameplay camera is missing. Cannot transition.");
                _transitionInProgress = false;
                yield break;
            }

            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(bossSceneName, LoadSceneMode.Additive);
            if (loadOperation == null)
            {
                Debug.LogError("[BossTransition] Failed to start load for scene '" + bossSceneName + "'. Is it in Build Settings?");
                _transitionInProgress = false;
                yield break;
            }

            while (!loadOperation.isDone)
                yield return null;

            Scene bossScene = SceneManager.GetSceneByName(bossSceneName);
            if (!bossScene.IsValid() || !bossScene.isLoaded)
            {
                Debug.LogError("[BossTransition] BossRoom scene failed to load.");
                _transitionInProgress = false;
                yield break;
            }

            Debug.Log("[BossTransition] BossRoom scene loaded.");

            BossRoomSceneController roomController = FindControllerInsideScene(bossScene);
            if (roomController == null || roomController.PlayerSpawnPoint == null)
            {
                Debug.LogError("[BossTransition] BossRoomSceneController or PlayerSpawnPoint is missing.");
                _transitionInProgress = false;
                yield break;
            }

            if (roomController.MainGroundCollider == null || !roomController.MainGroundCollider.enabled)
            {
                Debug.LogError("[BossTransition] Main ground collider is missing or disabled.");
                _transitionInProgress = false;
                yield break;
            }

            SceneManager.SetActiveScene(bossScene);

            runtime.TeleportPlayer(roomController.PlayerSpawnPoint.position);

            yield return new WaitForFixedUpdate();

            runtime.SnapCameraToPlayer();

            Debug.Log("[BossTransition] Player teleported. Unloading previous scene...");

            if (_previousGameplayScene.IsValid() && _previousGameplayScene.isLoaded)
                yield return SceneManager.UnloadSceneAsync(_previousGameplayScene);

            Debug.Log("[BossTransition] Scene transition complete.");
        }

        private BossRoomSceneController FindControllerInsideScene(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                var controller = root.GetComponentInChildren<BossRoomSceneController>();
                if (controller != null)
                    return controller;
            }
            return null;
        }
    }
}
