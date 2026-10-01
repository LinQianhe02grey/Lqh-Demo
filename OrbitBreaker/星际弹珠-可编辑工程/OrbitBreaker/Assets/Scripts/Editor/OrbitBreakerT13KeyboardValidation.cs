using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OrbitBreaker.Editor
{
    [InitializeOnLoad]
    public static class OrbitBreakerT13KeyboardValidation
    {
        private const string Prefix = "OrbitBreaker.T13Keys.";
        private static IEnumerator<Func<bool>> steps;
        private static Func<bool> waiting;
        private static OrbitBreakerPlayerMotor motor;
        private static OrbitBreakerGameManager manager;
        private static OrbitBreakerScoreManager score;
        private static float originalMaximumDeltaTime;
        private static readonly List<string> checks = new List<string>();
        static OrbitBreakerT13KeyboardValidation() { EditorApplication.playModeStateChanged += OnPlayState; EditorApplication.update += Timeout; }
        [MenuItem("Tools/Orbit Breaker/Check T13 Keyboard")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Start from Edit mode.");
            OrbitBreakerSceneBuilder.Build();
            SessionState.SetBool(Prefix + "Running", true);
            SessionState.SetBool(Prefix + "Complete", false);
            SessionState.SetString(Prefix + "Deadline", DateTime.UtcNow.AddMinutes(6).ToString("O"));
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
            Bind();motor.GetComponent<OrbitBreakerPlayerInput>().enabled=true;
            UnityEngine.Object.FindObjectOfType<OrbitBreakerEnemySpawner>().enabled=false;
            Report("RUNNING","Focus Game, press Enter.");
            yield return ()=>manager.IsPlaying;
            Check(manager.IsPlaying,"Real Enter starts production OrbitBreakerGameManager");
            var rb=motor.GetComponent<Rigidbody>();
            var clone=UnityEngine.Object.Instantiate(manager.Config);clone.hideFlags=HideFlags.DontSave;clone.attachDuration=120;
            var settings=new SerializedObject(manager);settings.FindProperty("config").objectReferenceValue=clone;settings.ApplyModifiedPropertiesWithoutUndo();
            bool w=false,a=false,s=false,d=false;
            Report("RUNNING","Press W, A, S, D in Game. Isolated input fixture holds player safely; only runtime attach limit temporarily 120s for tool latency.");
            yield return ()=>{w|=motor.MoveInput.y>.5f;a|=motor.MoveInput.x<-.5f;s|=motor.MoveInput.y<-.5f;d|=motor.MoveInput.x>.5f;rb.position=new Vector3(0,-1,0);rb.velocity=Vector3.zero;return w&&a&&s&&d;};
            Check(true,"Real W/A/S/D reaches OrbitBreakerPlayerInput and correct motor directions");
            foreach(var flipper in UnityEngine.Object.FindObjectsOfType<OrbitBreakerFlipperController>().OrderBy(f=>f.Side))
            {
                bool activated=false;
                System.Action observed=()=>activated=true;
                flipper.Activated+=observed;
                Report("RUNNING",flipper.Side==FlipperSide.Left?"Press J for left flipper.":"Press K for right flipper.");
                yield return ()=>{if(activated)return true;rb.position=flipper.transform.position;rb.velocity=Vector3.zero;return false;};
                yield return Delay(.04f);
                flipper.Activated-=observed;
                Check(motor.IsLaunched && motor.IsInvulnerable && rb.velocity.y>12,"Real "+flipper.Side+" key creates physical protected launch and combo");
            }
            var point=UnityEngine.Object.FindObjectsOfType<OrbitBreakerAttachmentPoint>().OrderBy(t=>t.Center.x).First();
            Report("RUNNING","Press E near LEFT green orbit point; move mouse around it to charge, then Space.");
            yield return ()=>{if(motor.IsAttached)return true;rb.position=(Vector3)point.Center+Vector3.down*OrbitBreakerAttachmentPoint.OrbitRadius;rb.velocity=Vector3.zero;return false;};
            Check(motor.IsAttached && motor.IsInvulnerable,"Real E enters protected orbit");
            yield return ()=>motor.AttachmentCharge>.99f;
            Check(motor.IsAttached && motor.GetComponent<OrbitBreakerPlayerFeedback>().OrbitVfxActive,"Real mouse movement charges actual rotation with VFX and no auto-fire");
            ScreenCapture.CaptureScreenshot(Evidence("T13-real-orbit.png"));
            float speed=0;int launches=0;motor.DashStarted+=()=>{speed=rb.velocity.magnitude;launches++;};
            Report("RUNNING","Charge full. Aim into the table and press Space.");
            yield return ()=>launches>0;
            Check(speed>23.9f && !motor.IsAttached && motor.IsLaunched,"Real Space fires charged planet at 24 with launch protection");
            rb.position=new Vector3(0,-9.6f,0);rb.velocity=Vector3.down*18;
            yield return ()=>manager.State==GameState.Defeat;
            int previous=manager.GetInstanceID();
            Report("RUNNING","Black hole reached. Press R to restart.");
            yield return ()=>UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>().GetInstanceID()!=previous;
            Bind();
            Check(manager.State==GameState.Ready && score.Score==0 && motor.ComboCount==0 && manager.Config.attachDuration==7.5f,
                "Real R reload clears score/combo/states and restores saved 7.5-second attachment");
            UnityEngine.Object.Destroy(clone);
        }
        private static Func<bool> Delay(float duration) { float end = Time.time + duration; return () => Time.time >= end; }
        private static void Tick()
        {
            try
            {
                if (waiting != null && !waiting()) return;
                if (steps.MoveNext()) waiting = steps.Current;
                else { SessionState.SetBool(Prefix + "Complete", true); Report("PASS", "All T13 integration checks passed."); EditorApplication.isPlaying = false; }
            }
            catch (Exception ex) { Fail(ex.ToString()); }
        }
        private static void Check(bool pass, string detail) { if (!pass) throw new InvalidOperationException(detail.Length > 0 ? detail : "Attachment expired before 7.35 seconds."); if (detail.Length > 0) checks.Add(detail); }
        private static void Fail(string detail) { Report("FAIL", detail); SessionState.SetBool(Prefix+"Running", false); Debug.LogError("T13: " + detail); EditorApplication.isPlaying = false; }
        private static void Timeout() { if (SessionState.GetBool(Prefix+"Running", false) && DateTime.UtcNow > DateTime.Parse(SessionState.GetString(Prefix+"Deadline", ""), null, System.Globalization.DateTimeStyles.RoundtripKind)) Fail("T13 deadline exceeded."); }
        private static void OnPlayState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode && originalMaximumDeltaTime > 0f) { Time.maximumDeltaTime = originalMaximumDeltaTime; originalMaximumDeltaTime = 0f; }
            if (!SessionState.GetBool(Prefix+"Running", false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                checks.Clear(); waiting = null; steps = RunChecks();
                originalMaximumDeltaTime = Time.maximumDeltaTime; Time.maximumDeltaTime = Time.fixedDeltaTime;
                var probe = new GameObject("T13_ValidationProbe") { hideFlags=HideFlags.DontSave };
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
        private static void Report(string status, string detail) => File.WriteAllText(Evidence("T13-keyboard.json"), JsonUtility.ToJson(new Result {status=status, observedAtUtc=DateTime.UtcNow.ToString("O"), unityVersion=Application.unityVersion, detail=detail, checks=checks.ToArray()}, true));
    }
}
