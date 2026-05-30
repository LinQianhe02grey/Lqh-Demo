using UnityEngine;
using UnityEngine.UI;
using Cardwin.Cards;

namespace Cardwin.UI
{
    public class CardSlotUI : MonoBehaviour
    {
        public Image backgroundImage;
        public Text nameText;
        public Text effectText;
        public bool isCurrent;

        public void SetCard(CardData card, bool current)
        {
            SetCard(card, current, false);
        }

        public void SetCard(CardData card, bool current, bool used)
        {
            isCurrent = current;
            gameObject.SetActive(true);

            if (nameText != null)
            {
                string prefix = current ? "> " : "  ";
                string suffix = current ? " <" : "";
                nameText.text = prefix + card.cardName + suffix;
                nameText.color = current ? Color.white : used ? new Color(0.4f, 0.4f, 0.4f) : new Color(0.7f, 0.7f, 0.7f);
                nameText.raycastTarget = false;
            }

            if (effectText != null)
            {
                effectText.text = used ? "[Used]" : $"L:{EffectToShort(card.leftClickEffect)} R:{EffectToShort(card.rightClickEffect)}";
                effectText.raycastTarget = false;
            }

            if (backgroundImage != null)
            {
                if (current)
                    backgroundImage.color = new Color(1f, 1f, 0.5f, 0.5f);
                else if (used)
                    backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 0.3f);
                else
                    backgroundImage.color = new Color(0.3f, 0.3f, 0.3f, 0.25f);

                backgroundImage.raycastTarget = false;
            }

            transform.localScale = current ? Vector3.one * 1.1f : Vector3.one;
        }

        public void SetEmpty()
        {
            gameObject.SetActive(true);
            isCurrent = false;
            transform.localScale = Vector3.one;

            if (nameText != null)
            {
                nameText.text = "---";
                nameText.color = new Color(0.3f, 0.3f, 0.3f);
                nameText.raycastTarget = false;
            }
            if (effectText != null)
            {
                effectText.text = "";
                effectText.raycastTarget = false;
            }
            if (backgroundImage != null)
            {
                backgroundImage.color = new Color(0.15f, 0.15f, 0.15f, 0.1f);
                backgroundImage.raycastTarget = false;
            }
        }

        public void SetCardForInventory(CardData card, UnityEngine.Events.UnityAction onClick)
        {
            SetCard(card, false, false);
            Button btn = GetComponent<Button>();
            if (btn == null)
                btn = gameObject.AddComponent<Button>();
            btn.targetGraphic = backgroundImage;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(onClick);
        }

        public void SetCardForLoadout(CardData card, int index, UnityEngine.Events.UnityAction<int> onClick)
        {
            SetCard(card, false, false);
            Button btn = GetComponent<Button>();
            if (btn == null)
                btn = gameObject.AddComponent<Button>();
            btn.targetGraphic = backgroundImage;
            btn.onClick.RemoveAllListeners();
            int capturedIndex = index;
            btn.onClick.AddListener(() => onClick(capturedIndex));
        }

        public void SetEmptyLoadoutSlot(int index, UnityEngine.Events.UnityAction<int> onClick)
        {
            SetEmpty();
            Button btn = GetComponent<Button>();
            if (btn == null)
                btn = gameObject.AddComponent<Button>();
            btn.targetGraphic = backgroundImage;
            btn.onClick.RemoveAllListeners();
            int capturedIndex = index;
            btn.onClick.AddListener(() => onClick(capturedIndex));
        }

        public void SetReloading()
        {
            gameObject.SetActive(true);
            isCurrent = false;
            transform.localScale = Vector3.one;

            if (nameText != null)
            {
                nameText.text = "Reloading";
                nameText.color = new Color(1f, 0.7f, 0.3f);
                nameText.raycastTarget = false;
            }
            if (effectText != null)
            {
                effectText.text = "";
                effectText.raycastTarget = false;
            }
            if (backgroundImage != null)
            {
                backgroundImage.color = new Color(1f, 0.5f, 0f, 0.2f);
                backgroundImage.raycastTarget = false;
            }
        }

        public static string EffectToShortPublic(CardEffectType e)
        {
            return EffectToShort(e);
        }

        private static string EffectToShort(CardEffectType e)
        {
            switch (e)
            {
                case CardEffectType.Damage: return "Dmg";
                case CardEffectType.Block:  return "Blk";
                case CardEffectType.Heal:   return "Heal";
                case CardEffectType.Focus:  return "Fcs";
                default:                    return e.ToString();
            }
        }
    }
}
