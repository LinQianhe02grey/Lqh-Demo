using UnityEngine;
namespace OrbitBreaker
{
    [DefaultExecutionOrder(10010)]
    public sealed class OrbitBreakerDashCapsules : MonoBehaviour
    {
        private OrbitBreakerPlayerMotor motor;
        private Transform row;
        private Renderer[] capsules;
        private Material empty,full;
        public int LitCount {get;private set;}
        private void Awake()
        {
            motor=GetComponent<OrbitBreakerPlayerMotor>();
            row=new GameObject("DashChargeCapsules").transform;
            empty=new Material(Shader.Find("Sprites/Default")){color=new Color(.10f,.20f,.27f,.8f),hideFlags=HideFlags.DontSave};
            full=new Material(Shader.Find("Sprites/Default")){color=new Color(.25f,1,1),hideFlags=HideFlags.DontSave};
            Rebuild();
        }
        private void Rebuild()
        {
            if(capsules!=null)foreach(var r in capsules)Destroy(r.gameObject);
            capsules=new Renderer[motor.DashCapacity];
            for(int i=0;i<capsules.Length;i++)
            {
                var go=GameObject.CreatePrimitive(PrimitiveType.Capsule);go.name="DashCharge"+i;go.transform.SetParent(row,false);
                go.GetComponent<Collider>().enabled=false;Destroy(go.GetComponent<Collider>());
                go.transform.localPosition=Vector3.right*((i-(capsules.Length-1)*.5f)*.22f);
                go.transform.localScale=new Vector3(.13f,.14f,.13f);capsules[i]=go.GetComponent<Renderer>();
            }
        }
        private void LateUpdate()
        {
            if(capsules.Length!=motor.DashCapacity)Rebuild();
            var camera=Camera.main;if(camera==null)return;
            row.position=transform.position+camera.transform.up*.88f-camera.transform.forward*.25f;row.rotation=camera.transform.rotation;
            LitCount=Mathf.Min(motor.DashCharges,capsules.Length);
            for(int i=0;i<capsules.Length;i++)capsules[i].sharedMaterial=i<LitCount?full:empty;
        }
        private void OnDisable(){if(row!=null)row.gameObject.SetActive(false);}
        private void OnEnable(){if(row!=null)row.gameObject.SetActive(true);}
        private void OnDestroy(){if(row!=null)Destroy(row.gameObject);if(empty!=null)Destroy(empty);if(full!=null)Destroy(full);}
    }
}
