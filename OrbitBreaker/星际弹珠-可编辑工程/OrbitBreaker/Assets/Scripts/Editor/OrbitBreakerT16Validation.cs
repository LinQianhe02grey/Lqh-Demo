using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OrbitBreaker.Editor
{
    [InitializeOnLoad]
    public static class OrbitBreakerT16Validation
    {
        private const string Prefix = "OrbitBreaker.T16.";
        private static IEnumerator<Func<bool>> steps;
        private static Func<bool> waiting;
        private static OrbitBreakerPlayerMotor motor;
        private static OrbitBreakerGameManager manager;
        private static OrbitBreakerScoreManager score;
        private static float originalMaximumDeltaTime;
        private static readonly List<string> checks = new List<string>();
        static OrbitBreakerT16Validation() { EditorApplication.playModeStateChanged += OnPlayState; EditorApplication.update += Timeout; }
        [MenuItem("Tools/Orbit Breaker/Validate T16")]
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
            motor=UnityEngine.Object.FindObjectOfType<OrbitBreakerPlayerMotor>();manager=motor.OrbitBreakerGameManager;
            score=UnityEngine.Object.FindObjectOfType<OrbitBreakerScoreManager>();body=motor.GetComponent<Rigidbody>();
            arena=UnityEngine.Object.FindObjectOfType<OrbitBreakerProceduralArena>();
            motor.GetComponent<OrbitBreakerPlayerInput>().enabled=false;UnityEngine.Object.Destroy(motor.GetComponent<OrbitBreakerPlayerInput>());
        }
        private static void Place(Vector3 point,Vector3 velocity)
        { body.position=point;body.velocity=velocity;motor.SetMoveInput(Vector2.zero);Physics.SyncTransforms();arena.RefreshAroundPlayer(); }
        private static Func<bool> Safe(float seconds)
        { float end=Time.time+seconds;return ()=>{Place(new Vector3(1,1,0),Vector3.zero);return Time.time>=end;}; }
        private static string Pins() => string.Join(";",UnityEngine.Object.FindObjectsOfType<OrbitBreakerBumper>().Select(b=>b.transform.position.ToString("F3")).OrderBy(s=>s));
        private static OrbitBreakerEnemyController Spawn(Vector3 position)
        {
            var e=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/OrbitBreakerEnemy.prefab"),position,Quaternion.identity).GetComponent<OrbitBreakerEnemyController>();
            e.transform.localScale*=.8f;e.Initialize(motor,score);return e;
        }
        private static IEnumerator<Func<bool>> RunChecks()
        {
            Bind();var spawner=UnityEngine.Object.FindObjectOfType<OrbitBreakerEnemySpawner>();spawner.enabled=false;
            Check(manager.State==GameState.Ready && motor.Shells==3 && motor.DashCharges==0,"Ready: three shells, zero dash charges");
            Check(GameObject.Find("OrbitBreaker").transform.Find("Arena").gameObject.activeSelf==false && UnityEngine.Object.FindObjectsOfType<OrbitBreakerKillZone>().Length==0 && UnityEngine.Object.FindObjectsOfType<OrbitBreakerFlipperController>().Length==0,"Legacy boundaries, fixed bottom hole and J/K flippers inactive");
            Check(Vector3.Distance(motor.transform.localScale,Vector3.one*.8f)<.001f,"Player scaled down exactly 20 percent");
            Check(arena.LiveChunkCount==25 && UnityEngine.Object.FindObjectsOfType<OrbitBreakerBumper>().Length>80 && UnityEngine.Object.FindObjectsOfType<OrbitBreakerFloatingBoard>().Length>25,"Random neighbourhood contains 25 bounded sectors and varied rebound props");
            Check(UnityEngine.Object.FindObjectsOfType<OrbitBreakerDriftingBlackHole>().Length<10,"Black holes remain a minority of encounters");
            string pins=Pins();int generated=arena.GeneratedCount;
            manager.StartGame();Place(new Vector3(180,-180),Vector3.zero);yield return Delay(.04f);
            Check(manager.IsPlaying && motor.Shells==3 && arena.LiveChunkCount==25 && arena.GeneratedCount>generated,"Travel beyond old x/y boundaries generates new sectors without death");
            var camera=Camera.main;camera.GetComponent<OrbitBreakerCenteredCamera>().Follow();var vp=camera.WorldToViewportPoint(motor.transform.position);
            Check(Mathf.Abs(vp.x-.5f)<.0001f && Mathf.Abs(vp.y-.5f)<.0001f && !camera.orthographic,"Oblique perspective keeps rendered player exactly at centre after long travel");
            Vector2 aim;Check(OrbitBreakerPlayerMagnet.TryProjectMouse(camera,new Vector3(camera.pixelWidth*.75f,camera.pixelHeight*.7f),out aim) && aim.x>motor.Position.x && aim.y>motor.Position.y,"Mouse projection follows translated camera, still targets upper right");
            Place(new Vector3(1,1,0),Vector3.zero);yield return Delay(.04f);
            Check(Pins()==pins && arena.LiveChunkCount==25,"Returning to same sector regenerates same seeded pin positions and unloads distant sectors");
            var pin=UnityEngine.Object.FindObjectsOfType<OrbitBreakerBumper>().OrderBy(b=>b.transform.position.sqrMagnitude).First();
            Check(Mathf.Abs(pin.GetComponent<CapsuleCollider>().radius*pin.transform.lossyScale.x-.304f)<.001f,"Pin collider and visible body both receive 20 percent shrink");
            for(int i=0;i<4;i++)
            {
                int before=motor.TotalQualifiedContacts;Place(pin.transform.position+Vector3.down*1.15f,Vector3.up*9);yield return Delay(.12f);
                Check(motor.TotalQualifiedContacts==before+1,"Real slim-pin collision credits once "+i);yield return Safe(.3f);
            }
            Check(motor.DashCharges==1 && motor.DashContactProgress==0,"Four real contacts award exactly one dash");
            Place(Vector3.zero,Vector3.zero);motor.SetAttachmentAim(Vector2.right*6);motor.SetMoveInput(Vector2.left);
            Check(motor.TryRequestDash(),"Earned mouse dash accepted");yield return Delay(.04f);
            Check(motor.DashCharges==0 && body.velocity.x>17 && !motor.TryReceiveHit() && motor.Shells==3,"Mouse-directed dash spends once and protects all three shells");
            var enemy=Spawn(new Vector3(0,1.2f,0));
            for(int i=0;i<4;i++)
            {
                yield return Safe(.45f);if(enemy==null||enemy.IsDead)break;
                enemy.GetComponent<Rigidbody>().position=new Vector3(0,1.2f,0);Place(new Vector3(0,-.3f,0),Vector3.up*14);motor.TryRequestBoardImpulse(Vector2.up,17);
                int hp=enemy.Health;float end=Time.time+.24f;yield return ()=>enemy==null||enemy.Health<hp||Time.time>=end;
            }
            Check(enemy==null||enemy.IsDead,"Actual rebound attacks deplete enemy HP");
            Check(motor.DashCharges>=1 && score.Score>=80,"Kill awards dash and scaled points");
            yield return Safe(1.5f);
            var platform=UnityEngine.Object.FindObjectsOfType<OrbitBreakerFloatingBoard>().OrderBy(b=>b.transform.position.sqrMagnitude).First();
            var origin=platform.transform.position;yield return Safe(.5f);
            Check(Vector3.Distance(origin,platform.transform.position)>.05f,"Procedural floating terrain physically moves");
            bool contact=false,applied=false,up=false;motor.Impact+=(c,d)=>{if(c.collider.attachedRigidbody==platform.GetComponent<Rigidbody>())contact=true;};
            motor.BoardImpulseApplied+=()=>{if(contact){applied=true;contact=false;}};
            Place(platform.transform.position+platform.transform.up*1.05f,-platform.transform.up*10);float until=Time.time+.2f;
            yield return ()=>{up|=applied&&body.velocity.y>5;return Time.time>=until;};
            Check(up,"Generated floating board rebounds player via real contact");
            yield return Safe(1.5f);
            var magnet=UnityEngine.Object.FindObjectsOfType<OrbitBreakerAttachmentPoint>().OrderBy(p=>p.transform.position.sqrMagnitude).First();
            Place((Vector3)magnet.Center+Vector3.down*OrbitBreakerAttachmentPoint.OrbitRadius,Vector3.zero);motor.SetAttachmentAim(magnet.Center+Vector2.right*4);
            Check(motor.TryAttach(magnet),"Scaled magnet accepts E-equivalent attach within smaller capture radius");
            enemy=Spawn((Vector3)magnet.Center+new Vector3(.7f,-.85f,0));bool repel=false;float began=Time.time;
            yield return ()=>{float a=(-90+(Time.time-began)*270)*Mathf.Deg2Rad;motor.SetAttachmentAim(magnet.Center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*4);repel|=enemy.IsRepelled;return Time.time-began>.5f;};
            Check(motor.IsAttached && motor.Shells==3 && repel && enemy.Health<100,"Orbit retains shells, repels and damages ghost enemy");UnityEngine.Object.Destroy(enemy.gameObject);
            began=Time.time;yield return ()=>{float a=(45+(Time.time-began)*270)*Mathf.Deg2Rad;motor.SetAttachmentAim(magnet.Center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*4);return Time.time-began>.8f;};
            Check(motor.AttachmentCharge>.9f && motor.GetComponent<OrbitBreakerPlayerFeedback>().OrbitVfxActive,"Scaled orbit charges from real spin with effects");
            motor.SetAttachmentAim(magnet.Center+Vector2.right*5);int charges=motor.DashCharges;
            Check(motor.TryRequestDash(),"Orbit Space launches with earned charge");yield return Delay(.04f);
            Check(!motor.IsAttached && motor.DashCharges==charges-1 && body.velocity.magnitude>22 && motor.Shells==3,"Charged orbit launch remains fast and invulnerable");
            yield return Safe(1.5f);enemy=Spawn(new Vector3(0,.85f,0));Place(Vector3.zero,Vector3.zero);
            until=Time.time+1;yield return ()=>motor.Shells<3||Time.time>=until;
            Check(motor.Shells==2 && motor.IsStunned && !motor.TryReceiveHit(),"Real ordinary enemy contact breaks one shell; repeated hit rejected during recovery");UnityEngine.Object.Destroy(enemy.gameObject);
            yield return Safe(1.3f);
            var hole=UnityEngine.Object.FindObjectsOfType<OrbitBreakerDriftingBlackHole>().FirstOrDefault();
            Check(hole!=null,"Seeded neighbourhood includes a rare black hole for physical validation");
            Place(hole.transform.position+Vector3.right*.95f,Vector3.zero);yield return Delay(.04f);
            Check(motor.Shells==2 && !hole.ContainsCore(motor.Position),"Accretion glow outside visible scaled core does not damage");
            Place(hole.transform.position,Vector3.zero);motor.TryRequestBoardImpulse(Vector2.right,18);yield return Delay(.04f);
            Check(motor.Shells==1 && manager.IsPlaying,"Actual black-hole core breaks exactly one shell even during launch immunity");
            Check(!motor.TryReceiveBlackHole(hole.transform.position),"Global hole cooldown prevents multi-trigger shell drain");
            yield return Delay(.18f);Check(!hole.ContainsCore(motor.Position) && body.velocity.magnitude>10,"Black hole ejects player out of core with a real motor impulse");
            yield return Safe(2.1f);Check(motor.TryReceiveHit() && motor.Shells==0 && manager.State==GameState.Defeat && body.isKinematic,"Third valid hit exhausts shells and freezes Defeat");
            int id=manager.GetInstanceID();manager.RestartGame();yield return ()=>UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>().GetInstanceID()!=id;
            Bind();Check(motor.Shells==3 && manager.State==GameState.Ready && motor.DashCharges==0 && score.Score==0,"Real scene reload restores three shells and clean round state");
            // No more fixture teleports, resource/score injection or enemy manipulation from here.
            manager.StartGame();began=Time.time;int peakContacts=0,peakEnemies=0,peakChunks=0;float maxCenterError=0;bool captured=false;
            yield return ()=>
            {
                peakContacts=Mathf.Max(peakContacts,motor.TotalQualifiedContacts);peakEnemies=Mathf.Max(peakEnemies,UnityEngine.Object.FindObjectOfType<OrbitBreakerEnemySpawner>().AliveCount);peakChunks=Mathf.Max(peakChunks,arena.GeneratedCount);
                Camera.main.GetComponent<OrbitBreakerCenteredCamera>().Follow();var point=Camera.main.WorldToViewportPoint(motor.transform.position);maxCenterError=Mathf.Max(maxCenterError,Vector2.Distance(point,new Vector2(.5f,.5f)));
                if(!manager.IsPlaying||Time.time-began>40)return true;
                float age=Time.time-began;motor.SetMoveInput(new Vector2(Mathf.Sin(age*.25f)*.6f,.35f));motor.SetAttachmentAim((Vector2)motor.Position+new Vector2(Mathf.Sin(age*.5f)*6,7));
                if(body.velocity.y<0)motor.TryRequestDash();
                if(!captured&&age>12){captured=true;ScreenCapture.CaptureScreenshot(Evidence("T16-play-final.png"));}
                return false;
            };
            Check(peakContacts>=4 && peakEnemies>0 && peakChunks>25 && maxCenterError<.0001f,"Production input-only exploration: seconds="+(Time.time-began).ToString("F2")+", contacts="+peakContacts+", peakEnemies="+peakEnemies+", generatedSectors="+peakChunks+", centreError="+maxCenterError+", shells="+motor.Shells+", score="+score.Score+", state="+manager.State);
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
        private static void Report(string status, string detail) => File.WriteAllText(Evidence("T16-integration.json"), JsonUtility.ToJson(new Result {status=status, observedAtUtc=DateTime.UtcNow.ToString("O"), unityVersion=Application.unityVersion, detail=detail, checks=checks.ToArray()}, true));
    }
}
