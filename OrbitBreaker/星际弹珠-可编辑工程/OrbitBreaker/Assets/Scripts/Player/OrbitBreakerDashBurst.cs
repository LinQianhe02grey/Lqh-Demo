using UnityEngine;
namespace OrbitBreaker
{
    // Cosmetic world-space pulse: never changes time scale or any Rigidbody.
    public sealed class OrbitBreakerDashBurst : MonoBehaviour
    {
        public const float Radius=2.4f, DamageFactor=.4f, Duration=.32f;
        private Material material;
        private LineRenderer ring;
        private LineRenderer[] rays;
        private float started;
        public static void Spawn(Vector3 center)
        {var go=new GameObject("DashImpactBurst");go.transform.position=center+Vector3.back*.7f;go.AddComponent<OrbitBreakerDashBurst>();}
        private LineRenderer Line(string label,int count)
        {
            var go=new GameObject(label);go.transform.SetParent(transform,false);
            var l=go.AddComponent<LineRenderer>();l.sharedMaterial=material;l.useWorldSpace=false;l.positionCount=count;l.startWidth=l.endWidth=.09f;return l;
        }
        private void Awake()
        {
            started=Time.time;material=new Material(Shader.Find("Sprites/Default"));
            ring=Line("ShockRing",64);ring.loop=true;rays=new LineRenderer[12];
            for(int i=0;i<rays.Length;i++)rays[i]=Line("Spark",2);
            OrbitBreakerCombatAudio.Play(CombatSound.Burst);Draw(0);
        }
        private void Update(){float t=(Time.time-started)/Duration;if(t>=1){Destroy(gameObject);return;}Draw(t);}
        private void Draw(float t)
        {
            float radius=Mathf.Lerp(.15f,Radius,Mathf.Sqrt(t));
            Color c=Color.Lerp(new Color(1,1,.8f),new Color(1,.28f,.04f,0),t);
            ring.startColor=ring.endColor=c;ring.startWidth=ring.endWidth=Mathf.Lerp(.22f,.035f,t);
            for(int i=0;i<64;i++){float a=i*Mathf.PI*2/64;ring.SetPosition(i,new Vector3(Mathf.Cos(a),Mathf.Sin(a))*radius);}
            for(int i=0;i<rays.Length;i++)
            {float a=i*Mathf.PI*2/rays.Length;var dir=new Vector3(Mathf.Cos(a),Mathf.Sin(a));rays[i].startColor=rays[i].endColor=c;rays[i].SetPosition(0,dir*radius*.65f);rays[i].SetPosition(1,dir*radius);}
        }
        private void OnDestroy(){if(material!=null)Destroy(material);}
    }
}
