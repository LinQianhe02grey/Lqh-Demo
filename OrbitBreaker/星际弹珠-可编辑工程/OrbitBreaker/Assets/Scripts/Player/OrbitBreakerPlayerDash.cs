using UnityEngine;

namespace OrbitBreaker
{
    [DisallowMultipleComponent, RequireComponent(typeof(OrbitBreakerPlayerMotor))]
    public sealed class OrbitBreakerPlayerDash : MonoBehaviour
    {
        private OrbitBreakerPlayerMotor motor;
        private void Awake() => motor = GetComponent<OrbitBreakerPlayerMotor>();
        public bool IsDashing => motor != null && motor.IsDashing;
        public bool CanDash => isActiveAndEnabled && motor != null && motor.CanDash;
        public float CooldownRemaining => motor == null ? 0f : motor.DashCooldownRemaining;
        public bool TryDash() => CanDash && motor.TryRequestDash();
        private void OnDisable() { if (motor != null) motor.CancelPendingDash(); }
    }
}
