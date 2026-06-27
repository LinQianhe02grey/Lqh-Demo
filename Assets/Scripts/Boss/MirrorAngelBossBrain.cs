using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MirrorSaintessBossPack;
using Random = UnityEngine.Random;

namespace Cardwin.Boss
{
    public enum MirrorAngelBossBrainState
    {
        Idle,
        Approach,
        KeepDistance,
        Reposition,
        Windup,
        Casting,
        Recovery,
        Dead
    }

    [System.Serializable]
    public class MirrorAngelBossSkillOption
    {
        public string skillId = "MirrorTripleBeam";
        public float cooldown = 4.5f;
        public float lastUseTime = -999f;
        public float minRange = 4f;
        public float maxRange = 12f;
        public float baseWeight = 10f;
        public float repeatPenalty = 3f;
    }

    [RequireComponent(typeof(MirrorSaintessBoss))]
    public sealed class MirrorAngelBossBrain : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private MirrorSaintessBoss boss;
        [SerializeField] private MirrorAngelBossGravityMover mover;
        [SerializeField] private MirrorAngelFacingController facing;
        [SerializeField] private MirrorAngelTripleBeamSkill beamSkill;
        [SerializeField] private MirrorAngelGroundRaySkill groundRaySkill;
        [SerializeField] private MirrorAngelDoubleSlashSkill doubleSlashSkill;
        [SerializeField] private MirrorAngelDoubleSlashDashSkill doubleSlashDashSkill;
        [SerializeField] private MirrorAngelBossActionController actionController;

        [Header("State")]
        [SerializeField] private MirrorAngelBossBrainState currentState;

        [Header("Distance")]
        [SerializeField] private float tooCloseDistance = 2.5f;
        [SerializeField] private float preferredMinDistance = 4f;
        [SerializeField] private float preferredMaxDistance = 7f;
        [SerializeField] private float farDistance = 10f;

        [Header("Decision")]
        [SerializeField] private float decisionIntervalMin = 0.5f;
        [SerializeField] private float decisionIntervalMax = 1.2f;
        [SerializeField, Range(0f, 1f)] private float attackChance = 0.65f;

        [Header("Reposition")]
        [SerializeField] private float repositionDurationMin = 0.4f;
        [SerializeField] private float repositionDurationMax = 0.8f;
        [SerializeField] private float repositionSpeedMultiplier = 1.0f;

        [Header("Recovery")]
        [SerializeField] private float recoveryDuration = 0.5f;

        [Header("Skills")]
        [SerializeField] private List<MirrorAngelBossSkillOption> skills = new List<MirrorAngelBossSkillOption>();

        [Header("Far Dash Approach")]
        [SerializeField] private float farDashMinDistance = 8.5f;
        [SerializeField] private float farDashStopDistance = 4.5f;
        [SerializeField] private float farDashMaxDistance = 6f;
        [SerializeField] private float farDashDuration = 0.35f;
        [SerializeField] private float farDashCooldown = 4.5f;
        [SerializeField, Range(0f, 1f)] private float farDashChance = 0.55f;
        private float _lastFarDashTime;

        [Header("Air Laser Mode")]
        [SerializeField] private MirrorAngelAirLaserSkill airLaserSkill;

        [Header("Animator")]
        [SerializeField] private MirrorAngelBossAnimatorBridge animBridge;

        [Header("Debug State (Stage 55 — visualization only)")]
        [SerializeField] private MirrorAngelBossDebugState debugState;

        private Transform _player;
        private float _nextDecisionTime;
        private string _lastSkillId = "";
        private Coroutine _stateRoutine;
        private float _desiredMoveX;
        private bool _brainMovementLocked;
        private float _repositionEndTime;
        private int _repositionDir;

        public MirrorAngelBossBrainState CurrentState => currentState;
        public float DesiredMoveX => _brainMovementLocked ? 0f : _desiredMoveX;
        public bool IsBrainMovementLocked => _brainMovementLocked;
        public bool IsBrainCasting => currentState == MirrorAngelBossBrainState.Windup
                                   || currentState == MirrorAngelBossBrainState.Casting;

        private void Awake()
        {
            if (boss == null) boss = GetComponent<MirrorSaintessBoss>();
            if (mover == null) mover = GetComponent<MirrorAngelBossGravityMover>();
            if (facing == null) facing = GetComponent<MirrorAngelFacingController>();
            if (beamSkill == null) beamSkill = GetComponent<MirrorAngelTripleBeamSkill>();
            if (groundRaySkill == null) groundRaySkill = GetComponent<MirrorAngelGroundRaySkill>();
            if (doubleSlashSkill == null) doubleSlashSkill = GetComponent<MirrorAngelDoubleSlashSkill>();
            if (doubleSlashDashSkill == null) doubleSlashDashSkill = GetComponent<MirrorAngelDoubleSlashDashSkill>();
            if (actionController == null) actionController = GetComponent<MirrorAngelBossActionController>();
            if (airLaserSkill == null) airLaserSkill = GetComponent<MirrorAngelAirLaserSkill>();
            if (animBridge == null) animBridge = GetComponent<MirrorAngelBossAnimatorBridge>();
            if (debugState == null) debugState = GetComponent<MirrorAngelBossDebugState>();
        }

        private void Start()
        {
            if (skills.Count == 0)
            {
                skills.Add(new MirrorAngelBossSkillOption
                {
                    skillId = "MirrorTripleBeam",
                    cooldown = 4.5f,
                    lastUseTime = -999f,
                    minRange = 4f,
                    maxRange = 12f,
                    baseWeight = 10f,
                    repeatPenalty = 3f
                });
                skills.Add(new MirrorAngelBossSkillOption
                {
                    skillId = "MirrorAngelGroundRay",
                    cooldown = 8f,
                    lastUseTime = -999f,
                    minRange = 0f,
                    maxRange = 100f,
                    baseWeight = 8f,
                    repeatPenalty = 4f
                });
                skills.Add(new MirrorAngelBossSkillOption
                {
                    skillId = "DoubleSlash",
                    cooldown = 3f,
                    lastUseTime = -999f,
                    minRange = 0.8f,
                    maxRange = 2.8f,
                    baseWeight = 9f,
                    repeatPenalty = 2f
                });
                skills.Add(new MirrorAngelBossSkillOption
                {
                    skillId = "DoubleSlashDash",
                    cooldown = 5f,
                    lastUseTime = -999f,
                    minRange = 2.0f,
                    maxRange = 5.0f,
                    baseWeight = 7f,
                    repeatPenalty = 3f
                });
                skills.Add(new MirrorAngelBossSkillOption
                {
                    skillId = "FarDashApproach",
                    cooldown = farDashCooldown,
                    lastUseTime = -999f,
                    minRange = farDashMinDistance,
                    maxRange = 100f,
                    baseWeight = 9f,
                    repeatPenalty = 2f
                });
                skills.Add(new MirrorAngelBossSkillOption
                {
                    skillId = "AirLaserMode",
                    cooldown = 10f,
                    lastUseTime = -999f,
                    minRange = 4f,
                    maxRange = 14f,
                    baseWeight = 6f,
                    repeatPenalty = 4f
                });
            }

            _nextDecisionTime = Time.time + 1f;
            _lastFarDashTime = -999f;
            currentState = MirrorAngelBossBrainState.Idle;
        }

        private void Update()
        {
            FindPlayer();

            if (boss == null || boss.IsDead)
            {
                if (currentState != MirrorAngelBossBrainState.Dead)
                {
                    currentState = MirrorAngelBossBrainState.Dead;
                    StopAllBossActions();
                    PushDebug(BossAIState.Dead, "hp<=0");
                }
                return;
            }

            if (_player == null)
            {
                _desiredMoveX = 0f;
                return;
            }

            if (currentState == MirrorAngelBossBrainState.Reposition)
            {
                if (Time.time >= _repositionEndTime)
                {
                    _desiredMoveX = 0f;
                    currentState = MirrorAngelBossBrainState.Idle;
                }
                else
                {
                    _desiredMoveX = _repositionDir * repositionSpeedMultiplier;
                }
                return;
            }

            if (currentState == MirrorAngelBossBrainState.Approach)
            {
                float dir = Mathf.Sign(_player.position.x - transform.position.x);
                _desiredMoveX = dir;
                float dist = DistanceToPlayer();
                if (dist <= preferredMaxDistance)
                {
                    _desiredMoveX = 0f;
                    currentState = MirrorAngelBossBrainState.Idle;
                }
                return;
            }

            if (currentState == MirrorAngelBossBrainState.Windup ||
                currentState == MirrorAngelBossBrainState.Casting ||
                currentState == MirrorAngelBossBrainState.Recovery)
                return;

            if (actionController != null && actionController.IsActionLocked)
                return;

            if (Time.time < _nextDecisionTime)
                return;

            _nextDecisionTime = Time.time + Random.Range(decisionIntervalMin, decisionIntervalMax);
            DecideNextAction();
        }

        private void DecideNextAction()
        {
            if (boss == null || boss.IsDead || _player == null)
                return;

            PushDebug(BossAIState.Decide, "deciding");

            float dist = DistanceToPlayer();

            if (dist < tooCloseDistance)
            {
                StartReposition();
                return;
            }

            if (dist > farDistance)
            {
                if (TryUseSkill(dist))
                    return;
                currentState = MirrorAngelBossBrainState.Approach;
                PushDebug(BossAIState.Approach, "player far");
                return;
            }

            if (dist > preferredMaxDistance)
            {
                if (TryUseSkill(dist))
                    return;
                currentState = MirrorAngelBossBrainState.Approach;
                PushDebug(BossAIState.Approach, "beyond preferred range");
                return;
            }

            if (TryUseSkill(dist))
                return;

            if (dist < preferredMinDistance)
            {
                StartReposition();
            }
            else
            {
                currentState = MirrorAngelBossBrainState.KeepDistance;
                _desiredMoveX = 0f;
                PushDebug(BossAIState.KeepDistance, "safe distance");
            }
        }

        private bool TryUseSkill(float distance)
        {
            MirrorAngelBossSkillOption best = ChooseBestSkill(distance);
            if (best == null)
                return false;

            if (Random.value > attackChance)
            {
                currentState = MirrorAngelBossBrainState.KeepDistance;
                _desiredMoveX = 0f;
                PushDebug(BossAIState.KeepDistance, "hold (attackChance roll)");
                return false;
            }

            StartSkill(best);
            return true;
        }

        private void StartSkill(MirrorAngelBossSkillOption skill)
        {
            if (_stateRoutine != null)
                StopCoroutine(_stateRoutine);
            _stateRoutine = StartCoroutine(CastSkillRoutine(skill));
        }

        private IEnumerator CastSkillRoutine(MirrorAngelBossSkillOption skill)
        {
            MirrorAngelActionType actionType;
            switch (skill.skillId)
            {
                case "MirrorTripleBeam": actionType = MirrorAngelActionType.TripleBeam; break;
                case "MirrorAngelGroundRay": actionType = MirrorAngelActionType.GroundRay; break;
                case "DoubleSlash": actionType = MirrorAngelActionType.DoubleSlash; break;
                case "DoubleSlashDash": actionType = MirrorAngelActionType.DoubleSlashDash; break;
                case "FarDashApproach": actionType = MirrorAngelActionType.FarDashApproach; break;
                case "AirLaserMode": actionType = MirrorAngelActionType.AirLaserMode; break;
                default: actionType = MirrorAngelActionType.None; break;
            }

            if (actionController == null) { yield break; }

            int token = actionController.BeginAction(actionType);
            if (token < 0) { yield break; }

            currentState = MirrorAngelBossBrainState.Windup;
            PushDebug(BossAIState.Windup, skill.skillId + " selected");
            PushSkill(skill.skillId);
            _brainMovementLocked = true;
            _desiredMoveX = 0f;

            bool started = false;

            try
            {
                if (skill.skillId == "MirrorTripleBeam" && beamSkill != null)
                    started = beamSkill.TryCast();
                else if (skill.skillId == "MirrorAngelGroundRay" && groundRaySkill != null)
                    started = groundRaySkill.TryCast();
                else if (skill.skillId == "DoubleSlash" && doubleSlashSkill != null)
                    started = doubleSlashSkill.TryCast();
                else if (skill.skillId == "DoubleSlashDash" && doubleSlashDashSkill != null)
                    started = doubleSlashDashSkill.TryCast();
                else if (skill.skillId == "FarDashApproach")
                {
                    started = true;
                    StartCoroutine(DoFarDashCoroutine());
                }
                else if (skill.skillId == "AirLaserMode" && airLaserSkill != null)
                    started = airLaserSkill.TryCast();

                if (started)
                {
                    currentState = MirrorAngelBossBrainState.Casting;
                    PushDebug(skill.skillId == "AirLaserMode" ? BossAIState.AirMode : BossAIState.Casting, skill.skillId);

                    bool IsSkillRunning()
                    {
                        if (skill.skillId == "MirrorTripleBeam") return beamSkill != null && beamSkill.IsCasting;
                        if (skill.skillId == "MirrorAngelGroundRay") return groundRaySkill != null && groundRaySkill.IsCasting;
                        if (skill.skillId == "DoubleSlash") return doubleSlashSkill != null && doubleSlashSkill.IsCasting;
                        if (skill.skillId == "DoubleSlashDash") return doubleSlashDashSkill != null && doubleSlashDashSkill.IsCasting;
                        if (skill.skillId == "FarDashApproach") return _brainMovementLocked;
                        if (skill.skillId == "AirLaserMode") return airLaserSkill != null && airLaserSkill.IsCasting;
                        return false;
                    }

                    while (IsSkillRunning())
                    {
                        if (boss == null || boss.IsDead)
                        {
                            StopAllBossActions();
                            yield break;
                        }
                        yield return null;
                    }
                }
            }
            finally
            {
                actionController.EndAction(token);
                _brainMovementLocked = false;
                _desiredMoveX = 0f;
                skill.lastUseTime = Time.time;
                _lastSkillId = skill.skillId;
                _nextDecisionTime = Time.time + Random.Range(decisionIntervalMin, decisionIntervalMax);
                ClearDebugSkill();
                if (currentState != MirrorAngelBossBrainState.Dead)
                {
                    PushDebug(BossAIState.Recovery, skill.skillId + " finished");
                    currentState = MirrorAngelBossBrainState.Idle;
                }
                _stateRoutine = null;
            }
        }

        private void StartReposition()
        {
            currentState = MirrorAngelBossBrainState.Reposition;
            PushDebug(BossAIState.Reposition, "player too close");
            _repositionDir = (int)Mathf.Sign(transform.position.x - _player.position.x);
            if (_repositionDir == 0) _repositionDir = -1;
            _repositionEndTime = Time.time + Random.Range(repositionDurationMin, repositionDurationMax);

            if (facing != null && !facing.IsFacingLocked)
                facing.FaceMoveDirection(_repositionDir);
        }

        private void StopAllBossActions()
        {
            if (_stateRoutine != null)
            {
                StopCoroutine(_stateRoutine);
                _stateRoutine = null;
            }
            _desiredMoveX = 0f;
            _brainMovementLocked = false;
            if (actionController != null)
                actionController.ForceCancelAction();
            else if (mover != null)
            {
                mover.SetMovementLocked(false);
                mover.SetCasting(false);
                mover.ClearExternalVelocity();
                var rb = mover.Rigidbody;
                if (rb != null) { rb.velocity = Vector2.zero; rb.angularVelocity = 0f; rb.gravityScale = 0f; }
            }
            if (facing != null)
                facing.UnlockFacing();
            if (animBridge != null && animBridge.Animator != null)
                animBridge.Animator.SetInteger("AttackType", 0);
        }

        private bool IsSkillUsable(MirrorAngelBossSkillOption skill, float distance)
        {
            if (skill == null) return false;
            bool cdReady = Time.time >= skill.lastUseTime + skill.cooldown;
            bool rangeOk = distance >= skill.minRange && distance <= skill.maxRange;
            bool stateOk = currentState != MirrorAngelBossBrainState.Windup
                        && currentState != MirrorAngelBossBrainState.Casting
                        && currentState != MirrorAngelBossBrainState.Recovery
                        && currentState != MirrorAngelBossBrainState.Dead;
            return cdReady && rangeOk && stateOk;
        }

        private float ScoreSkill(MirrorAngelBossSkillOption skill, float distance)
        {
            if (!IsSkillUsable(skill, distance))
                return -9999f;

            float score = skill.baseWeight;

            float idealRange = (skill.minRange + skill.maxRange) * 0.5f;
            float rangeScore = 1f - Mathf.Abs(distance - idealRange) / Mathf.Max(idealRange, 0.01f);
            score += rangeScore * 5f;

            if (skill.skillId == "FarDashApproach" && distance > farDashMinDistance)
                score += 5f;
            if (skill.skillId == "AirLaserMode" && distance >= 5f)
                score += 3f;

            if (_lastSkillId == skill.skillId)
                score -= skill.repeatPenalty;

            score += Random.Range(-1f, 1f);

            return score;
        }

        private MirrorAngelBossSkillOption ChooseBestSkill(float distance)
        {
            MirrorAngelBossSkillOption best = null;
            float bestScore = -9999f;

            foreach (MirrorAngelBossSkillOption skill in skills)
            {
                float s = ScoreSkill(skill, distance);
                if (s > bestScore)
                {
                    bestScore = s;
                    best = skill;
                }
            }

            return best;
        }

        private float DistanceToPlayer()
        {
            if (_player == null) return 999f;
            return Vector2.Distance(transform.position, _player.position);
        }

        private void FindPlayer()
        {
            if (_player != null) return;
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) _player = p.transform;
        }

        // ---- Stage 55: visualization-only debug-state mirroring (no logic change) ----
        private void PushDebug(BossAIState state, string reason = null)
        {
            if (debugState != null)
                debugState.SetState(state, reason);
        }

        private void PushSkill(string skillName)
        {
            if (debugState != null)
                debugState.SetSkill(skillName);
        }

        private void ClearDebugSkill()
        {
            if (debugState != null)
                debugState.ClearSkill();
        }

        private IEnumerator DoFarDashCoroutine()
        {
            if (_player == null || mover == null) { _brainMovementLocked = false; yield break; }

            float dir = Mathf.Sign(_player.position.x - transform.position.x);
            float distToPlayer = Mathf.Abs(_player.position.x - transform.position.x);
            float dashDist = Mathf.Min(farDashMaxDistance, Mathf.Max(0f, distToPlayer - farDashStopDistance));
            if (dashDist <= 0f) { _brainMovementLocked = false; yield break; }

            var rb = mover.Rigidbody;
            float startX = rb != null ? rb.position.x : transform.position.x;
            float targetX = startX + dir * dashDist;
            float elapsed = 0f;

            while (elapsed < farDashDuration)
            {
                if (boss != null && boss.IsDead) { _brainMovementLocked = false; yield break; }
                elapsed += Time.fixedDeltaTime;
                float t = Mathf.Clamp01(elapsed / farDashDuration);
                float eased = t * t * (3f - 2f * t);
                float x = Mathf.Lerp(startX, targetX, eased);
                if (rb != null) { var p = rb.position; p.x = x; rb.MovePosition(p); }
                else { var p = transform.position; p.x = x; transform.position = p; }
                yield return new WaitForFixedUpdate();
            }

            if (rb != null) { var fp = rb.position; fp.x = targetX; rb.MovePosition(fp); }
            else { var tp = transform.position; tp.x = targetX; transform.position = tp; }

            _brainMovementLocked = false;
            _lastFarDashTime = Time.time;
        }

#if UNITY_EDITOR
        [ContextMenu("Debug/Force Decide Now")]
        private void DebugForceDecide()
        {
            _nextDecisionTime = 0f;
            if (_player == null)
                FindPlayer();
        }

        [ContextMenu("Debug/Force FarDashApproach")]
        private void DebugForceFarDash()
        {
            if (!Application.isPlaying) return;
            FindPlayer();
            if (_player == null || actionController == null) return;
            int token = actionController.BeginAction(MirrorAngelActionType.FarDashApproach);
            if (token < 0) { Debug.Log("[BossAI] FarDash rejected: ActionLocked or dead"); return; }
            Debug.Log("[BossAI] FarDash started via debug");
            StartCoroutine(DoFarDashWithToken(token));
        }

        private IEnumerator DoFarDashWithToken(int token)
        {
            _brainMovementLocked = true;
            yield return DoFarDashCoroutine();
            if (actionController != null) actionController.EndAction(token);
            _brainMovementLocked = false;
            currentState = MirrorAngelBossBrainState.Idle;
        }

        [ContextMenu("Debug/Print State")]
        private void DebugPrintState()
        {
            float dist = DistanceToPlayer();
            Debug.Log($"[Brain] state={currentState}, distance={dist:F1}, " +
                      $"nextDecision={_nextDecisionTime - Time.time:F1}s, " +
                      $"lastSkill={_lastSkillId}, moveX={_desiredMoveX:F2}, " +
                      $"movementLocked={_brainMovementLocked}");
            for (int i = 0; i < skills.Count; i++)
            {
                MirrorAngelBossSkillOption sk = skills[i];
                Debug.Log($"  Skill[{i}] {sk.skillId} cdRemain={Mathf.Max(0, sk.lastUseTime + sk.cooldown - Time.time):F1}s " +
                          $"usable={IsSkillUsable(sk, dist)} score={ScoreSkill(sk, dist):F1}");
            }
        }
#endif
    }
}
