using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace OrbitBreaker.Editor
{
    [InitializeOnLoad]
    public static class OrbitBreakerT02Validation
    {
        private const string Prefix = "OrbitBreaker.T02.";
        private static int stage;
        private static int scenario;
        private static float stageTime;
        private static Vector3 checkpoint;
        private static int previousManager;
        private static int defeatEvents;
        private static GameObject nonPlayerProbe;

        static OrbitBreakerT02Validation()
        {
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += OnPlayState;
        }

        [MenuItem("Tools/Orbit Breaker/Validate T02")]
        public static void Run() => Begin("physics");

        [MenuItem("Tools/Orbit Breaker/Check T02 Keyboard")]
        public static void CheckKeyboard() => Begin("keyboard");

        private static void Begin(string mode)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || SessionState.GetBool(Prefix + "Running", false))
                throw new InvalidOperationException("Start T02 validation from idle Edit mode.");
            OrbitBreakerSceneBuilder.Build();
            SessionState.SetString(Prefix + "Mode", mode);
            SessionState.SetString(Prefix + "Checks", "");
            SessionState.SetString(Prefix + "Deadline", DateTime.UtcNow.AddMinutes(mode == "keyboard" ? 6 : 2).ToString("O"));
            SessionState.SetInt(Prefix + "Keys", 0);
            SessionState.SetBool(Prefix + "Running", true);
            SessionState.SetBool(Prefix + "Complete", false);
            WriteReport("RUNNING", "Waiting for Play mode.");
            EditorApplication.isPlaying = true;
        }

        private static void Tick()
        {
            if (!SessionState.GetBool(Prefix + "Running", false) || SessionState.GetBool(Prefix + "Complete", false)) return;
            try
            {
                if (DateTime.UtcNow > DateTime.Parse(SessionState.GetString(Prefix + "Deadline", ""), null,
                    System.Globalization.DateTimeStyles.RoundtripKind)) throw new TimeoutException("T02 validation timed out.");
                if (!EditorApplication.isPlaying || EditorApplication.isCompiling) return;
                var motor = UnityEngine.Object.FindObjectOfType<OrbitBreakerPlayerMotor>();
                if (motor == null) return;
                // Isolate the fixed T02 cases from random spawns.
                var spawner = UnityEngine.Object.FindObjectOfType<OrbitBreakerEnemySpawner>();
                if (spawner != null) spawner.enabled = false;
                var manager = motor.OrbitBreakerGameManager;
                var body = motor.GetComponent<Rigidbody>();
                if (SessionState.GetString(Prefix + "Mode", "") == "keyboard")
                    KeyboardTick(motor, manager, body);
                else PhysicsTick(motor, manager, body);
            }
            catch (Exception exception)
            {
                WriteReport("FAIL", exception.ToString());
                SessionState.SetBool(Prefix + "Running", false);
                Debug.LogError("[OrbitBreaker] T02 validation failed: " + exception);
                if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
            }
        }

        private static void PhysicsTick(OrbitBreakerPlayerMotor motor, OrbitBreakerGameManager manager, Rigidbody body)
        {
            if (stage == 0)
            {
                Check(manager.State == GameState.Ready && body.isKinematic && !body.useGravity, "Ready freezes custom-gravity player");
                Check(body.constraints == OrbitBreakerPlayerMotor.PlaneConstraints, "XY and rotation constraints configured");
                motor.GetComponent<OrbitBreakerPlayerInput>().enabled = false;
                checkpoint = body.position;
                stageTime = Time.time;
                stage = 1;
            }
            else if (stage == 1 && Elapsed(0.25f))
            {
                Check(Vector3.Distance(body.position, checkpoint) < 0.001f, "Ready remains stationary across physics steps");
                Check(manager.StartGame() && !body.isKinematic, "Start enables dynamic physics");
                Arrange(motor, body, new Vector3(0f, 4f, 0f), Vector3.zero, Vector2.zero);
                stage = 2;
            }
            else if (stage == 2 && Elapsed(0.3f))
            {
                Check(body.position.y < 3.7f && body.velocity.y < -2f, $"Natural fall y={body.position.y:F3}, vy={body.velocity.y:F3}");
                scenario = 0;
                BeginMovement(motor, body);
                stage = 3;
            }
            else if (stage == 3 && Elapsed(0.3f))
            {
                var velocity = body.velocity;
                bool correct = scenario == 0 ? velocity.x > 3f : scenario == 1 ? velocity.x < -3f :
                    scenario == 2 ? velocity.y > 1f : velocity.y < -7f;
                Check(correct, $"Movement {new[] { "D/right", "A/left", "W/up", "S/down" }[scenario]} velocity={velocity}");
                Check(Mathf.Abs(body.position.z) < 0.001f, "Movement remains in XY");
                if (++scenario < 4) BeginMovement(motor, body);
                else
                {
                    Arrange(motor, body, new Vector3(0f, 4f, 0f), Vector3.zero, Vector2.zero);
                    body.AddForce(Vector3.forward * 100f, ForceMode.VelocityChange);
                    body.AddTorque(Vector3.one * 100f, ForceMode.VelocityChange);
                    stage = 4;
                }
            }
            else if (stage == 4 && Elapsed(0.3f))
            {
                Check(Mathf.Abs(body.position.z) < 0.001f && Quaternion.Angle(body.rotation, Quaternion.identity) < 0.1f,
                    $"Out-of-plane constraint: position={body.position}, rotation={body.rotation.eulerAngles}, angularVelocity={body.angularVelocity}");
                scenario = 0;
                BeginWall(motor, body);
                stage = 5;
            }
            else if (stage == 5 && Elapsed(0.35f))
            {
                bool bounded = scenario == 0 ? body.position.x <= 7.02f && body.position.x >= 6.8f :
                    scenario == 1 ? body.position.x >= -7.02f && body.position.x <= -6.8f :
                    scenario == 2 ? body.position.y <= 9.52f && body.position.y >= 8.5f :
                    body.position.y >= -9.52f && body.position.y <= -9.3f;
                Check(bounded && manager.IsPlaying, $"Wall/guard {scenario} blocks player at {body.position}");
                if (++scenario < 5) BeginWall(motor, body);
                else
                {
                    Arrange(motor, body, new Vector3(0f, 4f, 0f), Vector3.zero, Vector2.zero);
                    nonPlayerProbe = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    nonPlayerProbe.transform.position = new Vector3(0f, -11.75f, 0f);
                    nonPlayerProbe.AddComponent<Rigidbody>().useGravity = false;
                    stage = 6;
                }
            }
            else if (stage == 6 && Elapsed(0.25f))
            {
                Check(manager.IsPlaying, "OrbitBreakerKillZone ignores non-player rigidbodies");
                UnityEngine.Object.Destroy(nonPlayerProbe);
                var spawn = GameObject.Find("OrbitBreaker/PlayerSpawn").transform.position;
                Arrange(motor, body, spawn, Vector3.zero, Vector2.zero);
                manager.StateChanged += state => { if (state == GameState.Defeat) defeatEvents++; };
                stage = 7;
            }
            else if (stage == 7 && manager.State == GameState.Defeat)
            {
                Check(body.position.y < -9.5f && body.isKinematic && defeatEvents == 1,
                    $"Natural central drain produces one Defeat at y={body.position.y:F3}");
                checkpoint = body.position;
                stageTime = Time.time;
                stage = 8;
            }
            else if (stage == 8 && Elapsed(0.3f))
            {
                Check(Vector3.Distance(body.position, checkpoint) < 0.001f && defeatEvents == 1, "Defeat freezes player with no repeated event");
                previousManager = manager.GetInstanceID();
                stage = 9;
                Check(manager.RestartGame(), "Defeat requests real scene reload");
            }
            else if (stage == 9 && manager.GetInstanceID() != previousManager)
            {
                var spawn = GameObject.Find("OrbitBreaker/PlayerSpawn").transform.position;
                Check(manager.State == GameState.Ready && body.isKinematic && Vector3.Distance(body.position, spawn) < 0.001f,
                    "Reload restores Ready at spawn");
                Check(motor.MoveInput == Vector2.zero, "Reload clears previous movement");
                Check(manager.StartGame() && manager.WinGame() && body.isKinematic, "Victory also freezes player");
                Complete();
            }
        }

        private static void KeyboardTick(OrbitBreakerPlayerMotor motor, OrbitBreakerGameManager manager, Rigidbody body)
        {
            int mask = SessionState.GetInt(Prefix + "Keys", 0);
            var input = motor.MoveInput;
            if (input.x > 0) mask |= 1;
            if (input.x < 0) mask |= 2;
            if (input.y > 0) mask |= 4;
            if (input.y < 0) mask |= 8;
            if (mask != SessionState.GetInt(Prefix + "Keys", 0))
            {
                SessionState.SetInt(Prefix + "Keys", mask);
                Check(true, "Real OrbitBreakerPlayerInput observed direction " + input + ", mask=" + mask);
                WriteReport("RUNNING", "Press WASD in Ready, then Enter, wait for Defeat, then R. Keys mask=" + mask);
            }
            if (stage == 0 && manager.IsPlaying)
            {
                Check(true, "Real Enter started Playing");
                stage = 1;
            }
            else if (stage == 1 && manager.State == GameState.Defeat)
            {
                Check(body.isKinematic, "Real no-input fall reached Defeat and froze player");
                previousManager = manager.GetInstanceID();
                stage = 2;
                WriteReport("RUNNING", "Defeat observed; press R to verify keyboard restart. Keys mask=" + mask);
            }
            else if (stage == 2 && manager.GetInstanceID() != previousManager && manager.State == GameState.Ready)
            {
                Check(mask == 15, "All four real WASD directions observed");
                Check(true, "Real R reloaded to Ready");
                Complete();
            }
        }

        private static void BeginMovement(OrbitBreakerPlayerMotor motor, Rigidbody body)
        {
            var directions = new[] { Vector2.right, Vector2.left, Vector2.up, Vector2.down };
            Arrange(motor, body, new Vector3(0f, 4f, 0f), Vector3.zero, directions[scenario]);
        }

        private static void BeginWall(OrbitBreakerPlayerMotor motor, Rigidbody body)
        {
            var positions = new[] { new Vector3(6.5f, 2f), new Vector3(-6.5f, 2f), new Vector3(0f, 9f), new Vector3(-4f, -9f), new Vector3(4f, -9f) };
            var velocities = new[] { Vector3.right * 24f, Vector3.left * 24f, Vector3.up * 24f, Vector3.down * 20f, Vector3.down * 20f };
            Arrange(motor, body, positions[scenario], velocities[scenario], Vector2.zero);
        }

        // Tests arrange initial conditions; production physics writers remain in OrbitBreakerPlayerMotor only.
        private static void Arrange(OrbitBreakerPlayerMotor motor, Rigidbody body, Vector3 position, Vector3 velocity, Vector2 input)
        {
            body.position = position;
            body.velocity = velocity;
            body.angularVelocity = Vector3.zero;
            motor.SetMoveInput(input);
            stageTime = Time.time;
        }

        private static bool Elapsed(float seconds) => Time.time - stageTime >= seconds;
        private static void Check(bool pass, string detail)
        {
            if (!pass) throw new InvalidOperationException(detail);
            SessionState.SetString(Prefix + "Checks", SessionState.GetString(Prefix + "Checks", "") + detail + "\n");
        }
        private static void Complete()
        {
            WriteReport("PASS", "All requested checks completed; exiting Play.");
            SessionState.SetBool(Prefix + "Complete", true);
            EditorApplication.isPlaying = false;
        }
        private static void OnPlayState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                stage = 0;
                defeatEvents = 0;
            }
            if (state != PlayModeStateChange.EnteredEditMode || !SessionState.GetBool(Prefix + "Running", false)) return;
            if (!SessionState.GetBool(Prefix + "Complete", false)) WriteReport("FAIL", "Play mode exited before checks completed.");
            else Debug.Log("[OrbitBreaker] T02 " + SessionState.GetString(Prefix + "Mode", "") + " validation PASS.");
            SessionState.SetBool(Prefix + "Running", false);
        }

        [Serializable]
        private sealed class Report { public string status; public string observedAtUtc; public string unityVersion; public string detail; public string[] checks; }
        private static void WriteReport(string status, string detail)
        {
            string mode = SessionState.GetString(Prefix + "Mode", "physics");
            string path = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Docs/EVIDENCE/T02-" + mode + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(new Report { status = status, observedAtUtc = DateTime.UtcNow.ToString("O"),
                unityVersion = Application.unityVersion, detail = detail,
                checks = SessionState.GetString(Prefix + "Checks", "").Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries) }, true));
        }
    }
}

