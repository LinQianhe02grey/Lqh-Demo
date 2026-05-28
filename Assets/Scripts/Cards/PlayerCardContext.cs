using UnityEngine;
using Cardwin.Combat;

namespace Cardwin.Cards
{
    [CreateAssetMenu(fileName = "PlayerCardContext_Default", menuName = "Cardwin/PlayerCardContext")]
    public class PlayerCardContext : ScriptableObject
    {
        public Health playerHealth;
        public int focusStacks;
        public int GetFocusBonus() { return focusStacks; }

        public void CacheReferences(GameObject playerObject)
        {
            playerHealth = playerObject.GetComponent<Health>();
        }
    }
}
