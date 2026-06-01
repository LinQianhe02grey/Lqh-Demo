# PROJECT_SCRIPT_INDEX.md — 项目脚本总表

> 生成时间：2026-06-01 | Stage 11A — Project Architecture Audit
> 脚本总数：46 C# 文件（38 Runtime + 6 Editor + 2 数据 asset）

---

## 目录结构

```
Assets/
├── Editor/Cardwin/          (6 cs)  — 编辑器工具
├── Scripts/
│   ├── Analytics/           (1 cs)  — 战斗数据统计
│   ├── Camera/              (1 cs)  — 摄像机跟随
│   ├── Cards/               (8 cs)  — 卡牌数据定义 / 效果执行
│   ├── Combat/              (9 cs)  — 玩家移动 / 战斗 / 伤害 / 连击 / 奖励
│   ├── Core/                (5 cs)  — 游戏入口 / 全局状态 / 场景配置
│   ├── Enemies/             (4 cs)  — 敌人 AI / 子弹 / HP UI
│   ├── Inventory/           (1 cs)  — 背包存储
│   ├── Magazine/            (2 cs)  — 弹夹管理
│   ├── Shop/                (2 cs)  — 商店 / 货币
│   └── UI/                  (8 cs)  — HUD / 背包 / 预览 / 商店
├── Prefabs/
│   ├── Enemies/             (3 prefab)
│   └── Projectiles/         (1 prefab)
├── Data/Cards/              (17 asset)
├── Data/CardImport/         (2 data + 1 report)
├── Art/Player/              (1 png)
└── Scenes/                  (2 unity)
```

---

## 脚本总表

| # | 脚本路径 | 类名 | 命名空间 | 类型 | 所属系统 | 主要职责 | 关键字段 | 关键函数 | 被谁引用/挂载 | 当前状态 | 备注 |
|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | `Scripts/Core/GameState.cs` | `GameState` | `Cardwin.Core` | Enum | Core | 游戏状态枚举 | — | — | GameManager, GameStateMachine | Data Only | MainMenu/Playing/Paused/GameOver/Victory |
| 2 | `Scripts/Core/GameManager.cs` | `GameManager` | `Cardwin.Core` | Runtime MB | Core | 全局单例，场景加载，状态管理 | `Instance`, `CurrentState` | `SetState()`, `LoadScene()`, `RestartCurrentLevel()`, `QuitGame()` | Scene Root | Stub | 骨架完成，方法为空实现 |
| 3 | `Scripts/Core/GameStateMachine.cs` | `GameStateMachine` + `IGameStateHandler` | `Cardwin.Core` | Runtime MB + Interface | Core | 状态机，注册处理器，状态切换通知 | — | `TransitionTo()`, `RegisterHandler<T>()` | GameManager | Stub | 骨架完成 |
| 4 | `Scripts/Core/DemoSceneRuntimeBootstrapper.cs` | `DemoSceneRuntimeBootstrapper` | `Cardwin.Core` | Runtime MB | Core | Legacy safety bootstrapper：运行时自动配置场景 | 18x Header fields | `ResolveLayers()`, `ConfigureCamera()`, `ConfigurePlayer()`, `ConfigureGroundAndPlatforms()`, `ConfigureEnemy()`, etc. | (Not mounted on LevelRoot in Demo_Combat) | Legacy | Stage 8A.5 后不再挂载到正式关卡层级；保留但标记 Legacy |
| 5 | `Scripts/Core/FinishGateTrigger.cs` | `FinishGateTrigger` | `Cardwin.Core` | Runtime MB | Core | 关卡终点触发 | `clearMessage` | `OnTriggerEnter2D()` | LevelRoot/FinishGate | Active | Demo 通关闭环 |
| 6 | `Scripts/Combat/PlayerController2D.cs` | `PlayerController2D` | `Cardwin.Combat` | Runtime MB | Player | 玩家控制：移动/跳跃/二段跳/冲刺/卡牌射击 | 15x Header fields | `Move()`, `Jump()`, `StartDash()`, `SetInputLocked()`, `IsGrounded()`, `Shoot()` | Player GameObject | Active | 核心逻辑，含 Magazine/Inventory/Combo/Input |
| 7 | `Scripts/Combat/Health.cs` | `Health` | `Cardwin.Combat` | Runtime MB | Combat | 通用血量：血/格挡/受击/治疗/死亡/无敌 | `maxHealth`, `currentHealth`, `currentBlock`, `OnDeath` | `TakeDamage()`, `Heal()`, `GainBlock()`, `IsDead()`, `Die()`, `SetInvincible()` | Player, All Enemies | Active | 共用组件，Player 和 Enemy 通用 |
| 8 | `Scripts/Combat/EnemyController.cs` | `EnemyController` + `EnemyBehavior` | `Cardwin.Combat` | Runtime MB + Enum | Enemies | **[LEGACY]** 旧版组合敌人控制器(Melee+Ranged) | `behavior`, `contactDamage`, `shootRange` | `Awake()`, `Start()`, `Update()`, `TryDamagePlayer()`, `FireAtPlayer()` | Enemy_Test_OLD (disabled) | Legacy | 已被 MeleeEnemyController/RangedEnemyController 取代 |
| 9 | `Scripts/Combat/Projectile.cs` | `Projectile` | `Cardwin.Combat` | Runtime MB | Combat | 玩家子弹：效果投射/命中过滤/视觉兜底 | `speed`, `lifetime`, `damage` | `Init(damage)`, `Init(card,effect,context)`, `HandleHit()` | PlayerController2D / CardEffectExecutor | Active | Kinematic Rigidbody2D, Trigger+Collision双路径 |
| 10 | `Scripts/Combat/DamageInfo.cs` | `DamageInfo` | `Cardwin.Combat` | Data Struct | Combat | 伤害数据结构 | `amount`, `focusBonus`, `sourceCardId` | `TotalDamage` (property) | Combat 系统 | Data Only | 简单 struct |
| 11 | `Scripts/Combat/PlayerAlignment.cs` | `PlayerAlignment` | `Cardwin.Combat` | Runtime MB | Player | 玩家善恶属性(Good/Evil) | `good`, `evil` | `SetGood()`, `SetEvil()`, `SetValues()` | Player GameObject | Active | Good=4 Evil=4; 影响 Loadout 装填规则 |
| 12 | `Scripts/Combat/ComboRatingSystem.cs` | `ComboRatingSystem` | `Cardwin.Combat` | Runtime MB | Combat | 连击评分(D/C/B/A) / 5s超时清零 | — | `RegisterCardUse()`, `ResetCombo()` | Player GameObject | Active | 基于 CardUseTarget 判断正确使用 |
| 13 | `Scripts/Combat/RewardManager.cs` | `RewardManager` | `Cardwin.Combat` | Runtime MB | Rewards | 击杀敌人→三选一奖励卡→AddCard到 Inventory | — | `OnEnemyKilled()`, `SelectCard()`, `OnGUI()` | Player GameObject | Active | 暂停战斗(timeScale=0)显示 OnGUI 面板 |
| 14 | `Scripts/Combat/SceneCollisionReporter.cs` | `SceneCollisionReporter` | `Cardwin.Combat` | Runtime MB | Level | 运行时 Debug：输出场景所有 Collider 信息 | `reportOnStart`, `reportKey` | `ReportSceneColliders()` | Canvas / DebugHolder | Active | 开发者调试，F1 触发 |
| 15 | `Scripts/Cards/CardData.cs` | `CardData` + `CardUseTarget` | `Cardwin.Cards` | ScriptableObject + Enum | Cards | 卡牌数据资产(ScriptableObject)：所有字段 | `cardId`, `cardName`, `cardType`, `rarity`, `icon`, `damage`, `block`, `heal`, `focusGain`, `leftClickEffect`, `rightClickEffect`, `useTarget`, `goodCost`, `evilCost`, `finalValue`, `enabled`, `implemented`, etc. | `IsOffensive` (computed property) | CardDatabase, CardEffectExecutor, MagazineSystem, UI | Active | 唯一卡牌数据源 |
| 16 | `Scripts/Cards/CardType.cs` | `CardType` | `Cardwin.Cards` | Enum | Cards | 卡牌类型 | — | — | CardData | Data Only | Attack/Defense/Support/Debuff/Heal/Utility |
| 17 | `Scripts/Cards/CardRarity.cs` | `CardRarity` | `Cardwin.Cards` | Enum | Cards | 稀有度 | — | — | CardData | Data Only | Common/Rare/Epic |
| 18 | `Scripts/Cards/CardEffectType.cs` | `CardEffectType` | `Cardwin.Cards` | Enum | Cards | 效果类型 | — | — | CardData, CardEffectExecutor | Data Only | None/Damage/Block/Heal/Focus/WeaknessMark/QuickReload/ComboSpark/AerialMark |
| 19 | `Scripts/Cards/CardDatabase.cs` | `CardDatabase` | `Cardwin.Cards` | ScriptableObject | Cards | 子弹功能总表：索引allCards/Dictionary缓存/查询/随机抽取/校验 | `allCards` | `Initialize()`, `GetById()`, `GetByName()`, `GetByType()`, `GetByRarity()`, `GetByEffect()`, `GetRandomCard()`, `GetRandomCards()`, `ValidateDatabase()` | MagazineSystem, RewardManager, InventorySystem | Active | 运行时获取卡牌的唯一入口 |
| 20 | `Scripts/Cards/CardRuntimeInstance.cs` | `CardRuntimeInstance` | `Cardwin.Cards` | Data Class | Cards | 运行时卡牌实例，包装 CardData + 升级等级 | — | `CardId`, `DisplayName` (properties) | MagazineSystem, InventorySystem | Active | — |
| 21 | `Scripts/Cards/CardEffectExecutor.cs` | `CardEffectExecutor` | `Cardwin.Cards` | Runtime MB | Cards | 卡牌效果执行器：左键生成Projectile/右键自用/ApplyEffectToTarget统一施加 | — | `Initialize()`, `ExecuteLeft()`, `ExecuteRight()`, `ApplyEffectToTarget()` | Player GameObject | Active | Damage/Block/Heal/Focus 唯一执行入口 |
| 22 | `Scripts/Cards/PlayerCardContext.cs` | `PlayerCardContext` | `Cardwin.Cards` | Data Class | Cards | 运行时上下文：Player引用/Health/FirePoint/Focus/鼠标方向 | `player`, `firePoint`, `playerHealth`, `focusStacks` | `AddFocus()`, `ConsumeFocusMultiplier()`, `GetShootDirectionToMouse()` | CardEffectExecutor | Active | — |
| 23 | `Scripts/Enemies/MeleeEnemyController.cs` | `MeleeEnemyController` + `EnemyState` | `Cardwin.Enemies` | Runtime MB + Enum | Enemies | 近战AI：巡逻/追击/攻击/返回 + Kinematic防重合 | `patrolSpeed`, `chaseSpeed`, `aggroRange`, `attackRange`, `stopDistance`, `contactDamage` | `ChaseAndAttack()`, `Patrol()`, `TryDamagePlayer()`, `FindPlayer()` | LevelRoot/Enemies 下 3 个 MeleeEnemy prefab 实例 | Active | 正式近战实现，含状态机 |
| 24 | `Scripts/Enemies/RangedEnemyController.cs` | `RangedEnemyController` | `Cardwin.Enemies` | Runtime MB | Enemies | 远程AI：水平巡逻/发射EnemyProjectile(prefab)/Kinematic/floating | `patrolSpeed`, `shootRange`, `fireCooldown`, `projectileSpeed`, `projectileDamage`, `isFlying` | `HorizontalPatrol()`, `FireAtPlayer()`, `FindPlayer()`, `OnDrawGizmosSelected()` | LevelRoot/Enemies 下 3 个 RangedEnemy prefab 实例 | Active | 正式远程实现，g=0 悬浮 |
| 25 | `Scripts/Enemies/EnemyProjectile.cs` | `EnemyProjectile` | `Cardwin.Enemies` | Runtime MB | Enemies | 敌人子弹：Dynamic velocity飞行/可见sprite/命中Player扣血/撞墙销毁 | — | `Init(dir, damage, speed)`, `HandleHit()`, `CheckManualHit()` | RangedEnemyController | Active | sortingOrder=150, Trigger+Overlap双路径 |
| 26 | `Scripts/Enemies/EnemyHealthBarUI.cs` | `EnemyHealthBarUI` | `Cardwin.Enemies` | Runtime MB | Enemies | 敌人HP/Shield UI (OnGUI: HP绿/黄/红条 + SH蓝条) | `worldOffset` | `OnGUI()` | 所有敌人 (prefab 预设) | Active | 编辑模式可见的 HP bar |
| 27 | `Scripts/Magazine/MagazineSystem.cs` | `MagazineSystem` | `Cardwin.Magazine` | Runtime MB | Magazine | 8发弹夹：随机装弹/消耗/换弹/预览/事件/Loadout管理 | `capacity`, `reloadTime`, `initialCards`, `cardDatabase`, `shuffleOnReload` | `GetCurrentCard()`, `HasUsableCurrentCard()`, `UseLeft()`, `UseRight()`, `ManualReload()`, `BuildRandomMagazine()`, `SetLoadoutCards()`, `InitializeDefaultLoadoutIfEmpty()` | Player GameObject | Active | Fisher-Yates 洗牌；事件驱动 UI 更新 |
| 28 | `Scripts/Magazine/MagazineSlot.cs` | `MagazineSlot` | `Cardwin.Magazine` | Data Class | Magazine | 弹夹预览槽位数据结构 | `index`, `cardId`, `displayName` | `SetCard()`, `Clear()` | MagazinePreviewUI | Active | [Serializable] |
| 29 | `Scripts/Inventory/InventorySystem.cs` | `InventorySystem` + `InventoryEntry` | `Cardwin.Inventory` | Runtime MB + Data Class | Inventory | 背包存储：增删查/测试库存/聚合统计/持久化 | `ownedCards`, `defaultDatabase`, `useTestStock` | `AddCard()`, `AddCards()`, `RemoveCard()`, `GetCardCounts()`, `ResetToTestStock()`, `InitializeForRun()`, `GetOwnedTotalCount()` | Player GameObject | Active | 每种正式卡 20 发测试库存 |
| 30 | `Scripts/Shop/ShopManager.cs` | `ShopManager` | `Cardwin.Shop` | Runtime MB | Shop | 商店管理：6货位、刷新、买卖 | `shopSlotCount`, `refreshCost` | `RefreshShop()`, `BuyItem()`, `SellItem()` | (not yet wired) | Stub | 骨架完成，方法为空 |
| 31 | `Scripts/Shop/EconomySystem.cs` | `EconomySystem` | `Cardwin.Shop` | Runtime MB | Shop | 货币系统：加减金钱、支付判定 | `currency` | `AddCurrency()`, `SpendCurrency()`, `CanAfford()` | ShopManager | Stub | CanAfford() 有实现，其余 stub |
| 32 | `Scripts/UI/CombatHUD.cs` | `CombatHUD` | `Cardwin.UI` | Runtime MB | UI | 战斗HUD总控：HP/Shield/Focus+Combo+3发预览+Reload进度 | — | `Awake()`, `Start()`, `Update()`, `RefreshHUD()`, `RefreshReloadProgress()`, `BindSystems()` | Canvas | Active | 运行时自动创建 UI 层级 |
| 33 | `Scripts/UI/HUDRuntimeBootstrapper.cs` | `HUDRuntimeBootstrapper` | `Cardwin.UI` | Runtime MB | UI | 运行时自动保证 Canvas 有 CombatHUD | — | `Awake()` | Canvas (ExecuteBefore) | Active | [DefaultExecutionOrder(-900)] |
| 34 | `Scripts/UI/MagazinePreviewUI.cs` | `MagazinePreviewUI` | `Cardwin.UI` | Runtime MB | UI | 弹夹下3发预览：3个PreviewSlot(150x60) | `previewCount` | `Bind()`, `RefreshPreview()` | CombatHUD | Active | 订阅 MagazineSystem 事件 |
| 35 | `Scripts/UI/MagazineFullBarUI.cs` | `MagazineFullBarUI` | `Cardwin.UI` | Runtime MB | UI | **保留给未来背包界面** — 完整8发弹夹显示 | `slotCount` | `Bind()`, `RefreshFullBar()` | (future) | Legacy / Retained | 战斗HUD不再使用；保留给未来用 |
| 36 | `Scripts/UI/MagazineEditUI.cs` | `MagazineEditUI` + `BagTab` | `Cardwin.UI` | Runtime MB + Enum | UI | 背包/弹夹编辑界面：5分页/Apply/Cancel/Clear/AutoFill | `inventorySystem`, `magazineSystem`, `cardDatabase` | `Toggle()`, `Open()`, `Close()`, `SwitchTab()`, `Apply()`, `CancelEdit()`, `ClearLoadout()`, `AutoFill()` | Canvas | Active | B键打开，1380x820 面板 |
| 37 | `Scripts/UI/CardSlotUI.cs` | `CardSlotUI` | `Cardwin.UI` | Runtime MB | UI | 单张卡牌槽三态显示+Effect缩写 | `backgroundImage`, `nameText`, `effectText`, `isCurrent` | `SetCard()`, `SetEmpty()`, `SetReloading()`, `SetCardForInventory()`, `SetCardForLoadout()`, `EffectToShortPublic()` | Multiple UIs | Active | 战斗预览/背包/弹夹编辑三模式 |
| 38 | `Scripts/UI/InventoryUI.cs` | `InventoryUI` | `Cardwin.UI` | Runtime MB | UI | 背包网格界面：拖拽/交换/显示 | `gridColumns`, `gridRows` | `Bind()`, `Show()`, `Hide()`, `RefreshDisplay()` | (not yet wired) | Stub | 骨架完成，MagazineEditUI 替代其功能 |
| 39 | `Scripts/UI/ShopUI.cs` | `ShopUI` | `Cardwin.UI` | Runtime MB | UI | 商店界面：商品列表/刷新/买卖/货币显示 | `shopPanel`, `currencyText` | `Bind()`, `Show()`, `Hide()`, `RefreshDisplay()` | (not yet wired) | Stub | 骨架完成 |
| 40 | `Scripts/Camera/CameraFollow2D.cs` | `CameraFollow2D` | `Cardwin.Cameras` | Runtime MB | Camera | 平滑跟随玩家，边界钳制(默认关闭) | `target`, `offset`, `smoothTime`, `useBounds` | `Awake()`, `LateUpdate()`, `FindTargetIfMissing()` | MainCamera | Active | 命名空间避让 UnityEngine.Camera |
| 41 | `Scripts/Analytics/BattleLogger.cs` | `BattleLogger` + `BattleEntry` | `Cardwin.Analytics` | Runtime MB + Struct | Analytics | 战斗日志记录：卡牌使用/伤害/治疗/击杀 | — | `LogCardPlay()`, `LogDamageDealt()`, `LogHeal()`, `LogEnemyDeath()`, `ClearLog()` | (not yet wired) | Stub | 骨架完成 |
| 42 | `Editor/Cardwin/CardwinSceneBuilder.cs` | `CardwinSceneBuilder` | — | Editor Tool | Editor Tools | **DISABLED** — 仅显示禁用提示弹窗 | — | `RebuildCleanDemoScene()` | Tools/Cardwin/Rebuild Clean Demo Scene | Legacy / Deprecated | 不再包含场景生成逻辑 |
| 43 | `Editor/Cardwin/CardAssetCreator.cs` | `CardAssetCreator` | — | Editor Tool | Editor Tools | 独立卡牌资产创建工具 | — | `CreateBasicCards()`, `CreateOrUpdateCard()` | Tools/Cardwin/Create Basic Card Assets | Active (谨慎使用) | 生成 Strike/Guard/Heal/Focus 旧资产 |
| 44 | `Editor/Cardwin/CardDatabaseEditorUtility.cs` | `CardDatabaseEditorUtility` | — | Editor Tool | Editor Tools | 扫描CardData→创建/更新CardDatabase.asset | — | `RebuildCardDatabase()`, `EnsureCardsFolder()` | Tools/Cardwin/Rebuild Card Database | Active | 排除 PlayMode |
| 45 | `Editor/Cardwin/CardCsvImporter.cs` | `CardCsvImporter` | — | Editor Tool | Config/Import | CSV→CardData批量导入+数据库同步 | — | `Import()` | Tools/Cardwin/Import Cards From CSV | Active | EditorWindow，从 bullets.csv 创建12张卡 |
| 46 | `Editor/Cardwin/CardLibraryWindow.cs` | `CardLibraryWindow` | — | Editor Tool | Editor Tools | 卡牌管理窗口：搜索/筛选/禁用/移除/删除/同步 | — | `ShowWindow()`, `RefreshCardList()` | Tools/Cardwin/Card Library | Active | EditorWindow，主卡牌管理入口 |
| 47 | `Editor/Cardwin/CardConfigValidator.cs` | `CardConfigValidator` | — | Editor Tool | Editor Tools | 卡牌配置合法性检查+报告输出 | — | `Validate()`, `CheckBasicFields()`, etc. | Tools/Cardwin/Validate Card Configs | Active | 生成 CardValidationReport.txt |

---

## 统计

| 分类 | 数量 |
|---|---|
| Runtime MonoBehaviour | 27 |
| ScriptableObject | 2 |
| Data Class / Struct | 4 |
| Enum | 5 |
| Interface | 1 |
| Editor Tool | 6 |
| **总计 C# 文件** | **46** |

| 状态 | 数量 |
|---|---|
| Active | 30 |
| Stub (骨架) | 7 |
| Legacy / Deprecated | 5 |
| Legacy (Retained for future) | 2 |
| Data Only | 3 |

## 资产统计

| 类型 | 数量 | 路径 |
|---|---|---|
| CardData .asset | 16 | `Assets/Data/Cards/` (12 formal + 4 legacy) |
| CardDatabase .asset | 1 | `Assets/Data/Cards/CardDatabase.asset` |
| Enemy Prefabs | 3 | `Assets/Prefabs/Enemies/` |
| Projectile Prefab | 1 | `Assets/Prefabs/Projectiles/` |
| Scene | 2 | `Assets/Scenes/` (Demo_Combat + SampleScene) |
| CSV Data | 1 | `Assets/Data/CardImport/bullets.csv` |
| Art Asset | 1 | `Assets/Art/Player/player_placeholder.png` |
