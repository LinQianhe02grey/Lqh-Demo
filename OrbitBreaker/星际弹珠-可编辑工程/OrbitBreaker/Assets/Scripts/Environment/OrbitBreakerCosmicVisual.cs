using UnityEngine;
namespace OrbitBreaker
{
    // Decorative rings only, never placed on a physics object.
    public sealed class OrbitBreakerCosmicVisual : MonoBehaviour
    {
        public float speed = 25f;
        private void Update() { transform.Rotate(0f, 0f, speed * Time.deltaTime, Space.Self); }
    }
}
