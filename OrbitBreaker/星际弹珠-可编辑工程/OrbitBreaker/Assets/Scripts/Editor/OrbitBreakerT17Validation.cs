using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OrbitBreaker.Editor
{
    [InitializeOnLoad]
    public static class OrbitBreakerT17Validation
    {
        private const string Prefix = "OrbitBreaker.T17.";
        private static IEnumerator<Func<bool>> steps;
        private static Func<bool> waiting;
        private static OrbitBreakerPlayerMotor motor;
        private static OrbitBreakerGameManager manager;
        private static OrbitBreakerScoreManager score;
        private static float originalMaximumDeltaTime;
        private static readonly List<string> checks = new List<string>();
        static OrbitBreakerT17Validation() { EditorApplication.playModeStateChanged += OnPlayState; EditorApplication.update += Timeout; }
        [MenuItem("Tools/Orbit Breaker/Validate T17")]
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
        private static OrbitBreakerCombatConfig original,config;
        private static void Bind()
        {
            motor=UnityEngine.Object.FindObjectOfType<OrbitBreakerPlayerMotor>();manager=motor.OrbitBreakerGameManager;score=UnityEngine.Object.FindObjectOfType<OrbitBreakerScoreManager>();body=motor.GetComponent<Rigidbody>();
            arena=UnityEngine.Object.FindObjectOfType<OrbitBreakerProceduralArena>();
            motor.GetComponent<OrbitBreakerPlayerInput>().enabled=false;UnityEngine.Object.Destroy(motor.GetComponent<OrbitBreakerPlayerInput>());
            original=manager.Config;config=UnityEngine.Object.Instantiate(original);
            var so=new SerializedObject(manager);so.FindProperty("config").objectReferenceValue=config;so.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void Place(Vector3 point,Vector3 velocity)
        {body.position=point;body.velocity=velocity;motor.SetMoveInput(Vector2.zero);Physics.SyncTransforms();arena.RefreshAroundPlayer();}
        private static Func<bool> Safe(float seconds)
        {float end=Time.time+seconds;return ()=>{Place(new Vector3(1,1),Vector3.zero);return Time.time>=end;};}
        private static OrbitBreakerEnemyController Spawn(Vector3 p,bool slime=false)
        {
            var enemy=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/OrbitBreakerEnemy.prefab"),p,Quaternion.identity).GetComponent<OrbitBreakerEnemyController>();
            enemy.transform.localScale*=.8f;enemy.Initialize(motor,score);if(slime)enemy.gameObject.AddComponent<OrbitBreakerSlimeAura>();return enemy;
        }
        private static IEnumerator<Func<bool>> RunChecks()
        {
            Bind();var spawner=UnityEngine.Object.FindObjectOfType<OrbitBreakerEnemySpawner>();spawner.enabled=false;
            Check(typeof(OrbitBreakerCombatConfig).GetFields(System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Instance).Count(f=>f.DeclaringType==typeof(OrbitBreakerCombatConfig)&&!Attribute.IsDefined(f,typeof(HideInInspector)))==8,"Eight visible editable core-rule fields; legacy tuning preserved hidden");
            Check(motor.Shells==3 && motor.DashCharges==0 && motor.DashCapacity==3,"Ready starts with three shells and empty capsule slots");
            Check(UnityEngine.Object.FindObjectsOfType<OrbitBreakerBumper>().Select(b=>b.Variant).Distinct().Count()==3,"Procedural pins contain cyan, purple and gold launch variants");
            Check(UnityEngine.Object.FindObjectOfType<OrbitBreakerKillZone>()==null && UnityEngine.Object.FindObjectOfType<OrbitBreakerFlipperController>()==null,"No fixed drain, boundary or bottom flippers restored");
            manager.StartGame();config.enemyHealth=160;var e=Spawn(new Vector3(0,1.2f));Check(e.Health==160&&e.HealthCapacity==160,"Edited enemy HP applies to live spawn and health-bar maximum");UnityEngine.Object.Destroy(e.gameObject);config.enemyHealth=100;
            var pin=UnityEngine.Object.FindObjectsOfType<OrbitBreakerBumper>().OrderBy(p=>p.transform.position.sqrMagnitude).First();
            config.maxDashCharges=2;float[] speeds=new float[3];
            for(int i=0;i<8;i++)
            {
                pin.Configure(i%3);int before=motor.TotalQualifiedContacts;Place(pin.transform.position+Vector3.up*1.15f,Vector3.down*9);yield return Delay(.13f);
                Check(motor.TotalQualifiedContacts==before+1,"Physical pin contact credits once "+i);
                if(i<3)speeds[i]=body.velocity.magnitude;
                yield return Safe(.25f);
            }
            Check(speeds[0]<speeds[1] && speeds[1]<speeds[2],"Physical launch speeds vary by pin colour: "+string.Join(",",speeds.Select(s=>s.ToString("F2"))));
            Check(motor.DashCharges==2&&motor.DashCapacity==2,"Edited capacity two limits stored dash charges");yield return Delay(.04f);
            Check(motor.GetComponent<OrbitBreakerDashCapsules>().LitCount==2 && GameObject.Find("DashChargeCapsules").transform.childCount==2,"Two earned charges render as two bright capsules above player");
            config.dashSpeed=12;config.dashCooldown=.5f;Place(Vector3.zero,Vector3.zero);motor.SetAttachmentAim(Vector2.right*5);Check(motor.TryRequestDash(),"Edited dash queued");yield return Delay(.04f);
            Check(body.velocity.x>11.5f&&body.velocity.x<12.5f&&motor.DashCooldownRemaining>.4f&&motor.DashCooldownRemaining<=.5f,"Edited dash speed and cooldown affect real motion");
            Check(motor.DashCharges==1 && !motor.TryReceiveHit() && motor.Shells==3,"Dash spends one capsule and preserves offensive immunity");
            yield return Safe(.6f);config.dashSpeed=18;config.dashCooldown=1;config.maxDashCharges=3;
            var magnet=UnityEngine.Object.FindObjectsOfType<OrbitBreakerAttachmentPoint>().OrderBy(p=>p.transform.position.sqrMagnitude).First();
            Place((Vector3)magnet.Center+Vector3.down*2.2f,Vector3.zero);motor.SetAttachmentAim(magnet.Center+Vector2.right*5);
            Check(motor.TryAttach(magnet),"Expanded E capture accepts distance 2.2, beyond old 1.6");
            float start=Time.time;float angle=Mathf.Atan2(motor.Position.y-magnet.Center.y,motor.Position.x-magnet.Center.x);yield return Delay(.85f);
            Check(motor.IsAttached&&motor.AttachmentCharge>.9f&&Vector2.Distance(motor.Position,magnet.Center)<1.2f,"Stationary mouse: motor automatically orbits and charges from actual motion");
            e=Spawn(motor.Position+Vector3.right*.6f,true);yield return Delay(.1f);
            Check(motor.IsAttached&&!motor.IsSlowed&&motor.Shells==3,"Black enemy aura cannot disturb attached player");UnityEngine.Object.Destroy(e.gameObject);
            motor.SetAttachmentAim((Vector2)motor.Position+Vector2.right*6);Check(motor.TryRequestDash(),"Space alone chooses auto-orbit launch time");yield return Delay(.04f);
            Check(!motor.IsAttached&&body.velocity.x>22&&motor.DashCharges==0,"Charged auto-orbit launches toward stationary mouse and consumes one");
            yield return Safe(1.5f);
            // Test slow and drop probability with disposable runtime config, never modify the asset.
            e=Spawn(new Vector3(1,3),true);Place(new Vector3(1,1),Vector3.zero);yield return Delay(.02f);
            Check(motor.IsSlowed&&Mathf.Abs(motor.MovementFactor-.65f)<.001f,"Black enemy slows inside visible radius");
            motor.SetMoveInput(Vector2.right);yield return Delay(.1f);float slowVX=body.velocity.x;
            Check(slowVX>.6f&&slowVX<2,"Slime reduces actual move acceleration, vx="+slowVX);
            Place(new Vector3(1,-2),Vector3.zero);Check(!motor.IsSlowed,"Leaving aura immediately restores movement factor");
            UnityEngine.Object.Destroy(e.gameObject);yield return Safe(.1f);
            for(int i=0;i<4;i++){Place(pin.transform.position+Vector3.up*1.15f,Vector3.down*9);yield return Delay(.13f);yield return Safe(.25f);}
            e=Spawn(new Vector3(1,3),true);Place(new Vector3(1,1),Vector3.zero);motor.SetAttachmentAim(new Vector2(6,1));Check(motor.TryRequestDash(),"Dash is still usable inside slime");yield return Delay(.04f);
            Check(body.velocity.x>10.8f&&body.velocity.x<12.5f,"Slime scales actual dash speed to 65 percent");UnityEngine.Object.Destroy(e.gameObject);
            yield return Safe(1.5f);Check(motor.TryReceiveHit()&&motor.Shells==2,"Test damage creates one missing shell");yield return Safe(1.1f);
            config.enemyHealth=20;config.shieldDropChance=0;e=Spawn(new Vector3(0,1.2f));
            Place(new Vector3(0,-.3f),Vector3.up*14);motor.TryRequestBoardImpulse(Vector2.up,17);yield return Delay(.16f);
            Check((e==null||e.IsDead)&&UnityEngine.Object.FindObjectsOfType<OrbitBreakerShieldPickup>().Length==0,"Zero configured drop probability produces no pickup after real kill");
            yield return Safe(.45f);config.shieldDropChance=1;e=Spawn(new Vector3(0,1.2f));Place(new Vector3(0,-.3f),Vector3.up*14);motor.TryRequestBoardImpulse(Vector2.up,17);yield return Delay(.16f);
            var pickup=UnityEngine.Object.FindObjectOfType<OrbitBreakerShieldPickup>();
            Check(e==null||e.IsDead,"Second real kill for guaranteed shield drop");
            if(pickup!=null){Place(pickup.transform.position,Vector3.zero);yield return Delay(.04f);}
            Check(motor.Shells==3 && UnityEngine.Object.FindObjectsOfType<OrbitBreakerShieldPickup>().Length==0,"Guaranteed drop is collected via trigger and restores one shell");
            Check(!motor.TryRestoreShell()&&motor.Shells==3,"Shield pickup cannot exceed three layers");config.enemyHealth=100;config.shieldDropChance=.3f;
            yield return Safe(1.5f);
            var jackpot=UnityEngine.Object.FindObjectsOfType<OrbitBreakerJackpotHole>().OrderBy(h=>h.transform.position.sqrMagnitude).First();int beforeScore=score.Score;
            Place(jackpot.transform.position+Vector3.down*1.4f,Vector3.up*10);yield return Delay(.14f);
            Check(score.Score-beforeScore>=600 && motor.IsInJackpot && motor.HasRewardBoost,"Real reward-hole entry pays high points and grants seven-second boost");
            Check(motor.RewardRemaining>6.6f && motor.LaunchSpeedFactor>1.3f,"Reward timer and speed multiplier active");yield return Delay(.75f);
            Check(motor.IsLaunched&&body.velocity.magnitude>23,"Reward-hole ejection receives boosted speed");
            Place(new Vector3(1,1),Vector3.zero);motor.TryRequestBoardImpulse(Vector2.right,20);yield return Delay(.04f);
            Check(body.velocity.x>26.5f && body.velocity.x<27.5f,"Reward boosts collision launch to 27 from 20");
            var trail=motor.GetComponentsInChildren<TrailRenderer>().First(t=>t.name=="LaunchComet");
            Check(trail.enabled&&trail.startColor.r>.9f&&trail.startColor.g<.2f,"Reward boost displays red comet trail");
            ScreenCapture.CaptureScreenshot(Evidence("T17-reward.png"));
            yield return Safe(1.1f);Place(Vector3.zero,Vector3.zero);motor.SetAttachmentAim(Vector2.right*5);
            Check(motor.DashCharges>0&&motor.TryRequestDash(),"Kill-earned resource launches boosted dash");yield return Delay(.04f);
            Check(body.velocity.x>23.8f&&body.velocity.x<24.8f,"Reward boosts dash 18 to 24.3");
            yield return Safe(7.1f);Check(!motor.HasRewardBoost&&motor.LaunchSpeedFactor==1,"Reward expires without permanent speed stacking");
            Check(UnityEngine.Object.FindObjectOfType<OrbitBreakerCombatAudio>().PlayedCount>15,"Gameplay events play synthesized audio with bounce rate limiting");
            var mechanism=UnityEngine.Object.FindObjectsOfType<OrbitBreakerCosmicMechanism>().First(m=>!m.pendulum);float previous=mechanism.MotionAngle;yield return Safe(.2f);
            Check(mechanism.MotionAngle-previous>20,"Rotating obstacle speed increased above 100 degrees per second");
            spawner.enabled=true;Place(new Vector3(180,180),Vector3.zero);for(int i=0;i<12;i++)spawner.TrySpawn();
            Check(spawner.AliveCount==12 && UnityEngine.Object.FindObjectsOfType<OrbitBreakerSlimeAura>().Length==4,"Twelve enemies with one black slime enemy per three spawns");
            Check(Mathf.Abs(config.enemySpawnInterval-1.2f)<.001f,"Production spawn interval shortened to 1.2 seconds");
            foreach(var enemy in UnityEngine.Object.FindObjectsOfType<OrbitBreakerEnemyController>())UnityEngine.Object.Destroy(enemy.gameObject);spawner.enabled=false;
            yield return Safe(1.5f);
            for(int i=0;i<3;i++){Check(motor.TryReceiveHit(),"Shell damage accepted "+i);if(manager.IsPlaying)yield return Safe(1.1f);}
            Check(manager.State==GameState.Defeat&&motor.Shells==0&&!motor.HasRewardBoost,"Shell exhaustion still ends round and clears temporary effects");
            int id=manager.GetInstanceID();manager.RestartGame();yield return ()=>UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>().GetInstanceID()!=id;
            Bind();Check(motor.Shells==3&&motor.DashCapacity==3&&motor.DashCharges==0&&!motor.HasRewardBoost&&!motor.IsSlowed,"Reload clears temporary states and restores production config and shells");
            // Fresh production scene, only movement/aim/dash requests; normal enemies and random terrain.
            manager.StartGame();float began=Time.time;int contacts=0,peak=0;bool screen=false;
            yield return ()=>
            {
                contacts=Mathf.Max(contacts,motor.TotalQualifiedContacts);peak=Mathf.Max(peak,UnityEngine.Object.FindObjectOfType<OrbitBreakerEnemySpawner>().AliveCount);
                if(!manager.IsPlaying||Time.time-began>30)return true;
                float age=Time.time-began;motor.SetMoveInput(new Vector2(Mathf.Sin(age*.3f)*.5f,.38f));motor.SetAttachmentAim((Vector2)motor.Position+new Vector2(5,7));
                if(body.velocity.y<0)motor.TryRequestDash();
                if(!screen&&age>10){screen=true;ScreenCapture.CaptureScreenshot(Evidence("T17-production.png"));}
                return false;
            };
            Check(contacts>=4&&peak>=3,"Production exploration: seconds="+(Time.time-began).ToString("F2")+", contacts="+contacts+", peakEnemies="+peak+", score="+score.Score+", shells="+motor.Shells+", state="+manager.State);
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
        private static void Report(string status, string detail) => File.WriteAllText(Evidence("T17-integration.json"), JsonUtility.ToJson(new Result {status=status, observedAtUtc=DateTime.UtcNow.ToString("O"), unityVersion=Application.unityVersion, detail=detail, checks=checks.ToArray()}, true));
    }
}
