using UnityEngine;
using UnityEngine.UI;
using Cardwin.Shop;

namespace Cardwin.UI
{
    public class ShopUI : MonoBehaviour
    {
        public GameObject shopPanel;
        public Text currencyText;
        public Transform merchandiseContainer;
        public Button refreshButton;
        public Text refreshCostText;

        private ShopManager _shopManager;
        private EconomySystem _economy;

        public void Bind(ShopManager shop, EconomySystem economy)
        {
            _shopManager = shop;
            _economy = economy;
        }

        public void Show() { shopPanel.SetActive(true); }

        public void Hide() { shopPanel.SetActive(false); }

        public void RefreshDisplay() { }

        public void OnBuyClicked(string itemId) { }

        public void OnSellClicked(string itemId) { }

        public void OnRefreshClicked() { }
    }
}
