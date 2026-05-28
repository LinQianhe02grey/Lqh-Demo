using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Cardwin.Shop
{
    public class ShopManager : MonoBehaviour
    {
        public int shopSlotCount = 6;
        public int refreshCost = 10;

        public List<string> CurrentMerchandise { get; private set; } = new();

        public UnityEvent OnShopRefreshed;

        public void RefreshShop() { }

        public bool BuyItem(string itemId) { return false; }

        public bool SellItem(string itemId) { return false; }
    }
}
