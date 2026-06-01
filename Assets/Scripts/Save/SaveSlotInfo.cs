using System;

namespace Cardwin.Save
{
    [Serializable]
    public class SaveSlotInfo
    {
        public int slotIndex;
        public bool hasSave;
        public string sceneName;
        public string savedAt;
        public int playerCurrentHealth;
        public int playerMaxHealth;
        public int inventoryTotalCards;
        public int loadoutCount;
    }
}
