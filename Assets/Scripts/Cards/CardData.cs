using System.Collections.Generic;
using UnityEngine;

namespace Cardwin.Cards
{
    [CreateAssetMenu(fileName = "CardData_New", menuName = "Cardwin/CardData")]
    public class CardData : ScriptableObject
    {
        public string cardId;
        public string displayName;
        [TextArea] public string description;
        public int cost = 1;
        public TargetType targetType = TargetType.Enemy;
        public List<CardEffectEntry> effects = new();

        public bool IsSelfTarget()
        {
            return targetType == TargetType.Self;
        }
    }

    public enum TargetType
    {
        Enemy,
        Self,
        SelfOrEnemy
    }

    [System.Serializable]
    public struct CardEffectEntry
    {
        public CardEffectType effectType;
        public int value;
        public int repeatCount;

        public CardEffectEntry(CardEffectType effectType, int value, int repeatCount = 1)
        {
            this.effectType = effectType;
            this.value = value;
            this.repeatCount = repeatCount;
        }
    }
}
