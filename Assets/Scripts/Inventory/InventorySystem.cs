using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Cardwin.Inventory
{
    public class InventorySystem : MonoBehaviour
    {
        public int inventoryCapacity = 24;
        public List<InventorySlot> Slots { get; private set; } = new();

        public UnityEvent OnInventoryChanged;

        public void Initialize() { }

        public int AddItem(string itemId, int count) { return count; }

        public bool RemoveItem(string itemId, int count) { return false; }

        public void SwapSlots(int indexA, int indexB) { }

        public void ClearSlot(int index) { }

        public bool HasItem(string itemId) { return false; }

        public int GetItemCount(string itemId) { return 0; }
    }

    [System.Serializable]
    public class InventorySlot
    {
        public string itemId;
        public int count;
        public int maxStack = 99;

        public bool IsEmpty() { return string.IsNullOrEmpty(itemId) || count <= 0; }

        public bool CanStackWith(string otherItemId) { return itemId == otherItemId; }
    }
}
