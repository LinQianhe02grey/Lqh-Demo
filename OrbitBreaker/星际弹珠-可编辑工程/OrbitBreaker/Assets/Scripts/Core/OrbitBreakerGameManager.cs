using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OrbitBreaker
{
    public enum GameMode { Free, OrbitBreakerBattle }
    public enum GameState { Ready, Playing, Victory, Defeat }

    [DisallowMultipleComponent]
    public sealed class OrbitBreakerGameManager : MonoBehaviour
    {
        [SerializeField] private OrbitBreakerCombatConfig config;
        [SerializeField] private OrbitBreakerEncounterConfig encounter;
        public OrbitBreakerEncounterConfig Rules => encounter;
        public GameMode Mode { get; private set; } = GameMode.Free;
        public bool EncyclopediaOpen { get; set; }
        private static GameMode? pendingMode;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetPending()=>pendingMode=null;
        private void Start(){if(pendingMode.HasValue){var next=pendingMode.Value;pendingMode=null;StartMode(next);}}
        public bool StartMode(GameMode mode){if(State!=GameState.Ready)return false;Mode=mode;EncyclopediaOpen=false;return StartGame();}
        public void HandleEscape()
        {
            if(EncyclopediaOpen){EncyclopediaOpen=false;return;}
            if(State!=GameState.Ready){ReturnToMenu();return;}
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
#else
            Application.Quit();
#endif
        }
        public void ReturnToMenu(){if(restartRequested)return;pendingMode=null;Reload();}
        private void Reload(){restartRequested=true;Time.timeScale=1;SceneManager.LoadScene(gameObject.scene.buildIndex,LoadSceneMode.Single);}

        public OrbitBreakerCombatConfig Config => config;
        public GameState State { get; private set; } = GameState.Ready;
        public bool IsPlaying => State == GameState.Playing;
        public event Action<GameState> StateChanged;

        private bool restartRequested;

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape)){HandleEscape();return;}
            if (!EncyclopediaOpen && State == GameState.Ready &&
                (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
                StartGame();
            else if (Input.GetKeyDown(KeyCode.R))
                RestartGame();
        }

        public bool StartGame()
        {
            if (State != GameState.Ready || config == null || restartRequested)
                return false;
            SetState(GameState.Playing);
            return true;
        }

        public bool WinGame() => EndGame(GameState.Victory);
        public bool LoseGame() => EndGame(GameState.Defeat);

        private bool EndGame(GameState result)
        {
            if (!IsPlaying || restartRequested)
                return false;
            SetState(result);
            return true;
        }

        public bool RestartGame()
        {
            if (restartRequested || (State != GameState.Victory && State != GameState.Defeat))
                return false;

            // Reload this manager's scene, not whichever additive scene happens to be active.
            var scene = gameObject.scene;
            if (scene.buildIndex < 0)
            {
                Debug.LogError("OrbitBreakerBattle must be enabled in Build Settings before restarting.", this);
                return false;
            }

            pendingMode=Mode;
            Reload();
            return true;
        }

        private void SetState(GameState next)
        {
            State = next;
            StateChanged?.Invoke(next);
        }
    }
}
