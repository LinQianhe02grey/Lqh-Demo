using UnityEditor;
using UnityEngine;

namespace Cardwin.EditorTools
{
    public static class CardwinSceneBuilder
    {
        [MenuItem("Tools/Cardwin/Legacy/Rebuild Clean Demo Scene")]
        public static void RebuildCleanDemoScene()
        {
            EditorUtility.DisplayDialog(
                "Cardwin Scene Builder (Legacy)",
                "This is a LEGACY tool. SceneBuilder is disabled. Demo_Combat.unity is the main working scene. Do not rebuild.",
                "OK"
            );

            Debug.Log("[Cardwin] SceneBuilder is disabled. Use existing Demo_Combat.unity.");
        }
    }
}
