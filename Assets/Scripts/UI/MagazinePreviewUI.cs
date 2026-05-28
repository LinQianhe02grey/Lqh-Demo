using UnityEngine;
using UnityEngine.UI;
using Cardwin.Magazine;

namespace Cardwin.UI
{
    public class MagazinePreviewUI : MonoBehaviour
    {
        public Text[] previewSlots;
        public Image[] previewIcons;
        public int previewCount = 3;

        private MagazineSystem _magazineSystem;

        public void Bind(MagazineSystem magazineSystem) { _magazineSystem = magazineSystem; }

        public void RefreshPreview() { }

        public void HighlightCurrentBullet(int index) { }
    }
}
