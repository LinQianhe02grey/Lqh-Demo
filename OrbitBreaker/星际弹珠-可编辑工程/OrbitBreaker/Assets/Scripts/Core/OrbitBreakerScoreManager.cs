using System;
using System.Collections.Generic;
using UnityEngine;

namespace OrbitBreaker
{
    [DisallowMultipleComponent]
    public sealed class OrbitBreakerScoreManager : MonoBehaviour
    {
        [SerializeField] private OrbitBreakerGameManager gameManager;
        private OrbitBreakerPlayerMotor player;
        private void Awake() { player = FindObjectOfType<OrbitBreakerPlayerMotor>(); }
        private readonly Dictionary<int, int> lastInteractions = new Dictionary<int, int>();
        public int Score { get; private set; }
        public int TargetScore => gameManager != null && gameManager.Config != null ? gameManager.Config.targetScore : 0;
        public OrbitBreakerGameManager OrbitBreakerGameManager => gameManager;
        public event Action<int> ScoreChanged;

        // Each source supplies increasing interaction IDs; duplicate/stale events cannot score twice.
        public bool TryAward(int points, UnityEngine.Object source, int interactionId)
        {
            if (!isActiveAndEnabled || gameManager == null || !gameManager.IsPlaying ||
                TargetScore <= 0 || points <= 0 || source == null || interactionId <= 0) return false;
            int sourceId = source.GetInstanceID();
            if (lastInteractions.TryGetValue(sourceId, out int previous) && interactionId <= previous) return false;
            lastInteractions[sourceId] = interactionId;
            int boosted = Mathf.RoundToInt(points * (player == null ? 1f : player.ComboMultiplier));
            Score = (int)Math.Min(int.MaxValue, (long)Score + boosted);
            // Commit the terminal state before notifying listeners, preventing reentrant terminal awards.
            // Free mode has an endless score; OrbitBreakerBattle ends only when the Boss dies.
            ScoreChanged?.Invoke(Score);
            return true;
        }
    }
}
