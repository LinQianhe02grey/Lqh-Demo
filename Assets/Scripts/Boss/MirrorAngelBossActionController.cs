using UnityEngine;
using MirrorSaintessBossPack;

namespace Cardwin.Boss
{
    public enum MirrorAngelActionType
    {
        None = 0,
        TripleBeam = 1,
        GroundRay = 2,
        DoubleSlash = 3,
        DoubleSlashDash = 4,
        FarDashApproach = 5,
        AirLaserMode = 6
    }

    /// <summary>
    /// Stage 49 — unified action lock for all boss skills. Only ONE skill can
    /// be active at a time (except Death which bypasses via ForceCancel). Uses
    /// a monotonically increasing token so stale coroutine finally-blocks never
    /// accidentally cancel a newer action.
    ///
    /// This controller owns:
    ///   IsActionLocked / CurrentAction / token
    ///   Animator IsCasting + AttackType params
    ///   Mover movement lock
    ///   FacingController lock
    ///
    /// All skill scripts MUST call BeginAction / EndAction instead of writing
    /// those values directly.
    /// </summary>
    [RequireComponent(typeof(MirrorSaintessBoss))]
    public sealed class MirrorAngelBossActionController : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private MirrorSaintessBoss boss;
        [SerializeField] private MirrorAngelBossGravityMover mover;
        [SerializeField] private MirrorAngelFacingController facing;
        [SerializeField] private MirrorAngelBossAnimatorBridge animBridge;

        private int _actionToken;
        private bool _skillMotionAllowed;

        public bool IsActionLocked { get; private set; }
        public MirrorAngelActionType CurrentAction { get; private set; }
        public int CurrentToken => _actionToken;
        public bool IsSkillMotionAllowed => _skillMotionAllowed;

        private void Awake()
        {
            if (boss == null) boss = GetComponent<MirrorSaintessBoss>();
            if (mover == null) mover = GetComponent<MirrorAngelBossGravityMover>();
            if (facing == null) facing = GetComponent<MirrorAngelFacingController>();
            if (animBridge == null) animBridge = GetComponent<MirrorAngelBossAnimatorBridge>();
        }

        public int BeginAction(MirrorAngelActionType actionType)
        {
            if (IsActionLocked)
                return -1;
            if (boss != null && boss.IsDead)
                return -1;

            IsActionLocked = true;
            CurrentAction = actionType;
            _actionToken++;
            _skillMotionAllowed = false;

            var anim = animBridge != null ? animBridge.Animator : null;
            if (anim != null)
            {
                anim.SetFloat("MoveSpeed", 0f);
                anim.SetBool("IsCasting", true);
                anim.SetInteger("AttackType", (int)actionType);
            }

            if (mover != null)
            {
                mover.SetMovementLocked(true);
                mover.SetCasting(true);
            }

            if (facing != null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    facing.LockFacing(facing.GetFacingToTarget(player.transform));
                else
                    facing.LockFacing(facing.CurrentFacingSign);
            }

            return _actionToken;
        }

        public bool EndAction(int token)
        {
            if (token != _actionToken)
                return false;

            IsActionLocked = false;
            CurrentAction = MirrorAngelActionType.None;
            _skillMotionAllowed = false;

            var anim = animBridge != null ? animBridge.Animator : null;
            if (anim != null)
            {
                anim.SetFloat("MoveSpeed", 0f);
                anim.SetBool("IsCasting", false);
                anim.SetInteger("AttackType", 0);
            }

            if (mover != null)
            {
                mover.SetCasting(false);
                mover.SetMovementLocked(false);
                mover.ClearExternalVelocity();
            }

            if (facing != null)
                facing.UnlockFacing();

            return true;
        }

        public void ForceCancelAction()
        {
            IsActionLocked = false;
            CurrentAction = MirrorAngelActionType.None;
            _actionToken++;
            _skillMotionAllowed = false;

            var anim = animBridge != null ? animBridge.Animator : null;
            if (anim != null)
            {
                anim.SetFloat("MoveSpeed", 0f);
                anim.SetBool("IsCasting", false);
                anim.SetInteger("AttackType", 0);
            }

            if (mover != null)
            {
                mover.SetCasting(false);
                mover.SetMovementLocked(false);
                mover.ClearExternalVelocity();
            }

            if (facing != null)
                facing.UnlockFacing();
        }

        public void AllowSkillMotion(bool allow)
        {
            _skillMotionAllowed = allow;
        }
    }
}
