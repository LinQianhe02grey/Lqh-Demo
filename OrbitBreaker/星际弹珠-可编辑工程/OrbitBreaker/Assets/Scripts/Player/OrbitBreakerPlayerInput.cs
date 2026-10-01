using UnityEngine;

namespace OrbitBreaker
{
    [DisallowMultipleComponent, RequireComponent(typeof(OrbitBreakerPlayerMotor), typeof(OrbitBreakerPlayerDash), typeof(OrbitBreakerPlayerMagnet))]
    public sealed class OrbitBreakerPlayerInput : MonoBehaviour
    {
        private OrbitBreakerPlayerMotor motor;
        private OrbitBreakerPlayerDash dash;
        private OrbitBreakerPlayerMagnet magnet;
        private void Awake()
        {
            motor = GetComponent<OrbitBreakerPlayerMotor>();
            dash = GetComponent<OrbitBreakerPlayerDash>();
            magnet = GetComponent<OrbitBreakerPlayerMagnet>();
        }

        private void Update()
        {
            var direction = new Vector2(
                (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f),
                (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f));
            motor.SetMoveInput(direction);
            magnet.UpdateMouseAim();
            if(Input.GetMouseButtonDown(0))GetComponent<OrbitBreakerTargetLock>()?.SelectAtScreen(Input.mousePosition);
            if(Input.GetMouseButtonDown(1))motor.TryBeginFocus();
            if (Input.GetKeyDown(KeyCode.E)) magnet.ToggleAttachment();
            if (Input.GetKeyDown(KeyCode.Space)) dash.TryDash();
        }

        private void OnDisable()
        {
            if (motor != null) { motor.EndFocus(); motor.SetMoveInput(Vector2.zero); motor.CancelPendingDash(); motor.ReleaseAttachment(); }
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused && motor != null) { motor.EndFocus(); motor.SetMoveInput(Vector2.zero); motor.CancelPendingDash(); motor.ReleaseAttachment(); }
        }
    }
}
