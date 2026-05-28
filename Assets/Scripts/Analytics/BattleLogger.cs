using System.Collections.Generic;
using UnityEngine;

namespace Cardwin.Analytics
{
    public class BattleLogger : MonoBehaviour
    {
        public struct BattleEntry
        {
            public string cardId;
            public string effectType;
            public int value;
            public string targetName;
            public float timestamp;
        }

        public List<BattleEntry> Entries { get; private set; } = new();

        public void LogCardPlay(string cardId, string effectType, int value, string targetName) { }

        public void LogDamageDealt(int amount, string targetName) { }

        public void LogHeal(int amount, string targetName) { }

        public void LogEnemyDeath(string enemyName) { }

        public void ClearLog() { Entries.Clear(); }

        public List<BattleEntry> GetEntriesByCard(string cardId) { return new List<BattleEntry>(); }
    }
}
