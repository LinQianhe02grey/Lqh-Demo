using UnityEngine;
using UnityEngine.UI;
using Cardwin.Inventory;

namespace Cardwin.UI
{
    public class InventoryUI : MonoBehaviour
    {
        public GameObject inventoryPanel;
        public Transform slotContainer;
        public int gridColumns = 6;
        public int gridRows = 4;

        private InventorySystem _inventory;

        public void Bind(InventorySystem inventory) { _inventory = inventory; }

        public void Show() { inventoryPanel.SetActive(true); }

        public void Hide() { inventoryPanel.SetActive(false); }

        public void RefreshDisplay() { }

        public void OnSlotClicked(int slotIndex) { }

        public void OnDragStart(int slotIndex) { }

        public void OnDragEnd(int fromIndex, int toIndex) { }
    }
}
