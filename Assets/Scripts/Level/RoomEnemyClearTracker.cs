using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardwin.Level
{
    public class RoomEnemyClearTracker : MonoBehaviour
    {
        public Transform enemiesRoot;

        [SerializeField]
        private BossPortal bossPortal;

        [SerializeField]
        private List<GameObject> trackedEnemies = new List<GameObject>();

        public event Action OnAllEnemiesCleared;

        private bool _allCleared;
        private int _checkFrameDelay = 2;

        private void Start()
        {
            if (enemiesRoot == null)
            {
                var found = GameObject.Find("LevelRoot/NormalRoom/Enemies");
                if (found != null)
                    enemiesRoot = found.transform;
            }

            if (enemiesRoot != null)
            {
                for (int i = 0; i < enemiesRoot.childCount; i++)
                {
                    var enemy = enemiesRoot.GetChild(i).gameObject;
                    if (enemy.activeInHierarchy)
                        trackedEnemies.Add(enemy);
                }
            }

            if (trackedEnemies.Count == 0)
            {
                Debug.LogWarning("[RoomEnemyClearTracker] No enemies found to track.");
            }
            else
            {
                Debug.Log($"[RoomEnemyClearTracker] Tracking {trackedEnemies.Count} enemies.");
            }
        }

        private void Update()
        {
            if (_allCleared) return;

            if (_checkFrameDelay > 0)
            {
                _checkFrameDelay--;
                return;
            }

            bool allDead = true;
            foreach (var enemy in trackedEnemies)
            {
                if (enemy != null && enemy.activeInHierarchy)
                {
                    allDead = false;
                    break;
                }
            }

            if (allDead)
            {
                _allCleared = true;
                Debug.Log("[RoomEnemyClearTracker] All enemies cleared!");
                OnAllEnemiesCleared?.Invoke();

                if (bossPortal != null)
                {
                    bossPortal.ActivatePortal();
                }
                else
                {
                    Debug.LogWarning("[RoomEnemyClearTracker] No BossPortal assigned.");
                }
            }
        }
    }
}
