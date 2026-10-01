using UnityEngine;
namespace OrbitBreaker
{
    [RequireComponent(typeof(OrbitBreakerGameManager))]
    public sealed class OrbitBreakerBossDirector : MonoBehaviour
    {
        private OrbitBreakerGameManager manager;
        public OrbitBreakerOctopusBoss Boss { get; private set; }
        private void Awake(){manager=GetComponent<OrbitBreakerGameManager>();}
        private void OnEnable(){if(manager!=null)manager.StateChanged+=Changed;}
        private void OnDisable(){if(manager!=null)manager.StateChanged-=Changed;}
        private void Changed(GameState state)
        {
            if(state!=GameState.Playing || manager.Mode!=GameMode.OrbitBreakerBattle || Boss!=null)return;
            var player=FindObjectOfType<OrbitBreakerPlayerMotor>();
            var go=new GameObject("AbyssOctopusBoss");go.transform.position=player.Position+new Vector3(0,8,0);
            Boss=go.AddComponent<OrbitBreakerOctopusBoss>();Boss.Initialize(player,manager);
        }
    }
}
