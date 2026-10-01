using UnityEngine;
namespace OrbitBreaker
{
    public sealed class OrbitBreakerPlanetCombatVisual : MonoBehaviour
    {
        private OrbitBreakerPlayerMotor motor;
        private LineRenderer shield;
        private LineRenderer aim;
        private readonly LineRenderer[] shells=new LineRenderer[3];
        private float shellFlash;
        private void OnShellBroken() { shellFlash=Time.time+.45f; }
        private TrailRenderer trail;
        private Material material;
        private void Awake()
        {
            motor=GetComponent<OrbitBreakerPlayerMotor>();material=new Material(Shader.Find("Sprites/Default")){hideFlags=HideFlags.DontSave};
            var ring=new GameObject("PlanetShield");ring.transform.SetParent(transform,false);
            shield=ring.AddComponent<LineRenderer>();shield.useWorldSpace=false;shield.loop=true;shield.positionCount=48;shield.sharedMaterial=material;
            shield.startWidth=shield.endWidth=.035f;
            for(int i=0;i<48;i++){float a=i*Mathf.PI*2/48;shield.SetPosition(i,new Vector3(Mathf.Cos(a)*.66f,Mathf.Sin(a)*.66f,-.1f));}
            for(int j=0;j<3;j++)
            {
                var shell=new GameObject("ProtectionShell"+j);shell.transform.SetParent(transform,false);
                var l=shell.AddComponent<LineRenderer>();shells[j]=l;l.useWorldSpace=false;l.loop=true;l.positionCount=64;l.sharedMaterial=material;l.startWidth=l.endWidth=.018f;
                for(int i=0;i<64;i++){float a=i*Mathf.PI*2/64;float r=.59f+j*.11f;l.SetPosition(i,new Vector3(Mathf.Cos(a)*r,Mathf.Sin(a)*r,-.12f));}
            }
            motor.ShellBroken+=OnShellBroken;
            var tail=new GameObject("LaunchComet");tail.transform.SetParent(transform,false);trail=tail.AddComponent<TrailRenderer>();
            trail.sharedMaterial=material;trail.time=.3f;trail.startWidth=.38f;trail.endWidth=0;trail.minVertexDistance=.08f;
            trail.endColor=Color.clear;trail.emitting=false;trail.enabled=false;
            var arrow=new GameObject("MouseDashAim");arrow.transform.SetParent(transform,false);aim=arrow.AddComponent<LineRenderer>();
            aim.sharedMaterial=material;aim.useWorldSpace=true;aim.positionCount=5;aim.startWidth=aim.endWidth=.04f;
            aim.startColor=aim.endColor=new Color(.4f,1,1,.65f);
        }
        private void LateUpdate()
        {
            for(int i=0;i<3;i++)
            {
                shells[i].enabled=i<motor.Shells;
                shells[i].startColor=shells[i].endColor=Time.time<shellFlash?new Color(1,.35f,.2f,.9f):new Color(.25f,.8f,1,.32f+i*.15f);
            }
            bool active=motor.IsInvulnerable;
            shield.enabled=active||motor.HasRewardBoost;shield.startColor=shield.endColor=motor.HasRewardBoost?new Color(1,.12f,.06f):Color.Lerp(Color.cyan,new Color(1,.65f,.15f),motor.ComboCount/8f);
            bool flying=motor.IsAttackActive||motor.HasRewardBoost;
            trail.enabled=flying;trail.emitting=flying;trail.startColor=shield.startColor;
            if(!flying)trail.Clear();
            aim.enabled=motor.CanDash;
            if(aim.enabled)
            {
                Vector3 dir=((Vector3)motor.AttachmentAim-motor.Position).normalized,side=new Vector3(-dir.y,dir.x,0);
                Vector3 origin=motor.Position+Vector3.back*.3f,tip=origin+dir*1.9f;
                aim.SetPosition(0,origin+dir*.8f);aim.SetPosition(1,tip);aim.SetPosition(2,tip-dir*.3f+side*.16f);aim.SetPosition(3,tip);aim.SetPosition(4,tip-dir*.3f-side*.16f);
            }
        }
        private void OnDisable(){if(aim!=null)aim.enabled=false;if(shield!=null)shield.enabled=false;if(trail!=null){trail.emitting=false;trail.enabled=false;trail.Clear();}}
        private void OnDestroy(){if(motor!=null)motor.ShellBroken-=OnShellBroken;if(material!=null)Destroy(material);}
    }
}
