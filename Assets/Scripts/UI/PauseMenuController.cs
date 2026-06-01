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
        public Button mainMenuButton;
        public Button quitButton;

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
                resumeButton.onClick.AddListener(OnResume);

            if (saveButton != null)
                saveButton.onClick.AddListener(OnSave);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(() =>
                {
                    Time.timeScale = 1f;
                    GameFlowManager.Instance.ReturnToMainMenu();
                });

            if (quitButton != null)
                quitButton.onClick.AddListener(() => GameFlowManager.Instance.QuitGame());
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape))
                return;

            if (GameOverController.IsGameOver)
                return;

            if (_magazineEditUI != null && _magazineEditUI.IsOpen)
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
