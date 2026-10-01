using System;
using System.Collections.Generic;
using UnityEngine;

namespace OrbitBreaker
{
    [DisallowMultipleComponent]
    public sealed class OrbitBreakerEnemySpawner : MonoBehaviour
    {
        public const int MaximumEnemies = 12;
        public const float MinimumSpawnY = -3.5f;
        [SerializeField] private OrbitBreakerPlayerMotor player;
        [SerializeField] private OrbitBreakerScoreManager scoreManager;
        [SerializeField] private OrbitBreakerEnemyController enemyPrefab;
        [SerializeField] private Transform liveRoot;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private bool roaming;
        private readonly List<OrbitBreakerEnemyController> alive = new List<OrbitBreakerEnemyController>();
        private float nextSpawnAt;
        private int spawned;
        private OrbitBreakerGameManager Manager => scoreManager == null ? null : scoreManager.OrbitBreakerGameManager;
        public int AliveCount { get { alive.RemoveAll(e => e == null || e.IsDead); return alive.Count; } }
        public float SpawnCooldownRemaining => Mathf.Max(0f, nextSpawnAt - Time.time);
        public event Action<OrbitBreakerEnemyController> Spawned;

        private void OnEnable()
        {
            if (Manager == null) return;
            Manager.StateChanged += OnStateChanged;
            OnStateChanged(Manager.State);
        }
        private void OnDisable() { if (Manager != null) Manager.StateChanged -= OnStateChanged; }
        private void OnStateChanged(GameState state)
        {
            nextSpawnAt = state == GameState.Playing ? Time.time + Manager.Config.enemySpawnInterval : float.PositiveInfinity;
        }
        private void Update()
        {
            if (Manager == null || !Manager.IsPlaying || Time.time < nextSpawnAt) return;
            // One attempt per interval, without catch-up bursts after stalls or a full board.
            nextSpawnAt = Time.time + Manager.Config.enemySpawnInterval;
            TrySpawn();
        }
        public OrbitBreakerEnemyController TrySpawn()
        {
            if (!isActiveAndEnabled || Manager == null || !Manager.IsPlaying || player == null ||
                enemyPrefab == null || liveRoot == null || (!roaming && (spawnPoints == null || spawnPoints.Length == 0)) || AliveCount >= MaximumEnemies) return null;
            if (roaming)
            {
                float angle=UnityEngine.Random.Range(0f,Mathf.PI*2);
                Vector3 position=player.Position+new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),0)*UnityEngine.Random.Range(11f,15f);
                var enemy=Instantiate(enemyPrefab,position,Quaternion.identity,liveRoot);
                enemy.transform.localScale*=OrbitBreakerProceduralArena.WorldScale;
                enemy.name="OrbitBreakerEnemy";enemy.Initialize(player,scoreManager);
                if(++spawned%3==0){enemy.name="SlimeEnemy";enemy.gameObject.AddComponent<OrbitBreakerSlimeAura>();}alive.Add(enemy);Spawned?.Invoke(enemy);return enemy;
            }
            Physics.SyncTransforms();
            int first = UnityEngine.Random.Range(0, spawnPoints.Length);
            for (int offset = 0; offset < spawnPoints.Length; offset++)
            {
                var point = spawnPoints[(first + offset) % spawnPoints.Length];
                if (point == null) continue;
                var position = point.position;
                // Reject unsafe edited markers as well as the default drain area.
                if (position.y < MinimumSpawnY || position.y > 9.3f || Mathf.Abs(position.x) > 10f || Mathf.Abs(position.z) > 0.001f) continue;
                var box = enemyPrefab.GetComponent<BoxCollider>();
                var half = Vector3.Scale(box.size, enemyPrefab.transform.localScale) * 0.5f + Vector3.one * 0.1f;
                var center = position + Vector3.Scale(box.center, enemyPrefab.transform.localScale);
                if (Physics.CheckBox(center, half, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore)) continue;
                var enemy = Instantiate(enemyPrefab, position, Quaternion.identity, liveRoot);
                enemy.name = "OrbitBreakerEnemy";
                enemy.Initialize(player, scoreManager);
                alive.Add(enemy);
                Spawned?.Invoke(enemy);
                return enemy;
            }
            return null; // All safe points occupied: skip this interval instead of overlapping a body.
        }
    }
}
