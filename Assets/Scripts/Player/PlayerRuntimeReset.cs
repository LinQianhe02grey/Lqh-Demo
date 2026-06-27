using UnityEngine;
using Cardwin.Combat;
using Cardwin.Characters;
using Cardwin.Runtime;

namespace Cardwin.Player
{
    /// <summary>
    /// Single entry point that restores the global (DontDestroyOnLoad) Player from a
    /// death state back to a fully controllable state on Retry.
    ///
    /// This is intentionally separate from SceneRespawnService.RespawnPlayerAtCurrentPoint
    /// (fall recovery), which only resets position + velocity. ResetForRetry performs a
    /// FULL death recovery: health revive, death-flag clear, Rigidbody2D / Collider2D /
    /// PlayerController re-enable, input unlock, Animator death exit, and placement at the
    /// current scene's SceneRespawnPoint with a camera snap.
    ///
    /// Long-term physics parameters (gravityScale / mass / constraints / collider size /
    /// layer) are never modified here. Inventory / magazine / settings are not touched.
    /// </summary>
    public sealed class PlayerRuntimeReset : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private PlayerController2D playerController;
        [SerializeField] private GothicNunAnimationBridge animationBridge;
        [SerializeField] private SceneRespawnService respawnService;

        /// <summary>
        /// Full death-to-alive recovery for the same persistent Player instance.
        /// Only called on Retry / Restart, never on plain fall recovery.
        /// </summary>
        public void ResetForRetry()
        {
            ResolveReferences();

            // 1. Health revive (restores HP, clears Health death flag, fires UI events).
            if (health != null)
                health.ReviveToFull();
            else
                Debug.LogError("[Retry] Health missing - cannot revive.");

            // 2. Clear PlayerController death state + re-enable Rigidbody2D / Collider2D /
            //    sprite and unlock input. Reuses the existing symmetric SetDead path so no
            //    movement / gravity / collider-size logic is changed.
            if (playerController != null)
            {
                playerController.SetDead(false);
                Debug.Log("[Retry] Death state cleared. Rigidbody / Collider / Controller restored. Gameplay input restored.");
            }
            else
            {
                Debug.LogError("[Retry] PlayerController2D missing - cannot clear death state.");
            }

            // 3. Exit the death animation.
            if (animationBridge != null)
                animationBridge.ResetDeathVisual();

            // 4. Place at the current scene's SceneRespawnPoint (zeroes velocity, syncs
            //    transforms, snaps camera). Works for Demo_Combat and BossRoom alike.
            if (respawnService != null)
            {
                respawnService.RespawnPlayerAtCurrentPoint();
                Debug.Log("[Retry] Player moved to respawn.");
            }
            else
            {
                Debug.LogError("[Retry] SceneRespawnService missing - player not repositioned.");
            }

            Physics2D.SyncTransforms();
            Debug.Log("[Retry] Retry reset complete.");
        }

        private void ResolveReferences()
        {
            if (health == null)
                health = GetComponent<Health>();

            if (playerController == null)
                playerController = GetComponent<PlayerController2D>();

            if (animationBridge == null)
                animationBridge = GetComponentInChildren<GothicNunAnimationBridge>(true);

            if (respawnService == null)
                respawnService = GetComponentInParent<SceneRespawnService>();

            if (respawnService == null)
                respawnService = FindObjectOfType<SceneRespawnService>();
        }
    }
}
