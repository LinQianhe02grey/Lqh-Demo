using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OrbitBreaker.Editor
{
    [InitializeOnLoad]
    public static class OrbitBreakerT18KeyboardValidation
    {
        private const string Prefix = "OrbitBreaker.T18Keyboard.";
        private static IEnumerator<Func<bool>> steps;
        private static Func<bool> waiting;
        private static OrbitBreakerPlayerMotor motor;
        private static OrbitBreakerGameManager manager;
        private static OrbitBreakerScoreManager score;
        private static float originalMaximumDeltaTime;
        private static readonly List<string> checks = new List<string>();
        static OrbitBreakerT18KeyboardValidation() { EditorApplication.playModeStateChanged += OnPlayState; EditorApplication.update += Timeout; }
        [MenuItem("Tools/Orbit Breaker/Check T18 Mouse Keyboard")]
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
            Bind();body=motor.GetComponent<Rigidbody>();Report("RUNNING","Click Free mode or press real Enter.");yield return ()=>manager.IsPlaying;
            Check(manager.Mode==GameMode.Free,"Real main-menu input starts Free mode");
            var inputConfig=UnityEngine.Object.Instantiate(manager.Config);inputConfig.attachDuration=20;
            var so=new SerializedObject(manager);so.FindProperty("config").objectReferenceValue=inputConfig;so.ApplyModifiedPropertiesWithoutUndo();
            Check(manager.Config.attachDuration==20,"Input fixture extends attachment to20s only in runtime clone for desktop tool latency; saved default remains7.5s");UnityEngine.Object.FindObjectOfType<OrbitBreakerEnemySpawner>().enabled=false;
            var arena=UnityEngine.Object.FindObjectOfType<OrbitBreakerProceduralArena>();arena.gameObject.SetActive(false);
            var point=UnityEngine.Object.Instantiate(arena.templates[4],new Vector3(1,3.2f),Quaternion.identity).GetComponent<OrbitBreakerAttachmentPoint>();point.gameObject.SetActive(true);point.transform.localScale*=.8f;
            var target=UnityEngine.Object.Instantiate(arena.templates[0],new Vector3(7,3),Quaternion.identity).GetComponent<OrbitBreakerBumper>();target.gameObject.SetActive(true);target.transform.localScale*=.8f;
            typeof(OrbitBreakerPlayerMotor).GetField("<DashCharges>k__BackingField",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(motor,1);
            var selected=motor.GetComponent<OrbitBreakerTargetLock>();
            Report("RUNNING","Fixture: one capsule, green magnet above, cyan target right. Click right pillar, then E, then Space after spin-up.");
            yield return ()=>{Place(new Vector3(1,1),Vector3.zero);return selected.HasTarget;};
            Check(selected.Target==target.transform,"Real mouse click selects right pillar through perspective camera");
            yield return ()=>{if(!motor.IsAttached)Place(new Vector3(1,1),Vector3.zero);return motor.IsAttached;};
            Check(motor.IsAttached,"Real E enters automatic orbit from 2.2m");
            bool fired=false;Vector2 actual=Vector2.zero,expected=Vector2.zero;float speed=0;int charges=-1;
            motor.DashStarted+=()=>{fired=true;actual=motor.DashDirection;expected=((Vector2)target.transform.position-(Vector2)motor.Position).normalized;speed=body.velocity.magnitude;charges=motor.DashCharges;};
            yield return Delay(2.4f);Check(motor.IsAttached&&motor.AttachmentCharge>.95f&&motor.CurrentSpeed>23.5f,"Real E auto-accelerates to top speed without mouse circling");
            Time.timeScale=.1f;
            ScreenCapture.CaptureScreenshot(Evidence("T18-real-orbit.png"));Report("RUNNING","Charged. Press real Space before20s fixture attachment expires.");
            float deadline=Time.time+motor.AttachmentRemaining+.2f;
            bool rawSpace=false;yield return ()=>{rawSpace|=Input.GetKeyDown(KeyCode.Space);return fired||Time.time>=deadline;};
            Time.timeScale=1;
            Check(fired&&Vector2.Dot(actual,expected)>.995f&&speed>23.5f&&charges==0&&!selected.HasTarget,"Real Space aims at clicked pillar, releases and spends capsule; speed="+speed+", rawSpace="+rawSpace+", charges="+charges);
            ScreenCapture.CaptureScreenshot(Evidence("T18-real-space.png"));yield return Delay(.1f);
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
        private static void Fail(string detail) { Report("FAIL", detail); SessionState.SetBool(Prefix+"Running", false); Debug.LogError("T18: " + detail); EditorApplication.isPlaying = false; }
        private static void Timeout() { if (SessionState.GetBool(Prefix+"Running", false) && DateTime.UtcNow > DateTime.Parse(SessionState.GetString(Prefix+"Deadline", ""), null, System.Globalization.DateTimeStyles.RoundtripKind)) Fail("T18 deadline exceeded."); }
        private static void OnPlayState(PlayModeStateChange state)
        {
            if(state==PlayModeStateChange.EnteredEditMode)Time.timeScale=1;
            if (state == PlayModeStateChange.EnteredEditMode && originalMaximumDeltaTime > 0f) { Time.maximumDeltaTime = originalMaximumDeltaTime; originalMaximumDeltaTime = 0f; }
            if (!SessionState.GetBool(Prefix+"Running", false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                checks.Clear(); waiting = null; steps = RunChecks();
                originalMaximumDeltaTime = Time.maximumDeltaTime; Time.maximumDeltaTime = Time.fixedDeltaTime;
                var probe = new GameObject("T18_ValidationProbe") { hideFlags=HideFlags.DontSave };
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
        private static void Report(string status, string detail) => File.WriteAllText(Evidence("T18-keyboard.json"), JsonUtility.ToJson(new Result {status=status, observedAtUtc=DateTime.UtcNow.ToString("O"), unityVersion=Application.unityVersion, detail=detail, checks=checks.ToArray()}, true));
    }
}
