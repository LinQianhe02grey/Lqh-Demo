using UnityEngine;
using UnityEngine.UI;

namespace Cardwin.UI
{
    public class CardSlotUI : MonoBehaviour
    {
        public Image cardIcon;
        public Text cardNameText;
        public Text cardDescText;
        public Text costText;
        public Button useButton;

        private string _cardId;

        public void SetCard(string cardId, string name, string desc, int cost) { }

        public void Clear() { }

        public void OnUseClicked() { }
    }
}
