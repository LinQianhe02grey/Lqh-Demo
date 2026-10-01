using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OrbitBreaker.Editor
{
    [InitializeOnLoad]
    public static class OrbitBreakerT07Validation
    {
        private const string Prefix = "OrbitBreaker.T07.";
        private static IEnumerator<Func<bool>> steps;
        private static Func<bool> waiting;
        private static OrbitBreakerPlayerMotor motor;
        private static Rigidbody body;
        private static OrbitBreakerGameManager manager;
        private static OrbitBreakerScoreManager score;
        private static OrbitBreakerEnemySpawner spawner;
        private static int hits, dashStarts;
        private static float originalMaximumDeltaTime;

        static OrbitBreakerT07Validation()
        {
            EditorApplication.update += CheckTimeout;
            EditorApplication.playModeStateChanged += OnPlayState;
        }
        [MenuItem("Tools/Orbit Breaker/Validate T07")]
        public static void Run() => Begin("physics");
        [MenuItem("Tools/Orbit Breaker/Check T07 Keyboard")]
        public static void Keyboard() => Begin("keyboard");

        private static void Begin(string mode)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Start T07 from idle Edit mode.");
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
            Fail("T07 timed out. Completed checks remain in the report.");
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
            Debug.LogError("[OrbitBreaker] T07 validation failed: " + detail);
            EditorApplication.isPlaying = false;
        }
        private static void Bind(bool keyboard)
        {
            motor = UnityEngine.Object.FindObjectOfType<OrbitBreakerPlayerMotor>();
            body = motor.GetComponent<Rigidbody>();
            manager = motor.OrbitBreakerGameManager;
            score = UnityEngine.Object.FindObjectOfType<OrbitBreakerScoreManager>();
            spawner = UnityEngine.Object.FindObjectOfType<OrbitBreakerEnemySpawner>();
            Check(spawner.enabled && spawner.AliveCount == 0, "Saved spawner enabled and empty after reload");
            spawner.enabled = false;
            if (!keyboard) motor.GetComponent<OrbitBreakerPlayerInput>().enabled = false;
            motor.HitReceived += () => hits++;
            motor.DashStarted += () => dashStarts++;
            Check(UnityEngine.Object.FindObjectsOfType<OrbitBreakerHazard>().Length == 2, "Two persistent solid hazards");
        }
        private static IEnumerator<Func<bool>> PhysicsChecks()
        {
            Bind(false);
            var leftWall = GameObject.Find("OrbitBreaker/Arena/Walls/LeftWall").GetComponent<BoxCollider>();
            var rightWall = GameObject.Find("OrbitBreaker/Arena/Walls/RightWall").GetComponent<BoxCollider>();
            Check(Mathf.Abs(rightWall.bounds.min.x - leftWall.bounds.max.x - 15f) < 0.01f, "Playable width is 15 instead of 12 (25 percent wider)");
            Check(Mathf.Abs(GameObject.Find("OrbitBreaker/Arena/Walls/LeftGuard").GetComponent<BoxCollider>().bounds.max.x + 2f) < 0.01f &&
                Mathf.Abs(GameObject.Find("OrbitBreaker/Arena/Walls/RightGuard").GetComponent<BoxCollider>().bounds.min.x - 2f) < 0.01f,
                "Central four-unit drain opening preserved");
            Check(!Camera.main.orthographic && Mathf.Abs(Camera.main.fieldOfView - 42f) < 0.01f && body.transform.localScale == Vector3.one,
                "Perspective camera and spherical player scale preserved");
            Check(!motor.TryReceiveHit(), "Ready refuses hits");
            manager.StartGame();
            Arrange(Vector3.zero, Vector3.zero);
            Check(motor.TryRequestDash() && motor.TryReceiveHit(), "Hit interrupts a queued dash");
            Check(motor.IsStunned && motor.IsInvulnerable && !motor.CanControl && !motor.TryReceiveHit() && !motor.TryRequestDash(),
                "Immediate stun locks abilities and rejects same-step repeated hits");
            motor.SetMoveInput(Vector2.one);
            yield return Delay(0.04f);
            Check(hits == 1 && dashStarts == 0 && body.velocity.y < -6f && motor.MoveInput == Vector2.zero,
                "One downward impulse, no queued dash or movement leaks through");
            float vy = body.velocity.y;
            yield return Delay(0.12f);
            Check(body.velocity.y < vy - 1f && Mathf.Abs(body.position.x) < 0.001f && !body.isKinematic,
                "Stunned ball remains dynamic with gravity and no steering");
            var block = new MaterialPropertyBlock();
            body.GetComponent<Renderer>().GetPropertyBlock(block);
            Check(block.GetColor("_Color").r > 0.9f && block.GetColor("_Color").b < 0.5f, "Stun renders red without changing shared blue material");
            ScreenCapture.CaptureScreenshot(EvidencePath("T07-stunned.png"));
            yield return Hold(0.02f, Vector3.zero);
            yield return () => !motor.IsStunned;
            Check(motor.IsInvulnerable && motor.CanControl && !motor.TryReceiveHit(), "Control returns before recovery protection expires");
            Arrange(Vector3.zero, Vector3.zero);
            motor.SetMoveInput(Vector2.right);
            yield return Delay(0.08f);
            Check(body.velocity.x > 1f, "Steering returns after configured stunDuration");
            yield return Hold(0.4f, Vector3.zero);
            Check(!motor.IsInvulnerable && !body.GetComponent<Renderer>().HasPropertyBlock(), "Protection expires and original appearance restores");

            Check(motor.TryReceiveHit(), "New hit accepted after protection");
            Arrange(new Vector3(6.6f, 3f, 0f), Vector3.right * 24f);
            yield return Delay(0.08f);
            Check(motor.IsStunned && body.position.x <= 7.02f && body.position.x >= 6.8f && Mathf.Abs(body.position.z) < 0.001f,
                "Stunned player still collides with widened wall and stays on XY plane");
            yield return Hold(1.1f, Vector3.zero);
            int kicks = 0;
            motor.BoardImpulseApplied += () => kicks++;
            Check(motor.TryReceiveHit(), "OrbitBreakerBumper stun fixture accepted");
            Arrange(new Vector3(3.1f, 5.05f, 0f), Vector3.up * 12f);
            yield return () => kicks > 0;
            Check(motor.IsStunned && body.velocity.y < -10f, "Actual bumper contact still bounces a stunned ball");
            yield return Hold(1.1f, Vector3.zero);
            var flippers = UnityEngine.Object.FindObjectsOfType<OrbitBreakerFlipperController>().OrderBy(f => f.Side).ToArray();
            for (int side = 0; side < 2; side++)
            {
                Arrange(flippers[side].transform.position, Vector3.down * 24f);
                if (side == 0) Check(flippers[side].TryActivate() && motor.TryReceiveHit(), "Hit after queued left rescue keeps launch request");
                else Check(motor.TryReceiveHit() && flippers[side].TryActivate(), "Right rescue allowed after hit");
                float beforeY = body.position.y;
                yield return Delay(0.06f);
                Check(motor.IsStunned && body.velocity.y > 10f && body.position.y > beforeY && !motor.TryRequestDash(),
                    "Flipper rescues stunned ball upward without clearing stun, side " + side);
                if (side == 0) { ScreenCapture.CaptureScreenshot(EvidencePath("T07-stun-rescue.png")); yield return Delay(0.02f); }
                yield return Hold(1.1f, Vector3.zero);
            }

            var enemy = SpawnFixture(Vector3.zero);
            int beforeHits = hits;
            Arrange(new Vector3(0f, -1.3f, 0f), Vector3.up * 12f);
            yield return () => hits > beforeHits;
            Check(motor.IsStunned && enemy != null && !enemy.IsDead && score.Score == 0 && enemy.AttackCooldownRemaining > 1f,
                "Real ordinary enemy collision stuns once, starts attack cooldown and never scores");
            // Fixed enemy allows a stable contact test while normal player physics keeps touching it.
            var enemyBody = enemy.GetComponent<Rigidbody>();
            enemyBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            enemyBody.isKinematic = true;
            yield return Hold(1.02f, new Vector3(-2f, -2f, 0f));
            Check(!motor.IsInvulnerable && enemy.AttackCooldownRemaining > 0f, "OrbitBreakerEnemy cooldown outlasts player protection");
            enemyBody.position = Vector3.zero;
            Arrange(new Vector3(0f, -1.15f, 0f), Vector3.up * 12f);
            yield return Delay(0.06f);
            Check(hits == beforeHits + 1 && !motor.IsStunned, "Early re-entry cannot bypass enemy attack cooldown");
            yield return Hold(0.35f, new Vector3(-2f, -2f, 0f));
            Arrange(new Vector3(0f, -1.15f, 0f), Vector3.up * 12f);
            yield return () => hits > beforeHits + 1;
            Check(hits == beforeHits + 2 && motor.IsStunned, "New contact after both cooldowns can hit again");
            // Maintain a physically overlapping contact across multiple cooldown windows.
            float contactUntil = Time.time + 1.5f;
            yield return () => { Arrange(new Vector3(0f, -0.88f, 0f), Vector3.up); return Time.time >= contactUntil; };
            Check(hits == beforeHits + 2, "Sustained contact never refreshes stun even after cooldown expires");
            ClearEnemies();
            yield return Hold(1.1f, Vector3.zero);
            beforeHits = hits;
            enemy = SpawnFixture(Vector3.zero);
            Arrange(new Vector3(0f, -1.3f, 0f), Vector3.zero);
            motor.SetMoveInput(Vector2.up);
            Check(motor.TryRequestDash(), "Dash kill fixture accepted");
            yield return () => enemy == null;
            Check(score.Score == 20 && hits == beforeHits && !motor.IsStunned, "Real dash kills enemy for 20 without also hitting player");
            yield return Hold(1.1f, Vector3.zero);

            var hazards = UnityEngine.Object.FindObjectsOfType<OrbitBreakerHazard>().OrderBy(h => h.transform.position.x).ToArray();
            beforeHits = hits;
            Arrange(hazards[0].transform.position + Vector3.down * 1.45f, Vector3.zero);
            motor.SetMoveInput(Vector2.up);
            Check(motor.TryRequestDash(), "Dash towards hazard accepted");
            yield return () => hits > beforeHits;
            Check(motor.IsStunned && !motor.IsDashing && hazards[0].CooldownRemaining > 1f && score.Score == 20,
                "Real hazard collision interrupts active dash, stuns and awards no points");
            Arrange(hazards[1].transform.position + Vector3.down * 1.3f, Vector3.up * 12f);
            yield return Delay(0.08f);
            Check(hits == beforeHits + 1, "Another hazard cannot stack hits during player protection");
            yield return Hold(1.25f, Vector3.zero);
            Arrange(hazards[1].transform.position + Vector3.down * 1.3f, Vector3.up * 12f);
            yield return () => hits > beforeHits + 1;
            Check(hits == beforeHits + 2, "Other hazard can hit on a fresh unprotected contact");
            ScreenCapture.CaptureScreenshot(EvidencePath("T07-hazard-hit.png"));
            yield return Delay(0.02f);
            Arrange(new Vector3(0f, -9.8f, 0f), Vector3.down * 10f);
            yield return () => manager.State == GameState.Defeat;
            Check(!motor.IsStunned && !motor.IsInvulnerable && !motor.TryReceiveHit() && body.isKinematic,
                "Actual drain overrides stun and freezes all further damage");
            int previous = manager.GetInstanceID();
            manager.RestartGame();
            yield return () => UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>().GetInstanceID() != previous;
            Bind(false);
            Check(!motor.IsStunned && !motor.IsInvulnerable && score.Score == 0 && motor.DashCooldownRemaining == 0f &&
                UnityEngine.Object.FindObjectsOfType<OrbitBreakerHazard>().All(h => h.CooldownRemaining == 0f), "Real reload resets damage state, score and hazard cooldowns");
            manager.StartGame();
            Check(motor.TryReceiveHit() && manager.WinGame(), "Victory can supersede a queued hit");
            var frozen = body.position;
            yield return Delay(0.08f);
            Check(body.isKinematic && Vector3.Distance(frozen, body.position) < 0.001f && !motor.IsStunned && !motor.TryReceiveHit(),
                "Victory discards queued damage and freezes body");
        }
        private static IEnumerator<Func<bool>> KeyboardChecks()
        {
            Bind(true);
            Report("RUNNING", "Focus Game and press Enter. Keyboard fixture uses a runtime-only 120-second stun to allow real input.");
            yield return () => manager.IsPlaying;
            // Clone, never edit the persistent eight-parameter asset. Automatic tests use the actual .65 seconds.
            var clone = UnityEngine.Object.Instantiate(manager.Config);
            clone.hideFlags = HideFlags.DontSave;
            clone.stunDuration = 120f;
            var settings = new SerializedObject(manager);
            settings.FindProperty("config").objectReferenceValue = clone;
            settings.ApplyModifiedPropertiesWithoutUndo();
            var left = UnityEngine.Object.FindObjectsOfType<OrbitBreakerFlipperController>().Single(f => f.Side == FlipperSide.Left);
            var right = UnityEngine.Object.FindObjectsOfType<OrbitBreakerFlipperController>().Single(f => f.Side == FlipperSide.Right);
            var enemy = SpawnFixture(left.transform.position + Vector3.up * 1.2f);
            Arrange(left.transform.position, Vector3.up * 12f);
            yield return () => motor.IsStunned;
            ClearEnemies();
            int beforeDash = dashStarts;
            Report("RUNNING", "Real enemy hit: press Space while stunned (must be rejected), then J to rescue.");
            yield return () => { Arrange(left.transform.position, Vector3.zero); return Input.GetKeyDown(KeyCode.Space); };
            Check(motor.IsStunned && dashStarts == beforeDash && !motor.CanDash, "Real Space cannot dash during stun");
            bool launched = false;
            left.Activated += () => launched = true;
            yield return () => { if (launched) return true; Arrange(left.transform.position, Vector3.zero); return false; };
            yield return Delay(0.06f);
            Check(motor.IsStunned && body.velocity.y > 10f, "Real J rescues a stunned ball through normal flipper input");
            ScreenCapture.CaptureScreenshot(EvidencePath("T07-keyboard-rescue.png"));
            yield return Delay(0.02f);
            launched = false;
            right.Activated += () => launched = true;
            Report("RUNNING", "Press K for right-side stunned rescue, then R after the actual drain.");
            yield return () => { if (launched) return true; Arrange(right.transform.position, Vector3.zero); return false; };
            yield return Delay(0.06f);
            Check(motor.IsStunned && body.velocity.y > 10f, "Real K rescues while stunned; remaining=" + motor.StunRemaining + ", velocity=" + body.velocity);
            Arrange(new Vector3(0f, -9.8f, 0f), Vector3.down * 10f);
            yield return () => manager.State == GameState.Defeat;
            int previous = manager.GetInstanceID();
            yield return () => UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>().GetInstanceID() != previous;
            Bind(true);
            Check(manager.State == GameState.Ready && !motor.IsStunned && !motor.IsInvulnerable && manager.Config.stunDuration < 1f,
                "Real R reload clears stun and restores persistent config");
            UnityEngine.Object.Destroy(clone);
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
                hits = dashStarts = 0;
                waiting = null;
                steps = SessionState.GetString(Prefix + "Mode", "") == "keyboard" ? KeyboardChecks() : PhysicsChecks();
                originalMaximumDeltaTime = Time.maximumDeltaTime;
                Time.maximumDeltaTime = Time.fixedDeltaTime;
                var observer = new GameObject("T07_ValidationProbe") { hideFlags = HideFlags.DontSave };
                UnityEngine.Object.DontDestroyOnLoad(observer);
                observer.AddComponent<OrbitBreaker.Testing.OrbitBreakerT03Probe>().Observe = Tick;
            }
            if (state != PlayModeStateChange.EnteredEditMode) return;
            if (!SessionState.GetBool(Prefix + "Complete", false)) Report("FAIL", "Play mode exited before completion.");
            else Debug.Log("[OrbitBreaker] T07 validation PASS.");
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
            File.WriteAllText(EvidencePath("T07-" + SessionState.GetString(Prefix + "Mode", "physics") + ".json"),
                JsonUtility.ToJson(new Result { status = status, observedAtUtc = DateTime.UtcNow.ToString("O"),
                    unityVersion = Application.unityVersion, detail = detail,
                    checks = SessionState.GetString(Prefix + "Checks", "").Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries) }, true));
        }
    }
}






