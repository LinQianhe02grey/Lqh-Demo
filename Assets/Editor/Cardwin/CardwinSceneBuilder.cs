using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using Cardwin.Combat;

namespace Cardwin.Editor
{
    public class CardwinSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Demo_Combat.unity";
        private const float GroundY = -4f;
        private const float GroundWidth = 40f;
        private const float GroundThickness = 1f;

        [MenuItem("Tools/Cardwin/Build Demo Scene")]
        public static void BuildDemoScene()
        {
            string sceneDir = Path.GetDirectoryName(ScenePath);
            if (!Directory.Exists(sceneDir))
                Directory.CreateDirectory(sceneDir);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreatePlaceholderSprite();

            CreateMainCamera();
            CreateGround();
            CreatePlatforms();
            CreateCameraBounds();
            CreatePlayer();
            CreateTestMarkers();
            CreateCanvasHUD();

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
                AssetDatabase.DeleteAsset(ScenePath);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(ScenePath);

            Debug.Log("[Cardwin] Demo scene built and opened: " + ScenePath);
        }

        private static void CreateMainCamera()
        {
            GameObject camObj = new GameObject("MainCamera");
            Camera cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.backgroundColor = new Color(0.15f, 0.15f, 0.2f);
            cam.transform.position = new Vector3(0f, 2f, -10f);
            camObj.tag = "MainCamera";
        }

        private static GameObject CreateGround()
        {
            GameObject ground = new GameObject("Ground");
            ground.transform.position = new Vector3(0f, GroundY, 0f);

            SpriteRenderer sr = ground.AddComponent<SpriteRenderer>();
            sr.sprite = CreateWhiteSquareSprite();
            sr.color = new Color(0.3f, 0.35f, 0.25f);
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = new Vector2(GroundWidth, GroundThickness);

            BoxCollider2D bc = ground.AddComponent<BoxCollider2D>();
            bc.size = new Vector2(GroundWidth, GroundThickness);

            return ground;
        }

        private static void CreatePlatforms()
        {
            float[] heights = { 0f, 2.5f, 5f };
            float[] xs = { -6f, 4f, -2f };
            float[] widths = { 5f, 4f, 3f };

            for (int i = 0; i < heights.Length; i++)
            {
                GameObject plat = new GameObject("Platform_" + (i + 1));
                plat.transform.position = new Vector3(xs[i], heights[i], 0f);

                SpriteRenderer sr = plat.AddComponent<SpriteRenderer>();
                sr.sprite = CreateWhiteSquareSprite();
                sr.color = new Color(0.4f, 0.35f, 0.3f);
                sr.drawMode = SpriteDrawMode.Sliced;
                sr.size = new Vector2(widths[i], 0.4f);

                BoxCollider2D bc = plat.AddComponent<BoxCollider2D>();
                bc.size = new Vector2(widths[i], 0.4f);
            }
        }

        private static void CreateCameraBounds()
        {
            GameObject bounds = new GameObject("CameraBounds");
            bounds.transform.position = new Vector3(0f, 2f, 0f);

            float camHalfW = 6f * 16f / 9f;
            float camHalfH = 6f;
            float margin = 1f;

            BoxCollider2D bc = bounds.AddComponent<BoxCollider2D>();
            bc.size = new Vector2(
                GroundWidth - camHalfW * 2f - margin,
                camHalfH * 2f + margin * 2f
            );
            bc.isTrigger = true;
        }

        private static void CreatePlayer()
        {
            GameObject player = new GameObject("Player");
            player.transform.position = new Vector3(-8f, -1f, 0f);
            player.tag = "Player";

            SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
            sr.sprite = CreateWhiteSquareSprite();
            sr.color = new Color(0.2f, 0.6f, 1f);
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = new Vector2(1f, 1.5f);

            Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 2.2f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            CapsuleCollider2D cc = player.AddComponent<CapsuleCollider2D>();
            cc.size = new Vector2(0.6f, 1.4f);
            cc.offset = new Vector2(0f, 0.1f);

            player.AddComponent<PlayerController2D>();
            Health health = player.AddComponent<Health>();
            health.maxHealth = 50;
            health.currentHealth = 50;

            GameObject groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(player.transform);
            groundCheck.transform.localPosition = new Vector3(0f, -0.8f, 0f);

            Selection.activeGameObject = player;
        }

        private static void CreateTestMarkers()
        {
            CreateMarker("SpawnPoint_Player", new Vector3(-8f, -1f, 0f), Color.green);
            CreateMarker("SpawnPoint_Enemy", new Vector3(6f, -1f, 0f), Color.red);
            CreateMarker("BossDoor_Placeholder", new Vector3(18f, -1.8f, 0f), new Color(1f, 0.5f, 0f));
        }

        private static void CreateMarker(string name, Vector3 position, Color color)
        {
            GameObject marker = new GameObject(name);
            marker.transform.position = position;
            SpriteRenderer sr = marker.AddComponent<SpriteRenderer>();
            sr.sprite = CreateWhiteSquareSprite();
            sr.color = color;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = new Vector2(1f, 2f);
            sr.sortingOrder = -999;
        }

        private static void CreateCanvasHUD()
        {
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            Vector2 refRes = new Vector2(1920, 1080);

            CreateHUDText(canvasObj.transform, "HP_Text", "HP: 50/50",
                new Vector2(-580, 480), 28, TextAnchor.UpperLeft);

            CreateHUDText(canvasObj.transform, "MagazinePreview_Placeholder", "[1] --- | [2] --- | [3] ---",
                new Vector2(0, -440), 22, TextAnchor.LowerCenter);

            CreateHUDText(canvasObj.transform, "State_Text", "State: Idle",
                new Vector2(580, 480), 24, TextAnchor.UpperRight);
        }

        private static void CreateHUDText(Transform parent, string name, string text, Vector2 pos, int fontSize, TextAnchor alignment)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Text txt = go.AddComponent<Text>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.color = Color.white;
            txt.alignment = alignment;
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(400, 50);
        }

        private static Sprite CreateWhiteSquareSprite()
        {
            const int size = 4;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(tex,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                size);
        }

        private static void CreatePlaceholderSprite()
        {
            string artDir = "Assets/Art/Player";
            string assetPath = artDir + "/player_placeholder.png";

            if (AssetDatabase.LoadAssetAtPath<Sprite>(assetPath) != null)
                return;

            const int texSize = 32;
            Texture2D tex = new Texture2D(texSize, texSize, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[texSize * texSize];

            for (int y = 0; y < texSize; y++)
            {
                for (int x = 0; x < texSize; x++)
                {
                    bool isHead = y > texSize * 0.7f && x > texSize * 0.25f && x < texSize * 0.75f;
                    bool isBody = y > texSize * 0.3f && y <= texSize * 0.7f
                        && x > texSize * 0.2f && x < texSize * 0.8f;
                    bool isLegs = y <= texSize * 0.3f
                        && ((x > texSize * 0.25f && x < texSize * 0.45f) ||
                            (x > texSize * 0.55f && x < texSize * 0.75f));

                    if (isHead || isBody || isLegs)
                        pixels[y * texSize + x] = new Color(0.2f, 0.55f, 1f, 1f);
                    else
                        pixels[y * texSize + x] = Color.clear;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            byte[] pngData = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(
                System.IO.Path.Combine(Application.dataPath, "Art/Player/player_placeholder.png"),
                pngData
            );

            AssetDatabase.Refresh();

            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 32;
                importer.SaveAndReimport();
            }

            Debug.Log("[Cardwin] Created player placeholder sprite at: " + assetPath);
        }
    }
}
