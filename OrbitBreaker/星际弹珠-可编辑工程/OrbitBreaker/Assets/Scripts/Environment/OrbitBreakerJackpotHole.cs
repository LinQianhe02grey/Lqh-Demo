using UnityEngine;

namespace OrbitBreaker
{
    [RequireComponent(typeof(SphereCollider))]
    public sealed class OrbitBreakerJackpotHole : MonoBehaviour
    {
        public const int BasePoints = 600;
        public const float Cooldown = 4f;
        private OrbitBreakerScoreManager score;
        private float readyAt;
        private int interaction;
        private OrbitBreakerPlayerMotor occupant;
        public float CooldownRemaining => Mathf.Max(0f, readyAt - Time.time);
        public int Captures { get; private set; }
        public bool Spent { get; private set; }
        private Material rewardMaterial;
        private void Awake()
        {
            score=FindObjectOfType<OrbitBreakerScoreManager>();GetComponent<SphereCollider>().isTrigger=true;
            rewardMaterial=new Material(Shader.Find("Sprites/Default")){hideFlags=HideFlags.DontSave};
            var go=new GameObject("RewardRedHalo");go.transform.SetParent(transform,false);
            var l=go.AddComponent<LineRenderer>();l.sharedMaterial=rewardMaterial;l.useWorldSpace=false;l.loop=true;l.positionCount=64;l.startWidth=l.endWidth=.075f;
            l.startColor=l.endColor=new Color(1,.18f,.1f,.9f);
            for(int i=0;i<64;i++){float a=i*Mathf.PI*2/64;l.SetPosition(i,new Vector3(Mathf.Cos(a),Mathf.Sin(a),-.3f)*.92f);}
            var star=new GameObject("RewardStar");star.transform.SetParent(transform,false);
            var s=star.AddComponent<LineRenderer>();s.sharedMaterial=rewardMaterial;s.useWorldSpace=false;s.loop=true;s.positionCount=8;s.startWidth=s.endWidth=.055f;
            s.startColor=s.endColor=new Color(1,.9f,.35f);
            for(int i=0;i<8;i++){float a=i*Mathf.PI/4;float r=i%2==0?.38f:.13f;s.SetPosition(i,new Vector3(Mathf.Cos(a)*r,Mathf.Sin(a)*r,-.65f));}
        }
        private void OnDestroy(){if(rewardMaterial!=null)Destroy(rewardMaterial);}
        private void Update()
        {
            // Must physically leave the well before another capture. Works even after a disabled collider.
            if (occupant != null && Vector2.Distance(occupant.Position, transform.position) > 1.8f) occupant = null;
        }
        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponentInParent<OrbitBreakerPlayerMotor>();
            if (Spent || player == null || occupant != null || Time.time < readyAt || score == null || !score.OrbitBreakerGameManager.IsPlaying) return;
            if (!player.TryCaptureJackpot(transform.position)) return;
            player.GrantRewardBoost();
            occupant = player; readyAt = Time.time + Cooldown; Captures++;
            score.TryAward(BasePoints, this, ++interaction);
            Spent=true;OrbitBreakerSpentProp.Consume(gameObject);
        }
    }
}
