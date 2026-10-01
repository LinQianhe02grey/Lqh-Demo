using UnityEngine;
namespace OrbitBreaker
{
    public sealed class OrbitBreakerShieldPickup : MonoBehaviour
    {
        private float expires;
        private OrbitBreakerPlayerMotor player;
        private Material material;
        private bool consumed;
        public static OrbitBreakerShieldPickup Spawn(Vector3 position,OrbitBreakerPlayerMotor owner)
        {
            if(FindObjectsOfType<OrbitBreakerShieldPickup>().Length>=12)return null;
            var go=new GameObject("OrbitBreakerShieldPickup");go.transform.position=position;
            var pickup=go.AddComponent<OrbitBreakerShieldPickup>();pickup.player=owner;pickup.expires=Time.time+20;
            var c=go.AddComponent<SphereCollider>();c.radius=.55f;c.isTrigger=true;
            pickup.material=new Material(Shader.Find("Sprites/Default")){color=new Color(.2f,1,.55f),hideFlags=HideFlags.DontSave};
            for(int i=0;i<2;i++)
            {
                var bar=GameObject.CreatePrimitive(PrimitiveType.Cube);bar.name="ShieldPlus";bar.transform.SetParent(go.transform,false);
                bar.transform.localScale=i==0?new Vector3(.7f,.18f,.15f):new Vector3(.18f,.7f,.15f);
                bar.GetComponent<Collider>().enabled=false;Destroy(bar.GetComponent<Collider>());bar.GetComponent<Renderer>().sharedMaterial=pickup.material;
            }
            return pickup;
        }
        private void Update()
        {
            transform.localScale=Vector3.one*(1+Mathf.Sin(Time.time*5)*.08f);
            if(player==null || Time.time>expires || (player.Position-transform.position).sqrMagnitude>2500)Destroy(gameObject);
        }
        private void OnTriggerEnter(Collider c)=>Collect(c);
        private void OnTriggerStay(Collider c)=>Collect(c);
        private void Collect(Collider c)
        {
            if(consumed || player==null || c.attachedRigidbody!=player.GetComponent<Rigidbody>() || !player.TryRestoreShell())return;
            consumed=true;
            GetComponent<Collider>().enabled=false;enabled=false;OrbitBreakerCombatAudio.Play(CombatSound.Shield);Destroy(gameObject);
        }
        private void OnDestroy(){if(material!=null)Destroy(material);}
    }
}
