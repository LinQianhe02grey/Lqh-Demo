using System.Collections.Generic;
using UnityEngine;
namespace OrbitBreaker
{
    public sealed class OrbitBreakerSlimeAura : MonoBehaviour
    {
        public const float Radius=3.4f, SpeedFactor=.65f;
        private static readonly HashSet<OrbitBreakerSlimeAura> active=new HashSet<OrbitBreakerSlimeAura>();
        private OrbitBreakerEnemyController enemy;
        private Transform blob;
        private Material material,ringMaterial;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry()=>active.Clear();
        public static bool Contains(Vector3 position)
        {
            foreach(var a in active)if(a!=null && a.enemy!=null && !a.enemy.IsDead && (a.transform.position-position).sqrMagnitude<Radius*Radius)return true;
            return false;
        }
        private void Awake()
        {
            enemy=GetComponent<OrbitBreakerEnemyController>();GetComponent<Renderer>().enabled=false;
            material=new Material(Shader.Find("Standard")){color=new Color(.025f,.018f,.04f),hideFlags=HideFlags.DontSave};
            material.SetFloat("_Glossiness",.9f);material.EnableKeyword("_EMISSION");material.SetColor("_EmissionColor",new Color(.065f,.015f,.10f));
            var go=GameObject.CreatePrimitive(PrimitiveType.Sphere);go.name="ViscousBody";go.transform.SetParent(transform,false);
            go.GetComponent<Collider>().enabled=false;Destroy(go.GetComponent<Collider>());go.GetComponent<Renderer>().sharedMaterial=material;blob=go.transform;
            ringMaterial=new Material(Shader.Find("Sprites/Default")){hideFlags=HideFlags.DontSave};
            var ring=new GameObject("SlowingField");ring.transform.SetParent(transform,false);var l=ring.AddComponent<LineRenderer>();
            l.sharedMaterial=ringMaterial;l.useWorldSpace=false;l.loop=true;l.positionCount=64;l.startWidth=l.endWidth=.035f;
            l.startColor=l.endColor=new Color(.52f,.22f,.72f,.36f);
            float r=Radius/transform.lossyScale.x;
            for(int i=0;i<64;i++){float a=i*Mathf.PI*2/64;l.SetPosition(i,new Vector3(Mathf.Cos(a)*r,Mathf.Sin(a)*r,.3f));}
        }
        private void OnEnable()=>active.Add(this);
        private void OnDisable()=>active.Remove(this);
        private void Update()
        {
            float wave=Mathf.Sin(Time.time*4+transform.position.x)*.12f;
            if(blob!=null)blob.localScale=new Vector3(1.35f+wave,1.15f-wave,.9f);
            if(material!=null)material.color=enemy.IsRepelled?new Color(.3f,.7f,.55f):new Color(.025f,.018f,.04f);
        }
        private void OnDestroy(){if(material!=null)Destroy(material);if(ringMaterial!=null)Destroy(ringMaterial);}
    }
}
