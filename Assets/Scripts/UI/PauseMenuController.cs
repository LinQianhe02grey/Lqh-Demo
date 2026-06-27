using UnityEngine;
using UnityEngine.UI;
using Cardwin.Combat;
using Cardwin.Core;

namespace Cardwin.UI
{
    public class PauseMenuController : MonoBehaviour
    {
        [Header("Pause Panel")]
        public GameObject pausePanel;

        [Header("Current Slot")]
        public Text currentSlotText;

        [Header("Buttons")]
        public Button resumeButton;
        public Button saveButton;
        public Button settingsButton;
        public Button mainMenuButton;
        public Button quitButton;

        [Header("Settings Panel")]
        public GameObject settingsPanel;
        public SettingsMenuController settingsMenuController;

        [Header("Hints")]
        public Text hintText;

        private PlayerController2D _playerController;
        private MagazineEditUI _magazineEditUI;

        private void Start()
        {
            _playerController = FindObjectOfType<PlayerController2D>();
            _magazineEditUI = FindObjectOfType<MagazineEditUI>();

            if (pausePanel != null)
                pausePanel.SetActive(false);

            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveAllListeners();
                resumeButton.onClick.AddListener(OnResume);
            }

            if (saveButton != null)
            {
                saveButton.onClick.RemoveAllListeners();
                saveButton.onClick.AddListener(OnSave);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.RemoveAllListeners();
                settingsButton.onClick.AddListener(OpenSettings);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveAllListeners();
                mainMenuButton.onClick.AddListener(() =>
                {
                    Time.timeScale = 1f;
                    GameFlowManager.Instance.ReturnToMainMenu();
                });
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveAllListeners();
                quitButton.onClick.AddListener(() => GameFlowManager.Instance.QuitGame());
            }
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape))
                return;

            if (GameOverController.IsGameOver)
                return;

            if (_magazineEditUI != null && _magazineEditUI.IsOpen)
                return;

            if (settingsMenuController != null && settingsMenuController.IsOpen)
                return;

            TogglePause();
        }

        private void TogglePause()
        {
            if (pausePanel == null)
                return;

            bool wasPaused = pausePanel.activeSelf;
            pausePanel.SetActive(!wasPaused);

            if (!wasPaused)
            {
                if (currentSlotText != null)
                    currentSlotText.text = $"Current Slot: {GameFlowManager.Instance.CurrentSlotIndex}";

                Time.timeScale = 0f;
                if (_playerController != null)
                    _playerController.SetInputLocked(true);
            }
            else
            {
                Time.timeScale = 1f;
                if (_playerController != null)
                    _playerController.SetInputLocked(false);
                if (hintText != null)
                    hintText.text = "";
            }
        }

        private void OnResume()
        {
            if (pausePanel != null)
                pausePanel.SetActive(false);

            Time.timeScale = 1f;
            if (_playerController != null)
                _playerController.SetInputLocked(false);
            if (hintText != null)
                hintText.text = "";
        }

        private void OnSave()
        {
            int slot = GameFlowManager.Instance.CurrentSlotIndex;
            GameFlowManager.Instance.SaveCurrentGame();
            if (hintText != null)
                hintText.text = $"Saved to Slot {slot}.";
        }

        private void OpenSettings()
        {
            Debug.Log("[PauseMenu] Settings clicked.");

            if (settingsMenuController == null)
                settingsMenuController = FindObjectOfType<SettingsMenuController>(true);

            if (settingsMenuController == null)
            {
                Debug.LogError("[PauseMenu] SettingsMenuController not found.");
                return;
            }

            bool opened = settingsMenuController.OpenFromPauseMenu();

            if (!opened)
            {
                Debug.LogError("[PauseMenu] Settings open failed.");
                if (pausePanel != null) pausePanel.SetActive(true);
            }
        }

        public void HidePausePanel()
        {
            if (pausePanel != null)
                pausePanel.SetActive(false);
            Time.timeScale = 1f;
            if (_playerController != null)
                _playerController.SetInputLocked(false);
        }
    }
}
