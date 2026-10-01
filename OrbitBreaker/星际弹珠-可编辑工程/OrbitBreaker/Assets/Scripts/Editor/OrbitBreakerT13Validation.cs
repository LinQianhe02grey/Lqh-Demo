using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OrbitBreaker.Editor
{
    [InitializeOnLoad]
    public static class OrbitBreakerT13Validation
    {
        private const string Prefix = "OrbitBreaker.T13.";
        private static IEnumerator<Func<bool>> steps;
        private static Func<bool> waiting;
        private static OrbitBreakerPlayerMotor motor;
        private static OrbitBreakerGameManager manager;
        private static OrbitBreakerScoreManager score;
        private static float originalMaximumDeltaTime;
        private static readonly List<string> checks = new List<string>();
        static OrbitBreakerT13Validation() { EditorApplication.playModeStateChanged += OnPlayState; EditorApplication.update += Timeout; }
        [MenuItem("Tools/Orbit Breaker/Validate T13")]
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
            Bind(); body=motor.GetComponent<Rigidbody>();
            var spawner=UnityEngine.Object.FindObjectOfType<OrbitBreakerEnemySpawner>();spawner.enabled=false;
            Check(Camera.main != null && !Camera.main.orthographic && manager.Config.targetScore==3000,"Perspective cosmic board and production target 3000");
            Check(UnityEngine.Object.FindObjectsOfType<OrbitBreakerCosmicMechanism>().Length==2 && UnityEngine.Object.FindObjectsOfType<OrbitBreakerJackpotHole>().Length==1,"Saved spinner, pendulum and jackpot exist once");
            manager.StartGame();
            Place(new Vector3(0,-1,0),Vector3.zero);yield return Delay(.2f);
            Check(body.velocity.y< -1 && Mathf.Abs(body.position.z)<.001f,"XY gravity remains real");
            motor.SetMoveInput(Vector2.right);yield return Delay(.2f);Check(body.velocity.x>1,"WASD request accelerates planet");
            Place(new Vector3(0,-1,0),Vector3.zero);motor.SetMoveInput(Vector2.up);
            Check(motor.TryRequestDash(),"Normal dash accepted");yield return Delay(.04f);
            Check(motor.IsInvulnerable && !motor.TryReceiveHit() && body.velocity.y>15,"Dash is visibly protected, has real speed and rejects damage");
            yield return SafeDelay(1.4f);
            var enemy=Spawn(new Vector3(0,1,0));
            for(int i=0;i<3;i++)
            {
                if(i>0)yield return SafeDelay(.85f);
                enemy.GetComponent<Rigidbody>().position=new Vector3(0,1,0);
                Place(new Vector3(0,-.7f,0),Vector3.zero);motor.SetMoveInput(Vector2.up);Check(motor.TryRequestDash(),"Dash attack "+i);
                int before=enemy.Health; float until=Time.time+.4f;
                yield return ()=> enemy==null || enemy.Health<before || Time.time>=until;
                if(i<2)Check(enemy!=null && enemy.Health==100-40*(i+1),"Real dash contact reduces HP to "+(100-40*(i+1))+" without one-hit kill");
                else Check(enemy==null || enemy.IsDead,"Third physical dash kills enemy");
            }
            Check(score.Score==80,"Kill pays once; nonlethal contacts pay zero");
            yield return SafeDelay(.9f);
            foreach(var flipper in UnityEngine.Object.FindObjectsOfType<OrbitBreakerFlipperController>().OrderBy(f=>f.Side))
            {
                int before=motor.ComboCount;Place(flipper.transform.position,Vector3.down*8);
                Check(flipper.TryActivate() && !flipper.TryActivate(),"Flipper accepts once and rejects same-frame repeat");yield return Delay(.04f);
                Check(body.velocity.y>14 && motor.ComboCount==before+1 && motor.IsLaunched && motor.IsInvulnerable && !motor.TryReceiveHit(),"Real flipper impulse increments combo and grants launch immunity");
            }
            Check(Mathf.Abs(motor.ComboMultiplier-1.5f)<.001f && motor.AttackDamage==60,"Two genuine launches give x1.50 damage/score");
            enemy=Spawn(new Vector3(0,1,0));Place(new Vector3(0,-.4f,0),Vector3.up*16);
            yield return Delay(.12f);
            Check(enemy.Health==40 && !motor.IsStunned,"Protected launch collision deals scaled 60 damage");UnityEngine.Object.Destroy(enemy.gameObject);
            var bumper=GameObject.Find("IonBumperLeft");int combo=motor.ComboCount;
            Place(bumper.transform.position+Vector3.down*1.7f,Vector3.up*10);yield return Delay(.15f);
            Check(motor.ComboCount>combo && body.velocity.y<0,"Real ion bumper collision reflects ball and continues combo");
            foreach(var machine in UnityEngine.Object.FindObjectsOfType<OrbitBreakerCosmicMechanism>())
            {
                float angle=machine.MotionAngle;yield return SafeDelay(.2f);Check(Mathf.Abs(machine.MotionAngle-angle)>1,"Machine animates by kinematic physics: "+machine.name);
                combo=motor.ComboCount;
                Vector3 local=machine.pendulum?new Vector3(.95f,-2.3f,0):new Vector3(.68f,0,0);
                Vector3 normal=machine.transform.right;
                Place(machine.transform.TransformPoint(local),-normal*8);yield return Delay(.10f);
                Check(motor.ComboCount>combo,"Actual contact launches ball: "+machine.name);
            }
            var hole=UnityEngine.Object.FindObjectOfType<OrbitBreakerJackpotHole>();int previousScore=score.Score;
            int expected=Mathf.RoundToInt(OrbitBreakerJackpotHole.BasePoints*motor.ComboMultiplier);
            Place(hole.transform.position+Vector3.down*1.6f,Vector3.up*8);yield return Delay(.12f);
            Check(motor.IsInJackpot && hole.Captures==1 && score.Score==previousScore+expected,"Jackpot physically captures and pays 300 x current combo once");
            Check(!motor.TryRequestDash() && !motor.TryReceiveHit(),"Jackpot capture safely owns movement");
            yield return Delay(.75f);
            Check(!motor.IsInJackpot && motor.IsLaunched && body.velocity.y< -10 && hole.Captures==1,"Jackpot ejects with protected downward impulse, no stay farming");
            Place(hole.transform.position+Vector3.down*2,Vector3.zero);yield return Delay(.05f);
            Place(hole.transform.position,Vector3.zero);yield return Delay(.05f);
            Check(hole.Captures==1,"Jackpot cooldown rejects immediate re-entry");
            yield return SafeDelay(1.5f);
            var point=UnityEngine.Object.FindObjectsOfType<OrbitBreakerAttachmentPoint>().OrderBy(p=>p.Center.x).First();
            Place((Vector3)point.Center+Vector3.down*OrbitBreakerAttachmentPoint.OrbitRadius,Vector3.zero);
            motor.SetAttachmentAim(point.Center+Vector2.right*4);
            Check(motor.TryAttach(point) && motor.IsInvulnerable && !motor.TryReceiveHit(),"Orbit attachment is immune to enemies and hazard hit requests");
            enemy=Spawn((Vector3)point.Center+new Vector3(.9f,-1.15f,0));
            float began=Time.time; bool observedRepel=false;
            yield return ()=>{float age=Time.time-began;float angle=(-90+age*270)*Mathf.Deg2Rad;motor.SetAttachmentAim(point.Center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*4);observedRepel |= enemy.IsRepelled;return age>.45f;};
            Check(motor.IsAttached && enemy.Health<100 && observedRepel,"Actual orbit sweep damages and repels enemy without breaking attachment; attached="+motor.IsAttached+", HP="+enemy.Health+", repel="+observedRepel);
            Check(Physics.GetIgnoreCollision(body.GetComponent<SphereCollider>(),enemy.GetComponent<BoxCollider>()),"Attachment ignores enemy physical pushing");
            UnityEngine.Object.Destroy(enemy.gameObject);
            began=Time.time;
            yield return ()=>{float age=Time.time-began;float angle=(30+age*270)*Mathf.Deg2Rad;motor.SetAttachmentAim(point.Center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*4);return age>.8f;};
            Check(motor.AttachmentCharge>.95f && motor.GetComponent<OrbitBreakerPlayerFeedback>().OrbitVfxActive,"Rotating planet charges with energy VFX");
            ScreenCapture.CaptureScreenshot(Evidence("T13-orbit-play.png"));
            motor.SetAttachmentAim(point.Center+((Vector2)motor.Position-point.Center).normalized*5);combo=motor.ComboCount;
            float firedSpeed=0;int firedCombo=0;bool firedProtection=false;
            System.Action observeFire=()=>{firedSpeed=body.velocity.magnitude;firedCombo=motor.ComboCount;firedProtection=motor.IsLaunched;};
            motor.DashStarted+=observeFire;
            Check(motor.TryRequestDash(),"Only Space request releases aimed magnet launch");
            float fireDeadline=Time.time+.3f;yield return ()=>firedSpeed>0 || Time.time>=fireDeadline;
            motor.DashStarted-=observeFire;
            Check(firedProtection && firedCombo==combo+1 && firedSpeed>23.9f,"Charged magnet fires at 24 and continues launch combo; speed="+firedSpeed+", combo="+firedCombo);
            yield return SafeDelay(1.5f);
            Check(motor.TryReceiveHit() && motor.ComboCount==0,"Unprotected hit resets combo and stuns");
            Check(!motor.TryRequestDash() && !motor.TryReceiveHit(),"Stun blocks steering abilities and repeated damage");yield return SafeDelay(1.2f);
            Place((Vector3)point.Center+Vector3.down*OrbitBreakerAttachmentPoint.OrbitRadius,Vector3.zero);
            motor.SetAttachmentAim(point.Center+Vector2.up*5);
            Check(motor.TryAttach(point) && motor.TryRequestDash() && motor.TryRequestBoardImpulse(Vector2.up,18),"Board impulse supersedes queued magnet launch in same physics step");
            yield return Delay(.04f);Check(motor.ComboCount==1,"Competing requests create only one launch");
            yield return SafeDelay(1.5f);
            motor.SetMoveInput(Vector2.up);Check(motor.TryRequestDash(),"Ordinary dash after cancelled magnet request");yield return Delay(.04f);
            Check(motor.ComboCount==1,"Cancelled magnet request cannot leak into a later ordinary dash or farm combo");
            yield return SafeDelay(8.1f);
            motor.TryRequestBoardImpulse(Vector2.up,18);yield return Delay(.04f);
            Check(motor.ComboCount==1,"Combo can restart after recovery");yield return SafeDelay(8.1f);
            Check(motor.ComboCount==0 && motor.ComboMultiplier==1,"Combo expires after eight seconds without real launch");
            // Actual hazardous contact, not only an API call.
            var hazard=UnityEngine.Object.FindObjectsOfType<OrbitBreakerHazard>().First();
            Place(hazard.transform.position+Vector3.down*1.5f,Vector3.up*8);yield return Delay(.15f);
            Check(motor.IsStunned,"Unprotected real hazard collision causes stun");yield return SafeDelay(1.2f);
            Place(hazard.transform.position+Vector3.down*1.7f,Vector3.zero);motor.SetMoveInput(Vector2.up);motor.TryRequestDash();yield return Delay(.12f);
            Check(!motor.IsStunned && motor.IsInvulnerable,"Actual hazard contact during dash cannot stun");
            // Preserve production threshold; fill via real upward target contacts, not direct score calls.
            yield return SafeDelay(1.4f);
            var target=UnityEngine.Object.FindObjectsOfType<OrbitBreakerScoreTarget>().OrderByDescending(t=>t.transform.position.y).First();
            int hits=0;
            while(manager.IsPlaying && hits<35)
            {
                Place(target.transform.position+Vector3.down*1.65f,Vector3.up*18);yield return Delay(.12f);
                hits++;if(manager.IsPlaying)yield return SafeDelay(.55f);
            }
            Check(manager.State==GameState.Victory && score.Score>=3000,"Production threshold reached through actual target hits; contact fixture count="+hits);
            Check(body.isKinematic && !motor.IsInvulnerable && motor.ComboCount==0,"Victory freezes physics and clears combat/combos");
            int id=manager.GetInstanceID();manager.RestartGame();yield return ()=>UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>().GetInstanceID()!=id;
            Bind();body=motor.GetComponent<Rigidbody>();spawner=UnityEngine.Object.FindObjectOfType<OrbitBreakerEnemySpawner>();
            Check(spawner.enabled && UnityEngine.Object.FindObjectsOfType<OrbitBreakerEnemyController>().Length==0 && motor.ComboCount==0,"Real scene reload resets enemies, score, combo and machinery");
            manager.StartGame();spawner.enabled=false;Place(new Vector3(0,-8.9f,0),Vector3.down*10);
            motor.TryRequestBoardImpulse(Vector2.down,18);yield return Delay(.04f);
            Check(motor.IsInvulnerable,"Black-hole fixture enters while launch-protected");yield return ()=>!manager.IsPlaying;
            Check(manager.State==GameState.Defeat && body.isKinematic,"Actual bottom black hole kills even during invulnerable launch");
            id=manager.GetInstanceID();manager.RestartGame();yield return ()=>UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>().GetInstanceID()!=id;
            Bind();body=motor.GetComponent<Rigidbody>();manager.StartGame();
            float natural=Time.time;
            yield return ()=>{if(Time.time-natural>30)throw new InvalidOperationException("No-input drain failed");return !manager.IsPlaying;};
            Check(manager.State==GameState.Defeat,"Unmodified production scene naturally drains without input in "+(Time.time-natural).ToString("F2")+"s");
            id=manager.GetInstanceID();manager.RestartGame();yield return ()=>UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>().GetInstanceID()!=id;
            Bind();body=motor.GetComponent<Rigidbody>();UnityEngine.Random.InitState(1301);manager.StartGame();spawner=UnityEngine.Object.FindObjectOfType<OrbitBreakerEnemySpawner>();
            float fullStart=Time.time;int peakEnemies=0,scoreEvents=0;bool retreat=false;
            score.ScoreChanged+=total=>{scoreEvents++;retreat=motor.Position.y>7f;};
            // Full production round: input-equivalent requests only; no placements, score calls or disabled spawns.
            yield return ()=>
            {
                if(!manager.IsPlaying)return true;
                peakEnemies=Mathf.Max(peakEnemies,spawner.AliveCount);
                Vector2 pos=motor.Position,vel=body.velocity;
                if(Time.time-fullStart>100)throw new InvalidOperationException("Production input-driver timeout: "+score.Score+", "+pos);
                if(pos.y < -5f)
                    foreach(var flipper in UnityEngine.Object.FindObjectsOfType<OrbitBreakerFlipperController>())flipper.TryActivate();
                if(pos.y>7.5f && Mathf.Abs(pos.x)>1.5f)
                {
                    // A real player can dash away from a crowded upper corner. The input-only driver
                    // must do the same instead of permanently pressing W against both walls.
                    retreat=true;motor.SetMoveInput(new Vector2(-Mathf.Sign(pos.x),-1));
                    motor.TryRequestDash();return false;
                }
                if(retreat && pos.y<6.3f)retreat=false;
                float x=Mathf.Clamp(-pos.x*1.8f-vel.x*.45f,-.7f,.7f);
                motor.SetMoveInput(new Vector2(x,retreat?-1:1));
                if(!retreat && !motor.IsStunned && motor.DashCooldownRemaining<=0 && pos.y<8f)
                {motor.SetMoveInput(new Vector2(Mathf.Clamp(-pos.x*.25f,-.4f,.4f),1));motor.TryRequestDash();}
                if(Time.time-fullStart>100)throw new InvalidOperationException("Production input-driver timeout: "+score.Score+", "+pos);
                return false;
            };
            Check(manager.State==GameState.Victory && score.Score>=3000 && peakEnemies>0 && scoreEvents>5,
                "Full production input-only round wins with automatic enemies; seconds="+(Time.time-fullStart).ToString("F2")+", score="+score.Score+", peakEnemies="+peakEnemies+", scoringEvents="+scoreEvents);
            ScreenCapture.CaptureScreenshot(Evidence("T13-full-victory.png"));yield return Delay(.06f);
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
        private static void Report(string status, string detail) => File.WriteAllText(Evidence("T13-integration.json"), JsonUtility.ToJson(new Result {status=status, observedAtUtc=DateTime.UtcNow.ToString("O"), unityVersion=Application.unityVersion, detail=detail, checks=checks.ToArray()}, true));
    }
}
