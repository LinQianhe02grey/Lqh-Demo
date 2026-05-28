using UnityEngine;
using UnityEngine.UI;

namespace Cardwin.UI
{
    public class CombatHUD : MonoBehaviour
    {
        public Slider hpBar;
        public Slider blockBar;
        public Text hpText;
        public Text blockText;
        public Text ammoText;
        public Text stateText;
        public Slider reloadBar;

        public void UpdateHP(int current, int max) { }

        public void UpdateBlock(int amount) { }

        public void UpdateAmmo(int current, int total) { }

        public void UpdateReloadProgress(float progress) { }

        public void SetStateText(string text) { }
    }
}
