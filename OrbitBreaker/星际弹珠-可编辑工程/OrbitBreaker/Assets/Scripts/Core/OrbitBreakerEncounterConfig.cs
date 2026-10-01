using UnityEngine;

namespace OrbitBreaker
{
    [CreateAssetMenu(menuName="Orbit Breaker/Speed and Boss Rules", fileName="OrbitBreakerEncounterConfig")]
    public sealed class OrbitBreakerEncounterConfig : ScriptableObject
    {
        [Range(9,20)] public float damageThreshold = 12;
        [Range(21,40)] public float maximumSpeed = 24;
        [Range(1,100)] public int thresholdDamage = 24;
        [Range(100,5000)] public int bossHealth = 2400;
        [Range(.5f,6)] public float orbitRampSeconds = 2;
        [Range(.5f,5)] public float bossMoveSpeed = 1.8f;
        [Range(2,10)] public float bossAttackInterval = 4;
        [Range(30,200)] public int maximumSpeedDamage = 80;
        public const float ManualSpeed = 8;
        public void Validate()
        {
            damageThreshold=Finite(damageThreshold,12,9,20);
            maximumSpeed=Finite(maximumSpeed,24,21,40);
            thresholdDamage=Mathf.Clamp(thresholdDamage,1,100);
            maximumSpeedDamage=Mathf.Clamp(maximumSpeedDamage,Mathf.Max(30,thresholdDamage),200);
            orbitRampSeconds=Finite(orbitRampSeconds,2,.5f,6);
            bossHealth=Mathf.Clamp(bossHealth,100,5000);
            bossMoveSpeed=Finite(bossMoveSpeed,1.8f,.5f,5);
            bossAttackInterval=Finite(bossAttackInterval,4,2,10);
        }
        private void OnValidate()=>Validate();
        private static float Finite(float v,float fallback,float min,float max)=>float.IsNaN(v)||float.IsInfinity(v)?fallback:Mathf.Clamp(v,min,max);
    }
}
