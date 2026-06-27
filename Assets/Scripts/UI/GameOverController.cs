using UnityEngine;
using UnityEngine.UI;
using Cardwin.Combat;
using Cardwin.Core;
using Cardwin.Save;
using Cardwin.Player;

namespace Cardwin.UI
{
    public class GameOverController : MonoBehaviour
    {
        [Header("Game Over Panel")]
        public GameObject gameOverPanel;

        [Header("Buttons")]
        public Button retryButton;
        public Button loadSaveButton;
        public Button mainMenuButton;
        public Button quitButton;

        [Header("Hints")]
        public Text hintText;

        public static bool IsGameOver { get; private set; }
        public static GameOverController Instance { get; private set; }

        private PlayerController2D _playerController;

        private void Start()
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            IsGameOver = false;

            if (retryButton != null)
            {
                retryButton.onClick.RemoveAllListeners();
                retryButton.onClick.AddListener(OnRetryClicked);
            }

            if (loadSaveButton != null)
            {
                loadSaveButton.onClick.RemoveAllListeners();
                loadSaveButton.onClick.AddListener(OnLoadSaveClicked);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveAllListeners();
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveAllListeners();
                quitButton.onClick.AddListener(OnQuitClicked);
            }
        }

        private void OnEnable()
        {
            Instance = this;
        }

        private void OnDisable()
        {
            if (Instance == this)
                Instance = null;
        }

        public static void HandlePlayerDeath()
        {
            if (IsGameOver)
                return;

            var goc = Instance;
            if (goc == null)
            {
                Debug.LogError("[GameOver] Instance is null - cannot handle player death.");
                return;
            }

            goc.TriggerGameOver();
        }

        public void TriggerGameOver()
        {
            if (IsGameOver)
                return;

            OnPlayerDeath();
        }

        private void OnPlayerDeath()
        {
            if (IsGameOver)
                return;

            IsGameOver = true;
            Debug.Log("[GameOver] Player died.");

            // Authoritatively tear down the Confession Night rhythm mode BEFORE freezing
            // time / showing the GameOver panel, so the (DontDestroyOnLoad) rhythm canvas
            // can't cover the panel and stale rhythm state can't conflict with Retry.
            // No-ops safely when rhythm mode is not active (Instance == null).
            Cardwin.Modules.RhythmGameController.ForceStopRhythmMode("PlayerDeath");

            Time.timeScale = 0f;

            if (_playerController == null)
                _playerController = FindObjectOfType<PlayerController2D>();
            if (_playerController != null)
            {
                _playerController.SetDead(true);
                Debug.Log("[GameOver] Player control disabled.");
            }

            var pauseMenu = FindObjectOfType<PauseMenuController>();
            if (pauseMenu != null)
                pauseMenu.HidePausePanel();

            var magUI = FindObjectOfType<MagazineEditUI>();
            if (magUI != null && magUI.IsOpen)
                magUI.Close();

            ShowGameOverPanel();
        }

        private void ShowGameOverPanel()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.transform.SetAsLastSibling();
                gameOverPanel.SetActive(true);
                Debug.Log("[GameOver] Show panel.");
            }

            UpdateLoadSaveButton();

            if (hintText != null)
                hintText.text = "";
        }

        private void UpdateLoadSaveButton()
        {
            if (loadSaveButton == null)
                return;

            int slot = GameFlowManager.Instance != null
                ? GameFlowManager.Instance.CurrentSlotIndex
                : 1;

            loadSaveButton.interactable = SaveSystem.HasSave(slot);
        }

        private void OnRetryClicked()
        {
            Debug.Log("[Retry] Retry clicked.");

            // Close GameOver UI and restore time before recovering the player.
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            Time.timeScale = 1f;
            IsGameOver = false;

            // Belt-and-suspenders: ensure the rhythm mode is fully stopped before the
            // in-place player revive (idempotent; no-ops if already stopped at death).
            Cardwin.Modules.RhythmGameController.ForceStopRhythmMode("RetryBeforeReload");

            // The Player is a global DontDestroyOnLoad object, so a scene reload does NOT
            // rebuild it. Retry must explicitly clear its death runtime state via the
            // unified PlayerRuntimeReset entry (revive + re-enable + respawn placement).
            var reset = ResolvePlayerRuntimeReset();
            if (reset != null)
            {
                reset.ResetForRetry();
            }
            else
            {
                Debug.LogError("[Retry] PlayerRuntimeReset not found on Player. Falling back to scene reload.");
                GameFlowManager.Instance.RetryCurrentScene();
            }
        }

        private PlayerRuntimeReset ResolvePlayerRuntimeReset()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var reset = player.GetComponent<PlayerRuntimeReset>();
                if (reset != null)
                    return reset;
            }
            return FindObjectOfType<PlayerRuntimeReset>();
        }

        private void OnLoadSaveClicked()
        {
            int slot = GameFlowManager.Instance != null
                ? GameFlowManager.Instance.CurrentSlotIndex
                : 1;

            if (!SaveSystem.HasSave(slot))
            {
                if (hintText != null)
                    hintText.text = $"No save in Slot {slot}.";
                Debug.LogWarning($"[GameOver] No save in slot={slot}");
                return;
            }

            Debug.Log($"[GameOver] Load Save clicked. slot={slot}");
            Time.timeScale = 1f;
            IsGameOver = false;
            GameFlowManager.Instance.ContinueGame(slot);
        }

        private void OnMainMenuClicked()
        {
            Debug.Log("[GameOver] Main Menu clicked.");
            Time.timeScale = 1f;
            IsGameOver = false;
            GameFlowManager.Instance.ReturnToMainMenu();
        }

        private void OnQuitClicked()
        {
            Debug.Log("[GameOver] Quit clicked.");
            Time.timeScale = 1f;
            GameFlowManager.Instance.QuitGame();
        }
    }
}
