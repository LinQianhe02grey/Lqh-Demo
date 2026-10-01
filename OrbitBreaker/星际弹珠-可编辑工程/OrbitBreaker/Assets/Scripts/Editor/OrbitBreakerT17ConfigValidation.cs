using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OrbitBreaker.Editor
{
    [InitializeOnLoad]
    public static class OrbitBreakerT17ConfigValidation
    {
        private const string Prefix = "OrbitBreaker.T17Config.";
        private static IEnumerator<Func<bool>> steps;
        private static Func<bool> waiting;
        private static OrbitBreakerPlayerMotor motor;
        private static OrbitBreakerGameManager manager;
        private static OrbitBreakerScoreManager score;
        private static float originalMaximumDeltaTime;
        private static readonly List<string> checks = new List<string>();
        static OrbitBreakerT17ConfigValidation() { EditorApplication.playModeStateChanged += OnPlayState; EditorApplication.update += Timeout; }
        [MenuItem("Tools/Orbit Breaker/Validate T17 Config Audio")]
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
        private static IEnumerator<Func<bool>> RunChecks()
        {
            motor=UnityEngine.Object.FindObjectOfType<OrbitBreakerPlayerMotor>();manager=motor.OrbitBreakerGameManager;score=UnityEngine.Object.FindObjectOfType<OrbitBreakerScoreManager>();var body=motor.GetComponent<Rigidbody>();
            motor.GetComponent<OrbitBreakerPlayerInput>().enabled=false;UnityEngine.Object.Destroy(motor.GetComponent<OrbitBreakerPlayerInput>());
            var config=UnityEngine.Object.Instantiate(manager.Config);config.enemySpawnInterval=.4f;config.attachDuration=1.2f;config.targetScore=500;
            var so=new SerializedObject(manager);so.FindProperty("config").objectReferenceValue=config;so.ApplyModifiedPropertiesWithoutUndo();
            manager.StartGame();var spawner=UnityEngine.Object.FindObjectOfType<OrbitBreakerEnemySpawner>();int spawned=0;spawner.Spawned+=e=>spawned++;
            float start=Time.time;yield return ()=>{body.position=new Vector3(1,1);body.velocity=Vector3.zero;return Time.time-start>1.3f;};
            Check(spawned==3,"Edited .4-second interval actually spawns three enemies in 1.3 seconds");spawner.enabled=false;
            foreach(var e in UnityEngine.Object.FindObjectsOfType<OrbitBreakerEnemyController>())UnityEngine.Object.Destroy(e.gameObject);
            var point=UnityEngine.Object.FindObjectsOfType<OrbitBreakerAttachmentPoint>().OrderBy(p=>p.transform.position.sqrMagnitude).First();
            body.position=(Vector3)point.Center+Vector3.down*1.08f;body.velocity=Vector3.zero;Physics.SyncTransforms();
            Check(motor.TryAttach(point),"Attach for edited duration");yield return Delay(1f);Check(motor.IsAttached,"Edited 1.2-second attachment persists at one second");
            yield return Delay(.3f);Check(!motor.IsAttached&&motor.DashCharges==0,"Edited attachment expires without automatic dash or free capsule");
            body.position=new Vector3(1,1);body.velocity=Vector3.zero;Physics.SyncTransforms();
            Check(motor.TryReceiveHit(),"Create first missing shield for single-pickup check");
            float recover=Time.time+1.1f;yield return ()=>{body.position=new Vector3(1,1);body.velocity=Vector3.zero;return Time.time>=recover;};
            Check(motor.TryReceiveHit()&&motor.Shells==1,"Create second missing shield");
            OrbitBreakerShieldPickup.Spawn(body.position,motor);yield return Delay(.08f);
            Check(motor.Shells==2&&UnityEngine.Object.FindObjectsOfType<OrbitBreakerShieldPickup>().Length==0,"One pickup restores exactly one layer from one to two, even across trigger callbacks");
            recover=Time.time+1.1f;yield return ()=>{body.position=new Vector3(1,1);body.velocity=Vector3.zero;return Time.time>=recover;};
            var source=UnityEngine.Object.FindObjectOfType<OrbitBreakerCombatAudio>().GetComponent<AudioSource>();var data=new float[1024];
            float rms=0;bool playing=false;float audioUntil=Time.realtimeSinceStartup+1.2f;OrbitBreakerCombatAudio.Play(CombatSound.Reward);
            yield return ()=>
            {
                playing|=source.isPlaying;source.GetOutputData(data,0);rms=Mathf.Max(rms,Mathf.Sqrt(data.Sum(v=>v*v)/data.Length));
                AudioListener.GetOutputData(data,0);rms=Mathf.Max(rms,Mathf.Sqrt(data.Sum(v=>v*v)/data.Length));
                return rms>.0001f || Time.realtimeSinceStartup>audioUntil;
            };
            Check(playing && rms>.0001f,"AudioSource/listener outputs actual nonzero audio samples, peak RMS="+rms+", playing="+playing);
            var jackpot=UnityEngine.Object.FindObjectsOfType<OrbitBreakerJackpotHole>().OrderBy(h=>h.transform.position.sqrMagnitude).First();
            Check(jackpot.transform.Find("RewardStar")!=null,"Reward well has a gold star distinct from lethal black-hole core");
            body.position=jackpot.transform.position+Vector3.down*1.4f;body.velocity=Vector3.up*10;Physics.SyncTransforms();yield return Delay(.18f);
            Check(score.Score>=600&&manager.State==GameState.Victory&&body.isKinematic,"Edited 500-point target wins on real 600-point reward entry");
            Check(!motor.HasRewardBoost&&!motor.IsSlowed,"Victory clears temporary movement states");
            int id=manager.GetInstanceID();manager.RestartGame();yield return ()=>UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>().GetInstanceID()!=id;
            var next=UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>();var p=UnityEngine.Object.FindObjectOfType<OrbitBreakerPlayerMotor>();
            Check(next.State==GameState.Ready&&next.Config.targetScore==3000&&next.Config.attachDuration==7.5f&&p.Shells==3,"Victory reload restores saved production config, Ready and shells");
        }
        private static Func<bool> Delay(float duration) { float end = Time.time + duration; return () => Time.time >= end; }
        private static void Tick()
        {
            try
            {
                if (waiting != null && !waiting()) return;
                if (steps.MoveNext()) waiting = steps.Current;
                else { SessionState.SetBool(Prefix + "Complete", true); Report("PASS", "All T17 integration checks passed."); EditorApplication.isPlaying = false; }
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
        private static void Report(string status, string detail) => File.WriteAllText(Evidence("T17-config-audio.json"), JsonUtility.ToJson(new Result {status=status, observedAtUtc=DateTime.UtcNow.ToString("O"), unityVersion=Application.unityVersion, detail=detail, checks=checks.ToArray()}, true));
    }
}
