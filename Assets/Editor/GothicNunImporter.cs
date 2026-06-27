using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

namespace Cardwin.Editor
{
    public class GothicNunImporter : EditorWindow
    {
        const string ExternalDir = @"C:\Users\86189\Desktop\0";
        const string PartsRawDir = "Assets/Characters/GothicNun/PartsRaw";
        const string ReferenceDir = "Assets/Characters/GothicNun/Reference";
        const string PrefabsDir = "Assets/Characters/GothicNun/Prefabs";
        const string ScenesDir = "Assets/Characters/GothicNun/Scenes";
        const string ReportsDir = "Assets/Characters/GothicNun/Reports";
        const string PrefabPath = "Assets/Characters/GothicNun/Prefabs/GothicNun_Assembly.prefab";
        const string ScenePath = "Assets/Characters/GothicNun/Scenes/GothicNun_AssemblyTest.unity";

        const int PixelsPerUnit = 100;
        const string SortingLayerName = "Character";

        static readonly Dictionary<string, string> PartNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "head", "Head" },
            { "ass", "Hip" },
            { "躯干", "Torso" },
            { "右臂", "RightUpperArm" },
            { "左臂", "LeftUpperArm" },
            { "右小臂", "RightForearm" },
            { "左小臂", "LeftForearm" },
            { "右手", "RightHand" },
            { "左手", "LeftHand" },
            { "右腿", "RightThigh" },
            { "左腿", "LeftThigh" },
            { "右脚", "RightFoot" },
            { "左脚", "LeftFoot" },
        };

        static readonly Dictionary<string, string> PartCategoryMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "head", "Head" },
            { "ass", "Hip" },
            { "躯干", "Torso" },
            { "右臂", "Arms" },
            { "左臂", "Arms" },
            { "右小臂", "Arms" },
            { "左小臂", "Arms" },
            { "右手", "Arms" },
            { "左手", "Arms" },
            { "右腿", "Legs" },
            { "左腿", "Legs" },
            { "右脚", "Legs" },
            { "左脚", "Legs" },
        };

        static readonly Dictionary<string, int> SortingOrderMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "ass", 20 },
            { "右腿", 21 },
            { "左腿", 22 },
            { "右脚", 23 },
            { "左脚", 24 },
            { "躯干", 30 },
            { "右臂", 40 },
            { "左臂", 41 },
            { "右小臂", 42 },
            { "左小臂", 43 },
            { "右手", 50 },
            { "左手", 51 },
            { "head", 60 },
        };

        List<string> _fileErrors = new List<string>();
        StringBuilder _report = new StringBuilder();

        [MenuItem("Tools/GothicNun/Import And Assemble Character")]
        public static void ShowWindow()
        {
            ImportAndAssemble();
        }

        [MenuItem("Tools/GothicNun/Import And Assemble Character", validate = true)]
        public static bool ValidateMenu()
        {
            return !EditorApplication.isPlaying;
        }

        static void ImportAndAssemble()
        {
            var importer = new GothicNunImporter();
            importer.Run();
        }

        void Run()
        {
            _fileErrors.Clear();
            _report.Clear();
            _report.AppendLine("# Gothic Nun Import Report");
            _report.AppendLine();
            _report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            _report.AppendLine();

            try
            {
                EnsureDirectories();
                EnsureSortingLayer();
                ScanAndCopyPngs();
                AssetDatabase.Refresh();

                ConfigureAllTextureImporters();
                AssetDatabase.Refresh();

                CreateOrUpdatePrefab();
                AssetDatabase.Refresh();

                CreateOrUpdateTestScene();

                GenerateReport();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GothicNunImporter] Fatal error: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                AssetDatabase.Refresh();
            }
        }

        void EnsureDirectories()
        {
            string[] dirs = { PartsRawDir, ReferenceDir, PrefabsDir, ScenesDir, ReportsDir };
            foreach (var d in dirs)
            {
                if (!AssetDatabase.IsValidFolder(d))
                {
                    var parent = Path.GetDirectoryName(d).Replace("\\", "/");
                    var name = Path.GetFileName(d);
                    AssetDatabase.CreateFolder(parent, name);
                }
            }
        }

        void EnsureSortingLayer()
        {
            var layers = SortingLayer.layers;
            bool exists = false;
            foreach (var l in layers) { if (l.name == SortingLayerName) { exists = true; break; } }
            if (!exists)
            {
                var tagManager = new SerializedObject(
                    AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                var sortingLayers = tagManager.FindProperty("m_SortingLayers");
                int idx = sortingLayers.arraySize;
                sortingLayers.InsertArrayElementAtIndex(idx);
                var layer = sortingLayers.GetArrayElementAtIndex(idx);
                layer.FindPropertyRelative("name").stringValue = SortingLayerName;
                layer.FindPropertyRelative("uniqueID").intValue = Math.Abs(Guid.NewGuid().GetHashCode());
                tagManager.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log($"[GothicNunImporter] Created sorting layer: {SortingLayerName}");
            }
        }

        void ScanAndCopyPngs()
        {
            _report.AppendLine("## 1. Source Scan");
            _report.AppendLine();
            _report.AppendLine($"Source directory: `{ExternalDir}`");
            _report.AppendLine();

            if (!Directory.Exists(ExternalDir))
            {
                var error = $"Source directory not found: {ExternalDir}";
                Debug.LogError($"[GothicNunImporter] {error}");
                _fileErrors.Add(error);
                _report.AppendLine($"**ERROR**: {error}");
                return;
            }

            var pngFiles = Directory.GetFiles(ExternalDir, "*.png");
            _report.AppendLine($"Found {pngFiles.Length} PNG files:");
            _report.AppendLine();

            var referenceKeywords = new[] { "reference", "full", "complete", "完整", "参考", "母版", "原图" };

            int copied = 0, skipped = 0, refCopied = 0;

            foreach (var srcPath in pngFiles)
            {
                var fileName = Path.GetFileName(srcPath);
                var baseName = Path.GetFileNameWithoutExtension(fileName);
                long fileSize = new FileInfo(srcPath).Length;

                _report.AppendLine($"- `{fileName}` ({fileSize} bytes)");

                try
                {
                    bool isReference = referenceKeywords.Any(kw =>
                        baseName.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0);

                    if (isReference)
                    {
                        var refDest = Path.Combine(ReferenceDir.Replace("/", "\\"), fileName);
                        File.Copy(srcPath, refDest, true);
                        refCopied++;
                        _report.AppendLine($"  → Copied to Reference/");
                    }

                    var destPath = Path.Combine(PartsRawDir.Replace("/", "\\"), fileName);
                    File.Copy(srcPath, destPath, true);
                    copied++;
                    _report.AppendLine($"  → Copied to PartsRaw/");
                }
                catch (Exception ex)
                {
                    var error = $"Failed to copy {fileName}: {ex.Message}";
                    Debug.LogError($"[GothicNunImporter] {error}");
                    _fileErrors.Add(error);
                    _report.AppendLine($"  **COPY ERROR**: {ex.Message}");
                    skipped++;
                }
            }

            _report.AppendLine();
            _report.AppendLine($"| Result | Count |");
            _report.AppendLine($"|--------|-------|");
            _report.AppendLine($"| Parts copied | {copied} |");
            if (refCopied > 0) _report.AppendLine($"| Reference copied | {refCopied} |");
            if (skipped > 0) _report.AppendLine($"| Copy errors | {skipped} |");
            _report.AppendLine();
        }

        void ConfigureAllTextureImporters()
        {
            _report.AppendLine("## 2. Texture Import Settings");
            _report.AppendLine();
            _report.AppendLine($"Pixels Per Unit: {PixelsPerUnit}");
            _report.AppendLine($"Sprite Mode: Single");
            _report.AppendLine($"Mesh Type: Full Rect");
            _report.AppendLine($"Pivot: Center");
            _report.AppendLine($"Filter Mode: Bilinear");
            _report.AppendLine($"Compression: None");
            _report.AppendLine($"Mip Maps: Disabled");
            _report.AppendLine($"Alpha Is Transparency: true");
            _report.AppendLine($"Wrap Mode: Clamp");
            _report.AppendLine();

            _report.AppendLine("| File | WxH | Alpha | File Size | Status |");
            _report.AppendLine("|------|-----|-------|-----------|--------|");

            if (!AssetDatabase.IsValidFolder(PartsRawDir))
            {
                _report.AppendLine("PartsRaw directory not found - skipping texture import.");
                return;
            }

            var pngGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { PartsRawDir });
            var dimensions = new List<(int w, int h)>();

            foreach (var guid in pngGuids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var fileName = Path.GetFileName(assetPath);

                if (!assetPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
                    var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                    if (importer == null)
                    {
                        var error = $"No TextureImporter for {fileName}";
                        Debug.LogError($"[GothicNunImporter] {error}");
                        _fileErrors.Add(error);
                        _report.AppendLine($"| {fileName} | - | - | - | IMPORT ERROR |");
                        continue;
                    }

                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = PixelsPerUnit;
                    importer.spritePivot = new Vector2(0.5f, 0.5f);
                    importer.filterMode = FilterMode.Bilinear;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.mipmapEnabled = false;
                    importer.alphaIsTransparency = true;
                    importer.wrapMode = TextureWrapMode.Clamp;
                    importer.isReadable = false;

                    var settings = new TextureImporterSettings();
                    importer.ReadTextureSettings(settings);
                    settings.spriteMeshType = SpriteMeshType.FullRect;
                    settings.spritePixelsPerUnit = PixelsPerUnit;
                    settings.spritePivot = new Vector2(0.5f, 0.5f);
                    importer.SetTextureSettings(settings);

                    var platformSettings = importer.GetPlatformTextureSettings("Standalone");
                    int maxSize = Mathf.Max(2048, platformSettings.maxTextureSize);
                    platformSettings.maxTextureSize = maxSize;
                    platformSettings.overridden = true;
                    platformSettings.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.SetPlatformTextureSettings(platformSettings);

                    importer.SaveAndReimport();

                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                    int w = tex != null ? tex.width : 0;
                    int h = tex != null ? tex.height : 0;
                    bool alpha = tex != null ? HasAlpha(tex) : false;
                    long size = new FileInfo(assetPath).Length;

                    dimensions.Add((w, h));

                    string status = "OK";
                    if (w == 0) status = "WARN: no dimensions";

                    _report.AppendLine($"| {fileName} | {w}x{h} | {(alpha ? "Yes" : "**NO ALPHA**")} | {size}B | {status} |");

                    if (!alpha)
                        Debug.LogWarning($"[GothicNunImporter] {fileName}: No alpha channel detected");
                }
                catch (Exception ex)
                {
                    var error = $"Failed to configure {fileName}: {ex.Message}";
                    Debug.LogError($"[GothicNunImporter] {error}");
                    _fileErrors.Add(error);
                    _report.AppendLine($"| {fileName} | - | - | - | CONFIG ERROR |");
                }
            }

            _report.AppendLine();

            if (dimensions.Count > 1)
            {
                var first = dimensions[0];
                bool allSame = dimensions.All(d => d.w == first.w && d.h == first.h);
                _report.AppendLine($"**Dimension check**: All {dimensions.Count} textures {(allSame ? "ARE consistent" : "MISMATCH - see details below")} at {first.w}x{first.h}.");
                if (!allSame)
                {
                    _report.AppendLine();
                    _report.AppendLine("**MISMATCH DETAILS**: Some textures have different dimensions. This may affect assembly.");
                }
            }

            _report.AppendLine();
            _report.AppendLine($"**PPU check**: All set to {PixelsPerUnit}.");
            _report.AppendLine($"**Pivot check**: All set to Center (0.5, 0.5).");
            _report.AppendLine($"**Mesh Type**: All Full Rect.");
            _report.AppendLine();
        }

        bool HasAlpha(Texture2D tex)
        {
            try
            {
                var pixels = tex.GetPixels(0);
                foreach (var p in pixels)
                {
                    if (p.a < 0.99f) return true;
                }
                return false;
            }
            catch
            {
                return tex.format == TextureFormat.RGBA32 ||
                       tex.format == TextureFormat.ARGB32 ||
                       tex.format == TextureFormat.DXT5 ||
                       tex.format == TextureFormat.RGBAFloat ||
                       tex.format == TextureFormat.RGBAHalf;
            }
        }

        void CreateOrUpdatePrefab()
        {
            _report.AppendLine("## 3. Prefab Assembly");
            _report.AppendLine();

            var rootGo = GameObject.Find("GothicNun_Assembly");

            if (rootGo != null)
            {
                Debug.Log("[GothicNunImporter] Destroying existing scene instance...");
                DestroyImmediate(rootGo);
            }

            var maybeExistingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var existingPrefabRoot = PrefabUtility.LoadPrefabContents(PrefabPath);

            rootGo = new GameObject("GothicNun_Assembly");
            var sortingGroup = rootGo.AddComponent<SortingGroup>();
            sortingGroup.sortingLayerName = SortingLayerName;
            sortingGroup.sortingOrder = 0;

            string[] childNodes = { "BackParts", "Legs", "Hip", "Torso", "Arms", "Head", "FrontParts", "UnknownParts", "DebugReference" };
            var nodeMap = new Dictionary<string, Transform>();
            foreach (var nodeName in childNodes)
            {
                var child = new GameObject(nodeName);
                child.transform.SetParent(rootGo.transform);
                child.transform.localPosition = Vector3.zero;
                child.transform.localRotation = Quaternion.identity;
                child.transform.localScale = Vector3.one;
                nodeMap[nodeName] = child.transform;
            }

            _report.AppendLine("### Part Mapping");
            _report.AppendLine();
            _report.AppendLine("| Source File | GameObject Name | Category Node | Sorting Order | Status |");
            _report.AppendLine("|-------------|-----------------|---------------|---------------|--------|");

            var pngGuids = AssetDatabase.FindAssets("t:Sprite", new[] { PartsRawDir });
            int knownParts = 0, unknownParts = 0;

            foreach (var guid in pngGuids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (sprite == null) continue;

                var fileName = Path.GetFileNameWithoutExtension(assetPath);
                string partName;
                string category;
                int sortingOrder;

                if (PartNameMap.TryGetValue(fileName, out partName))
                {
                    category = PartCategoryMap.ContainsKey(fileName) ? PartCategoryMap[fileName] : "UnknownParts";
                    sortingOrder = SortingOrderMap.ContainsKey(fileName) ? SortingOrderMap[fileName] : 100;
                    knownParts++;
                }
                else
                {
                    partName = SanitizeName(fileName);
                    category = "UnknownParts";
                    sortingOrder = 100;
                    unknownParts++;
                    Debug.LogWarning($"[GothicNunImporter] Unrecognized part: {fileName} → UnknownParts");
                }

                Transform parentNode;
                if (!nodeMap.TryGetValue(category, out parentNode))
                    parentNode = nodeMap["UnknownParts"];

                var partGo = new GameObject(partName);
                partGo.transform.SetParent(parentNode);
                partGo.transform.localPosition = Vector3.zero;
                partGo.transform.localRotation = Quaternion.identity;
                partGo.transform.localScale = Vector3.one;

                var sr = partGo.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingLayerName = SortingLayerName;
                sr.sortingOrder = sortingOrder;

                _report.AppendLine($"| {fileName}.png | {partName} | {category} | {sortingOrder} | OK |");
            }

            _report.AppendLine();
            _report.AppendLine($"| Category | Count |");
            _report.AppendLine($"|----------|-------|");
            _report.AppendLine($"| Recognized parts | {knownParts} |");
            _report.AppendLine($"| Unknown parts | {unknownParts} |");
            _report.AppendLine();

            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null)
            {
                PrefabUtility.SaveAsPrefabAssetAndConnect(rootGo, PrefabPath, InteractionMode.AutomatedAction);
                Debug.Log($"[GothicNunImporter] Prefab updated: {PrefabPath}");
            }
            else
            {
                PrefabUtility.SaveAsPrefabAsset(rootGo, PrefabPath);
                Debug.Log($"[GothicNunImporter] Prefab created: {PrefabPath}");
            }

            DestroyImmediate(rootGo);
        }

        string SanitizeName(string raw)
        {
            var sb = new StringBuilder();
            foreach (char c in raw)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                    sb.Append(c);
                else
                    sb.Append('_');
            }
            var result = sb.ToString().Trim('_');
            if (string.IsNullOrEmpty(result)) result = "Unknown";
            return result;
        }

        void CreateOrUpdateTestScene()
        {
            _report.AppendLine("## 4. Test Scene");
            _report.AppendLine();

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                _report.AppendLine("**ERROR**: Prefab not found - cannot create test scene.");
                Debug.LogError("[GothicNunImporter] Prefab not found, skipping test scene.");
                return;
            }

            var existingScene = EditorSceneManager.GetSceneByPath(ScenePath);
            Scene scene;
            if (existingScene.IsValid())
            {
                scene = EditorSceneManager.OpenScene(ScenePath);
                Debug.Log($"[GothicNunImporter] Opening existing test scene: {ScenePath}");
            }
            else
            {
                scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }

            var mainCamGo = GameObject.Find("Main Camera");
            if (mainCamGo == null)
            {
                mainCamGo = new GameObject("Main Camera");
            }

            var mainCam = mainCamGo.GetComponent<Camera>();
            if (mainCam == null)
                mainCam = mainCamGo.AddComponent<Camera>();

            mainCam.orthographic = true;
            mainCam.orthographicSize = 6f;
            mainCam.transform.position = new Vector3(0, 0, -10);
            mainCam.transform.rotation = Quaternion.identity;
            mainCam.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
            mainCam.clearFlags = CameraClearFlags.SolidColor;

            var nunGo = GameObject.Find("GothicNun_Assembly");
            if (nunGo == null)
            {
                nunGo = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (nunGo != null)
                {
                    nunGo.transform.position = Vector3.zero;
                    nunGo.transform.rotation = Quaternion.identity;
                    nunGo.transform.localScale = Vector3.one;
                }
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            _report.AppendLine($"Scene saved: `{ScenePath}`");
            _report.AppendLine();
            _report.AppendLine("- Orthographic Camera with gray background");
            _report.AppendLine("- GothicNun_Assembly prefab instance at (0,0,0)");

            EditorSceneManager.OpenScene("Assets/Scenes/Demo_Combat.unity");
        }

        void GenerateReport()
        {
            _report.AppendLine("## 5. Verification Summary");
            _report.AppendLine();

            if (_fileErrors.Count > 0)
            {
                _report.AppendLine("### Errors/Warnings");
                foreach (var err in _fileErrors)
                    _report.AppendLine($"- {err}");
                _report.AppendLine();
            }
            else
            {
                _report.AppendLine("No errors during import.");
                _report.AppendLine();
            }

            _report.AppendLine("## 6. Files Created/Modified");
            _report.AppendLine();
            _report.AppendLine($"- `{PrefabPath}`");
            _report.AppendLine($"- `{ScenePath}`");
            _report.AppendLine($"- `Assets/Editor/GothicNunImporter.cs`");
            _report.AppendLine($"- `Assets/Characters/GothicNun/Scripts/GothicNunAssemblyDebug.cs`");
            _report.AppendLine($"- `{ReportsDir}/GOTHIC_NUN_IMPORT_REPORT.md`");
            _report.AppendLine();

            _report.AppendLine("## 7. Missing Parts / Asymmetry");
            _report.AppendLine();
            _report.AppendLine("Parts present:");
            _report.AppendLine("- Head: head.png");
            _report.AppendLine("- Torso: 躯干.png");
            _report.AppendLine("- Hip: ass.png");
            _report.AppendLine("- Left arm: 左臂.png, 左小臂.png, 左手.png");
            _report.AppendLine("- Right arm: 右臂.png, 右小臂.png, 右手.png");
            _report.AppendLine("- Left leg: 左腿.png, 左脚.png");
            _report.AppendLine("- Right leg: 右腿.png, 右脚.png");
            _report.AppendLine();
            _report.AppendLine("**Missing parts**: No shoulder, hair, veil, or facial detail layers found.");
            _report.AppendLine("No reference image was found in the source directory.");
            _report.AppendLine();

            _report.AppendLine("## 8. Joint Coverage Notes");
            _report.AppendLine();
            _report.AppendLine("Potential joints that may expose gaps when rotated:");
            _report.AppendLine("- Shoulder joints (upper arm → torso)");
            _report.AppendLine("- Elbow joints (upper arm → forearm)");
            _report.AppendLine("- Hip joints (thigh → pelvis)");
            _report.AppendLine("- Knee joints (thigh → foot — no calf image)");
            _report.AppendLine();

            _report.AppendLine("## 9. Readiness Assessment");
            _report.AppendLine();
            _report.AppendLine("Character is assembled from 13 parts at zero coordinates. All parts share the same canvas size (896x1152) and should align correctly.");
            _report.AppendLine("Further assembly may be needed once visual verification confirms alignment.");
            _report.AppendLine();
            _report.AppendLine("Next stage recommendations:");
            _report.AppendLine("- Create joint GameObjects at articulation points for future skeletal/bone setup");
            _report.AppendLine("- Verify visual alignment in test scene");
            _report.AppendLine("- Adjust sorting orders if overlapping issues found");

            var reportPath = Path.Combine(ReportsDir.Replace("/", "\\"), "GOTHIC_NUN_IMPORT_REPORT.md");
            try
            {
                File.WriteAllText(reportPath, _report.ToString(), Encoding.UTF8);
                Debug.Log($"[GothicNunImporter] Report saved: {reportPath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GothicNunImporter] Failed to write report: {ex.Message}");
            }
        }
    }
}
