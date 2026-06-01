using UnityEngine;

namespace Cardwin.Core
{
    public class FinishGateTrigger : MonoBehaviour
    {
        public string clearMessage = "[Level] Demo stage clear.";

        private bool _hasCleared;

        private void Awake()
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col == null)
                col = gameObject.AddComponent<BoxCollider2D>();

            col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryClear(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryClear(other);
        }

        private void TryClear(Collider2D other)
        {
            if (_hasCleared || !other.CompareTag("Player"))
                return;

            _hasCleared = true;
            Debug.Log(clearMessage);
        }
    }
}
