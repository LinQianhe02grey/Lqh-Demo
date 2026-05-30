using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Cardwin.Cards;

namespace Cardwin.Editor
{
    public static class CardDatabaseEditorUtility
    {
        private const string CardsFolder = "Assets/Data/Cards";
        private const string DatabasePath = "Assets/Data/Cards/CardDatabase.asset";

        [MenuItem("Tools/Cardwin/Rebuild Card Database")]
        public static void RebuildCardDatabase()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Card Database", "Cannot rebuild CardDatabase while in Play Mode.", "OK");
                return;
            }

            EnsureCardsFolder();

            string[] guids = AssetDatabase.FindAssets("t:CardData", new[] { CardsFolder });
            List<CardData> cards = new List<CardData>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                CardData card = AssetDatabase.LoadAssetAtPath<CardData>(path);
                if (card == null)
                    continue;

                if (card is CardDatabase)
                    continue;

                cards.Add(card);
            }

            CardDatabase database = AssetDatabase.LoadAssetAtPath<CardDatabase>(DatabasePath);
            bool created = database == null;

            if (created)
            {
                database = ScriptableObject.CreateInstance<CardDatabase>();
                AssetDatabase.CreateAsset(database, DatabasePath);
            }

            database.allCards.Clear();
            database.allCards.AddRange(cards);
            database.Initialize();

            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[CardDatabaseEditor] Rebuilt CardDatabase. Count={cards.Count}");

            database.ValidateDatabase();

            if (created)
                Debug.Log($"[CardDatabaseEditor] Created new CardDatabase at {DatabasePath}");
        }

        private static void EnsureCardsFolder()
        {
            if (!AssetDatabase.IsValidFolder(CardsFolder))
            {
                string[] parts = CardsFolder.Split('/');
                string current = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string next = current + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(current, parts[i]);
                    current = next;
                }
            }
        }
    }
}
