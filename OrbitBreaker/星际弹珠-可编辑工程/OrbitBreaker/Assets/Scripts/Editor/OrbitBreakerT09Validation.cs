using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OrbitBreaker.Editor
{
    [InitializeOnLoad]
    public static class OrbitBreakerT09Validation
    {
        private const string Prefix = "OrbitBreaker.T09.";
        private static IEnumerator<Func<bool>> steps;
        private static Func<bool> waiting;
        private static OrbitBreakerPlayerMotor motor;
        private static Rigidbody body;
        private static OrbitBreakerGameManager manager;
        private static OrbitBreakerScoreManager score;
        private static OrbitBreakerEnemySpawner spawner;
        private static int hits, dashStarts;
        private static float originalMaximumDeltaTime;

        static OrbitBreakerT09Validation()
        {
            EditorApplication.update += CheckTimeout;
            EditorApplication.playModeStateChanged += OnPlayState;
        }
        [MenuItem("Tools/Orbit Breaker/Validate T09")]
        public static void Run() => Begin("physics");
        [MenuItem("Tools/Orbit Breaker/Check T09 Keyboard")]
        public static void Keyboard() => Begin("keyboard");

        private static void Begin(string mode)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Start T09 from idle Edit mode.");
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
            Fail("T09 timed out. Completed checks remain in the report.");
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
            Debug.LogError("[OrbitBreaker] T09 validation failed: " + detail);
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
        private static OrbitBreakerAttachmentPoint Point => UnityEngine.Object.FindObjectsOfType<OrbitBreakerAttachmentPoint>().OrderBy(p => p.Center.x).First();
        private static void AttachFixture()
        {
            Arrange((Vector3)Point.Center + Vector3.down * OrbitBreakerAttachmentPoint.OrbitRadius, Vector3.zero);
            motor.SetAttachmentAim(Point.Center + Vector2.down * 4f);
            Check(motor.TryAttach(Point), "Nearby attachment accepted");
        }
        private static Func<bool> Spin(float duration)
        {
            float start = Time.time;
            Vector2 radial = (Vector2)body.position - Point.Center;
            float initial = Mathf.Atan2(radial.y, radial.x) * Mathf.Rad2Deg;
            return () =>
            {
                float angle = (initial + (Time.time - start) * 300f) * Mathf.Deg2Rad;
                motor.SetAttachmentAim(Point.Center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 4f);
                return Time.time >= start + duration;
            };
        }
        private static IEnumerator<Func<bool>> PhysicsChecks()
        {
            Bind(false);
            var hud = UnityEngine.Object.FindObjectOfType<OrbitBreakerScoreDisplay>();
            yield return Delay(0.04f);
            Check(hud != null && hud.StateText == "准备就绪" && hud.DashText == "等待开局" && hud.ChineseFontAvailable,
                "Ready HUD uses real game state and available Chinese font");
            ScreenCapture.CaptureScreenshot(EvidencePath("T09-ready.png"));
            yield return Delay(0.04f);
            manager.StartGame();
            Arrange(Vector3.zero, Vector3.zero);
            Check(hud.StateText == "自由移动" && hud.DashText.Contains("就绪"), "Playing HUD shows normal state and ready dash");
            var existing = SpawnFixture(new Vector3(-5f, -5f, 0f));
            AttachFixture();
            Check(Physics.GetIgnoreCollision(body.GetComponent<Collider>(), existing.GetComponent<Collider>()), "Existing enemy pair ignores physical push immediately on E attach");
            existing.GetComponent<Rigidbody>().position = body.position + Vector3.down * 0.9f;
            Physics.SyncTransforms();
            yield return () => existing.IsRepelled;
            Check(motor.IsAttached && !motor.IsStunned && hits == 0 && score.Score == 0 && !existing.IsDead,
                "Real proximity repels existing enemy without stun, detachment, kill or score");
            float distance = Vector3.Distance(body.position, existing.Position);
            yield return Delay(0.08f);
            Check(Vector3.Distance(body.position, existing.Position) > distance + 0.4f && existing.GetComponent<Rigidbody>().velocity.y < -6f,
                "OrbitBreakerEnemy moves outward at knockback speed instead of overwriting it with pursuit");
            Check(hud.StateText.Contains("敌人防护") && hud.MagnetText.Contains("剩余") && hud.FeedbackText.Contains("击退"),
                "Shield, countdown and event-driven repel feedback visible in HUD");
            var block = new MaterialPropertyBlock();
            existing.GetComponent<Renderer>().GetPropertyBlock(block);
            Check(block.GetColor("_Color").g > 0.9f, "Repelled enemy visibly flashes green without changing material");
            ScreenCapture.CaptureScreenshot(EvidencePath("T09-repel.png"));
            yield return Delay(0.02f);
            var spawned = SpawnFixture(body.position + Vector3.right * 0.95f);
            Check(Physics.GetIgnoreCollision(body.GetComponent<Collider>(), spawned.GetComponent<Collider>()), "OrbitBreakerEnemy spawned during attachment inherits collision protection");
            yield return () => spawned.IsRepelled;
            Check(motor.IsAttached && hits == 0 && score.Score == 0, "Second simultaneous attacker also repels without stacked damage or score");
            yield return Spin(0.72f);
            Check(motor.IsAttached && !motor.IsStunned && motor.AttachmentCharge > 0.9f &&
                Mathf.Abs(Vector2.Distance(body.position, Point.Center) - OrbitBreakerAttachmentPoint.OrbitRadius) < 0.04f,
                "OrbitBreakerEnemy interference cannot displace actual orbit or stop charging");
            ScreenCapture.CaptureScreenshot(EvidencePath("T09-orbit.png"));
            yield return Delay(0.02f);
            motor.ReleaseAttachment();
            Check(!Physics.GetIgnoreCollision(body.GetComponent<Collider>(), existing.GetComponent<Collider>()) &&
                !Physics.GetIgnoreCollision(body.GetComponent<Collider>(), spawned.GetComponent<Collider>()), "Manual release restores both solid enemy pairs immediately");
            ClearEnemies();
            yield return Hold(1.1f, Vector3.zero);
            AttachFixture();
            var cleanup = SpawnFixture(new Vector3(-5f, -5f, 0f));
            cleanup.enabled = false;
            Check(!Physics.GetIgnoreCollision(body.GetComponent<Collider>(), cleanup.GetComponent<Collider>()), "Disabled enemy restores ignored pair");
            cleanup.enabled = true;
            Check(Physics.GetIgnoreCollision(body.GetComponent<Collider>(), cleanup.GetComponent<Collider>()), "Re-enabled enemy correctly reapplies active shield pair");
            yield return () => !motor.IsAttached;
            Check(!Physics.GetIgnoreCollision(body.GetComponent<Collider>(), cleanup.GetComponent<Collider>()) && !motor.IsStunned,
                "Production 7.5-second timeout still ends protection and restores physical contact");
            ClearEnemies();
            yield return Hold(0.7f, Vector3.zero);
            var enemy = SpawnFixture(Vector3.zero);
            int beforeHits = hits;
            Arrange(new Vector3(0f, -1.3f, 0f), Vector3.up * 12f);
            yield return () => hits > beforeHits;
            Check(motor.IsStunned && hud.StateText == "受击失控" && hud.DashText.Contains("不可用") && hud.MagnetText.Contains("不可用") && hud.FeedbackText.Contains("受击"),
                "Ordinary unshielded enemy collision still stuns and HUD locks both abilities");
            ScreenCapture.CaptureScreenshot(EvidencePath("T09-stunned.png"));
            ClearEnemies();
            yield return Hold(1.1f, Vector3.zero);
            AttachFixture();
            var hazard = UnityEngine.Object.FindObjectsOfType<OrbitBreakerHazard>().First();
            Vector3 originalHazard = hazard.transform.position;
            hazard.transform.position = body.position + Vector3.down * 0.9f;
            Physics.SyncTransforms();
            beforeHits = hits;
            yield return () => hits > beforeHits;
            Check(!motor.IsAttached && motor.IsStunned, "Shield is enemy-specific: a real solid hazard still interrupts and stuns");
            hazard.transform.position = originalHazard;
            Physics.SyncTransforms();
            yield return Hold(1.1f, Vector3.zero);
            motor.SetMoveInput(Vector2.up);
            Check(motor.TryRequestDash(), "Normal dash accepted for HUD test");
            yield return Delay(0.04f);
            Check(hud.StateText == "冲刺中" && hud.FeedbackText.Contains("冲刺"), "Actual dash state and event feedback render");
            yield return Hold(0.22f, Vector3.zero);
            Check(hud.DashText.Contains("冷却"), "Post-dash HUD shows remaining cooldown");
            ScreenCapture.CaptureScreenshot(EvidencePath("T09-cooldown.png"));
            yield return Hold(0.8f, Vector3.zero);
            enemy = SpawnFixture(new Vector3(0f, 1.3f, 0f));
            motor.SetMoveInput(Vector2.up);
            Check(motor.TryRequestDash(), "Actual scoring collision fixture dash accepted");
            yield return () => enemy == null;
            Check(score.Score == 20 && hud.FeedbackText.Contains("+20"), "Real dash kill updates HUD score and one positive score event");
            ScreenCapture.CaptureScreenshot(EvidencePath("T09-score.png"));
            yield return Hold(1.3f, Vector3.zero);
            Check(hud.FeedbackText == "", "Transient feedback expires without stale score or damage text");
            AttachFixture();
            enemy = SpawnFixture(new Vector3(-5f, -5f, 0f));
            Check(manager.WinGame(), "Victory boundary reached");
            yield return Delay(0.04f);
            Check(hud.StateText == "挑战成功" && body.isKinematic && !motor.IsAttached && !enemy.IsRepelled &&
                !Physics.GetIgnoreCollision(body.GetComponent<Collider>(), enemy.GetComponent<Collider>()), "Victory UI and terminal freeze clear all attachment/repel pairs");
            ScreenCapture.CaptureScreenshot(EvidencePath("T09-victory.png"));
            yield return Delay(0.04f);
            int previous = manager.GetInstanceID();
            manager.RestartGame();
            yield return () => UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>().GetInstanceID() != previous;
            Bind(false);
            hud = UnityEngine.Object.FindObjectOfType<OrbitBreakerScoreDisplay>();
            Check(hud.StateText == "准备就绪" && hud.FeedbackText == "" && score.Score == 0 && !motor.IsAttached, "Real reload resets HUD and protection state");
            manager.StartGame();
            Arrange(new Vector3(0f, -9.8f, 0f), Vector3.down * 10f);
            yield return () => manager.State == GameState.Defeat;
            Check(hud.StateText == "漏球失败" && hud.DashText == "本局已结束", "Actual drain presents defeat UI and no stale active ability");
            ScreenCapture.CaptureScreenshot(EvidencePath("T09-defeat.png"));
            yield return Delay(0.04f);
        }
        private static IEnumerator<Func<bool>> KeyboardChecks()
        {
            Bind(true);
            Report("RUNNING", "Focus Game and press Enter. Runtime-only 120s attachment allows real mouse/tool input; production 7.5s checked separately.");
            yield return () => manager.IsPlaying;
            var clone = UnityEngine.Object.Instantiate(manager.Config);
            clone.hideFlags = HideFlags.DontSave;
            clone.attachDuration = 120f;
            var settings = new SerializedObject(manager);
            settings.FindProperty("config").objectReferenceValue = clone;
            settings.ApplyModifiedPropertiesWithoutUndo();
            Vector3 position = (Vector3)Point.Center + Vector3.down * OrbitBreakerAttachmentPoint.OrbitRadius;
            Report("RUNNING", "Press E near LEFT green post; then move cursor around post to build charge. Only Space fires.");
            yield return () => { if (motor.IsAttached) return true; Arrange(position, Vector3.zero); return false; };
            Check(motor.IsAttached, "Real E attaches through OrbitBreakerPlayerInput");
            var attacker = SpawnFixture(body.position + Vector3.down * 0.9f);
            yield return () => attacker.IsRepelled;
            Check(motor.IsAttached && !motor.IsStunned && score.Score == 0, "Real E shield survives actual enemy and repels it without score");
            ScreenCapture.CaptureScreenshot(EvidencePath("T09-keyboard-repel.png"));
            ClearEnemies();
            float angle = Mathf.Atan2(body.position.y - Point.Center.y, body.position.x - Point.Center.x);
            yield return () => motor.AttachmentCharge > 0.99f;
            Check(motor.IsAttached && !motor.IsDashing && dashStarts == 0, "Real mouse movement physically orbits, charges, and does not auto fire");
            Report("RUNNING", "Charge full. Aim outward from the post, press Space to launch.");
            ScreenCapture.CaptureScreenshot(EvidencePath("T09-keyboard-charged.png"));
            Vector2 acceptedAim = Vector2.zero;
            float actualSpeed = 0f;
            motor.DashStarted += () => { actualSpeed = body.velocity.magnitude; acceptedAim = motor.DashDirection; };
            yield return () => dashStarts > 0;
            Check(!motor.IsAttached && actualSpeed > 23.9f && motor.IsDashing, "Real Space launches fully charged at 24; direction=" + acceptedAim);
            yield return Hold(1.1f, Vector3.zero);
            Report("RUNNING", "Press E to attach again, then E once more to release.");
            yield return () => { if (motor.IsAttached) return true; Arrange(position, Vector3.zero); return false; };
            yield return () => !motor.IsAttached;
            Check(!motor.IsDashing && dashStarts == 1, "Second real E releases without dash");
            Arrange(new Vector3(0f, -9.8f, 0f), Vector3.down * 10f);
            yield return () => manager.State == GameState.Defeat;
            int previous = manager.GetInstanceID();
            Report("RUNNING", "Actual drain reached. Press R to reload.");
            yield return () => UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>().GetInstanceID() != previous;
            Bind(true);
            Check(manager.State == GameState.Ready && !motor.IsAttached && manager.Config.attachDuration == 7.5f,
                "Real R reload restores production config and clears attachment");
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
                var observer = new GameObject("T09_ValidationProbe") { hideFlags = HideFlags.DontSave };
                UnityEngine.Object.DontDestroyOnLoad(observer);
                observer.AddComponent<OrbitBreaker.Testing.OrbitBreakerT03Probe>().Observe = Tick;
            }
            if (state != PlayModeStateChange.EnteredEditMode) return;
            if (!SessionState.GetBool(Prefix + "Complete", false)) Report("FAIL", "Play mode exited before completion.");
            else Debug.Log("[OrbitBreaker] T09 validation PASS.");
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
            File.WriteAllText(EvidencePath("T09-" + SessionState.GetString(Prefix + "Mode", "physics") + ".json"),
                JsonUtility.ToJson(new Result { status = status, observedAtUtc = DateTime.UtcNow.ToString("O"),
                    unityVersion = Application.unityVersion, detail = detail,
                    checks = SessionState.GetString(Prefix + "Checks", "").Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries) }, true));
        }
    }
}













