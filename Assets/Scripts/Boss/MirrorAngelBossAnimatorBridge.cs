using UnityEngine;
using MirrorSaintessBossPack;

namespace Cardwin.Boss
{
    /// <summary>
    /// Pure visual bridge (Stage 40): reads the Mirror Angel boss + gravity mover state
    /// and writes Animator parameters so the controller shows Idle / Walk / Dash / Fly /
    /// CastMirror / Death. Derives all values from already-public boss members
    /// (IsDead / CanMove) and mover read-only state. Does NOT contain combat logic and
    /// does NOT modify any boss/player/projectile script.
    /// </summary>
    [RequireComponent(typeof(MirrorSaintessBoss))]
    public sealed class MirrorAngelBossAnimatorBridge : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private MirrorSaintessBoss boss;
        [SerializeField] private MirrorAngelBossGravityMover mover;
        [SerializeField] private MirrorAngelBossActionController actionController;

        private static readonly int PMoveSpeed = Animator.StringToHash("MoveSpeed");
        private static readonly int PIsGrounded = Animator.StringToHash("IsGrounded");
        private static readonly int PIsFlying = Animator.StringToHash("IsFlying");
        private static readonly int PIsDashing = Animator.StringToHash("IsDashing");
        private static readonly int PIsCasting = Animator.StringToHash("IsCasting");
        private static readonly int PIsDead = Animator.StringToHash("IsDead");

        public Animator Animator => animator;

        private void Awake()
        {
            if (boss == null) boss = GetComponent<MirrorSaintessBoss>();
            if (animator == null) animator = GetComponent<Animator>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (mover == null) mover = GetComponent<MirrorAngelBossGravityMover>();
            if (actionController == null) actionController = GetComponent<MirrorAngelBossActionController>();
        }

        private void Update()
        {
            if (animator == null || boss == null)
                return;

            bool dead = boss.IsDead;

            // Death always wins — write it and stop
            if (dead)
            {
                animator.SetBool(PIsDead, true);
                animator.SetBool(PIsCasting, false);
                animator.SetInteger("AttackType", 0);
                return;
            }

            // Stage 49: action lock — don't overwrite skill animation with Idle/Walk
            if (actionController != null && actionController.IsActionLocked)
                return;

            // Normal locomotion animation
            bool flying = mover != null && mover.IsFlying;
            bool dashing = mover != null && mover.IsDashing;
            bool casting = !dead && ((mover != null && mover.IsCasting) || !boss.CanMove);
            bool grounded = mover == null || mover.IsGrounded;
            float moveSpeed = mover != null ? mover.CurrentMoveSpeed
                                            : (rb != null ? Mathf.Abs(rb.velocity.x) : 0f);

            animator.SetFloat(PMoveSpeed, moveSpeed);
            animator.SetBool(PIsGrounded, grounded);
            animator.SetBool(PIsFlying, flying && !dead);
            animator.SetBool(PIsDashing, dashing && !dead);
            animator.SetBool(PIsCasting, casting);
            animator.SetBool(PIsDead, false);
        }
    }
}
