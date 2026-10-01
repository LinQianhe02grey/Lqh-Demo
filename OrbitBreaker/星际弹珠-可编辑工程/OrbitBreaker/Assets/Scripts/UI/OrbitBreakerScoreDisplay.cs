using UnityEngine;

namespace OrbitBreaker
{
    // Single HUD for score, ability state and event feedback. Never drives player physics.
    [DisallowMultipleComponent]
    public sealed class OrbitBreakerScoreDisplay : MonoBehaviour
    {
        [SerializeField] private OrbitBreakerScoreManager scoreManager;
        private OrbitBreakerPlayerMotor player;
        private OrbitBreakerFlipperController left, right;
        private Font font;
        private GUIStyle small, text, heading, number, centered;
        private int previousScore;
        private float feedbackUntil, hitUntil;
        private string feedback;
        private Color feedbackColor;
        private bool subscribed;
        private static readonly Color Cyan = new Color(0.25f, 0.86f, 1f);
        private static readonly Color Green = new Color(0.25f, 1f, 0.7f);
        private static readonly Color Red = new Color(1f, 0.3f, 0.38f);
        private static readonly Color Gold = new Color(1f, 0.83f, 0.35f);
        private static readonly Color Muted = new Color(0.57f, 0.68f, 0.76f);
        private OrbitBreakerGameManager Manager => scoreManager == null ? null : scoreManager.OrbitBreakerGameManager;
        private string InactiveText => Manager != null && Manager.State != GameState.Ready ? "本局已结束" : "等待开局";
        public string FeedbackText => Time.time < feedbackUntil ? feedback : "";
        public string StateText => Manager == null ? "" : Manager.State == GameState.Ready ? "准备就绪" :
            Manager.State == GameState.Victory ? "挑战成功" : Manager.State == GameState.Defeat ? "护壳耗尽" :
            player == null ? "战斗中" : player.IsFocusActive ? $"慢动作 {player.FocusRemaining:0.0}s · Space转向" : player.IsStunned ? "受击失控" : player.IsAttached ? (player.IsAttackActive ? "高速绕转 · 无敌" : "绕转加速中") :
            player.IsInJackpot ? "奖励洞充能" : player.HasRewardBoost ? (player.IsSlowed ? "加速中 · 粘稠减速" : $"红色加速 {player.RewardRemaining:0.0}s") : player.IsSlowed ? "粘稠领域 · 减速" :
            player.IsPiercing ? "最高速 · 贯穿" : player.IsAttackActive ? "高速 · 无敌" : player.IsInvulnerable ? "恢复保护" : "自由移动";
        public string DashText => Manager == null || !Manager.IsPlaying ? InactiveText : player == null ? "不可用" :
            player.IsStunned ? "失控中，暂不可用" : player.IsDashing ? "冲刺中" : player.DashCooldownRemaining > 0f ?
            $"冷却 {player.DashCooldownRemaining:0.00} 秒" : !player.HasAttachmentAim ? "请将鼠标移入游戏画面" :
            player.IsAttached ? "就绪 · 朝鼠标发射" : "就绪 · 朝鼠标冲刺";
        public string MagnetText
        {
            get
            {
                if (Manager == null || !Manager.IsPlaying) return InactiveText;
                if (player == null) return "不可用";
                if (player.IsStunned) return "失控中，暂不可用";
                if (player.IsAttached) return $"剩余 {player.AttachmentRemaining:0.0} 秒";
                if (player.AttachmentCooldownRemaining > 0f) return $"冷却 {player.AttachmentCooldownRemaining:0.0} 秒";
                if (player.CanAttach(OrbitBreakerAttachmentPoint.Nearest(player.Position))) return "可吸附 · 按 E";
                return player.MustLeaveAttachment ? "先离开该吸附点再回来" : "靠近绿柱按 E · 自动绕转";
            }
        }
        public bool ChineseFontAvailable => font != null && font.HasCharacter('吸');

        private void OnEnable()
        {
            player = FindObjectOfType<OrbitBreakerPlayerMotor>();
            foreach (var flipper in FindObjectsOfType<OrbitBreakerFlipperController>())
                if (flipper.Side == FlipperSide.Left) left = flipper; else right = flipper;
            if (scoreManager == null || Manager == null || player == null) return;
            previousScore = scoreManager.Score;
            scoreManager.ScoreChanged += OnScore;
            Manager.StateChanged += OnState;
            player.HitReceived += OnHit;
            player.DashStarted += OnDash;
            player.BoardImpulseApplied += OnRescue;
            player.OrbitRepelled += OnRepel;
            player.DashChargeEarned += OnCharge;
            player.ShellRestored += OnRestore;
            player.RewardStarted += OnReward;
            subscribed = true;
        }
        private void OnDisable()
        {
            if (subscribed)
            {
                if (scoreManager != null) scoreManager.ScoreChanged -= OnScore;
                if (Manager != null) Manager.StateChanged -= OnState;
                if (player != null)
                {
                    player.HitReceived -= OnHit;
                    player.DashStarted -= OnDash;
                    player.BoardImpulseApplied -= OnRescue;
                    player.OrbitRepelled -= OnRepel;
                    player.DashChargeEarned -= OnCharge;
                    player.ShellRestored -= OnRestore;
                    player.RewardStarted -= OnReward;
                }
            }
            subscribed = false;
            feedbackUntil = hitUntil = 0f;
        }
        private void OnDestroy() { if (font != null) Destroy(font); }
        private void Notify(string message, Color color) { feedback = message; feedbackColor = color; feedbackUntil = Time.time + 1.1f; }
        private void OnScore(int total)
        {
            int delta = total - previousScore;
            previousScore = total;
            if (delta > 0) Notify($"+{delta}  得分！", Gold);
        }
        private void OnHit() { Notify($"护壳破裂 · 剩余 {player.Shells} 层", Red); hitUntil = Time.time + 0.22f; }
        private void OnDash() => Notify("冲刺！", Cyan);
        private void OnRescue() => Notify($"连弹 {player.ComboCount}  ×{player.ComboMultiplier:0.00}", Gold);
        private void OnRestore()=>Notify("补盾 +1",Green);
        private void OnReward()=>Notify("奖励洞！高分 + 红色加速",Red);
        private void OnCharge() => Notify("+1 冲刺 · 鼠标瞄准 + Space", Cyan);
        private void OnRepel() => Notify("旋转击退！", Green);
        private void OnState(GameState state) { if (state != GameState.Playing) { feedbackUntil = hitUntil = 0f; } }
        private GUIStyle Style(int size, Color color, bool bold = false)
        {
            var style = new GUIStyle(GUI.skin.label) { font = font, fontSize = size, fontStyle = bold ? FontStyle.Bold : FontStyle.Normal,
                wordWrap = true, padding = new RectOffset(0, 0, 0, 0) };
            style.normal.textColor = color;
            return style;
        }
        private void EnsureStyles()
        {
            if (text != null) return;
            font = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei", "SimHei", "Arial" }, 20);
            font.hideFlags = HideFlags.DontSave;
            small = Style(14, Muted);
            text = Style(17, Color.white);
            heading = Style(18, Color.white, true);
            number = Style(34, Color.white, true);
            centered = Style(17, Color.white);
            centered.alignment = TextAnchor.MiddleCenter;
        }
        private static void Fill(Rect area, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(area, Texture2D.whiteTexture);
            GUI.color = previous;
        }
        private static void Panel(float x, float y, float w, float h, Color accent)
        {
            Fill(new Rect(x, y, w, h), new Color(0.025f, 0.045f, 0.075f, 0.94f));
            Fill(new Rect(x, y, 3, h), accent);
        }
        private static void Bar(float x, float y, float w, float amount, Color color)
        {
            Fill(new Rect(x, y, w, 5), new Color(0.13f, 0.2f, 0.25f));
            Fill(new Rect(x, y, w * Mathf.Clamp01(amount), 5), color);
        }
        private void Label(float x, float y, float w, float h, string value, GUIStyle style) => GUI.Label(new Rect(x, y, w, h), value, style);
        private string FlipperText(OrbitBreakerFlipperController flipper)
        {
            if (Manager == null || !Manager.IsPlaying) return InactiveText;
            if (flipper == null) return "不可用";
            if (flipper.CooldownRemaining > 0f) return $"{flipper.CooldownRemaining:0.00}s";
            return flipper.ContainsPlayer() ? "可救球" : "球未进入区域";
        }
        private void OnGUI()
        {
            if (Manager == null || player == null || !Manager.IsPlaying) return;
            EnsureStyles();
            Matrix4x4 previous = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280f * scale) * 0.5f, (Screen.height - 720f * scale) * 0.5f, 0f),
                Quaternion.identity, Vector3.one * scale);
            Panel(18,18,260,250,Gold);
            Label(30,27,215,25,$"积分 {scoreManager.Score:D3}    ×{player.ComboMultiplier:0.00}",heading);
            Bar(30,59,210,(float)scoreManager.Score/scoreManager.TargetScore,Gold);
            Label(30,70,215,27,$"碰撞充能  {player.DashContactProgress}/4",text);
            string ready=player.DashCooldownRemaining>0?$"冷却 {player.DashCooldownRemaining:0.0}s":player.DashCharges>0?"Space · 朝鼠标发射":"碰撞4次 / 击杀1敌 → +1冲刺";
            Label(30,103,215,25,ready,small);
            Label(30,130,215,26,StateText,small);
            string orbit=player.IsAttached?$"吸附 {player.AttachmentRemaining:0.0}s · 蓄力 {player.AttachmentCharge:P0}":$"连弹 {player.ComboCount} · 攻击 {player.AttackDamage}";
            Label(30,161,215,25,orbit,small);
            Label(30,191,215,25,$"防护壳  {new string('●', player.Shells)}{new string('○', OrbitBreakerPlayerMotor.MaximumShells-player.Shells)}",text);
            Label(30,226,240,30,$"速度 {player.CurrentSpeed:0.0} / {player.BaseSpeedLimit*player.LaunchSpeedFactor:0.0}",heading);
            var locked=player.GetComponent<OrbitBreakerTargetLock>();
            Label(285,680,930,26,locked!=null&&locked.HasTarget?"已锁定 · Space发射  |  右键慢动作2秒  |  点击空白取消":"WASD移动 · E绕转 · 左键锁定 / Space发射 · 右键慢动作 · Esc返回",centered);
            if(player.IsFocusActive)Bar(400,645,480,player.FocusRemaining/OrbitBreakerPlayerMotor.FocusDuration,Cyan);
            if (FeedbackText.Length > 0)
            {
                Label(400, 5, 480, 30, FeedbackText, centered);
            }
            if (Time.time < hitUntil)
            {
                Color edge = new Color(1f, 0.12f, 0.2f, 0.55f * (hitUntil - Time.time) / 0.22f);
                Fill(new Rect(0, 0, 1280, 7), edge); Fill(new Rect(0, 713, 1280, 7), edge);
                Fill(new Rect(0, 0, 7, 720), edge); Fill(new Rect(1273, 0, 7, 720), edge);
            }
            GUI.matrix = previous;
        }
    }
}

