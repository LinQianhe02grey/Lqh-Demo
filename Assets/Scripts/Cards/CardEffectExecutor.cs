using UnityEngine;

namespace Cardwin.Cards
{
    public class CardEffectExecutor : MonoBehaviour
    {
        private PlayerCardContext _cardContext;

        public void Initialize(PlayerCardContext context)
        {
            _cardContext = context;
        }

        public void ExecuteOnEnemy(CardData card, GameObject enemyTarget) { }

        public void ExecuteOnSelf(CardData card) { }

        private void ExecuteDamage(GameObject target, int value, int repeatCount) { }

        private void ExecuteHeal(GameObject target, int value, int repeatCount) { }

        private void ExecuteGainBlock(GameObject target, int value, int repeatCount) { }

        private void ExecuteApplyStatus(GameObject target, string statusTag, int stacks) { }
    }
}
