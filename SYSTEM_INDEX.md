# SYSTEM_INDEX.md — 系统索引

> 最后更新：2026-06-01 (Stage 12B.1 — Fix Player Death State)

---

## 1. Core System
游戏入口、全局状态管理、场景加载、事件总控。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| GameState.cs | `GameState` (enum) | 游戏状态枚举 (MainMenu/Playing/Paused/GameOver/Victory) | — | — | GameManager, GameStateMachine | 骨架完成 |
| GameManager.cs | `GameManager` | 全局单例，场景加载，状态管理 | `Awake()`, `SetState()`, `LoadScene()`, `RestartCurrentLevel()`, `QuitGame()` | 初始化单例 / 状态切换 / 场景切换 / 重载关卡 / 退出 | 全局 | 骨架完成 |
| GameStateMachine.cs | `GameStateMachine` | 状态机，注册处理器，状态切换通知 | `TransitionTo()`, `RegisterHandler<T>()` | 切换状态并通知所有处理器 | GameManager | 骨架完成 |
| GameStateMachine.cs | `IGameStateHandler` (interface) | 状态变化监听接口 | `OnStateChanged(GameState)` | 响应状态切换 | GameStateMachine | 骨架完成 |
| DemoSceneRuntimeBootstrapper.cs | `DemoSceneRuntimeBootstrapper` | **Legacy safety bootstrapper**：保留脚本但 Stage 8A.5 后不再挂载到 `Demo_Combat` 的正式关卡层级 | `Awake()`, `ResolveLayers()`, `FindCoreObjects()`, `ConfigureCamera()`, `ConfigureGroundAndPlatforms()`, `ConfigurePlayer()`, `PlacePlayerAtSpawn()`, `ResolveSpawnY()`, `ConfigureEnemy()`, `DisableBlockingPlaceholders()`, `IgnorePlayerEnemyCollision()`, `PrintColliderReport()` | 旧版运行时兜底配置；正式地图/敌人改为静态场景实例，不再依赖它生成或管理主要敌人 | (Legacy / not mounted in Demo_Combat) | 保留但不作为 Stage 8A.5 正式流程 |

---

## 2. Combat System
伤害计算、格挡、治疗、命中判定、死亡处理。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| PlayerController2D.cs | `PlayerController2D` | 玩家控制：移动/跳跃/二段跳/冲刺/卡牌系统。**Stage 7B.1: MagazineSystem存在时永不fallback到testCard；Reloading/Empty状态禁止使用卡牌** | `Awake()`, `Update()`, `FixedUpdate()`, `Jump()`, `StartDash()`, `SetInputLocked()`, `IsGrounded()`, `Shoot()`, `EnsureRigidbodySetup()` | 左键/右键: magazineSystem存在时检查IsReloading→Log+return / !HasUsableCurrentCard→Log+return / 仅在magazineSystem==null时fallback testCard/Shoot / Awake+解锁+Jump前兜底恢复Dynamic与gravityScale=3并移除FreezePositionY | Input Manager / MagazineEditUI | 已完成（Stage 8A.3修复重力为0导致的跳跃异常） |
| Health.cs | `Health` | 通用血量：血量/格挡/受击/治疗/死亡(自毁)/无敌 | `Awake()`, `SetInvincible()`, `TakeDamage()`, `Heal()`, `GainBlock()`, `IsDead()`, `Die()` | 初始化 / 无敌标记 / 受击(无敌检查+格挡先吸收+死亡) / 治疗(上限保护) / 格挡 / 死亡判定 / 死亡+Destroy(gameObject) | PlayerController2D / EnemyController / Projectile | 已完成 |
| EnemyController.cs | `EnemyController` | **[LEGACY]** 旧版组合敌人控制器(Melee+Ranged)，已被MeleeEnemyController/RangedEnemyController取代 | `Awake()`, `Start()`, `Update()`, `OnTriggerStay2D()`, `TryDamagePlayer()`, `FireAtPlayer()` | 保留用于向后兼容 | (Legacy) | 已废弃（Stage 8A.1） |
| Projectile.cs | `Projectile` | 子弹：运行时视觉兜底/支持CardData效果投射/swift移动/命中过滤 | `Awake()`, `EnsureVisibleDebugSprite()`, `CreateRuntimeSprite()`, `Init(damage)`, `Init(card+effect+context)`, `Update()`, `OnTriggerEnter2D()` | 运行时sprite兜底 / 旧fallback伤害Init / 新卡牌效果Init(携带CardData+CardEffectType+PlayerCardContext) / 命中→调用CardEffectExecutor.ApplyEffectToTarget / 过滤非战斗目标 | CardEffectExecutor.ExecuteLeft / PlayerController2D.Shoot | 已完成 |
| EnemyProjectile.cs | `EnemyProjectile` | 敌人子弹：Dynamic Rigidbody2D.velocity飞行/运行时可见sprite兜底/命中Player调用Health.TakeDamage / 撞墙销毁 | `Awake()`, `Init(Vector2, int, float)`, `Update()`, `OnTriggerEnter2D()`, `CheckManualHit()`, `HandleHit()`, `EnsureVisibleProjectile()`, `CreateRuntimeSprite()` | 初始化方向/int伤害/速度 / Dynamic+gravity=0+Continuous / 高sortingOrder可见弹体 / 飞行+超时自毁 / Trigger或手动Overlap命中Player→TakeDamage(int) / 撞Ground/Default销毁 | RangedEnemyController | 已完成（Stage 8A.3可见性与命中可靠性修复） |
| MeleeEnemyController.cs | `MeleeEnemyController` | 近战AI：巡逻/追击(stopDistance 1.0)/攻击(attackRange 1.2)/Kinematic Trigger防重合 | `Awake()`, `Start()`, `Update()`, `ChaseAndAttack()`, `Patrol()`, `TryDamagePlayer()`, `FindPlayer()`, `EnsureVisual()` | 初始化Kinematic RB+gravityScale=0 / 查找Player / 巡逻或追击 / stopDistance停步+attackRange内按冷却扣血 | Static scene enemies | 已完成（Stage 8A.5去繁就简） |
| RangedEnemyController.cs | `RangedEnemyController` | 远程AI：水平悬浮巡逻/发射可见EnemyProjectile(prefab)/Kinematic Rigidbody2D/floating | `Awake()`, `Start()`, `Update()`, `HorizontalPatrol()`, `FireAtPlayer()`, `FireFallback()`, `ResolveProjectilePrefab()`, `FindPlayer()`, `EnsureVisual()` | 初始化Kinematic RB+g=0 / 查找Player / 水平巡逻 / prefab发射(自动尝试绑定Assets/Prefabs/Enemies/EnemyProjectile.prefab) / prefab缺失Error+fallback | Scene Pre-placed / Bootstrapper | 已完成（Stage 8A.3 Play验证3个远程均可发射可见子弹） |
| DamageInfo.cs | `DamageInfo` (struct) | 伤害数据结构：基础伤害+Focus加成+来源 | `TotalDamage` (property) | 计算最终伤害值 | Combat 系统 | 骨架完成 |
| SceneCollisionReporter.cs | `SceneCollisionReporter` | 运行时 Debug：输出场景所有 Collider 信息 | `Start()`, `Update()`, `ReportSceneColliders()` | 启动时/F1键输出 / 打印Collider名/Layer/Trigger/Rigidbody类型 | 开发者调试 | 已完成 |

---

## 3. Camera System
摄像机跟随、边界限制。命名空间：`Cardwin.Cameras`（避免与 `UnityEngine.Camera` 冲突）。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| CameraFollow2D.cs | `CameraFollow2D` | 平滑跟随玩家，边界钳制(默认关闭) | `Awake()`, `LateUpdate()`, `FindTargetIfMissing()` | 缓存Camera / 跟随+边界Clamp(useBounds=false默认) / 按Tag查找Player并警告(仅一次) | Camera Update Loop | 已完成 |

---

## 3.1 Enemies System / Combat Enemies
敌人 AI、敌方子弹、敌人 Prefab、场景敌人摆放。该小节属于 Combat 大系统下的敌人实现，脚本实际目录为 `Assets/Scripts/Enemies/`，命名空间为 `Cardwin.Enemies`。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| MeleeEnemyController.cs | `MeleeEnemyController` | 近战AI：巡逻/追击/停止距离/攻击距离；Kinematic Trigger防重合 | `Awake()`, `Start()`, `Update()`, `ChaseAndAttack()`, `Patrol()`, `TryDamagePlayer()`, `FindPlayer()`, `EnsureVisual()` | 初始化刚体和视觉 / 查找Player / 巡逻或追击 / stopDistance停步 / 进入attackRange后造成伤害 | Scene Pre-placed / DemoSceneRuntimeBootstrapper | 已完成（Stage 8A.3 Play验证3个近战均可扣血） |
| RangedEnemyController.cs | `RangedEnemyController` | 远程AI：水平悬浮巡逻/发射EnemyProjectile(prefab)/Kinematic Rigidbody2D/floating | `Awake()`, `Start()`, `Update()`, `HorizontalPatrol()`, `FireAtPlayer()`, `FireFallback()`, `ResolveProjectilePrefab()`, `FindPlayer()`, `EnsureVisual()` | 初始化刚体和视觉 / 查找Player / 水平巡逻 / prefab发射或fallback / prefab为空时自动尝试绑定固定路径 | Scene Pre-placed / DemoSceneRuntimeBootstrapper | 已完成（Stage 8A.3 Play验证3个远程均可发射可见子弹） |
| EnemyProjectile.cs | `EnemyProjectile` | 敌人子弹：Dynamic Rigidbody2D.velocity飞行/可见sprite兜底/命中Player调用Health.TakeDamage/撞墙销毁 | `Awake()`, `Init(Vector2, int, float)`, `Update()`, `OnTriggerEnter2D()`, `CheckManualHit()`, `HandleHit()`, `EnsureVisibleProjectile()` | 初始化方向/int伤害/速度 / Dynamic+gravity=0+Continuous / Trigger+手动Overlap双路径命中Player或地形后处理 | RangedEnemyController / EnemyController(legacy) | 已完成（Stage 8A.3 Play验证可扣Player HP） |

| Prefab路径 | 说明 | 状态 |
|------------|------|------|
| `Assets/Prefabs/Enemies/MeleeEnemy.prefab` | 近战敌人 Prefab：SpriteRenderer/Rigidbody2D/BoxCollider2D/Health/MeleeEnemyController | 已存在 |
| `Assets/Prefabs/Enemies/RangedEnemy.prefab` | 远程敌人 Prefab：SpriteRenderer/Rigidbody2D/BoxCollider2D/Health/RangedEnemyController，绑定 EnemyProjectile | 已存在 |
| `Assets/Prefabs/Enemies/EnemyProjectile.prefab` | 敌人子弹 Prefab：SpriteRenderer(紫色+sortingOrder=150)/Dynamic Rigidbody2D(gravity=0, Continuous)/CircleCollider2D(isTrigger)/EnemyProjectile，scale=(0.45,0.20,1) | 已完成（Stage 8A.3可见性修复） |

当前 `Demo_Combat.unity` 已存在 `LevelRoot/Enemies`，其下放置 3 个近战敌人和 3 个远程敌人；`Enemy_Test_OLD` 保留为禁用 legacy 对象。Stage 8A.3 已验证近战扣血、远程发射可见子弹、敌方子弹扣Player HP、玩家Projectile可伤害近战/远程敌人；完整关卡节奏和数值微调进入下一步 Level Polish / Enemy Tuning。

---

## 4. Card System
ScriptableObject 卡牌数据定义、卡牌效果接口与实现。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| CardData.cs | `CardData` | ScriptableObject 卡牌数据资产（flat字段：damage/block/heal/focusGain + 左右键效果类型） | — | 数据载体：cardId/cardName/cardType/rarity/icon/damage/block/heal/focusGain/leftClickEffect/rightClickEffect/projectilePrefab/description | CardEffectExecutor / PlayerController2D / CardDatabase | 已完成 |
| CardData.cs | `TargetType` (enum) | —（Stage 5 移除，改用 CardEffectType 区分左右键） | — | — | — | 已移除 |
| CardData.cs | `CardEffectEntry` (struct) | —（Stage 5 移除，改用 flat 字段） | — | — | — | 已移除 |
| CardType.cs | `CardType` (enum) | 卡牌类型：Attack / Defense / Heal / Utility | — | — | CardData | 已完成 |
| CardRarity.cs | `CardRarity` (enum) | 稀有度：Common / Rare / Epic | — | — | CardData | 已完成 |
| CardEffectType.cs | `CardEffectType` (enum) | 效果类型：None / Damage / Block / Heal / Focus | — | — | CardEffectExecutor | 已完成 |
| CardDatabase.cs | `CardDatabase` | **子弹功能总表** ScriptableObject：索引allCards / Dictionary缓存 / 按ID/名称/类型/稀有度/效果查询 / 随机抽取(可重复/不重复) / 校验 | `Initialize()`, `GetById()`, `GetByName()`, `GetByType()`, `GetByRarity()`, `GetByEffect()`, `GetRandomCard()`, `GetRandomCards()`, `ValidateDatabase()` | Initialize构建Dict / GetById按cardId查 / GetByName按cardName查 / GetByType按类型筛 / GetByRarity按稀有度筛 / GetByEffect匹配左右键效果 / GetRandomCards(count,allowDuplicate)随机抽取 / ValidateDatabase检查null/空Id/重复/效果数值 | MagazineSystem / Editor / (未来Shop/Inventory) | 已完成 |
| CardRuntimeInstance.cs | `CardRuntimeInstance` | 运行时卡牌实例，包装 CardData + 升级等级 | `CardId`, `DisplayName` (properties) | 提供运行时只读属性 | MagazineSystem / InventorySystem | 已完成 |
| CardEffectExecutor.cs | `CardEffectExecutor` | 卡牌效果执行器：ExecuteLeft发射子弹(携带card+effect+context)/ExecuteRight自用/ApplyEffectToTarget统一施加 | `Initialize()`, `ExecuteLeft()`, `ExecuteRight()`, `ApplyEffectToTarget()` | 初始化上下文 / 左键生成Projectile / 右键直接对Player施效 / ApplyEffectToTarget(Damage/Block/Heal/Focus)不区分好坏对象 | PlayerController2D / Projectile | 已完成 |
| PlayerCardContext.cs | `PlayerCardContext` | 运行时上下文：Player引用/Health/FirePoint/Focus层数/鼠标方向 | `AddFocus()`, `ConsumeFocusMultiplier()`, `GetShootDirectionToMouse()` | 叠加Focus / 消耗Focus返回倍率(每层+50%) / 鼠标世界坐标方向 | CardEffectExecutor | 已完成 |

---

## 5. Magazine System
弹夹管理、弹药消耗、换弹、下 N 发预览。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| MagazineSystem.cs | `MagazineSystem` | 8发弹夹：**Stage 7B.1: 新增HasUsableCurrentCard/LoadedCount/UseLeft+UseRight已有Reloading检查** | `Start()`, `Update()`, `HasUsableCurrentCard()`, `GetCurrentCard()`, `UseCurrentCardLeft()`, `UseCurrentCardRight()`, `ManualReload()`, `StartReload()`, `FinishReload()`, `SetLoadoutCards()`, `BuildRandomMagazine()`, `ResolveSourcePool()` 等 | HasUsableCurrentCard检查!IsReloading+loadedCards.Count>0+index有效 / GetCurrentCard通过HasUsableCurrentCard判断 / UseLeft/UseRight已内置Reloading+null检查 | PlayerController2D / MagazinePreviewUI / MagazineEditUI | 已完成 |
| MagazineSlot.cs | `MagazineSlot` | 弹夹预览槽位数据结构 | `SetCard()`, `Clear()` | 设置预览内容 / 清空 | MagazinePreviewUI | 骨架完成 |

---

## 6. Inventory System
背包存储、卡牌增删查、上场/下场。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| InventorySystem.cs | `InventorySystem` | **Stage 7D: InitializeForRun只初始化一次/Open不再重置库存/useTestStock+resetTestStockOnStart配置/GetOwnedTotalCount** | `InitializeForRun()`, `GetOwnedTotalCount()`, `ResetToTestStock()`, `AddCard()`, `AddCards()`, `RemoveCard()`, `GetCardCounts()`, `SetOwnedCardsFromCounts()` | InitializeForRun检查_hasInitializedThisRun只执行一次 / Open只读取库存不重置 / SetOwnedCardsFromCounts写回后标记_hasInitializedThisRun | PlayerController2D.Awake / MagazineEditUI | 已完成 |

---

## 7. Shop System
商店、购买、出售、刷新。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| ShopManager.cs | `ShopManager` | 商店管理：6 货位、刷新、买卖 | `RefreshShop()`, `BuyItem()`, `SellItem()` | 刷新商品 / 购买 / 出售 | ShopUI | 骨架完成 |
| EconomySystem.cs | `EconomySystem` | 货币系统：加减金钱、支付判定 | `AddCurrency()`, `SpendCurrency()`, `CanAfford()` | 加钱 / 扣钱 / 是否买得起 | ShopManager | 骨架完成 |

---

## 8. UI System
HUD、卡牌预览条、血条、商店界面、背包界面。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| HUDRuntimeBootstrapper.cs | `HUDRuntimeBootstrapper` | 运行时自动保证 Canvas 有 CombatHUD：查找/创建Canvas → AddComponent<CombatHUD> | `Awake()` | 执行顺序-900，先于CombatHUD执行 / 确保Canvas存在 + CombatHUD挂载 | Play启动 | 已完成 |
| CombatHUD.cs | `CombatHUD` | 战斗HUD总控：Awake→禁用旧占位UI→创建CardwinHUDRoot(TopLeftStats/PreviewPanel/ReloadText)+Start绑定Player/MagazineSystem/PreviewUI+Update刷新。战斗HUD只显示3发预览，不显示完整8格弹夹 | `Awake()`, `Start()`, `Update()`, `EnsureCanvas()`, `DisableLegacyPlaceholders()`, `EnsureHUDRoot()`, `EnsureTopLeftStats()`, `EnsurePreviewPanel()`, `DisableFullBarIfExists()`, `EnsureReloadText()`, `EnsureTextInParent()`, `BindSystems()`, `RefreshHUD()`, `RefreshReloadProgress()` | Awake禁用旧占位+清CardwinHUDRoot / Start只绑定MagazinePreviewUI / 不再创建FullMagazinePanel | Canvas / HUDRuntimeBootstrapper | 已完成 |
| MagazinePreviewUI.cs | `MagazinePreviewUI` | 弹夹下3发预览：3个PreviewSlot(150x60)，订阅MagazineSystem事件 | `Bind()`, `RefreshPreview()`, `OnReloadStarted()`, `OnReloadFinished()`, `EnsureSlotsExist()`, `OnDestroy()` | 订阅事件 / 当前卡>Name<高亮 / Reloading="Reloading" / 效果缩写L:Dmg R:Dmg | CombatHUD.Bind() | 已完成 |
| MagazineFullBarUI.cs | `MagazineFullBarUI` | **保留给未来背包界面** — 完整8发弹夹显示。战斗HUD不再使用 | `Bind()`, `RefreshFullBar()`, `HandleReloadStarted()`, `HandleReloadFinished()`, `EnsureSlotsExist()`, `OnDestroy()` | 保留脚本，未来用于背包/弹夹编辑界面 | (未来) | 保留 |
| CardSlotUI.cs | `CardSlotUI` | 单张卡牌槽三态显示+EffectToShort缩写；支持战斗预览/背包/弹夹编辑三种模式 | `SetCard(card,current)`, `SetCard(card,current,used)`, `SetEmpty()`, `SetReloading()`, `SetCardForInventory(onClick)`, `SetCardForLoadout(index,onClick)`, `SetEmptyLoadoutSlot(index,onClick)`, `EffectToShort()`, `EffectToShortPublic()` | 效果缩写Dmg/Blk/Heal/Fcs / current=亮黄scale1.1 / used=灰+[Used] / Reloading=橙 / Inventory模式绑定点击回调 / Loadout模式绑定index+点击回调 | MagazinePreviewUI / MagazineFullBarUI / MagazineEditUI | 已完成 |
| MagazineEditUI.cs | `MagazineEditUI` | 背包/弹夹编辑界面：**Stage 7C.2: BagPanel 1380x820 + 5分页(Magazine/Inventory/Fusion/Equipment/Preview)+TabRow+BottomButtonRow修复+SwitchTab** | `Awake()`, `Start()`, `Update()`, `Toggle()`, `Open()`, `Close()`, `Refresh()`, `RefreshCurrentTab()`, `SwitchTab()`, `RefreshTabButtons()`, `RefreshOwnedCards()`, `RefreshLoadoutSlots()`, `RefreshPreviewPage()`, `OnOwnedCardClicked()`, `OnLoadoutSlotClicked()`, `Apply()`, `CancelEdit()`, `ClearLoadout()`, `AutoFill()`, `EnsureEventSystem()`, `EnsureUI()`, `CreateBagPanelBackground()`, `CreateTitleText()`, `CreateTabRow()`, `CreateTabButton()`, `CreateContentRoot()`, `CreateMagazinePage()`, `CreateInventoryPage()`, `CreateFusionPage()`, `CreateEquipmentPage()`, `CreatePreviewPage()`, `CreatePagePlaceholder()`, `CreateBottomButtonRow()`, `CreateHintText()`, `CreateReadOnlyCardSlot()`, `CreateActionButton()`, `CreateTextSlot()`, `CreateTextChild()`, `FindCardDatabaseInternal()` | 5分页框架/Magazine页可编辑/Inventory只读/Fusion+Equipment占位/Preview只读预览/当前代码以 MagazineEditUI.cs 为准：BagPanel 1380x820、ContentRoot 1260x610、两侧面板540x500、BottomButtonRow 820x52、操作按钮170x42 | PlayerController2D (SetInputLocked) | 已完成 |
| ShopUI.cs | `ShopUI` | 商店界面：商品列表/刷新/买卖/货币显示 | `Bind()`, `Show()`, `Hide()`, `RefreshDisplay()`, `OnBuyClicked()`, `OnSellClicked()`, `OnRefreshClicked()` | 绑定 / 显隐 / 刷新 / 买卖回调 | ShopManager | 骨架完成 |
| InventoryUI.cs | `InventoryUI` | 背包网格界面：拖拽/交换/显示 | `Bind()`, `Show()`, `Hide()`, `RefreshDisplay()`, `OnSlotClicked()`, `OnDragStart()`, `OnDragEnd()` | 绑定 / 显隐 / 刷新 / 格子/拖拽回调 | InventorySystem | 骨架完成 |

---

## 9. Analytics System
战斗数据采集与统计。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| BattleLogger.cs | `BattleLogger` | 战斗日志记录：卡牌使用/伤害/治疗/击杀 | `LogCardPlay()`, `LogDamageDealt()`, `LogHeal()`, `LogEnemyDeath()`, `ClearLog()`, `GetEntriesByCard()` | 记录卡牌使用 / 伤害 / 治疗 / 击杀 / 清空 / 按卡牌查询 | CardEffectExecutor / Combat 系统 | 骨架完成 |
| BattleLogger.cs | `BattleEntry` (struct) | 单条战斗记录 | — | — | BattleLogger | 骨架完成 |

---

## 10. Editor
编辑器工具脚本。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| CardwinSceneBuilder.cs | `CardwinSceneBuilder` | **DISABLED** — 仅显示禁用提示弹窗 | `RebuildCleanDemoScene()` | 弹窗提示已禁用 | Tools/Cardwin/Rebuild Clean Demo Scene (disabled) | DISABLED |
| CardAssetCreator.cs | `CardAssetCreator` | 独立卡牌资产创建工具（不依赖SceneBuilder） | `CreateBasicCards()`, `CreateOrUpdateCard()` | 菜单入口/检查PlayMode / 创建/更新Strike/Guard/Heal/Focus | Tools/Cardwin/Create Basic Card Assets | 已完成 |
| CardDatabaseEditorUtility.cs | `CardDatabaseEditorUtility` | 扫描 Assets/Data/Cards 下所有 CardData → 创建/更新 CardDatabase.asset → 调用 ValidateDatabase | `RebuildCardDatabase()`, `EnsureCardsFolder()` | 菜单 Tools/Cardwin/Rebuild Card Database / 排除CardDatabase自身 / 排除PlayMode | Editor | 已完成 |
| CardConfigValidator.cs | `CardConfigValidator` | **[STAGE 10C]** 卡牌配置合法性检查器：扫描CardData+CardDatabase / 检查CardID/Type/GoodEvil/IsOffensive/效果实现/数值异常/CardDatabase重复null旧资产/Reward池/背包测试库存 / 输出CardValidationReport.txt | `Validate()`, `ScanCardDataAssets()`, `CheckBasicFields()`, `CheckTypeAndUseTarget()`, `CheckGoodEvilCost()`, `CheckIsOffensive()`, `CheckEffectImplementation()`, `CheckNumericValues()`, `CheckCardDatabase()`, `CheckRewardPool()`, `CheckInventoryTestStock()`, `GenerateReport()`, `SaveReport()` | Tools/Cardwin/Validate Card Configs (菜单) | 已完成 (Stage 10C) |

## 11. Scenes

| 场景名 | 用途 | 当前状态 |
|--------|------|----------|
| `Demo_Combat.unity` | 主要测试场景，Stage 3.5 重建，Stage 4 后锁定（不可重建） | 活跃 — LOCKED |
| `CardwinSceneBuilder` | 备份恢复工具：`Tools/Cardwin/Rebuild Clean Demo Scene`（仅在明确要求时运行） | 备份 |

`Demo_Combat.unity` 当前已存在 `LevelRoot/Enemies`，包含 3 个近战敌人和 3 个远程敌人；`LevelRoot` 已挂载 `DemoSceneRuntimeBootstrapper`。Stage 8A.3 已完成基础运行验证；后续仍需 Level Polish / Enemy Tuning 做路线节奏、相机边界和战斗数值打磨。

## 12. Projectile Prefab

| 路径 | 说明 | 状态 |
|------|------|------|
| `Assets/Prefabs/Projectiles/Projectile_Test.prefab` | 测试投射物：SpriteRenderer + Kinematic Rigidbody2D(gravity=0) + CircleCollider2D(isTrigger) + Projectile | 已创建 |

## 13. Enemy Prefabs

| 路径 | 说明 | 状态 |
|------|------|------|
| `Assets/Prefabs/Enemies/MeleeEnemy.prefab` | 近战敌人：红色SpriteRenderer/Dynamic Rigidbody2D/BoxCollider2D/Health(30)/MeleeEnemyController | 新增（Stage 8A.1） |
| `Assets/Prefabs/Enemies/RangedEnemy.prefab` | 远程敌人：紫色SpriteRenderer/Kinematic Rigidbody2D(g=0)/BoxCollider2D/Health(20)/RangedEnemyController(binds EnemyProjectile) | 新增（Stage 8A.1） |
| `Assets/Prefabs/Enemies/EnemyProjectile.prefab` | 敌人子弹：紫色SpriteRenderer(sortingOrder=150)/Dynamic Rigidbody2D(gravity=0, Continuous)/CircleCollider2D(isTrigger)/EnemyProjectile，scale=(0.45,0.20,1) | 已完成（Stage 8A.3可见性与命中修复） |

## 14. Audit Documents (Stage 11A)

| 文档 | 内容 |
|---|---|
| PROJECT_SCRIPT_INDEX.md | 46 个脚本总表（路径/类名/命名空间/类型/系统/状态） |
| PROJECT_FUNCTION_INDEX.md | 核心函数级索引（函数名/调用时机/功能/风险） |
| CARDWIN_TOOLS_AUDIT.md | 6 个 Tools 菜单项审计（保留/废弃建议） |
| CARD_SYSTEM_AUDIT.md | 卡牌系统唯一性审计 |
| ACTOR_ARCHITECTURE_AUDIT.md | 角色属性架构审计 |
| ENEMY_SYSTEM_AUDIT.md | 敌人系统冗余审计 |
| UI_SYSTEM_AUDIT.md | UI 系统审计 |
| SCENE_STRUCTURE_AUDIT.md | 场景对象审计 |
| CLEANUP_PLAN.md | 清理计划（P0~P3 优先级） |
| README_PROJECT_OVERVIEW.md | 新人入门文档 |
| REGRESSION_TEST_REPORT.md | Stage 11C 清理后全功能回归测试报告 |
