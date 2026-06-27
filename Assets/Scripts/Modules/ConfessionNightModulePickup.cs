using UnityEngine;
using Cardwin.Combat;

namespace Cardwin.Modules
{
    /// <summary>
    /// Scene pickup for the third special module (Confession Night). When the
    /// player stands in range and presses F it ensures a
    /// PlayerConfessionNightModuleState exists on the player and activates it
    /// (starting the looping rhythm game). Independent from the Cursed / Blessed
    /// pickups; positioned away from them to avoid overlapping trigger ranges.
    /// </summary>
    public sealed class ConfessionNightModulePickup : MonoBehaviour
    {
        [Header("Visual / Prompt")]
        [SerializeField] private GameObject promptText;
        [SerializeField] private SpriteRenderer visualRenderer;

        [Header("Audio (optional - assign real mp3)")]
        [SerializeField] private AudioClip confessionNightClip;
        [SerializeField] private string resourceClipPath = "Audio/Ayasa_Confession_Night";

        private bool _consumed;
        private bool _playerInRange;
        private PlayerConfessionNightModuleState _playerModule;

        // Preloaded once at scene start so pressing F never pays the 13MB load cost.
        private static AudioClip _preloadedClip;

        private void Start()
        {
            PreloadAudio();
        }

        private void PreloadAudio()
        {
            if (_preloadedClip != null) return;

            _preloadedClip = confessionNightClip != null
                ? confessionNightClip
                : Resources.Load<AudioClip>(resourceClipPath);

            if (_preloadedClip != null)
            {
                _preloadedClip.LoadAudioData();   // warm streaming/decoded buffer off the F-press frame
                Debug.Log($"[ConfessionNightModule] Preloaded audio at scene start: {_preloadedClip.name}, length={_preloadedClip.length:F1}");
            }
            else
            {
                Debug.LogWarning($"[ConfessionNightModule] Preload could not find Resources/{resourceClipPath}; controller will retry at activation.");
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_consumed) return;
            if (!other.CompareTag("Player")) return;

            var health = other.GetComponentInParent<Health>();
            if (health == null) return;

            _playerModule = health.GetComponent<PlayerConfessionNightModuleState>();
            if (_playerModule == null)
                _playerModule = health.gameObject.AddComponent<PlayerConfessionNightModuleState>();

            _playerInRange = true;
            if (promptText != null) promptText.SetActive(true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = false;
            _playerModule = null;
            if (promptText != null) promptText.SetActive(false);
        }

        private void Update()
        {
            if (_consumed || !_playerInRange || _playerModule == null) return;
            if (Input.GetKeyDown(KeyCode.F))
                ActivateModule();
        }

        private void ActivateModule()
        {
            if (_playerModule.IsActive) return;
            if (_preloadedClip == null) PreloadAudio();
            _playerModule.Activate(_preloadedClip);

            _consumed = true;
            if (promptText != null) promptText.SetActive(false);
            if (visualRenderer != null) visualRenderer.enabled = false;
            Debug.Log("[ConfessionNightModulePickup] Module consumed.");
            Destroy(gameObject, 0.1f);
        }
    }
}
