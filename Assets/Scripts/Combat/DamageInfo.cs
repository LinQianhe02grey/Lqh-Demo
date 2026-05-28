namespace Cardwin.Combat
{
    public struct DamageInfo
    {
        public int amount;
        public int focusBonus;
        public string sourceCardId;

        public DamageInfo(int amount, int focusBonus, string sourceCardId)
        {
            this.amount = amount;
            this.focusBonus = focusBonus;
            this.sourceCardId = sourceCardId;
        }

        public int TotalDamage => amount + focusBonus;
    }
}
