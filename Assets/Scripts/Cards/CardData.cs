using UnityEngine;

namespace Cardwin.Cards
{
    public enum CardUseTarget
    {
        Enemy,
        Self,
        Both
    }

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

        [Header("Use Target")]
        public CardUseTarget useTarget = CardUseTarget.Enemy;

        [Header("CSV Imported Fields")]
        public int goodCost;
        public int evilCost;
        public string baseEffectDescription;
        public string finalValueRaw;
        public float finalValue;
        public string valueUnit;
        public string cooldownLimit;
        public string role;
        public string riskNotes;
        public bool enabled = true;
        public bool implemented = true;
        public bool isImportedFromCsv;

        [Header("Lua Bullet (runtime channel)")]
        public bool isLuaBullet;
        public string luaBulletId;

        public bool IsOffensive
        {
            get
            {
                if (cardType == CardType.Attack)
                    return true;
                if (leftClickEffect == CardEffectType.Damage)
                    return true;
                if (rightClickEffect == CardEffectType.Damage)
                    return true;
                return false;
            }
        }
    }
}
