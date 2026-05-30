using UnityEngine;

namespace Cardwin.Cards
{
    [CreateAssetMenu(fileName = "CardData_New", menuName = "Cardwin/Card Data")]
    public class CardData : ScriptableObject
    {
        public string cardId;
        public string cardName;
        public CardType cardType;
        public CardRarity rarity;
        public Sprite icon;

        public int damage;
        public int block;
        public int heal;
        public int focusGain;

        public CardEffectType leftClickEffect = CardEffectType.Damage;
        public CardEffectType rightClickEffect = CardEffectType.None;

        public GameObject projectilePrefab;

        [TextArea]
        public string description;
    }
}
