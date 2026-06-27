using UnityEngine;
using Cardwin.Combat;

namespace Cardwin.Modules
{
    public sealed class BlessedEightModulePickup : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private GameObject promptText;
        [SerializeField] private SpriteRenderer visualRenderer;

        private bool _consumed;
        private bool _playerInRange;
        private PlayerBlessedEightModuleState _playerModule;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_consumed) return;
            if (!other.CompareTag("Player")) return;

            var health = other.GetComponentInParent<Health>();
            if (health == null) return;

            _playerModule = health.GetComponent<PlayerBlessedEightModuleState>();
            if (_playerModule == null)
                _playerModule = health.gameObject.AddComponent<PlayerBlessedEightModuleState>();

            _playerInRange = true;
            if (promptText != null) promptText.SetActive(true);
            Debug.Log($"[BlessedPickup] Player in range on {health.gameObject.name}");
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
                ActivateBlessed();
        }

        private void ActivateBlessed()
        {
            if (_playerModule.IsActive) return;
            _playerModule.Activate();
            _consumed = true;
            if (promptText != null) promptText.SetActive(false);
            if (visualRenderer != null) visualRenderer.enabled = false;
            Debug.Log("[BlessedPickup] Module consumed.");
            Destroy(gameObject, 0.1f);
        }
    }
}
