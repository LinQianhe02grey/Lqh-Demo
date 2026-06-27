using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cardwin.Modules
{
    /// <summary>
    /// Shatters / scatters / fades the normal combat HUD when the rhythm module
    /// activates. It operates ONLY on the main HUD Canvas (the one hosting
    /// CombatHUD); menus (Pause / GameOver / Settings / Bag) and the rhythm canvas
    /// are skipped so retry / pause still work. Major HUD elements spawn a burst of
    /// scattering fragments and then fade out (never an instant SetActive(false)).
    /// Purely additive; does not modify any existing UI script.
    /// </summary>
    public sealed class CombatUIBreakController : MonoBehaviour
    {
        private sealed class Fragment
        {
            public RectTransform rect;
            public Image image;
            public Vector2 velocity;
            public float angularVelocity;
            public float life;
            public float maxLife;
            public Color startColor;
        }

        private static readonly string[] SkipNameKeywords =
        {
            "Pause", "GameOver", "Setting", "Bag", "Edit", "Rhythm", "Confession", "UIBreakFragments"
        };

        private readonly List<Fragment> _fragments = new List<Fragment>();
        private readonly List<GameObject> _hiddenObjects = new List<GameObject>();
        private readonly List<CanvasGroup> _fadedGroups = new List<CanvasGroup>();

        private Canvas _hudCanvas;
        private RectTransform _fragmentRoot;
        private static Sprite _quadSprite;

        [Header("Performance (anti-stutter)")]
        [Tooltip("Hard cap on total scatter fragments across all UI elements.")]
        [SerializeField] private int maxTotalFragments = 40;
        [Tooltip("Fragments spawned per UI element (also bounded by the remaining total budget).")]
        [SerializeField] private int fragmentsPerElement = 6;

        public bool HasBroken { get; private set; }

        public void BreakNormalCombatUI()
        {
            if (HasBroken) return;
            HasBroken = true;

            _hudCanvas = ResolveHudCanvas();
            if (_hudCanvas == null)
            {
                Debug.LogWarning("[CombatUIBreak] No HUD canvas found; nothing to break.");
                return;
            }

            EnsureFragmentRoot();

            float t0 = Time.realtimeSinceStartup;
            var targets = new List<RectTransform>();
            var canvasRect = _hudCanvas.GetComponent<RectTransform>();
            for (int i = 0; i < canvasRect.childCount; i++)
            {
                var child = canvasRect.GetChild(i) as RectTransform;
                if (child == null) continue;
                if (!child.gameObject.activeSelf) continue;
                if (ShouldSkip(child.name)) continue;
                if (child == _fragmentRoot) continue;
                targets.Add(child);
            }
            Debug.Log($"[ConfessionNightModule] BreakCombatUI collect cost={(Time.realtimeSinceStartup - t0) * 1000f:F1} ms, targets={targets.Count}, canvas={_hudCanvas.name}");

            // Spread fragment spawning across frames (one element per frame) to avoid a hitch.
            StartCoroutine(BreakRoutine(targets));
        }

        private System.Collections.IEnumerator BreakRoutine(List<RectTransform> targets)
        {
            yield return null;   // never spawn on the F-press frame

            int budget = Mathf.Max(0, maxTotalFragments);
            for (int i = 0; i < targets.Count; i++)
            {
                var element = targets[i];
                if (element == null) continue;
                float t0 = Time.realtimeSinceStartup;
                int spawned = ShatterElement(element, budget);
                budget -= spawned;
                Debug.Log($"[ConfessionNightModule] SpawnFragments({element.name}) count={spawned} budgetLeft={budget} cost={(Time.realtimeSinceStartup - t0) * 1000f:F1} ms");
                yield return null;
            }
        }

        private bool ShouldSkip(string objName)
        {
            for (int i = 0; i < SkipNameKeywords.Length; i++)
            {
                if (objName.IndexOf(SkipNameKeywords[i], System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }

        private int ShatterElement(RectTransform element, int budget)
        {
            Color sampleColor = SampleColor(element);

            Vector3[] corners = new Vector3[4];
            element.GetWorldCorners(corners);
            Vector3 center = (corners[0] + corners[2]) * 0.5f;
            float width = Mathf.Abs(corners[2].x - corners[0].x);
            float height = Mathf.Abs(corners[2].y - corners[0].y);
            float spread = Mathf.Clamp(Mathf.Max(width, height) * 0.5f, 30f, 260f);

            int count = Mathf.Clamp(fragmentsPerElement, 0, Mathf.Max(0, budget));
            for (int i = 0; i < count; i++)
            {
                Vector3 spawn = center + new Vector3(
                    Random.Range(-width, width) * 0.5f,
                    Random.Range(-height, height) * 0.5f,
                    0f);
                SpawnFragment(spawn, sampleColor, spread);
            }

            // Always fade the original element out (CanvasGroup) instead of instant hide,
            // even when the fragment budget is exhausted.
            var cg = element.GetComponent<CanvasGroup>();
            if (cg == null) cg = element.gameObject.AddComponent<CanvasGroup>();
            _fadedGroups.Add(cg);
            _hiddenObjects.Add(element.gameObject);
            StartCoroutine(FadeAndHide(cg, element.gameObject));
            return count;
        }

        private System.Collections.IEnumerator FadeAndHide(CanvasGroup cg, GameObject go)
        {
            float t = 0f;
            const float dur = 0.5f;
            float start = cg.alpha;
            Vector3 baseScale = go.transform.localScale;
            while (t < dur && cg != null)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / dur);
                cg.alpha = Mathf.Lerp(start, 0f, k);
                go.transform.localScale = baseScale * Mathf.Lerp(1f, 0.85f, k);
                yield return null;
            }
            if (cg != null) cg.alpha = 0f;
            if (go != null) go.SetActive(false);
        }

        private void SpawnFragment(Vector3 worldPos, Color color, float spread)
        {
            var go = new GameObject("UIFragment", typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(_fragmentRoot, false);
            float size = Random.Range(10f, 26f);
            rect.sizeDelta = new Vector2(size, size);
            rect.position = worldPos;
            rect.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

            var img = go.GetComponent<Image>();
            img.sprite = GetQuadSprite();
            img.raycastTarget = false;
            Color c = color;
            c.a = 1f;
            img.color = c;

            float angle = Random.Range(0f, Mathf.PI * 2f);
            float speed = Random.Range(spread * 1.5f, spread * 4f);
            var frag = new Fragment
            {
                rect = rect,
                image = img,
                velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * 0.6f + 0.4f) * speed,
                angularVelocity = Random.Range(-360f, 360f),
                life = 0f,
                maxLife = Random.Range(0.6f, 1.1f),
                startColor = c
            };
            _fragments.Add(frag);
        }

        private void Update()
        {
            if (_fragments.Count == 0) return;

            float dt = Time.deltaTime;
            const float gravity = -900f;
            for (int i = _fragments.Count - 1; i >= 0; i--)
            {
                var f = _fragments[i];
                if (f.rect == null) { _fragments.RemoveAt(i); continue; }

                f.life += dt;
                f.velocity.y += gravity * dt;
                f.rect.position += (Vector3)(f.velocity * dt);
                f.rect.Rotate(0f, 0f, f.angularVelocity * dt);

                float k = Mathf.Clamp01(f.life / f.maxLife);
                if (f.image != null)
                {
                    Color c = f.startColor;
                    c.a = Mathf.Lerp(1f, 0f, k);
                    f.image.color = c;
                }

                if (f.life >= f.maxLife)
                {
                    if (f.rect != null) Destroy(f.rect.gameObject);
                    _fragments.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Safety restore (used on death / scene exit). NOT called while looping in
        /// rhythm mode (the rhythm spec keeps the normal UI hidden during loop).
        /// </summary>
        public void RestoreNormalCombatUI()
        {
            foreach (var go in _hiddenObjects)
                if (go != null) go.SetActive(true);
            foreach (var cg in _fadedGroups)
                if (cg != null) cg.alpha = 1f;
            _hiddenObjects.Clear();
            _fadedGroups.Clear();
            HasBroken = false;
            Debug.Log("[CombatUIBreak] Normal combat UI restored.");
        }

        private Canvas ResolveHudCanvas()
        {
            var hud = FindObjectOfType<Cardwin.UI.CombatHUD>();
            if (hud != null)
            {
                var canvas = hud.GetComponentInParent<Canvas>();
                if (canvas != null) return canvas.rootCanvas != null ? canvas.rootCanvas : canvas;
            }

            // Fallback: first overlay canvas that is not the rhythm canvas.
            var canvases = FindObjectsOfType<Canvas>();
            foreach (var c in canvases)
            {
                if (c == null) continue;
                if (c.name.IndexOf("Rhythm", System.StringComparison.OrdinalIgnoreCase) >= 0) continue;
                if (c.renderMode == RenderMode.ScreenSpaceOverlay)
                    return c.rootCanvas != null ? c.rootCanvas : c;
            }
            return null;
        }

        private void EnsureFragmentRoot()
        {
            if (_fragmentRoot != null) return;
            var go = new GameObject("UIBreakFragments", typeof(RectTransform));
            _fragmentRoot = go.GetComponent<RectTransform>();
            _fragmentRoot.SetParent(_hudCanvas.transform, false);
            _fragmentRoot.anchorMin = Vector2.zero;
            _fragmentRoot.anchorMax = Vector2.one;
            _fragmentRoot.offsetMin = Vector2.zero;
            _fragmentRoot.offsetMax = Vector2.zero;
            _fragmentRoot.SetAsLastSibling();
        }

        private static Color SampleColor(RectTransform element)
        {
            var img = element.GetComponentInChildren<Image>();
            if (img != null) return img.color;
            var txt = element.GetComponentInChildren<Text>();
            if (txt != null) return txt.color;
            return new Color(0.85f, 0.85f, 0.95f, 1f);
        }

        private static Sprite GetQuadSprite()
        {
            if (_quadSprite != null) return _quadSprite;
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            var pixels = new Color[16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            _quadSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 100f);
            return _quadSprite;
        }
    }
}
