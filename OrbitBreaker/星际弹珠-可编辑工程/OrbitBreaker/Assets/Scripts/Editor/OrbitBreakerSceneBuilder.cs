using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OrbitBreaker.Editor
{
    public static class OrbitBreakerSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/OrbitBreakerBattle.unity";
        public const string ConfigPath = "Assets/Config/OrbitBreakerCombatConfig.asset";
        public const string MaterialPath = "Assets/Materials/OrbitBreakerBattleBackdrop.mat";
        public const string RootName = "OrbitBreaker";
        public const string BuildMenu = "Tools/Orbit Breaker/Build Demo Scene";

        [MenuItem(BuildMenu)]
        public static void BuildDemoScene()
        {
            Build();
            Debug.Log("[OrbitBreaker] OrbitBreakerBattle saved. Enter: start; WASD: move; Space: dash; E: attach; three shells; random roaming arena; R: restart after victory/defeat.");
        }

        public static Scene Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Build Demo Scene is available only in Edit mode.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Save your modified scenes before running Build Demo Scene.");

            EnsureFolder("Assets/Scenes");
            EnsureFolder("Assets/Config");
            EnsureFolder("Assets/Materials");
            var config = AssetDatabase.LoadAssetAtPath<OrbitBreakerCombatConfig>(ConfigPath);
            if (config == null)
            {
                EnsureAssetPathEmpty(ConfigPath);
                config = ScriptableObject.CreateInstance<OrbitBreakerCombatConfig>();
                AssetDatabase.CreateAsset(config, ConfigPath);
            }
            // Reuse the asset unchanged: rebuilding must never reset tuning values.
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                EnsureAssetPathEmpty(MaterialPath);
                var shader = Shader.Find("Standard");
                if (shader == null)
                    throw new InvalidOperationException("Built-In Standard shader was not found.");
                material = new Material(shader) { name = "OrbitBreakerBattleBackdrop", color = new Color(0.055f, 0.095f, 0.16f) };
                material.SetFloat("_Glossiness", 0.15f);
                AssetDatabase.CreateAsset(material, MaterialPath);
            }

            var scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
                scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null
                    ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
                    : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SceneManager.SetActiveScene(scene);

            var roots = scene.GetRootGameObjects().Where(go => go.name == RootName).ToArray();
            if (roots.Length > 1)
                throw new InvalidOperationException("Multiple OrbitBreaker roots found; resolve them before rebuilding.");
            var root = roots.Length == 1 ? roots[0] : new GameObject(RootName);
            if (root.scene != scene)
                SceneManager.MoveGameObjectToScene(root, scene);

            if (root.transform.Find("OrbitBreakerProceduralArena") != null)
            {
                if(OrbitBreakerCombatPolishLayout.Apply(root,config) | OrbitBreakerModeSceneLayout.Apply(root))
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene,ScenePath);
                }
                return scene;
            }

            var systems = Child(root.transform, "Systems");
            var manager = Ensure<OrbitBreakerGameManager>(systems);
            var serialized = new SerializedObject(manager);
            serialized.FindProperty("config").objectReferenceValue = config;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var cameraObject = Child(root.transform, "Main Camera");
            var camera = cameraObject.GetComponent<Camera>();
            if (camera == null)
            {
                camera = cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
                ConfigureTableCamera(camera);
                camera.nearClipPlane = 0.1f;
                camera.farClipPlane = 60f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.018f, 0.027f, 0.05f);
            }
            Ensure<AudioListener>(cameraObject);

            var lightObject = Child(root.transform, "Directional Light");
            if (lightObject.GetComponent<Light>() == null)
            {
                var light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.2f;
                lightObject.transform.localRotation = Quaternion.Euler(35f, -25f, 0f);
            }

            var arena = Child(root.transform, "Arena");
            if (FindChild(arena.transform, "Backdrop") == null)
            {
                var backdrop = GameObject.CreatePrimitive(PrimitiveType.Cube);
                backdrop.name = "Backdrop";
                backdrop.transform.SetParent(arena.transform, false);
                backdrop.transform.localPosition = new Vector3(0f, 0f, 1.5f);
                backdrop.transform.localScale = new Vector3(12f, 20f, 0.2f);
                backdrop.GetComponent<MeshRenderer>().sharedMaterial = material;
                UnityEngine.Object.DestroyImmediate(backdrop.GetComponent<Collider>());
            }
            if (FindChild(root.transform, "PlayerSpawn") == null)
                Child(root.transform, "PlayerSpawn").transform.localPosition = new Vector3(0f, -5f, 0f);

            BuildPlayerAndArena(root, arena, manager);
            BuildTablePresentation(root, arena, camera);
            BuildBoardDevices(root, arena);
            BuildScoring(systems, arena, manager);
            BuildEnemies(root, systems, arena);
            WidenTableOnce(root, arena);
            BuildHazards(arena);
            BuildAttachmentPoints(arena);
            OrbitBreakerCosmicSceneBuilder.Apply(root, arena, camera, config);
            OrbitBreakerCosmicLayoutRefinement.Apply(root, arena, camera);
            OrbitBreakerDenseRallyLayout.Apply(root, arena, config);
            OrbitBreakerEndlessArenaLayout.Apply(root);
            OrbitBreakerCombatPolishLayout.Apply(root,config);
            OrbitBreakerModeSceneLayout.Apply(root);

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new InvalidOperationException("Could not save OrbitBreakerBattle scene.");
            AssetDatabase.SaveAssetIfDirty(config);
            AssetDatabase.SaveAssetIfDirty(material);

            var buildScenes = EditorBuildSettings.scenes.ToList();
            var entry = buildScenes.FirstOrDefault(item => item.path == ScenePath);
            if (entry == null)
                buildScenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            else
                entry.enabled = true;
            EditorBuildSettings.scenes = buildScenes.ToArray();
            return scene;
        }

        private static T Ensure<T>(GameObject target) where T : Component
        {
            // Unity's missing native components can be fake-null wrappers in the Editor.
            var component = target.GetComponent<T>();
            return component == null ? target.AddComponent<T>() : component;
        }

        private static void ConfigureTableCamera(Camera camera)
        {
            // View from the drain end, like standing in front of a physical pinball cabinet.
            // XY remains the internal board coordinate system; depth is visible through perspective.
            var position = new Vector3(0f, -20f, -16f);
            var target = new Vector3(0f, -2.5f, 0f);
            camera.transform.localPosition = position;
            camera.transform.localRotation = Quaternion.LookRotation(target - position, Vector3.up);
            camera.orthographic = false;
            camera.fieldOfView = 42f;
        }

        private static void BuildTablePresentation(GameObject root, GameObject arena, Camera camera)
        {
            // One-time migration of the old front-on view. Subsequent rebuilds preserve adjustments.
            if (FindChild(root.transform, "TablePresentation") != null) return;
            var presentation = Child(root.transform, "TablePresentation");
            ConfigureTableCamera(camera);
            var felt = ColorMaterial("Assets/Materials/OrbitBreakerTableFelt.mat", new Color(0.055f, 0.22f, 0.19f));
            var cabinet = ColorMaterial("Assets/Materials/OrbitBreakerTableCabinet.mat", new Color(0.035f, 0.045f, 0.055f));
            var trim = ColorMaterial("Assets/Materials/OrbitBreakerTableTrim.mat", new Color(0.65f, 0.42f, 0.16f));
            trim.SetFloat("_Metallic", 0.65f);
            trim.SetFloat("_Glossiness", 0.5f);
            felt.SetFloat("_Glossiness", 0.12f);
            var backdrop = FindChild(arena.transform, "Backdrop");
            // Front face z=.5 is tangent to the radius-.5 ball at z=0: no floating gap.
            backdrop.localPosition = new Vector3(0f, 0f, 1.1f);
            backdrop.localScale = new Vector3(13f, 21f, 1.2f);
            backdrop.GetComponent<MeshRenderer>().sharedMaterial = felt;
            VisualCube(presentation.transform, "Cabinet", new Vector3(0f, -0.5f, 2f), new Vector3(14f, 22f, 1.2f), cabinet);
            VisualCube(presentation.transform, "LeftTrim", new Vector3(-6.65f, 0f, 0.1f), new Vector3(0.3f, 21.5f, 1.4f), trim);
            VisualCube(presentation.transform, "RightTrim", new Vector3(6.65f, 0f, 0.1f), new Vector3(0.3f, 21.5f, 1.4f), trim);
            VisualCube(presentation.transform, "FarTrim", new Vector3(0f, 10.7f, 0.1f), new Vector3(13.6f, 0.3f, 1.4f), trim);
            VisualCube(presentation.transform, "FrontApron", new Vector3(0f, -11.2f, 1f), new Vector3(14f, 1.5f, 1.8f), cabinet);
            VisualCube(presentation.transform, "LeftInlay", new Vector3(-5.25f, 0f, 0.47f), new Vector3(0.035f, 18.5f, 0.02f), trim);
            VisualCube(presentation.transform, "RightInlay", new Vector3(5.25f, 0f, 0.47f), new Vector3(0.035f, 18.5f, 0.02f), trim);
            // The trigger is gameplay geometry; the cabinet opening now supplies its visual cue.
            FindChild(arena.transform, "OrbitBreakerKillZone").GetComponent<MeshRenderer>().enabled = false;
            var light = FindChild(root.transform, "Directional Light").GetComponent<Light>();
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.85f;
            light.transform.localRotation = Quaternion.Euler(25f, -30f, 0f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.38f, 0.42f, 0.48f);
            AssetDatabase.SaveAssetIfDirty(felt);
            AssetDatabase.SaveAssetIfDirty(trim);
        }

        private static void VisualCube(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = position;
            cube.transform.localScale = scale;
            cube.GetComponent<MeshRenderer>().sharedMaterial = material;
            UnityEngine.Object.DestroyImmediate(cube.GetComponent<Collider>());
        }

        private static void BuildBoardDevices(GameObject root, GameObject arena)
        {
            var player = FindChild(root.transform, "Player").GetComponent<OrbitBreakerPlayerMotor>();
            var devices = Child(arena.transform, "Devices");
            var paddleMaterial = ColorMaterial("Assets/Materials/OrbitBreakerFlipperGold.mat", new Color(1f, 0.7f, 0.18f));
            var areaMaterial = ColorMaterial("Assets/Materials/OrbitBreakerFlipperZone.mat", new Color(0.16f, 0.36f, 0.29f));
            var bumperMaterial = ColorMaterial("Assets/Materials/OrbitBreakerBumperPurple.mat", new Color(0.65f, 0.22f, 0.9f));
            for (int i = 0; i < 2; i++)
            {
                bool left = i == 0;
                string name = left ? "LeftFlipper" : "RightFlipper";
                if (FindChild(devices.transform, name) == null)
                {
                    var flipper = Child(devices.transform, name);
                    flipper.transform.localPosition = new Vector3(left ? -2.8f : 2.8f, -8.6f, 0f);
                    var zone = flipper.AddComponent<BoxCollider>();
                    zone.isTrigger = true;
                    zone.size = new Vector3(3f, 1.8f, 1.5f);
                    VisualCube(flipper.transform, "EffectiveArea", new Vector3(0f, 0f, 0.47f), new Vector3(3f, 1.8f, 0.02f), areaMaterial);
                    var pivot = Child(flipper.transform, "PaddlePivot");
                    pivot.transform.localPosition = new Vector3(left ? -1.35f : 1.35f, -0.3f, 0.25f);
                    pivot.transform.localRotation = Quaternion.Euler(0f, 0f, left ? -12f : 12f);
                    VisualCube(pivot.transform, "Paddle", new Vector3(left ? 1.35f : -1.35f, 0f, 0f), new Vector3(2.7f, 0.3f, 0.5f), paddleMaterial);
                    var controller = flipper.AddComponent<OrbitBreakerFlipperController>();
                    var serialized = new SerializedObject(controller);
                    serialized.FindProperty("player").objectReferenceValue = player;
                    serialized.FindProperty("side").enumValueIndex = i;
                    serialized.FindProperty("paddle").objectReferenceValue = pivot.transform;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }

                name = left ? "LeftBumper" : "RightBumper";
                if (FindChild(devices.transform, name) != null) continue;
                var bumper = Child(devices.transform, name);
                bumper.transform.localPosition = new Vector3(left ? -3.1f : 3.1f, 6.5f, 0f);
                var collider = bumper.AddComponent<CapsuleCollider>();
                collider.direction = 2;
                collider.radius = 0.8f;
                collider.height = 2f;
                collider.sharedMaterial = AssetDatabase.LoadAssetAtPath<PhysicMaterial>("Assets/Materials/OrbitBreakerArenaSurface.physicMaterial");
                var visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                visual.name = "Visual";
                visual.transform.SetParent(bumper.transform, false);
                visual.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                visual.transform.localScale = new Vector3(1.6f, 0.7f, 1.6f);
                visual.GetComponent<MeshRenderer>().sharedMaterial = bumperMaterial;
                UnityEngine.Object.DestroyImmediate(visual.GetComponent<Collider>());
                var component = bumper.AddComponent<OrbitBreakerBumper>();
                var bumperSerialized = new SerializedObject(component);
                bumperSerialized.FindProperty("visual").objectReferenceValue = visual.transform;
                bumperSerialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void BuildEnemies(GameObject root, GameObject systems, GameObject arena)
        {
            EnsureFolder("Assets/Prefabs");
            const string prefabPath = "Assets/Prefabs/OrbitBreakerEnemy.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                EnsureAssetPathEmpty(prefabPath);
                var temporary = GameObject.CreatePrimitive(PrimitiveType.Cube);
                try
                {
                    temporary.name = "OrbitBreakerEnemy";
                    temporary.transform.localScale = Vector3.one * 0.8f;
                    temporary.GetComponent<Renderer>().sharedMaterial = ColorMaterial("Assets/Materials/OrbitBreakerEnemyRed.mat", new Color(0.9f, 0.055f, 0.06f));
                    temporary.GetComponent<BoxCollider>().sharedMaterial = AssetDatabase.LoadAssetAtPath<PhysicMaterial>("Assets/Materials/OrbitBreakerArenaSurface.physicMaterial");
                    var body = temporary.AddComponent<Rigidbody>();
                    body.useGravity = false;
                    body.isKinematic = true;
                    body.constraints = OrbitBreakerPlayerMotor.PlaneConstraints;
                    temporary.AddComponent<OrbitBreakerEnemyController>();
                    prefab = PrefabUtility.SaveAsPrefabAsset(temporary, prefabPath);
                }
                finally { UnityEngine.Object.DestroyImmediate(temporary); }
            }
            var points = Child(arena.transform, "EnemySpawnPoints");
            var positions = new[] { new Vector3(-5f, -3f, 0f), new Vector3(5f, -3f, 0f),
                new Vector3(-5f, 2f, 0f), new Vector3(5f, 2f, 0f), new Vector3(-5f, 7f, 0f),
                new Vector3(5f, 7f, 0f), new Vector3(-1.7f, 9f, 0f), new Vector3(1.7f, 9f, 0f) };
            var markers = new Transform[positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                string name = "Spawn" + i;
                var existing = FindChild(points.transform, name);
                markers[i] = existing == null ? Child(points.transform, name).transform : existing;
                if (existing == null) markers[i].localPosition = positions[i];
            }
            var live = Child(arena.transform, "Enemies");
            if (systems.GetComponent<OrbitBreakerEnemySpawner>() != null) return;
            var spawner = systems.AddComponent<OrbitBreakerEnemySpawner>();
            var serialized = new SerializedObject(spawner);
            serialized.FindProperty("player").objectReferenceValue = FindChild(root.transform, "Player").GetComponent<OrbitBreakerPlayerMotor>();
            serialized.FindProperty("scoreManager").objectReferenceValue = systems.GetComponent<OrbitBreakerScoreManager>();
            serialized.FindProperty("enemyPrefab").objectReferenceValue = prefab.GetComponent<OrbitBreakerEnemyController>();
            serialized.FindProperty("liveRoot").objectReferenceValue = live.transform;
            var array = serialized.FindProperty("spawnPoints");
            array.arraySize = markers.Length;
            for (int i = 0; i < markers.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = markers[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WidenTableOnce(GameObject root, GameObject arena)
        {
            // User-requested 25% wider playable surface. Never scale the player or all children.
            // The marker makes subsequent builds preserve tuned transforms and custom children.
            if (FindChild(root.transform, "WideBoardLayout") != null) return;
            var walls = FindChild(arena.transform, "Walls");
            foreach (string name in new[] { "LeftWall", "RightWall" }) MoveOutward(FindChild(walls, name), 1.5f);
            Widen(FindChild(walls, "TopWall"), 3f);
            foreach (string name in new[] { "LeftGuard", "RightGuard" })
            {
                var guard = FindChild(walls, name);
                MoveOutward(guard, 0.75f);
                Widen(guard, 1.5f); // Inner ends stay at +/-2: drain opening remains four units.
            }
            Widen(FindChild(arena.transform, "Backdrop"), 3f);
            Widen(FindChild(arena.transform, "OrbitBreakerKillZone"), 3f);
            var presentation = FindChild(root.transform, "TablePresentation");
            foreach (string name in new[] { "Cabinet", "FarTrim", "FrontApron" }) Widen(FindChild(presentation, name), 3f);
            foreach (string name in new[] { "LeftTrim", "RightTrim", "LeftInlay", "RightInlay" })
                MoveOutward(FindChild(presentation, name), 1.5f);
            var points = FindChild(arena.transform, "EnemySpawnPoints");
            foreach (Transform point in points)
                if (Mathf.Abs(point.localPosition.x) > 4f) MoveOutward(point, 1.5f);
            Child(root.transform, "WideBoardLayout");
        }

        private static void MoveOutward(Transform target, float amount)
        {
            var position = target.localPosition;
            position.x += Mathf.Sign(position.x) * amount;
            target.localPosition = position;
        }
        private static void Widen(Transform target, float amount)
        {
            var scale = target.localScale;
            scale.x += amount;
            target.localScale = scale;
        }

        private static void BuildHazards(GameObject arena)
        {
            var hazards = Child(arena.transform, "Hazards");
            var yellow = ColorMaterial("Assets/Materials/OrbitBreakerHazardYellow.mat", new Color(1f, 0.82f, 0.03f));
            var black = ColorMaterial("Assets/Materials/OrbitBreakerHazardStripe.mat", new Color(0.035f, 0.035f, 0.035f));
            var surface = AssetDatabase.LoadAssetAtPath<PhysicMaterial>("Assets/Materials/OrbitBreakerArenaSurface.physicMaterial");
            for (int i = 0; i < 2; i++)
            {
                string name = i == 0 ? "LeftHazard" : "RightHazard";
                if (FindChild(hazards.transform, name) != null) continue;
                var hazard = ArenaCube(hazards.transform, name, new Vector3(i == 0 ? -5.5f : 5.5f, 0.5f, 0f),
                    new Vector3(1.2f, 1.2f, 1f), yellow, surface);
                hazard.AddComponent<OrbitBreakerHazard>();
                for (int stripe = 0; stripe < 3; stripe++)
                {
                    string stripeName = "Stripe" + stripe;
                    VisualCube(hazard.transform, stripeName, new Vector3((stripe - 1) * 0.28f, 0f, -0.51f),
                        new Vector3(0.12f, 0.86f, 0.02f), black);
                    FindChild(hazard.transform, stripeName).localRotation = Quaternion.Euler(0f, 0f, -25f);
                }
            }
        }

        private static void BuildAttachmentPoints(GameObject arena)
        {
            var points = Child(arena.transform, "AttachmentPoints");
            var green = ColorMaterial("Assets/Materials/OrbitBreakerMagnetGreen.mat", new Color(0.12f, 1f, 0.65f));
            var surface = AssetDatabase.LoadAssetAtPath<PhysicMaterial>("Assets/Materials/OrbitBreakerArenaSurface.physicMaterial");
            for (int i = 0; i < 2; i++)
            {
                string name = i == 0 ? "LeftMagnet" : "RightMagnet";
                if (FindChild(points.transform, name) != null) continue;
                var point = Child(points.transform, name);
                point.transform.localPosition = new Vector3(i == 0 ? -3f : 3f, -3.5f, 0f);
                point.AddComponent<OrbitBreakerAttachmentPoint>();
                var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = "MagneticPost";
                post.transform.SetParent(point.transform, false);
                post.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                post.transform.localScale = new Vector3(0.75f, 0.4f, 0.75f);
                post.GetComponent<Renderer>().sharedMaterial = green;
                post.GetComponent<Collider>().sharedMaterial = surface;
                var ring = Child(point.transform, "OrbitGuide").AddComponent<LineRenderer>();
                ring.useWorldSpace = false;
                ring.loop = true;
                ring.positionCount = 64;
                ring.widthMultiplier = 0.045f;
                ring.sharedMaterial = green;
                ring.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                ring.receiveShadows = false;
                for (int k = 0; k < 64; k++)
                {
                    float angle = k * Mathf.PI * 2f / 64f;
                    ring.SetPosition(k, new Vector3(Mathf.Cos(angle) * OrbitBreakerAttachmentPoint.OrbitRadius,
                        Mathf.Sin(angle) * OrbitBreakerAttachmentPoint.OrbitRadius, 0.38f));
                }
            }
        }

        private static void BuildScoring(GameObject systems, GameObject arena, OrbitBreakerGameManager manager)
        {
            var score = Ensure<OrbitBreakerScoreManager>(systems);
            var serialized = new SerializedObject(score);
            serialized.FindProperty("gameManager").objectReferenceValue = manager;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var display = Ensure<OrbitBreakerScoreDisplay>(systems);
            serialized = new SerializedObject(display);
            serialized.FindProperty("scoreManager").objectReferenceValue = score;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var targets = Child(arena.transform, "ScoreTargets");
            var normal = ColorMaterial("Assets/Materials/OrbitBreakerScoreTargetOrange.mat", new Color(1f, 0.35f, 0.06f));
            var high = ColorMaterial("Assets/Materials/OrbitBreakerScoreTargetGold.mat", new Color(1f, 0.85f, 0.15f));
            var surface = AssetDatabase.LoadAssetAtPath<PhysicMaterial>("Assets/Materials/OrbitBreakerArenaSurface.physicMaterial");
            for (int i = 0; i < 3; i++)
            {
                string name = i == 0 ? "LeftTarget" : i == 1 ? "RightTarget" : "HighTarget";
                if (FindChild(targets.transform, name) != null) continue;
                var position = i == 2 ? new Vector3(0f, 7.8f, 0f) : new Vector3(i == 0 ? -3.6f : 3.6f, 3.8f, 0f);
                var target = ArenaCube(targets.transform, name, position, new Vector3(1.8f, 0.5f, 1f),
                    i == 2 ? high : normal, surface);
                var component = target.AddComponent<OrbitBreakerScoreTarget>();
                serialized = new SerializedObject(component);
                serialized.FindProperty("scoreManager").objectReferenceValue = score;
                serialized.FindProperty("highValue").boolValue = i == 2;
                serialized.FindProperty("targetRenderer").objectReferenceValue = target.GetComponent<Renderer>();
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void BuildPlayerAndArena(GameObject root, GameObject arena, OrbitBreakerGameManager manager)
        {
            var playerMaterial = ColorMaterial("Assets/Materials/OrbitBreakerPlayerBlue.mat", new Color(0.06f, 0.4f, 1f));
            var wallMaterial = ColorMaterial("Assets/Materials/OrbitBreakerArenaWall.mat", new Color(0.3f, 0.55f, 0.7f));
            var dangerMaterial = ColorMaterial("Assets/Materials/OrbitBreakerKillZoneRed.mat", new Color(0.65f, 0.08f, 0.1f));
            var surface = AssetDatabase.LoadAssetAtPath<PhysicMaterial>("Assets/Materials/OrbitBreakerArenaSurface.physicMaterial");
            if (surface == null)
            {
                EnsureAssetPathEmpty("Assets/Materials/OrbitBreakerArenaSurface.physicMaterial");
                surface = new PhysicMaterial("OrbitBreakerArenaSurface")
                {
                    dynamicFriction = 0f, staticFriction = 0f, bounciness = 0f,
                    frictionCombine = PhysicMaterialCombine.Minimum, bounceCombine = PhysicMaterialCombine.Minimum
                };
                AssetDatabase.CreateAsset(surface, "Assets/Materials/OrbitBreakerArenaSurface.physicMaterial");
            }
            var walls = Child(arena.transform, "Walls");
            ArenaCube(walls.transform, "LeftWall", new Vector3(-6.25f, 0f, 0f), new Vector3(0.5f, 21f, 2f), wallMaterial, surface);
            ArenaCube(walls.transform, "RightWall", new Vector3(6.25f, 0f, 0f), new Vector3(0.5f, 21f, 2f), wallMaterial, surface);
            ArenaCube(walls.transform, "TopWall", new Vector3(0f, 10.25f, 0f), new Vector3(12f, 0.5f, 2f), wallMaterial, surface);
            // The four-unit central opening is left clear for losing the ball / future flippers.
            ArenaCube(walls.transform, "LeftGuard", new Vector3(-4f, -10.25f, 0f), new Vector3(4f, 0.5f, 2f), wallMaterial, surface);
            ArenaCube(walls.transform, "RightGuard", new Vector3(4f, -10.25f, 0f), new Vector3(4f, 0.5f, 2f), wallMaterial, surface);
            var zone = ArenaCube(arena.transform, "OrbitBreakerKillZone", new Vector3(0f, -11.75f, 0f), new Vector3(12f, 2.5f, 2f), dangerMaterial, surface);
            zone.GetComponent<BoxCollider>().isTrigger = true;
            Ensure<OrbitBreakerKillZone>(zone);

            var player = FindChild(root.transform, "Player");
            if (player == null)
            {
                var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                ball.name = "Player";
                ball.transform.SetParent(root.transform, false);
                ball.transform.position = FindChild(root.transform, "PlayerSpawn").position;
                ball.GetComponent<MeshRenderer>().sharedMaterial = playerMaterial;
                ball.GetComponent<SphereCollider>().sharedMaterial = surface;
                player = ball.transform;
            }
            var rigidbody = Ensure<Rigidbody>(player.gameObject);
            rigidbody.useGravity = false;
            rigidbody.constraints = OrbitBreakerPlayerMotor.PlaneConstraints;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rigidbody.isKinematic = true;
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            var motor = Ensure<OrbitBreakerPlayerMotor>(player.gameObject);
            var serializedMotor = new SerializedObject(motor);
            serializedMotor.FindProperty("gameManager").objectReferenceValue = manager;
            serializedMotor.ApplyModifiedPropertiesWithoutUndo();
            Ensure<OrbitBreakerPlayerDash>(player.gameObject);
            Ensure<OrbitBreakerPlayerMagnet>(player.gameObject);
            Ensure<OrbitBreakerPlayerInput>(player.gameObject);
            Ensure<OrbitBreakerPlayerFeedback>(player.gameObject);
            AssetDatabase.SaveAssetIfDirty(surface);
        }

        private static GameObject ArenaCube(Transform parent, string name, Vector3 position, Vector3 scale,
            Material material, PhysicMaterial surface)
        {
            var existing = FindChild(parent, name);
            if (existing != null) return existing.gameObject;
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = position;
            cube.transform.localScale = scale;
            cube.GetComponent<MeshRenderer>().sharedMaterial = material;
            cube.GetComponent<BoxCollider>().sharedMaterial = surface;
            return cube;
        }

        private static Material ColorMaterial(string path, Color color)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            EnsureAssetPathEmpty(path);
            var shader = Shader.Find("Standard");
            if (shader == null) throw new InvalidOperationException("Built-In Standard shader was not found.");
            material = new Material(shader) { color = color };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static Transform FindChild(Transform parent, string name)
        {
            var matches = parent.Cast<Transform>().Where(child => child.name == name).ToArray();
            if (matches.Length > 1)
                throw new InvalidOperationException($"Duplicate child '{name}' under '{parent.name}'.");
            return matches.Length == 0 ? null : matches[0];
        }

        private static GameObject Child(Transform parent, string name)
        {
            var child = FindChild(parent, name);
            if (child != null)
                return child.gameObject;
            var result = new GameObject(name);
            result.transform.SetParent(parent, false);
            return result;
        }

        private static void EnsureAssetPathEmpty(string path)
        {
            if (AssetDatabase.LoadMainAssetAtPath(path) != null)
                throw new InvalidOperationException($"Another asset already occupies '{path}'.");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/');
            var parent = path.Substring(0, slash);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, path.Substring(slash + 1));
        }
    }
}
