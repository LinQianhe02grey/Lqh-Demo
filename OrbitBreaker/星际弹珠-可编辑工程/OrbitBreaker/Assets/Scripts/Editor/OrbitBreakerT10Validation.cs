using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OrbitBreaker.Editor
{
    [InitializeOnLoad]
    public static class OrbitBreakerT10Validation
    {
        private const string Prefix = "OrbitBreaker.T10.";
        private static IEnumerator<Func<bool>> steps;
        private static Func<bool> waiting;
        private static OrbitBreakerPlayerMotor motor;
        private static OrbitBreakerGameManager manager;
        private static OrbitBreakerScoreManager score;
        private static float originalMaximumDeltaTime;
        private static readonly List<string> checks = new List<string>();
        static OrbitBreakerT10Validation() { EditorApplication.playModeStateChanged += OnPlayState; EditorApplication.update += Timeout; }
        [MenuItem("Tools/Orbit Breaker/Validate T10")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Start from Edit mode.");
            OrbitBreakerSceneBuilder.Build();
            SessionState.SetBool(Prefix + "Running", true);
            SessionState.SetBool(Prefix + "Complete", false);
            SessionState.SetString(Prefix + "Deadline", DateTime.UtcNow.AddMinutes(4).ToString("O"));
            checks.Clear();
            Report("RUNNING", "Entering Play.");
            EditorApplication.isPlaying = true;
        }
        private static void Bind()
        {
            motor = UnityEngine.Object.FindObjectOfType<OrbitBreakerPlayerMotor>();
            manager = motor.OrbitBreakerGameManager;
            score = UnityEngine.Object.FindObjectOfType<OrbitBreakerScoreManager>();
            motor.GetComponent<OrbitBreakerPlayerInput>().enabled = false;
            Check(manager.State == GameState.Ready && score.Score == 0 && manager.Config.attachDuration == 7.5f,
                "Real reload starts Ready, score zero, production attachment 7.5 seconds");
        }
        private static IEnumerator<Func<bool>> RunChecks()
        {
            Bind();
            var spawner = UnityEngine.Object.FindObjectOfType<OrbitBreakerEnemySpawner>();
            Check(spawner.enabled && spawner.AliveCount == 0, "Saved automatic spawner starts enabled and empty");
            spawner.enabled = false; // Isolated VFX/time-limit check only. Full rounds reload all defaults below.
            manager.StartGame();
            var point = UnityEngine.Object.FindObjectsOfType<OrbitBreakerAttachmentPoint>().OrderBy(p => p.Center.x).First();
            var body = motor.GetComponent<Rigidbody>();
            body.position = (Vector3)point.Center + Vector3.down * OrbitBreakerAttachmentPoint.OrbitRadius;
            body.velocity = Vector3.zero;
            motor.SetAttachmentAim(point.Center + Vector2.down * 4f);
            Check(motor.TryAttach(point), "Isolated VFX fixture accepts attachment");
            var feedback = motor.GetComponent<OrbitBreakerPlayerFeedback>();
            float began = Time.time;
            bool captured = false;
            yield return () =>
            {
                float age = Time.time - began;
                float angle = (-90f + age * 270f) * Mathf.Deg2Rad;
                motor.SetAttachmentAim(point.Center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 4f);
                if (age > 2f && !captured)
                {
                    Check(motor.IsAttached && motor.AttachmentCharge >= 0.99f && feedback.OrbitVfxActive && feedback.OrbitTrailPoints > 3,
                        "After old 1.5s limit, actual orbit continues charged with visible arc and moving trail");
                    ScreenCapture.CaptureScreenshot(Evidence("T10-orbit-vfx.png"));
                    captured = true;
                }
                if (age < 7.35f) Check(motor.IsAttached, "");
                return !motor.IsAttached;
            };
            Check(Time.time - began >= 7.45f && Time.time - began < 7.7f, "Production timeout is 7.5 seconds, exactly five times previous limit");
            yield return Delay(0.04f);
            Check(!feedback.OrbitVfxActive && !feedback.OrbitTrailVisible && feedback.OrbitTrailPoints <= 1, "Timeout disables and clears arc/trail (at most one non-rendering anchor)");
            manager.LoseGame(); // End the isolated fixture before requesting the guarded real reload.
            int previous = manager.GetInstanceID();
            manager.RestartGame();
            yield return () => UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>().GetInstanceID() != previous;
            Bind();
            Check(UnityEngine.Object.FindObjectsOfType<TrailRenderer>().Length == 0, "Restart removes all runtime orbit VFX");
            spawner = UnityEngine.Object.FindObjectOfType<OrbitBreakerEnemySpawner>();
            Check(spawner.enabled, "Integrated win round retains normal automatic enemy spawning");
            manager.StartGame();
            float roundStart = Time.time;
            bool retreat = false;
            int lastScore = 0, scoreEvents = 0, spawns = 0, hits = 0, dashCount = 0;
            score.ScoreChanged += total => { scoreEvents++; retreat = true; lastScore = total; };
            motor.HitReceived += () => hits++;
            motor.DashStarted += () => dashCount++;
            // End-to-end driver sends the same movement/dash requests as input. No body writes,
            // score calls, spawn disabling, scene rearrangement or direct WinGame calls in this round.
            yield return () =>
            {
                if (!manager.IsPlaying) return true;
                spawns = Mathf.Max(spawns, spawner.AliveCount);
                Vector2 p = motor.Position;
                Vector2 v = motor.GetComponent<Rigidbody>().velocity;
                if (retreat && p.y < 4.6f) retreat = false;
                float x = Mathf.Clamp(-p.x * 1.8f - v.x * 0.45f, -0.7f, 0.7f);
                motor.SetMoveInput(new Vector2(x, retreat ? -1f : 1f));
                if (!retreat && !motor.IsStunned && motor.DashCooldownRemaining <= 0f && p.y < 6.5f)
                {
                    motor.SetMoveInput(new Vector2(Mathf.Clamp(-p.x * 0.25f, -0.4f, 0.4f), 1f));
                    motor.TryRequestDash();
                }
                if (Time.time - roundStart > 100f) throw new InvalidOperationException("Continuous win round timed out, score=" + lastScore + ", position=" + p);
                return false;
            };
            Check(manager.State == GameState.Victory && score.Score >= manager.Config.targetScore && scoreEvents >= 5 && spawns > 0,
                "Unrearranged full round reaches actual scoring Victory with automatic enemies; seconds=" + (Time.time-roundStart).ToString("F2") +
                ", score=" + score.Score + ", scoreEvents=" + scoreEvents + ", maxEnemies=" + spawns + ", hits=" + hits + ", dashes=" + dashCount);
            Check(motor.GetComponent<Rigidbody>().isKinematic && !motor.IsAttached && !motor.GetComponent<OrbitBreakerPlayerFeedback>().OrbitVfxActive,
                "Victory freezes player and clears orbit VFX");
            ScreenCapture.CaptureScreenshot(Evidence("T10-full-victory.png"));
            yield return Delay(0.06f);
            previous = manager.GetInstanceID();
            manager.RestartGame();
            yield return () => UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>().GetInstanceID() != previous;
            Bind();
            Check(UnityEngine.Object.FindObjectOfType<OrbitBreakerEnemySpawner>().enabled && UnityEngine.Object.FindObjectsOfType<OrbitBreakerEnemyController>().Length == 0,
                "Victory restart resets full scene and enabled spawner without leftover enemies");
            manager.StartGame();
            roundStart = Time.time;
            motor.SetMoveInput(Vector2.zero);
            yield return () => !manager.IsPlaying;
            Check(manager.State == GameState.Defeat && score.Score == 0,
                "Unrearranged no-input full round naturally drains from saved spawn; seconds=" + (Time.time-roundStart).ToString("F2"));
            ScreenCapture.CaptureScreenshot(Evidence("T10-full-defeat.png"));
            yield return Delay(0.06f);
            previous = manager.GetInstanceID();
            manager.RestartGame();
            yield return () => UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>().GetInstanceID() != previous;
            Bind();
            Check(!motor.IsStunned && !motor.IsAttached && motor.DashCooldownRemaining == 0f,
                "Defeat restart clears all states and retains 7.5-second production configuration");
        }
        private static Func<bool> Delay(float duration) { float end = Time.time + duration; return () => Time.time >= end; }
        private static void Tick()
        {
            try
            {
                if (waiting != null && !waiting()) return;
                if (steps.MoveNext()) waiting = steps.Current;
                else { SessionState.SetBool(Prefix + "Complete", true); Report("PASS", "All T10 integration checks passed."); EditorApplication.isPlaying = false; }
            }
            catch (Exception ex) { Fail(ex.ToString()); }
        }
        private static void Check(bool pass, string detail) { if (!pass) throw new InvalidOperationException(detail.Length > 0 ? detail : "Attachment expired before 7.35 seconds."); if (detail.Length > 0) checks.Add(detail); }
        private static void Fail(string detail) { Report("FAIL", detail); SessionState.SetBool(Prefix+"Running", false); Debug.LogError("T10: " + detail); EditorApplication.isPlaying = false; }
        private static void Timeout() { if (SessionState.GetBool(Prefix+"Running", false) && DateTime.UtcNow > DateTime.Parse(SessionState.GetString(Prefix+"Deadline", ""), null, System.Globalization.DateTimeStyles.RoundtripKind)) Fail("T10 deadline exceeded."); }
        private static void OnPlayState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode && originalMaximumDeltaTime > 0f) { Time.maximumDeltaTime = originalMaximumDeltaTime; originalMaximumDeltaTime = 0f; }
            if (!SessionState.GetBool(Prefix+"Running", false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                checks.Clear(); waiting = null; steps = RunChecks();
                originalMaximumDeltaTime = Time.maximumDeltaTime; Time.maximumDeltaTime = Time.fixedDeltaTime;
                var probe = new GameObject("T10_ValidationProbe") { hideFlags=HideFlags.DontSave };
                UnityEngine.Object.DontDestroyOnLoad(probe);
                probe.AddComponent<OrbitBreaker.Testing.OrbitBreakerT03Probe>().Observe = Tick;
            }
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                if (!SessionState.GetBool(Prefix+"Complete", false)) Report("FAIL", "Play exited before completion.");
                SessionState.SetBool(Prefix+"Running", false); steps?.Dispose(); steps=null;
            }
        }
        private static string Evidence(string file) => Path.Combine(Path.GetDirectoryName(Application.dataPath), "Docs/EVIDENCE", file);
        [Serializable] private sealed class Result { public string status, observedAtUtc, unityVersion, detail; public string[] checks; }
        private static void Report(string status, string detail) => File.WriteAllText(Evidence("T10-integration.json"), JsonUtility.ToJson(new Result {status=status, observedAtUtc=DateTime.UtcNow.ToString("O"), unityVersion=Application.unityVersion, detail=detail, checks=checks.ToArray()}, true));
    }
}
