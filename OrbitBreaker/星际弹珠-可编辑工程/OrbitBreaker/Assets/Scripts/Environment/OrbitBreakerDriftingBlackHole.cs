using UnityEngine;
namespace OrbitBreaker
{
    [RequireComponent(typeof(SphereCollider))]
    public sealed class OrbitBreakerDriftingBlackHole : MonoBehaviour
    {
        public const float CoreRadius=1f;
        public bool Spent { get; private set; }
        public bool ContainsCore(Vector3 position)
        {
            Vector3 local=transform.InverseTransformPoint(position);
            return new Vector2(local.x,local.y).sqrMagnitude<=CoreRadius*CoreRadius;
        }
        private void OnTriggerEnter(Collider other) => Touch(other);
        private void OnTriggerStay(Collider other) => Touch(other);
        private void Touch(Collider other)
        {
            var player=other.attachedRigidbody==null?null:other.attachedRigidbody.GetComponent<OrbitBreakerPlayerMotor>();
            if(!Spent && player!=null && ContainsCore(player.Position) && player.TryReceiveBlackHole(transform.position))
            {Spent=true;OrbitBreakerSpentProp.Consume(gameObject);}
        }
    }
}
