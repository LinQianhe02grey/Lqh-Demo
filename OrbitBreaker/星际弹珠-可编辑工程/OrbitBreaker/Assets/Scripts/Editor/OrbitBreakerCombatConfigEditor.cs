using UnityEditor;
using UnityEngine;
namespace OrbitBreaker.Editor
{
    [CustomEditor(typeof(OrbitBreakerCombatConfig))]
    public sealed class OrbitBreakerCombatConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.LabelField("战斗核心参数 · 8项",EditorStyles.boldLabel);
            Field("enemyHealth","敌人血量","对之后生成的红色/黑色敌人生效");
            Field("maxDashCharges","冲刺储存上限","1–6；小球上方胶囊数随之变化");
            Field("shieldDropChance","补盾掉落概率","0不掉落，1必掉落，默认0.3");
            Field("dashSpeed","基础冲刺速度","奖励加速与黑色领域减速在此基础上计算");
            Field("dashCooldown","冲刺冷却（秒）","");
            Field("attachDuration","吸附时长（秒）","期间自动绕转，Space发射");
            Field("enemySpawnInterval","刷敌间隔（秒）","最多12只，每3只中1只黑色敌人");
            Field("targetScore","自由模式积分里程碑","");
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.HelpBox("建议在非运行状态修改并保存，再进入Play体验。旧重力、移动加速度和受击时长作为兼容数据保留。",MessageType.Info);
        }
        private void Field(string property,string label,string tip)=>EditorGUILayout.PropertyField(serializedObject.FindProperty(property),new GUIContent(label,tip));
    }
}
