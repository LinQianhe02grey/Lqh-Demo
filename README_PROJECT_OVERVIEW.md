# README_PROJECT_OVERVIEW.md — Cardwin Unity Demo 项目总览

> 生成时间：2026-06-01 | Stage 12A（Stage 11A base + 11B/C/D + 12A）
> **目标读者**：项目外开发者 / 新加入的团队成员

---

## 1. 项目一句话简介

**Cardwin** 是一个 2D 横向卷轴卡牌战斗 Demo，玩家使用卡牌驱动战斗（射击/防御/治疗/+Focus），搭载 8 发弹夹、善恶装填规则、连击评分、击杀奖励三选一等机制。

---

## 2. 当前核心玩法

- 玩家在 2D 灰盒地图中向右推进
- 按 B 键打开背包编辑 8 格 Loadout
- 左键使用当前卡牌效果（对外射击或自用）
- 右键使用当前卡牌效果（对自己）
- R 键手动换弹
- 消灭 6 个敌人（3 近战 + 3 远程）后推进到 FinishGate
- 每次击杀弹窗三选一奖励卡

---

## 3. 当前操作方式

| 按键 | 功能 |
|---|---|
| A/D | 左右移动 |
| Space | 跳跃（二段跳） |
| LeftShift | 冲刺（无敌） |
| 鼠标左键 | 使用当前卡牌左键效果 |
| 鼠标右键 | 使用当前卡牌右键效果 |
| R | 手动换弹 |
| B | 打开/关闭背包编辑面板 |
| Esc | 关闭背包 / 打开暂停菜单（非背包状态时） |
| Pause Menu | Resume / Save to Slot X / Main Menu / Quit |

### 主菜单操作

| 操作 | 说明 |
|------|------|
| New Game | 进入 SaveSelectPanel，选择槽位开始新游戏 |
| Continue | 进入 SaveSelectPanel，选择已有存档槽继续 |
| Quit | 退出游戏 |
| SaveSelect Back | 返回主界面 |

### 死亡后操作

| 操作 | 说明 |
|------|------|
| Retry | 重新开始 Demo_Combat（不删存档） |
| Load Save | 从当前存档槽读档恢复 |
| Main Menu | 返回主界面 |
| Quit | 退出游戏 |

---

## 4. 场景入口

- **主菜单**：`Assets/Scenes/MainMenu.unity`（Build Index 0）
  - MainPanel：New Game / Continue / Quit
  - SaveSelectPanel：3 存档槽（Continue/Overwrite/Delete）→ Back 返回主界面
  - ConfirmPanel：Delete/Overwrite 确认弹窗
- **战斗场景**：`Assets/Scenes/Demo_Combat.unity`（Build Index 1）
- 备用场景：`Assets/Scenes/SampleScene.unity`（Unity 默认，不使用）

存档路径：`Application.persistentDataPath/cardwin_save_slot_1.json` ~ `slot_3.json`

---

## 5. 核心系统列表

| 系统 | 核心脚本 | 职责 |
|---|---|---|
| Player | `PlayerController2D.cs` | 输入/移动/跳跃/冲刺/射击 |
| Health | `Health.cs` | HP/Shield/Damage/Heal/Death (Player+Enemies共用，Player死亡不Destroy) |
| Cards | `CardData.cs`, `CardDatabase.cs`, `CardEffectExecutor.cs` | 卡牌数据/数据库/效果执行 |
| Magazine | `MagazineSystem.cs` | 8发弹夹/随机装弹/换弹/Loadout |
| Inventory | `InventorySystem.cs` | 背包存储/测试库存 |
| Enemies | `MeleeEnemyController.cs`, `RangedEnemyController.cs`, `EnemyProjectile.cs` | 敌人AI/子弹 |
| Combat | `ComboRatingSystem.cs`, `PlayerAlignment.cs`, `RewardManager.cs` | 连击/善恶/奖励 |
| Camera | `CameraFollow2D.cs` | 平滑跟随玩家 |
| UI | `CombatHUD.cs`, `MagazineEditUI.cs`, `MagazinePreviewUI.cs`, `CardSlotUI.cs`, `EnemyHealthBarUI.cs`, `MainMenuController.cs`, `PauseMenuController.cs`, `GameOverController.cs` | 战斗HUD/背包面板/弹夹预览/敌人血条/主菜单/暂停菜单/死亡界面 |
| Save | `SaveSystem.cs`, `GameSaveData.cs`, `SaveSlotInfo.cs` | JSON 三存档槽系统 |
| Core | `GameFlowManager.cs` | 全局流程管理（DontDestroyOnLoad） |

---

## 6. 脚本目录结构

```
Assets/Scripts/
├── Analytics/     — BattleLogger.cs (stub)
├── Camera/        — CameraFollow2D.cs
├── Cards/         — CardData, CardDatabase, CardEffectExecutor, etc.
├── Combat/        — PlayerController2D, Health, Projectile, etc.
├── Core/          — GameManager, GameStateMachine, etc. (多为stub)
├── Enemies/       — MeleeEnemyController, RangedEnemyController, EnemyProjectile, EnemyHealthBarUI
├── Inventory/     — InventorySystem.cs
├── Magazine/      — MagazineSystem.cs
├── Shop/          — ShopManager, EconomySystem (stub)
└── UI/            — CombatHUD, MagazineEditUI, etc.

Assets/Editor/Cardwin/
├── CardLibraryWindow.cs       — 卡牌管理窗口
├── CardCsvImporter.cs         — CSV 导入工具
├── CardDatabaseEditorUtility.cs — Database 重建
├── CardConfigValidator.cs     — 配置合法性检查
├── CardAssetCreator.cs        — 旧版卡牌创建 (Legacy)
└── CardwinSceneBuilder.cs     — 场景重建 (Disabled)
```

---

## 7. 数据目录结构

```
Assets/Data/
├── Cards/
│   ├── CardDatabase.asset         — 卡牌总表（运行时唯一入口）
│   ├── C001_Strike.asset          — 正式卡 1
│   ├── C002_Pierce.asset          — 正式卡 2
│   ├── C003_Burst.asset           — 正式卡 3
│   ├── C004_Guard.asset           — 正式卡 4
│   ├── C005_Heal.asset            — 正式卡 5
│   ├── C006_Focus.asset           — 正式卡 6
│   ├── C007_Evil_Shot.asset       — 正式卡 7
│   ├── C008_Mercy_Shield.asset    — 正式卡 8
│   ├── C009_Combo_Spark.asset     — 正式卡 9
│   ├── C010_Quick_Reload.asset    — 正式卡 10
│   ├── C011_Weakness_Mark.asset   — 正式卡 11
│   ├── C012_Aerial_Mark.asset     — 正式卡 12
│   └── Strike/Guard/Heal/Focus.asset — 旧资产（与 C001/C004/C005/C006 重复）
└── CardImport/
    ├── bullets.csv                — 卡牌数据源
    └── CardValidationReport.txt   — 自动生成的检查报告
```

---

## 8. 卡牌配置流程

1. 编辑 `Assets/Data/CardImport/bullets.csv`（Excel 导出为 CSV）
2. 点击 `Tools > Cardwin > Import Cards From CSV`
3. 导入后自动调用 `Rebuild Card Database`
4. 点击 `Tools > Cardwin > Validate Card Configs` 检查配置合法性
5. 点击 `Tools > Cardwin > Card Library` 浏览和管理卡牌

---

## 9. 如何导入卡牌

- 确保 `bullets.csv` 在当前路径：`Assets/Data/CardImport/bullets.csv`
- 点击 `Tools > Cardwin > Import Cards From CSV`
- CSV 列：CardID, CardName, Type, Rarity, UseTarget, LeftEffect, RightEffect, Damage, Block, Heal, FocusGain, GoodCost, EvilCost, Value, ValueUnit, CooldownLimit, Role, RiskNotes, Description

---

## 10. 如何验证卡牌配置

- 点击 `Tools > Cardwin > Validate Card Configs`
- 检查 Console 输出的检查 summary
- 打开 `Assets/Data/CardImport/CardValidationReport.txt` 查看详细报告

---

## 11. 如何打开 Card Library

- 点击 `Tools > Cardwin > Card Library`
- 打开 EditorWindow：搜索/筛选/禁用/删除/同步数据库/创建新卡/导入 CSV

---

## 12. 如何运行 Demo

1. 打开 `Assets/Scenes/Demo_Combat.unity`
2. 点击 Unity Play 按钮
3. 首次按 B 打开背包，库存初始化为每种卡 20 发
4. A/D 移动，Space 跳跃，鼠标左/右键使用卡牌，R 换弹
5. 消灭 6 个敌人，通关 FinishGate

---

## 13. 当前已知问题

1. **4 个未实现卡牌效果**：`WeaknessMark`、`QuickReload`、`ComboSpark`、`AerialMark` 的 CardEffectExecutor.ApplyEffectToTarget 空操作
2. **旧资产重复**：`Strike.asset`/`Guard.asset`/`Heal.asset`/`Focus.asset` 与 C001/C004/C005/C006 语义重复，CardDatabase 中同时存在
3. **GameManager/GameStateMachine 未实现**：场景切换/状态管理无实际逻辑
4. **Shop 系统未开发**：ShopManager/EconomySystem/ShopUI 均为 stub
5. **BattleLogger 未接入**：战斗日志未在运行时使用
6. **4 张卡 implemented=true 但效果未实现**：C009~C012 使用空操作
7. **UI 运行时创建较多**：CombatHUD 和 MagazineEditUI 在 Awake/Start 中大量新建 GameObject，可考虑 Prefab 化

---

## 14. 禁止事项

| 禁止操作 | 原因 |
|---|---|
| 运行 `Tools > Cardwin > Rebuild Clean Demo Scene` | SceneBuilder 已禁用，会弹窗阻止 |
| 直接删除 Assets/Data/Cards 下的旧资产 | 可能导致 CardDatabase 引用丢失 |
| 绕过 CardEffectExecutor 直接操作 Health | 破坏卡牌效果唯一性 |
| 手动修改 CardDatabase.asset 不重建 | 需通过 Rebuild Card Database 同步 |
| 删除或覆盖 Demo_Combat.unity | 主场景，不可重建 |
| 修改 MagazineSystem 核心逻辑 | 影响弹夹/装弹系统稳定性 |
| 修改 CardEffectExecutor 效果规则 | 影响所有卡牌行为 |

---

## 15. 下一步开发建议

| 优先级 | 任务 |
|---|---|
| **P0** | 实现 WeaknessMark/QuickReload/ComboSpark/AerialMark 效果逻辑 |
| **P0** | 从 CardDatabase 移除旧 Strike/Guard/Heal/Focus 资产引用 |
| **P1** | 清理 Legacy 对象（Enemy_Test_OLD, 高台平台, CameraBounds 等） |
| **P1** | 实现 GameManager/GameStateMachine（主菜单→战斗→GameOver） |
| **P2** | 开发 Shop 系统（商店买卖/货币） |
| **P2** | 接入 BattleLogger（战斗数据分析） |
| **P3** | UI Prefab 化（减少运行时创建） |
| **P3** | 美术资源替换（placeholders → 正式素材） |

---

## 16. 参考文档

- `SYSTEM_INDEX.md` — 系统索引（详细脚本/函数/状态表）
- `PROJECT_SCRIPT_INDEX.md` — 所有 46 个脚本总表
- `PROJECT_FUNCTION_INDEX.md` — 核心函数级索引
- `CARD_SYSTEM_AUDIT.md` — 卡牌系统唯一性审计
- `ACTOR_ARCHITECTURE_AUDIT.md` — 角色属性架构审计
- `ENEMY_SYSTEM_AUDIT.md` — 敌人系统审计
- `UI_SYSTEM_AUDIT.md` — UI 系统审计
- `SCENE_STRUCTURE_AUDIT.md` — 场景对象审计
- `CARDWIN_TOOLS_AUDIT.md` — 编辑器工具审计
- `CLEANUP_PLAN.md` — 清理计划
- `DEVELOPMENT_LOG.md` — 开发日志（完整历史）
- `TODO.md` — 任务清单
- `AGENTS.md` — 开发约束与规范
