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
        public string DisplayName => CardData.cardName;
        public int Cost => 0;
        public bool IsSelfTarget => false;
    }
}
