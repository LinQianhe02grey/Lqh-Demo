using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OrbitBreaker.Editor
{
    [InitializeOnLoad]
    public static class OrbitBreakerT06Validation
    {
        private const string Prefix = "OrbitBreaker.T06.";
        private static IEnumerator<Func<bool>> steps;
        private static Func<bool> waiting;
        private static OrbitBreakerPlayerMotor motor;
        private static Rigidbody body;
        private static OrbitBreakerGameManager manager;
        private static OrbitBreakerScoreManager score;
        private static OrbitBreakerEnemySpawner spawner;
        private static int spawned, killed;
        private static readonly List<Vector3> spawnPositions = new List<Vector3>();
        private static bool duplicateRejected;
        private static float originalMaximumDeltaTime;

        static OrbitBreakerT06Validation()
        {
            EditorApplication.update += CheckTimeout;
            EditorApplication.playModeStateChanged += OnPlayState;
        }
        [MenuItem("Tools/Orbit Breaker/Validate T06")]
        public static void Run() => Begin("physics");
        [MenuItem("Tools/Orbit Breaker/Check T06 Keyboard")]
        public static void Keyboard() => Begin("keyboard");

        private static void Begin(string mode)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Start T06 from idle Edit mode.");
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
            Fail("T06 timed out. Completed checks remain in the report.");
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
            Debug.LogError("[OrbitBreaker] T06 validation failed: " + detail);
            EditorApplication.isPlaying = false;
        }
        private static void Bind(bool keyboard)
        {
            motor = UnityEngine.Object.FindObjectOfType<OrbitBreakerPlayerMotor>();
            body = motor.GetComponent<Rigidbody>();
            manager = motor.OrbitBreakerGameManager;
            score = UnityEngine.Object.FindObjectOfType<OrbitBreakerScoreManager>();
            spawner = UnityEngine.Object.FindObjectOfType<OrbitBreakerEnemySpawner>();
            if (!keyboard) motor.GetComponent<OrbitBreakerPlayerInput>().enabled = false;
            Check(spawner != null && spawner.enabled && spawner.AliveCount == 0, "Enabled spawner persists with empty live container");
            var serialized = new SerializedObject(spawner);
            Check(serialized.FindProperty("enemyPrefab").objectReferenceValue != null &&
                serialized.FindProperty("spawnPoints").arraySize == 8 && serialized.FindProperty("liveRoot").objectReferenceValue != null,
                "OrbitBreakerEnemy prefab, eight markers and live container references persist");
            spawner.Spawned += enemy =>
            {
                spawned++;
                spawnPositions.Add(enemy.Position);
                enemy.Killed += victim =>
                {
                    killed++;
                    duplicateRejected = !score.TryAward(OrbitBreakerEnemyController.KillPoints, victim, 1);
                };
            };
        }
        private static IEnumerator<Func<bool>> PhysicsChecks()
        {
            Bind(false);
            Check(manager.State == GameState.Ready && spawner.TrySpawn() == null, "Ready refuses spawn requests");
            yield return Delay(manager.Config.enemySpawnInterval + 0.1f);
            Check(spawner.AliveCount == 0, "Ready never spawns over a full configured interval");
            manager.StartGame();
            yield return Hold(manager.Config.enemySpawnInterval - 0.12f, new Vector3(0f, -3f, 0f));
            Check(spawned == 0, "First enemy waits for configured spawn interval");
            yield return Hold(0.24f, new Vector3(0f, -3f, 0f));
            Check(spawned == 1 && spawner.AliveCount == 1, "Configured interval produces exactly one enemy");
            var enemy = UnityEngine.Object.FindObjectOfType<OrbitBreakerEnemyController>();
            float distance = Vector3.Distance(enemy.Position, body.position);
            yield return Hold(0.3f, new Vector3(0f, -3f, 0f));
            Check(Vector3.Distance(enemy.Position, body.position) < distance - 0.3f && Mathf.Abs(enemy.Position.z) < 0.001f,
                "OrbitBreakerEnemy approaches player through real XY physics");
            while (spawner.AliveCount < OrbitBreakerEnemySpawner.MaximumEnemies) Check(spawner.TrySpawn() != null, "Free edge marker spawns enemy");
            Check(spawner.TrySpawn() == null && spawner.AliveCount == 6, "Hard live cap rejects a seventh enemy");
            Check(spawnPositions.All(p => p.y >= OrbitBreakerEnemySpawner.MinimumSpawnY && Mathf.Abs(p.z) < 0.001f) &&
                spawnPositions.Distinct().Count() > 1, "Spawns use multiple safe edge points, never the drain");
            ScreenCapture.CaptureScreenshot(EvidencePath("T06-enemies.png"));
            yield return Hold(0.02f, new Vector3(0f, -3f, 0f));
            yield return Hold(manager.Config.enemySpawnInterval + 0.1f, new Vector3(0f, -3f, 0f));
            Check(spawned == 6 && spawner.AliveCount == 6, "Automatic schedule respects cap without catch-up bursts");
            spawner.enabled = false;
            ClearEnemies();
            yield return Delay(0.04f);
            Check(spawner.AliveCount == 0, "Destroyed enemies release live slots");

            var markers = GameObject.Find("OrbitBreaker/Arena/EnemySpawnPoints").transform.Cast<Transform>().ToArray();
            var blockers = new List<GameObject>();
            foreach (var point in markers)
            {
                var blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                blocker.name = "T06_SpawnBlocker";
                blocker.transform.position = point.position;
                blockers.Add(blocker);
            }
            spawner.enabled = true;
            Check(spawner.TrySpawn() == null, "All occupied markers skip spawn instead of overlapping bodies");
            spawner.enabled = false;
            foreach (var blocker in blockers) UnityEngine.Object.Destroy(blocker);
            yield return Delay(0.04f);
            var saved = markers.Select(p => p.position).ToArray();
            foreach (var marker in markers) marker.position = new Vector3(0f, -11f, 0f);
            spawner.enabled = true;
            Check(spawner.TrySpawn() == null, "Edited markers in drain are explicitly rejected");
            spawner.enabled = false;
            for (int i = 0; i < markers.Length; i++) markers[i].position = saved[i];

            enemy = SpawnFixture(Vector3.zero);
            Arrange(new Vector3(0f, -1.15f, 0f), Vector3.up * 12f);
            yield return Delay(0.2f);
            Check(enemy != null && !enemy.IsDead && score.Score == 0, "Ordinary fast contact cannot kill or score");
            motor.SetMoveInput(Vector2.up);
            yield return Delay(0.4f);
            Check(killed == 0 && score.Score == 0, "Sustained ordinary contact never farms kill points");
            ClearEnemies();
            // T07 ordinary contact now stuns. Wait for recovery before the independent dash case.
            yield return Hold(manager.Config.stunDuration + OrbitBreakerPlayerMotor.RecoveryProtection, new Vector3(0f, -2f, 0f));
            enemy = SpawnFixture(Vector3.zero);
            Arrange(new Vector3(0f, -1.3f, 0f), Vector3.zero);
            motor.SetMoveInput(Vector2.up);
            Check(motor.TryRequestDash(), "Upward dash requested against a live enemy");
            yield return () => killed == 1;
            Check(score.Score == 20 && duplicateRejected && !motor.IsDashing,
                "Actual dash collision kills once for 20 despite Motor ending dash before listeners");
            yield return Delay(0.06f);
            Check(enemy == null && spawner.AliveCount == 0, "Killed object and collider disappear and free cap slot");
            ScreenCapture.CaptureScreenshot(EvidencePath("T06-kill.png"));
            yield return Delay(0.02f);
            yield return Hold(0.8f, new Vector3(0f, -2f, 0f));
            Check(score.Score == 20 && killed == 1, "Destroyed enemy cannot score again");

            enemy = SpawnFixture(Vector3.zero);
            Arrange(new Vector3(-1.3f, 0f, 0f), Vector3.zero);
            motor.SetMoveInput(Vector2.right);
            Check(motor.TryRequestDash(), "Sideways dash requested");
            yield return () => killed == 2;
            Check(score.Score == 40, "Sideways dash also kills; enemy kill is not target upward-only rule");
            yield return Delay(0.06f);
            yield return Hold(0.8f, new Vector3(0f, -2f, 0f));

            enemy = SpawnFixture(Vector3.zero);
            enemy.GetComponent<Rigidbody>().position = new Vector3(0f, -10.7f, 0f);
            yield return Delay(0.08f);
            Check(enemy == null && score.Score == 40 && spawner.AliveCount == 0, "Out-of-board cleanup awards no kill points");
            enemy = SpawnFixture(new Vector3(2f, 0f, 0f));
            spawner.enabled = true;
            Arrange(new Vector3(0f, -9f, 0f), Vector3.down * 10f);
            yield return () => manager.State == GameState.Defeat;
            var frozen = enemy.Position;
            int before = spawned;
            yield return Delay(manager.Config.enemySpawnInterval + 0.1f);
            Check(spawned == before && spawner.TrySpawn() == null && enemy.GetComponent<Rigidbody>().isKinematic &&
                Vector3.Distance(frozen, enemy.Position) < 0.001f && score.Score == 40,
                "Drain Defeat freezes enemy, future spawns and score");
            int previous = manager.GetInstanceID();
            manager.RestartGame();
            yield return () => UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>().GetInstanceID() != previous;
            Bind(false);
            Check(manager.State == GameState.Ready && score.Score == 0 && UnityEngine.Object.FindObjectsOfType<OrbitBreakerEnemyController>().Length == 0,
                "Real restart removes all enemies, clears score and rebuilds spawner");
            manager.StartGame();
            spawner.enabled = false;
            // Four real high-target hits (400) plus five real dash kills (100) reach the configured 500.
            var high = UnityEngine.Object.FindObjectsOfType<OrbitBreakerScoreTarget>().Single(t => t.Points == 100);
            for (int i = 0; i < 4; i++)
            {
                yield return Hold(0.56f, new Vector3(0f, -2f, 0f));
                int value = score.Score;
                Arrange(high.transform.position + Vector3.down * 0.9f, Vector3.up * 10f);
                yield return () => score.Score == value + 100;
            }
            var survivor = SpawnFixture(new Vector3(4.8f, 7f, 0f));
            for (int i = 0; i < 5; i++)
            {
                yield return Hold(0.8f, new Vector3(0f, -2f, 0f));
                enemy = SpawnFixture(Vector3.zero);
                Arrange(new Vector3(0f, -1.3f, 0f), Vector3.zero);
                motor.SetMoveInput(Vector2.up);
                int value = score.Score;
                Check(motor.TryRequestDash(), "Integrated scoring dash accepted");
                yield return () => score.Score == value + 20;
            }
            Check(manager.State == GameState.Victory && score.Score == 500 && body.isKinematic && survivor.GetComponent<Rigidbody>().isKinematic,
                "Final real enemy kill reaches Victory and freezes surviving enemies");
            before = spawned;
            spawner.enabled = true;
            frozen = survivor.Position;
            yield return Delay(manager.Config.enemySpawnInterval + 0.1f);
            Check(spawned == before && spawner.TrySpawn() == null && Vector3.Distance(survivor.Position, frozen) < 0.001f,
                "Victory blocks new spawns and pursuit over a full interval");
            ScreenCapture.CaptureScreenshot(EvidencePath("T06-victory.png"));
            yield return Delay(0.02f);
            previous = manager.GetInstanceID();
            manager.RestartGame();
            yield return () => UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>().GetInstanceID() != previous;
            Bind(false);
            Check(score.Score == 0 && spawner.AliveCount == 0 && manager.State == GameState.Ready, "Victory reload clears enemies and score");
            manager.StartGame();
            yield return Hold(manager.Config.enemySpawnInterval - 0.1f, new Vector3(0f, -3f, 0f));
            Check(spawner.AliveCount == 0, "Restart resets the spawn countdown");
            yield return Hold(0.24f, new Vector3(0f, -3f, 0f));
            Check(spawner.AliveCount == 1, "Restarted round spawns normally after configured interval");
        }
        private static IEnumerator<Func<bool>> KeyboardChecks()
        {
            Bind(true);
            Report("RUNNING", "Focus Game, press Enter; then Space to dash into the waiting red enemy.");
            yield return () => manager.IsPlaying;
            Check(true, "Real Enter starts round");
            var enemy = SpawnFixture(Vector3.zero);
            bool dashed = false;
            motor.DashStarted += () => dashed = true;
            yield return () =>
            {
                if (dashed) return true;
                enemy.GetComponent<Rigidbody>().position = Vector3.zero;
                enemy.GetComponent<Rigidbody>().velocity = Vector3.zero;
                Arrange(new Vector3(0f, -1.3f, 0f), Vector3.zero);
                return false;
            };
            yield return () => killed == 1;
            Check(score.Score == 20 && duplicateRejected, "Real Space causes actual enemy collision, one kill and 20 points");
            yield return Delay(0.06f);
            Check(enemy == null, "Real-key killed enemy is removed");
            ScreenCapture.CaptureScreenshot(EvidencePath("T06-keyboard-kill.png"));
            yield return Delay(0.02f);
            spawner.enabled = true;
            Arrange(new Vector3(0f, -9f, 0f), Vector3.down * 10f);
            yield return () => manager.State == GameState.Defeat;
            Report("RUNNING", "Actual drain produced Defeat. Press R to reset enemies and score.");
            int previous = manager.GetInstanceID();
            yield return () => UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>().GetInstanceID() != previous;
            Bind(true);
            Check(manager.State == GameState.Ready && score.Score == 0 && spawner.AliveCount == 0,
                "Real R reload resets enemies and score after a scored kill");
        }
        private static OrbitBreakerEnemyController SpawnFixture(Vector3 position)
        {
            spawner.enabled = true;
            var enemy = spawner.TrySpawn();
            Check(enemy != null, "Spawn fixture uses real prefab and initialization");
            spawner.enabled = false;
            enemy.GetComponent<Rigidbody>().position = position;
            Physics.SyncTransforms();
            return enemy;
        }
        private static void ClearEnemies()
        {
            foreach (var enemy in UnityEngine.Object.FindObjectsOfType<OrbitBreakerEnemyController>()) UnityEngine.Object.Destroy(enemy.gameObject);
        }
        private static Func<bool> Hold(float duration, Vector3 position)
        {
            float until = Time.time + duration;
            return () => { Arrange(position, Vector3.zero); return Time.time >= until; };
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
                spawned = killed = 0; spawnPositions.Clear(); duplicateRejected = false;
                waiting = null;
                steps = SessionState.GetString(Prefix + "Mode", "") == "keyboard" ? KeyboardChecks() : PhysicsChecks();
                originalMaximumDeltaTime = Time.maximumDeltaTime;
                Time.maximumDeltaTime = Time.fixedDeltaTime;
                var observer = new GameObject("T06_ValidationProbe") { hideFlags = HideFlags.DontSave };
                UnityEngine.Object.DontDestroyOnLoad(observer);
                observer.AddComponent<OrbitBreaker.Testing.OrbitBreakerT03Probe>().Observe = Tick;
            }
            if (state != PlayModeStateChange.EnteredEditMode) return;
            if (!SessionState.GetBool(Prefix + "Complete", false)) Report("FAIL", "Play mode exited before completion.");
            else Debug.Log("[OrbitBreaker] T06 validation PASS.");
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
            File.WriteAllText(EvidencePath("T06-" + SessionState.GetString(Prefix + "Mode", "physics") + ".json"),
                JsonUtility.ToJson(new Result { status = status, observedAtUtc = DateTime.UtcNow.ToString("O"),
                    unityVersion = Application.unityVersion, detail = detail,
                    checks = SessionState.GetString(Prefix + "Checks", "").Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries) }, true));
        }
    }
}



