#if UNITY_EDITOR
using System;
using UnityEngine;

namespace OrbitBreaker.Testing
{
    // Spawned only by the Editor test runner; excluded entirely from player builds.
    [DefaultExecutionOrder(10000)]
    public sealed class OrbitBreakerT03Probe : MonoBehaviour
    {
        public Action Observe;
        private void LateUpdate() => Observe?.Invoke();
    }
}
#endif
