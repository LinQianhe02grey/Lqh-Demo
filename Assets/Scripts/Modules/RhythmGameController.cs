using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cardwin.Combat;

namespace Cardwin.Modules
{
    /// <summary>
    /// Core of the Confession Night rhythm module. Builds its own ScreenSpaceOverlay
    /// RhythmGameCanvas (HitCircle + NoteTrack) at runtime, plays the song on a
    /// loop, generates a chart that covers the ENTIRE song (BPM 93, 4/4), spawns
    /// red/blue notes driven by audioSource.time, judges Left=Red / Right=Blue, and
    /// applies effects: red hit -> homing bullet (3% target maxHp), blue hit ->
    /// heal 5% maxHp, miss/wrong -> lose 10% maxHp. When the music loops the chart
    /// loops too (nextNoteIndex resets, stale notes cleared); the normal UI is NOT
    /// auto-restored. Self-contained / additive.
    /// </summary>
    public sealed class RhythmGameController : MonoBehaviour
    {
        public static bool IsRhythmModeActive { get; private set; }
        public static RhythmGameController Instance { get; private set; }

        [Header("Tempo (告白の夜: 1=D, 4/4)")]
        [SerializeField] private float tempoBpm = 93f;
        [SerializeField] private float noteTravelTime = 2.0f;

        [Header("Chart range")]
        [SerializeField] private float introNoNoteTime = 6f;
        [SerializeField] private float endSafeTime = 2f;
        [SerializeField] private int chartSeed = 9301;

        [Header("Judgement (pixels, lenient)")]
        [SerializeField] private float hitWindowPixels = 110f;
        [SerializeField] private float perfectWindowPixels = 55f;
        [SerializeField] private float missWindowPixels = 150f;

        [Header("Layout (HitCircle anchor as fraction of screen)")]
        [Tooltip("HitCircle X = Screen.width * this. 0.25 = 25% from the left (center-left). All notes/judgement/miss derive from this.")]
        [SerializeField] private float hitCircleScreenX = 0.25f;
        [Tooltip("HitCircle Y = Screen.height * this. ~0.18 keeps it near the old magazine-UI height.")]
        [SerializeField] private float hitCircleScreenY = 0.18f;

        [Header("Audio")]
        [SerializeField] private AudioClip confessionNightClip;
        [Tooltip("Resources path (no extension) of the real song. Auto-loaded when no clip is assigned.")]
        [SerializeField] private string resourceClipPath = "Audio/Ayasa_Confession_Night";
        [Tooltip("DEBUG ONLY. When the real mp3 is missing, allow the procedural click-track placeholder. MUST stay false for normal play.")]
        [SerializeField] private bool allowPlaceholderWhenMissing = false;
        [SerializeField] private float fallbackSongLength = 290f;   // ~4:50
        [SerializeField] private float volume = 0.8f;

        [Header("Effects")]
        [SerializeField] private float healPercent = 0.05f;
        [SerializeField] private float penaltyPercent = 0.10f;
        [SerializeField] private float homingDamagePercent = 0.03f;
        [SerializeField] private float homingSpeed = 12f;
        [SerializeField] private float homingLifeTime = 4f;

        private GameObject _player;
        private PlayerController2D _playerController;
        private Health _playerHealth;
        private CombatUIBreakController _breakController;
        private AudioSource _audioSource;

        private Canvas _canvas;
        private RectTransform _noteTrack;
        private Image _hitCircle;
        private float _hitCircleX;
        private float _hitCircleY;
        private float _hitFlashEndTime;
        private bool _ready;

        private readonly List<RhythmNoteData> _chart = new List<RhythmNoteData>();
        private readonly List<RhythmNote> _activeNotes = new List<RhythmNote>();
        private int _nextNoteIndex;
        private int _loopCount;
        private float _lastAudioTime;
        private float _effectiveSongLength;
        private bool _usingPlaceholderClip;
        private bool _ended;
        private bool _forceStopped;

        private const float NoteDiameter = 64f;
        private const int RhythmCanvasSortingOrder = 1000;   // must stay clearly above BossHUD (50) and the normal HUD
        private static readonly Color GrayWhite = new Color(0.86f, 0.86f, 0.9f, 0.85f);
        private static readonly Color RedNote = new Color(0.95f, 0.25f, 0.32f, 0.95f);
        private static readonly Color BlueNote = new Color(0.3f, 0.55f, 1f, 0.95f);
        private static Sprite _discSprite;
        private static Sprite _ringSprite;
        private static AudioClip _placeholderCache;

        // ---- Reporting (read-only) ----
        public int ChartCount => _chart.Count;
        public float FirstNoteTime => _chart.Count > 0 ? _chart[0].hitTime : -1f;
        public float LastNoteTime => _chart.Count > 0 ? _chart[_chart.Count - 1].hitTime : -1f;
        public int LoopCount => _loopCount;
        public int NextNoteIndex => _nextNoteIndex;
        public int ActiveNoteCount => _activeNotes.Count;
        public float SongLength => _effectiveSongLength;
        public bool UsingPlaceholderClip => _usingPlaceholderClip;
        public bool AudioLoop => _audioSource != null && _audioSource.loop;
        public float Bpm => tempoBpm;
        public float BeatDuration => 60f / tempoBpm;
        public float NoteTravelTimeValue => noteTravelTime;
        public float HitWindowPixels => hitWindowPixels;
        public int RedCount { get; private set; }
        public int BlueCount { get; private set; }

        private void Awake()
        {
            // Persistent runtime: the rhythm controller (and its child RhythmGameCanvas
            // + AudioSource + active notes) must survive Demo_Combat -> BossRoom unloads.
            // Without this the whole rhythm tree was destroyed with Demo_Combat and the
            // mode silently ended (UI/music/notes gone, shooting unlocked).
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[RhythmGame] Duplicate controller destroyed.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            Debug.Log("[RhythmGame] Persistent runtime created. dontDestroy=true");
        }

        public void BeginRhythmMode(Health playerHealth, CombatUIBreakController breakController, AudioClip clipOverride)
        {
            if (IsRhythmModeActive) return;

            _playerHealth = playerHealth;
            _breakController = breakController;
            if (clipOverride != null) confessionNightClip = clipOverride;

            // Bind the current-scene player up front (also populates _player /
            // _playerController). Cheap tag lookup; safe on the F-press frame.
            RebindPlayerInCurrentScene();

            // Lightweight on the F-press frame: just set state + lock normal shooting,
            // then spread the heavy steps (canvas / audio / chart / play) across frames.
            _nextNoteIndex = 0;
            _loopCount = 0;
            _lastAudioTime = 0f;
            _ended = false;
            _forceStopped = false;
            _ready = false;
            IsRhythmModeActive = true;   // shooting is locked immediately (static, cross-scene)

            StartCoroutine(BeginRhythmModeRoutine());
        }

        private System.Collections.IEnumerator BeginRhythmModeRoutine()
        {
            // Let the pickup/F-press frame finish before doing any heavy work.
            yield return null;

            float t0 = Time.realtimeSinceStartup;
            BuildCanvas();
            Debug.Log($"[ConfessionNightModule] CreateRhythmCanvas+HitCircle cost={(Time.realtimeSinceStartup - t0) * 1000f:F1} ms");
            yield return null;

            t0 = Time.realtimeSinceStartup;
            SetupAudio();   // ResolveAudioClip should be cheap (preloaded + Streaming import)
            Debug.Log($"[ConfessionNightModule] ResolveAudioClip cost={(Time.realtimeSinceStartup - t0) * 1000f:F1} ms");
            yield return null;

            t0 = Time.realtimeSinceStartup;
            GenerateFullSongChart(_effectiveSongLength);   // data only, no GameObjects
            Debug.Log($"[ConfessionNightModule] GenerateFullSongChart cost={(Time.realtimeSinceStartup - t0) * 1000f:F1} ms, notes={_chart.Count}");
            yield return null;

            t0 = Time.realtimeSinceStartup;
            _nextNoteIndex = 0;
            _lastAudioTime = 0f;
            if (_audioSource != null && _audioSource.clip != null)
            {
                _audioSource.Play();
                Debug.Log($"[RhythmGame] Music started. clip={_audioSource.clip.name}, length={_audioSource.clip.length:F1}, loop={_audioSource.loop}, placeholder={_usingPlaceholderClip}");
            }
            else
            {
                Debug.LogError("[RhythmGame] No real music clip resolved. Rhythm mode runs WITHOUT audio (timeline frozen until a clip is provided).");
            }
            _ready = true;   // pipeline (spawn/judge/miss) starts now, aligned with audio
            Debug.Log($"[ConfessionNightModule] StartAudio cost={(Time.realtimeSinceStartup - t0) * 1000f:F1} ms");
            Debug.Log($"[RhythmGame] Full song chart generated. songLength={_effectiveSongLength:F1}, notes={_chart.Count}, first={FirstNoteTime:F2}, last={LastNoteTime:F2}, red={RedCount}, blue={BlueCount}, bpm={tempoBpm}, travel={noteTravelTime}, hitWindow={hitWindowPixels}");
        }

        private void Update()
        {
            if (!IsRhythmModeActive || _ended) return;

            if (_playerHealth != null && _playerHealth.IsDead())
            {
                StopRhythmModeInternal("PlayerDeath");
                return;
            }

            if (!_ready) return;   // staged activation still in progress

            float audioTime = _audioSource != null ? _audioSource.time : 0f;

            if (audioTime < _lastAudioTime - 1f)
                OnMusicLooped();
            _lastAudioTime = audioTime;

            SpawnNotesByAudioTime(audioTime);
            UpdateNotePositions(audioTime);
            UpdateInput();
            UpdateMissCheck();
            UpdateHitCircleFlash();
        }

        // ---------------- Chart ----------------

        private void GenerateFullSongChart(float songLength)
        {
            _chart.Clear();
            RedCount = 0;
            BlueCount = 0;

            Random.State prev = Random.state;
            Random.InitState(chartSeed);

            float beat = 60f / tempoBpm;
            float start = introNoNoteTime;
            float end = Mathf.Max(start, songLength - endSafeTime);

            int beatIndex = 0;
            for (float t = start; t < end; t += beat)
            {
                int barBeat = beatIndex % 4;
                if (ShouldSpawnNote(t, songLength, barBeat))
                {
                    RhythmNoteType type = ChooseNoteType(t, songLength, barBeat);
                    AddNote(t, type);

                    float p = t / songLength;
                    if (p >= 0.70f && p < 0.92f)
                    {
                        float half = t + beat * 0.5f;
                        if (half < end)
                            AddNote(half, ChooseNoteType(half, songLength, (barBeat + 1) % 4));
                    }
                }
                beatIndex++;
            }

            _chart.Sort((a, b) => a.hitTime.CompareTo(b.hitTime));
            Random.state = prev;
        }

        private void AddNote(float t, RhythmNoteType type)
        {
            _chart.Add(new RhythmNoteData(t, type));
            if (type == RhythmNoteType.Red) RedCount++; else BlueCount++;
        }

        private bool ShouldSpawnNote(float t, float songLength, int barBeat)
        {
            float p = t / songLength;
            if (p < 0.10f) return barBeat == 0 || barBeat == 2;
            if (p < 0.35f) return barBeat != 3;
            if (p < 0.70f) return true;
            if (p < 0.92f) return true;
            return barBeat == 0 || barBeat == 2;
        }

        private RhythmNoteType ChooseNoteType(float t, float songLength, int barBeat)
        {
            float p = t / songLength;
            float redChance;
            if (p < 0.10f) redChance = 0.50f;
            else if (p < 0.35f) redChance = 0.55f;
            else if (p < 0.70f) redChance = 0.65f;
            else if (p < 0.92f) redChance = 0.75f;
            else redChance = 0.50f;

            if (barBeat == 0) redChance += 0.15f;
            else if (barBeat == 2) redChance -= 0.15f;

            return Random.value < redChance ? RhythmNoteType.Red : RhythmNoteType.Blue;
        }

        // ---------------- Spawning / movement ----------------

        private void SpawnNotesByAudioTime(float audioTime)
        {
            while (_nextNoteIndex < _chart.Count)
            {
                RhythmNoteData data = _chart[_nextNoteIndex];
                if (audioTime >= data.hitTime - noteTravelTime)
                {
                    SpawnNote(data);
                    _nextNoteIndex++;
                }
                else break;
            }
        }

        private void SpawnNote(RhythmNoteData data)
        {
            var go = new GameObject(data.type + "Note", typeof(RectTransform), typeof(Image), typeof(RhythmNote));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(_noteTrack, false);

            var img = go.GetComponent<Image>();
            img.sprite = GetDiscSprite();
            img.raycastTarget = false;

            var note = go.GetComponent<RhythmNote>();
            note.Setup(data.type, data.hitTime, data.type == RhythmNoteType.Red ? RedNote : BlueNote, NoteDiameter);
            _activeNotes.Add(note);
        }

        private void UpdateNotePositions(float audioTime)
        {
            float spawnX = Screen.width * 1.05f;
            float speed = (spawnX - _hitCircleX) / Mathf.Max(0.01f, noteTravelTime);
            for (int i = 0; i < _activeNotes.Count; i++)
            {
                var note = _activeNotes[i];
                if (note == null) continue;
                float x = _hitCircleX + speed * (note.hitTime - audioTime);
                note.SetAnchoredPosition(x, _hitCircleY);
            }
        }

        // ---------------- Input / judgement ----------------

        private void UpdateInput()
        {
            bool left = Input.GetMouseButtonDown(0);
            bool right = Input.GetMouseButtonDown(1);
            if (!left && !right) return;

            if (left)
            {
                FlashHitCircle(RedNote);
                Judge(RhythmNoteType.Red);
            }
            if (right)
            {
                FlashHitCircle(BlueNote);
                Judge(RhythmNoteType.Blue);
            }
        }

        private void Judge(RhythmNoteType pressed)
        {
            // 1) Prefer the nearest matching-color note inside the hit window.
            RhythmNote matching = FindNearestNote(pressed, true, hitWindowPixels);
            if (matching != null)
            {
                float dx = Mathf.Abs(matching.CurrentX - _hitCircleX);
                bool perfect = dx <= perfectWindowPixels;
                matching.judged = true;
                _activeNotes.Remove(matching);
                Destroy(matching.gameObject);
                OnHit(pressed, perfect);
                return;
            }

            // 2) Otherwise, if a wrong-color note is inside the window -> Wrong.
            RhythmNote anyNote = FindNearestNote(pressed, false, hitWindowPixels);
            if (anyNote != null)
            {
                anyNote.judged = true;
                _activeNotes.Remove(anyNote);
                Destroy(anyNote.gameObject);
                OnWrong();
                return;
            }

            // 3) Empty click -> no penalty (lenient).
        }

        private RhythmNote FindNearestNote(RhythmNoteType color, bool requireMatchColor, float window)
        {
            RhythmNote best = null;
            float bestDx = float.MaxValue;
            for (int i = 0; i < _activeNotes.Count; i++)
            {
                var n = _activeNotes[i];
                if (n == null || n.judged) continue;
                if (requireMatchColor && n.type != color) continue;
                if (!requireMatchColor && n.type == color) continue; // looking specifically for wrong-color
                float dx = Mathf.Abs(n.CurrentX - _hitCircleX);
                if (dx <= window && dx < bestDx)
                {
                    bestDx = dx;
                    best = n;
                }
            }
            return best;
        }

        private void OnHit(RhythmNoteType type, bool perfect)
        {
            string grade = perfect ? "PERFECT" : "GOOD";
            if (type == RhythmNoteType.Red)
            {
                SpawnHomingBullet();
                Debug.Log($"[RhythmGame] RED hit ({grade}) -> homing bullet fired.");
            }
            else
            {
                HealPlayer();
                Debug.Log($"[RhythmGame] BLUE hit ({grade}) -> player healed.");
            }
        }

        private void OnWrong()
        {
            ApplyPenalty("WRONG (color mismatch)");
        }

        private void UpdateMissCheck()
        {
            float missLine = _hitCircleX - missWindowPixels;
            for (int i = _activeNotes.Count - 1; i >= 0; i--)
            {
                var n = _activeNotes[i];
                if (n == null) { _activeNotes.RemoveAt(i); continue; }
                if (n.judged) continue;
                if (n.CurrentX < missLine)
                {
                    n.judged = true;
                    _activeNotes.RemoveAt(i);
                    Destroy(n.gameObject);
                    ApplyPenalty("MISS");
                }
            }
        }

        // ---------------- Effects ----------------

        private void HealPlayer()
        {
            if (_playerHealth == null) RebindPlayerInCurrentScene();
            if (_playerHealth == null || _playerHealth.IsDead()) return;
            int amount = Mathf.Max(1, Mathf.CeilToInt(_playerHealth.maxHealth * healPercent));
            _playerHealth.Heal(amount);
            Debug.Log($"[RhythmGame] Heal {amount} ({healPercent:P0} of {_playerHealth.maxHealth}). player={_playerHealth.name}, hp={_playerHealth.currentHealth}/{_playerHealth.maxHealth}");
        }

        private void ApplyPenalty(string reason)
        {
            if (_playerHealth == null) RebindPlayerInCurrentScene();
            if (_playerHealth == null || _playerHealth.IsDead()) return;
            int amount = Mathf.Max(1, Mathf.CeilToInt(_playerHealth.maxHealth * penaltyPercent));
            _playerHealth.TakeDamage(amount);
            Debug.Log($"[RhythmGame] {reason} -> -{amount} ({penaltyPercent:P0} of {_playerHealth.maxHealth}). player={_playerHealth.name}, hp={_playerHealth.currentHealth}/{_playerHealth.maxHealth}");
        }

        private void SpawnHomingBullet()
        {
            // Spawn at the CURRENT (rebound) player; never a destroyed Demo_Combat player.
            if (_player == null && _playerHealth == null)
                RebindPlayerInCurrentScene();

            Transform player = _player != null ? _player.transform
                             : (_playerHealth != null ? _playerHealth.transform : null);
            if (player == null)
            {
                Debug.LogWarning("[RhythmGame] Red hit: no current-scene player; homing bullet not spawned.");
                return;
            }

            Vector3 origin = player.position + Vector3.up * 0.6f;

            Health target = FindNearestNormalEnemy(player);
            Vector2 fallbackDir = player.localScale.x >= 0f ? Vector2.right : Vector2.left;

            // No SpriteRenderer on the root: RhythmHomingBullet builds its own scaled
            // "Visual" child so the 5x visual never affects hit range. The bullet
            // re-acquires the nearest current-scene enemy in-flight (coasts if none).
            var go = new GameObject("RhythmHomingBullet", typeof(RhythmHomingBullet));
            go.transform.position = origin;
            var bullet = go.GetComponent<RhythmHomingBullet>();
            bullet.Init(target, fallbackDir, homingSpeed, homingLifeTime, homingDamagePercent);

            Debug.Log($"[RhythmGame] Homing bullet spawned at {player.name}, target={(target != null ? target.gameObject.name : "none (will retarget/coast)")}.");
        }

        private Health FindNearestNormalEnemy(Transform from)
        {
            Vector3 pos = from != null ? from.position : Vector3.zero;
            Scene activeScene = SceneManager.GetActiveScene();
            var all = FindObjectsOfType<Health>();
            Health best = null;
            float bestDist = float.MaxValue;
            for (int i = 0; i < all.Length; i++)
            {
                var h = all[i];
                if (h == null || h.IsDead()) continue;
                if (_playerHealth != null && h == _playerHealth) continue;
                if (h.CompareTag("Player")) continue;
                if (IsBoss(h.gameObject)) continue;

                // Current active scene only (seed target must not be a stale Demo_Combat enemy).
                var s = h.gameObject.scene;
                if (!s.IsValid() || !s.isLoaded || s != activeScene) continue;

                float d = Vector2.Distance(pos, h.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = h;
                }
            }
            return best;
        }

        private static bool IsBoss(GameObject go)
        {
            var comps = go.GetComponentsInParent<MonoBehaviour>(true);
            for (int i = 0; i < comps.Length; i++)
            {
                if (comps[i] == null) continue;
                if (comps[i].GetType().Name.IndexOf("Boss", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return go.name.IndexOf("Boss", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        // ---------------- Loop ----------------

        private void OnMusicLooped()
        {
            _loopCount++;
            _nextNoteIndex = 0;
            ClearAllActiveNotes();
            Debug.Log($"[RhythmGame] Music looped. loopCount={_loopCount}, nextNoteIndex reset=0, notes cleared, RhythmModeActive={IsRhythmModeActive}");
        }

        private void ClearAllActiveNotes()
        {
            for (int i = 0; i < _activeNotes.Count; i++)
                if (_activeNotes[i] != null) Destroy(_activeNotes[i].gameObject);
            _activeNotes.Clear();
        }

        // ---------------- Lifecycle ----------------

        /// <summary>
        /// Public, static, idempotent force-stop used by player death / Retry. Safe when
        /// Instance is null (non-rhythm play) and safe to call repeatedly.
        /// </summary>
        public static void ForceStopRhythmMode(string reason)
        {
            if (Instance == null)
                return;
            Instance.StopRhythmModeInternal(reason);
        }

        public void EndRhythmMode(bool restoreUI)
        {
            StopRhythmModeInternal(restoreUI ? "EndRhythmMode" : "EndRhythmMode(noRestore)");
        }

        /// <summary>
        /// Idempotent teardown: stops audio, kills coroutines, clears notes, DESTROYS the
        /// rhythm canvas (so it can never cover/intercept the GameOver/Retry UI), restores
        /// the normal combat HUD, nulls the player refs and resets chart counters, and
        /// flips IsRhythmModeActive=false so normal shooting unlocks. Keeps the persistent
        /// controller alive with cleared state. Never throws on null audio/canvas/notes/
        /// break-controller, and no-ops on repeat.
        /// </summary>
        private void StopRhythmModeInternal(string reason)
        {
            if (_forceStopped)
                return;
            _forceStopped = true;

            Debug.Log($"[RhythmGame] Force stop rhythm mode. reason={reason}");

            _ended = true;
            IsRhythmModeActive = false;   // normal shooting unlocks (PlayerController2D reads this static)

            StopAllCoroutines();
            ClearAllActiveNotes();

            if (_audioSource != null)
            {
                _audioSource.Stop();
                _audioSource.time = 0f;
            }

            // Destroy (not just hide) the canvas so it cannot cover or intercept the
            // GameOver / Retry UI. GraphicRaycaster was already disabled; destroy is safest.
            if (_canvas != null)
            {
                Destroy(_canvas.gameObject);
                _canvas = null;
                _noteTrack = null;
                _hitCircle = null;
            }

            // Bring the normal combat HUD back (the break controller is parented under this
            // persistent controller, so the reference survives Demo_Combat -> BossRoom).
            if (_breakController != null)
                _breakController.RestoreNormalCombatUI();

            _player = null;
            _playerHealth = null;
            _playerController = null;

            _nextNoteIndex = 0;
            _lastAudioTime = 0f;
            _loopCount = 0;

            Debug.Log($"[RhythmGame] Rhythm mode fully stopped. canvasDestroyed=true, audioStopped=true, notes=0, IsRhythmModeActive={IsRhythmModeActive}.");
        }

        private void OnDestroy()
        {
            // Only the live persistent instance clears the statics. A destroyed
            // duplicate (Instance != this) must NOT flip IsRhythmModeActive/Instance,
            // and because the controller is DontDestroyOnLoad this never fires on a
            // normal scene change (so the mode survives Demo_Combat -> BossRoom).
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                IsRhythmModeActive = false;
                Instance = null;
            }
        }

        // ---------------- Scene persistence (cross-scene rhythm) ----------------

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!IsRhythmModeActive || _ended)
                return;

            EnsureRhythmCanvasExists();
            EnsureAudioSourceStillPlaying();
            RebindPlayerInCurrentScene();   // bind to the BossRoom player; never touches audio time / chart / UI / IsRhythmModeActive

            Debug.Log($"[RhythmGame] SceneLoaded: {scene.name}, active={IsRhythmModeActive}, canvas={(_canvas != null)}, sortingOrder={(_canvas != null ? _canvas.sortingOrder : -1)}, audioTime={(_audioSource != null ? _audioSource.time : -1f):F2}, isPlaying={(_audioSource != null && _audioSource.isPlaying)}");
            Debug.Log($"[RhythmGame] SceneLoaded rebind done: scene={scene.name}, player={(_player != null ? _player.name : "null")}, health={(_playerHealth != null)}, controller={(_playerController != null)}");
        }

        private void EnsureRhythmCanvasExists()
        {
            // The canvas is a child of this DontDestroyOnLoad controller, so it
            // normally survives scene loads and this returns immediately (no
            // duplicate). If it was somehow lost, rebuild it (HitCircle + NoteTrack
            // + RectTransform/Image) so the rhythm UI keeps showing in the new scene.
            if (_canvas != null)
                return;

            BuildCanvas();
            Debug.Log("[RhythmGame] Rhythm canvas recreated after scene load.");
        }

        private void EnsureAudioSourceStillPlaying()
        {
            // Resume without resetting time: never set _audioSource.time = 0 here.
            if (_audioSource == null || _audioSource.clip == null)
                return;

            if (!_audioSource.isPlaying)
            {
                _audioSource.Play();
                Debug.Log("[RhythmGame] Audio resumed after scene load.");
            }
        }

        /// <summary>
        /// Re-resolves the player (and its Health / PlayerController2D) for the
        /// currently loaded scene so heal / penalty always act on the live player and
        /// never on a destroyed Demo_Combat object. Non-destructive: if no player is
        /// found it keeps the previous references (so a transient empty frame can't
        /// null out a valid binding). Touches NOTHING else (no audio/chart/UI/state).
        /// </summary>
        private void RebindPlayerInCurrentScene()
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");

            if (foundPlayer == null)
            {
                var controllers = FindObjectsOfType<PlayerController2D>();
                if (controllers != null && controllers.Length > 0)
                    foundPlayer = controllers[0].gameObject;
            }

            if (foundPlayer == null)
            {
                var healths = FindObjectsOfType<Health>();
                for (int i = 0; i < healths.Length; i++)
                {
                    var h = healths[i];
                    if (h == null) continue;
                    var pc = h.GetComponentInParent<PlayerController2D>();
                    if (pc != null)
                    {
                        foundPlayer = pc.gameObject;
                        break;
                    }
                }
            }

            if (foundPlayer == null)
            {
                Debug.LogWarning("[RhythmGame] RebindPlayer: no player found in current scene; keeping previous references.");
                return;
            }

            _player = foundPlayer;

            var health = foundPlayer.GetComponentInParent<Health>();
            if (health == null) health = foundPlayer.GetComponentInChildren<Health>();
            if (health != null) _playerHealth = health;

            var controller = foundPlayer.GetComponentInParent<PlayerController2D>();
            if (controller == null) controller = foundPlayer.GetComponentInChildren<PlayerController2D>();
            if (controller != null) _playerController = controller;

            Debug.Log($"[RhythmGame] Player rebound: player={_player.name}, health={(_playerHealth != null ? _playerHealth.name : "null")}, controller={(_playerController != null ? _playerController.name : "null")}");
        }

        // ---------------- Debug helpers (for tests) ----------------

        [ContextMenu("Debug/Force Music Loop")]
        public void DebugForceMusicLoop()
        {
            if (_audioSource != null) _audioSource.time = 0.01f;
            OnMusicLooped();
            _lastAudioTime = 0f;
        }

        public void DebugSeekAudio(float t)
        {
            if (_audioSource != null && _audioSource.clip != null)
                _audioSource.time = Mathf.Clamp(t, 0f, Mathf.Max(0.1f, _audioSource.clip.length - 0.2f));
        }

        public bool IsAudioPlaying => _audioSource != null && _audioSource.isPlaying;

        public void DebugForceRedHit() { OnHit(RhythmNoteType.Red, true); }
        public void DebugForceBlueHit() { OnHit(RhythmNoteType.Blue, true); }
        public void DebugForceMiss() { ApplyPenalty("MISS (debug)"); }
        public void DebugForceWrong() { OnWrong(); }
        public Health DebugNearestEnemy()
        {
            Transform player = _playerHealth != null ? _playerHealth.transform : null;
            return FindNearestNormalEnemy(player);
        }

        // ---------------- Build ----------------

        private void BuildCanvas()
        {
            var canvasGo = new GameObject("RhythmGameCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            _canvas = canvasGo.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = RhythmCanvasSortingOrder;   // above BossHUD (50) so rhythm UI stays on top in BossRoom
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;
            canvasGo.GetComponent<GraphicRaycaster>().enabled = false;

            _hitCircleX = Screen.width * hitCircleScreenX;
            _hitCircleY = Screen.height * hitCircleScreenY;

            var trackGo = new GameObject("NoteTrack", typeof(RectTransform));
            _noteTrack = trackGo.GetComponent<RectTransform>();
            _noteTrack.SetParent(canvasGo.transform, false);
            _noteTrack.anchorMin = Vector2.zero;
            _noteTrack.anchorMax = Vector2.one;
            _noteTrack.offsetMin = Vector2.zero;
            _noteTrack.offsetMax = Vector2.zero;

            var hitGo = new GameObject("HitCircle", typeof(RectTransform), typeof(Image));
            var hitRect = hitGo.GetComponent<RectTransform>();
            hitRect.SetParent(canvasGo.transform, false);
            hitRect.anchorMin = Vector2.zero;
            hitRect.anchorMax = Vector2.zero;
            hitRect.pivot = new Vector2(0.5f, 0.5f);
            hitRect.sizeDelta = new Vector2(NoteDiameter * 1.45f, NoteDiameter * 1.45f);
            hitRect.anchoredPosition = new Vector2(_hitCircleX, _hitCircleY);
            _hitCircle = hitGo.GetComponent<Image>();
            _hitCircle.sprite = GetRingSprite();
            _hitCircle.raycastTarget = false;
            _hitCircle.color = GrayWhite;
        }

        private void SetupAudio()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();

            AudioClip clip = ResolveRealClip();

            if (clip == null)
            {
                if (allowPlaceholderWhenMissing)
                {
                    _usingPlaceholderClip = true;
                    clip = GetPlaceholderClip(fallbackSongLength, tempoBpm);
                    Debug.LogWarning($"[RhythmGame] Real song missing; allowPlaceholderWhenMissing=true -> using procedural placeholder ({fallbackSongLength:F0}s). This is NOT the real song.");
                }
                else
                {
                    Debug.LogError($"[ConfessionNightModule] Real mp3 not found: Resources/{resourceClipPath} (placeholder disabled). Put the real song at Assets/Resources/Audio/Ayasa_Confession_Night.mp3. Rhythm-mode audio will NOT start (no placeholder boop).");
                    _usingPlaceholderClip = false;
                    _effectiveSongLength = fallbackSongLength;
                    _audioSource.clip = null;
                    _audioSource.loop = true;
                    _audioSource.playOnAwake = false;
                    _audioSource.volume = volume;
                    _audioSource.spatialBlend = 0f;
                    return;
                }
            }
            else
            {
                _usingPlaceholderClip = false;
            }

            _audioSource.clip = clip;
            _effectiveSongLength = clip.length;
            _audioSource.loop = true;
            _audioSource.playOnAwake = false;
            _audioSource.volume = volume;
            _audioSource.spatialBlend = 0f;

            Debug.Log($"[RhythmGame] Using AudioClip: {clip.name}, length={clip.length:F1}, frequency={clip.frequency}, channels={clip.channels}, placeholder={_usingPlaceholderClip}");
        }

        private AudioClip ResolveRealClip()
        {
            if (confessionNightClip != null)
                return confessionNightClip;

            var clip = Resources.Load<AudioClip>(resourceClipPath);
            if (clip != null)
            {
                confessionNightClip = clip;
                return clip;
            }
            return null;
        }

        private static AudioClip GetPlaceholderClip(float length, float bpm)
        {
            if (_placeholderCache != null && Mathf.Abs(_placeholderCache.length - length) < 0.05f)
                return _placeholderCache;

            int freq = 11025;
            int samples = Mathf.Max(1, Mathf.RoundToInt(length * freq));
            var data = new float[samples];
            float beat = 60f / bpm;
            int beatSamples = Mathf.Max(1, Mathf.RoundToInt(beat * freq));
            int clickLen = Mathf.RoundToInt(0.04f * freq);
            for (int b = 0; b * beatSamples < samples; b++)
            {
                int s0 = b * beatSamples;
                bool downbeat = (b % 4) == 0;
                float tone = downbeat ? 660f : 880f;
                float amp = downbeat ? 0.18f : 0.12f;
                for (int i = 0; i < clickLen && (s0 + i) < samples; i++)
                {
                    float env = 1f - (i / (float)clickLen);
                    data[s0 + i] = Mathf.Sin(2f * Mathf.PI * tone * (i / (float)freq)) * amp * env;
                }
            }

            var clip = AudioClip.Create("Ayasa_Confession_Night_Placeholder", samples, 1, freq, false);
            clip.SetData(data, 0);
            _placeholderCache = clip;
            return clip;
        }

        private void FlashHitCircle(Color c)
        {
            if (_hitCircle == null) return;
            _hitCircle.color = c;
            _hitFlashEndTime = Time.time + 0.12f;
        }

        private void UpdateHitCircleFlash()
        {
            if (_hitCircle == null) return;
            if (_hitFlashEndTime > 0f && Time.time >= _hitFlashEndTime)
            {
                _hitCircle.color = GrayWhite;
                _hitFlashEndTime = 0f;
            }
        }

        private static Sprite GetDiscSprite()
        {
            if (_discSprite != null) return _discSprite;
            _discSprite = MakeCircleSprite(64, false);
            return _discSprite;
        }

        private static Sprite GetRingSprite()
        {
            if (_ringSprite != null) return _ringSprite;
            _ringSprite = MakeCircleSprite(96, true);
            return _ringSprite;
        }

        private static Sprite MakeCircleSprite(int size, bool ring)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            Vector2 c = new Vector2(size / 2f, size / 2f);
            float outer = size / 2f - 1f;
            float inner = outer * 0.72f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), c);
                    float a;
                    if (ring)
                        a = (d <= outer && d >= inner) ? Mathf.Clamp01(Mathf.Min(outer - d, d - inner) + 1f) : 0f;
                    else
                        a = Mathf.Clamp01(outer - d);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
