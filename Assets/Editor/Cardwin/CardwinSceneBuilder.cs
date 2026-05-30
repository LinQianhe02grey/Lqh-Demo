using UnityEditor;
using UnityEngine;

namespace Cardwin.EditorTools
{
    public static class CardwinSceneBuilder
    {
        [MenuItem("Tools/Cardwin/Rebuild Clean Demo Scene")]
        public static void RebuildCleanDemoScene()
        {
            EditorUtility.DisplayDialog(
                "Cardwin Scene Builder",
                "SceneBuilder is disabled. Demo_Combat.unity is now the main working scene. Do not rebuild the scene unless explicitly requested.",
                "OK"
            );

            Debug.Log("[Cardwin] SceneBuilder is disabled. Use existing Demo_Combat.unity.");
        }
    }
}
