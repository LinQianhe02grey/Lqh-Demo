using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace OrbitBreaker.Editor
{
    [InitializeOnLoad]
    public static class OrbitBreakerT03Validation
    {
        private const string Prefix = "OrbitBreaker.T03.";
        private static IEnumerator<Func<bool>> steps;
        private static Func<bool> waiting;
        private static OrbitBreakerPlayerMotor motor;
        private static OrbitBreakerPlayerDash dash;
        private static OrbitBreakerPlayerFeedback feedback;
        private static Rigidbody body;
        private static OrbitBreakerGameManager manager;
        private static int starts, impacts;
        private static bool lastImpactWasDash, flashAtImpact;
        private static Vector3 startVelocity;
        private static Vector2 startDirection;
        private static float startCooldown;
        private static float originalMaximumDeltaTime;

        static OrbitBreakerT03Validation()
        {
            EditorApplication.update += CheckTimeout;
            EditorApplication.playModeStateChanged += OnPlayState;
        }

        [MenuItem("Tools/Orbit Breaker/Validate T03")]
        public static void Run() => Begin("physics");

        [MenuItem("Tools/Orbit Breaker/Check T03 Keyboard")]
        public static void Keyboard() => Begin("keyboard");

        private static void Begin(string mode)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Run T03 validation from idle Edit mode.");
            OrbitBreakerSceneBuilder.Build();
            SessionState.SetString(Prefix + "Mode", mode);
            SessionState.SetString(Prefix + "Checks", "");
            SessionState.SetString(Prefix + "Deadline", DateTime.UtcNow.AddMinutes(mode == "keyboard" ? 6 : 2).ToString("O"));
            SessionState.SetBool(Prefix + "Running", true);
            SessionState.SetBool(Prefix + "Complete", false);
            Report("RUNNING", "Entering Play mode.");
            EditorApplication.isPlaying = true;
        }

        private static void CheckTimeout()
        {
            if (!SessionState.GetBool(Prefix + "Running", false) || SessionState.GetBool(Prefix + "Complete", false)) return;
            if (DateTime.UtcNow <= DateTime.Parse(SessionState.GetString(Prefix + "Deadline", ""), null,
                System.Globalization.DateTimeStyles.RoundtripKind)) return;
            Report("FAIL", "T03 validation timed out.");
            SessionState.SetBool(Prefix + "Running", false);
            EditorApplication.isPlaying = false;
        }

        internal static void Tick()
        {
            if (!SessionState.GetBool(Prefix + "Running", false) || SessionState.GetBool(Prefix + "Complete", false)) return;
            try
            {
                if (DateTime.UtcNow > DateTime.Parse(SessionState.GetString(Prefix + "Deadline", ""), null,
                    System.Globalization.DateTimeStyles.RoundtripKind)) throw new TimeoutException("T03 validation timed out.");
                if (!EditorApplication.isPlaying || EditorApplication.isCompiling || steps == null) return;
                if (waiting != null && !waiting()) return;
                if (steps.MoveNext()) waiting = steps.Current;
                else
                {
                    SessionState.SetBool(Prefix + "Complete", true);
                    Report("PASS", "All requested checks passed; exiting Play.");
                    EditorApplication.isPlaying = false;
                }
            }
            catch (Exception exception)
            {
                Report("FAIL", exception.ToString());
                SessionState.SetBool(Prefix + "Running", false);
                Debug.LogError("[OrbitBreaker] T03 validation failed: " + exception);
                EditorApplication.isPlaying = false;
            }
        }

        private static void Bind(bool keyboard)
        {
            // Keep this fixed fixture independent of random spawns; T06 covers enemy integration.
            var spawner = UnityEngine.Object.FindObjectOfType<OrbitBreakerEnemySpawner>();
            if (spawner != null) spawner.enabled = false;
            motor = UnityEngine.Object.FindObjectOfType<OrbitBreakerPlayerMotor>();
            manager = motor.OrbitBreakerGameManager;
            body = motor.GetComponent<Rigidbody>();
            dash = motor.GetComponent<OrbitBreakerPlayerDash>();
            feedback = motor.GetComponent<OrbitBreakerPlayerFeedback>();
            Check(dash != null && feedback != null, "Dash and feedback components persisted in OrbitBreakerBattle");
            if (!keyboard) motor.GetComponent<OrbitBreakerPlayerInput>().enabled = false;
            motor.DashStarted += () =>
            {
                starts++;
                startVelocity = body.velocity;
                startDirection = motor.DashDirection;
                startCooldown = dash.CooldownRemaining;
            };
            motor.Impact += (collision, wasDash) =>
            {
                impacts++;
                lastImpactWasDash = wasDash;
                flashAtImpact = feedback.IsImpactFlashing;
            };
        }

        private static IEnumerator<Func<bool>> PhysicsChecks()
        {
            Bind(false);
            Check(manager.State == GameState.Ready && !dash.TryDash(), "Ready rejects dash");
            manager.StartGame();
            Arrange(Vector3.zero, Vector2.zero);
            Check(dash.TryDash() && !dash.TryDash(), "One pending request; duplicate requests rejected");
            yield return Delay(0.06f);
            Check(starts == 1 && startDirection == Vector2.up && body.position.y > 0.4f,
                "No input initially dashes up with real displacement");
            Check(Mathf.Abs(startVelocity.magnitude - Mathf.Min(manager.Config.dashSpeed, OrbitBreakerPlayerMotor.MaximumSpeed)) < 0.01f &&
                Mathf.Abs(startCooldown - manager.Config.dashCooldown) < 0.01f,
                $"Configured launch speed={startVelocity.magnitude:F2}, cooldown={startCooldown:F2}");
            Check(motor.IsDashing && body.velocity.y > 16f && !dash.TryDash(),
                $"Active dash survives physics steps and rejects repeat: active={motor.IsDashing}, vy={body.velocity.y:F2}, cooldown={dash.CooldownRemaining:F2}");
            CheckColor(new Color(0.25f, 1f, 1f), "Dash renders cyan without changing shared material");
            ScreenCapture.CaptureScreenshot(EvidencePath("T03-dash.png"));
            float velocity = body.velocity.y;
            float observed = Time.time;
            motor.SetMoveInput(Vector2.down);
            yield return Delay(0.06f);
            float expected = velocity - manager.Config.gravityStrength * (Time.time - observed);
            Check(motor.IsDashing && Mathf.Abs(body.velocity.y - expected) < 0.5f,
                $"Opposite steering does not overwrite dash; gravity remains active vy={body.velocity.y:F2}");
            yield return Delay(0.12f);
            Check(!motor.IsDashing && dash.CooldownRemaining > 0f && !dash.TryDash(), "Dash ends before cooldown; early retry rejected");
            Check(!body.GetComponent<MeshRenderer>().HasPropertyBlock(), "Feedback restores original appearance after dash");
            velocity = body.velocity.y;
            yield return Delay(0.1f);
            Check(body.velocity.y < velocity - 2.5f, "Normal steering resumes after dash window");

            var directions = new[] { Vector2.right, Vector2.left, Vector2.down, new Vector2(1f, 1f).normalized };
            foreach (var direction in directions)
            {
                yield return () => dash.CanDash;
                Arrange(new Vector3(0f, 2f, 0f), direction);
                int before = starts;
                Check(dash.TryDash(), "Cooldown expiry accepts direction " + direction);
                yield return Delay(0.05f);
                Check(starts == before + 1 && Vector2.Distance(startDirection, direction) < 0.001f &&
                    Vector3.Distance(startVelocity, (Vector3)direction * manager.Config.dashSpeed) < 0.01f,
                    "Normalized directional launch " + startVelocity);
                Check(Mathf.Abs(body.position.z) < 0.001f, "Dash remains on board plane");
                yield return Delay(0.2f);
                Arrange(new Vector3(0f, 2f, 0f), Vector2.zero);
            }
            yield return () => dash.CanDash;
            Arrange(new Vector3(0f, 2f, 0f), Vector2.left);
            motor.SetMoveInput(Vector2.zero);
            Check(dash.TryDash(), "No-input request accepted using remembered direction");
            motor.SetMoveInput(Vector2.right);
            yield return Delay(0.05f);
            Check(startDirection == Vector2.left && body.velocity.x < -17f,
                "Last direction remembered and request direction captured before next physics step");

            yield return () => dash.CanDash;
            Arrange(new Vector3(6.3f, 2f, 0f), Vector2.right);
            int impactBefore = impacts;
            Check(dash.TryDash(), "Wall dash accepted");
            yield return () => impacts > impactBefore;
            Check(lastImpactWasDash && flashAtImpact && !motor.IsDashing && body.position.x <= 7.02f,
                "Real wall collision stops dash, preserves dash-hit qualification and triggers flash");
            yield return Delay(0.02f);
            CheckColor(new Color(1f, 0.65f, 0.12f), "Impact renders warm flash");
            ScreenCapture.CaptureScreenshot(EvidencePath("T03-impact.png"));
            yield return Delay(0.3f);
            Check(impacts == impactBefore + 1 && !feedback.IsImpactFlashing, "Sustained wall contact does not repeatedly flash");
            Check(!body.GetComponent<MeshRenderer>().HasPropertyBlock(), "Impact color restores without material mutation");

            yield return () => dash.CanDash;
            Arrange(new Vector3(6.3f, 2f, 0f), Vector2.zero);
            // Move off the previous contact before approaching again.
            yield return Delay(0.04f);
            impactBefore = impacts;
            body.velocity = Vector3.right * 8f;
            yield return () => impacts > impactBefore;
            Check(!lastImpactWasDash && flashAtImpact, "Ordinary fast collision also produces feedback");

            yield return () => dash.CanDash;
            Arrange(Vector3.zero, Vector2.up);
            int startBefore = starts;
            Check(dash.TryDash(), "Pending cancellation fixture accepted");
            dash.enabled = false;
            yield return Delay(0.06f);
            Check(starts == startBefore && !motor.IsDashing, "Disabling dash cancels queued request");
            dash.enabled = true;
            Check(dash.TryDash() && manager.LoseGame(), "Queued dash followed by Defeat");
            yield return Delay(0.05f);
            Check(starts == startBefore && !motor.IsDashing && body.isKinematic && !dash.TryDash(),
                "Defeat cancels queued dash and prevents future launches");
            int oldManager = manager.GetInstanceID();
            manager.RestartGame();
            yield return () => UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>().GetInstanceID() != oldManager;
            Bind(false);
            Check(manager.State == GameState.Ready && dash.CooldownRemaining == 0f && motor.DashDirection == Vector2.up,
                "Real scene reload resets cooldown and default direction");
            manager.StartGame();
            Check(dash.TryDash(), "Dash usable immediately in restarted game");
            yield return Delay(0.05f);
            Check(motor.IsDashing, "Restarted dash active");
            manager.WinGame();
            var frozen = body.position;
            yield return Delay(0.08f);
            Check(!motor.IsDashing && !dash.TryDash() && body.isKinematic && Vector3.Distance(body.position, frozen) < 0.001f,
                "Victory interrupts active dash and freezes motion");
            Check(!body.GetComponent<MeshRenderer>().HasPropertyBlock(), "Terminal state restores blue appearance");
        }

        private static IEnumerator<Func<bool>> KeyboardChecks()
        {
            Bind(true);
            Report("RUNNING", "Focus Game and press Enter, then Space. Waiting for real input.");
            yield return () => manager.IsPlaying;
            Check(true, "Real Enter starts game");
            // Keep an isolated keyboard fixture at spawn until Space arrives; never issue a dash request here.
            yield return () =>
            {
                if (starts > 0) return true;
                body.position = new Vector3(0f, -5f, 0f);
                body.velocity = Vector3.zero;
                return false;
            };
            Check(startDirection == Vector2.up && startVelocity.y > 17f, "Real Space launches default upward dash through OrbitBreakerPlayerInput");
            yield return Delay(0.06f);
            Check(body.position.y > -4.5f, "Real key launch produces visible displacement");
            yield return Delay(0.85f);
            Check(starts == 1, "No second dash without a fresh Space key-down");
        }

        private static void Arrange(Vector3 position, Vector2 input)
        {
            body.position = position;
            body.velocity = Vector3.zero;
            motor.SetMoveInput(input);
        }
        private static Func<bool> Delay(float seconds)
        {
            float end = Time.time + seconds;
            return () => Time.time >= end;
        }
        private static void CheckColor(Color expected, string message)
        {
            var block = new MaterialPropertyBlock();
            body.GetComponent<MeshRenderer>().GetPropertyBlock(block);
            var actual = block.GetColor("_Color");
            Check(Vector4.Distance(actual, expected) < 0.01f, message + "; color=" + actual);
        }
        private static void Check(bool pass, string message)
        {
            if (!pass) throw new InvalidOperationException(message);
            SessionState.SetString(Prefix + "Checks", SessionState.GetString(Prefix + "Checks", "") + message + "\n");
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
                starts = impacts = 0;
                waiting = null;
                steps = SessionState.GetString(Prefix + "Mode", "") == "keyboard" ? KeyboardChecks() : PhysicsChecks();
                originalMaximumDeltaTime = Time.maximumDeltaTime;
                // Bound only this test session's simulation catch-up so brief feedback can be sampled.
                Time.maximumDeltaTime = Time.fixedDeltaTime;
                var probe = new GameObject("T03_ValidationProbe");
                probe.hideFlags = HideFlags.DontSave;
                UnityEngine.Object.DontDestroyOnLoad(probe);
                probe.AddComponent<OrbitBreaker.Testing.OrbitBreakerT03Probe>().Observe = Tick;
            }
            if (state != PlayModeStateChange.EnteredEditMode) return;
            if (!SessionState.GetBool(Prefix + "Complete", false)) Report("FAIL", "Play mode exited early.");
            else Debug.Log("[OrbitBreaker] T03 validation PASS.");
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
            File.WriteAllText(EvidencePath("T03-" + SessionState.GetString(Prefix + "Mode", "physics") + ".json"),
                JsonUtility.ToJson(new Result { status = status, observedAtUtc = DateTime.UtcNow.ToString("O"),
                    unityVersion = Application.unityVersion, detail = detail,
                    checks = SessionState.GetString(Prefix + "Checks", "").Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries) }, true));
        }
    }
}

