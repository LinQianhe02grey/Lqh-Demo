using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OrbitBreaker.Editor
{
    [InitializeOnLoad]
    public static class OrbitBreakerT19Validation
    {
        private const string Prefix = "OrbitBreaker.T19.";
        private static IEnumerator<Func<bool>> steps;
        private static Func<bool> waiting;
        private static OrbitBreakerPlayerMotor motor;
        private static OrbitBreakerGameManager manager;
        private static OrbitBreakerScoreManager score;
        private static float originalMaximumDeltaTime;
        private static readonly List<string> checks = new List<string>();
        static OrbitBreakerT19Validation() { EditorApplication.playModeStateChanged += OnPlayState; EditorApplication.update += Timeout; }
        [MenuItem("Tools/Orbit Breaker/Validate T19")]
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
        private static void Bind()
        {
            motor=UnityEngine.Object.FindObjectOfType<OrbitBreakerPlayerMotor>();manager=motor.OrbitBreakerGameManager;score=UnityEngine.Object.FindObjectOfType<OrbitBreakerScoreManager>();body=motor.GetComponent<Rigidbody>();
            var input=motor.GetComponent<OrbitBreakerPlayerInput>();input.enabled=false;UnityEngine.Object.Destroy(input);
            arena=UnityEngine.Object.FindObjectOfType<OrbitBreakerProceduralArena>();UnityEngine.Object.FindObjectOfType<OrbitBreakerEnemySpawner>().enabled=false;
        }
        private static void Place(Vector3 p,Vector3 v){body.position=p;body.velocity=v;motor.SetMoveInput(Vector2.zero);Physics.SyncTransforms();}
        private static Func<bool> Hold(float seconds,Vector3 p)
        {float end=Time.time+seconds;return ()=>{Place(p,Vector3.zero);return Time.time>=end;};}
        private static OrbitBreakerEnemyController Spawn(Vector3 p)
        {
            var e=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/OrbitBreakerEnemy.prefab"),p,Quaternion.identity).GetComponent<OrbitBreakerEnemyController>();
            e.transform.localScale*=.8f;e.Initialize(motor,score);return e;
        }
        private static void Charges(int n)=>typeof(OrbitBreakerPlayerMotor).GetField("<DashCharges>k__BackingField",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(motor,n);
        private static IEnumerator<Func<bool>> RunChecks()
        {
            Bind();var menu=manager.GetComponent<OrbitBreakerModeMenu>();
            Check(manager.StartMode(GameMode.Free),"Free starts normally");arena.gameObject.SetActive(false);
            var art=new OrbitBreakerCatalogIllustrations();
            int icons=0;for(int tab=0;tab<3;tab++)for(int i=0;i<menu.CatalogEntries(tab).Length;i++)
            {var tex=art.Get(tab,i);Check(tex!=null&&tex.width==160&&tex.GetPixels().Distinct().Count()>1,"Illustration "+tab+"/"+i+" contains visible geometry");icons++;}
            art.Dispose();Check(icons==19,"All 19 catalog entries illustrated");
            var point=UnityEngine.Object.Instantiate(arena.templates[4],new Vector3(1,3),Quaternion.identity).GetComponent<OrbitBreakerAttachmentPoint>();
            point.transform.localScale*=.8f;point.gameObject.SetActive(true);
            Place(new Vector3(1+OrbitBreakerAttachmentPoint.OrbitRadius,3),Vector3.zero);Charges(3);
            Check(motor.TryAttach(point),"Attach begins");var camera=Camera.main.GetComponent<OrbitBreakerCenteredCamera>();camera.Follow();var camPos=camera.transform.position;var camRot=camera.transform.rotation;
            float drift=0,angle=0,end=Time.time+2.4f;yield return ()=>{camera.Follow();drift=Mathf.Max(drift,Vector3.Distance(camera.transform.position,camPos));angle=Mathf.Max(angle,Quaternion.Angle(camera.transform.rotation,camRot));return Time.time>=end;};
            Check(motor.IsAttached&&motor.CurrentSpeed>23.8f&&drift<.001f&&angle<.001f,"Actual full-speed orbit keeps camera position/rotation fixed; drift="+drift);
            motor.SetAttachmentAim(motor.Position+Vector3.up*15);Check(motor.TryRequestDash(),"Orbit Space accepted");
            Check(point.Spent&&!point.gameObject.activeSelf,"Orbit pillar disappears only after release");
            yield return Delay(.2f);Check(motor.IsDashing&&Mathf.Abs(motor.CurrentSpeed-24)<.02f&&body.velocity.y>23.9f,"Upward orbit launch preserves24 through initial .2s");
            camera.Follow();Check(Vector3.Distance(camera.transform.position,motor.transform.position+camera.offset)<.001f,"Free flight camera resumes player tracking");
            yield return Delay(.18f);Check(motor.CurrentSpeed<24,"Gravity resumes after protected dash startup");
            yield return Hold(1.1f,new Vector3(0,0));Charges(3);motor.SetAttachmentAim(Vector2.right*20);
            var enemy=Spawn(new Vector3(2.2f,0));var nearby=Spawn(new Vector3(2.8f,1.7f));var far=Spawn(new Vector3(8,4));
            foreach(var e in new[]{enemy,nearby,far})e.GetComponent<Rigidbody>().isKinematic=true;
            int bursts=motor.DashBurstCount;Check(motor.TryRequestDash(),"Ordinary Space accepted");yield return Delay(.14f);
            Check(enemy.Health<60&&nearby.Health<100&&far.Health==100,"Real18-speed impact applies direct plus local blast; hp="+enemy.Health+"/"+nearby.Health+"/"+far.Health);
            Check(motor.DashBurstCount==bursts+1&&UnityEngine.Object.FindObjectOfType<OrbitBreakerDashBurst>()!=null,"Impact creates one visible burst");
            Check(!motor.TryDashBurst(enemy.Position,100),"Same dash cannot repeat blast");
            foreach(var e in new[]{enemy,nearby,far})UnityEngine.Object.Destroy(e.gameObject);
            yield return Hold(1.1f,Vector3.zero);Charges(3);Place(Vector3.zero,Vector3.right*24);motor.SetAttachmentAim(Vector2.right*20);
            enemy=Spawn(new Vector3(2.2f,0));nearby=Spawn(new Vector3(4.5f,0));enemy.GetComponent<Rigidbody>().isKinematic=true;nearby.GetComponent<Rigidbody>().isKinematic=true;
            bursts=motor.DashBurstCount;Check(motor.TryRequestDash(),"Maximum speed Space accepted");yield return Delay(.26f);
            Check(body.position.x>5 && (enemy==null||enemy.Health<100) && (nearby==null||nearby.Health<100),"Actual maximum-speed dash pierces two enemies");
            Check(motor.DashBurstCount==bursts+1,"Swept piercing impact triggers exactly one blast");
            foreach(var e in UnityEngine.Object.FindObjectsOfType<OrbitBreakerEnemyController>())UnityEngine.Object.Destroy(e.gameObject);
            yield return Hold(1.1f,Vector3.zero);Charges(3);Place(Vector3.zero,Vector3.right*21);
            float step=Time.fixedDeltaTime,realStart=Time.unscaledTime;
            Check(motor.TryBeginFocus()&&Time.timeScale==.2f&&Mathf.Abs(Time.fixedDeltaTime-step*.2f)<.00001f,"Focus slows global game and scales physics step");
            Check(!motor.TryBeginFocus(),"Holding/repeating focus cannot extend active window");
            yield return ()=>Time.unscaledTime>=realStart+.5f;
            float speed=motor.CurrentSpeed;motor.SetAttachmentAim(motor.Position+Vector3.up*20);Check(motor.TryRequestDash(),"Space can redirect during focus");yield return ()=>motor.IsDashing;
            Check(!motor.IsFocusActive&&Time.timeScale==1&&Mathf.Abs(Time.fixedDeltaTime-step)<.00001f,"Actual dash restores clock and physics step");
            Check(Mathf.Abs(motor.CurrentSpeed-speed)<.02f&&body.velocity.y>0&&Mathf.Abs(body.velocity.x)<.01f,"Focus dash changes direction while preserving request speed="+speed);
            yield return Hold(1.1f,Vector3.zero);Charges(0);Check(motor.TryBeginFocus(),"Focus allowed without capsule for observation");
            Check(!motor.TryRequestDash()&&motor.IsFocusActive,"Failed no-capsule dash does not cancel focus");
            realStart=Time.unscaledTime;yield return ()=>!motor.IsFocusActive;
            Check(Time.unscaledTime-realStart>=1.95f&&Time.unscaledTime-realStart<2.25f&&Time.timeScale==1,"Focus automatically expires after two REAL seconds");
            // Successful hole use is one-shot; capture/ejection survives the object's disappearance.
            yield return Hold(.2f,new Vector3(-5,0));
            var hole=UnityEngine.Object.Instantiate(arena.templates[8],Vector3.zero,Quaternion.identity).GetComponent<OrbitBreakerDriftingBlackHole>();hole.transform.localScale*=.8f;hole.gameObject.SetActive(true);
            int shells=motor.Shells;Place(Vector3.zero,Vector3.zero);yield return Delay(.06f);
            Check(hole.Spent&&!hole.gameObject.activeSelf&&motor.Shells==shells-1,"Real black-hole core entry consumes it and removes exactly one shell");motor.TryRestoreShell();
            yield return Hold(.2f,new Vector3(-5,0));
            var well=UnityEngine.Object.Instantiate(arena.templates[6],Vector3.zero,Quaternion.identity).GetComponent<OrbitBreakerJackpotHole>();well.transform.localScale*=.8f;well.gameObject.SetActive(true);
            int points=score.Score;Place(Vector3.zero,Vector3.zero);yield return Delay(.06f);
            Check(well.Spent&&!well.gameObject.activeSelf&&motor.IsInJackpot&&motor.HasRewardBoost&&score.Score>points,"Reward capture scores, boosts and consumes well exactly once");
            yield return Delay(.7f);Check(!motor.IsInJackpot&&motor.CurrentSpeed>15,"Reward ejection still completes after well disappeared");
            // Re-enter a consumed generated chunk: its key must stay absent.
            arena.gameObject.SetActive(true);arena.RefreshAroundPlayer();var generated=UnityEngine.Object.FindObjectsOfType<OrbitBreakerAttachmentPoint>().First(a=>a.GetComponentInParent<OrbitBreakerProceduralArena>()!=null);
            string chunk=generated.transform.parent.name,prop=generated.name;Vector3 oldPosition=motor.Position;
            generated.Consume();Check(!generated.gameObject.activeSelf,"Generated consumed prop disabled");
            Place(oldPosition+Vector3.right*180,Vector3.zero);arena.RefreshAroundPlayer();yield return Delay(.04f);
            Place(oldPosition,Vector3.zero);arena.RefreshAroundPlayer();yield return Delay(.04f);
            var returned=arena.transform.Find(chunk).Find(prop);Check(returned!=null&&!returned.gameObject.activeSelf,"Consumed pillar remains absent after chunk unload and regeneration");
            Check(arena.templates.All(t=>t!=null)&&!GameObject.Find("OrbitBreaker").transform.Find("Arena").gameObject.activeSelf,"All source templates preserved and inactive");arena.gameObject.SetActive(false);
            // Return and end paths cannot leak global slow motion.
            yield return Hold(1.1f,Vector3.zero);Check(motor.TryBeginFocus(),"Focus before menu");
            int old=manager.GetInstanceID();manager.ReturnToMenu();yield return ()=>UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>().GetInstanceID()!=old;Bind();
            Check(Time.timeScale==1&&Mathf.Abs(Time.fixedDeltaTime-step)<.00001f&&manager.State==GameState.Ready,"Menu reload restores timing");
            manager.StartMode(GameMode.OrbitBreakerBattle);arena.gameObject.SetActive(false);var boss=manager.GetComponent<OrbitBreakerBossDirector>().Boss;
            boss.enabled=false;boss.GetComponent<Rigidbody>().position=new Vector3(3,0);Place(Vector3.zero,Vector3.right*24);Charges(3);motor.SetAttachmentAim(Vector2.right*20);Check(motor.TryRequestDash(),"Boss test dash starts");
            yield return Delay(.08f);Check(boss.Health<manager.Rules.bossHealth-80&&body.velocity.x<0,"Real Boss dash adds explosion damage and still rebounds");
            yield return Hold(1.1f,new Vector3(0,-10));Check(motor.TryBeginFocus(),"Focus before terminal state");manager.LoseGame();
            Check(!motor.IsFocusActive&&Time.timeScale==1&&Mathf.Abs(Time.fixedDeltaTime-step)<.00001f&&body.isKinematic,"Defeat restores clock and freezes motor");
        }
        private static Func<bool> Delay(float duration) { float end = Time.time + duration; return () => Time.time >= end; }
        private static void Tick()
        {
            try
            {
                if (waiting != null && !waiting()) return;
                if (steps.MoveNext()) waiting = steps.Current;
                else { SessionState.SetBool(Prefix + "Complete", true); Report("PASS", "All T19 integration checks passed."); EditorApplication.isPlaying = false; }
            }
            catch (Exception ex) { Fail(ex.ToString()); }
        }
        private static void Check(bool pass, string detail) { if (!pass) throw new InvalidOperationException(detail.Length > 0 ? detail : "Attachment expired before 7.35 seconds."); if (detail.Length > 0) {checks.Add(detail);Report("RUNNING",detail);} }
        private static void Fail(string detail) { Report("FAIL", detail); SessionState.SetBool(Prefix+"Running", false); Debug.LogError("T19: " + detail); EditorApplication.isPlaying = false; }
        private static void Timeout() { if (SessionState.GetBool(Prefix+"Running", false) && DateTime.UtcNow > DateTime.Parse(SessionState.GetString(Prefix+"Deadline", ""), null, System.Globalization.DateTimeStyles.RoundtripKind)) Fail("T19 deadline exceeded."); }
        private static void OnPlayState(PlayModeStateChange state)
        {
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
        private static void Report(string status, string detail) => File.WriteAllText(Evidence("T19-integration.json"), JsonUtility.ToJson(new Result {status=status, observedAtUtc=DateTime.UtcNow.ToString("O"), unityVersion=Application.unityVersion, detail=detail, checks=checks.ToArray()}, true));
    }
}

