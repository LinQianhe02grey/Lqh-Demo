using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OrbitBreaker.Editor
{
    [InitializeOnLoad]
    public static class OrbitBreakerT17KeyboardValidation
    {
        private const string Prefix = "OrbitBreaker.T17Keyboard.";
        private static IEnumerator<Func<bool>> steps;
        private static Func<bool> waiting;
        private static OrbitBreakerPlayerMotor motor;
        private static OrbitBreakerGameManager manager;
        private static OrbitBreakerScoreManager score;
        private static float originalMaximumDeltaTime;
        private static readonly List<string> checks = new List<string>();
        static OrbitBreakerT17KeyboardValidation() { EditorApplication.playModeStateChanged += OnPlayState; EditorApplication.update += Timeout; }
        [MenuItem("Tools/Orbit Breaker/Check T17 Mouse Keyboard")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Start from Edit mode.");
            OrbitBreakerSceneBuilder.Build();
            SessionState.SetBool(Prefix + "Running", true);
            SessionState.SetBool(Prefix + "Complete", false);
            SessionState.SetString(Prefix + "Deadline", DateTime.UtcNow.AddMinutes(5).ToString("O"));
            checks.Clear();
            Report("RUNNING", "Entering Play.");
            EditorApplication.isPlaying = true;
        }
        private static void Bind()
        {
            motor = UnityEngine.Object.FindObjectOfType<OrbitBreakerPlayerMotor>();
            manager = motor.OrbitBreakerGameManager;
            score = UnityEngine.Object.FindObjectOfType<OrbitBreakerScoreManager>();
            Check(manager.State == GameState.Ready && score.Score == 0 && manager.Config.attachDuration == 7.5f,
                "Real reload starts Ready, score zero, production attachment 7.5 seconds");
        }
        private static Rigidbody body;
        private static void Place(Vector3 pos, Vector3 velocity)
        { body.position=pos; body.velocity=velocity; motor.SetMoveInput(Vector2.zero); Physics.SyncTransforms(); }
        private static Func<bool> SafeDelay(float duration)
        { float until=Time.time+duration;return ()=>{ Place(new Vector3(1,1,0),Vector3.zero);return Time.time>=until;}; }
        private static OrbitBreakerEnemyController Spawn(Vector3 pos)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/OrbitBreakerEnemy.prefab");
            var enemy=UnityEngine.Object.Instantiate(prefab,pos,Quaternion.identity).GetComponent<OrbitBreakerEnemyController>();
            enemy.Initialize(motor,score);return enemy;
        }
        private static IEnumerator<Func<bool>> RunChecks()
        {
            Bind();body=motor.GetComponent<Rigidbody>();Report("RUNNING","Waiting for real Enter.");yield return ()=>manager.IsPlaying;
            Check(manager.IsPlaying,"Real Enter starts production game");UnityEngine.Object.FindObjectOfType<OrbitBreakerEnemySpawner>().enabled=false;
            var pin=UnityEngine.Object.FindObjectsOfType<OrbitBreakerBumper>().OrderBy(b=>b.transform.position.sqrMagnitude).First();
            for(int i=0;i<4;i++){Place(pin.transform.position+Vector3.up*1.15f,Vector3.down*9);yield return Delay(.13f);yield return SafeDelay(.25f);}
            Check(motor.DashCharges==1,"Four physical collisions prepare one capsule for real keyboard test");
            var point=UnityEngine.Object.FindObjectsOfType<OrbitBreakerAttachmentPoint>().OrderBy(p=>p.transform.position.sqrMagnitude).First();
            Report("RUNNING","Aim upper-right and press real E. Player held 2.2 units from magnet.");
            yield return ()=>{if(!motor.IsAttached)Place((Vector3)point.Center+Vector3.down*2.2f,Vector3.zero);return motor.IsAttached;};
            Check(motor.IsAttached,"Real E attaches from expanded range");Vector2 mouse=motor.AttachmentAim;yield return Delay(.85f);
            Check(motor.IsAttached&&motor.AttachmentCharge>.9f,"Real E automatically spins and charges without circling mouse");
            ScreenCapture.CaptureScreenshot(Evidence("T17-auto-orbit.png"));
            bool fired=false;Vector2 actual=Vector2.zero,expected=Vector2.zero;float speed=0;
            motor.DashStarted+=()=>{fired=true;actual=motor.DashDirection;expected=(motor.AttachmentAim-(Vector2)motor.Position).normalized;speed=body.velocity.magnitude;};
            Report("RUNNING","Auto-orbit charged. Press real Space now.");
            yield return ()=>fired||!motor.IsAttached;
            Check(fired&&Vector2.Dot(actual,expected)>.999f&&speed>22&&motor.DashCharges==0,"Real Space releases charged automatic orbit toward mouse, spends capsule, speed="+speed);
            ScreenCapture.CaptureScreenshot(Evidence("T17-real-space.png"));yield return Delay(.1f);
        }
        private static Func<bool> Delay(float duration) { float end = Time.time + duration; return () => Time.time >= end; }
        private static void Tick()
        {
            try
            {
                if (waiting != null && !waiting()) return;
                if (steps.MoveNext()) waiting = steps.Current;
                else { SessionState.SetBool(Prefix + "Complete", true); Report("PASS", "Real keyboard and mouse checks passed."); EditorApplication.isPlaying = false; }
            }
            catch (Exception ex) { Fail(ex.ToString()); }
        }
        private static void Check(bool pass, string detail) { if (!pass) throw new InvalidOperationException(detail.Length > 0 ? detail : "Attachment expired before 7.35 seconds."); if (detail.Length > 0) checks.Add(detail); }
        private static void Fail(string detail) { Report("FAIL", detail); SessionState.SetBool(Prefix+"Running", false); Debug.LogError("T17: " + detail); EditorApplication.isPlaying = false; }
        private static void Timeout() { if (SessionState.GetBool(Prefix+"Running", false) && DateTime.UtcNow > DateTime.Parse(SessionState.GetString(Prefix+"Deadline", ""), null, System.Globalization.DateTimeStyles.RoundtripKind)) Fail("T17 deadline exceeded."); }
        private static void OnPlayState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode && originalMaximumDeltaTime > 0f) { Time.maximumDeltaTime = originalMaximumDeltaTime; originalMaximumDeltaTime = 0f; }
            if (!SessionState.GetBool(Prefix+"Running", false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                checks.Clear(); waiting = null; steps = RunChecks();
                originalMaximumDeltaTime = Time.maximumDeltaTime; Time.maximumDeltaTime = Time.fixedDeltaTime;
                var probe = new GameObject("T17_ValidationProbe") { hideFlags=HideFlags.DontSave };
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
        private static void Report(string status, string detail) => File.WriteAllText(Evidence("T17-keyboard.json"), JsonUtility.ToJson(new Result {status=status, observedAtUtc=DateTime.UtcNow.ToString("O"), unityVersion=Application.unityVersion, detail=detail, checks=checks.ToArray()}, true));
    }
}
