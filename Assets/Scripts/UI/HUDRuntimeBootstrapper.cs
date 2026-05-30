using UnityEngine;
using UnityEngine.UI;

namespace Cardwin.UI
{
    [DefaultExecutionOrder(-900)]
    public class HUDRuntimeBootstrapper : MonoBehaviour
    {
        private void Awake()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                Debug.Log("[HUDBootstrapper] Canvas created.");
            }

            CombatHUD hud = canvas.GetComponent<CombatHUD>();
            if (hud == null)
            {
                hud = canvas.gameObject.AddComponent<CombatHUD>();
                Debug.Log("[HUDBootstrapper] CombatHUD attached to Canvas.");
            }
        }
    }
}
