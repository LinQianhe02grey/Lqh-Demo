using UnityEngine;
namespace OrbitBreaker
{
    [DisallowMultipleComponent]
    public sealed class OrbitBreakerTargetLock : MonoBehaviour
    {
        private OrbitBreakerPlayerMotor player;
        private Transform target;
        private LineRenderer ring;
        private Material material;
        public Transform Target=>HasTarget?target:null;
        public bool HasTarget=>target!=null && target.gameObject.activeInHierarchy &&
            (target.GetComponent<OrbitBreakerEnemyController>()==null || !target.GetComponent<OrbitBreakerEnemyController>().IsDead) &&
            (target.GetComponent<OrbitBreakerOctopusBoss>()==null || target.GetComponent<OrbitBreakerOctopusBoss>().Health>0);
        private void Awake()
        {
            player=GetComponent<OrbitBreakerPlayerMotor>();
            var go=new GameObject("SelectedTarget");go.transform.SetParent(transform,false);
            ring=go.AddComponent<LineRenderer>();ring.useWorldSpace=true;ring.loop=true;ring.positionCount=48;ring.widthMultiplier=.05f;
            material=new Material(Shader.Find("Sprites/Default"));ring.sharedMaterial=material;
            ring.startColor=ring.endColor=new Color(1,.85f,.2f);ring.enabled=false;
        }
        public bool Select(Transform candidate)
        {
            if(!player.OrbitBreakerGameManager.IsPlaying || candidate==null)return false;
            var enemy=candidate.GetComponentInParent<OrbitBreakerEnemyController>();
            var boss=candidate.GetComponentInParent<OrbitBreakerOctopusBoss>();
            var pin=candidate.GetComponentInParent<OrbitBreakerBumper>();
            var magnet=candidate.GetComponentInParent<OrbitBreakerAttachmentPoint>();
            target=enemy!=null?enemy.transform:boss!=null?boss.transform:pin!=null?pin.transform:magnet!=null?magnet.transform:null;
            return HasTarget;
        }
        public void Consume(){target=null;}
        public bool SelectAtScreen(Vector2 point)
        {
            if(Camera.main==null || !player.OrbitBreakerGameManager.IsPlaying || OrbitBreakerModeMenu.IsPointerOverUI(point))return false;
            var ray=Camera.main.ScreenPointToRay(point);var hits=Physics.RaycastAll(ray,200,~0,QueryTriggerInteraction.Collide);
            System.Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
            foreach(var hit in hits)if(Select(hit.transform))return true;
            target=null;return false;
        }
        private void LateUpdate()
        {
            if(!player.OrbitBreakerGameManager.IsPlaying || !HasTarget){target=null;ring.enabled=false;return;}
            ring.enabled=true;float radius=target.GetComponent<OrbitBreakerOctopusBoss>()!=null?2.8f:.8f;
            for(int i=0;i<48;i++){float a=i*Mathf.PI*2/48;ring.SetPosition(i,target.position+new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,-.5f));}
        }
        private void OnDestroy(){if(material!=null)Destroy(material);}
    }
}
