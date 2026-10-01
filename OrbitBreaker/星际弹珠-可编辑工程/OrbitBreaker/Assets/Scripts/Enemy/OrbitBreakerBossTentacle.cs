using UnityEngine;
namespace OrbitBreaker
{
    public sealed class OrbitBreakerBossTentacle : MonoBehaviour
    {
        private OrbitBreakerOctopusBoss boss;
        private float start,nextHit;
        private LineRenderer ring;
        private Transform tendril;
        public bool IsActive=>Time.time>=start+OrbitBreakerOctopusBoss.TentacleWarning;
        public void Initialize(OrbitBreakerOctopusBoss source,Material skin,Material warning)
        {
            boss=source;start=Time.time;
            ring=gameObject.AddComponent<LineRenderer>();ring.sharedMaterial=warning;ring.loop=true;ring.useWorldSpace=false;ring.positionCount=48;ring.widthMultiplier=.08f;
            for(int i=0;i<48;i++){float a=i*Mathf.PI*2/48;ring.SetPosition(i,new Vector3(Mathf.Cos(a),Mathf.Sin(a),-.5f)*OrbitBreakerOctopusBoss.TentacleRadius);}
            var go=GameObject.CreatePrimitive(PrimitiveType.Capsule);go.name="RisingTentacle";go.transform.SetParent(transform,false);go.transform.localRotation=Quaternion.Euler(90,0,0);
            var col=go.GetComponent<Collider>();col.enabled=false;Destroy(col);go.GetComponent<Renderer>().sharedMaterial=skin;tendril=go.transform;
        }
        private void Update()
        {
            if(boss==null || !boss.Manager.IsPlaying || Time.time>start+OrbitBreakerOctopusBoss.TentacleWarning+OrbitBreakerOctopusBoss.TentacleDuration){Destroy(gameObject);return;}
            ring.startColor=ring.endColor=IsActive?new Color(1,.15f,.3f):new Color(1,.65f,.2f,.8f);
            tendril.gameObject.SetActive(IsActive);
            if(IsActive){tendril.localScale=new Vector3(.65f,1.6f,.65f);tendril.localPosition=Vector3.back*1.1f;}
        }
        private void FixedUpdate()
        {
            if(boss==null || !boss.Manager.IsPlaying || !IsActive || Time.time<nextHit)return;
            if(Vector2.Distance(transform.position,boss.Player.Position)<OrbitBreakerOctopusBoss.TentacleRadius+.4f && boss.Player.TryReceiveHit())nextHit=Time.time+1.2f;
        }
    }
}
