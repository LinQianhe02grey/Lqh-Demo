using UnityEngine;

namespace Cardwin.Combat
{
    public class SceneCollisionReporter : MonoBehaviour
    {
        [Header("Report Settings")]
        public bool reportOnStart = true;
        public KeyCode reportKey = KeyCode.F1;

        private void Start()
        {
            if (reportOnStart)
                ReportSceneColliders();
        }

        private void Update()
        {
            if (Input.GetKeyDown(reportKey))
                ReportSceneColliders();
        }

        private static void ReportSceneColliders()
        {
            Debug.Log("===== Scene Collider Report =====");

            Collider2D[] allColliders = FindObjectsByType<Collider2D>(FindObjectsSortMode.None);

            foreach (Collider2D col in allColliders)
            {
                GameObject go = col.gameObject;
                string layerName = LayerMask.LayerToName(go.layer);
                string triggerTag = col.isTrigger ? "[TRIGGER]" : "[SOLID]";
                string bodyType = "";

                Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
                if (rb != null)
                    bodyType = $"  BodyType={rb.bodyType}";

                Debug.Log($"  {go.name}  Layer={layerName}  {triggerTag}  {col.GetType().Name}{bodyType}");
            }

            Debug.Log("===== End Report =====");
        }
    }
}
