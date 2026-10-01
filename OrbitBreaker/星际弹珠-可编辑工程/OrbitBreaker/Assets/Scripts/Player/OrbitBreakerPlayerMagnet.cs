using UnityEngine;

namespace OrbitBreaker
{
    // Queries/input projection only. All player physics and state live in OrbitBreakerPlayerMotor.
    [DisallowMultipleComponent, RequireComponent(typeof(OrbitBreakerPlayerMotor))]
    public sealed class OrbitBreakerPlayerMagnet : MonoBehaviour
    {
        private OrbitBreakerPlayerMotor motor;
        private void Awake() => motor = GetComponent<OrbitBreakerPlayerMotor>();
        public bool ToggleAttachment()
        {
            if (!isActiveAndEnabled) return false;
            if (motor.IsAttached) { motor.ReleaseAttachment(); return true; }
            return motor.TryAttach(OrbitBreakerAttachmentPoint.Nearest(motor.Position));
        }
        public static bool TryProjectMouse(Camera camera, Vector3 screen, out Vector2 point)
        {
            point = Vector2.zero;
            if (camera == null || !camera.pixelRect.Contains(screen)) return false;
            Ray ray = camera.ScreenPointToRay(screen);
            if (!new Plane(Vector3.forward, Vector3.zero).Raycast(ray, out float distance)) return false;
            Vector3 world = ray.GetPoint(distance);
            if (float.IsNaN(world.x) || float.IsInfinity(world.x) || float.IsNaN(world.y) || float.IsInfinity(world.y)) return false;
            point = world;
            return true;
        }
        public void UpdateMouseAim()
        {
            if (!isActiveAndEnabled) return;
            var camera=Camera.main;
            if(camera!=null){var follow=camera.GetComponent<OrbitBreakerCenteredCamera>();if(follow!=null)follow.Follow();}
            bool valid = TryProjectMouse(camera, Input.mousePosition, out Vector2 point);
            motor.SetAttachmentAim(point, valid);
        }
        private void OnDisable() { if (motor != null) motor.ReleaseAttachment(); }
    }
}
