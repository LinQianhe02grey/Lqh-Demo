using UnityEditor;
using UnityEngine;

namespace OrbitBreaker.Editor
{
    // Both panels edit the assets referenced by the scene. No parallel copy of gameplay rules.
    public sealed class OrbitBreakerCombatTuningWindow : EditorWindow
    {
        private UnityEditor.Editor combatEditor, encounterEditor;
        private Vector2 scroll;
        private int tab;
        [MenuItem("Tools/Orbit Breaker/战斗参数编辑器")]
        public static void Open()=>GetWindow<OrbitBreakerCombatTuningWindow>("弹珠 · 战斗参数");
        private void OnGUI()
        {
            EditorGUILayout.LabelField("PINBALL / 战斗参数",EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("每页8项。修改支持撤销；点击保存后用于下次游戏。图鉴读取同一配置。运行中禁止保存实验参数。",MessageType.Info);
            tab=GUILayout.Toolbar(tab,new[]{"基础与资源 · 8项","速度与Boss · 8项"});
            var asset=AssetDatabase.LoadAssetAtPath<ScriptableObject>(tab==0?OrbitBreakerSceneBuilder.ConfigPath:OrbitBreakerModeSceneLayout.ConfigPath);
            if(asset==null){EditorGUILayout.HelpBox("请先生成/更新 Demo 场景。",MessageType.Warning);return;}
            using(new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
            {
                scroll=EditorGUILayout.BeginScrollView(scroll);
                if(tab==0){UnityEditor.Editor.CreateCachedEditor(asset,null,ref combatEditor);combatEditor.OnInspectorGUI();}
                else{UnityEditor.Editor.CreateCachedEditor(asset,null,ref encounterEditor);encounterEditor.OnInspectorGUI();}
                EditorGUILayout.EndScrollView();
                if(GUILayout.Button("保存参数"))AssetDatabase.SaveAssets();
            }
            if(GUILayout.Button("定位当前配置资源")){Selection.activeObject=asset;EditorGUIUtility.PingObject(asset);}
        }
        private void OnDisable(){if(combatEditor!=null)DestroyImmediate(combatEditor);if(encounterEditor!=null)DestroyImmediate(encounterEditor);}
    }
    [CustomEditor(typeof(OrbitBreakerEncounterConfig))]
    public sealed class EncounterConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            string[] fields={"damageThreshold","maximumSpeed","thresholdDamage","maximumSpeedDamage","orbitRampSeconds","bossHealth","bossMoveSpeed","bossAttackInterval"};
            string[] labels={"伤害 / 无敌起速","最高速 / 贯穿起速","起速基础伤害","最高速基础伤害","绕转加速时间（秒）","章鱼Boss血量","Boss移动速度","Boss攻击间隔（秒）"};
            for(int i=0;i<fields.Length;i++)EditorGUILayout.PropertyField(serializedObject.FindProperty(fields[i]),new GUIContent(labels[i]));
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.HelpBox("WASD上限8；实际速度超过起速才伤敌/攻击无敌。伤害线性增长，再乘连弹倍率。奖励可突破基础最高速。Boss触手与激光先预警再生效。",MessageType.None);
        }
    }
}
