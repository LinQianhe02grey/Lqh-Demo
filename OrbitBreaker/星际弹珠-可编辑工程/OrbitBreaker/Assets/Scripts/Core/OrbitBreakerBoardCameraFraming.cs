using UnityEngine;

namespace OrbitBreaker
{
    [RequireComponent(typeof(Camera))]
    public sealed class OrbitBreakerBoardCameraFraming : MonoBehaviour
    {
        public const float ReferenceFov = 27.314f;
        public static float FovForAspect(float aspect) => 2f * Mathf.Atan(
            Mathf.Tan(ReferenceFov * Mathf.Deg2Rad * .5f) / Mathf.Min(1f, Mathf.Max(.1f, aspect) / (16f / 9f))) * Mathf.Rad2Deg;
        private Camera view;
        private void Awake() { view = GetComponent<Camera>(); }
        private void LateUpdate() { view.fieldOfView = FovForAspect(view.aspect); }
    }
}
