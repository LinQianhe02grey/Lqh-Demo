using UnityEngine;
using Cardwin.Runtime;

namespace Cardwin.Level
{
    public class BossRoomSceneController : MonoBehaviour
    {
        [Header("Spawn Points")]
        [SerializeField]
        private Transform playerSpawnPoint;

        [SerializeField]
        private Transform bossSpawnPoint;

        [Header("Environment")]
        [SerializeField]
        private Collider2D mainGroundCollider;

        [SerializeField]
        private Transform bossArenaCenter;

        [SerializeField]
        private Transform safetyFloorTransform;

        public Transform PlayerSpawnPoint => playerSpawnPoint;
        public Transform BossSpawnPoint => bossSpawnPoint;
        public Collider2D MainGroundCollider => mainGroundCollider;
        public Transform BossArenaCenter => bossArenaCenter;

        private void Start()
        {
            PlacePlayerAtSpawn();
        }

        private void PlacePlayerAtSpawn()
        {
            var runtime = GlobalRuntimeBootstrap.Instance;
            if (runtime == null || runtime.PlayerTransform == null)
            {
                Debug.LogWarning("[BossRoom] GlobalRuntimeBootstrap or Player not found. Skipping spawn placement.");
                return;
            }

            if (playerSpawnPoint == null)
            {
                Debug.LogWarning("[BossRoom] PlayerSpawnPoint not assigned.");
                return;
            }

            runtime.TeleportPlayer(playerSpawnPoint.position);
            runtime.SnapCameraToPlayer();
            Debug.Log("[BossRoom] Player placed at spawn point: " + playerSpawnPoint.position);
        }

        private void OnDrawGizmos()
        {
            if (playerSpawnPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(playerSpawnPoint.position, 0.5f);
                Gizmos.DrawLine(playerSpawnPoint.position + Vector3.up * 1.5f, playerSpawnPoint.position + Vector3.down * 0.5f);
                Gizmos.DrawLine(playerSpawnPoint.position + Vector3.left * 0.3f, playerSpawnPoint.position + Vector3.right * 0.3f);
            }

            if (bossSpawnPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(bossSpawnPoint.position, 0.5f);
                Gizmos.DrawLine(bossSpawnPoint.position + Vector3.up * 1.5f, bossSpawnPoint.position + Vector3.down * 0.5f);
                Gizmos.DrawLine(bossSpawnPoint.position + Vector3.left * 0.3f, bossSpawnPoint.position + Vector3.right * 0.3f);
            }

            if (bossArenaCenter != null)
            {
                Gizmos.color = new Color(0.8f, 0.2f, 0.8f, 1f);
                Gizmos.DrawWireSphere(bossArenaCenter.position, 0.4f);
                Gizmos.DrawWireCube(bossArenaCenter.position, new Vector3(2f, 0.1f, 0.1f));
            }

            if (safetyFloorTransform != null)
            {
                Gizmos.color = new Color(0.2f, 0.4f, 1f, 0.3f);
                Vector3 size = safetyFloorTransform.localScale;
                Gizmos.DrawCube(safetyFloorTransform.position, new Vector3(size.x, size.y, 0.1f));
                Gizmos.color = new Color(0.2f, 0.4f, 1f, 0.8f);
                Gizmos.DrawWireCube(safetyFloorTransform.position, new Vector3(size.x, size.y, 0.1f));
            }
        }
    }
}
