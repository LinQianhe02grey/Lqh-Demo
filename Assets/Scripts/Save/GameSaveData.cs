using System;
using System.Collections.Generic;

namespace Cardwin.Save
{
    [Serializable]
    public class CardStackSaveData
    {
        public string cardId;
        public int count;
    }

    [Serializable]
    public class GameSaveData
    {
        public int slotIndex;
        public string savedAt;
        public string gameVersion;

        public string sceneName;

        public float playerPositionX;
        public float playerPositionY;
        public int currentHealth;
        public int maxHealth;
        public int currentBlock;
        public int good;
        public int evil;

        public List<CardStackSaveData> inventoryCards = new();
        public List<string> loadoutCardIds = new();
        public List<string> defeatedEnemyNames = new();
    }
}
