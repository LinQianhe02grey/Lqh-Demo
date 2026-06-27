using UnityEngine;
using MirrorSaintessBossPack;

namespace Cardwin.Boss
{
    /// <summary>
    /// Stage 55 — runtime Boss AI state monitor (visualization only). Shows the current
    /// automaton state, the active skill name, distance to player, action lock, current
    /// action token, dead flag and air-mode flag in the Inspector and (optionally) the
    /// Scene view. State / skill changes are pushed by MirrorAngelBossBrain; the rest of
    /// the runtime info is self-refreshed at a low frequency from already-public members
    /// of the action controller / mover / boss root.
    ///
    /// This component contains NO combat logic and does NOT modify any boss / player /
    /// projectile / skill behaviour. Console logging is throttled to fire only on a real
    /// state change (never per-frame).
    /// </summary>
    [RequireComponent(typeof(MirrorSaintessBoss))]
    public sealed class MirrorAngelBossDebugState : MonoBehaviour
    {
        [Header("Runtime State (read-only)")]
        [SerializeField] private BossAIState currentState = BossAIState.Idle;
        [SerializeField] private string currentSkillName = "";
        [SerializeField] private float distanceToPlayer;
        [SerializeField] private bool actionLocked;
        [SerializeField] private int currentActionToken;
        [SerializeField] private bool isDead;
        [SerializeField] private bool isAirMode;

        [Header("Refs (auto-resolved in Awake)")]
        [SerializeField] private MirrorSaintessBoss boss;
        [SerializeField] private MirrorAngelBossActionController actionController;
        [SerializeField] private MirrorAngelBossGravityMover mover;

        [Header("Options")]
        [Tooltip("Log a single line ONLY when the state actually changes (never per-frame).")]
        [SerializeField] private bool logStateChanges = true;
        [Tooltip("Draw a small status label above the boss in the Scene view (editor only).")]
        [SerializeField] private bool drawSceneLabel = true;
        [Tooltip("Seconds between self-refreshes of distance / lock / token / dead / air.")]
        [SerializeField] private float refreshInterval = 0.1f;

        private Transform _player;
        private float _nextRefreshTime;

        public BossAIState CurrentState => currentState;
        public string CurrentSkillName => currentSkillName;
        public float DistanceToPlayer => distanceToPlayer;
        public bool ActionLocked => actionLocked;
        public int CurrentActionToken => currentActionToken;
        public bool IsDead => isDead;
        public bool IsAirMode => isAirMode;

        private void Awake()
        {
            if (boss == null) boss = GetComponent<MirrorSaintessBoss>();
            if (actionController == null) actionController = GetComponent<MirrorAngelBossActionController>();
            if (mover == null) mover = GetComponent<MirrorAngelBossGravityMover>();
        }

        private void Update()
        {
            if (Time.time < _nextRefreshTime)
                return;
            _nextRefreshTime = Time.time + Mathf.Max(0.02f, refreshInterval);
            RefreshRuntimeInfo();
        }

        private void RefreshRuntimeInfo()
        {
            FindPlayer();
            distanceToPlayer = _player != null
                ? Vector2.Distance(transform.position, _player.position)
                : -1f;

            if (actionController != null)
            {
                actionLocked = actionController.IsActionLocked;
                currentActionToken = actionController.CurrentToken;
                isAirMode = actionController.CurrentAction == MirrorAngelActionType.AirLaserMode
                            || (mover != null && mover.IsFlying);
            }
            else if (mover != null)
            {
                isAirMode = mover.IsFlying;
            }

            isDead = boss != null && boss.IsDead;
        }

        /// <summary>Push a new automaton state. Logs once on a real change only.</summary>
        public void SetState(BossAIState state, string reason = null)
        {
            if (state == currentState)
                return;

            if (logStateChanges)
            {
                string r = string.IsNullOrEmpty(reason) ? "" : $", reason={reason}";
                Debug.Log($"[BossAI] State: {currentState} -> {state}{r}", this);
            }

            currentState = state;
        }

        /// <summary>Set the currently active / selected skill name (display only).</summary>
        public void SetSkill(string skillName)
        {
            currentSkillName = skillName ?? "";
        }

        /// <summary>Clear the active skill name (display only).</summary>
        public void ClearSkill()
        {
            currentSkillName = "";
        }

        /// <summary>Optional external push of the raw runtime info (display only).</summary>
        public void UpdateRuntimeInfo(float distance, bool locked, int token, bool dead, bool airMode)
        {
            distanceToPlayer = distance;
            actionLocked = locked;
            currentActionToken = token;
            isDead = dead;
            isAirMode = airMode;
        }

        private void FindPlayer()
        {
            if (_player != null)
                return;
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                _player = p.transform;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!drawSceneLabel)
                return;

            string skill = string.IsNullOrEmpty(currentSkillName) ? "" : $" [{currentSkillName}]";
            string label = $"BossAI: {currentState}{skill}\n" +
                           $"locked={actionLocked} token={currentActionToken}\n" +
                           $"dist={distanceToPlayer:F1} dead={isDead} air={isAirMode}";

            UnityEditor.Handles.color = Color.cyan;
            UnityEditor.Handles.Label(transform.position + Vector3.up * 3.2f, label);
        }
#endif
    }
}
