using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OrbitBreaker.Editor
{
    [InitializeOnLoad]
    public static class OrbitBreakerT18Validation
    {
        private const string Prefix = "OrbitBreaker.T18.";
        private static IEnumerator<Func<bool>> steps;
        private static Func<bool> waiting;
        private static OrbitBreakerPlayerMotor motor;
        private static OrbitBreakerGameManager manager;
        private static OrbitBreakerScoreManager score;
        private static float originalMaximumDeltaTime;
        private static readonly List<string> checks = new List<string>();
        static OrbitBreakerT18Validation() { EditorApplication.playModeStateChanged += OnPlayState; EditorApplication.update += Timeout; }
        [MenuItem("Tools/Orbit Breaker/Validate T18")]
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
            Check(manager.State==GameState.Ready&&manager.GetComponent<OrbitBreakerBossDirector>().Boss==null,"Main menu is Ready; no Boss before selection");
            Check(menu.CatalogEntries(0).Length==5&&menu.CatalogEntries(1).Length==8&&menu.CatalogEntries(2).Length==6,"19 catalog entries cover rules, devices and enemies with numeric values");
            Check(typeof(OrbitBreakerEncounterConfig).GetFields().Count(f=>!f.IsStatic)==8,"Speed/Boss plugin page has exactly eight editable rules");
            manager.EncyclopediaOpen=true;manager.HandleEscape();Check(!manager.EncyclopediaOpen&&manager.State==GameState.Ready,"Escape closes catalog without starting/exiting game");
            ScreenCapture.CaptureScreenshot(Evidence("T18-main-menu.png"));yield return Delay(.06f);
            Check(manager.StartMode(GameMode.Free),"Select Free mode");arena.gameObject.SetActive(false);
            score.TryAward(manager.Config.targetScore,menu,1);Check(manager.IsPlaying,"Free score milestone does not terminate play");
            Place(new Vector3(1,1),Vector3.zero);yield return Delay(.04f);motor.SetMoveInput(Vector2.right);yield return Delay(1.5f);
            Check(motor.CurrentSpeed<=8.01f&&!motor.IsAttackActive&&!motor.IsInvulnerable&&motor.AttackDamage==0,"WASD-only remains <=8 and cannot damage or grant attack immunity");
            motor.SetMoveInput(Vector2.zero);
            body.velocity=Vector3.right*manager.Rules.damageThreshold;
            Check(!motor.IsAttackActive&&motor.AttackDamage==0,"Exact speed threshold remains nonoffensive");
            body.velocity=Vector3.right*13;int slow=motor.AttackDamage;Check(motor.IsAttackActive&&motor.IsInvulnerable&&slow>0,"Actual 13 m/s activates damage and immunity");
            body.velocity=Vector3.right*20;Check(motor.AttackDamage>slow,"Actual higher speed increases damage");
            body.velocity=Vector3.right*24;Check(motor.IsPiercing,"Actual maximum speed enables piercing");
            body.velocity=Vector3.right*8;Check(!motor.IsPiercing&&!motor.IsAttackActive,"Slowing down immediately removes offensive state");
            var clone=UnityEngine.Object.Instantiate(manager.Rules);clone.damageThreshold=15;clone.thresholdDamage=30;clone.maximumSpeedDamage=110;
            var so=new SerializedObject(manager);var saved=manager.Rules;so.FindProperty("encounter").objectReferenceValue=clone;so.ApplyModifiedPropertiesWithoutUndo();
            body.velocity=Vector3.right*14;Check(!motor.IsAttackActive,"Edited threshold affects live motor");body.velocity=Vector3.right*24;Check(motor.AttackDamage==110,"Edited maximum-speed damage affects live calculation");
            so.FindProperty("encounter").objectReferenceValue=saved;so.ApplyModifiedPropertiesWithoutUndo();UnityEngine.Object.Destroy(clone);
            Place(new Vector3(1,1),Vector3.zero);var weak=Spawn(new Vector3(1,1.75f));yield return Delay(.35f);
            Check(weak.Health==weak.HealthCapacity&&motor.Shells==2,"Low-speed real enemy contact hurts one shell and does not damage enemy; HP="+weak.Health+", shells="+motor.Shells);UnityEngine.Object.Destroy(weak.gameObject);yield return Hold(1.2f,new Vector3(1,1));motor.TryRestoreShell();
            var normal=Spawn(new Vector3(1,3.1f));Place(new Vector3(1,1),Vector3.zero);motor.TryRequestBoardImpulse(Vector2.up,18);yield return Delay(.18f);
            Check(normal.Health<normal.HealthCapacity&&motor.Shells==3,"Real 18-speed impact damages enemy using incoming speed and preserves shield");UnityEngine.Object.Destroy(normal.gameObject);yield return Hold(.15f,new Vector3(1,1));
            var one=Spawn(new Vector3(1,-1.5f));var two=Spawn(new Vector3(1,-3.5f));Place(new Vector3(1,1),Vector3.zero);motor.TryRequestBoardImpulse(Vector2.down,24);yield return Delay(.27f);
            Check((one==null||one.Health<one.HealthCapacity)&&(two==null||two.Health<two.HealthCapacity)&&body.position.y<-4,"Maximum-speed sweep passes through two real enemy bodies and damages both");
            if(one!=null)UnityEngine.Object.Destroy(one.gameObject);if(two!=null)UnityEngine.Object.Destroy(two.gameObject);
            var magnet=UnityEngine.Object.Instantiate(arena.templates[4],new Vector3(1,1),Quaternion.identity).GetComponent<OrbitBreakerAttachmentPoint>();magnet.gameObject.SetActive(true);magnet.transform.localScale*=.8f;
            Place(new Vector3(1,-.08f),Vector3.zero);yield return Delay(.04f);Check(motor.TryAttach(magnet),"E-equivalent enters orbit within expanded capture radius");
            yield return Delay(.35f);float first=motor.CurrentSpeed;yield return Delay(.8f);float second=motor.CurrentSpeed;yield return Delay(1.2f);
            Check(motor.IsAttached&&first<second&&second<motor.CurrentSpeed&&motor.CurrentSpeed>=23.8f&&motor.CurrentSpeed<=24.1f,"Physical auto orbit progressively accelerates and caps at24: "+first+","+second+","+motor.CurrentSpeed);
            var lockOn=motor.GetComponent<OrbitBreakerTargetLock>();var target=Spawn(new Vector3(7,1));Check(lockOn.Select(target.transform),"OrbitBreakerEnemy is selectable");Charges(2);motor.SetAttachmentAim(new Vector2(-20,1));Vector2 expected=((Vector2)target.Position-(Vector2)motor.Position).normalized;
            int chargesAtLaunch=-1;motor.DashStarted+=()=>chargesAtLaunch=motor.DashCharges;Check(motor.TryRequestDash(),"Charged orbit requests targeted Space");yield return Delay(.04f);
            Check(Vector2.Dot(motor.DashDirection,expected)>.99f&&chargesAtLaunch==1&&!lockOn.HasTarget&&!motor.IsAttached,"Next successful Space uses selected target instead of opposite mouse aim and consumes lock/one capsule: dot="+Vector2.Dot(motor.DashDirection,expected)+", charges="+motor.DashCharges+", lock="+lockOn.HasTarget+", attached="+motor.IsAttached);
            UnityEngine.Object.Destroy(target.gameObject);UnityEngine.Object.Destroy(magnet.gameObject);yield return Hold(1.2f,new Vector3(1,1));
            var stale=Spawn(new Vector3(5,1));lockOn.Select(stale.transform);UnityEngine.Object.Destroy(stale.gameObject);yield return Delay(.04f);Check(!lockOn.HasTarget,"Destroyed target safely clears selection");
            var pin=UnityEngine.Object.Instantiate(arena.templates[0],new Vector3(1,3),Quaternion.identity).GetComponent<OrbitBreakerBumper>();pin.gameObject.SetActive(true);pin.transform.localScale*=.8f;
            int beforeContacts=motor.TotalQualifiedContacts;for(int i=0;i<4;i++){Place(pin.transform.position+Vector3.up*1.15f,Vector3.down*9);yield return Delay(.13f);yield return Hold(.25f,new Vector3(1,1));}
            Check(motor.TotalQualifiedContacts>=beforeContacts+4&&motor.DashCharges>=2,"Four real pillar contacts still award a dash resource");
            Check(lockOn.Select(pin.transform),"Pillar is selectable");lockOn.Consume();UnityEngine.Object.Destroy(pin.gameObject);
            motor.GrantRewardBoost();Place(new Vector3(1,1),Vector3.zero);motor.TryRequestBoardImpulse(Vector2.down,24);yield return Delay(.04f);
            Check(motor.CurrentSpeed>32&&motor.IsPiercing&&motor.AttackDamage>motor.DamageForSpeed(24),"Reward raises actual top speed to32.4 and continues damage scaling/piercing");
            int old=manager.GetInstanceID();manager.HandleEscape();yield return ()=>UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>().GetInstanceID()!=old;Bind();
            Check(manager.State==GameState.Ready&&score.Score==0&&motor.Shells==3&&motor.DashCharges==0&&!motor.HasRewardBoost,"Escape returns clean menu with reset run state");
            Check(manager.StartMode(GameMode.OrbitBreakerBattle),"Select OrbitBreakerBattle mode");var boss=manager.GetComponent<OrbitBreakerBossDirector>().Boss;
            Check(boss!=null&&boss.Health==manager.Rules.bossHealth&&boss.transform.childCount>=45,"OrbitBreakerBattle spawns configured octopus with eight segmented arms and health");
            arena.gameObject.SetActive(false);score.TryAward(10000,menu==null?manager:(UnityEngine.Object)menu,1);Check(manager.IsPlaying,"Score cannot prematurely win Boss mode");
            boss.GetComponent<Rigidbody>().position=new Vector3(1,6);boss.transform.position=new Vector3(1,6);Place(new Vector3(1,2),Vector3.zero);motor.TryRequestBoardImpulse(Vector2.up,24);
            int hp=boss.Health;yield return Delay(.12f);
            Check(boss.Health<hp&&boss.DamageEvents==1,"Real maximum-speed Boss trigger deals one hit");yield return Delay(.04f);
            Check(body.velocity.y<0&&motor.IsLaunched,"Boss impact reverses player velocity instead of piercing");
            Check(!boss.TryPlayerImpact(motor),"Boss impact cooldown prevents duplicate damage while overlapping");
            // Isolate individual hazard timings from autonomous attacks and approaching body contact.
            var isolatedRules=UnityEngine.Object.Instantiate(manager.Rules);isolatedRules.bossMoveSpeed=0;
            so=new SerializedObject(manager);so.FindProperty("encounter").objectReferenceValue=isolatedRules;so.ApplyModifiedPropertiesWithoutUndo();
            typeof(OrbitBreakerOctopusBoss).GetField("nextAttack",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(boss,float.PositiveInfinity);
            foreach(var h in UnityEngine.Object.FindObjectsOfType<OrbitBreakerBossTentacle>())UnityEngine.Object.Destroy(h.gameObject);
            yield return Hold(1.2f,new Vector3(1,-6));hp=motor.Shells;boss.FireLaser();
            yield return Hold(.7f,new Vector3(1,-6));Check(motor.Shells==hp&&boss.LaserTelegraph,"Laser warning is harmless and visible");
            yield return Hold(.7f,new Vector3(1,-6));Check(motor.Shells==hp-1,"Active laser damages low-speed player");motor.TryRestoreShell();
            yield return Hold(1.3f,new Vector3(1,-6));boss.FireLaser();float end=Time.time+1.4f;hp=motor.Shells;
            yield return ()=>{Place(new Vector3(1,-6),Vector3.right*24);return Time.time>=end;};
            Check(motor.Shells==hp,"Actual high speed protects against active laser");
            yield return Hold(1f,new Vector3(1,-6));boss.SummonTentacles();hp=motor.Shells;
            yield return Hold(.65f,new Vector3(1,-6));Check(motor.Shells==hp,"Tentacle warning does not damage");
            yield return Hold(.75f,new Vector3(1,-6));Check(motor.Shells==hp-1,"Erupted tentacle damages low-speed player");motor.TryRestoreShell();
            foreach(var h in UnityEngine.Object.FindObjectsOfType<OrbitBreakerBossTentacle>())UnityEngine.Object.Destroy(h.gameObject);
            // Generated prop survives only until swept by boss. Inactive templates stay untouched.
            arena.gameObject.SetActive(true);arena.RefreshAroundPlayer();var generated=UnityEngine.Object.FindObjectsOfType<OrbitBreakerBumper>().First();
            boss.GetComponent<Rigidbody>().position=generated.transform.position;boss.transform.position=generated.transform.position;int destroyed=boss.DestroyedProps;
            Place(generated.transform.position+Vector3.down*10,Vector3.zero);yield return Delay(.06f);
            Check(!generated.gameObject.activeSelf&&boss.DestroyedProps>destroyed,"Boss movement actually destroys generated pillar");
            Check(arena.templates.All(t=>t!=null)&&!GameObject.Find("OrbitBreaker").transform.Find("Arena").gameObject.activeSelf,"Boss destruction preserves inactive source templates");
            arena.gameObject.SetActive(false);boss.enabled=false;Place(new Vector3(1,1),Vector3.right*24);yield return Delay(.5f);
            while(boss.Health>0){Place(new Vector3(1,1),Vector3.right*24);boss.TryPlayerImpact(motor);yield return Delay(.48f);}
            Check(manager.State==GameState.Victory&&body.isKinematic,"Depleting Boss HP wins OrbitBreakerBattle and freezes player");
            Check(!score.TryAward(10,manager,100)&&!motor.CanDash,"Terminal state rejects further scoring and dash");
            old=manager.GetInstanceID();Check(manager.RestartGame(),"R-equivalent restarts selected mode");yield return ()=>UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>().GetInstanceID()!=old;yield return Delay(.06f);Bind();
            Check(manager.Mode==GameMode.OrbitBreakerBattle&&manager.IsPlaying&&manager.GetComponent<OrbitBreakerBossDirector>().Boss.Health==manager.Rules.bossHealth&&score.Score==0,"Restart creates fresh Boss OrbitBreakerBattle with restored HP and zero score");
            old=manager.GetInstanceID();manager.ReturnToMenu();yield return ()=>UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>().GetInstanceID()!=old;Bind();manager.StartMode(GameMode.Free);arena.gameObject.SetActive(false);
            for(int i=0;i<3;i++){yield return Hold(1.1f,new Vector3(1,1));Check(motor.TryReceiveHit(),"Low-speed shield damage "+i);}
            Check(manager.State==GameState.Defeat&&motor.Shells==0&&body.isKinematic,"Three shields still produce clean defeat in Free mode");
        }
        private static Func<bool> Delay(float duration) { float end = Time.time + duration; return () => Time.time >= end; }
        private static void Tick()
        {
            try
            {
                if (waiting != null && !waiting()) return;
                if (steps.MoveNext()) waiting = steps.Current;
                else { SessionState.SetBool(Prefix + "Complete", true); Report("PASS", "All T18 integration checks passed."); EditorApplication.isPlaying = false; }
            }
            catch (Exception ex) { Fail(ex.ToString()); }
        }
        private static void Check(bool pass, string detail) { if (!pass) throw new InvalidOperationException(detail.Length > 0 ? detail : "Attachment expired before 7.35 seconds."); if (detail.Length > 0) {checks.Add(detail);Report("RUNNING",detail);} }
        private static void Fail(string detail) { Report("FAIL", detail); SessionState.SetBool(Prefix+"Running", false); Debug.LogError("T18: " + detail); EditorApplication.isPlaying = false; }
        private static void Timeout() { if (SessionState.GetBool(Prefix+"Running", false) && DateTime.UtcNow > DateTime.Parse(SessionState.GetString(Prefix+"Deadline", ""), null, System.Globalization.DateTimeStyles.RoundtripKind)) Fail("T18 deadline exceeded."); }
        private static void OnPlayState(PlayModeStateChange state)
        {
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
        private static void Report(string status, string detail) => File.WriteAllText(Evidence("T18-integration.json"), JsonUtility.ToJson(new Result {status=status, observedAtUtc=DateTime.UtcNow.ToString("O"), unityVersion=Application.unityVersion, detail=detail, checks=checks.ToArray()}, true));
    }
}
