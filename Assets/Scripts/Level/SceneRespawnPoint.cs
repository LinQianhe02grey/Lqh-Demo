using UnityEngine;

namespace Cardwin.Level
{
    /// <summary>
    /// Marks the player's spawn / respawn point for the current gameplay scene and
    /// declares the fall limit below which the player should be recovered.
    /// This component only describes data; it never creates or mutates the player.
    /// </summary>
    public sealed class SceneRespawnPoint : MonoBehaviour
    {
        [Header("Fall Recovery")]
        [Tooltip("When the player's Y position drops below this value, SceneRespawnService teleports the player back here.")]
        [SerializeField]
        private float fallLimitY = -20f;

        public Vector3 Position => transform.position;

        public float FallLimitY => fallLimitY;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.45f);
            Gizmos.DrawLine(
                transform.position + Vector3.left * 0.8f,
                transform.position + Vector3.right * 0.8f);
            Gizmos.DrawLine(
                transform.position + Vector3.up * 1.2f,
                transform.position + Vector3.down * 0.4f);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                new Vector3(transform.position.x - 12f, fallLimitY, 0f),
                new Vector3(transform.position.x + 12f, fallLimitY, 0f));
        }
    }
}
