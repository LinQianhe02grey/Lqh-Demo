#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using MirrorSaintessBossPack;

public static class MirrorSaintessBossInstaller
{
    private const string GeneratedRoot = "Assets/MirrorSaintessBossPack/Generated";

    [MenuItem("Tools/Mirror Saintess Boss/Build Prototype Prefab")]
    public static void BuildPrototypePrefab()
    {
        Directory.CreateDirectory(GeneratedRoot);
        Directory.CreateDirectory("Assets/Prefabs/Boss");

        SetAllTexturesAsSprites();

        Sprite body = FindSprite("Boss_Body_Transparent");
        Sprite blueGun = FindSprite("BlueGun_Intact");
        Sprite blueGunBroken = FindSprite("BlueGun_Broken");
        Sprite redGun = FindSprite("RedGun_Intact");
        Sprite redGunBroken = FindSprite("RedGun_Broken");
        Sprite chestCore = FindSprite("ChestCore_Intact");
        Sprite chestCoreBroken = FindSprite("ChestCore_Broken");

        if (body == null)
        {
            Debug.LogError("[MirrorSaintessBossInstaller] Boss_Body_Transparent sprite not found.");
            return;
        }

        GameObject root = new GameObject("MirrorSaintessBoss_Prototype");
        root.transform.position = Vector3.zero;
        root.transform.localScale = Vector3.one;

        var boss = root.AddComponent<MirrorSaintessBoss>();
        var rb = root.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        var rootCollider = root.AddComponent<BoxCollider2D>();
        rootCollider.isTrigger = true;
        rootCollider.size = new Vector2(2.2f, 5.2f);
        rootCollider.offset = new Vector2(0f, 2.6f);

        GameObject bodyGo = new GameObject("Body");
        bodyGo.transform.SetParent(root.transform, false);
        bodyGo.transform.localPosition = new Vector3(0f, 2.7f, 0f);
        var bodyRenderer = bodyGo.AddComponent<SpriteRenderer>();
        bodyRenderer.sprite = body;
        bodyRenderer.sortingOrder = 10;

        Animator animator = root.AddComponent<Animator>();
        AnimatorController controller = CreateAnimatorController();
        animator.runtimeAnimatorController = controller;

        CreatePart(root.transform, "Part_ChestCore", MirrorSaintessPartType.ChestCore, chestCore, chestCoreBroken, new Vector3(0f, 2.95f, -0.05f), new Vector2(0.65f,0.65f), 160f);
        CreatePart(root.transform, "Part_RightHand_BlueGun", MirrorSaintessPartType.BlueGun, blueGun, blueGunBroken, new Vector3(-1.25f, 1.35f, -0.05f), new Vector2(0.75f,1.0f), 120f);
        CreatePart(root.transform, "Part_LeftHand_RedGun", MirrorSaintessPartType.RedGun, redGun, redGunBroken, new Vector3(1.25f, 1.35f, -0.05f), new Vector2(0.75f,1.0f), 120f);

        CreateFirePoint(root.transform, "FirePoint_Blue", new Vector3(-1.75f, 1.1f, 0f));
        CreateFirePoint(root.transform, "FirePoint_Red", new Vector3(1.75f, 1.1f, 0f));

        PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/Boss/MirrorSaintessBoss_Prototype.prefab");
        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[MirrorSaintessBossInstaller] Created Assets/Prefabs/Boss/MirrorSaintessBoss_Prototype.prefab");
    }

    private static void CreatePart(Transform parent, string name, MirrorSaintessPartType type, Sprite intact, Sprite broken, Vector3 localPos, Vector2 colliderSize, float hp)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = Vector3.one * 0.18f;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = intact;
        sr.sortingOrder = 20;
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = colliderSize;
        var part = go.AddComponent<MirrorSaintessBossPart>();

        SerializedObject so = new SerializedObject(part);
        so.FindProperty("partType").enumValueIndex = (int)type;
        so.FindProperty("maxHp").floatValue = hp;
        so.FindProperty("spriteRenderer").objectReferenceValue = sr;
        so.FindProperty("intactSprite").objectReferenceValue = intact;
        so.FindProperty("brokenSprite").objectReferenceValue = broken;
        so.FindProperty("partCollider").objectReferenceValue = col;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateFirePoint(Transform parent, string name, Vector3 localPos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
    }

    private static AnimatorController CreateAnimatorController()
    {
        string controllerPath = GeneratedRoot + "/MirrorSaintessBoss.controller";
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        string[] triggers = { "Idle", "Intro", "CastBlue", "CastRed", "Hurt", "Phase2", "Stun", "Death" };
        foreach (string t in triggers)
        {
            controller.AddParameter(t, AnimatorControllerParameterType.Trigger);
        }

        var sm = controller.layers[0].stateMachine;
        CreateState(sm, "Idle", CreateSpriteClip("MirrorSaintess_Idle", "idle_", "Art/Animations/Frames/Idle", 8f), true);
        CreateState(sm, "CastBlue", CreateSpriteClip("MirrorSaintess_CastBlue", "cast_blue_", "Art/Animations/Frames/CastBlue", 10f), false);
        CreateState(sm, "CastRed", CreateSpriteClip("MirrorSaintess_CastRed", "cast_red_", "Art/Animations/Frames/CastRed", 10f), false);
        CreateState(sm, "Hurt", CreateSpriteClip("MirrorSaintess_Hurt", "hurt_", "Art/Animations/Frames/Hurt", 12f), false);
        CreateState(sm, "Phase2", CreateSpriteClip("MirrorSaintess_Phase2", "phase2_", "Art/Animations/Frames/Phase2", 10f), false);
        CreateState(sm, "Death", CreateSpriteClip("MirrorSaintess_Death", "death_", "Art/Animations/Frames/Death", 8f), false);
        return controller;
    }

    private static void CreateState(AnimatorStateMachine sm, string name, AnimationClip clip, bool isDefault)
    {
        AnimatorState state = sm.AddState(name);
        state.motion = clip;
        if (isDefault)
        {
            sm.defaultState = state;
        }
    }

    private static AnimationClip CreateSpriteClip(string clipName, string prefix, string folderSuffix, float fps)
    {
        string path = GeneratedRoot + "/" + clipName + ".anim";
        AnimationClip clip = new AnimationClip();
        clip.frameRate = fps;
        EditorCurveBinding binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "Body",
            propertyName = "m_Sprite"
        };

        List<Sprite> frames = FindSpritesByPrefix(prefix, folderSuffix);
        ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[frames.Count];
        for (int i = 0; i < frames.Count; i++)
        {
            keys[i] = new ObjectReferenceKeyframe
            {
                time = i / fps,
                value = frames[i]
            };
        }

        if (keys.Length > 0)
        {
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
        }

        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    private static List<Sprite> FindSpritesByPrefix(string prefix, string folderSuffix)
    {
        List<Sprite> result = new List<Sprite>();
        string[] guids = AssetDatabase.FindAssets(prefix + " t:Sprite");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.Contains(folderSuffix))
            {
                continue;
            }
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
            {
                result.Add(sprite);
            }
        }
        result.Sort((a,b) => string.CompareOrdinal(AssetDatabase.GetAssetPath(a), AssetDatabase.GetAssetPath(b)));
        return result;
    }

    private static Sprite FindSprite(string name)
    {
        string[] guids = AssetDatabase.FindAssets(name + " t:Sprite");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path) == name)
            {
                return AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
        }
        return null;
    }

    private static void SetAllTexturesAsSprites()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/MirrorSaintessBossPack" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                continue;
            }
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 256f;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }
    }
}
#endif
