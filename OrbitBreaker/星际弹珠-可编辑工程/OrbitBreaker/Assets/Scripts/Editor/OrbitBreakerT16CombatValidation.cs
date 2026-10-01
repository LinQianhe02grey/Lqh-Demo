using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OrbitBreaker.Editor
{
    [InitializeOnLoad]
    public static class OrbitBreakerT16CombatValidation
    {
        private const string Prefix = "OrbitBreaker.T16Combat.";
        private static IEnumerator<Func<bool>> steps;
        private static Func<bool> waiting;
        private static OrbitBreakerPlayerMotor motor;
        private static OrbitBreakerGameManager manager;
        private static OrbitBreakerScoreManager score;
        private static float originalMaximumDeltaTime;
        private static readonly List<string> checks = new List<string>();
        static OrbitBreakerT16CombatValidation() { EditorApplication.playModeStateChanged += OnPlayState; EditorApplication.update += Timeout; }
        [MenuItem("Tools/Orbit Breaker/Validate T16 Combat")]
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
        private static Rigidbody body;
        private static OrbitBreakerProceduralArena arena;
        private static void Place(Vector3 p,Vector3 v){body.position=p;body.velocity=v;Physics.SyncTransforms();arena.RefreshAroundPlayer();}
        private static IEnumerator<Func<bool>> RunChecks()
        {
            motor=UnityEngine.Object.FindObjectOfType<OrbitBreakerPlayerMotor>();manager=motor.OrbitBreakerGameManager;score=UnityEngine.Object.FindObjectOfType<OrbitBreakerScoreManager>();
            body=motor.GetComponent<Rigidbody>();arena=UnityEngine.Object.FindObjectOfType<OrbitBreakerProceduralArena>();
            motor.GetComponent<OrbitBreakerPlayerInput>().enabled=false;UnityEngine.Object.Destroy(motor.GetComponent<OrbitBreakerPlayerInput>());
            manager.StartGame();Place(new Vector3(180,180),Vector3.zero);
            var spawner=UnityEngine.Object.FindObjectOfType<OrbitBreakerEnemySpawner>();var enemies=new List<OrbitBreakerEnemyController>();
            for(int i=0;i<6;i++){var e=spawner.TrySpawn();Check(e!=null && Vector3.Distance(e.Position,motor.Position)>=10.99f && Vector3.Distance(e.Position,motor.Position)<=15.01f,"Roaming spawn relative to remote player "+i);enemies.Add(e);}
            Check(spawner.TrySpawn()==null && spawner.AliveCount==6,"Roaming spawn cap remains six");
            Check(Mathf.Abs(enemies[0].transform.localScale.x-.64f)<.001f,"OrbitBreakerEnemy prefab previous scale .8 receives another 20 percent shrink to .64");
            var collider=enemies[0].GetComponent<Collider>();int pairs=0;
            foreach(var c in UnityEngine.Object.FindObjectsOfType<Collider>())if(c!=collider&&!c.isTrigger&&c.GetComponentInParent<OrbitBreakerPlayerMotor>()==null){Check(Physics.GetIgnoreCollision(collider,c),"");pairs++;}
            Check(pairs>150 && !Physics.GetIgnoreCollision(collider,motor.GetComponent<Collider>()),"Ghost enemies ignore procedural terrain and retain player contact, pairs="+pairs);
            var before=enemies[0].Position;yield return Delay(.18f);
            Check(enemies[0]!=null && Vector3.Distance(enemies[0].Position,motor.Position)<Vector3.Distance(before,motor.Position),"OrbitBreakerEnemy chases at x/y 180 instead of old board despawn");
            Place(new Vector3(360,360),Vector3.zero);yield return Delay(.04f);
            Check(spawner.AliveCount==0,"Distant enemies retire relative to player after crossing sectors");spawner.enabled=false;
            Place(new Vector3(1,1),Vector3.zero);yield return Delay(.1f);
            var target=UnityEngine.Object.FindObjectsOfType<OrbitBreakerScoreTarget>().OrderBy(t=>t.transform.position.sqrMagnitude).First();
            int previous=score.Score;Place(target.transform.position+Vector3.down*1.5f,Vector3.up*12);yield return Delay(.15f);
            Check(score.Score>previous && target.InteractionId>0,"Cloned score target retains live OrbitBreakerScoreManager reference and scores real upward impact");
            Place(new Vector3(1,1),Vector3.zero);yield return Delay(1.4f);
            int captures=0;
            while(manager.IsPlaying && captures<12)
            {
                var go=UnityEngine.Object.Instantiate(arena.templates[6],new Vector3(1,1,0),Quaternion.identity);
                go.transform.localScale=arena.templates[6].transform.localScale*.8f;go.SetActive(true);
                previous=score.Score;Place(new Vector3(1,-.4f),Vector3.up*10);float deadline=Time.time+.3f;
                yield return ()=>score.Score>previous||Time.time>=deadline;
                Check(score.Score>previous,"Real generated jackpot capture scores "+captures);captures++;
                if(manager.IsPlaying){Check(motor.IsInJackpot,"Jackpot captures player before timed ejection");yield return Delay(.76f);Check(!motor.IsInJackpot&&motor.IsLaunched,"Jackpot exits via protected motor impulse");}
                UnityEngine.Object.Destroy(go);yield return Delay(.04f);
            }
            Check(manager.State==GameState.Victory && score.Score>=3000 && body.isKinematic,"Real target/jackpot interactions still reach Victory and freeze movement");
            int id=manager.GetInstanceID(),seed=arena.seed;manager.RestartGame();yield return ()=>UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>().GetInstanceID()!=id;
            var p=UnityEngine.Object.FindObjectOfType<OrbitBreakerPlayerMotor>();
            Check(p.Shells==3 && p.OrbitBreakerGameManager.State==GameState.Ready && UnityEngine.Object.FindObjectOfType<OrbitBreakerScoreManager>().Score==0,"Victory restart restores Ready, score and shells");
            Check(UnityEngine.Object.FindObjectOfType<OrbitBreakerProceduralArena>().seed!=seed,"Restart rolls a fresh procedural seed");
        }
        private static Func<bool> Delay(float duration) { float end = Time.time + duration; return () => Time.time >= end; }
        private static void Tick()
        {
            try
            {
                if (waiting != null && !waiting()) return;
                if (steps.MoveNext()) waiting = steps.Current;
                else { SessionState.SetBool(Prefix + "Complete", true); Report("PASS", "All T16 integration checks passed."); EditorApplication.isPlaying = false; }
            }
            catch (Exception ex) { Fail(ex.ToString()); }
        }
        private static void Check(bool pass, string detail) { if (!pass) throw new InvalidOperationException(detail.Length > 0 ? detail : "Attachment expired before 7.35 seconds."); if (detail.Length > 0) checks.Add(detail); }
        private static void Fail(string detail) { Report("FAIL", detail); SessionState.SetBool(Prefix+"Running", false); Debug.LogError("T16: " + detail); EditorApplication.isPlaying = false; }
        private static void Timeout() { if (SessionState.GetBool(Prefix+"Running", false) && DateTime.UtcNow > DateTime.Parse(SessionState.GetString(Prefix+"Deadline", ""), null, System.Globalization.DateTimeStyles.RoundtripKind)) Fail("T16 deadline exceeded."); }
        private static void OnPlayState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode && originalMaximumDeltaTime > 0f) { Time.maximumDeltaTime = originalMaximumDeltaTime; originalMaximumDeltaTime = 0f; }
            if (!SessionState.GetBool(Prefix+"Running", false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                checks.Clear(); waiting = null; steps = RunChecks();
                originalMaximumDeltaTime = Time.maximumDeltaTime; Time.maximumDeltaTime = Time.fixedDeltaTime;
                var probe = new GameObject("T16_ValidationProbe") { hideFlags=HideFlags.DontSave };
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
        private static void Report(string status, string detail) => File.WriteAllText(Evidence("T16-combat.json"), JsonUtility.ToJson(new Result {status=status, observedAtUtc=DateTime.UtcNow.ToString("O"), unityVersion=Application.unityVersion, detail=detail, checks=checks.ToArray()}, true));
    }
}
