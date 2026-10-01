using UnityEngine;

namespace OrbitBreaker
{
    [CreateAssetMenu(fileName = "OrbitBreakerCombatConfig", menuName = "Orbit Breaker/Combat Config")]
    public sealed class OrbitBreakerCombatConfig : ScriptableObject
    {
        // Preserve serialized tuning from older scenes without crowding the eight-rule Inspector.
        [HideInInspector] public float gravityStrength = 7.5f;
        [HideInInspector] public float moveAcceleration = 20f;
        [HideInInspector] public float stunDuration = 0.65f;
        [Tooltip("基础冲刺速度；奖励洞和粘稠领域在此基础上乘算"), Range(6,24)] public float dashSpeed = 18f;
        [Tooltip("冲刺使用间隔，秒"), Range(.1f,5f)] public float dashCooldown = 1f;
        [Tooltip("自动绕转最多持续的秒数"), Range(1f,20f)] public float attachDuration = 7.5f;
        [Tooltip("两次敌人刷新之间的秒数"), Range(.3f,5f)] public float enemySpawnInterval = 1.2f;
        [Tooltip("自由模式的积分进度里程碑，不结束游戏"), Min(1)] public int targetScore = 3000;
        [Tooltip("红色与黑色敌人的初始血量"), Range(20,500)] public int enemyHealth = 100;
        [Tooltip("最多储存的冲刺次数；对应球上胶囊数量"), Range(1,6)] public int maxDashCharges = 3;
        [Tooltip("敌人被击杀后掉落一层补盾的概率"), Range(0,1)] public float shieldDropChance = .3f;

        private void OnValidate()
        {
            gravityStrength = Positive(gravityStrength);
            moveAcceleration = Positive(moveAcceleration);
            dashSpeed = Positive(dashSpeed);
            dashCooldown = Positive(dashCooldown);
            attachDuration = Positive(attachDuration);
            stunDuration = Positive(stunDuration);
            enemySpawnInterval = Positive(enemySpawnInterval);
            targetScore = Mathf.Max(1, targetScore);
            enemyHealth = Mathf.Clamp(enemyHealth,20,500);
            maxDashCharges = Mathf.Clamp(maxDashCharges,1,6);
            shieldDropChance = float.IsNaN(shieldDropChance) ? .3f : Mathf.Clamp01(shieldDropChance);
        }

        private static float Positive(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value) ? 0.01f : Mathf.Max(0.01f, value);
        }
    }
}
