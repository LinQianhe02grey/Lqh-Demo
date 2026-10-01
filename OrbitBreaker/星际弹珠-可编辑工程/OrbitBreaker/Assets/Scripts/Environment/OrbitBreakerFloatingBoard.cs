using UnityEngine;

namespace OrbitBreaker
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class OrbitBreakerFloatingBoard : MonoBehaviour
    {
        public Vector2 travel = new Vector2(.8f,.3f);
        public float phase;
        private Rigidbody body;
        private Vector3 origin;
        private Quaternion rest;
        private OrbitBreakerGameManager manager;
        private float elapsed, nextHit;
        private void Awake()
        {
            body=GetComponent<Rigidbody>();body.isKinematic=true;body.useGravity=false;
            origin=body.position;rest=body.rotation;manager=FindObjectOfType<OrbitBreakerGameManager>();
        }
        private void FixedUpdate()
        {
            if(manager==null || !manager.IsPlaying)return;
            elapsed+=Time.fixedDeltaTime;
            body.MovePosition(origin+(Vector3)(travel*(Mathf.Sin(elapsed*1.7f+phase)-Mathf.Sin(phase))));
            body.MoveRotation(rest*Quaternion.Euler(0,0,Mathf.Sin(elapsed*1.5f)*22));
        }
        private void OnCollisionEnter(Collision hit)
        {
            if(Time.time<nextHit||hit.rigidbody==null)return;
            var player=hit.rigidbody.GetComponent<OrbitBreakerPlayerMotor>();if(player==null)return;
            Vector2 away=player.Position-transform.position;
            away=new Vector2(away.x*.4f,Mathf.Max(.7f,Mathf.Abs(away.y)));
            if(player.TryRequestBoardImpulse(away,20f))nextHit=Time.time+.18f;
        }
    }
}
