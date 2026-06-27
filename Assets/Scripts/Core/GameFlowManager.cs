using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cardwin.Combat;
using Cardwin.Inventory;
using Cardwin.Cards;
using Cardwin.Magazine;
using Cardwin.Save;
using Cardwin.Settings;

namespace Cardwin.Core
{
    public class GameFlowManager : MonoBehaviour
    {
        private static GameFlowManager _instance;

        public static GameFlowManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameFlowManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("GameFlowManager");
                        _instance = go.AddComponent<GameFlowManager>();
                    }
                }
                return _instance;
            }
        }

        private int _currentSlotIndex = 1;
        private GameSaveData _pendingSaveData;

        public int CurrentSlotIndex => _currentSlotIndex;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            SettingsSystem.Load();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_pendingSaveData != null)
            {
                var data = _pendingSaveData;
                _pendingSaveData = null;
                ApplySaveAfterSceneLoaded(data);
            }
        }

        public void NewGame(int slotIndex)
        {
            if (!SaveSystem.IsValidSlot(slotIndex))
            {
                Debug.LogError($"[GameFlow] Invalid slot: {slotIndex}");
                return;
            }

            _currentSlotIndex = slotIndex;
            SaveSystem.DeleteSave(slotIndex);
            Time.timeScale = 1f;
            Debug.Log($"[GameFlow] New Game started. slot={slotIndex}");
            SceneManager.LoadScene("Demo_Combat");
        }

        public void ContinueGame(int slotIndex)
        {
            if (!SaveSystem.IsValidSlot(slotIndex))
            {
                Debug.LogError($"[GameFlow] Invalid slot: {slotIndex}");
                return;
            }

            if (!SaveSystem.HasSave(slotIndex))
            {
                Debug.LogWarning($"[GameFlow] No save in slot={slotIndex}");
                return;
            }

            _currentSlotIndex = slotIndex;
            Debug.Log($"[GameFlow] Continue Game. slot={slotIndex}");

            if (SaveSystem.TryLoad(slotIndex, out GameSaveData data))
            {
                string sceneName = string.IsNullOrEmpty(data.sceneName) ? "Demo_Combat" : data.sceneName;
                _pendingSaveData = data;
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogWarning($"[GameFlow] Failed to load save slot={slotIndex}.");
            }
        }

        public void OverwriteGame(int slotIndex)
        {
            if (!SaveSystem.IsValidSlot(slotIndex))
            {
                Debug.LogError($"[GameFlow] Invalid slot: {slotIndex}");
                return;
            }

            _currentSlotIndex = slotIndex;
            SaveSystem.DeleteSave(slotIndex);
            Time.timeScale = 1f;
            Debug.Log($"[GameFlow] Overwrite slot={slotIndex} and start new game.");
            SceneManager.LoadScene("Demo_Combat");
        }

        public void DeleteSaveSlot(int slotIndex)
        {
            if (!SaveSystem.IsValidSlot(slotIndex))
            {
                Debug.LogError($"[GameFlow] Invalid slot: {slotIndex}");
                return;
            }

            SaveSystem.DeleteSave(slotIndex);
        }

        public void SaveCurrentGame()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("[GameFlow] Cannot save: Player not found.");
                return;
            }

            var data = new GameSaveData();
            data.sceneName = SceneManager.GetActiveScene().name;

            var t = player.transform;
            data.playerPositionX = t.position.x;
            data.playerPositionY = t.position.y;

            var health = player.GetComponent<Health>();
            if (health != null)
            {
                data.currentHealth = health.currentHealth;
                data.maxHealth = health.maxHealth;
                data.currentBlock = health.currentBlock;
            }

            var alignment = player.GetComponent<PlayerAlignment>();
            if (alignment != null)
            {
                data.good = alignment.Good;
                data.evil = alignment.Evil;
            }

            var inventory = player.GetComponent<InventorySystem>();
            if (inventory != null)
            {
                var entries = inventory.GetCardCounts();
                data.inventoryCards = new List<CardStackSaveData>();
                foreach (var entry in entries)
                {
                    if (entry.card == null) continue;
                    data.inventoryCards.Add(new CardStackSaveData
                    {
                        cardId = entry.card.cardId,
                        count = entry.count
                    });
                }
            }

            var magazine = player.GetComponent<MagazineSystem>();
            if (magazine != null)
            {
                data.loadoutCardIds = new List<string>();
                var loadout = magazine.GetLoadoutCards();
                foreach (var card in loadout)
                {
                    if (card != null)
                        data.loadoutCardIds.Add(card.cardId);
                }
            }

            var enemiesRoot = GameObject.Find("LevelRoot/Enemies");
            if (enemiesRoot != null)
            {
                data.defeatedEnemyNames = new List<string>();
                for (int i = 0; i < enemiesRoot.transform.childCount; i++)
                {
                    var enemy = enemiesRoot.transform.GetChild(i).gameObject;
                    if (!enemy.activeInHierarchy)
                        data.defeatedEnemyNames.Add(enemy.name);
                }
            }

            SaveSystem.Save(_currentSlotIndex, data);
        }

        public void RetryCurrentScene()
        {
            Time.timeScale = 1f;
            Debug.Log("[GameFlow] Retry current scene.");
            SceneManager.LoadScene("Demo_Combat");
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        public void QuitGame()
        {
            Debug.Log("[GameFlow] Quit Game.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void ApplySaveAfterSceneLoaded(GameSaveData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[GameFlow] Cannot apply save: data is null.");
                return;
            }

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("[GameFlow] Cannot apply save: Player not found.");
                return;
            }

            player.transform.position = new Vector2(data.playerPositionX, data.playerPositionY);

            var health = player.GetComponent<Health>();
            if (health != null)
            {
                health.currentHealth = data.currentHealth;
                health.currentBlock = data.currentBlock;
            }

            var alignment = player.GetComponent<PlayerAlignment>();
            if (alignment != null)
            {
                alignment.SetValues(data.good, data.evil);
            }

            var db = FindObjectOfType<CardDatabase>();
            if (db == null)
            {
                CardDatabase[] all = Resources.FindObjectsOfTypeAll<CardDatabase>();
                if (all.Length > 0) db = all[0];
            }

            var inventory = player.GetComponent<InventorySystem>();
            if (inventory != null && data.inventoryCards != null)
            {
                var counts = new Dictionary<CardData, int>();
                foreach (var stack in data.inventoryCards)
                {
                    CardData card = db?.GetById(stack.cardId);
                    if (card == null)
                    {
                        Debug.LogWarning($"[Save] Missing cardId={stack.cardId}");
                        continue;
                    }
                    counts[card] = stack.count;
                }
                inventory.SetOwnedCardsFromCounts(counts);
            }

            var magazine = player.GetComponent<MagazineSystem>();
            if (magazine != null && data.loadoutCardIds != null)
            {
                var loadout = new List<CardData>();
                foreach (var cid in data.loadoutCardIds)
                {
                    CardData card = db?.GetById(cid);
                    if (card != null)
                        loadout.Add(card);
                }
                magazine.SetLoadoutCards(loadout, rebuildImmediately: true);
            }

            if (data.defeatedEnemyNames != null)
            {
                var enemiesRoot = GameObject.Find("LevelRoot/Enemies");
                if (enemiesRoot != null)
                {
                    foreach (string name in data.defeatedEnemyNames)
                    {
                        var enemy = enemiesRoot.transform.Find(name);
                        if (enemy != null)
                            enemy.gameObject.SetActive(false);
                    }
                }
            }

            Debug.Log("[GameFlow] Save data applied to scene.");
        }
    }
}
