namespace Cardwin.Magazine
{
    [System.Serializable]
    public class MagazineSlot
    {
        public int index;
        public string cardId;
        public string displayName;
        public string description;

        public MagazineSlot(int index = 0)
        {
            this.index = index;
        }

        public void SetCard(string cardId, string displayName, string description)
        {
            this.cardId = cardId;
            this.displayName = displayName;
            this.description = description;
        }

        public void Clear()
        {
            cardId = null;
            displayName = null;
            description = null;
        }
    }
}
