using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OrbitBreaker.Editor
{
    // Reproducible acceptance checks without adding test packages or runtime components.
    [InitializeOnLoad]
    public static class OrbitBreakerT01Validation
    {
        private const string PhaseKey = "OrbitBreaker.T01.Phase";
        private const string InstanceKey = "OrbitBreaker.T01.PreviousManager";
        private const string DeadlineKey = "OrbitBreaker.T01.Deadline";
        private const string ReportPath = "Docs/EVIDENCE/T01-validation.json";

        static OrbitBreakerT01Validation()
        {
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        [MenuItem("Tools/Orbit Breaker/Validate T01")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || SessionState.GetInt(PhaseKey, 0) != 0)
                throw new InvalidOperationException("Run T01 validation from idle Edit mode.");
            try
            {
                ValidateFoundation();
                WriteReport("RUNNING", "Edit checks passed; entering Play mode for state/reload checks.");
                SessionState.SetInt(PhaseKey, 1);
                SessionState.SetString(DeadlineKey, DateTime.UtcNow.AddSeconds(90).ToString("O"));
                EditorApplication.isPlaying = true;
            }
            catch (Exception exception)
            {
                Fail(exception);
            }
        }

        private static void ValidateFoundation()
        {
            var scene = OrbitBreakerSceneBuilder.Build();
            var root = scene.GetRootGameObjects().Single(go => go.name == OrbitBreakerSceneBuilder.RootName);
            Check(root.GetComponentsInChildren<OrbitBreakerGameManager>(true).Length == 1, "Exactly one OrbitBreakerGameManager");
            var camera = root.GetComponentsInChildren<Camera>(true).Single();
            Check(!camera.orthographic && camera.transform.position.z < 0f && camera.transform.position.y < -10f &&
                  camera.transform.forward.y > 0.4f && camera.transform.forward.z > 0.4f, "Perspective camera views table obliquely from drain end");
            Check(root.GetComponentsInChildren<Light>(true).Single().type == LightType.Directional, "Directional light");
            Check(camera.GetComponent<AudioListener>() != null &&
                  root.GetComponentsInChildren<AudioListener>(true).Length == 1, "One camera AudioListener");
            var config = AssetDatabase.LoadAssetAtPath<OrbitBreakerCombatConfig>(OrbitBreakerSceneBuilder.ConfigPath);
            Check(config != null && root.GetComponentInChildren<OrbitBreakerGameManager>().Config == config, "Persisted config reference");
            Check(typeof(OrbitBreakerCombatConfig).GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Length == 8,
                "Exactly eight tuning fields");
            var configGuid = AssetDatabase.AssetPathToGUID(OrbitBreakerSceneBuilder.ConfigPath);
            var sceneGuid = AssetDatabase.AssetPathToGUID(OrbitBreakerSceneBuilder.ScenePath);
            float originalGravity = config.gravityStrength;
            var probe = new GameObject("T01_ValidationProbe_" + Guid.NewGuid().ToString("N"));
            probe.transform.SetParent(root.transform, false);
            try
            {
                config.gravityStrength = originalGravity + 1f;
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssetIfDirty(config);
                EditorSceneManager.MarkSceneDirty(scene);
                Check(EditorSceneManager.SaveScene(scene), "Save custom scene child");
                var before = ObjectIds(scene);
                OrbitBreakerSceneBuilder.Build();
                Check(before.SequenceEqual(ObjectIds(scene)), "Repeat build preserves every object identity, including custom child");
                Check(config.gravityStrength == originalGravity + 1f, "Repeat build preserves tuned config");
                Check(configGuid == AssetDatabase.AssetPathToGUID(OrbitBreakerSceneBuilder.ConfigPath), "Config GUID stable");
                Check(sceneGuid == AssetDatabase.AssetPathToGUID(OrbitBreakerSceneBuilder.ScenePath), "Scene GUID stable");
                Check(EditorBuildSettings.scenes.Count(item => item.path == OrbitBreakerSceneBuilder.ScenePath && item.enabled) == 1,
                    "One enabled OrbitBreakerBattle entry in Build Settings");
            }
            finally
            {
                config.gravityStrength = originalGravity;
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssetIfDirty(config);
                if (probe != null) UnityEngine.Object.DestroyImmediate(probe);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            scene = EditorSceneManager.OpenScene(OrbitBreakerSceneBuilder.ScenePath, OpenSceneMode.Single);
            var manager = scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<OrbitBreakerGameManager>(true)).Single();
            Check(manager.Config != null && manager.Config.gravityStrength == originalGravity, "Save/reopen retains config and restored tuning");
            Check(scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<AudioListener>(true)).Count() == 1,
                "AudioListener survives save/reopen");
            Check(!scene.isDirty, "Reopened OrbitBreakerBattle is saved");
        }

        private static int[] ObjectIds(Scene scene)
        {
            return scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<Transform>(true))
                .Select(item => item.GetInstanceID()).OrderBy(id => id).ToArray();
        }

        private static void Tick()
        {
            int phase = SessionState.GetInt(PhaseKey, 0);
            if (phase == 0 || phase >= 4) return;
            try
            {
                if (DateTime.UtcNow > DateTime.Parse(SessionState.GetString(DeadlineKey, ""), null,
                        System.Globalization.DateTimeStyles.RoundtripKind))
                    throw new TimeoutException("Timed out waiting for Play mode/reload.");
                if (!EditorApplication.isPlaying || EditorApplication.isCompiling) return;
                var manager = UnityEngine.Object.FindObjectOfType<OrbitBreakerGameManager>();
                if (manager == null) return;
                if (phase > 1 && manager.GetInstanceID() == SessionState.GetInt(InstanceKey, 0)) return;
                Check(manager.Config != null && manager.State == GameState.Ready, "New scene starts Ready with config");

                if (phase == 1)
                {
                    int notifications = 0;
                    manager.StateChanged += state => notifications++;
                    Check(!manager.WinGame() && !manager.LoseGame() && !manager.RestartGame(), "Ready rejects ending/restart");
                    Check(manager.StartGame() && !manager.StartGame(), "Start accepted exactly once");
                    Check(manager.IsPlaying && !manager.RestartGame(), "Playing does not allow debug restart");
                    Check(manager.WinGame() && manager.State == GameState.Victory, "Victory transition");
                    Check(!manager.LoseGame() && !manager.WinGame() && !manager.StartGame(), "Victory is terminal");
                    Check(notifications == 2, "Only real transitions emit events");
                    SessionState.SetInt(InstanceKey, manager.GetInstanceID());
                    SessionState.SetInt(PhaseKey, 2);
                    Check(manager.RestartGame(), "Victory reload requested");
                }
                else if (phase == 2)
                {
                    Check(manager.StartGame() && manager.LoseGame() && manager.State == GameState.Defeat, "Defeat transition after real reload");
                    Check(!manager.WinGame() && !manager.LoseGame() && !manager.StartGame(), "Defeat is terminal");
                    SessionState.SetInt(InstanceKey, manager.GetInstanceID());
                    SessionState.SetInt(PhaseKey, 3);
                    Check(manager.RestartGame(), "Defeat reload requested");
                }
                else
                {
                    SessionState.SetInt(PhaseKey, 4);
                    EditorApplication.isPlaying = false;
                }
            }
            catch (Exception exception)
            {
                Fail(exception);
            }
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode) return;
            int phase = SessionState.GetInt(PhaseKey, 0);
            if (phase == 4)
            {
                SessionState.SetInt(PhaseKey, 0);
                WriteReport("PASS", "Build/rebuild/reopen, tuning and custom-child preservation, state guards/events, Victory and Defeat scene reloads passed. Keyboard input and gameplay were not simulated.");
                Debug.Log("[OrbitBreaker] T01 validation PASS. See " + ReportPath);
            }
            else if (phase != 0)
            {
                Fail(new InvalidOperationException("Play mode ended before validation completed."));
            }
        }

        private static void Check(bool condition, string description)
        {
            if (!condition) throw new InvalidOperationException("T01 check failed: " + description);
        }

        private static void Fail(Exception exception)
        {
            SessionState.SetInt(PhaseKey, 0);
            WriteReport("FAIL", exception.ToString());
            Debug.LogError("[OrbitBreaker] T01 validation failed: " + exception);
            if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
        }

        [Serializable]
        private sealed class Report
        {
            public string status;
            public string observedAtUtc;
            public string unityVersion;
            public string detail;
        }

        private static void WriteReport(string status, string detail)
        {
            var path = Path.Combine(Path.GetDirectoryName(Application.dataPath), ReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(new Report
            {
                status = status, observedAtUtc = DateTime.UtcNow.ToString("O"),
                unityVersion = Application.unityVersion, detail = detail
            }, true));
        }
    }
}
