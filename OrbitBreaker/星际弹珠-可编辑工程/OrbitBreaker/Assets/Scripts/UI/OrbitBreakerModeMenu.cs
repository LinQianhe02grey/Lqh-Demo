using UnityEngine;
namespace OrbitBreaker
{
    [RequireComponent(typeof(OrbitBreakerGameManager))]
    public sealed class OrbitBreakerModeMenu : MonoBehaviour
    {
        private OrbitBreakerGameManager manager;
        private OrbitBreakerPlayerMotor player;
        private Font font;
        private GUIStyle title,heading,body,small,button;
        private Vector2 scroll;
        private int category;
        private readonly OrbitBreakerCatalogIllustrations illustrations=new OrbitBreakerCatalogIllustrations();
        public static readonly Rect HudRect=new Rect(18,18,260,250);
        private void Awake(){manager=GetComponent<OrbitBreakerGameManager>();player=FindObjectOfType<OrbitBreakerPlayerMotor>();}
        public static Vector2 CanvasPoint(Vector2 screen)
        {
            float s=Mathf.Min(Screen.width/1280f,Screen.height/720f);
            return new Vector2((screen.x-(Screen.width-1280*s)/2)/s,(Screen.height-screen.y-(Screen.height-720*s)/2)/s);
        }
        public static bool IsPointerOverUI(Vector2 screen)=>HudRect.Contains(CanvasPoint(screen)) || CanvasPoint(screen).y>666;
        private GUIStyle Style(int size,Color color,bool bold=false)
        {var s=new GUIStyle(GUI.skin.label){font=font,fontSize=size,wordWrap=true,fontStyle=bold?FontStyle.Bold:FontStyle.Normal};s.normal.textColor=color;return s;}
        private void Styles()
        {
            if(title!=null)return;
            font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","SimHei","Arial"},20);
            title=Style(44,Color.white,true);heading=Style(24,new Color(.5f,.91f,1),true);body=Style(18,Color.white);small=Style(15,new Color(.6f,.7f,.8f));
            button=new GUIStyle(GUI.skin.button){font=font,fontSize=20,padding=new RectOffset(14,14,10,10)};
        }
        private static void Fill(Rect rect,Color color){var old=GUI.color;GUI.color=color;GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=old;}
        private void OnGUI()
        {
            if(manager==null || player==null)return;Styles();var previous=GUI.matrix;
            float s=Mathf.Min(Screen.width/1280f,Screen.height/720f);
            GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1280*s)/2,(Screen.height-720*s)/2),Quaternion.identity,Vector3.one*s);
            if(manager.EncyclopediaOpen)DrawCatalog();
            else if(manager.State==GameState.Ready)DrawMain();
            else if(!manager.IsPlaying)DrawEnd();
            else if(manager.Mode==GameMode.OrbitBreakerBattle)DrawBoss();
            GUI.matrix=previous;
        }
        private void DrawMain()
        {
            Fill(new Rect(0,0,1280,720),new Color(.015f,.025f,.065f,.82f));
            GUI.Label(new Rect(120,85,950,64),"星际弹珠",title);
            GUI.Label(new Rect(123,156,1000,35),"ORBIT BREAKER  /  用速度撕开深空",heading);
            Card(120,"自由模式","探索随机宇宙，借柱子连续弹射。\n无限积分，三层护壳，尽情练习高速穿敌。",GameMode.Free);
            Card(660,"战斗模式","迎战深渊章鱼。躲开触手与激光预警，\n蓄满绕转、高速撞击，击败Boss获胜。",GameMode.OrbitBreakerBattle);
            if(GUI.Button(new Rect(120,540,300,55),"星际图鉴 · 数值与说明",button)){manager.EncyclopediaOpen=true;scroll=Vector2.zero;}
            if(GUI.Button(new Rect(900,540,260,55),"退出  /  Esc",button))manager.HandleEscape();
            GUI.Label(new Rect(120,628,1060,40),"WASD 移动 · E 自动绕转 · 左键锁定 / Space 发射 · 右键慢动作2秒 · Esc 返回",small);
        }
        private void Card(float x,string name,string description,GameMode mode)
        {
            Fill(new Rect(x,240,500,245),new Color(.035f,.065f,.12f,.97f));
            Fill(new Rect(x,240,4,245),mode==GameMode.Free?Color.cyan:new Color(.9f,.35f,1));
            GUI.Label(new Rect(x+25,262,450,38),name,heading);
            GUI.Label(new Rect(x+25,319,450,70),description,body);
            if(GUI.Button(new Rect(x+25,408,450,52),"开始"+name,button))manager.StartMode(mode);
        }
        private void DrawEnd()
        {
            Fill(new Rect(0,0,1280,720),new Color(.015f,.025f,.065f,.72f));
            Fill(new Rect(380,235,520,270),new Color(.035f,.06f,.1f,.98f));
            GUI.Label(new Rect(410,258,470,66),manager.State==GameState.Victory?"深渊章鱼已击败":"护壳耗尽",heading);
            GUI.Label(new Rect(410,323,470,45),"本局积分  "+GetComponent<OrbitBreakerScoreManager>().Score,body);
            if(GUI.Button(new Rect(410,385,460,44),"再来一局  /  R",button))manager.RestartGame();
            if(GUI.Button(new Rect(410,440,460,44),"返回主界面  /  Esc",button))manager.ReturnToMenu();
        }
        private void DrawBoss()
        {
            var boss=GetComponent<OrbitBreakerBossDirector>().Boss;if(boss==null)return;
            Fill(new Rect(400,35,530,9),new Color(.12f,.045f,.15f));
            Fill(new Rect(400,35,530*(float)boss.Health/boss.HealthCapacity,9),new Color(.88f,.25f,.7f));
            GUI.Label(new Rect(400,49,560,29),$"深渊章鱼   {boss.Health} / {boss.HealthCapacity}",body);
            Vector2 direction=boss.transform.position-player.Position;
            string location=direction.y>=0?"上":"下";location+=direction.x>=0?"右":"左";
            GUI.Label(new Rect(400,79,530,26),$"Boss 在{location}方 {direction.magnitude:0.0}m  ·  高速命中后反弹",small);
        }
        public string[] CatalogEntries(int page)
        {
            var c=manager.Config;var r=manager.Rules;
            if(page==0)return new[]{
                $"速度规则|普通移动上限 {OrbitBreakerEncounterConfig.ManualSpeed} m/s；实际速度 > {r.damageThreshold} 才能伤敌并免普通伤害。{r.maximumSpeed} m/s 贯穿普通敌人；Boss始终反弹。",
                $"伤害计算|基础伤害 = {r.thresholdDamage} + (速度−{r.damageThreshold}) / ({r.maximumSpeed}−{r.damageThreshold}) × {r.maximumSpeedDamage-r.thresholdDamage}，再乘连弹倍率，取整。阈值及以下为0。奖励超速继续增伤。",
                $"冲刺与防护|基础速度 {c.dashSpeed}，冷却 {c.dashCooldown:0.##}s，最多 {c.maxDashCharges} 次；4碰撞/1击杀补1次。起步0.3s保速；首次命中半径2.4m爆炸，额外40%伤害。三层壳，黑洞仍扣壳。",
                $"连弹与锁定|8s内连弹+0.25倍，最高×3。左键锁定下一次Space。右键0.2倍慢动作持续真实2s；Space消耗次数保留当前速度转向，并结束慢动作。冷却和胶囊规则不变。",
                $"自由 / 战斗|自由模式无限刷分，{c.targetScore}为积分里程碑。战斗模式仅击败Boss获胜。局内Esc结束本局并回主菜单；主菜单Esc退出（Editor中停止Play）。"};
            if(page==1)return new[]{
                "青色反弹柱|基础弹速14 m/s，沿接触径向弹开；快速补充连弹。",
                "紫色反弹柱|基础弹速17 m/s，方向叠加0.28侧向偏转，更容易切换路线。",
                "金色反弹柱|基础弹速21 m/s，方向叠加0.25向上偏转，适合建立高速。",
                $"绿色吸附柱|E范围 {OrbitBreakerAttachmentPoint.CaptureRadius}m，轨道 {OrbitBreakerAttachmentPoint.OrbitRadius}m；6→{r.maximumSpeed}m/s，用 {r.orbitRampSeconds}s加速，最多 {c.attachDuration}s。镜头固定柱心，Space消耗1次发射。离柱后消失；绕转免敌人推挤/减速。",
                "浮动弹板|弹速20 m/s；位移频率1.7 rad/s；转动幅度22°、频率1.5 rad/s。",
                "旋转器 / 摆锤|弹速23 m/s，加0.45切向与0.2向上分量。旋转器126°/s；摆锤±80°、2.8 rad/s。Boss可摧毁这些机关及柱子。",
                "奖励洞 / 金色星标|一次性：捕获后洞消失，600基础分×连弹；0.65s后弹出。7s内冲刺/弹射及最高速×1.35，带红色特效。不同洞只刷新奖励时长。",
                "黑洞 / 得分靶 / 平台|一次性黑洞核心半径0.8m，球心进入扣1壳并20速弹出后消失，洞伤害全局间隔2s。靶向上速度≥6得50/100基础分，0.5s冷却。平台仅装饰。"};
            return new[]{
                $"红色幽灵敌人|生命 {c.enemyHealth}，移动2.2m/s；穿过环境。攻击冷却1.2s，普通碰撞扣1壳。受伤间隔0.35s，击杀80基础分并补1次冲刺。",
                $"黑色粘稠敌人|生命 {c.enemyHealth}；每3只刷新1只。半径 {OrbitBreakerSlimeAura.Radius}m，移动/普通冲刺×{OrbitBreakerSlimeAura.SpeedFactor}；不叠加，离开恢复，吸附免减速。",
                $"敌群 / 补盾|每 {c.enemySpawnInterval}s尝试刷1只，上限12；距玩家11–15m出生，40m外回收。击杀掉盾概率 {c.shieldDropChance:P0}；绿色+补1壳，上限3，20s回收，满壳不消耗。",
                $"深渊章鱼 Boss|生命 {r.bossHealth}，速度 {r.bossMoveSpeed}m/s；身体半径 {OrbitBreakerOctopusBoss.Radius}m。沿路摧毁弹柱/吸附柱/浮动板/旋转器。受伤间隔 {OrbitBreakerOctopusBoss.HitCooldown}s；每次有效撞击反向弹开玩家。",
                $"召唤触手|Boss每 {r.bossAttackInterval}s交替使用触手或激光；3处触手，预警 {OrbitBreakerOctopusBoss.TentacleWarning}s，活跃 {OrbitBreakerOctopusBoss.TentacleDuration}s；半径 {OrbitBreakerOctopusBoss.TentacleRadius}m（另计球体0.4），命中扣1壳，高速可免疫。",
                $"深空激光|锁定预警时的射线，长度 {OrbitBreakerOctopusBoss.LaserRange}m；预警 {OrbitBreakerOctopusBoss.LaserWarning}s，攻击 {OrbitBreakerOctopusBoss.LaserDuration}s，线宽0.7m（伤害距离含球半径0.75）。每目标1.2s冷却，高速可免疫。"};
        }
        private void DrawCatalog()
        {
            Fill(new Rect(0,0,1280,720),new Color(.015f,.025f,.065f,.97f));
            GUI.Label(new Rect(95,45,650,60),"星际图鉴",title);
            if(GUI.Button(new Rect(1020,55,165,45),"返回  /  Esc",button))manager.EncyclopediaOpen=false;
            string[] tabs={"战斗规则","柱子与机关","敌人与Boss"};
            for(int i=0;i<3;i++)if(GUI.Button(new Rect(95+i*365,130,350,46),(category==i?"● ":"")+tabs[i],button)){category=i;scroll=Vector2.zero;}
            var entries=CatalogEntries(category);
            float[] heights=new float[entries.Length];float total=0;
            for(int i=0;i<entries.Length;i++){heights[i]=Mathf.Max(132,body.CalcHeight(new GUIContent(entries[i].Split('|')[1]),845)+65);total+=heights[i]+10;}
            scroll=GUI.BeginScrollView(new Rect(95,198,1090,450),scroll,new Rect(0,0,1065,total));
            float y=0;
            for(int i=0;i<entries.Length;i++)
            {
                var parts=entries[i].Split('|');Fill(new Rect(0,y,1065,heights[i]),new Color(.04f,.075f,.13f));
                GUI.DrawTexture(new Rect(14,y+12,160,120),illustrations.Get(category,i),ScaleMode.ScaleToFit);
                GUI.Label(new Rect(195,y+10,845,33),parts[0],heading);GUI.Label(new Rect(195,y+48,845,heights[i]-52),parts[1],body);
                y+=heights[i]+10;
            }
            GUI.EndScrollView();GUI.Label(new Rect(95,667,1000,32),"数值来自当前配置；速度单位 m/s。鼠标滚轮浏览。",small);
        }
        private void OnDestroy(){illustrations.Dispose();if(font!=null)Destroy(font);}
    }
}
