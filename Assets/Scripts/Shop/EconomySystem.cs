using UnityEngine;
using UnityEngine.Events;

namespace Cardwin.Shop
{
    public class EconomySystem : MonoBehaviour
    {
        public int currency;
        public const string CurrencyName = "Gold";

        public UnityEvent<int> OnCurrencyChanged;

        public void AddCurrency(int amount) { }

        public bool SpendCurrency(int amount) { return false; }

        public bool CanAfford(int cost) { return currency >= cost; }
    }
}
