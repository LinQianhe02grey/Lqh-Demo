using UnityEngine;
using Cardwin.Combat;

namespace Cardwin.Modules
{
    /// <summary>
    /// Player-side state for the third special module (Confession Night). On
    /// Activate it: (1) deactivates the other two modules, (2) shatters the normal
    /// combat UI, (3) spins up a RhythmGameController that plays 告白の夜 on loop and
    /// runs the full-song rhythm chart. The mode persists (music + chart loop, UI
    /// stays hidden) until the player dies or the scene changes. Additive only.
    /// </summary>
    public sealed class PlayerConfessionNightModuleState : MonoBehaviour
    {
        [Header("Module State")]
        [SerializeField] private bool isActive;

        [Header("Audio (optional - assign real mp3 here)")]
        [Tooltip("Drop Assets/Audio/Ayasa_Confession_Night.mp3 here. If empty, a procedural placeholder is used so the loop is still testable.")]
        [SerializeField] private AudioClip confessionNightClip;

        private Health _health;
        private CombatUIBreakController _breakController;
        private RhythmGameController _rhythmController;

        public bool IsActive => isActive;

        private void Awake()
        {
            ResolveHealth();
        }

        private void ResolveHealth()
        {
            if (_health != null) return;
            _health = GetComponent<Health>();
            if (_health == null) _health = GetComponentInParent<Health>();
            if (_health == null) _health = GetComponentInChildren<Health>();
        }

        [ContextMenu("Debug/Activate Confession Night Module")]
        public void Activate()
        {
            Activate(null);
        }

        /// <summary>
        /// Activate with an optional pre-resolved clip (passed by the pickup which
        /// preloads the song at scene start). Heavy work (UI shatter, canvas, chart,
        /// audio) is spread across frames inside the break controller and rhythm
        /// controller coroutines, so this call itself stays light on the F-press frame.
        /// </summary>
        public void Activate(AudioClip clipOverride)
        {
            if (isActive) return;
            isActive = true;

            DeactivateOtherModules();
            ResolveHealth();

            if (_health == null)
            {
                Debug.LogError("[ConfessionNightModule] No Health found on player; cannot start rhythm mode.");
                isActive = false;
                return;
            }

            AudioClip clip = clipOverride != null ? clipOverride : confessionNightClip;
            if (clip != null) confessionNightClip = clip;

            // 1) Break the normal combat UI (shatter + fade); spreads across frames.
            var breakGo = new GameObject("ConfessionNight_UIBreak");
            _breakController = breakGo.AddComponent<CombatUIBreakController>();
            _breakController.BreakNormalCombatUI();

            // 2) Build the rhythm controller (DontDestroyOnLoad; own persistent canvas).
            var rhythmGo = new GameObject("ConfessionNight_RhythmController");
            _rhythmController = rhythmGo.AddComponent<RhythmGameController>();

            // Parent the UI-break controller under the (now DontDestroyOnLoad) rhythm
            // controller so it survives Demo_Combat -> BossRoom. This keeps the
            // _breakController reference valid so the normal HUD can be restored on
            // death / Retry (otherwise it would be a destroyed Demo_Combat object).
            breakGo.transform.SetParent(rhythmGo.transform, true);

            _rhythmController.BeginRhythmMode(_health, _breakController, clip);

            Debug.Log($"[ConfessionNightModule] ACTIVATED (staged). RhythmModeActive={RhythmGameController.IsRhythmModeActive}, clip={(clip != null ? clip.name : "resolve-at-runtime")}");
        }

        private void DeactivateOtherModules()
        {
            var cursed = GetComponent<PlayerCursedEightModuleState>();
            if (cursed != null && cursed.IsActive)
            {
                cursed.Deactivate();
                Debug.Log("[ConfessionNightModule] Deactivated existing Cursed module.");
            }

            var blessed = GetComponent<PlayerBlessedEightModuleState>();
            if (blessed != null && blessed.IsActive)
            {
                blessed.Deactivate();
                Debug.Log("[ConfessionNightModule] Deactivated existing Blessed module.");
            }
        }

        public void Deactivate()
        {
            if (!isActive) return;
            isActive = false;
            if (_rhythmController != null)
                _rhythmController.EndRhythmMode(restoreUI: true);
        }
    }
}
