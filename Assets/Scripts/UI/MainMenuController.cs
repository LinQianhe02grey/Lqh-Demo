using UnityEngine;
using UnityEngine.UI;
using Cardwin.Core;
using Cardwin.Save;

namespace Cardwin.UI
{
    public class MainMenuController : MonoBehaviour
    {
        private enum SaveSelectMode { Continue, NewGame }
        private SaveSelectMode _currentMode;
        private int _confirmTargetSlot;
        private string _confirmAction;

        [Header("Main Panel")]
        public GameObject mainPanel;
        public Button newGameButton;
        public Button continueButton;
        public Button quitButton;

        [Header("Save Select Panel")]
        public GameObject saveSelectPanel;
        public Button backButton;

        [Header("Slot 1")]
        public Text slot1Info;
        public Button slot1Action;
        public Button slot1Delete;
        public Text slot1ActionLabel;

        [Header("Slot 2")]
        public Text slot2Info;
        public Button slot2Action;
        public Button slot2Delete;
        public Text slot2ActionLabel;

        [Header("Slot 3")]
        public Text slot3Info;
        public Button slot3Action;
        public Button slot3Delete;
        public Text slot3ActionLabel;

        [Header("Confirm Panel")]
        public GameObject confirmPanel;
        public Text confirmMessage;
        public Button confirmYesButton;
        public Button confirmNoButton;

        [Header("Hints")]
        public Text hintText;

        private void Start()
        {
            ShowMainPanel();

            if (newGameButton != null)
                newGameButton.onClick.AddListener(OnNewGameClicked);

            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(() => GameFlowManager.Instance.QuitGame());

            if (backButton != null)
                backButton.onClick.AddListener(ShowMainPanel);

            if (confirmYesButton != null)
                confirmYesButton.onClick.AddListener(OnConfirmYes);

            if (confirmNoButton != null)
                confirmNoButton.onClick.AddListener(() => { if (confirmPanel != null) confirmPanel.SetActive(false); });

            RefreshMainPanelButtons();
        }

        private void OnEnable()
        {
            RefreshMainPanelButtons();
        }

        private void RefreshMainPanelButtons()
        {
            if (continueButton != null)
                continueButton.interactable = SaveSystem.HasSave(1) || SaveSystem.HasSave(2) || SaveSystem.HasSave(3);
        }

        public void ShowMainPanel()
        {
            if (mainPanel != null) mainPanel.SetActive(true);
            if (saveSelectPanel != null) saveSelectPanel.SetActive(false);
            if (confirmPanel != null) confirmPanel.SetActive(false);
            RefreshMainPanelButtons();
        }

        private void OnNewGameClicked()
        {
            _currentMode = SaveSelectMode.NewGame;
            ShowSaveSelectPanel();
        }

        private void OnContinueClicked()
        {
            _currentMode = SaveSelectMode.Continue;
            ShowSaveSelectPanel();
        }

        private void ShowSaveSelectPanel()
        {
            if (mainPanel != null) mainPanel.SetActive(false);
            if (saveSelectPanel != null) saveSelectPanel.SetActive(true);
            RefreshSaveSlots();
        }

        private void RefreshSaveSlots()
        {
            var infos = SaveSystem.GetAllSlotInfos();
            if (infos == null || infos.Count < 3) return;

            SetupSlot(1, infos[0], slot1Info, slot1Action, slot1Delete, slot1ActionLabel);
            SetupSlot(2, infos[1], slot2Info, slot2Action, slot2Delete, slot2ActionLabel);
            SetupSlot(3, infos[2], slot3Info, slot3Action, slot3Delete, slot3ActionLabel);
        }

        private void SetupSlot(int slotIdx, SaveSlotInfo info, Text infoText,
            Button actionBtn, Button deleteBtn, Text actionLabel)
        {
            bool hasSave = info.hasSave;

            if (infoText != null)
            {
                if (hasSave)
                    infoText.text = $"Slot {slotIdx}  |  {info.sceneName}  |  HP {info.playerCurrentHealth}/{info.playerMaxHealth}  |  Cards {info.inventoryTotalCards}  |  {info.savedAt}";
                else
                    infoText.text = $"Slot {slotIdx}  |  Empty";
            }

            if (actionLabel != null)
            {
                if (_currentMode == SaveSelectMode.Continue)
                    actionLabel.text = "Continue";
                else
                    actionLabel.text = hasSave ? "Overwrite" : "New Game";
            }

            if (actionBtn != null)
            {
                actionBtn.onClick.RemoveAllListeners();
                int slot = slotIdx;
                if (_currentMode == SaveSelectMode.Continue)
                {
                    actionBtn.onClick.AddListener(() => GameFlowManager.Instance.ContinueGame(slot));
                    actionBtn.interactable = hasSave;
                }
                else
                {
                    if (hasSave)
                        actionBtn.onClick.AddListener(() => RequestConfirm("Overwrite", slot,
                            $"Overwrite Slot {slot}? All progress will be lost.",
                            () => GameFlowManager.Instance.OverwriteGame(slot)));
                    else
                        actionBtn.onClick.AddListener(() => GameFlowManager.Instance.NewGame(slot));
                    actionBtn.interactable = true;
                }
            }

            if (deleteBtn != null)
            {
                deleteBtn.onClick.RemoveAllListeners();
                deleteBtn.gameObject.SetActive(hasSave);
                if (hasSave)
                {
                    int slot = slotIdx;
                    deleteBtn.onClick.AddListener(() => RequestConfirm("Delete", slot,
                        $"Delete Slot {slot} save permanently?",
                        () =>
                        {
                            GameFlowManager.Instance.DeleteSaveSlot(slot);
                            RefreshSaveSlots();
                        }));
                }
            }
        }

        private void RequestConfirm(string action, int slot, string message, System.Action onConfirm)
        {
            _confirmAction = action;
            _confirmTargetSlot = slot;

            if (confirmPanel != null)
            {
                if (confirmMessage != null)
                    confirmMessage.text = message;
                confirmPanel.SetActive(true);
            }

            _pendingConfirmCallback = onConfirm;
        }

        private System.Action _pendingConfirmCallback;

        private void OnConfirmYes()
        {
            if (confirmPanel != null) confirmPanel.SetActive(false);
            _pendingConfirmCallback?.Invoke();
            _pendingConfirmCallback = null;
        }
    }
}
