using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OrbitBreaker.Editor
{
    [InitializeOnLoad]
    public static class OrbitBreakerT05Validation
    {
        private const string Prefix = "OrbitBreaker.T05.";
        private static IEnumerator<Func<bool>> steps;
        private static Func<bool> waiting;
        private static OrbitBreakerPlayerMotor motor;
        private static Rigidbody body;
        private static OrbitBreakerGameManager manager;
        private static OrbitBreakerScoreManager score;
        private static OrbitBreakerScoreTarget[] targets;
        private static int scoreEvents;
        private static float originalMaximumDeltaTime;

        static OrbitBreakerT05Validation()
        {
            EditorApplication.update += CheckTimeout;
            EditorApplication.playModeStateChanged += OnPlayState;
        }
        [MenuItem("Tools/Orbit Breaker/Validate T05")]
        public static void Run() => Begin("physics");
        [MenuItem("Tools/Orbit Breaker/Check T05 Keyboard")]
        public static void Keyboard() => Begin("keyboard");

        private static void Begin(string mode)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Start T05 from idle Edit mode.");
            OrbitBreakerSceneBuilder.Build();
            SessionState.SetString(Prefix + "Mode", mode);
            SessionState.SetString(Prefix + "Checks", "");
            SessionState.SetString(Prefix + "Deadline", DateTime.UtcNow.AddMinutes(mode == "keyboard" ? 6 : 3).ToString("O"));
            SessionState.SetBool(Prefix + "Running", true);
            SessionState.SetBool(Prefix + "Complete", false);
            Report("RUNNING", "Entering Play mode.");
            EditorApplication.isPlaying = true;
        }
        private static void CheckTimeout()
        {
            if (!SessionState.GetBool(Prefix + "Running", false)) return;
            if (DateTime.UtcNow <= DateTime.Parse(SessionState.GetString(Prefix + "Deadline", ""), null,
                System.Globalization.DateTimeStyles.RoundtripKind)) return;
            Fail("T05 timed out. Completed checks remain in the report.");
        }
        private static void Tick()
        {
            if (!SessionState.GetBool(Prefix + "Running", false) || SessionState.GetBool(Prefix + "Complete", false) || steps == null) return;
            try
            {
                if (waiting != null && !waiting()) return;
                if (steps.MoveNext()) waiting = steps.Current;
                else
                {
                    SessionState.SetBool(Prefix + "Complete", true);
                    Report("PASS", "All checks passed; exiting Play.");
                    EditorApplication.isPlaying = false;
                }
            }
            catch (Exception exception) { Fail(exception.ToString()); }
        }
        private static void Fail(string detail)
        {
            Report("FAIL", detail);
            SessionState.SetBool(Prefix + "Running", false);
            Debug.LogError("[OrbitBreaker] T05 validation failed: " + detail);
            EditorApplication.isPlaying = false;
        }
        private static void Bind(bool keyboard)
        {
            // Keep this fixed fixture independent of random spawns; T06 covers enemy integration.
            var spawner = UnityEngine.Object.FindObjectOfType<OrbitBreakerEnemySpawner>();
            if (spawner != null) spawner.enabled = false;
            motor = UnityEngine.Object.FindObjectOfType<OrbitBreakerPlayerMotor>();
            body = motor.GetComponent<Rigidbody>();
            manager = motor.OrbitBreakerGameManager;
            score = UnityEngine.Object.FindObjectOfType<OrbitBreakerScoreManager>();
            targets = UnityEngine.Object.FindObjectsOfType<OrbitBreakerScoreTarget>().OrderBy(t => t.Points).ThenBy(t => t.transform.position.x).ToArray();
            if (!keyboard) motor.GetComponent<OrbitBreakerPlayerInput>().enabled = false;
            Check(targets.Length == 3 && targets[0].Points == 50 && targets[1].Points == 50 && targets[2].Points == 100,
                "Two ordinary 50-point targets and one 100-point target persist");
            Check(score != null && score.OrbitBreakerGameManager == manager && UnityEngine.Object.FindObjectsOfType<OrbitBreakerScoreDisplay>().Length == 1,
                "OrbitBreakerScoreManager and minimal display references persist");
            score.ScoreChanged += value => scoreEvents++;
        }
        private static IEnumerator<Func<bool>> PhysicsChecks()
        {
            Bind(false);
            Check(score.Score == 0 && score.TargetScore == manager.Config.targetScore, "Ready starts at zero and reads configured target score");
            Check(!score.TryAward(50, targets[0], 1), "Ready rejects scoring");
            manager.StartGame();
            Check(!score.TryAward(0, targets[0], 1) && !score.TryAward(-1, targets[0], 1) &&
                !score.TryAward(50, null, 1) && !score.TryAward(50, targets[0], 0), "Invalid awards rejected");
            var target = targets[0];
            var center = target.transform.position;
            Arrange(center + Vector3.up * 0.9f, Vector3.down * 10f);
            yield return Delay(0.15f);
            Check(score.Score == 0 && target.LastIncomingVelocity.y < -6f,
                "Falling contact rejected using incoming velocity " + target.LastIncomingVelocity);
            yield return Delay(0.6f);
            Check(score.Score == 0 && target.ContactCount > 0, "Resting on target never farms points");
            Arrange(new Vector3(0f, -2f, 0f), Vector3.zero);
            yield return Delay(0.06f);
            Arrange(center + Vector3.down * 0.85f, Vector3.up * 4f);
            yield return Delay(0.15f);
            Check(score.Score == 0 && target.LastIncomingVelocity.y > 0f && target.LastIncomingVelocity.y < 6f,
                "Slow upward contact below threshold rejected: " + target.LastIncomingVelocity);
            Arrange(new Vector3(0f, -2f, 0f), Vector3.zero);
            yield return Delay(0.06f);
            Arrange(center + Vector3.left * 1.6f, Vector3.right * 10f);
            yield return Delay(0.15f);
            Check(score.Score == 0 && target.LastIncomingVelocity.x > 6f && target.LastIncomingVelocity.y < 6f,
                "Fast horizontal contact does not count as upward hit");
            Arrange(new Vector3(0f, -2f, 0f), Vector3.zero);
            yield return Delay(0.06f);
            var probe = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            probe.name = "T05_NonPlayerProbe";
            probe.transform.position = center + Vector3.down * 0.9f;
            var probeBody = probe.AddComponent<Rigidbody>();
            probeBody.useGravity = false;
            probeBody.velocity = Vector3.up * 10f;
            yield return Delay(0.15f);
            Check(score.Score == 0, "Non-player sphere cannot score");
            UnityEngine.Object.Destroy(probe);
            yield return Delay(0.04f);

            Arrange(center + Vector3.down * 0.9f, Vector3.up * 10f);
            yield return () => score.Score > 0;
            Check(score.Score == 50 && scoreEvents == 1 && target.LastIncomingVelocity.y >= OrbitBreakerScoreTarget.MinimumUpwardSpeed && body.velocity.y < 1f,
                $"Real upward impact awards 50 using pre-solver vy={target.LastIncomingVelocity.y:F2}; post-solver vy={body.velocity.y:F2}");
            Check(target.GetComponent<Renderer>().HasPropertyBlock(), "Successful hit flashes target without shared material mutation");
            Check(!score.TryAward(50, target, target.InteractionId) && !score.TryAward(50, target, target.InteractionId - 1),
                "Duplicate and stale interaction IDs rejected at score entry");
            ScreenCapture.CaptureScreenshot(EvidencePath("T05-hit.png"));
            yield return Delay(0.02f);
            motor.SetMoveInput(Vector2.up);
            yield return Delay(0.7f);
            Check(score.Score == 50 && target.ContactCount > 0, "Continuous upward pressure does not score again after cooldown");
            Check(!target.GetComponent<Renderer>().HasPropertyBlock(), "Target flash restores original appearance");
            body.velocity = Vector3.up * 12f;
            yield return Delay(0.08f);
            Check(score.Score == 50, "Changing velocity during same contact cannot farm points");
            Arrange(new Vector3(0f, -2f, 0f), Vector3.zero);
            yield return Delay(0.06f);
            Arrange(center + Vector3.down * 0.9f, Vector3.up * 10f);
            yield return () => score.Score == 100;
            Check(true, "Leaving and valid new hit after cooldown awards a second 50");
            Arrange(new Vector3(0f, -2f, 0f), Vector3.zero);
            yield return Delay(0.06f);
            Arrange(center + Vector3.down * 0.9f, Vector3.up * 10f);
            yield return Delay(0.12f);
            motor.SetMoveInput(Vector2.up);
            yield return Delay(0.6f);
            Check(score.Score == 100, "Rapid re-entry rejected; staying past cooldown cannot score retroactively");
            foreach (var step in ReachVictory()) yield return step;
            Check(score.Score >= score.TargetScore && manager.State == GameState.Victory && body.isKinematic,
                "Real high-target contacts reach configured score and freeze in Victory");
            int terminalScore = score.Score;
            Check(!score.TryAward(100, targets[2], targets[2].InteractionId + 1), "Victory rejects future awards");
            yield return Delay(0.6f);
            Check(score.Score == terminalScore, "Victory score remains frozen");
            ScreenCapture.CaptureScreenshot(EvidencePath("T05-victory.png"));
            yield return Delay(0.02f);
            int previous = manager.GetInstanceID();
            Check(manager.RestartGame(), "Victory requests real scene reload");
            yield return () => UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>().GetInstanceID() != previous;
            Bind(false);
            CheckReset();
            manager.StartGame();
            Arrange(targets[1].transform.position + Vector3.down * 0.9f, Vector3.up * 10f);
            yield return () => score.Score == 50;
            Check(true, "Right target works immediately after complete reload");
            Arrange(new Vector3(0f, -9f, 0f), Vector3.down * 10f);
            yield return () => manager.State == GameState.Defeat;
            Check(score.Score == 50 && !score.TryAward(50, targets[1], targets[1].InteractionId + 1), "Drain Defeat preserves displayed score and rejects awards");
            previous = manager.GetInstanceID();
            manager.RestartGame();
            yield return () => UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>().GetInstanceID() != previous;
            Bind(false);
            CheckReset();
        }
        private static IEnumerable<Func<bool>> ReachVictory()
        {
            while (manager.IsPlaying)
            {
                Arrange(new Vector3(0f, -2f, 0f), Vector3.zero);
                yield return Delay(0.56f);
                int before = score.Score;
                Arrange(targets[2].transform.position + Vector3.down * 0.9f, Vector3.up * 10f);
                yield return () => score.Score > before;
                Check(score.Score == before + 100, "High target awards exactly 100 on fresh upward contact");
            }
        }
        private static void CheckReset()
        {
            Check(manager.State == GameState.Ready && score.Score == 0 && body.isKinematic &&
                Vector3.Distance(body.position, new Vector3(0f, -5f, 0f)) < 0.01f && !motor.IsDashing && motor.DashCooldownRemaining == 0f &&
                targets.All(t => t.ContactCount == 0 && t.CooldownRemaining == 0f && t.InteractionId == 0) &&
                UnityEngine.Object.FindObjectsOfType<OrbitBreakerFlipperController>().All(f => f.CooldownRemaining == 0f),
                "Reload resets score/contact IDs/target and flipper cooldowns/player and dash to Ready");
        }
        private static IEnumerator<Func<bool>> KeyboardChecks()
        {
            Bind(true);
            Report("RUNNING", "Focus Game and press Enter, then Space; target fixture waits for real dash.");
            yield return () => manager.IsPlaying;
            Check(true, "Real Enter starts round");
            bool dashed = false;
            motor.DashStarted += () => dashed = true;
            yield return () =>
            {
                if (dashed) return true;
                Arrange(targets[2].transform.position + Vector3.down * 1.1f, Vector3.zero);
                return false;
            };
            yield return () => score.Score == 100;
            Check(true, "Real Space drives actual upward target collision for 100 points");
            foreach (var step in ReachVictory()) yield return step;
            Check(manager.State == GameState.Victory, "Subsequent physical target hits complete Victory without direct score injection");
            ScreenCapture.CaptureScreenshot(EvidencePath("T05-keyboard-victory.png"));
            Report("RUNNING", "Victory achieved. Press R to restart.");
            int previous = manager.GetInstanceID();
            yield return () => UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>().GetInstanceID() != previous;
            Bind(true);
            CheckReset();
            Check(true, "Real R reloads after Victory");
            Report("RUNNING", "Ready again. Press Enter; a physical score and central drain will produce Defeat.");
            yield return () => manager.IsPlaying;
            Arrange(targets[0].transform.position + Vector3.down * 0.9f, Vector3.up * 10f);
            yield return () => score.Score == 50;
            Arrange(new Vector3(0f, -9f, 0f), Vector3.down * 10f);
            yield return () => manager.State == GameState.Defeat;
            Check(score.Score == 50, "Actual central drain produces Defeat with prior score");
            Report("RUNNING", "Ball lost. Press R to verify Defeat restart.");
            previous = manager.GetInstanceID();
            yield return () => UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>().GetInstanceID() != previous;
            Bind(true);
            CheckReset();
            Check(true, "Real R reloads after Defeat");
        }
        private static void Arrange(Vector3 position, Vector3 velocity)
        {
            body.position = position;
            body.velocity = velocity;
            motor.SetMoveInput(Vector2.zero);
        }
        private static Func<bool> Delay(float duration)
        {
            float until = Time.time + duration;
            return () => Time.time >= until;
        }
        private static void Check(bool pass, string detail)
        {
            if (!pass) throw new InvalidOperationException(detail);
            SessionState.SetString(Prefix + "Checks", SessionState.GetString(Prefix + "Checks", "") + detail + "\n");
        }
        private static void OnPlayState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode && originalMaximumDeltaTime > 0f)
            {
                Time.maximumDeltaTime = originalMaximumDeltaTime;
                originalMaximumDeltaTime = 0f;
            }
            if (!SessionState.GetBool(Prefix + "Running", false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                scoreEvents = 0;
                waiting = null;
                steps = SessionState.GetString(Prefix + "Mode", "") == "keyboard" ? KeyboardChecks() : PhysicsChecks();
                originalMaximumDeltaTime = Time.maximumDeltaTime;
                Time.maximumDeltaTime = Time.fixedDeltaTime;
                var observer = new GameObject("T05_ValidationProbe") { hideFlags = HideFlags.DontSave };
                UnityEngine.Object.DontDestroyOnLoad(observer);
                observer.AddComponent<OrbitBreaker.Testing.OrbitBreakerT03Probe>().Observe = Tick;
            }
            if (state != PlayModeStateChange.EnteredEditMode) return;
            if (!SessionState.GetBool(Prefix + "Complete", false)) Report("FAIL", "Play mode exited before completion.");
            else Debug.Log("[OrbitBreaker] T05 validation PASS.");
            SessionState.SetBool(Prefix + "Running", false);
            steps?.Dispose();
            steps = null;
        }
        private static string EvidencePath(string file) => Path.Combine(Path.GetDirectoryName(Application.dataPath), "Docs/EVIDENCE", file);
        [Serializable] private sealed class Result
        {
            public string status, observedAtUtc, unityVersion, detail;
            public string[] checks;
        }
        private static void Report(string status, string detail)
        {
            File.WriteAllText(EvidencePath("T05-" + SessionState.GetString(Prefix + "Mode", "physics") + ".json"),
                JsonUtility.ToJson(new Result { status = status, observedAtUtc = DateTime.UtcNow.ToString("O"),
                    unityVersion = Application.unityVersion, detail = detail,
                    checks = SessionState.GetString(Prefix + "Checks", "").Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries) }, true));
        }
    }
}


