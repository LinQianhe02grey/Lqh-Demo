using UnityEditor;
using UnityEngine;
namespace OrbitBreaker.Editor
{
    public static class OrbitBreakerModeSceneLayout
    {
        public const string ConfigPath="Assets/Config/OrbitBreakerEncounterConfig.asset";
        public static bool Apply(GameObject root)
        {
            if(root.transform.Find("ModesT18")!=null)return false;
            var rules=AssetDatabase.LoadAssetAtPath<OrbitBreakerEncounterConfig>(ConfigPath);
            if(rules==null){rules=ScriptableObject.CreateInstance<OrbitBreakerEncounterConfig>();AssetDatabase.CreateAsset(rules,ConfigPath);}
            var manager=root.GetComponentInChildren<OrbitBreakerGameManager>();
            var so=new SerializedObject(manager);so.FindProperty("encounter").objectReferenceValue=rules;so.ApplyModifiedPropertiesWithoutUndo();
            if(manager.GetComponent<OrbitBreakerModeMenu>()==null)manager.gameObject.AddComponent<OrbitBreakerModeMenu>();
            if(manager.GetComponent<OrbitBreakerBossDirector>()==null)manager.gameObject.AddComponent<OrbitBreakerBossDirector>();
            var player=root.GetComponentInChildren<OrbitBreakerPlayerMotor>();
            if(player.GetComponent<OrbitBreakerTargetLock>()==null)player.gameObject.AddComponent<OrbitBreakerTargetLock>();
            new GameObject("ModesT18").transform.SetParent(root.transform,false);
            AssetDatabase.SaveAssets();return true;
        }
    }
}
