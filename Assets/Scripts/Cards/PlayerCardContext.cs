using UnityEngine;
using Cardwin.Combat;

namespace Cardwin.Cards
{
    public class PlayerCardContext
    {
        public GameObject player;
        public Transform firePoint;
        public Health playerHealth;
        public GameObject defaultProjectilePrefab;
        
        public event System.Action<int> OnFocusChanged;
public int focusStacks;

public void AddFocus(int amount)
        {
            focusStacks += amount;
            Debug.Log($"[CardContext] Focus stacks: {focusStacks}");
            OnFocusChanged?.Invoke(focusStacks);
        }

public float ConsumeFocusMultiplier()
        {
            if (focusStacks <= 0)
                return 1f;

            float multiplier = 1f + focusStacks * 0.5f;
            Debug.Log($"[CardContext] Consumed {focusStacks} Focus stacks, multiplier={multiplier}");
            focusStacks = 0;
            OnFocusChanged?.Invoke(focusStacks);
            return multiplier;
        }

        public Vector2 GetShootDirectionToMouse()
        {
            Vector3 mouseWorld = UnityEngine.Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;

            Vector3 origin = firePoint != null ? firePoint.position : player.transform.position;
            Vector2 dir = ((Vector2)mouseWorld - (Vector2)origin).normalized;

            if (dir == Vector2.zero)
                dir = Vector2.right;

            return dir;
        }
    }
}
