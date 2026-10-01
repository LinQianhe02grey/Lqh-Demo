using UnityEngine;
namespace OrbitBreaker
{
    // Persist consumption through the same per-run keys used for Boss destruction.
    public static class OrbitBreakerSpentProp
    {
        public static void Consume(GameObject prop)
        {
            if(prop==null || !prop.activeSelf)return;
            var arena=prop.GetComponentInParent<OrbitBreakerProceduralArena>();
            if(arena!=null)arena.DestroyProp(prop);
            else prop.SetActive(false);
        }
    }
}
