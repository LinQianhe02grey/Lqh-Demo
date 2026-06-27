using UnityEngine;

namespace Cardwin.Level
{
    /// <summary>
    /// Tags a scene as an actual gameplay scene. Scenes WITHOUT this marker
    /// (e.g. MainMenu) are treated as non-gameplay: SceneRespawnService disables
    /// the global player's physics and visuals so it does not free-fall.
    /// </summary>
    public sealed class SceneGameplayMarker : MonoBehaviour
    {
        [SerializeField]
        private bool isGameplayScene = true;

        public bool IsGameplayScene => isGameplayScene;
    }
}
