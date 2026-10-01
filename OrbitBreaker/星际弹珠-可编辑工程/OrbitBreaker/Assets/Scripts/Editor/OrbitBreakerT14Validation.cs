using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OrbitBreaker.Editor
{
    [InitializeOnLoad]
    public static class OrbitBreakerT14Validation
    {
        private const string Prefix = "OrbitBreaker.T14.";
        private static IEnumerator<Func<bool>> steps;
        private static Func<bool> waiting;
        private static OrbitBreakerPlayerMotor motor;
        private static OrbitBreakerGameManager manager;
        private static OrbitBreakerScoreManager score;
        private static float originalMaximumDeltaTime;
        private static readonly List<string> checks = new List<string>();
        static OrbitBreakerT14Validation() { EditorApplication.playModeStateChanged += OnPlayState; EditorApplication.update += Timeout; }
        [MenuItem("Tools/Orbit Breaker/Validate T14")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Start from Edit mode.");
            OrbitBreakerSceneBuilder.Build();
            SessionState.SetBool(Prefix + "Running", true);
            SessionState.SetBool(Prefix + "Complete", false);
            SessionState.SetString(Prefix + "Deadline", DateTime.UtcNow.AddMinutes(3).ToString("O"));
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
            Bind();body=motor.GetComponent<Rigidbody>();
            var spawner=UnityEngine.Object.FindObjectOfType<OrbitBreakerEnemySpawner>();spawner.enabled=false;
            Check(Camera.main.GetComponent<OrbitBreakerBoardCameraFraming>()!=null && !Camera.main.orthographic,"Perspective camera with responsive framing");
            manager.StartGame();
            var enemy=Spawn(new Vector3(-5,1.1f,0));
            var box=enemy.GetComponent<BoxCollider>();
            var solids=UnityEngine.Object.FindObjectsOfType<Collider>().Where(c=>c!=box&&!c.isTrigger&&c.GetComponentInParent<OrbitBreakerPlayerMotor>()==null).ToArray();
            Check(solids.All(c=>Physics.GetIgnoreCollision(box,c)),"OrbitBreakerEnemy ignores all existing environmental solids, including moving machinery");
            Check(!Physics.GetIgnoreCollision(box,body.GetComponent<SphereCollider>()),"Unattached player/enemy contact is preserved");
            var mat=enemy.GetComponent<Renderer>().sharedMaterial;
            Check(mat.shader.name=="Pinball/OrbitBreakerEnemyPhase" && mat.GetFloat("_Opacity")>.3f && mat.GetFloat("_Opacity")<.65f,"OrbitBreakerEnemy uses translucent material with readable edge");
            float until=Time.time+2.1f;
            yield return ()=>{Place(new Vector3(1.2f,1.1f,0),Vector3.zero);return Time.time>=until;};
            Check(enemy.Position.x>-.7f,"OrbitBreakerEnemy physically crosses solid bumper without blockage; x="+enemy.Position.x);
            var second=Spawn(enemy.Position+Vector3.right*.2f);
            Check(Physics.GetIgnoreCollision(box,second.GetComponent<BoxCollider>()),"Newly spawned enemies do not block each other");
            UnityEngine.Object.Destroy(second.gameObject);
            var machine=UnityEngine.Object.FindObjectsOfType<OrbitBreakerCosmicMechanism>().First(m=>!m.pendulum);
            enemy.GetComponent<Rigidbody>().position=machine.transform.position+Vector3.down*2.4f;
            until=Time.time+2.3f;
            yield return ()=>{Place(machine.transform.position+Vector3.up*3.1f,Vector3.zero);return Time.time>=until;};
            Check(enemy.Position.y>machine.transform.position.y+1.8f,"OrbitBreakerEnemy crosses rotating mechanism while it moves");
            var added=GameObject.CreatePrimitive(PrimitiveType.Cube);added.name="T14_RuntimeBarrier";added.transform.position=new Vector3(0,0,0);added.transform.localScale=new Vector3(1,4,1);
            enemy.GetComponent<Rigidbody>().position=new Vector3(-2,0,0);
            until=Time.time+2;
            yield return ()=>{Place(new Vector3(4,0,0),Vector3.zero);return Time.time>=until;};
            Check(enemy.Position.x>1.5f && Physics.GetIgnoreCollision(box,added.GetComponent<Collider>()),"Runtime-added solid is phased and enemy traverses it");
            UnityEngine.Object.Destroy(added);UnityEngine.Object.Destroy(enemy.gameObject);
            yield return SafeDelay(.1f);
            enemy=Spawn(new Vector3(0,1,0));Place(new Vector3(0,-.4f,0),Vector3.up*8);yield return Delay(.12f);
            Check(motor.IsStunned && enemy.Health==100,"Normal real enemy contact still stuns and does not damage enemy");
            UnityEngine.Object.Destroy(enemy.gameObject);yield return SafeDelay(1.2f);
            enemy=Spawn(new Vector3(0,1,0));
            for(int i=0;i<3;i++)
            {
                if(i>0)yield return SafeDelay(.85f);
                enemy.GetComponent<Rigidbody>().position=new Vector3(0,1,0);Place(new Vector3(0,-.7f,0),Vector3.zero);
                motor.SetMoveInput(Vector2.up);Check(motor.TryRequestDash(),"Dash request "+i);int hp=enemy.Health;until=Time.time+.4f;
                yield return ()=>enemy==null||enemy.Health<hp||Time.time>=until;
                Check(!motor.IsStunned,"Dash hit remains immune");
                Check(i<2?enemy!=null&&enemy.Health==100-40*(i+1):enemy==null||enemy.IsDead,"Real phased-enemy HP/kill contact "+i);
            }
            Check(score.Score==80,"Kill score paid once");yield return SafeDelay(1);
            var point=UnityEngine.Object.FindObjectsOfType<OrbitBreakerAttachmentPoint>().OrderBy(p=>p.Center.x).First();
            Place((Vector3)point.Center+Vector3.down*OrbitBreakerAttachmentPoint.OrbitRadius,Vector3.zero);motor.SetAttachmentAim(point.Center+Vector2.right*4);
            Check(motor.TryAttach(point),"Moved orbit point is reachable");
            enemy=Spawn((Vector3)point.Center+new Vector3(.9f,-1.15f,0));float began=Time.time;bool repelled=false;
            yield return ()=>{float age=Time.time-began;float a=(-90+age*270)*Mathf.Deg2Rad;motor.SetAttachmentAim(point.Center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*4);repelled|=enemy.IsRepelled;return age>.45f;};
            Check(motor.IsAttached&&enemy.Health<100&&repelled,"Orbit sweep damages and repels ghost while shielding player");
            Check(Physics.GetIgnoreCollision(box=enemy.GetComponent<BoxCollider>(),body.GetComponent<SphereCollider>()),"Orbit collision shield intact");
            UnityEngine.Object.Destroy(enemy.gameObject);motor.ReleaseAttachment();yield return SafeDelay(1);
            foreach(var f in UnityEngine.Object.FindObjectsOfType<OrbitBreakerFlipperController>())
            {
                Place(f.transform.position,Vector3.down*8);int before=motor.ComboCount;Check(f.TryActivate(),"Flipper request "+f.Side);yield return Delay(.04f);
                Check(body.velocity.y>14&&motor.ComboCount==before+1&&motor.IsInvulnerable,"Flipper launch, combo and immunity "+f.Side);
            }
            var hole=UnityEngine.Object.FindObjectOfType<OrbitBreakerJackpotHole>();int total=score.Score;int expected=Mathf.RoundToInt(OrbitBreakerJackpotHole.BasePoints*motor.ComboMultiplier);
            Place(hole.transform.position+Vector3.down*1.6f,Vector3.up*8);yield return Delay(.12f);
            Check(motor.IsInJackpot&&score.Score==total+expected,"Offset jackpot physically captures and awards scaled score");yield return Delay(.75f);
            Check(motor.IsLaunched&&!motor.IsInJackpot&&body.velocity.y< -10,"Offset jackpot ejects safely");
            yield return SafeDelay(1.5f);
            spawner.enabled=true;var spawned=spawner.TrySpawn();Check(spawned!=null,"Production spawner creates phased enemy");
            Check(Physics.GetIgnoreCollision(spawned.GetComponent<BoxCollider>(),GameObject.Find("LeftWall").GetComponent<Collider>()),"Production spawn already phases through walls");spawner.enabled=false;UnityEngine.Object.Destroy(spawned.gameObject);
            Place(new Vector3(0,-8.9f,0),Vector3.down*10);motor.TryRequestBoardImpulse(Vector2.down,18);yield return Delay(.04f);
            Check(motor.IsInvulnerable,"Drain fixture enters protected");yield return ()=>!manager.IsPlaying;
            Check(manager.State==GameState.Defeat,"Black hole still kills protected planet");
            int id=manager.GetInstanceID();manager.RestartGame();yield return ()=>UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>().GetInstanceID()!=id;
            Bind();body=motor.GetComponent<Rigidbody>();Check(UnityEngine.Object.FindObjectsOfType<OrbitBreakerEnemyController>().Length==0&&motor.ComboCount==0,"Real reload clears enemies and combo");
        }
        private static Func<bool> Delay(float duration) { float end = Time.time + duration; return () => Time.time >= end; }
        private static void Tick()
        {
            try
            {
                if (waiting != null && !waiting()) return;
                if (steps.MoveNext()) waiting = steps.Current;
                else { SessionState.SetBool(Prefix + "Complete", true); Report("PASS", "All T14 integration checks passed."); EditorApplication.isPlaying = false; }
            }
            catch (Exception ex) { Fail(ex.ToString()); }
        }
        private static void Check(bool pass, string detail) { if (!pass) throw new InvalidOperationException(detail.Length > 0 ? detail : "Attachment expired before 7.35 seconds."); if (detail.Length > 0) checks.Add(detail); }
        private static void Fail(string detail) { Report("FAIL", detail); SessionState.SetBool(Prefix+"Running", false); Debug.LogError("T14: " + detail); EditorApplication.isPlaying = false; }
        private static void Timeout() { if (SessionState.GetBool(Prefix+"Running", false) && DateTime.UtcNow > DateTime.Parse(SessionState.GetString(Prefix+"Deadline", ""), null, System.Globalization.DateTimeStyles.RoundtripKind)) Fail("T14 deadline exceeded."); }
        private static void OnPlayState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode && originalMaximumDeltaTime > 0f) { Time.maximumDeltaTime = originalMaximumDeltaTime; originalMaximumDeltaTime = 0f; }
            if (!SessionState.GetBool(Prefix+"Running", false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                checks.Clear(); waiting = null; steps = RunChecks();
                originalMaximumDeltaTime = Time.maximumDeltaTime; Time.maximumDeltaTime = Time.fixedDeltaTime;
                var probe = new GameObject("T14_ValidationProbe") { hideFlags=HideFlags.DontSave };
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
        private static void Report(string status, string detail) => File.WriteAllText(Evidence("T14-integration.json"), JsonUtility.ToJson(new Result {status=status, observedAtUtc=DateTime.UtcNow.ToString("O"), unityVersion=Application.unityVersion, detail=detail, checks=checks.ToArray()}, true));
    }
}
