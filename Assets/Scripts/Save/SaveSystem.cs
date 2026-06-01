using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Cardwin.Save
{
    public static class SaveSystem
    {
        public const int MaxSlots = 3;

        public static string GetSavePath(int slotIndex)
        {
            return Path.Combine(Application.persistentDataPath, $"cardwin_save_slot_{slotIndex}.json");
        }

        public static bool IsValidSlot(int slotIndex)
        {
            return slotIndex >= 1 && slotIndex <= MaxSlots;
        }

        public static bool HasSave(int slotIndex)
        {
            if (!IsValidSlot(slotIndex))
                return false;
            return File.Exists(GetSavePath(slotIndex));
        }

        public static void Save(int slotIndex, GameSaveData data)
        {
            if (!IsValidSlot(slotIndex))
            {
                Debug.LogError($"[SaveSystem][Error] Invalid slot index: {slotIndex}");
                return;
            }

            if (data == null)
            {
                Debug.LogError("[SaveSystem][Error] Save data is null.");
                return;
            }

            data.slotIndex = slotIndex;
            data.savedAt = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            data.gameVersion = Application.version;

            try
            {
                string path = GetSavePath(slotIndex);
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(path, json);
                Debug.Log($"[SaveSystem] Saved slot={slotIndex} to: {path}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SaveSystem][Error] Failed to save slot={slotIndex}: {ex.Message}");
            }
        }

        public static bool TryLoad(int slotIndex, out GameSaveData data)
        {
            data = null;

            if (!IsValidSlot(slotIndex))
            {
                Debug.LogError($"[SaveSystem][Error] Invalid slot index: {slotIndex}");
                return false;
            }

            if (!HasSave(slotIndex))
                return false;

            try
            {
                string path = GetSavePath(slotIndex);
                string json = File.ReadAllText(path);
                data = JsonUtility.FromJson<GameSaveData>(json);
                Debug.Log($"[SaveSystem] Loaded slot={slotIndex} from: {path}");
                return data != null;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SaveSystem][Error] Failed to load slot={slotIndex}: {ex.Message}");
                return false;
            }
        }

        public static void DeleteSave(int slotIndex)
        {
            if (!IsValidSlot(slotIndex))
            {
                Debug.LogError($"[SaveSystem][Error] Invalid slot index: {slotIndex}");
                return;
            }

            if (!HasSave(slotIndex))
                return;

            try
            {
                string path = GetSavePath(slotIndex);
                File.Delete(path);
                Debug.Log($"[SaveSystem] Deleted save slot={slotIndex}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SaveSystem][Error] Failed to delete slot={slotIndex}: {ex.Message}");
            }
        }

        public static List<SaveSlotInfo> GetAllSlotInfos()
        {
            var list = new List<SaveSlotInfo>();
            for (int i = 1; i <= MaxSlots; i++)
            {
                var info = new SaveSlotInfo { slotIndex = i };

                if (TryLoad(i, out GameSaveData data) && data != null)
                {
                    info.hasSave = true;
                    info.sceneName = data.sceneName;
                    info.savedAt = data.savedAt;
                    info.playerCurrentHealth = data.currentHealth;
                    info.playerMaxHealth = data.maxHealth;
                    info.inventoryTotalCards = data.inventoryCards?.Count ?? 0;
                    info.loadoutCount = data.loadoutCardIds?.Count ?? 0;
                }

                list.Add(info);
            }
            return list;
        }
    }
}
