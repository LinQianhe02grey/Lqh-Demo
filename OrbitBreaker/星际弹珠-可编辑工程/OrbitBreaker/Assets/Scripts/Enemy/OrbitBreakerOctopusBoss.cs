using System.Collections.Generic;
using UnityEngine;
namespace OrbitBreaker
{
    [RequireComponent(typeof(Rigidbody),typeof(SphereCollider))]
    public sealed class OrbitBreakerOctopusBoss : MonoBehaviour
    {
        public const float Radius=2.1f, HitCooldown=.45f, LaserWarning=1.1f, LaserDuration=.8f, LaserRange=24;
        public const float TentacleWarning=1.2f, TentacleDuration=1.1f, TentacleRadius=1.1f;
        public OrbitBreakerPlayerMotor Player {get;private set;}
        public OrbitBreakerGameManager Manager {get;private set;}
        public int Health {get;private set;}
        public int HealthCapacity {get;private set;}
        public int DamageEvents {get;private set;}
        public int DestroyedProps {get;private set;}
        public int LaserCasts {get;private set;}
        public int TentacleCasts {get;private set;}
        public bool LaserActive=>laserStart>0 && Time.time>=laserStart+LaserWarning && Time.time<laserStart+LaserWarning+LaserDuration;
        public bool LaserTelegraph=>laserStart>0 && Time.time<laserStart+LaserWarning;
        private Rigidbody body;
        private Material skin,eye,ink,warningMat;
        private readonly List<Transform> limbs=new List<Transform>();
        private readonly List<GameObject> hazards=new List<GameObject>();
        private readonly Collider[] overlaps=new Collider[128];
        private LineRenderer laser;
        private Vector3 laserOrigin,laserEnd;
        private float nextAttack, nextHit, laserStart, nextLaserHit;
        private bool summonNext=true;
        public void Initialize(OrbitBreakerPlayerMotor player,OrbitBreakerGameManager manager)
        {
            Player=player;Manager=manager;Health=HealthCapacity=manager.Rules.bossHealth;
            body=GetComponent<Rigidbody>();body.isKinematic=true;body.useGravity=false;
            var collider=GetComponent<SphereCollider>();collider.radius=Radius;collider.isTrigger=true;
            skin=Mat(new Color(.24f,.08f,.43f),new Color(.22f,.035f,.35f));
            eye=Mat(new Color(1,.57f,.18f),new Color(1,.3f,.04f));
            ink=Mat(new Color(.025f,.01f,.06f),Color.black);
            warningMat=new Material(Shader.Find("Sprites/Default"));
            Part("Mantle",new Vector3(0,.3f,-.5f),new Vector3(3.7f,4.3f,2.8f),skin);
            Part("LeftEye",new Vector3(-.75f,-.35f,-1.8f),new Vector3(.7f,.85f,.3f),eye);
            Part("RightEye",new Vector3(.75f,-.35f,-1.8f),new Vector3(.7f,.85f,.3f),eye);
            Part("LeftPupil",new Vector3(-.75f,-.4f,-2f),new Vector3(.17f,.52f,.12f),ink);
            Part("RightPupil",new Vector3(.75f,-.4f,-2f),new Vector3(.17f,.52f,.12f),ink);
            for(int i=0;i<8;i++)for(int j=0;j<5;j++)
            {
                var t=Part($"Arm_{i}_{j}",Vector3.zero,Vector3.one*(.72f-j*.105f),skin);limbs.Add(t);
            }
            var beam=new GameObject("LaserTelegraph");beam.transform.SetParent(transform,false);laser=beam.AddComponent<LineRenderer>();
            laser.sharedMaterial=warningMat;laser.positionCount=2;laser.useWorldSpace=true;laser.enabled=false;
            nextAttack=Time.time+manager.Rules.bossAttackInterval;
        }
        private Material Mat(Color color,Color emission){var m=new Material(Shader.Find("Standard"));m.color=color;m.SetFloat("_Glossiness",.65f);m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",emission);return m;}
        private Transform Part(string name,Vector3 pos,Vector3 scale,Material mat)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Sphere);go.name=name;go.transform.SetParent(transform,false);go.transform.localPosition=pos;go.transform.localScale=scale;
            var col=go.GetComponent<Collider>();col.enabled=false;Destroy(col);go.GetComponent<Renderer>().sharedMaterial=mat;return go.transform;
        }
        private void FixedUpdate()
        {
            if(Manager==null || !Manager.IsPlaying || Health<=0)return;
            Vector2 delta=Player.Position-body.position;
            Vector3 destination=body.position+(Vector3)(delta.normalized*Manager.Rules.bossMoveSpeed*Time.fixedDeltaTime);
            body.MovePosition(destination);
            int count=Physics.OverlapCapsuleNonAlloc(body.position,destination,Radius+.3f,overlaps,~0,QueryTriggerInteraction.Collide);
            for(int i=0;i<count;i++)
            {
                var hit=overlaps[i];
                if(hit.GetComponentInParent<OrbitBreakerProceduralArena>()==null)continue;
                Component prop=hit.GetComponentInParent<OrbitBreakerBumper>();
                if(prop==null)prop=hit.GetComponentInParent<OrbitBreakerFloatingBoard>();
                if(prop==null)prop=hit.GetComponentInParent<OrbitBreakerCosmicMechanism>();
                if(prop==null)prop=hit.GetComponentInParent<OrbitBreakerAttachmentPoint>();
                if(prop!=null && prop.gameObject.activeSelf){prop.GetComponentInParent<OrbitBreakerProceduralArena>().DestroyProp(prop.gameObject);DestroyedProps++;}
            }
            if(LaserActive && Time.time>=nextLaserHit && DistanceToSegment(Player.Position,laserOrigin,laserEnd)<.75f)
            {if(Player.TryReceiveHit())nextLaserHit=Time.time+1.2f;}
        }
        private void Update()
        {
            if(Manager==null)return;
            if(!Manager.IsPlaying){if(laser!=null)laser.enabled=false;return;}
            for(int i=0;i<limbs.Count;i++)
            {
                int arm=i/5,j=i%5;float a=arm*Mathf.PI/4+Mathf.Sin(Time.time*2.4f+j*.6f+arm)*(.06f+j*.035f);
                float r=1.7f+j*.44f;limbs[i].localPosition=new Vector3(Mathf.Cos(a)*r,Mathf.Sin(a)*r,-.3f-Mathf.Sin(Time.time*2+j)*.2f);
            }
            if(Time.time>=nextAttack){if(summonNext)SummonTentacles();else FireLaser();summonNext=!summonNext;nextAttack=Time.time+Manager.Rules.bossAttackInterval;}
            laser.enabled=LaserTelegraph||LaserActive;
            if(laser.enabled){laser.SetPosition(0,laserOrigin+Vector3.back*.8f);laser.SetPosition(1,laserEnd+Vector3.back*.8f);laser.widthMultiplier=LaserActive?.7f:.08f;laser.startColor=laser.endColor=LaserActive?new Color(1,.15f,.25f):new Color(1,.7f,.2f,.8f);}
        }
        public void FireLaser()
        {
            if(Manager==null || !Manager.IsPlaying)return;
            laserStart=Time.time;laserOrigin=body.position;
            Vector2 direction=Player.Position-body.position;if(direction.sqrMagnitude<.01f)direction=Vector2.down;
            laserEnd=laserOrigin+(Vector3)direction.normalized*LaserRange;LaserCasts++;
        }
        public void SummonTentacles()
        {
            if(Manager==null || !Manager.IsPlaying)return;
            TentacleCasts++;hazards.RemoveAll(h=>h==null);
            for(int i=0;i<3;i++)
            {
                var go=new GameObject("SummonedTentacle");float a=i*Mathf.PI*2/3;
                go.transform.position=Player.Position+new Vector3(Mathf.Cos(a),Mathf.Sin(a),0)*(i==0?0:2.3f);
                go.AddComponent<OrbitBreakerBossTentacle>().Initialize(this,skin,warningMat);hazards.Add(go);
            }
        }
        private void OnTriggerEnter(Collider other)=>Contact(other);
        private void OnTriggerStay(Collider other)=>Contact(other);
        private void Contact(Collider other)
        {
            if(Player==null || other.attachedRigidbody!=Player.GetComponent<Rigidbody>())return;
            if(!TryPlayerImpact(Player) && Time.time>=nextHit && !Player.IsAttackActive)Player.TryReceiveHit();
        }
        public bool TryPlayerImpact(OrbitBreakerPlayerMotor player)
        {
            if(player!=Player || Manager==null || !Manager.IsPlaying || Health<=0 || Time.time<nextHit)return false;
            // Trigger bodies do not change velocity. Actual current speed is authoritative here.
            Vector2 incoming=player.GetComponent<Rigidbody>().velocity;int damage=player.DamageForSpeed(incoming.magnitude);
            if(damage<=0)return false;
            nextHit=Time.time+HitCooldown;Health=Mathf.Max(0,Health-damage);DamageEvents++;
            player.TryDashBurst(player.Position,damage);
            player.QueueBossRebound(incoming);OrbitBreakerCombatAudio.Play(CombatSound.Hit);
            if(Health==0){OrbitBreakerCombatAudio.Play(CombatSound.Kill);Manager.WinGame();}
            return true;
        }
        public bool TryExplosionDamage(int damage)
        {
            if(Manager==null || !Manager.IsPlaying || Health<=0 || damage<=0)return false;
            Health=Mathf.Max(0,Health-damage);
            if(Health==0){OrbitBreakerCombatAudio.Play(CombatSound.Kill);Manager.WinGame();}
            return true;
        }
        public static float DistanceToSegment(Vector2 point,Vector2 a,Vector2 b)
        {var d=b-a;float t=d.sqrMagnitude<.001f?0:Mathf.Clamp01(Vector2.Dot(point-a,d)/d.sqrMagnitude);return Vector2.Distance(point,a+d*t);}
        private void OnDestroy()
        {
            foreach(var h in hazards)if(h!=null)Destroy(h);
            foreach(var m in new[]{skin,eye,ink,warningMat})if(m!=null)Destroy(m);
        }
    }
}
