using UnityEngine;

namespace OrbitBreaker
{
    [DisallowMultipleComponent, RequireComponent(typeof(BoxCollider))]
    public sealed class OrbitBreakerKillZone : MonoBehaviour
    {
        [SerializeField] private Transform visibleCore;
        public bool ContainsCore(Vector2 world)
        {
            if (visibleCore == null) return false;
            var p = visibleCore.InverseTransformPoint(new Vector3(world.x, world.y, visibleCore.position.z));
            return p.x*p.x + p.y*p.y <= .25f;
        }
        private void Reset() => GetComponent<BoxCollider>().isTrigger = true;
        private void OnTriggerEnter(Collider other) => TryEndGame(other);
        private void OnTriggerStay(Collider other) => TryEndGame(other);

        private void TryEndGame(Collider other)
        {
            var player = other.GetComponentInParent<OrbitBreakerPlayerMotor>();
            if (player == null || player.OrbitBreakerGameManager == null || !player.OrbitBreakerGameManager.IsPlaying) return;
            if (!ContainsCore(player.Position)) return;
            player.OrbitBreakerGameManager.LoseGame();
        }
    }
}
