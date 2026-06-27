using UnityEngine;

namespace Cardwin.Runtime
{
    public static class GlobalRuntimeAutoLoader
    {
        private const string ResourcePath = "System/GlobalRuntimeRoot";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureRuntimeRoot()
        {
            if (Object.FindFirstObjectByType<GlobalRuntimeBootstrap>() != null)
                return;

            GameObject prefab = Resources.Load<GameObject>(ResourcePath);
            if (prefab == null)
            {
                Debug.LogError("[GlobalRuntime] GlobalRuntimeRoot prefab not found at Resources/" + ResourcePath + ". Global systems will be unavailable.");
                return;
            }

            GameObject instance = Object.Instantiate(prefab);
            instance.name = "GlobalRuntimeRoot";
            Debug.Log("[GlobalRuntime] GlobalRuntimeRoot instantiated from prefab.");
        }
    }
}
