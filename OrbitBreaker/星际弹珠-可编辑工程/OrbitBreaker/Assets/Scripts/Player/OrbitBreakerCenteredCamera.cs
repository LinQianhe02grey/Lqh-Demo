using UnityEngine;
namespace OrbitBreaker
{
    [DefaultExecutionOrder(10000), RequireComponent(typeof(Camera))]
    public sealed class OrbitBreakerCenteredCamera : MonoBehaviour
    {
        public Transform target;
        public Transform background;
        public Vector3 offset=new Vector3(0,-25,-20);
        private void LateUpdate() => Follow();
        public void Follow()
        {
            if(target==null)return;
            // Anchor at the pillar throughout an orbit; following the ball would orbit the whole view.
            var motor=target.GetComponent<OrbitBreakerPlayerMotor>();
            Vector3 center=motor!=null && motor.CurrentAttachment!=null?(Vector3)motor.CurrentAttachment.Center:target.position;
            transform.position=center+offset;
            transform.rotation=Quaternion.LookRotation(-offset,Vector3.up);
            if(background!=null)background.position=new Vector3(center.x,center.y,7);
        }
    }
}
