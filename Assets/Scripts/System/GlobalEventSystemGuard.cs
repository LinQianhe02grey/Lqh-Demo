using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Cardwin.Runtime
{
    public sealed class GlobalEventSystemGuard : MonoBehaviour
    {
        [SerializeField]
        private EventSystem globalEventSystem;

        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            RemoveDuplicateEventSystems();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RemoveDuplicateEventSystems();
        }

        private void RemoveDuplicateEventSystems()
        {
            if (globalEventSystem == null)
            {
                globalEventSystem = GetComponentInChildren<EventSystem>(true);
                if (globalEventSystem == null) return;
            }

            var systems = Object.FindObjectsByType<EventSystem>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var system in systems)
            {
                if (system == null || system == globalEventSystem)
                    continue;

                Debug.LogWarning(
                    "[GlobalUI] Removing duplicate EventSystem: " +
                    system.name + ", scene=" + system.gameObject.scene.name,
                    system);

                Destroy(system.gameObject);
            }
        }
    }
}
