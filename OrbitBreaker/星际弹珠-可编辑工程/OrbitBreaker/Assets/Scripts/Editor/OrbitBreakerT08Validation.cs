using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OrbitBreaker.Editor
{
    [InitializeOnLoad]
    public static class OrbitBreakerT08Validation
    {
        private const string Prefix = "OrbitBreaker.T08.";
        private static IEnumerator<Func<bool>> steps;
        private static Func<bool> waiting;
        private static OrbitBreakerPlayerMotor motor;
        private static Rigidbody body;
        private static OrbitBreakerGameManager manager;
        private static OrbitBreakerScoreManager score;
        private static OrbitBreakerEnemySpawner spawner;
        private static int hits, dashStarts;
        private static float originalMaximumDeltaTime;

        static OrbitBreakerT08Validation()
        {
            EditorApplication.update += CheckTimeout;
            EditorApplication.playModeStateChanged += OnPlayState;
        }
        [MenuItem("Tools/Orbit Breaker/Validate T08")]
        public static void Run() => Begin("physics");
        [MenuItem("Tools/Orbit Breaker/Check T08 Keyboard")]
        public static void Keyboard() => Begin("keyboard");

        private static void Begin(string mode)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Start T08 from idle Edit mode.");
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
            Fail("T08 timed out. Completed checks remain in the report.");
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
            Debug.LogError("[OrbitBreaker] T08 validation failed: " + detail);
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
            Check(UnityEngine.Object.FindObjectsOfType<OrbitBreakerAttachmentPoint>().Length == 2, "Two saved green attachment points");
            Check(!motor.TryAttach(Point) && !motor.TryRequestDash(), "Ready refuses attachment and dash");
            manager.StartGame();
            Arrange(Vector3.zero, Vector3.zero);
            Check(!motor.TryAttach(Point), "Distant attachment refused");
            AttachFixture();
            Vector3 initial = body.position;
            motor.SetMoveInput(Vector2.one);
            yield return Delay(0.3f);
            Check(motor.IsAttached && !body.isKinematic && body.velocity.magnitude < 0.01f && Vector3.Distance(initial, body.position) < 0.01f &&
                motor.MoveInput == Vector2.zero && motor.AttachmentCharge == 0f, "Stationary attachment suspends gravity and steering, stays dynamic, never charges");
            Check(motor.GetComponent<OrbitBreakerPlayerMagnet>().ToggleAttachment() && !motor.IsAttached && !motor.TryAttach(Point), "E request toggles release; immediate reattach refused");
            yield return Hold(0.6f, initial);
            Check(!motor.TryAttach(Point), "Waiting cooldown inside capture zone cannot renew hovering");
            yield return Hold(0.6f, Vector3.zero);
            AttachFixture();
            yield return Delay(manager.Config.attachDuration + 0.12f);
            Check(!motor.IsAttached && body.velocity.y < -0.5f && dashStarts == 0, "Configured 7.5-second timeout drops ball without auto launch");
            yield return Hold(0.65f, Vector3.zero);
            AttachFixture();
            motor.SetAttachmentAim(body.position + new Vector3(3f, 4f));
            Check(motor.TryRequestDash(), "Space request launches uncharged attachment");
            yield return Delay(0.04f);
            Check(!motor.IsAttached && motor.IsDashing && Vector2.Dot(motor.DashDirection, new Vector2(3f, 4f).normalized) > 0.999f &&
                body.velocity.magnitude > 17f && body.velocity.magnitude < 19f, "Uncharged launch uses mouse direction and base speed 18");
            yield return Hold(1.1f, Vector3.zero);
            AttachFixture();
            yield return Spin(0.85f);
            Check(motor.IsAttached && motor.AttachmentCharge > 0.99f && !motor.IsDashing && dashStarts == 1,
                "Actual orbit charges fully without automatic firing; radius=" + Vector2.Distance(body.position, Point.Center));
            Check(Mathf.Abs(Vector2.Distance(body.position, Point.Center) - OrbitBreakerAttachmentPoint.OrbitRadius) < 0.08f && Mathf.Abs(body.position.z) < 0.001f,
                "Orbit keeps radius and XY constraint");
            ScreenCapture.CaptureScreenshot(EvidencePath("T08-charged.png"));
            // Aim outward so the central solid post cannot intercept this real dash kill.
            Vector2 outward = ((Vector2)body.position - Point.Center).normalized;
            var enemy = SpawnFixture(body.position + (Vector3)outward * 1.5f);
            motor.SetAttachmentAim((Vector2)body.position + outward * 4f);
            float launchSpeed = 0f;
            motor.DashStarted += () => launchSpeed = body.velocity.magnitude;
            Check(motor.TryRequestDash(), "Charged Space launch accepted");
            yield return () => enemy == null;
            Check(launchSpeed > 23.9f && launchSpeed <= OrbitBreakerPlayerMotor.MaximumSpeed + 0.01f && score.Score == 20 && !motor.IsStunned,
                "Charged launch reaches 24 (base 18), real enemy collision kills for 20");
            yield return Hold(1.1f, Vector3.zero);
            AttachFixture();
            motor.SetAttachmentAim(Vector2.zero, false);
            Check(!motor.TryRequestDash() && motor.IsAttached, "Outside/invalid mouse aim refuses launch without losing attachment");
            enemy = SpawnFixture(body.position + Vector3.down * 1.15f);
            int beforeHits = hits;
            yield return () => enemy.IsRepelled;
            Check(motor.IsAttached && !motor.IsStunned && hits == beforeHits && score.Score == 20,
                "T09 user revision: enemy contact repels without breaking attachment or scoring");
            ClearEnemies();
            motor.ReleaseAttachment();
            yield return Hold(1.1f, Vector3.zero);
            AttachFixture();
            Check(motor.TryReceiveHit() && motor.TryRequestBoardImpulse(Vector2.up, 18f), "Hit and rescue can interrupt attachment in same step");
            yield return Delay(0.04f);
            Check(!motor.IsAttached && motor.IsStunned && body.velocity.y > 16f, "Rescue impulse survives hit while movement remains stunned");
            yield return Hold(1.1f, Vector3.zero);
            AttachFixture();
            var point = Point;
            point.gameObject.SetActive(false);
            yield return Delay(0.04f);
            Check(!motor.IsAttached && body.velocity.y < 0f, "Disabled attachment point releases safely");
            point.gameObject.SetActive(true);
            yield return Hold(0.65f, Vector3.zero);
            AttachFixture();
            var obstacle = new GameObject("T08_RuntimeCollisionFixture");
            obstacle.transform.position = (Vector3)Point.Center + new Vector3(1.35f, -0.2f, 0f);
            obstacle.AddComponent<BoxCollider>().size = new Vector3(0.2f, 0.4f, 2f);
            Physics.SyncTransforms();
            motor.SetAttachmentAim(Point.Center + Vector2.right * 4f);
            yield return () => !motor.IsAttached;
            Check(body.position.x < Point.Center.x + 1.5f, "Dynamic orbit collides and releases instead of crossing a solid obstacle");
            UnityEngine.Object.Destroy(obstacle);
            yield return Hold(0.65f, Vector3.zero);
            AttachFixture();
            motor.ReleaseAttachment();
            yield return Delay(0.04f);
            Check(body.velocity.y < 0f, "Manual release resumes gravity immediately");
            yield return Hold(0.65f, Vector3.zero);
            AttachFixture();
            var magnet = motor.GetComponent<OrbitBreakerPlayerMagnet>();
            magnet.enabled = false;
            Check(!motor.IsAttached && !magnet.ToggleAttachment(), "Disabled magnet releases and ignores further input calls");
            magnet.enabled = true;
            yield return Hold(0.65f, Vector3.zero);
            Arrange((Vector3)Point.Center + Vector3.down * OrbitBreakerAttachmentPoint.OrbitRadius, Vector3.zero);
            motor.SetMoveInput(Vector2.right);
            Check(motor.TryRequestDash() && motor.TryAttach(Point), "Attachment supersedes queued dash");
            motor.SetAttachmentAim(Point.Center + Vector2.down * 4f);
            int beforePriorityDash = dashStarts;
            yield return Delay(0.04f);
            Check(motor.IsAttached && !motor.IsDashing && dashStarts == beforePriorityDash, "Queued dash cannot leak into attachment");
            motor.ReleaseAttachment();
            yield return Hold(0.65f, Vector3.zero);
            Arrange((Vector3)Point.Center + Vector3.down * OrbitBreakerAttachmentPoint.OrbitRadius, Vector3.zero);
            motor.SetMoveInput(Vector2.right);
            Check(motor.TryRequestDash(), "Active dash fixture queued");
            yield return Delay(0.02f);
            Check(motor.IsDashing && motor.TryAttach(Point) && !motor.IsDashing && motor.DashCooldownRemaining > 0f,
                "Attachment interrupts active dash but preserves cooldown");
            Check(!motor.TryRequestDash(), "Attachment launch cannot bypass the normal dash cooldown");
            Check(manager.WinGame(), "Victory supersedes attachment");
            yield return Delay(0.04f);
            Check(!motor.IsAttached && body.isKinematic && !motor.TryAttach(Point) && !motor.TryRequestDash(), "Terminal state cancels attachment and all requests");
            int previous = manager.GetInstanceID();
            manager.RestartGame();
            yield return () => UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>().GetInstanceID() != previous;
            Bind(false);
            Check(!motor.IsAttached && motor.AttachmentCharge == 0f && motor.DashCooldownRemaining == 0f && score.Score == 0,
                "Real reload resets attachment, charge, cooldown, score");
            manager.StartGame();
            AttachFixture();
            Arrange(new Vector3(0f, -9.8f, 0f), Vector3.down * 10f);
            yield return () => manager.State == GameState.Defeat;
            Check(!motor.IsAttached && body.isKinematic, "Displaced attachment releases and real drain still defeats");
            Vector3 screen = Camera.main.WorldToScreenPoint(new Vector3(2f, 4f, 0f));
            Check(OrbitBreakerPlayerMagnet.TryProjectMouse(Camera.main, screen, out Vector2 projected) && Vector2.Distance(projected, new Vector2(2f, 4f)) < 0.001f &&
                !OrbitBreakerPlayerMagnet.TryProjectMouse(Camera.main, new Vector3(-100f, -100f, 0f), out projected), "Perspective screen ray maps to XY plane and rejects offscreen cursor");
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
            float angle = Mathf.Atan2(body.position.y - Point.Center.y, body.position.x - Point.Center.x);
            yield return () => motor.AttachmentCharge > 0.99f;
            Check(motor.IsAttached && !motor.IsDashing && dashStarts == 0, "Real mouse movement physically orbits, charges, and does not auto fire");
            Report("RUNNING", "Charge full. Aim outward from the post, press Space to launch.");
            ScreenCapture.CaptureScreenshot(EvidencePath("T08-keyboard-charged.png"));
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
                var observer = new GameObject("T08_ValidationProbe") { hideFlags = HideFlags.DontSave };
                UnityEngine.Object.DontDestroyOnLoad(observer);
                observer.AddComponent<OrbitBreaker.Testing.OrbitBreakerT03Probe>().Observe = Tick;
            }
            if (state != PlayModeStateChange.EnteredEditMode) return;
            if (!SessionState.GetBool(Prefix + "Complete", false)) Report("FAIL", "Play mode exited before completion.");
            else Debug.Log("[OrbitBreaker] T08 validation PASS.");
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
            File.WriteAllText(EvidencePath("T08-" + SessionState.GetString(Prefix + "Mode", "physics") + ".json"),
                JsonUtility.ToJson(new Result { status = status, observedAtUtc = DateTime.UtcNow.ToString("O"),
                    unityVersion = Application.unityVersion, detail = detail,
                    checks = SessionState.GetString(Prefix + "Checks", "").Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries) }, true));
        }
    }
}











