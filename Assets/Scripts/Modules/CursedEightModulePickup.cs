using UnityEngine;
using Cardwin.Combat;

namespace Cardwin.Modules
{
    public sealed class CursedEightModulePickup : MonoBehaviour
    {
        public enum ModuleType { Cursed, Blessed }

        [Header("Visual")]
        [SerializeField] private GameObject promptText;
        [SerializeField] private SpriteRenderer visualRenderer;

        [Header("Config")]
        [SerializeField] private ModuleType moduleType = ModuleType.Cursed;

        private bool _consumed;
        private bool _playerInRange;
        private MonoBehaviour _playerModule;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_consumed) return;
            if (!other.CompareTag("Player")) return;

            var health = other.GetComponentInParent<Cardwin.Combat.Health>();
            if (health == null) return;

            _playerModule = moduleType == ModuleType.Cursed
                ? health.GetComponent<PlayerCursedEightModuleState>()
                : (MonoBehaviour)health.GetComponent<PlayerBlessedEightModuleState>();

            if (_playerModule == null)
            {
                _playerModule = moduleType == ModuleType.Cursed
                    ? health.gameObject.AddComponent<PlayerCursedEightModuleState>()
                    : health.gameObject.AddComponent<PlayerBlessedEightModuleState>();
            }

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
            if (_playerModule is PlayerCursedEightModuleState cursed && !cursed.IsActive)
                cursed.Activate();
            else if (_playerModule is PlayerBlessedEightModuleState blessed && !blessed.IsActive)
                blessed.Activate();
            else
                return;

            _consumed = true;
            if (promptText != null) promptText.SetActive(false);
            if (visualRenderer != null) visualRenderer.enabled = false;
            Debug.Log($"[{moduleType}ModulePickup] Module consumed.");
            Destroy(gameObject, 0.1f);
        }
    }
}
