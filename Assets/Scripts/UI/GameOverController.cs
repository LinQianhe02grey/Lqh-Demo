using UnityEngine;
using UnityEngine.UI;
using Cardwin.Combat;
using Cardwin.Core;
using Cardwin.Save;

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
            Debug.Log("[GameOver] Retry clicked.");
            Time.timeScale = 1f;
            IsGameOver = false;
            GameFlowManager.Instance.RetryCurrentScene();
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
