namespace Cardwin.Cards
{
    public class CardRuntimeInstance
    {
        public CardData CardData { get; private set; }
        public int UpgradeLevel { get; private set; }

        public CardRuntimeInstance(CardData cardData, int upgradeLevel = 0)
        {
            CardData = cardData;
            UpgradeLevel = upgradeLevel;
        }

        public string CardId => CardData.cardId;
        public string DisplayName => CardData.displayName;
        public int Cost => CardData.cost;
        public bool IsSelfTarget => CardData.IsSelfTarget();
    }
}
