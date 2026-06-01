using UnityEngine;

namespace Cardwin.Core
{
    [DefaultExecutionOrder(-1000)]
    public class DemoSceneRuntimeBootstrapper : MonoBehaviour
    {
        [Header("Object Names")]
        public string playerName = "Player";
        public string cameraName = "MainCamera";
        public string spawnPointName = "SpawnPoint_Player";
        public string enemyNameContains = "Enemy";

        [Header("Camera Settings")]
        public Vector3 cameraOffset = new Vector3(0f, 1.5f, -10f);
        public float cameraSmoothTime = 0.15f;
        public bool disableCameraFollowBounds = true;

        [Header("Ground Check")]
        public float groundCheckRadius = 0.2f;
        public Vector3 groundCheckLocalPos = new Vector3(0f, -0.55f, 0f);

        [Header("Spawn")]
        public bool placePlayerAtSpawnOnAwake = true;
        public float spawnSafetyOffset = 0.08f;
        public float spawnRaycastHeight = 5f;
        public float spawnRaycastDistance = 20f;

        [Header("Toggles")]
        public bool disableBlockingPlaceholders = true;
        public bool forceEnemyTrigger = true;
        public bool forceGroundNoRigidbody = true;
        public bool logColliderReport = true;

        private GameObject _player;
        private GameObject _camera;
        private GameObject _spawnPoint;
        private int _playerLayer;
        private int _enemyLayer;
        private int _groundLayer;

        private void Awake()
        {
            ResolveLayers();
            FindCoreObjects();
            ConfigureCamera();
            ConfigureGroundAndPlatforms();
            ConfigurePlayer();
            ConfigureEnemy();
            DisableBlockingPlaceholders();
            IgnorePlayerEnemyCollision();
            if (logColliderReport)
                PrintColliderReport();
        }

        private void ResolveLayers()
        {
            _playerLayer = LayerMask.NameToLayer("Player");
            _enemyLayer = LayerMask.NameToLayer("Enemy");
            _groundLayer = LayerMask.NameToLayer("Ground");

            if (_playerLayer < 0)
                Debug.LogError("[SceneBootstrapper] Layer 'Player' not found. Create it in Project Settings > Tags and Layers.");
            if (_enemyLayer < 0)
                Debug.LogError("[SceneBootstrapper] Layer 'Enemy' not found. Create it in Project Settings > Tags and Layers.");
            if (_groundLayer < 0)
                Debug.LogError("[SceneBootstrapper] Layer 'Ground' not found. Create it in Project Settings > Tags and Layers.");
        }

        private void FindCoreObjects()
        {
            _camera = GameObject.Find(cameraName);
            _player = GameObject.Find(playerName);
            _spawnPoint = GameObject.Find(spawnPointName);

            if (_camera == null)
                Debug.LogError($"[SceneBootstrapper] Camera '{cameraName}' not found in scene.");
            if (_player == null)
                Debug.LogError($"[SceneBootstrapper] Player '{playerName}' not found in scene.");
            if (_spawnPoint == null)
                Debug.LogWarning($"[SceneBootstrapper] Spawn point '{spawnPointName}' not found in scene.");
        }

        private void ConfigureCamera()
        {
            if (_camera == null)
                return;

            Vector3 pos = _camera.transform.position;
            pos.z = -10f;
            _camera.transform.position = pos;

            if (_player != null)
            {
                var follow = _camera.GetComponent<Cardwin.Cameras.CameraFollow2D>();
                if (follow == null)
                    follow = _camera.AddComponent<Cardwin.Cameras.CameraFollow2D>();

                follow.target = _player.transform;
                follow.offset = cameraOffset;
                follow.smoothTime = cameraSmoothTime;

                if (disableCameraFollowBounds)
                    follow.useBounds = false;

                Debug.Log("[SceneBootstrapper] CameraFollow2D target assigned: Player");
            }
        }

        private void ConfigurePlayer()
        {
            if (_player == null)
                return;

            _player.tag = "Player";

            if (_playerLayer >= 0)
                _player.layer = _playerLayer;

            Rigidbody2D rb = _player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = 3f;
                rb.freezeRotation = true;
                if ((rb.constraints & RigidbodyConstraints2D.FreezePositionY) != 0)
                    rb.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
            }

            Collider2D col = _player.GetComponent<Collider2D>();
            if (col != null)
                col.isTrigger = false;

            var controller = _player.GetComponent<Cardwin.Combat.PlayerController2D>();
            if (controller != null)
            {
                if (_groundLayer >= 0)
                    controller.groundLayer = 1 << _groundLayer;

                controller.groundCheckRadius = groundCheckRadius;

                Transform gc = _player.transform.Find("GroundCheck");
                if (gc == null)
                {
                    GameObject gcObj = new GameObject("GroundCheck");
                    gcObj.transform.SetParent(_player.transform);
                    gcObj.transform.localPosition = groundCheckLocalPos;
                    gc = gcObj.transform;
                }
                controller.groundCheck = gc;
            }

            PlacePlayerAtSpawn(rb);

            Debug.Log("[SceneBootstrapper] Player configured: Tag=Player, Layer=Player, groundLayer=Ground, GroundCheck created");
        }

        private void PlacePlayerAtSpawn(Rigidbody2D rb)
        {
            if (!placePlayerAtSpawnOnAwake || _player == null || _spawnPoint == null)
                return;

            if (_spawnPoint.transform.IsChildOf(_player.transform))
                _spawnPoint.transform.SetParent(null, true);

            Vector3 spawnPos = _spawnPoint.transform.position;
            spawnPos.z = 0f;
            spawnPos.y = ResolveSpawnY(spawnPos.x, spawnPos.y);

            _spawnPoint.transform.position = spawnPos;
            _player.transform.position = spawnPos;

            if (rb != null)
                rb.velocity = Vector2.zero;

            Debug.Log($"[SceneBootstrapper] Player placed at spawn: {spawnPos}");
        }

        private float ResolveSpawnY(float x, float fallbackY)
        {
            float bottomOffset = 0.9f;
            Collider2D playerCollider = _player != null ? _player.GetComponent<Collider2D>() : null;
            if (playerCollider != null)
                bottomOffset = _player.transform.position.y - playerCollider.bounds.min.y;

            if (_groundLayer < 0)
                return fallbackY;

            Vector2 origin = new Vector2(x, fallbackY + spawnRaycastHeight);
            float rayDistance = spawnRaycastHeight + spawnRaycastDistance;
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayDistance, 1 << _groundLayer);

            if (hit.collider == null)
            {
                Debug.LogWarning($"[SceneBootstrapper] No ground found below spawn x={x:F2}; using authored spawn y.");
                return fallbackY;
            }

            return hit.point.y + bottomOffset + spawnSafetyOffset;
        }

        private void ConfigureGroundAndPlatforms()
        {
            if (_groundLayer < 0)
                return;

            GameObject[] allObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            int configuredCount = 0;

            foreach (GameObject root in allObjects)
            {
                ConfigureGroundRecursive(root, ref configuredCount);
            }

            Debug.Log($"[SceneBootstrapper] Ground/Platform objects configured: {configuredCount}");
        }

        private void ConfigureGroundRecursive(GameObject obj, ref int count)
        {
            if (_player != null && (obj == _player || obj.transform.IsChildOf(_player.transform)))
                return;

            string lower = obj.name.ToLower();
            bool isGround = lower == "ground" || lower.Contains("platform");

            if (isGround)
            {
                obj.layer = _groundLayer;

                Collider2D col = obj.GetComponent<Collider2D>();
                if (col != null)
                {
                    col.isTrigger = false;
                }
                else
                {
                    BoxCollider2D box = obj.AddComponent<BoxCollider2D>();
                    box.isTrigger = false;

                    SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                    if (sr != null && sr.drawMode == SpriteDrawMode.Simple)
                    {
                        box.size = sr.bounds.size;
                    }
                }

                if (forceGroundNoRigidbody)
                {
                    Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
                    if (rb != null)
                        Destroy(rb);
                }

                count++;
            }

            foreach (Transform child in obj.transform)
                ConfigureGroundRecursive(child.gameObject, ref count);
        }

        private void ConfigureEnemy()
        {
            GameObject[] allObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            int configuredCount = 0;

            foreach (GameObject root in allObjects)
            {
                ConfigureEnemyRecursive(root, ref configuredCount);
            }

            Debug.Log($"[SceneBootstrapper] Enemy objects configured: {configuredCount}");
        }

        private void ConfigureEnemyRecursive(GameObject obj, ref int count)
        {
            bool isEnemy = obj.name.ToLower().Contains(enemyNameContains.ToLower());

            if (isEnemy)
            {
                if (_enemyLayer >= 0)
                    obj.layer = _enemyLayer;

                bool hasMeleeController = obj.GetComponent<Cardwin.Enemies.MeleeEnemyController>() != null;
                bool hasRangedController = obj.GetComponent<Cardwin.Enemies.RangedEnemyController>() != null;
                bool hasNewController = hasMeleeController || hasRangedController;

                if (hasNewController)
                {
                    if (forceEnemyTrigger)
                    {
                        Collider2D[] colliders = obj.GetComponents<Collider2D>();
                        foreach (Collider2D col in colliders)
                            col.isTrigger = true;
                    }
                }
                else
                {
                    if (forceEnemyTrigger)
                    {
                        Collider2D[] colliders = obj.GetComponents<Collider2D>();
                        foreach (Collider2D col in colliders)
                            col.isTrigger = true;
                    }

                    Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
                    if (rb == null)
                        rb = obj.AddComponent<Rigidbody2D>();

                    rb.bodyType = RigidbodyType2D.Kinematic;
                    rb.freezeRotation = true;
                }

                Rigidbody2D rb2 = obj.GetComponent<Rigidbody2D>();
                if (rb2 != null)
                    rb2.freezeRotation = true;

                count++;
            }

            foreach (Transform child in obj.transform)
                ConfigureEnemyRecursive(child.gameObject, ref count);
        }

        private void DisableBlockingPlaceholders()
        {
            if (!disableBlockingPlaceholders)
                return;

            string[] placeholderNames = {
                "CameraBounds", "SpawnPoint_Player", "SpawnPoint_Enemy"
            };

            foreach (string name in placeholderNames)
            {
                GameObject obj = GameObject.Find(name);
                if (obj == null)
                    continue;

                Collider2D[] colliders = obj.GetComponents<Collider2D>();
                foreach (Collider2D col in colliders)
                {
                    col.enabled = false;
                }

                foreach (Transform child in obj.transform)
                {
                    Collider2D[] childColliders = child.GetComponents<Collider2D>();
                    foreach (Collider2D col in childColliders)
                        col.enabled = false;
                }

                Debug.Log($"[SceneBootstrapper] Disabled colliders on: {name}");
            }

            GameObject bossDoor = GameObject.Find("BossDoor_Placeholder");
            if (bossDoor != null)
            {
                Collider2D[] bossColliders = bossDoor.GetComponents<Collider2D>();
                foreach (Collider2D col in bossColliders)
                    col.isTrigger = true;

                foreach (Transform child in bossDoor.transform)
                {
                    Collider2D[] childColliders = child.GetComponents<Collider2D>();
                    foreach (Collider2D col in childColliders)
                        col.isTrigger = true;
                }

                Debug.Log("[SceneBootstrapper] BossDoor_Placeholder colliders set to Trigger");
            }
        }

        private void IgnorePlayerEnemyCollision()
        {
            if (_playerLayer < 0 || _enemyLayer < 0)
                return;

            Physics2D.IgnoreLayerCollision(_playerLayer, _enemyLayer, true);
            Debug.Log("[SceneBootstrapper] IgnoreLayerCollision Player-Enemy = true");
        }

        private void PrintColliderReport()
        {
            Debug.Log("===== Scene Collider Report (Bootstrapper) =====");

            Collider2D[] allColliders = FindObjectsByType<Collider2D>(FindObjectsSortMode.None);

            foreach (Collider2D col in allColliders)
            {
                GameObject go = col.gameObject;
                string layerName = LayerMask.LayerToName(go.layer);
                string triggerTag = col.isTrigger ? "[TRIGGER]" : "[SOLID]";
                string enabledTag = col.enabled ? "" : "[DISABLED]";
                string bodyType = "";

                Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
                if (rb != null)
                    bodyType = $"  BodyType={rb.bodyType}";

                Debug.Log($"  {go.name}  Layer={layerName}  {triggerTag} {enabledTag} {col.GetType().Name}{bodyType}");
            }

            Debug.Log("===== End Report =====");
        }
    }
}
