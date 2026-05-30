using System.IO;
using UnityEditor;
using UnityEngine;
using Cardwin.Cards;

namespace Cardwin.EditorTools
{
    public static class CardAssetCreator
    {
        private const string CardDir = "Assets/Data/Cards";
        private const string ProjPrefabPath = "Assets/Prefabs/Projectiles/Projectile_Test.prefab";

        [MenuItem("Tools/Cardwin/Create Basic Card Assets")]
        public static void CreateBasicCards()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Error", "Cannot create cards while in Play Mode.", "OK");
                return;
            }

            EnsureDirectory();
            GameObject projPrefab = FindProjectilePrefab();

            CreateOrUpdateCard("Strike", "Strike_001", CardType.Attack, CardRarity.Common,
                10, 0, 0, 0, CardEffectType.Damage, CardEffectType.Damage,
                projPrefab, "左键发射子弹命中目标造成伤害，右键对自己造成伤害。");

            CreateOrUpdateCard("Guard", "Guard_001", CardType.Defense, CardRarity.Common,
                0, 10, 0, 0, CardEffectType.Block, CardEffectType.Block,
                projPrefab, "左键发射子弹给命中目标加盾，右键给自己加盾。");

            CreateOrUpdateCard("Heal", "Heal_001", CardType.Heal, CardRarity.Common,
                0, 0, 12, 0, CardEffectType.Heal, CardEffectType.Heal,
                projPrefab, "左键发射子弹治疗命中目标，右键治疗自己。");

            CreateOrUpdateCard("Focus", "Focus_001", CardType.Utility, CardRarity.Common,
                0, 0, 0, 1, CardEffectType.Focus, CardEffectType.Focus,
                null, "获得 Focus 层数，强化下一次 Damage 攻击。左键对非Player目标忽略。");

            AssetDatabase.Refresh();
            Debug.Log("[CardAssetCreator] 4 basic card assets created/updated in " + CardDir);
        }

        private static void EnsureDirectory()
        {
            if (!Directory.Exists(CardDir))
                Directory.CreateDirectory(CardDir);
        }

        private static GameObject FindProjectilePrefab()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ProjPrefabPath);
            if (prefab == null)
                Debug.LogWarning($"[CardAssetCreator] Projectile_Test.prefab not found at {ProjPrefabPath}. Damage cards will have no projectilePrefab.");
            return prefab;
        }

        private static void CreateOrUpdateCard(string name, string cardId, CardType type, CardRarity rarity,
            int damage, int block, int heal, int focus, CardEffectType left, CardEffectType right,
            GameObject proj, string desc)
        {
            string path = $"{CardDir}/{name}.asset";
            CardData card = AssetDatabase.LoadAssetAtPath<CardData>(path);

            if (card == null)
            {
                card = ScriptableObject.CreateInstance<CardData>();
                AssetDatabase.CreateAsset(card, path);
            }

            card.cardId = cardId;
            card.cardName = name;
            card.cardType = type;
            card.rarity = rarity;
            card.damage = damage;
            card.block = block;
            card.heal = heal;
            card.focusGain = focus;
            card.leftClickEffect = left;
            card.rightClickEffect = right;
            card.projectilePrefab = proj;
            card.description = desc;

            EditorUtility.SetDirty(card);
            Debug.Log($"[CardAssetCreator] {name}.asset created/updated");
        }
    }
}
