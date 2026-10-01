using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OrbitBreaker.Editor
{
    [InitializeOnLoad]
    public static class OrbitBreakerT15Validation
    {
        private const string Prefix = "OrbitBreaker.T15.";
        private static IEnumerator<Func<bool>> steps;
        private static Func<bool> waiting;
        private static OrbitBreakerPlayerMotor motor;
        private static OrbitBreakerGameManager manager;
        private static OrbitBreakerScoreManager score;
        private static float originalMaximumDeltaTime;
        private static readonly List<string> checks = new List<string>();
        static OrbitBreakerT15Validation() { EditorApplication.playModeStateChanged += OnPlayState; EditorApplication.update += Timeout; }
        [MenuItem("Tools/Orbit Breaker/Validate T15")]
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
            motor.GetComponent<OrbitBreakerPlayerInput>().enabled = false;
            // Disabled MonoBehaviours still receive application-focus callbacks. Remove only this
            // runtime fixture input component so tool/user focus changes cannot cancel scripted requests.
            UnityEngine.Object.Destroy(motor.GetComponent<OrbitBreakerPlayerInput>());
            Check(manager.State == GameState.Ready && score.Score == 0 && manager.Config.attachDuration == 7.5f,
                "Real reload starts Ready, score zero, production attachment 7.5 seconds");
        }
        private static Rigidbody body;
        private static void Place(Vector3 pos, Vector3 velocity)
        { body.position=pos; body.velocity=velocity; motor.SetMoveInput(Vector2.zero); Physics.SyncTransforms(); }
        private static Func<bool> SafeDelay(float duration)
        { float until=Time.time+duration;return ()=>{ Place(new Vector3(0,-1,0),Vector3.zero);return Time.time>=until;}; }
        private static OrbitBreakerEnemyController Spawn(Vector3 pos)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/OrbitBreakerEnemy.prefab");
            var enemy=UnityEngine.Object.Instantiate(prefab,pos,Quaternion.identity).GetComponent<OrbitBreakerEnemyController>();
            enemy.Initialize(motor,score);return enemy;
        }
        private static IEnumerator<Func<bool>> RunChecks()
        {
            Bind();body=motor.GetComponent<Rigidbody>();var spawner=UnityEngine.Object.FindObjectOfType<OrbitBreakerEnemySpawner>();spawner.enabled=false;
            Check(UnityEngine.Object.FindObjectsOfType<OrbitBreakerBumper>().Length==22 && UnityEngine.Object.FindObjectsOfType<OrbitBreakerFloatingBoard>().Length==5,"22 slim bumpers and five floating rebound boards");
            Check(manager.Config.gravityStrength==7.5f && manager.Config.dashCooldown==1,"Slower gravity and one-second dash cooldown saved");
            manager.StartGame();Check(motor.DashCharges==0 && body.velocity.y>16,"Opening serve starts upward with zero free dash charges");
            motor.SetAttachmentAim(new Vector2(5,0));Check(!motor.TryRequestDash(),"Zero charge rejects dash");
            var pin=GameObject.Find("RelayPin04");
            var impactNames=new List<string>();motor.Impact+=(c,d)=>impactNames.Add(c.collider.name+":"+c.relativeVelocity.magnitude.ToString("F2"));
            for(int i=0;i<4;i++)
            {
                Place(pin.transform.position+Vector3.down*1.4f,Vector3.up*9);int before=motor.TotalQualifiedContacts;
                yield return Delay(.12f);
                Check(motor.TotalQualifiedContacts==before+1,"Real bumper contact counted once "+i+", before="+before+", now="+motor.TotalQualifiedContacts+", hits="+string.Join(";",impactNames));
                Check(motor.DashCharges==(i==3?1:0)&&motor.DashContactProgress==(i+1)%4,"Four-contact charge threshold "+i);
                yield return SafeDelay(.35f);
            }
            Place(new Vector3(0,-1,0),Vector3.zero);motor.SetAttachmentAim(new Vector2(5,-1),false);
            Check(!motor.TryRequestDash()&&motor.DashCharges==1,"Invalid mouse projection cannot consume charge");
            motor.SetAttachmentAim(new Vector2(5,-1));Check(motor.TryRequestDash(),"Earned dash queued");motor.CancelPendingDash();
            Check(motor.DashCharges==1,"Cancelled queued dash keeps charge");
            motor.SetMoveInput(Vector2.left);Check(motor.TryRequestDash(),"Mouse dash accepted against opposite WASD");yield return Delay(.04f);
            Check(motor.DashCharges==0&&body.velocity.x>17&&motor.DashDirection.x>.99f,"Mouse-right dash ignores left movement and consumes exactly one");
            Check(motor.IsInvulnerable&&!motor.TryReceiveHit(),"Dash still shields player");
            var enemy=Spawn(new Vector3(0,1.2f,0));
            for(int i=0;i<3;i++)
            {
                if(i>0)yield return SafeDelay(.45f);
                if(enemy==null||enemy.IsDead)break;
                enemy.GetComponent<Rigidbody>().position=new Vector3(0,1.2f,0);Place(new Vector3(0,-.3f,0),Vector3.up*14);
                motor.TryRequestBoardImpulse(Vector2.up,17);int hp=enemy.Health;float end=Time.time+.25f;
                yield return ()=>enemy==null||enemy.Health<hp||Time.time>=end;
                if(enemy==null||enemy.IsDead)break;
            }
            Check(enemy==null||enemy.IsDead,"Actual launch attacks kill HP enemy");
            Check(motor.DashCharges>=1,"OrbitBreakerEnemy kill grants a dash charge");Check(score.Score>=80,"Real kill pays scaled score");
            yield return SafeDelay(1.2f);
            for(int i=0;i<12;i++)
            {
                Place(pin.transform.position+Vector3.down*1.4f,Vector3.up*9);yield return Delay(.12f);yield return SafeDelay(.25f);
            }
            Check(motor.DashCharges==3,"Dash storage caps at three after repeated real collisions");
            Place(new Vector3(0,-1,0),Vector3.zero);motor.SetAttachmentAim(new Vector2(5,-1));Check(motor.TryRequestDash(),"Stored charge can fire");yield return Delay(.04f);
            Check(motor.DashCharges==2&&!motor.TryRequestDash()&&motor.DashCooldownRemaining>.9f,"One-second cooldown rejects repeat even with reserve charges");
            yield return SafeDelay(1.1f);Check(motor.CanDash,"Cooldown expires without automatically restoring a consumed charge");
            int contacts=motor.TotalQualifiedContacts;
            var platform=GameObject.Find("FloatingBoard2").GetComponent<OrbitBreakerFloatingBoard>();Vector3 origin=platform.transform.position;
            yield return SafeDelay(.6f);Check(Vector3.Distance(origin,platform.transform.position)>.05f,"Floating board physically moves");
            bool boardContact=false,boardApplied=false,boardLaunchedUp=false;
            motor.Impact+=(c,d)=>{if(c.collider.attachedRigidbody==platform.GetComponent<Rigidbody>())boardContact=true;};
            motor.BoardImpulseApplied+=()=>{if(boardContact){boardApplied=true;boardContact=false;}};
            Place(platform.transform.position+platform.transform.up*1.2f,-platform.transform.up*10);float boardUntil=Time.time+.18f;
            yield return ()=>{boardLaunchedUp|=boardApplied&&body.velocity.y>5;return Time.time>=boardUntil;};
            Check(motor.TotalQualifiedContacts>contacts&&boardLaunchedUp,"Floating board physically launches upward before any subsequent pin reflection");
            yield return SafeDelay(1.2f);
            var point=UnityEngine.Object.FindObjectsOfType<OrbitBreakerAttachmentPoint>().OrderBy(p=>p.Center.x).First();
            Place((Vector3)point.Center+Vector3.down*OrbitBreakerAttachmentPoint.OrbitRadius,Vector3.zero);motor.SetAttachmentAim(point.Center+Vector2.right*4);
            Check(motor.TryAttach(point),"Dense layout keeps an unobstructed attachment orbit");
            enemy=Spawn((Vector3)point.Center+new Vector3(.9f,-1.15f,0));float spinStart=Time.time;bool repelled=false;
            yield return ()=>{float age=Time.time-spinStart;float angle=(-90+age*270)*Mathf.Deg2Rad;motor.SetAttachmentAim(point.Center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*4);repelled|=enemy.IsRepelled;return age>.45f;};
            Check(motor.IsAttached&&repelled&&enemy.Health<100,"Orbit damage, repel and enemy immunity preserved");UnityEngine.Object.Destroy(enemy.gameObject);
            spinStart=Time.time;
            yield return ()=>{float age=Time.time-spinStart;float angle=(30+age*270)*Mathf.Deg2Rad;motor.SetAttachmentAim(point.Center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*4);return age>.8f;};
            Check(motor.AttachmentCharge>.95f&&motor.GetComponent<OrbitBreakerPlayerFeedback>().OrbitVfxActive,"Physical orbit charge and visual effects survive dense layout");
            int stored=motor.DashCharges;motor.SetAttachmentAim(point.Center+Vector2.right*5);Check(motor.TryRequestDash(),"Attached launch requires and accepts earned charge");yield return Delay(.04f);
            Check(!motor.IsAttached&&motor.DashCharges==stored-1&&body.velocity.magnitude>22&&motor.IsInvulnerable,"Charged orbit Space spends one and launches with protection");
            yield return SafeDelay(1.2f);
            var kz=UnityEngine.Object.FindObjectOfType<OrbitBreakerKillZone>();var core=GameObject.Find("BlackHoleCore").transform;
            Check(kz.ContainsCore(core.position)&&!kz.ContainsCore((Vector2)core.position+Vector2.right*1.41f),"Kill ellipse exactly references visible core transform");
            Place(core.position+Vector3.right*1.6f,Vector3.zero);yield return Delay(.08f);
            Check(manager.IsPlaying,"Visible glow outside core does not kill even when broad trigger overlaps ball");
            Place(core.position+Vector3.up*2.5f,Vector3.down*8);motor.TryRequestBoardImpulse(Vector2.down,18);yield return Delay(.04f);
            Check(motor.IsInvulnerable,"Core approach retains launch immunity");yield return ()=>!manager.IsPlaying;
            Check(manager.State==GameState.Defeat,"Real core entry kills protected player");
            int id=manager.GetInstanceID();manager.RestartGame();yield return ()=>UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>().GetInstanceID()!=id;
            Bind();body=motor.GetComponent<Rigidbody>();Check(motor.DashCharges==0&&motor.DashContactProgress==0,"Reload clears charges and partial progress");
            // Production scene: no fixture positioning, spawning, score or resource injection from here.
            UnityEngine.Random.InitState(1501);manager.StartGame();float began=Time.time;int peak=0,maxContacts=0,maxCharges=0;float ended=0;
            yield return ()=>
            {
                peak=Mathf.Max(peak,UnityEngine.Object.FindObjectOfType<OrbitBreakerEnemySpawner>().AliveCount);
                maxContacts=Mathf.Max(maxContacts,motor.TotalQualifiedContacts);maxCharges=Mathf.Max(maxCharges,motor.DashCharges);
                if(!manager.IsPlaying||Time.time-began>=45){ended=Time.time-began;return true;}
                var pos=(Vector2)motor.Position;var vel=(Vector2)body.velocity;
                motor.SetMoveInput(new Vector2(Mathf.Clamp(-pos.x*.2f-vel.x*.08f,-.5f,.5f),.35f));
                motor.SetAttachmentAim(new Vector2(-pos.x*.2f,7.5f));
                if(pos.y<3.5f)motor.TryRequestDash();
                if(pos.y< -6)foreach(var f in UnityEngine.Object.FindObjectsOfType<OrbitBreakerFlipperController>())f.TryActivate();
                return false;
            };
            Check(ended>=15&&maxContacts>=8&&peak>0,"Production input-only rally: seconds="+ended.ToString("F2")+", contacts="+maxContacts+", peakEnemies="+peak+", charges="+maxCharges+", score="+score.Score+", state="+manager.State);
        }
        private static Func<bool> Delay(float duration) { float end = Time.time + duration; return () => Time.time >= end; }
        private static void Tick()
        {
            try
            {
                if (waiting != null && !waiting()) return;
                if (steps.MoveNext()) waiting = steps.Current;
                else { SessionState.SetBool(Prefix + "Complete", true); Report("PASS", "All T15 integration checks passed."); EditorApplication.isPlaying = false; }
            }
            catch (Exception ex) { Fail(ex.ToString()); }
        }
        private static void Check(bool pass, string detail) { if (!pass) throw new InvalidOperationException(detail.Length > 0 ? detail : "Attachment expired before 7.35 seconds."); if (detail.Length > 0) checks.Add(detail); }
        private static void Fail(string detail) { Report("FAIL", detail); SessionState.SetBool(Prefix+"Running", false); Debug.LogError("T15: " + detail); EditorApplication.isPlaying = false; }
        private static void Timeout() { if (SessionState.GetBool(Prefix+"Running", false) && DateTime.UtcNow > DateTime.Parse(SessionState.GetString(Prefix+"Deadline", ""), null, System.Globalization.DateTimeStyles.RoundtripKind)) Fail("T15 deadline exceeded."); }
        private static void OnPlayState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode && originalMaximumDeltaTime > 0f) { Time.maximumDeltaTime = originalMaximumDeltaTime; originalMaximumDeltaTime = 0f; }
            if (!SessionState.GetBool(Prefix+"Running", false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                checks.Clear(); waiting = null; steps = RunChecks();
                originalMaximumDeltaTime = Time.maximumDeltaTime; Time.maximumDeltaTime = Time.fixedDeltaTime;
                var probe = new GameObject("T15_ValidationProbe") { hideFlags=HideFlags.DontSave };
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
        private static void Report(string status, string detail) => File.WriteAllText(Evidence("T15-integration.json"), JsonUtility.ToJson(new Result {status=status, observedAtUtc=DateTime.UtcNow.ToString("O"), unityVersion=Application.unityVersion, detail=detail, checks=checks.ToArray()}, true));
    }
}
