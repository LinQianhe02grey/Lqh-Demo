using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OrbitBreaker.Editor
{
    [InitializeOnLoad]
    public static class OrbitBreakerT19KeyboardValidation
    {
        private const string Prefix = "OrbitBreaker.T19Keyboard.";
        private static IEnumerator<Func<bool>> steps;
        private static Func<bool> waiting;
        private static OrbitBreakerPlayerMotor motor;
        private static OrbitBreakerGameManager manager;
        private static OrbitBreakerScoreManager score;
        private static float originalMaximumDeltaTime;
        private static readonly List<string> checks = new List<string>();
        static OrbitBreakerT19KeyboardValidation() { EditorApplication.playModeStateChanged += OnPlayState; EditorApplication.update += Timeout; }
        [MenuItem("Tools/Orbit Breaker/Check T19 Mouse Keyboard")]
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
            Bind();body=motor.GetComponent<Rigidbody>();Report("RUNNING","Press Enter or click Free mode.");yield return ()=>manager.IsPlaying;
            UnityEngine.Object.FindObjectOfType<OrbitBreakerEnemySpawner>().enabled=false;
            var arena=UnityEngine.Object.FindObjectOfType<OrbitBreakerProceduralArena>();arena.gameObject.SetActive(false);
            typeof(OrbitBreakerPlayerMotor).GetField("<DashCharges>k__BackingField",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(motor,3);
            bool fired=false;float capturedSpeed=0;Vector2 actual=Vector2.zero;int charges=0;bool focusAtFire=true;
            motor.DashStarted+=()=>{fired=true;capturedSpeed=body.velocity.magnitude;actual=body.velocity;charges=motor.DashCharges;focusAtFire=motor.IsFocusActive;};
            Report("RUNNING","Right click in Game; first focus should expire by itself in2 real seconds.");
            bool rawRight=false;
            yield return ()=>{Place(Vector3.zero,Vector3.right*21);rawRight|=Input.GetMouseButtonDown(1);return motor.IsFocusActive;};
            float start=Time.unscaledTime;Check(rawRight&&Time.timeScale==.2f,"Real right click starts .2x slow motion");
            yield return ()=>{Place(Vector3.zero,Vector3.right*21);return !motor.IsFocusActive;};
            Check(Time.unscaledTime-start>=1.85f&&Time.unscaledTime-start<2.2f&&Time.timeScale==1,"Real right-click focus expires in2 real seconds without input");
            Report("RUNNING","Right click again, then Space. Input-only fixture holds focus after activation for desktop latency; real2s expiry tested above.");
            bool spaceDuringFocus=false;float deadline=Time.unscaledTime+120;
            yield return ()=>
            {
                spaceDuringFocus|=Input.GetKeyDown(KeyCode.Space)&&motor.IsFocusActive;
                if(!fired){Place(Vector3.zero,Vector3.right*21);if(motor.IsFocusActive)typeof(OrbitBreakerPlayerMotor).GetField("focusEnd",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(motor,Time.unscaledTime+2f);}
                return fired||Time.unscaledTime>deadline;
            };
            Check(fired&&spaceDuringFocus&&!focusAtFire&&Time.timeScale==1&&charges==2&&Mathf.Abs(capturedSpeed-21)<.1f&&actual.sqrMagnitude>400,
                "Real Space in input-only extended-focus fixture fires at retained21 speed, spends one capsule and restores time; speed="+capturedSpeed+", observedDuringFocus="+spaceDuringFocus);
            ScreenCapture.CaptureScreenshot(Evidence("T19-real-space.png"));yield return Delay(.15f);
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
        private static void Fail(string detail) { Report("FAIL", detail); SessionState.SetBool(Prefix+"Running", false); Debug.LogError("T19: " + detail); EditorApplication.isPlaying = false; }
        private static void Timeout() { if (SessionState.GetBool(Prefix+"Running", false) && DateTime.UtcNow > DateTime.Parse(SessionState.GetString(Prefix+"Deadline", ""), null, System.Globalization.DateTimeStyles.RoundtripKind)) Fail("T19 deadline exceeded."); }
        private static void OnPlayState(PlayModeStateChange state)
        {
            if(state==PlayModeStateChange.EnteredEditMode)Time.timeScale=1;
            if (state == PlayModeStateChange.EnteredEditMode && originalMaximumDeltaTime > 0f) { Time.maximumDeltaTime = originalMaximumDeltaTime; originalMaximumDeltaTime = 0f; }
            if (!SessionState.GetBool(Prefix+"Running", false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                checks.Clear(); waiting = null; steps = RunChecks();
                originalMaximumDeltaTime = Time.maximumDeltaTime; Time.maximumDeltaTime = Time.fixedDeltaTime;
                var probe = new GameObject("T19_ValidationProbe") { hideFlags=HideFlags.DontSave };
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
        private static void Report(string status, string detail) => File.WriteAllText(Evidence("T19-keyboard.json"), JsonUtility.ToJson(new Result {status=status, observedAtUtc=DateTime.UtcNow.ToString("O"), unityVersion=Application.unityVersion, detail=detail, checks=checks.ToArray()}, true));
    }
}

