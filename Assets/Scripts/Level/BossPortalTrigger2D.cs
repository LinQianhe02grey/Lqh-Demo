using UnityEngine;

namespace Cardwin.Level
{
    public sealed class BossPortalTrigger2D : MonoBehaviour
    {
        [SerializeField]
        private BossPortal owner;

        private void Reset()
        {
            owner = GetComponentInParent<BossPortal>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (owner == null)
            {
                Debug.LogError("[BossPortalTrigger2D] Owner BossPortal is not assigned.", this);
                return;
            }

            owner.TryEnterPortal(other);
        }
    }
}
