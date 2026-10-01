using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OrbitBreaker.Editor
{
    [InitializeOnLoad]
    public static class OrbitBreakerT04Validation
    {
        private const string Prefix = "OrbitBreaker.T04.";
        private static IEnumerator<Func<bool>> steps;
        private static Func<bool> waiting;
        private static OrbitBreakerPlayerMotor motor;
        private static Rigidbody body;
        private static OrbitBreakerGameManager manager;
        private static OrbitBreakerFlipperController left, right;
        private static OrbitBreakerBumper[] bumpers;
        private static int leftHits, rightHits, bounces, impulses;
        private static float originalMaximumDeltaTime;

        static OrbitBreakerT04Validation()
        {
            EditorApplication.update += CheckTimeout;
            EditorApplication.playModeStateChanged += OnPlayState;
        }
        [MenuItem("Tools/Orbit Breaker/Validate T04")]
        public static void Run() => Begin("physics");
        [MenuItem("Tools/Orbit Breaker/Check T04 Keyboard")]
        public static void Keyboard() => Begin("keyboard");

        private static void Begin(string mode)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Start T04 from idle Edit mode.");
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
            Fail("T04 timed out. Completed checks remain in the report.");
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
            Debug.LogError("[OrbitBreaker] T04 validation failed: " + detail);
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
            if (!keyboard) motor.GetComponent<OrbitBreakerPlayerInput>().enabled = false;
            var flippers = UnityEngine.Object.FindObjectsOfType<OrbitBreakerFlipperController>();
            bumpers = UnityEngine.Object.FindObjectsOfType<OrbitBreakerBumper>().OrderBy(b => b.transform.position.x).ToArray();
            Check(flippers.Length == 2 && bumpers.Length == 2, "Exactly two flippers and two bumpers persisted");
            left = flippers.Single(f => f.Side == FlipperSide.Left);
            right = flippers.Single(f => f.Side == FlipperSide.Right);
            left.Activated += () => leftHits++;
            right.Activated += () => rightHits++;
            foreach (var bumper in bumpers) bumper.Bounced += () => bounces++;
            motor.BoardImpulseApplied += () => impulses++;
        }
        private static IEnumerator<Func<bool>> PhysicsChecks()
        {
            Bind(false);
            Check(!left.TryActivate() && !right.TryActivate(), "Ready rejects flippers");
            manager.StartGame();
            Arrange(Vector3.zero, Vector3.zero);
            Check(!left.TryActivate() && !right.TryActivate(), "Both keys outside effective areas do nothing");
            Arrange(left.transform.position + Vector3.right * 1.51f, Vector3.zero);
            Check(!left.TryActivate(), "Player center just outside effective area is rejected");
            Arrange(left.transform.position, Vector3.down * 24f);
            Check(!right.TryActivate(), "Opposite-side flipper cannot rescue this ball");
            body.mass = 2f;
            int before = impulses;
            Check(left.TryActivate() && !left.TryActivate(), "Left accepts once and rejects immediate repeat");
            Check(!motor.TryRequestBoardImpulse(Vector2.up, 18f) && !motor.TryRequestDash(), "Pending board impulse rejects stacking and dash overwrite");
            yield return Delay(0.06f);
            Check(impulses == before + 1 && body.velocity.y > 12f && body.position.y > left.transform.position.y + 0.5f,
                $"Left rescues fast falling mass-2 ball with actual impulse: velocity={body.velocity}");
            Check(Mathf.Abs(body.position.z) < 0.001f, "Flipper impulse stays in board plane");
            ScreenCapture.CaptureScreenshot(EvidencePath("T04-left-rescue.png"));
            yield return Delay(0.02f);
            body.mass = 1f;
            Arrange(left.transform.position, Vector3.zero);
            Check(!left.TryActivate(), "Cooldown rejects another press even with ball inside zone");
            yield return Delay(0.3f);
            Check(impulses == before + 1 && leftHits == 1, "Staying in area does not automatically reapply force");
            Arrange(left.transform.position, Vector3.down * 10f);
            Check(left.TryActivate(), "Fresh press after cooldown can rescue again");
            yield return Delay(0.05f);

            Arrange(right.transform.position, Vector3.down * 24f);
            before = impulses;
            Check(right.TryActivate(), "Right flipper accepts ball in its own area");
            yield return Delay(0.06f);
            Check(impulses == before + 1 && body.velocity.y > 12f && body.velocity.x < 0f,
                $"Right rescue points up and toward table center: velocity={body.velocity}");
            ScreenCapture.CaptureScreenshot(EvidencePath("T04-right-rescue.png"));
            yield return Delay(0.02f);
            yield return Delay(0.3f);
            Check(rightHits == 1, "No repeated right-flipper activation without new press");

            Arrange(left.transform.position + Vector3.up * 0.6f, Vector3.zero);
            motor.SetMoveInput(Vector2.down);
            Check(motor.TryRequestDash(), "Downward dash fixture starts");
            yield return Delay(0.04f);
            Check(motor.IsDashing && left.TryActivate(), "Flipper accepts rescue during active downward dash");
            yield return Delay(0.05f);
            Check(!motor.IsDashing && body.velocity.y > 12f, "Board rescue takes priority over dash with movement input disabled");

            motor.SetMoveInput(Vector2.zero);
            for (int i = 0; i < 2; i++)
            {
                var center = bumpers[i].transform.position;
                var incoming = i == 0 ? Vector3.up : Vector3.down;
                Arrange(center - incoming * 1.75f, incoming * 8f);
                int hitsBefore = bounces;
                before = impulses;
                yield return () => bounces > hitsBefore;
                yield return Delay(0.04f);
                var outward = -incoming;
                Check(impulses == before + 1 && Vector3.Dot(body.velocity, outward) > 10f,
                    $"OrbitBreakerBumper {i} real contact launches away: velocity={body.velocity}");
                Check(Mathf.Abs(body.position.z) < 0.001f, "OrbitBreakerBumper impulse stays in XY");
                yield return Delay(0.08f);
                Check(bounces == hitsBefore + 1, "Single bumper contact yields one impulse");
                Arrange(Vector3.zero, Vector3.zero);
                yield return Delay(0.16f);
            }

            // A separate physical sphere must not invoke the player-only boost.
            int bounceBefore = bounces;
            var probe = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            probe.name = "T04_NonPlayerProbe";
            probe.transform.position = bumpers[0].transform.position + Vector3.down * 1.75f;
            var probeBody = probe.AddComponent<Rigidbody>();
            probeBody.useGravity = false;
            probeBody.constraints = OrbitBreakerPlayerMotor.PlaneConstraints;
            probeBody.velocity = Vector3.up * 8f;
            yield return Delay(0.2f);
            Check(bounces == bounceBefore && probeBody.velocity.y > -1f, "Non-player collision does not receive bumper boost");
            UnityEngine.Object.Destroy(probe);

            Arrange(left.transform.position, Vector3.zero);
            before = impulses;
            Check(left.TryActivate() && manager.LoseGame(), "Defeat follows a queued rescue");
            var frozen = body.position;
            yield return Delay(0.06f);
            Check(impulses == before && body.isKinematic && Vector3.Distance(frozen, body.position) < 0.001f && !left.TryActivate(),
                "Defeat cancels queued board impulse and prevents activation");
            int previous = manager.GetInstanceID();
            manager.RestartGame();
            yield return () => UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>().GetInstanceID() != previous;
            Bind(false);
            Check(manager.State == GameState.Ready && left.CooldownRemaining == 0f && right.CooldownRemaining == 0f,
                "Real reload resets both flippers and keeps all device references");
            manager.StartGame();
            Arrange(right.transform.position, Vector3.down * 24f);
            Check(right.TryActivate(), "Reloaded right flipper works immediately");
            yield return Delay(0.05f);
            Check(body.velocity.y > 12f, "Reloaded flipper applies a real impulse");
            manager.WinGame();
            Check(!left.TryActivate() && !right.TryActivate() && !motor.TryRequestBoardImpulse(Vector2.up, 18f), "Victory rejects all board launches");
        }
        private static IEnumerator<Func<bool>> KeyboardChecks()
        {
            Bind(true);
            Report("RUNNING", "Focus Game, Enter; then K (wrong side), J (left rescue), then K (right rescue).");
            yield return () => manager.IsPlaying;
            Check(true, "Real Enter started game");
            bool wrongKeyObserved = false;
            yield return () =>
            {
                if (Input.GetKeyDown(KeyCode.K))
                {
                    Check(leftHits == 0 && rightHits == 0, "Real K in left zone produces no launch");
                    wrongKeyObserved = true;
                    Report("RUNNING", "Wrong-side K rejected; press J to rescue left ball.");
                }
                if (leftHits > 0) return true;
                Arrange(left.transform.position, Vector3.zero);
                return false;
            };
            Check(wrongKeyObserved, "Wrong-side real key checked before left activation");
            yield return Delay(0.06f);
            Check(leftHits == 1 && body.velocity.y > 12f, "Real J raises left ball through normal flipper input");
            Report("RUNNING", "Left real-key rescue passed; press K for right rescue.");
            yield return () =>
            {
                if (rightHits > 0) return true;
                Arrange(right.transform.position, Vector3.zero);
                return false;
            };
            yield return Delay(0.06f);
            Check(rightHits == 1 && body.velocity.y > 12f, "Real K raises right ball through normal flipper input");
            yield return Delay(0.4f);
            Check(leftHits == 1 && rightHits == 1, "No automatic repeated launches after key release");
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
                leftHits = rightHits = bounces = impulses = 0;
                waiting = null;
                steps = SessionState.GetString(Prefix + "Mode", "") == "keyboard" ? KeyboardChecks() : PhysicsChecks();
                originalMaximumDeltaTime = Time.maximumDeltaTime;
                Time.maximumDeltaTime = Time.fixedDeltaTime;
                var observer = new GameObject("T04_ValidationProbe") { hideFlags = HideFlags.DontSave };
                UnityEngine.Object.DontDestroyOnLoad(observer);
                observer.AddComponent<OrbitBreaker.Testing.OrbitBreakerT03Probe>().Observe = Tick;
            }
            if (state != PlayModeStateChange.EnteredEditMode) return;
            if (!SessionState.GetBool(Prefix + "Complete", false)) Report("FAIL", "Play mode exited before completion.");
            else Debug.Log("[OrbitBreaker] T04 validation PASS.");
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
            File.WriteAllText(EvidencePath("T04-" + SessionState.GetString(Prefix + "Mode", "physics") + ".json"),
                JsonUtility.ToJson(new Result { status = status, observedAtUtc = DateTime.UtcNow.ToString("O"),
                    unityVersion = Application.unityVersion, detail = detail,
                    checks = SessionState.GetString(Prefix + "Checks", "").Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries) }, true));
        }
    }
}

