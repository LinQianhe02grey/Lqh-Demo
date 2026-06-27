using UnityEngine;
using Cardwin.Combat;

namespace Cardwin.Enemies
{
    public class EnemyHealthBarUI : MonoBehaviour
    {
        private Health _health;
        private Transform _target;

        [Header("Offset")]
        public Vector3 worldOffset = new Vector3(0f, 1.2f, 0f);

        private GUIStyle _style;
        private bool _initialized;

        private void Start()
        {
            _target = transform;
            _health = GetComponent<Health>();

            if (_health == null)
            {
                _health = GetComponentInParent<Health>();
                if (_health != null)
                    _target = _health.transform;
            }

            if (_health == null)
            {
                Debug.LogWarning($"[EnemyHealthBarUI] No Health component found on {gameObject.name}. Self-disabling.");
                enabled = false;
                return;
            }

            _initialized = true;
        }

        private void Update()
        {
            if (!_initialized || _health == null || _health.IsDead())
            {
                Destroy(this);
            }
        }

        private void OnGUI()
        {
            if (!_initialized || _health == null || Camera.main == null)
                return;

            if (_health.IsDead())
            {
                Destroy(this);
                return;
            }

            if (Time.timeScale <= 0f)
                return;

            Vector3 worldPos = _target.position + worldOffset;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            if (screenPos.z < 0f)
                return;

            float y = Screen.height - screenPos.y;

            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.label);
                _style.alignment = TextAnchor.MiddleCenter;
                _style.fontSize = 12;
                _style.normal.textColor = Color.white;
                _style.fontStyle = FontStyle.Bold;
            }

            float barWidth = 80f;
            float barHeight = 8f;
            float x = screenPos.x - barWidth * 0.5f;

            float healthPercent = (float)_health.currentHealth / _health.maxHealth;
            Color hpColor = healthPercent > 0.5f ? Color.green : healthPercent > 0.25f ? Color.yellow : Color.red;

            GUI.Box(new Rect(x - 1, y - 1, barWidth + 2, barHeight + 2), "");
            GUI.DrawTexture(new Rect(x, y, barWidth * healthPercent, barHeight), Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 1f, hpColor, 0f, 0f);

            string hpText = $"{_health.currentHealth}/{_health.maxHealth}";
            Vector2 labelSize = _style.CalcSize(new GUIContent(hpText));
            _style.normal.textColor = Color.white;
            GUI.Label(new Rect(screenPos.x - labelSize.x * 0.5f, y + barHeight + 2f, labelSize.x, labelSize.y), hpText, _style);

            if (_health.currentBlock > 0)
            {
                float shieldY = y + barHeight + 4f + labelSize.y;
                float shieldBarHeight = 5f;

                GUI.Box(new Rect(x - 1, shieldY - 1, barWidth + 2, shieldBarHeight + 2), "");
                GUI.DrawTexture(new Rect(x, shieldY, barWidth, shieldBarHeight), Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 1f, Color.cyan, 0f, 0f);

                string shieldText = $"SH {_health.currentBlock}";
                Vector2 shLabelSize = _style.CalcSize(new GUIContent(shieldText));
                _style.normal.textColor = Color.cyan;
                GUI.Label(new Rect(screenPos.x - shLabelSize.x * 0.5f, shieldY + shieldBarHeight + 2f, shLabelSize.x, shLabelSize.y), shieldText, _style);
            }
        }
    }
}
