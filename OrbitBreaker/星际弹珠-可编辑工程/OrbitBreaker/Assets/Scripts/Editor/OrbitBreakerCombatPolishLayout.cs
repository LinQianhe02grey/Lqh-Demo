using UnityEditor;
using UnityEngine;
namespace OrbitBreaker.Editor
{
    public static class OrbitBreakerCombatPolishLayout
    {
        public static bool Apply(GameObject root,OrbitBreakerCombatConfig config)
        {
            if(root.transform.Find("CombatPolishT17")!=null)return false;
            var marker=new GameObject("CombatPolishT17");marker.transform.SetParent(root.transform,false);
            var player=root.GetComponentInChildren<OrbitBreakerPlayerMotor>();
            if(player.GetComponent<OrbitBreakerDashCapsules>()==null)player.gameObject.AddComponent<OrbitBreakerDashCapsules>();
            var systems=root.transform.Find("Systems");if(systems.GetComponent<OrbitBreakerCombatAudio>()==null)systems.gameObject.AddComponent<OrbitBreakerCombatAudio>();
            config.enemySpawnInterval=1.2f;
            EditorUtility.SetDirty(config);AssetDatabase.SaveAssetIfDirty(config);
            return true;
        }
    }
}
