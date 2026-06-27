# SYSTEM_INDEX.md — 系统索引

> 最后更新：2026-06-27 (Stage 57 — 最小 Lua 子弹系统试点：自研简化 Lua 表解析器 + LuaBulletDatabase/Host/BattleAPI/CardBridge/DropBridge/RuntimeManager + BulletRegistry.lua(lua_pierce_001/lua_homing_001) + Docs/LuaBulletSpec.md；增量接入背包与敌人掉落，不重写旧 Projectile/Boss/玩家/三模组，未接 xLua 用 C# 行为桥接，可打包。见 §28)
>
> 上次更新：2026-06-26 (Stage 55/56 — Boss AI 自动机审计 + MirrorAngelBossDebugState + 子弹系统审计 Docs/BulletSystemAudit.md)

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
| Health.cs | `Health` | 通用血量：血量/格挡/受击/治疗/死亡(自毁)/无敌。**Stage 33: 新增 ReviveToFull() 供死亡Retry复活** | `Awake()`, `SetInvincible()`, `TakeDamage()`, `Heal()`, `GainBlock()`, `IsDead()`, `ReviveToFull()`, `Die()` | 初始化 / 无敌标记 / 受击(无敌检查+格挡先吸收+死亡) / 治疗(上限保护) / 格挡 / 死亡判定 / **ReviveToFull清死亡flag+满血+清block+触发OnHealed/OnBlockChanged** / 死亡(Player→GameOverController.HandlePlayerDeath, 敌人→Destroy) | PlayerController2D / EnemyController / Projectile / PlayerRuntimeReset | Stage 33 更新 |
| EnemyController.cs | `EnemyController` | **[LEGACY]** 旧版组合敌人控制器(Melee+Ranged)，已被MeleeEnemyController/RangedEnemyController取代 | `Awake()`, `Start()`, `Update()`, `OnTriggerStay2D()`, `TryDamagePlayer()`, `FireAtPlayer()` | 保留用于向后兼容 | (Legacy) | 已废弃（Stage 8A.1） |
| Projectile.cs | `Projectile` | 子弹：命中过滤+伤害投射+移动。**Stage 36 命中优先级=BossPart→IDamageable→Health**。**Stage 38(仅视觉): 新增 redSprite/blueSprite/bulletScale；Init 按效果选图(Damage=红 PlayerProjectile_Red / 其它=蓝 PlayerProjectile_Blue)+白色 tint+scale 0.25；移除原黄色强制 tint；命中/伤害/卡牌逻辑不变** | `HandleHit()`, `ResolveGenericDamage()`, `Init()`, `ApplyBulletVisual()`, `OnTriggerEnter2D()` | 命中先查 BossPart→IDamageable→Health / Init 选红蓝视觉 / 过滤非战斗目标 | CardEffectExecutor.ExecuteLeft / PlayerController2D.Shoot | Stage 38 更新 |
| IDamageable.cs | `IDamageable` (interface) | **[Stage 35]** 通用伤害接收接口：`TakeHit(int amount, GameObject source)`。供不使用 Health 的目标（Boss 部位/根）实现；普通敌人继续用 Health，不实现此接口 | `TakeHit(int, GameObject)` | Projectile.HandleHit 优先查找并调用 | MirrorSaintessBossPart / MirrorSaintessBoss | Stage 35 新增 |
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
| PlayerCardContext.cs | `PlayerCardContext` | 运行时上下文：Player引用/Health/FirePoint/Focus层数/鼠标方向 + OnFocusChanged事件 | `AddFocus()`, `ConsumeFocusMultiplier()`, `GetShootDirectionToMouse()` | 叠加Focus + 触发OnFocusChanged / 消耗Focus返回倍率(每层+50%) + 触发OnFocusChanged / 鼠标世界坐标方向 | CardEffectExecutor | Stage 15 更新 (新增事件) |

---

## 5. Magazine System
弹夹管理、弹药消耗、换弹、下 N 发预览。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| MagazineSystem.cs | `MagazineSystem` | 8发弹夹：**Stage 18: 新增OnCardConsumed事件(firedCardSnapshot+targetsSelf)在AdvanceIndex前触发** | `Start()`, `Update()`, `HasUsableCurrentCard()`, `GetCurrentCard()`, `UseCurrentCardLeft()`, `UseCurrentCardRight()`, `ManualReload()`, `StartReload()`, `FinishReload()`, `SetLoadoutCards()`, `BuildRandomMagazine()`, `ResolveSourcePool()` 等 | UseLeft/UseRight在AdvanceIndex前保存快照并触发OnCardConsumed / HasUsableCurrentCard检查!IsReloading | PlayerController2D / MagazinePreviewUI / MagazineEditUI | Stage 18 更新 |
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
| CombatHUD.cs | `CombatHUD` | Stage 26：`_comboRankDisplay`序列化引用替代运行时创建；移除SetNativeSize；移除GetRankSprite | `Awake()`, `Start()`, `Update()`, `EnsureComboText()`, `RefreshHUD()`, `BindSystems()` | 通过ComboRankDisplay管理评级贴图 / 不再运行时创建ComboRankImage | Canvas / HUDRuntimeBootstrapper | Stage 26 |
| ComboRatingSystem.cs | `ComboRatingSystem` | 连击评级核心：1-2→D,3-5→C,6-9→B,10-14→A,15+→S,5秒超时 | `RegisterCardUse()`, `ResetCombo()`, `CalculateRank()`, `Update()` | 连击计数+目标匹配检查 | PlayerController2D / CombatHUD | Stage 25 |
| ComboRankDisplay.cs | `ComboRankDisplay` | **[ExecuteAlways]** Stage 26 新增：评级贴图显示+编辑预览(previewRank)+ApplyRankVisual+固定尺寸 | `ApplyRankVisual()`, `ClearRank()`, `SetSpriteOnly()`, `GetRankSprite()` | D/C/B/A/S sprite映射 / OnValidate切换预览 / 不调用SetNativeSize | CombatHUD.RefreshHUD | Stage 26 新增 |
| MagazinePreviewUI.cs | `MagazinePreviewUI` | Stage 24: selfTargetRect绑定为HPBar/EmptyBase而非Background | `Bind()`, `RequestRefresh()`, `PlayCurrentBulletConsumed()`, `RefreshAllSlotsImmediately()`, `PlaySelfAndRefresh()`, `LoadBulletSprites()` | selfTarget→EmptyBase / 外层Background不再触发命中 | CombatHUD.Bind() | Stage 24 |
| MagazineFullBarUI.cs | `MagazineFullBarUI` | **保留给未来背包界面** — 完整8发弹夹显示。战斗HUD不再使用 | `Bind()`, `RefreshFullBar()`, `HandleReloadStarted()`, `HandleReloadFinished()`, `EnsureSlotsExist()`, `OnDestroy()` | 保留脚本，未来用于背包/弹夹编辑界面 | (未来) | 保留 |
| CardSlotUI.cs | `CardSlotUI` | 单张卡牌槽三态显示+EffectToShort缩写；支持战斗预览/背包/弹夹编辑三种模式 | `SetCard(card,current)`, `SetCard(card,current,used)`, `SetEmpty()`, `SetReloading()`, `SetCardForInventory(onClick)`, `SetCardForLoadout(index,onClick)`, `SetEmptyLoadoutSlot(index,onClick)`, `EffectToShort()`, `EffectToShortPublic()` | 效果缩写Dmg/Blk/Heal/Fcs / current=亮黄scale1.1 / used=灰+[Used] / Reloading=橙 / Inventory模式绑定点击回调 / Loadout模式绑定index+点击回调 | MagazinePreviewUI / MagazineFullBarUI / MagazineEditUI | 已完成 |
| MagazineEditUI.cs | `MagazineEditUI` | 背包/弹夹编辑界面：**Stage 7C.2: BagPanel 1380x820 + 5分页(Magazine/Inventory/Fusion/Equipment/Preview)+TabRow+BottomButtonRow修复+SwitchTab** | `Awake()`, `Start()`, `Update()`, `Toggle()`, `Open()`, `Close()`, `Refresh()`, `RefreshCurrentTab()`, `SwitchTab()`, `RefreshTabButtons()`, `RefreshOwnedCards()`, `RefreshLoadoutSlots()`, `RefreshPreviewPage()`, `OnOwnedCardClicked()`, `OnLoadoutSlotClicked()`, `Apply()`, `CancelEdit()`, `ClearLoadout()`, `AutoFill()`, `EnsureEventSystem()`, `EnsureUI()`, `CreateBagPanelBackground()`, `CreateTitleText()`, `CreateTabRow()`, `CreateTabButton()`, `CreateContentRoot()`, `CreateMagazinePage()`, `CreateInventoryPage()`, `CreateFusionPage()`, `CreateEquipmentPage()`, `CreatePreviewPage()`, `CreatePagePlaceholder()`, `CreateBottomButtonRow()`, `CreateHintText()`, `CreateReadOnlyCardSlot()`, `CreateActionButton()`, `CreateTextSlot()`, `CreateTextChild()`, `FindCardDatabaseInternal()` | 5分页框架/Magazine页可编辑/Inventory只读/Fusion+Equipment占位/Preview只读预览/当前代码以 MagazineEditUI.cs 为准：BagPanel 1380x820、ContentRoot 1260x610、两侧面板540x500、BottomButtonRow 820x52、操作按钮170x42 | PlayerController2D (SetInputLocked) | 已完成 |
| SettingsMenuController.cs | `SettingsMenuController` | Stage 28: 全屏改为3模式下拉框(ExclusiveFullScreen/FullScreenWindow/Windowed)+CreateFullscreenSection重建 | `Awake()`, `Update()`, `OpenFromMainMenu()`, `OpenFromPauseMenu()`, `EnsureUI()`, `Close()`, `IsOpen`, `LoadCurrentSettingsToUI()`, `OnVolumeChanged()`, `OnApplyClicked()`, `OnResumeClicked()`, `OnMainMenuClicked()` | 1920×1080全屏默认 / 编辑器不切换宿主全屏 | MainMenuController / PauseMenuController | Stage 28 |
| SettingsMenuController.cs | `SettingsSource` (enum) | 设置来源枚举：MainMenu / PauseMenu | — | SettingsMenuController | 已完成 (Stage 12C.1) |
| ShopUI.cs | `ShopUI` | 商店界面：商品列表/刷新/买卖/货币显示 | `Bind()`, `Show()`, `Hide()`, `RefreshDisplay()`, `OnBuyClicked()`, `OnSellClicked()`, `OnRefreshClicked()` | 绑定 / 显隐 / 刷新 / 买卖回调 | ShopManager | 骨架完成 |
| InventoryUI.cs | `InventoryUI` | 背包网格界面：拖拽/交换/显示 | `Bind()`, `Show()`, `Hide()`, `RefreshDisplay()`, `OnSlotClicked()`, `OnDragStart()`, `OnDragEnd()` | 绑定 / 显隐 / 刷新 / 格子/拖拽回调 | InventorySystem | 骨架完成 |
| PlayerStatusHUDView.cs | `PlayerStatusHUDView` | 玩家状态 HUD 视图：HP/MP/Shield 进度条 + StatusEffectStrip 引用。Stage 23：ImpactGlow子节点 | `SetHPBar()`, `SetMPBar()`, `SetShieldBar()`, `RefreshPreview()`, `StatusEffectStrip` (property) | HP Fill+Text / MP Fill+Text / Shield Fill+Text / 编辑预览 / StatusEffectStrip 访问 | PlayerStatusHUDBinder | Stage 23 更新 |
| PlayerStatusImpactFeedback.cs | `PlayerStatusImpactFeedback` | Stage 27: 修复CS0103编译错误(PlayFeedbackEditor调用加#if UNITY_EDITOR)；redShakeFrequency集成到shake波形 | `PlayRedImpact()`, `PlayBlueImpact()`, `Play(SelfImpactVisualType)`, `ResetVisual()` | PreviewRedImpact/BlueImpact安全处理editor/runtime双路径 / shakeFrequency参与sin波形 | MagazinePreviewUI.PlaySelfAndRefresh | Stage 27 |
| PlayerStatusHUDBinder.cs | `PlayerStatusHUDBinder` | 绑定 Player 数据到 PlayerStatusHUDView：HP/Block/Focus 轮询 + OnFocusChanged 事件订阅驱动状态图标 | `Start()`, `LateUpdate()`, `SubscribeToFocus()`, `OnFocusChanged()`, `ApplyFocusIcon()` | 查找 Player → 获取 Health/CardContext / 轮询 HP+Block / 订阅 Focus 事件驱动图标 | PlayerStatusHUDView | Stage 15 重写 |
| StatusEffectIconStripView.cs | `StatusEffectIconStripView` | [ExecuteAlways] 状态图标栏：8 预创建槽位 + 编辑模式预览 + 运行时 Show/Hide/Clear + 容量限制(宽度自适应) + RectMask2D 安全裁剪 | `CacheSlots()`, `RefreshPreview()`, `ApplyLayoutSettings()`, `ShowStatusIcon()`, `HideStatusIcon()`, `ClearAllStatusIcons()`, `CompactSlots()`, `CalculateVisibleCapacity()`, `RefreshSlots()`, `OnRectTransformDimensionsChange()` | 编辑预览 / 运行时图标显示/隐藏 / 自动左移补位 / 容量计算与超量隐藏 / 尺寸变化实时刷新 | PlayerStatusHUDBinder | Stage 15 新增 / Stage 16 容量与排列修复 |
| BulletPreviewItem.cs | `BulletPreviewItem` | Stage 23: 自射子步抗穿透+IsOverlappingTarget(GetScreenRect+Rect.Overlaps)边碰即隐藏 | `Bind()`, `SetupBullet()`, `SetEmpty()`, `ApplyBackground()`, `IsOverlappingTarget()`, `GetScreenRect()`, `ResetVisualState()`, `RecordBaseState()`, `EnsureReferences()` | AC动画+5px子步/边碰检测/命中同帧隐藏 | MagazinePreviewUI | Stage 23 |

---

## 9. Analytics System
战斗数据采集与统计。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| BattleLogger.cs | `BattleLogger` | 战斗日志记录：卡牌使用/伤害/治疗/击杀 | `LogCardPlay()`, `LogDamageDealt()`, `LogHeal()`, `LogEnemyDeath()`, `ClearLog()`, `GetEntriesByCard()` | 记录卡牌使用 / 伤害 / 治疗 / 击杀 / 清空 / 按卡牌查询 | CardEffectExecutor / Combat 系统 | 骨架完成 |
| BattleLogger.cs | `BattleEntry` (struct) | 单条战斗记录 | — | — | BattleLogger | 骨架完成 |

---

## 10. Settings System
游戏设置：音量、全屏/窗口、分辨率，保存到本地 JSON。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| SettingsData.cs | `SettingsData` | Stage 28: 默认1920×1080全屏+fullscreenMode(int,0=ExclusiveFullScreen) | — | masterVolume/fullscreen/resolutionWidth/Height/fullscreenMode | SettingsSystem | Stage 28 |
| SettingsSystem.cs | `SettingsSystem` (static) | Stage 28: Apply使用FullScreenMode枚举+验证模式合法性+编辑器保护 | `Load()`, `Save()`, `Apply()`, `ResetToDefault()`, `GetAvailableResolutions()` | JSON持久化 / 启动应用保存值 / 无效值回退默认 / #if UNITY_STANDALONE_WIN | GameFlowManager / SettingsMenuController | Stage 28 |

存档路径：`Application.persistentDataPath/cardwin_settings.json`

## 10.1 Runtime System (Stage 31)
全局运行环境自动初始化、DontDestroyOnLoad 持久化。命名空间：`Cardwin.Runtime`。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| GlobalRuntimeBootstrap.cs | `GlobalRuntimeBootstrap` | 全局运行根节点单例：DontDestroyOnLoad + 持有Player/Camera/Canvas引用 + TeleportPlayer + SnapCameraToPlayer | `Awake()`, `TeleportPlayer()`, `SnapCameraToPlayer()`, `OnDestroy()` | 单例保护 / DontDestroyOnLoad / 自动查找Player/Camera引用 / 传送+物理同步 / 相机快照 | GlobalRuntimeAutoLoader / BossSceneTransitionController / BossRoomSceneController | Stage 31 新增 |
| GlobalRuntimeAutoLoader.cs | `GlobalRuntimeAutoLoader` (static) | RuntimeInitializeOnLoadMethod(BeforeSceneLoad)：检查GlobalRuntimeBootstrap是否存在，不存在则从Resources实例化Prefab | `EnsureRuntimeRoot()` | 任意场景启动前自动创建全局运行环境 | Unity引擎(BeforeSceneLoad) | Stage 31 新增 |
| GlobalEventSystemGuard.cs | `GlobalEventSystemGuard` | sceneLoaded 时自动销毁非全局的重复 EventSystem | `Awake()`, `OnSceneLoaded()`, `RemoveDuplicateEventSystems()` | 保护 GlobalEventSystem 为唯一 / 销毁场景级重复 EventSystem | SceneManager.sceneLoaded | Stage 31A 新增 |
| SceneRespawnService.cs | `SceneRespawnService` | **[Stage 32]** 跨场景出生/复活唯一权威（挂 GlobalRuntimeRoot）：玩法场景启用玩家物理/可见/解锁并放到 SceneRespawnPoint+相机Snap；非玩法场景(MainMenu)禁用 simulated+隐藏 Visual+锁输入（不改重力/不冻Y）；坠落低于 FallLimitY 按冷却复活，仅改位置+速度不动战斗状态 | `OnSceneLoaded()`, `EvaluateScene()`, `EnterGameplayScene()`, `EnterNonGameplayScene()`, `Update()`, `RespawnPlayerAtCurrentPoint()`, `PlacePlayer()`, `SnapCameraNextFixedUpdate()`, `SetInputLocked()`, `SetVisualActive()`, `ResolvePlayerReferences()`, `FindRespawnPointInScene()`, `FindMarkerInScene()` | SceneManager.sceneLoaded / Start / PlayerRuntimeReset | Stage 32 新增 |
| PlayerRuntimeReset.cs | `PlayerRuntimeReset` (Cardwin.Player) | **[Stage 33]** 死亡 Retry 统一重置入口（挂全局 Player）：完整把同一常驻 Player 从死亡恢复到可控——Health.ReviveToFull + PlayerController2D.SetDead(false)(恢复 rb.simulated/Collider/输入) + AnimationBridge.ResetDeathVisual + SceneRespawnService.RespawnPlayerAtCurrentPoint(放回当前场景复活点+相机Snap)。与坠落复活区分：只在 Retry 调用 | `ResetForRetry()`, `ResolveReferences()` | GameOverController.OnRetryClicked | Stage 33 新增 |

### GlobalRuntimeRoot Prefab
```
Assets/Resources/System/GlobalRuntimeRoot.prefab
├── Player (PlayerController2D, Health, MagazineSystem, InventorySystem, CardEffectExecutor, etc.)
│   ├── GroundCheck
│   ├── FirePoint
│   └── VisualRoot (GothicNunFrameVisual active)
├── MainCamera (Camera ortho=6, CameraFollow2D, AudioListener, tag=MainCamera)
├── Canvas (ScreenSpaceOverlay, CanvasScaler, GraphicRaycaster)
│   ├── CombatHUD, MagazineEditUI, PauseMenuController, GameOverController
│   ├── PausePanel, GameOverPanel
│   ├── SettingsMenuHost (SettingsMenuController)
│   ├── PlayerStatusHUD, BulletPreviewHUD, ComboRankHUD
│   └── ...
└── GlobalEventSystem (EventSystem + StandaloneInputModule)
```
> Stage 32: 根对象新增 `SceneRespawnService`（跨场景出生/复活/防坠落唯一权威，wire Player Transform+Rigidbody2D）。

## 10.2 Level System (Stage 29 → Stage 31 更新)
场景切换、敌人清场跟踪、BossPortal、BossRoomSceneController。命名空间：`Cardwin.Level`。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| RoomEnemyClearTracker.cs | `RoomEnemyClearTracker` | 监控 LevelRoot/NormalRoom/Enemies 下所有敌人，全部死亡后调用 BossPortal.ActivatePortal() + 触发 OnAllEnemiesCleared | `Start()`, `Update()` | 初始化跟踪列表 / 每帧检查是否全部死亡 | BossPortal | Stage 29A 更新 |
| BossPortal.cs | `BossPortal` | 传送门：forceOpenForTesting调试开关(Start立即ActivatePortal) + 编辑模式预览 + Gizmos + 清场后ActivatePortal + TryEnterPortal玩家识别 + 场景切换 | `Awake()`, `Start()`, `ActivatePortal()`, `SetPortalAvailable(bool)`, `TryEnterPortal(Collider2D)`, `ApplyEditorPreview()`, `OnValidate()`, `OnDrawGizmos()`, `ForceActivatePortal()` | Start: forceOpenForTesting→立即ActivatePortal / Awake: SetPortalAvailable(false) / 编辑预览 / 清场后激活 / 玩家进入触发UnityEvent | RoomEnemyClearTracker / BossPortalTrigger2D | Stage 30A 更新 |
| BossPortalTrigger2D.cs | `BossPortalTrigger2D` | **[Stage 29B NEW]** Collider 子对象转发：挂载在 `PortalTrigger` 上接收 `OnTriggerEnter2D`，转发给根对象 `BossPortal.TryEnterPortal()` | `Reset()`, `OnTriggerEnter2D()` | 自动绑定父级 BossPortal / 触发器进入时转发 | Physics2D | Stage 29B 新增 |
| BossSceneTransitionController.cs | `BossSceneTransitionController` | 场景切换：Additive加载BossRoom → 验证MainGroundCollider → GlobalRuntimeBootstrap.TeleportPlayer → WaitForFixedUpdate → SnapCamera → 卸载旧场景。不再使用MoveGameObjectToScene | `TransitionToBossRoom()`, `TransitionRoutine()`, `FindControllerInsideScene()` | 异步加载 / 地面验证 / 通过GlobalRuntimeBootstrap传送+快照 / 卸载旧场景 | BossPortal.onPlayerEnterPortal | Stage 31 重写 |
| BossRoomSceneController.cs | `BossRoomSceneController` | BossRoom场景数据：SpawnPoints/MainGroundCollider/ArenaCenter/SafetyFloor + Start自动放置Player | `Start()`, `PlacePlayerAtSpawn()`, `OnDrawGizmos()` | Start→PlacePlayerAtSpawn(GlobalRuntimeBootstrap.TeleportPlayer+SnapCamera) / Gizmos绘制 | BossSceneTransitionController / 自身Start | Stage 31 更新 |
| SceneRespawnPoint.cs | `SceneRespawnPoint` | **[Stage 32]** 标记当前场景玩家出生/复活点 + FallLimitY；只提供数据，不创建/修改玩家 | `Position` (property), `FallLimitY` (property), `OnDrawGizmos()` | 绿色出生 Gizmo + 红色 FallLimit 横线 | SceneRespawnService | Stage 32 新增 |
| SceneGameplayMarker.cs | `SceneGameplayMarker` | **[Stage 32]** 标记玩法场景（IsGameplayScene）；MainMenu 不挂 → 被判为非玩法场景 | `IsGameplayScene` (property) | 玩法/非玩法场景判定 | SceneRespawnService | Stage 32 新增 |

### BossRoom 场景结构 (Stage 30 更新 / Stage 32 碰撞修复)
> Stage 32: 7 个地面对象（MainGround/SafetyFloor/LeftPlatform/RightPlatform/LeftPlatform(1)/LeftWall/RightWall）的 `BoxCollider2D.m_Size` 由错误的 `{0.0001,0.0001}` 修正为 `{1,1}`，碰撞体现与 transform scale 匹配（修复玩家穿地板/持续下坠）。
```
BossRoom
├── BossRoomEnvironment
│   ├── MainGround (BoxCollider2D, SpriteRenderer, Layer=Ground, pos=(0,-3,0), scale=40×1)│   ├── LeftWall (BoxCollider2D, SpriteRenderer, Layer=Ground, pos=(-20,2,0), scale=1×12)
│   ├── RightWall (BoxCollider2D, SpriteRenderer, Layer=Ground, pos=(20,2,0), scale=1×12)
│   ├── LeftPlatform (BoxCollider2D, SpriteRenderer, Layer=Ground, pos=(-10,-0.5,0), scale=6×0.5)
│   ├── RightPlatform (BoxCollider2D, SpriteRenderer, Layer=Ground, pos=(10,-0.5,0), scale=6×0.5)
│   └── SafetyFloor (BoxCollider2D, SpriteRenderer α=0.15, Layer=Ground, pos=(0,-13,0), scale=60×1)
├── SpawnPoints
│   ├── BossPlayerSpawnPoint (-8, -1.4, 0) + SceneRespawnPoint (Stage 32, fallLimitY=-20，与出生点统一)
│   └── BossSpawnPoint (8, -1.4, 0)
├── BossRoomMarkers
│   └── BossArenaCenter (0, 0, 0)
├── BossRoomSceneController (BossRoomSceneController 组件, 引用 MainGround/ArenaCenter/SafetyFloor/SpawnPoints) + SceneGameplayMarker (Stage 32)
└── EditorPreviewCamera (Camera ortho=12, Tag=EditorOnly, pos=(0,-1,-10))
```

### 场景切换流程 (Stage 31 更新)
```
GlobalRuntimeRoot (DontDestroyOnLoad, auto-loaded via RuntimeInitializeOnLoadMethod)
├── Player (DontDestroyOnLoad)
├── MainCamera (DontDestroyOnLoad)
├── Canvas (DontDestroyOnLoad)
└── GlobalEventSystem (DontDestroyOnLoad)

Demo_Combat (关卡内容)
├── Ground, LevelRoot/NormalRoom, Environment, layer
├── SceneRuntime (SceneGameplayMarker) → PlayerRespawnPoint (SceneRespawnPoint, pos=(12,-1.3), fallLimitY=-15)  [Stage 32]
└── BossSceneTransitionController

流程: Demo_Combat → BossRoom
1. BossPortal.onPlayerEnterPortal → BossSceneTransitionController.TransitionToBossRoom()
2. LoadSceneAsync("BossRoom", Additive)
3. Validate MainGroundCollider
4. SetActiveScene(BossRoom)
5. GlobalRuntimeBootstrap.TeleportPlayer(spawn) + Physics2D.SyncTransforms
6. WaitForFixedUpdate
7. GlobalRuntimeBootstrap.SnapCameraToPlayer
8. UnloadSceneAsync(Demo_Combat)

流程: BossRoom 直接启动
1. GlobalRuntimeAutoLoader.EnsureRuntimeRoot() (BeforeSceneLoad)
2. Resources.Load → Instantiate GlobalRuntimeRoot prefab
3. DontDestroyOnLoad
4. BossRoom scene loads
5. BossRoomSceneController.Start() → PlacePlayerAtSpawn()
```

### Build Settings 场景列表
```
0: Assets/Scenes/MainMenu.unity
1: Assets/Scenes/Demo_Combat.unity
2: Assets/Scenes/BossRoom.unity
```

## 11. Editor
编辑器工具脚本。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| CardwinSceneBuilder.cs | `CardwinSceneBuilder` | **DISABLED** — 仅显示禁用提示弹窗 | `RebuildCleanDemoScene()` | 弹窗提示已禁用 | Tools/Cardwin/Rebuild Clean Demo Scene (disabled) | DISABLED |
| CardAssetCreator.cs | `CardAssetCreator` | 独立卡牌资产创建工具（不依赖SceneBuilder） | `CreateBasicCards()`, `CreateOrUpdateCard()` | 菜单入口/检查PlayMode / 创建/更新Strike/Guard/Heal/Focus | Tools/Cardwin/Create Basic Card Assets | 已完成 |
| MagazinePreviewUIEditor.cs | `MagazinePreviewUIEditor` | 自定义 Inspector：预览敌方/自身射击动画按钮 + 重置预览 | `OnInspectorGUI()`, `AnimateEnemy()`, `AnimateSelf()` | EditorApplication.update驱动编辑器动画 / PreviewMode + Progress slider | (Editor) | Stage 19 新增 |
| CardDatabaseEditorUtility.cs | `CardDatabaseEditorUtility` | 扫描 Assets/Data/Cards 下所有 CardData → 创建/更新 CardDatabase.asset → 调用 ValidateDatabase | `RebuildCardDatabase()`, `EnsureCardsFolder()` | 菜单 Tools/Cardwin/Rebuild Card Database / 排除CardDatabase自身 / 排除PlayMode | Editor | 已完成 |
| CardConfigValidator.cs | `CardConfigValidator` | **[STAGE 10C]** 卡牌配置合法性检查器：扫描CardData+CardDatabase / 检查CardID/Type/GoodEvil/IsOffensive/效果实现/数值异常/CardDatabase重复null旧资产/Reward池/背包测试库存 / 输出CardValidationReport.txt | `Validate()`, `ScanCardDataAssets()`, `CheckBasicFields()`, `CheckTypeAndUseTarget()`, `CheckGoodEvilCost()`, `CheckIsOffensive()`, `CheckEffectImplementation()`, `CheckNumericValues()`, `CheckCardDatabase()`, `CheckRewardPool()`, `CheckInventoryTestStock()`, `GenerateReport()`, `SaveReport()` | Tools/Cardwin/Validate Card Configs (菜单) | 已完成 (Stage 10C) |

## 12. Scenes

| 场景名 | 用途 | 当前状态 |
|--------|------|----------|
| `Demo_Combat.unity` | 主要测试场景。Stage 31 后只保留关卡内容（Ground/LevelRoot/Environment/BossSceneTransitionController）。Player/Camera/Canvas/EventSystem 由 GlobalRuntimeRoot 自动提供 | 活跃 — Stage 31 重构 |
| `BossRoom.unity` | Boss 房间场景：MainGround+墙体+平台+SafetyFloor+出生点+场景控制器+EditorPreviewCamera。Start()自动放置Player | 活跃 — Stage 31 更新 |
| `MainMenu.unity` | 主菜单：New Game / Continue / Settings / Quit，SettingsPanel 含 Volume / Fullscreen / Resolution | 活跃 |
| `CardwinSceneBuilder` | 备份恢复工具（禁用） | 备份 |

`Demo_Combat.unity` 当前已存在 `LevelRoot/Enemies`，包含 3 个近战敌人和 3 个远程敌人；`LevelRoot` 已挂载 `DemoSceneRuntimeBootstrapper`。Stage 8A.3 已完成基础运行验证；后续仍需 Level Polish / Enemy Tuning 做路线节奏、相机边界和战斗数值打磨。

## 13. Projectile Prefab

| 路径 | 说明 | 状态 |
|------|------|------|
| `Assets/Prefabs/Projectiles/Projectile_Test.prefab` | 测试投射物：SpriteRenderer + Kinematic Rigidbody2D(gravity=0) + CircleCollider2D(isTrigger) + Projectile | 已创建 |

## 14. Enemy Prefabs

| 路径 | 说明 | 状态 |
|------|------|------|
| `Assets/Prefabs/Enemies/MeleeEnemy.prefab` | 近战敌人：红色SpriteRenderer/Dynamic Rigidbody2D/BoxCollider2D/Health(30)/MeleeEnemyController | 新增（Stage 8A.1） |
| `Assets/Prefabs/Enemies/RangedEnemy.prefab` | 远程敌人：紫色SpriteRenderer/Kinematic Rigidbody2D(g=0)/BoxCollider2D/Health(20)/RangedEnemyController(binds EnemyProjectile) | 新增（Stage 8A.1） |
| `Assets/Prefabs/Enemies/EnemyProjectile.prefab` | 敌人子弹：紫色SpriteRenderer(sortingOrder=150)/Dynamic Rigidbody2D(gravity=0, Continuous)/CircleCollider2D(isTrigger)/EnemyProjectile，scale=(0.45,0.20,1) | 已完成（Stage 8A.3可见性与命中修复） |

## 15. Audit Documents (Stage 11A)

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
| Docs/BulletSystemAudit.md | **[Stage 56]** 子弹系统完整审计：3 套飞行物(Projectile/EnemyProjectile/RhythmHomingBullet)+Boss hitscan 技能；卡牌/弹夹/子弹关系；真实 Tag/Layer/Collider 要求；新增普通弹/音游弹步骤；Lua 热更改造建议与最小路线。仅分析无代码改动 |

## 16. Characters — Gothic Nun (NEW — Stage 13A)

哥特修女 2D 角色分块 PNG 导入与刚性骨骼装配。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| GothicNunImporter.cs | `GothicNunImporter` | Editor 工具：扫描桌面PNG→复制到 PartsRaw→配置TextureImporter→创建Assembly预制体→创建测试场景→生成报告 | `ImportAndAssemble()`, `ScanAndCopyPngs()`, `ConfigureAllTextureImporters()`, `CreateOrUpdatePrefab()`, `CreateOrUpdateTestScene()`, `GenerateReport()` | 菜单 Tools/GothicNun/Import And Assemble Character | Editor | 已完成 |
| GothicNunAssemblyDebug.cs | `GothicNunAssemblyDebug` | Inspector 调试组件：开启/关闭参考图、调节透明度、一键重置Transform | `OnValidate()`, `ApplyReferenceState()`, `ResetAllToZero()` | 挂载到 GothicNun_Assembly | 已完成 |
| GothicNunRigPoseTest.cs | `GothicNunRigPoseTest` | Inspector 关节旋转测试：Head/Torso/Shoulder/Elbow/Wrist/Hip/Knee/Ankle 14路旋转参数+测试预设 | `OnValidate()`, `AutoBindJoints()`, `ApplyRotations()`, `ResetAllJoints()` | 挂载到 GothicNun_Rig | 已完成 |

### Character Assets

| 路径 | 说明 | 状态 |
|------|------|------|
| `Assets/Characters/GothicNun/PartsRaw/` | 13 PNG 部件（896x1152, PPU=100） | 已导入 |
| `Assets/Characters/GothicNun/Prefabs/GothicNun_Assembly.prefab` | 原始拼装预制体（13 SpriteRenderer, 全零Transform, 分类节点） | 已完成 |
| `Assets/Characters/GothicNun/Prefabs/GothicNun_Rig.prefab` | 刚性骨骼预制体（17骨骼节点, 2D Transform父子链） | 已完成 |
| `Assets/Characters/GothicNun/Scenes/GothicNun_AssemblyTest.unity` | 拼装测试场景（正交Camera, 灰色背景） | 已完成 |
| `Assets/Characters/GothicNun/Scenes/GothicNun_RigTest.unity` | 骨骼测试场景 | 已完成 |
| `Assets/Characters/GothicNun/Reports/GOTHIC_NUN_IMPORT_REPORT.md` | 导入报告 | 已完成 |
| `Assets/Characters/GothicNun/Reports/GOTHIC_NUN_RIG_REPORT.md` | 骨骼报告 | 已完成 |

### Body Parts (13 total, all 896x1152 PPU=100 Alpha=Yes)

| 文件名 | 语义 | 绑定关节 | Sorting Order |
|--------|------|----------|---------------|
| head.png | Head | HeadJoint | 60 |
| 躯干.png | Torso | TorsoJoint | 30 |
| ass.png | Hip | Pelvis | 20 |
| 右臂.png | RightUpperArm | Shoulder_R | 40 |
| 左臂.png | LeftUpperArm | Shoulder_L | 41 |
| 右小臂.png | RightForearm | Elbow_R | 42 |
| 左小臂.png | LeftForearm | Elbow_L | 43 |
| 右手.png | RightHand | Wrist_R | 50 |
| 左手.png | LeftHand | Wrist_L | 51 |
| 右腿.png | RightThigh | Hip_R | 21 |
| 左腿.png | LeftThigh | Hip_L | 22 |
| 右脚.png | RightFoot | Ankle_R | 23 |
| 左脚.png | LeftFoot | Ankle_L | 24 |

### Missing Parts
- 无头发 (hair)、头纱 (veil)、面部细节
- 无小腿/胫骨 (calf/shin) — 膝关节旋转后露缝
- 无肩甲 (shoulder armor)
- 无参考图 (reference image)

### Game Integration (Stage 13B)

| 文件 | 说明 | 状态 |
|------|------|------|
| `GothicNun_PlayerVisual.prefab` | 正式游戏视觉 Prefab（基于 Rig，移除 PoseTest，重置关节旋转） | 已完成 |
| `Demo_Combat_Before_GothicNun.unity` | 集成前备份 (Assets/Scenes/Backups/) | 已完成 |

**Player 视觉替换方案**：
- `Demo_Combat.unity` 中 Player 根节点原 SpriteRenderer 已禁用
- 新增 `Player/VisualRoot`（localScale=0.10, localY=-0.50）挂载 GothicNun_PlayerVisual
- 朝向逻辑：利用 `PlayerController2D.FlipSprite()` 翻转 `transform.localScale.x`，VisualRoot 作为子对象自动镜像
- GroundCheck、FirePoint、Collider、Rigidbody2D 未受影响
- 所有游戏系统保持原有功能

## 17. Animation Sample (Stage 13C)

外部 2D 动画角色素材导入、AnimationClip 创建、Animator Controller 构建、Demo_Combat 视觉替换。

| 文件名 | 类名 | 主要职责 | 状态 |
|--------|------|----------|------|
| SamplePlayerAnimationBridge.cs | `SamplePlayerAnimationBridge` | Animator 参数桥接：读取 Rigidbody2D.velocity→Speed/VerticalVelocity, Physics2D→Grounded, Health.OnDeath→Dead | 已完成 |

### Animation Sample Assets

| 路径 | 说明 | 状态 |
|------|------|------|
| `AnimationSample/Raw/` | 17 PNG 动画帧 (512×512, PPU=100, 含 sword 版) | 已导入 |
| `AnimationSample/Animations/Sample_Idle.anim` | Idle 4帧 8FPS Loop | 已创建 |
| `AnimationSample/Animations/Sample_Run.anim` | Run 4帧 12FPS Loop | 已创建 |
| `AnimationSample/Animations/Sample_Jump.anim` | Jump 2帧 8FPS | 已创建 |
| `AnimationSample/Animations/Sample_Death.anim` | Death 3帧 8FPS | 已创建 |
| `AnimationSample/Animations/Sample_Attack.anim` | Attack 4帧 12FPS | 已创建 |
| `AnimationSample/Controllers/SamplePlayerAnimator.controller` | 5状态 Animator (Speed/Grounded/VerticalVelocity/Attack/Dead) | 已创建 |
| `AnimationSample/Prefabs/SamplePlayerVisual.prefab` | SpriteRenderer+Animator | 已创建 |

### 18. Gothic Nun Frame Animation (Stage 13G — Animation Transition Fix)

| 文件名 | 类名 | 主要职责 | 状态 |
|--------|------|----------|------|
| GothicNunAnimationBridge.cs | `GothicNunAnimationBridge` | 纯视觉桥接: Speed/Grounded/VV/Dead/MoveRequested更新 / 4种动作(RedEnemyShot/BlueEnemyShot/BlueSelfAction/RedSelfAction)统一0.4s恢复 / 不执行任何卡牌逻辑 (14B) | 已完成 (14B) |
| CardVisualEventBus.cs | `CardVisualEventBus` (static) | 静态事件总线：CardEffectExecutor→AnimationBridge 通知 FireRed/FireBlue/SelfActionRed/SelfActionBlue | 已完成 |

### Gothic Nun Frame Animation Assets (Stage 13F — Idle1Fix, 14 images, no bg removal)

| 路径 | 说明 | 状态 |
|------|------|------|
| `FrameAnimation/RawOriginal/` | 14 PNG 桌面原图拷贝 (1254×1254, Format32bppArgb, 自带Alpha) | 已导入 |
| `FrameAnimation/Imported/` | 14 PNG 正式动画素材 (1254×1254, PPU=100, Alpha=Yes, 未做任何背景处理) | 已导入 |
| `FrameAnimation/Animations/GothicNun_Idle.anim` | Idle **1帧** (idle_0 only) Loop | 已创建 |
| `FrameAnimation/Animations/GothicNun_Run.anim` | Run 4帧 12FPS Loop | 已创建 |
| `FrameAnimation/Animations/GothicNun_Jump.anim` | Jump 2帧 8FPS | 已创建 |
| `FrameAnimation/Animations/GothicNun_Death.anim` | Death 3帧 8FPS | 已创建 |
| `FrameAnimation/Animations/GothicNun_BlueSelfBuff.anim` | 蓝枪自身强化 0.14s | 已创建 |
| `FrameAnimation/Animations/GothicNun_BlueEnemyShot.anim` | 蓝枪对敌射击 **0.4s** (13H) | 已创建 |
| `FrameAnimation/Animations/GothicNun_RedSelfBuff.anim` | 红枪自身强化 0.14s | 已创建 |
| `FrameAnimation/Animations/GothicNun_RedEnemyShot.anim` | 红枪对敌射击 **0.4s** (13H) | 已创建 |
| `FrameAnimation/Controllers/GothicNunPlayerAnimator.controller` | 8状态+MoveRequested: 4动作状态(RedEnemyShot/BlueEnemyShot/BlueSelfAction/RedSelfAction)exitT=1; SelfActionBlue/SelfActionRed Triggers (14B) | 已创建 |
| `FrameAnimation/Prefabs/GothicNunFrameVisual.prefab` | SpriteRenderer+Animator+GothicNunAnimationBridge | 已创建 |

### CardEffectExecutor 修改
- ExecuteLeft: 弹丸生成后 → Damage 效果触发 FireRed 动画, 非 Damage 触发 FireBlue
- ExecuteRight: 自身效果后 → Damage 效果触发 SelfBuffRed 动画, 非 Damage 触发 SelfBuffBlue

### Demo_Combat 当前视觉状态 (Stage 13F)
- GothicNunFrameVisual 激活 (frame animation, Imported sprites, 未去背景)
- Idle 仅使用 1 张 (gothic_nun_idle_0.png)
- SamplePlayerVisual 禁用保留 (SamplePlayerVisual_DISABLED)
- GothicNun_PlayerVisual_DISABLED 禁用保留 (rigid bone)
- VisualRoot pos=(0,0,0) scale=(1,1,1)
- GothicNunFrameVisual localPos=(0,-0.10,0) localScale=(0.15,0.15,1)
- RawOriginal/ 保留 14 张桌面原始图（Format32bppArgb, 自带Alpha）
- Imported/ 14 张正式素材（与原图完全一致, 未做任何处理）
- 0 Transform 曲线 / 0 Console Error

---

## 19. Mirror Saintess Boss (战斗闭环 V1 + 移动 V2 — Stage 35/36)

镜面圣女 Boss。命名空间：`MirrorSaintessBossPack`（脚本）/ `Cardwin.Boss`（HUD/Mover）。Stage 35 战斗闭环 V1；**Stage 36 V2**：(1) 修复实战部位破坏——Projectile 命中优先 `MirrorSaintessBossPart`(高于 BossRoot)+放大部位 Collider 覆盖躯干两侧无缝隙+`[ProjectileHit]` 日志+破坏闪烁/抖动/明显日志/HUD；(2) `allowDirectBodyDamage=false` 默认禁止直接打身体绕过部位；(3) 新增最小移动 AI `MirrorSaintessBossMover`（Kinematic 巡逻/靠近/停距/Phase2 提速/Dead 停/锁 Y 不掉落/边界内）。不做复杂技能。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| MirrorSaintessBoss.cs | `MirrorSaintessBoss` | **[Stage 35/36]** Boss 战斗根：int 总 HP(400)/Phase1-2-Dead/受击-死亡。实现 `IDamageable`(根)。`DealBossDamage` 漏斗：扣总HP→死亡优先→50%进Phase2(一次)→否则Hurt。**Stage 36: `allowDirectBodyDamage`(默认false→核心未破时直接打身体无效)；`CanMove`(非Dead且非Phase2过渡时可移动)供 Mover；Phase2 过渡期 `_inPhase2Transition` 停移动**。事件 OnHealthChanged/OnPartStateChanged/OnPhaseChanged/OnBossDefeated；ResetBoss/ContextMenu | `TakeHit(int,GO)`,`DealBossDamageFromPart()`,`DealBossDamage()`,`NotifyPartBroken()`,`EnterPhase2()`,`Die()`,`ResetBoss()`,`ForcePlayState()`,`CanMove` | Projectile/部位转发/Mover/HUD | Projectile / Part / Mover / BossHUD | V2 完成（Stage 36） |
| MirrorSaintessBoss.cs | `MirrorSaintessPhase` (enum) | 相位：Phase1/Phase2/Dead | — | — | MirrorSaintessBoss | 已完成 |
| MirrorSaintessBossPart.cs | `MirrorSaintessBossPart` | **[Stage 35/36]** 可破坏部位，实现 `IDamageable`：每次命中→恒向 Boss 总 HP 转发(破损后仍转发,保证可击杀)+扣部位 HP→0 时一次 BreakPart。**Stage 36: 破坏明显反馈(闪红+抖动 ShakeRoutine+`[BossPart] X broken.` 日志)+`[BossPart] X hit -n` 命中日志+OnDrawGizmos(intact 品红/broken 灰描边)+可选运行时半透明 hitbox(showRuntimeHitbox)**。partId/maxHp/currentHp/isBroken/visualRenderer/hitCollider/disableColliderWhenBroken(false)。ContextMenu(Damage25/Break/Reset) | `TakeHit(int,GO)`,`TakeDamage(float)`,`BreakPart()`,`ResetPart()`,`OnDrawGizmos()` | Projectile / Boss / ContextMenu | V2 完成（Stage 36） |
| MirrorSaintessBossPart.cs | `MirrorSaintessPartType` (enum) | 部位类型：ChestCore/BlueGun/RedGun | — | — | MirrorSaintessBoss / Installer | 已完成 |
| MirrorSaintessBossMover.cs | `MirrorSaintessBossMover` (Cardwin.Boss) | **[Stage 36]** 最小移动 AI：Kinematic+MovePosition 锁 Y(不掉落)，在 leftBound/rightBound 间巡逻；玩家进 detectRange 则靠近，距离<stopDistanceToPlayer 停；Phase2 用 phase2MoveSpeed(1.8>1.2)；`!boss.CanMove`(Dead/Phase2过渡)停；只翻转 visualRoot(Body) 朝向玩家，不翻部位/根；边界 Gizmos。无跳跃/寻路/接触伤害 | `Awake()`,`Start()`,`FixedUpdate()`,`UpdateFacing()`,`FindPlayer()`,`ResolveBounds()`,`OnDrawGizmos()` | 挂 Boss 根(RequireComponent MirrorSaintessBoss) | Boss(自身FixedUpdate) | V2 完成（Stage 36） |
| BossHUD.cs | `BossHUD` (Cardwin.Boss) | **[Stage 35]** BossRoom 本地 HUD：运行时自建 ScreenSpaceOverlay Canvas(不动 GlobalRuntimeRoot/EventSystem)：Boss 名/总 HP 条/3 部位状态(OK/BROKEN)/DEFEATED。订阅 Boss 事件 + LateUpdate 轮询兜底 | `Start()`,`LateUpdate()`,`BuildUI()`,`RefreshBar()`,`RefreshParts()`,`Handle*` | 挂 BossRoom 的 BossHUD 对象 | (自身/场景) | 已完成（Stage 35） |
| MirrorSaintessProjectile.cs | `MirrorSaintessProjectile` | 原型 Boss 子弹：Rigidbody2D.velocity 飞行+寿命自毁+命中Player SendMessageUpwards("TakeDamage")。本轮未接玩家 | `Awake()`, `OnEnable()`, `Fire()`, `Update()`, `OnTriggerEnter2D()` | 缓存刚体 / 寿命销毁 / 设方向速度 / 无刚体兜底位移 / 命中Player标签转交伤害 | MirrorSaintessBoss.FireProjectile | 原型完成（未接入） |
| Editor/MirrorSaintessBossInstaller.cs | `MirrorSaintessBossInstaller` (Editor) | 一键生成工具：贴图转Sprite(PPU=256) + 创建 AnimatorController/6 AnimationClip + 拼装 Prefab → `Assets/Prefabs/Boss/MirrorSaintessBoss_Prototype.prefab` | `BuildPrototypePrefab()` | 菜单 `Tools/Mirror Saintess Boss/Build Prototype Prefab` | (Editor) | 已完成（Stage 34） |

### Mirror Saintess Boss Assets

| 路径 | 说明 | 状态 |
|------|------|------|
| `Assets/MirrorSaintessBossPack/Art/Sprites/Boss_Body_Transparent.png` | Boss 主体（Body 渲染） | 已导入（Sprite,PPU256） |
| `Assets/MirrorSaintessBossPack/Art/Parts/{ChestCore,BlueGun,RedGun}_{Intact,Broken}.png` | 三个可破坏部位 完整/破损 贴图 | 已导入 |
| `Assets/MirrorSaintessBossPack/Art/Animations/Frames/{Idle,CastBlue,CastRed,Hurt,Phase2,Death}/` | 6 套逐帧动画 PNG | 已导入 |
| `Assets/MirrorSaintessBossPack/Generated/MirrorSaintessBoss.controller` | 6 状态 AnimatorController(无 transition，原型) | 已生成 |
| `Assets/MirrorSaintessBossPack/Generated/MirrorSaintess_{Idle,CastBlue,CastRed,Hurt,Phase2,Death}.anim` | 6 个逐帧 AnimationClip(绑定 Body 的 m_Sprite) | 已生成 |
| `Assets/Prefabs/Boss/MirrorSaintessBoss_Prototype.prefab` | Boss Prefab：root(MirrorSaintessBoss+Rigidbody2D Kinematic+Animator+**MirrorSaintessBossMover(Stage36)**，无根 Collider) / Body / Part_ChestCore(hp120,col 世界1.8×2.6) / Part_RightHand_BlueGun(hp80,1.7×2.4) / Part_LeftHand_RedGun(hp80,1.7×2.4)（**Stage36 放大覆盖躯干两侧**，trigger，disableColliderWhenBroken=false）/ FirePoint×2。总 HP=400。无 Missing Script | 已更新（Stage 36） |

### Mirror Saintess Boss in BossRoom (Stage 35/36)
- `BossRoom.unity` root 实例 `MirrorSaintessBoss_Prototype` pos=(8,-2.372,0)；总 HP 400，部位 Chest120/Blue80/Red80。
- root 对象 `BossHUD`(本地 Overlay Canvas)。
- **Stage 36 新增** root `BossArea` → 子 `BossLeftMoveBound`(x=-6) / `BossRightMoveBound`(x=9)（Transform 标记，可编辑，Mover 用）；Mover 已 wire leftBound/rightBound/visualRoot(Body)/bossRigidbody。
- 实例 `startAttackLoop=false`。
- 未改 BossSpawnPoint/BossPlayerSpawnPoint/MainGround/SafetyFloor/EditorPreviewCamera/GlobalRuntimeRoot/SceneRespawnPoint。
### BossRoom 美术装饰 (Stage 37 — 哥特 Boss 房换装)

> 实测当前 BossRoom 几何（权威，旧索引数值已过时）：`MainGround` pos=(0,-10) scale 40×1 → **顶面 y=-9.5**，x[-20,20]，Layer=Ground；`LeftWall/RightWall` (±20,-10) scale 1×12，y[-16,-4]；**当前场景无 LeftPlatform/RightPlatform/SafetyFloor**。仅 6 个 Collider2D：MainGround + 2 墙 + 3 个 Boss 部位 trigger。

美术与碰撞**完全分离**：所有装饰为纯 SpriteRenderer，无任何 Collider；Collider2D 总数保持 6 不变。

| 导入素材 (Assets/Art/Gothic/BossRoom/) | 来源 | 尺寸 | 用途 | 导入设置 |
|---|---|---|---|---|
| `Background/background.png` | 桌面 bossR | 1672×941 | 背景教堂 | Sprite/Single/PPU100/FullRect/noMips/Bilinear/Uncompressed/noPhysShape |
| `Wall/wall.png` | 桌面 bossR | 1254×1254 | 左右墙装饰 | 同上 |
| `Floor/ground.png` | 桌面 bossR | 1672×941 | 地板视觉 | 同上 |
| `Platform/taijie.png` | 桌面 bossR | 1672×941 | 中央祭坛/台阶装饰(非平台) | 同上 |
| `Lighting/PurpleGlow.png` | 程序生成(radial) | 256×256 | 紫色光晕 | Sprite |

BossRoomEnvironment 美术层级（Default 排序层 + sortingOrder 控制前后）：
```
BossRoomEnvironment
├── MainGround / LeftWall / RightWall   (原碰撞体，未改)
├── BackgroundRoot
│   ├── BossRoom_Background  (background, order -100, 世界 48.5×28.2 @ (0,-5.5), 暗化 0.72)
│   └── AltarSteps_Decor     (taijie,     order -30,  中央祭坛装饰, 无碰撞)
├── WallDecorRoot
│   ├── LeftWallDecor_01/02  (wall, order -50, x=-17, 暗化)
│   └── RightWallDecor_01/02 (wall, order -50, x=17,  暗化)
├── FloorVisualRoot
│   └── MainGroundVisual     (ground, order 0, 世界 40×6, **顶面 y=-9.5 对齐 MainGround 碰撞顶**)
├── PlatformVisualRoot       (空：当前场景无平台碰撞体，不放伪平台以免误导)
├── ForegroundDecorRoot      (空：避免遮挡战斗；项目无 Foreground 排序层)
└── LightingDecorRoot
    └── CenterPurpleGlow      (PurpleGlow, order -90, 半透明 0.55)
```
排序：背景 -100 < 光晕 -90 < 墙 -50 < 祭坛 -30 < 地板 0 < Boss Body 10 < 部位 20 < 子弹 100 < 玩家(Character 层) → 角色/子弹/部位永远在美术之上，可读性保证。
背景为 BossRoom 场景对象（**非 DontDestroyOnLoad**），直接开/传送进入均覆盖视野无黑边。
Play 实测（Stage 37）：玩家落在地板 y≈-9（站立正常），背景/地板/墙/Boss/HUD 均可见，Boss 仍移动(8→-4)，6 个 Collider 不变，0 红色错误。截图 `Assets/Screenshots/bossroom_decorated_play.png`。

---

## 20. Mirror Angel Boss — 白色天使镜视觉替换 (Stage 39)

> 本轮**仅替换 Boss 外观**：用 `C:\Users\86189\Desktop\base` 的白色天使镜素材替换 BossRoom 旧 Boss 的视觉与 Animator，**完整复用旧 Boss 战斗逻辑**（MirrorSaintessBoss / MirrorSaintessBossPart×3 / MirrorSaintessBossMover / IDamageable / BossHUD / Rigidbody2D / 部位 Collider / 总 HP 400）。未修改任何战斗脚本、玩家、Projectile、卡牌/弹匣/背包/设置/传送门/地面/出生点。

### 新素材 (Assets/Art/Gothic/Boss/MirrorAngel/States/，全部透明 PNG, Sprite/Single/PPU100/FullRect/AlphaTrans/Bilinear/Uncompressed/noMips)
| 文件 | 来源(desktop base, 含空格) | 用途 |
|---|---|---|
| `MirrorAngel_Idle_0.png` | `Idle_0  .png` | Idle |
| `MirrorAngel_Walk_0.png` | `Walk_0  .png` | Walk 帧0 |
| `MirrorAngel_Walk_1.png` | `Walk_1  .png` | Walk 帧1 |
| `MirrorAngel_Dash_0.png` | `Dash_0  .png` | Dash |
| `MirrorAngel_Fly_0.png` | `Fly_0  .png` | Fly |
| `MirrorAngel_CastMirror_0.png` | `CastMirror_0   .png` | CastMirror |
| `MirrorAngel_Death_0.png` | `Death_0.png` | Death |
> `Parts/` 文件夹已建但为空（base 无可破坏部件图）。7 张图均 1254×1254、Format32bppArgb、四角 A=0 → 全透明，无黑底。

### 动画资产 (Assets/Animations/Boss/MirrorAngel/) — 均绑定 `Body` 子物体 SpriteRenderer.m_Sprite
| 资产 | 帧 | 循环 |
|---|---|---|
| `MirrorAngel_Idle.anim` | Idle_0 | Loop |
| `MirrorAngel_Walk.anim` | Walk_0 + Walk_1 (8fps) | Loop |
| `MirrorAngel_Dash.anim` | Dash_0 | Loop |
| `MirrorAngel_Fly.anim` | Fly_0 | Loop |
| `MirrorAngel_CastMirror.anim` | CastMirror_0 | Loop |
| `MirrorAngel_Death.anim` | Death_0 | **不循环** |
| `MirrorAngelBossAnimator.controller` | 12 状态（无 transition，配合 `animator.Play`），默认 Idle | — |

**Animator 状态映射**（控制器含旧脚本 `ForcePlayState` 会 Play 的全部状态名，避免缺状态报错）：
`Idle/Walk/Dash/Fly/CastMirror/Death` → 各自新 Clip；临时映射 `Hurt→Idle`（靠 Body 受击闪烁）、`Phase2→CastMirror`、`CastBlue→CastMirror`、`CastRed→CastMirror`、`Stunned→Idle`、`Dead→Death`。实测 Play 无任何缺状态 warning。

### 新 Prefab `Assets/Prefabs/Boss/MirrorAngelBoss.prefab`（由旧 prefab `AssetDatabase.CopyAsset` 复制后改装）
- root 改名 `MirrorAngelBoss`；Animator → `MirrorAngelBossAnimator`。
- `Body`：sprite=MirrorAngel_Idle_0，localScale (0.42,0.42,1)（1254@PPU100=12.54→世界 5.27 高，适合横版），localPos (0,2.2,0)，order 10。
- **3 个部位 SpriteRenderer 禁用**（隐藏旧枪/镜美术，保证新天使外观纯净）；部位 **Collider/script/HP/partType 完全保留**（Chest120/Blue80/Red80，trigger，disableColliderWhenBroken=false）。
- 复用组件：MirrorSaintessBoss(总 HP400)/MirrorSaintessBossPart×3/MirrorSaintessBossMover/Rigidbody2D(Kinematic g=0)/FirePoint×2。无 Missing Script。

### BossRoom 场景实例 (Stage 39)
- 旧实例 `MirrorSaintessBoss_Prototype`（pos 8,-2.37）**已从场景删除**；新实例 `MirrorAngelBoss` 放在**同一 pos (8,-2.37,0) scale1**。
- Mover 重新 wire：leftBound=BossLeftMoveBound(-6) / rightBound=BossRightMoveBound(9) / visualRoot=Body / bossRigidbody=root / artFacesRight=false（沿用旧设定）。
- BossHUD（场景对象，无 Inspector 引用）运行时 `FindObjectOfType<MirrorSaintessBoss>()` 自动绑定新 Boss。
- 部位命中盒世界坐标与旧 Boss **完全一致**（Chest 8.0/0.58、Blue 6.75/-1.02、Red 9.25/-1.02），落在新 body 轮廓内；偏差：新 body 头部(y>1.88)/脚部(y<-2.22)无命中盒（本轮允许，命中逻辑不变）。
- 未改 BossSpawnPoint/BossPlayerSpawnPoint/MainGround/SafetyFloor/BossMoveBounds/EditorPreviewCamera/GlobalRuntimeRoot/SceneRespawnPoint。

### 旧 Boss 资源处理
- 旧 prefab → `Assets/_Deprecated/Boss/OldMirrorSaintess/MirrorSaintessBoss_Prototype_DEPRECATED.prefab`（移动+改名，未物理删除）。
- 旧战斗脚本（MirrorSaintessBoss.cs / MirrorSaintessBossPart.cs / MirrorSaintessBossMover.cs / BossHUD.cs / IDamageable.cs / Projectile.cs）**全部保留**，新 Boss 仍复用。
- 旧美术 `Assets/MirrorSaintessBossPack/Art/` **保留原位**（被 deprecated prefab + 旧 controller + installer 引用，本轮不动；后续确认稳定再清理）。

### 测试（Play 实测，0 红色错误 / 0 warning）
- A：BossRoom 仅 1 个 Boss = MirrorAngelBoss（白色天使镜），旧实例已删。
- B：直接 Play BossRoom — Boss 显示新 Idle 图、移动(X 8→6.62, Y 锁 -2.37 不掉落)、BossHUD 自建、animator=Idle。
- C：模拟玩家子弹命中（IDamageable.TakeHit，即 Projectile 调用的同一路径）：HP 400→320(Blue破)→240(Red破)→180→**Phase2**；致命→HP0→**Death(MirrorAngel_Death)**、CanMove=False(停动)、全 Collider 禁用。
- D：Demo_Combat → `TransitionToBossRoom()` 加性载入 BossRoom，全场仅 1 Boss = MirrorAngelBoss(新图)、HUD 在、0 红错。
- 截图：`Assets/Screenshots/mirrorangel_scene_check.png`。

---

## 21. MirrorAngel Boss — 真动画状态机 + 重力运动 (Stage 40)

> 修复 Stage 39 遗留的「Boss 只会站着平移、不播放真动画、无重力」。本轮：给 Boss 接**参数驱动 Animator**（真正按状态切换 Idle/Walk/Dash/Fly/CastMirror/Death）+ **Dynamic Rigidbody2D 重力**（落地/不穿地/不旋转）+ **重力移动脚本**（地面巡逻/冲刺/短飞）。**未改任何战斗 .cs**（桥接只读 `MirrorSaintessBoss` 已公开成员）。

### 新增脚本 (Assets/Scripts/Boss/，命名空间 Cardwin.Boss)
| 文件 | 类 | 职责 | 关键函数 | 状态 |
|---|---|---|---|---|
| `MirrorAngelBossGravityMover.cs` | `MirrorAngelBossGravityMover` | Dynamic RB 重力移动：地面 Walk(walkSpeed1.2，leftBound/rightBound 间巡逻/靠近玩家 stopDist3.5)、周期 Dash(冷却4s/时长0.35s/速度4.5)、周期短 Fly(冷却6s/时长1.2s/gravityScale→0 上浮 flyHeight2 +正弦漂浮，结束恢复 g=3 落地)；Death/Phase2(CanMove=false) 停。向下三射线 Ground 检测 IsGrounded。只翻 visualRoot(Body) 朝向。公开只读 IsGrounded/IsDashing/IsFlying/IsCasting/CurrentMoveSpeed。ContextMenu Force Dash/Fly/CastMirror | `FixedUpdate/UpdateGroundCheck/StartDash/StartFly/TickFly/ComputeWalkDir/ClampX/UpdateFacing` | 新增 |
| `MirrorAngelBossAnimatorBridge.cs` | `MirrorAngelBossAnimatorBridge` | 纯视觉桥接：每 Update 由 boss(IsDead/CanMove)+mover 状态写 Animator 参数 MoveSpeed/IsGrounded/IsFlying/IsDashing/IsCasting/IsDead。IsCasting=Phase2(CanMove=false 且未死)或 mover 调试施法。无战斗逻辑、不改任何脚本 | `Awake/Update` | 新增 |

### Animator 资产
- `Assets/Animations/Boss/MirrorAngel/MirrorAngelBoss.controller`（**参数驱动**，6 参数 MoveSpeed/IsGrounded/IsFlying/IsDashing/IsCasting/IsDead；12 状态；6 条 AnyState 过渡，优先级 Death>CastMirror>Dash>Fly>Walk(MoveSpeed>0.1&&IsGrounded)>Idle，均 hasExitTime=false/canTransitionToSelf=false）。
- 复用 Stage 39 的 6 个 Clip：`MirrorAngel_{Idle,Walk(2帧),Dash,Fly,CastMirror}`(Loop) + `MirrorAngel_Death`(不循环)，绑定 `Body` 的 m_Sprite。
- 含 6 个旧脚本兼容状态（Hurt→Idle / Phase2,CastBlue,CastRed→CastMirror / Stunned→Idle / Dead→Death），使 `MirrorSaintessBoss.ForcePlayState(animator.Play)` 永不缺状态报错。Death 无出口（不回 Idle）。

### Prefab `MirrorAngelBoss.prefab` 变更 (Stage 40)
- Animator controller → `MirrorAngelBoss.controller`（替换 Stage39 的 MirrorAngelBossAnimator）。
- **Rigidbody2D → Dynamic**，gravityScale=3，constraints=FreezeRotation(不倒下旋转)，interpolation=Interpolate，collisionDetection=Continuous。
- **新增身体 CapsuleCollider2D**（root，Vertical，size(1.5,3.0) offset(0,1.07)，非 trigger）：`includeLayers=Ground(8)` 强制与地面碰撞、`excludeLayers=Default(0)|Player(9)`→**不拦玩家子弹、不挡玩家**。capsule 底对齐 Body 视觉脚底。
- **移除旧 `MirrorSaintessBossMover`**（Kinematic 平移，与 Dynamic 冲突）；**新增 `MirrorAngelBossGravityMover` + `MirrorAngelBossAnimatorBridge`**（已 wire rb/boss/visualRoot=Body/animator/mover）。
- 部位 3 个 trigger Collider/HP(120/80/80)/IDamageable/IsDamageable 破坏逻辑全保留；Body sprite/scale(0.42)/localPos(0,2.2) 不变。

### BossRoom 实例 (Stage 40)
- 仍 1 个 `MirrorAngelBoss`（来自 prefab），spawn pos(8,-2.37)；运行时受重力落到 MainGround 顶（groundTop=-11.84，root 落到 -11.41，**脚底正好贴地、无穿透、rotZ=0**）。
- Mover wire：leftBound=BossLeftMoveBound(-6) / rightBound=BossRightMoveBound(9) / visualRoot=Body。BossHUD 仍自动绑定。
- 未改 SpawnPoint/MainGround/墙/SafetyFloor/EditorPreviewCamera/GlobalRuntimeRoot/SceneRespawnPoint/传送门/玩家。

### 测试（Play + 确定性步进实测，0 红错 / 0 warning）
- A 重力落地：root -2.38→-11.41，capsBottom=feetY=groundTop=-11.84，穿透0.005，rotZ=0（不穿地/不倒下）。
- B 状态贴图（直接驱动参数）：Idle→Idle_0 / Walk→Walk_0 / Dash→Dash_0 / Fly→Fly_0 / CastMirror→CastMirror_0 / Death→Death_0；Death 锁定不回 Idle。
- 集成（真 mover+bridge）：落地行走→**State=Walk/Walk_0（不再 Idle 平移）**；Dash→Dash_0；Fly→gravityScale=0 后恢复3。
- 战斗：部位 TakeHit HP 400→240(蓝红破)→180→Phase2(→CastMirror_0)→致命 0→Death_0、mover vel.x=0 停。
- 传送：Demo_Combat→BossRoom 仅 1 Boss，落地(-11.41 贴地 rotZ0)、有动画、HP 400→360 可战、HUD 在。

---

## 22. MirrorAngel Boss — 受击修复 + 仅 MainGround 碰撞 + 视觉置顶 (Stage 41)

> 修复用户实测「删旧组件后 Boss 打不中 / 被地形挡 / 被地形图遮挡」。**根因**：Stage 40 给身体 `CapsuleCollider2D` 设了 `excludeLayers=Default(0)|Player(9)=513` —— Default(0) 正是玩家子弹层，导致**子弹打到身体区域（部位之间/外侧）时身体 collider 直接不接触、无回调**；又因 `allowDirectBodyDamage=false`，即便接触也不扣血。部位 trigger 仍可命中（实测 part hit=True），所以表现为"瞄身体打不中"。本轮：身体 capsule 改为只排除 Player、开启 allowDirectBodyDamage、加 SortingGroup 置顶、加 CollisionFilter 让身体只与 MainGround 实体碰撞。**未改玩家/子弹伤害/普通敌人/IDamageable 接口/MainGround 玩法碰撞。**

### 新增脚本
| 文件 | 类 | 职责 | 状态 |
|---|---|---|---|
| `Assets/Scripts/Boss/MirrorAngelBossCollisionFilter.cs` | `MirrorAngelBossCollisionFilter` (Cardwin.Boss) | Start 时收集 Boss 自身**非 trigger 身体 collider**，对场景中所有其它 Collider2D 调 `Physics2D.IgnoreCollision`：仅与名为 `MainGround` 的不忽略，其余（round0/LeftWall/RightWall/平台/装饰）全部忽略 → Boss 只被 MainGround 承托。**不删/不禁用任何场景 collider**（其它角色照常使用），只对本 Boss 忽略。部位 trigger 不受影响（仍接子弹）。ContextMenu 可重应用 | 新增 |

### Prefab `MirrorAngelBoss.prefab` 变更 (Stage 41)
- 新增 root `SortingGroup`：Sorting Layer=Default，Order=**50** → 全部子 SpriteRenderer 统一渲染在地形装饰之上（背景 -100 < 墙 -50 < 光晕 -90 < 地板 0 < **Boss 50** < 子弹 100）。
- 身体 `CapsuleCollider2D`：`isTrigger=false`，`includeLayers=0`(默认矩阵)，`excludeLayers=Player(9)=512`（**移除对 Default 的排除** → 子弹层 Default 现可与身体接触并触发命中；仍永不挡玩家）。size(1.5,3.0) offset(0,1.07) 不变。
- `MirrorSaintessBoss.allowDirectBodyDamage=true`（打身体/根也能扣总血，保证"至少能被打中"；打部位仍优先且会破坏部位）。
- 新增 `MirrorAngelBossCollisionFilter` 组件。
- 部位 3 个 trigger Collider/HP/IDamageable/破坏逻辑全保留并重新确认 `isTrigger=true`、无 layer override。

### BossRoom 实例 (Stage 41)
- 仍 1 个 `MirrorAngelBoss`（无旧 Boss 实例）；RevertPrefabInstance 同步上述改动后重 wire mover bounds（BossLeftMoveBound/-6、BossRightMoveBound/9、visualRoot=Body）。spawn pos(8,-2.37)。
- 物理实测：身体 vs MainGround `IgnoreCollision=False`（承托）；vs round0/LeftWall/RightWall `=True`（忽略，不卡住）；落地 rotZ=0 不倒下不穿地。

### 测试（Play + 确定性步进，0 红错 / 0 warning）
- 受击：真 Projectile_Test 命中**部位** ChestCore→HP 400→370（部位扣血+转发）；命中**身体/根**（无部位处）→HP 370→340（allowDirectBodyDamage 生效）。
- 破坏/相位/死亡：Blue/Red TakeHit 破坏→HP 240→180→Phase2(CastMirror_0)→致命→Death(Death_0)；HUD 在。
- 碰撞过滤：身体只与 MainGround 碰撞，其它 Ground/装饰全忽略。
- 排序：SortingGroup Default/50 高于全部地形装饰 → Boss 不被地板/墙/背景图遮挡。
- 动画保留：Idle/Walk/Dash/Fly/CastMirror/Death 仍由 Stage40 状态机驱动。
- 传送 G：Demo_Combat→BossRoom 仅 1 Boss、落地 rotZ0、SortingGroup/Filter 在、射击部位 HP 400→360、HUD 在、0 红错。

---

## 23. MirrorAngel Boss — 简化为单 Body 受击目标 (Stage 42)

> 用户要求删除所有部位/FirePoint，Boss 只保留一个 `Body`。最终 Hierarchy = `MirrorAngelBoss / Body`。不做可破坏部位、不做发射、不做镜面技能。Boss 仍可被打、Phase2、Death、有动画、只被 MainGround 承托、显示在地形之上。**未改玩家/子弹伤害/普通敌人/MainGround 玩法碰撞。**

### 新增脚本
| 文件 | 类 | 职责 | 状态 |
|---|---|---|---|
| `Assets/Scripts/Boss/MirrorAngelBodyDamageReceiver.cs` | `MirrorAngelBodyDamageReceiver` (Cardwin.Boss) | 挂 `Body`，实现 `IDamageable.TakeHit(amount,source)` → 转发 `owner.TakeHit`（owner=`MirrorSaintessBoss` 根）。命中日志 `[MirrorAngelBoss] Body hit, damage=, hp=/`。替代被删除的部位受击 | 新增 |

### 删除内容（Prefab + 场景实例）
- 删除子对象：`Part_ChestCore` / `Part_RightHand_BlueGun` / `Part_LeftHand_RedGun` / `FirePoint_Blue` / `FirePoint_Red`。
- `MirrorSaintessBoss.destructibleParts` 序列化列表清空（无 Missing/0 NullRef）。
- `MirrorSaintessBossPart.cs` 脚本文件**保留**（不删，便于将来恢复部位系统）；只是场景/Prefab 不再使用。

### 最终 Hierarchy
```
MirrorAngelBoss
└─ Body
```

### Prefab `MirrorAngelBoss.prefab` 组件 (Stage 42)
- **Root `MirrorAngelBoss`**：`MirrorSaintessBoss`(IDamageable，总 HP400/Phase2/Death，allowDirectBodyDamage=true)、`Rigidbody2D`(Dynamic g3 FreezeRotation Interpolate Continuous)、`CapsuleCollider2D`(实体，非trigger，excludeLayers=Player(9)，只与 MainGround 碰撞由 Filter 保障)、`Animator`(MirrorAngelBoss controller)、`MirrorAngelBossGravityMover`、`MirrorAngelBossAnimatorBridge`、`MirrorAngelBossCollisionFilter`、`MirrorAngelBossEffectReceiver`、`MirrorAngelTripleBeamSkill`、`MirrorAngelFacingController`、**`MirrorAngelBossBrain`(Stage 46.3)**、`SortingGroup`(Default/50)。
- **`Body`**：`SpriteRenderer`(由 SortingGroup 管理，受动画驱动 m_Sprite)、`BoxCollider2D`(Hurtbox，**isTrigger=true**，size 7.5×10 本地→世界 ~3.15×4.2，覆盖躯干)、`MirrorAngelBodyDamageReceiver`(IDamageable，owner=根)。

### 受击链路
```
玩家子弹 → Body Trigger Collider → Projectile.HandleHit 找到 IDamageable(MirrorAngelBodyDamageReceiver)
→ owner.TakeHit → MirrorSaintessBoss 扣总 HP → BossHUD 更新 → ≤50% Phase2 → ≤0 Death → 子弹销毁
```
> 根 Capsule 也能被命中（allowDirectBodyDamage=true），双保险。Projectile.cs 未改（原 BossPart→IDamageable→Health 分支，BossPart 现已无，落到 IDamageable=Body receiver）。

### BossHUD 兼容 (Stage 42)
- `BossHUD.cs`：部位三行状态（BlueGun/Core/RedGun）改为单行 `Body: OK / DEAD`（`_blueText=_redText=null`，`RefreshParts` 只刷 Body 状态，不再读 IsBlueGunBroken/IsRedGunBroken/IsCoreBroken）。无找不到部位报错。`MirrorSaintessBoss` 的 IsXxxBroken 属性保留未删。

### 测试（Play + 确定性步进，0 红错/0 warning）
- Hierarchy：MirrorAngelBoss 下只有 Body（无 Part/FirePoint），missing 组件=0。
- 受击：真子弹命中 Body Hurtbox → HP 400→360（`[MirrorAngelBoss] Body hit` 日志）；经 receiver IDamageable.TakeHit(160) → 200 触发 Phase2(CastMirror_0)；TakeHit(300) → Death(Death_0)。
- 碰撞：身体 vs MainGround IgnoreCollision=False（承托落地 rotZ0），vs round0/LeftWall/RightWall=True（忽略不卡）。
- 排序：SortingGroup Default/50 高于地形装饰（背景-100/墙-50/地板0）。
- 动画：Idle→Idle_0 / Walk→Walk_0,Walk_1 / Dash→Dash_0 / Fly→Fly_0 / CastMirror→CastMirror_0 / Death→Death_0 全部正常。
- 传送：Demo_Combat→BossRoom 仅 1 Boss、只有 Body、落地 rotZ0、射击 Body HP 400→360、HUD 在。

---

## 24. 玩家子弹卡牌效果作用于 MirrorAngelBoss (Stage 43)

> 让玩家 Heal/Guard(护盾)/Focus/Damage 子弹命中 Boss 后也对 Boss 生效。**根因**：旧链路里 Boss 走 `IDamageable`，`Projectile` 只对它传 `ResolveGenericDamage`（Damage=card.damage×focus，非伤害=0），故 Heal/Guard/Focus 子弹对 Boss 无效。修复＝最小转发完整卡牌效果给 Boss，**不改玩家/发射/卡牌系统/普通敌人 Health 路径/伤害数值**。

### 新增脚本
| 文件 | 类/接口 | 职责 | 状态 |
|---|---|---|---|
| `Assets/Scripts/Combat/IProjectileEffectReceiver.cs` | `IProjectileEffectReceiver` | `ReceiveProjectileEffect(Projectile, Vector2)`，只有 Boss 实现；普通敌人不实现→走原 Health 路径 | 新增 |
| `Assets/Scripts/Boss/MirrorAngelBossEffectReceiver.cs` | `MirrorAngelBossEffectReceiver` (Cardwin.Boss，挂 root) | 实现 IProjectileEffectReceiver：读 `Projectile.SourceCard/EffectType/UsesCardEffect`，按效果应用——Damage→护盾吸收后扣总血(owner.TakeHit)、Block→加护盾、Heal→owner.Heal、Focus→定时 Buff(默认5s)。持有 `currentShield`+buff，公开 CurrentShield/HasBuff/BuffName/BuffRemaining + OnShieldChanged/OnBuffChanged；Update 计时清 Buff。`ApplyExternalDamage` 供 Body IDamageable 走护盾。日志 `[MirrorAngelBoss] Heal/Shield/Buff/Shield absorbed ...` | 新增 |

### 最小修改
- `Projectile.cs`（仅追加，不改伤害/普通敌人路径）：① 新增只读属性 `SourceCard/EffectType/UsesCardEffect/CardContext` + `ResolveDamage()`；② `HandleHit` 顶部新增分支：`IProjectileEffectReceiver`(self→parent) 存在→`ReceiveProjectileEffect(this,pos)`+Destroy（仅 Boss 命中；普通敌人无此接口→跳过→原 BossPart→IDamageable→Health 分支逐字不变）。
- `MirrorSaintessBoss.cs`：新增最小公开 `Heal(int)`（封顶 max、触发 OnHealthChanged、不动 Phase2/Death/伤害逻辑）。
- `MirrorAngelBodyDamageReceiver.cs`：IDamageable.TakeHit 改为优先经 `MirrorAngelBossEffectReceiver.ApplyExternalDamage`（护盾感知），无则回退 owner.TakeHit。
- `BossHUD.cs`：状态行改为 `Shield: X | Body: OK/DEAD | Status: <Buff>/None`，自动获取 root 上的 EffectReceiver 轮询显示。
- `MirrorAngelBoss.prefab`：root 加 `MirrorAngelBossEffectReceiver`(owner=根)。场景实例 Revert 同步。

### 效果链路
```
玩家子弹(携带 CardData+EffectType) → 命中 Body/Root → Projectile.HandleHit
→ IProjectileEffectReceiver(MirrorAngelBossEffectReceiver) → 按 EffectType:
   Damage → 护盾吸收→剩余 owner.TakeHit(扣总血/Phase2/Death)
   Block  → currentShield += card.block
   Heal   → owner.Heal(card.heal)（封顶 max）
   Focus  → Buff "Focus" 定时(5s)
→ BossHUD 显示 HP/Shield/Status → 子弹销毁
```

### 测试（Play + 真子弹/确定性步进，0 红错）
- Damage(Strike10)：HP 400→390。Heal(12)：390→400 封顶不超 max。Guard(Block15)：shield 0→15。Damage 带盾：HP 不变 shield 15→5；再 Damage：shield 5→0 + HP→395（**护盾先吸收再扣血**）。Focus：HasBuff=True buff=Focus 5s。
- Phase2/Death（经 EffectReceiver 伤害路径）：dmg200→HP195 Phase2(CastMirror_0)；致命→HP0 Death(Death_0)。
- BossHUD 实时：`HP 400/400 (Phase 1) | Shield: 30 | Body: OK | Status: Focus`。
- 普通敌人回归：MeleeEnemy 无 IProjectileEffectReceiver→跳过新分支→raw dmg10 经 Health.TakeDamage 30→20（原路径不变）。
- 玩家自效果：未改 CardEffectExecutor.ExecuteRight/ApplyEffectToTarget(player)→自身 Heal/Guard/Focus 不受影响。

---

## 25. MirrorAngel Boss — 第一个主动技能：三连镜光束 MirrorTripleBeam (Stage 44)

> Boss 第一个攻击技能。流程：停止普通移动 + 播放 CastMirror → 锁定玩家方向显示红色预警线 1 秒 → 朝玩家**当前位置**依次发射 3 束光束（每束重新瞄准），命中玩家扣血（复用 `Cardwin.Combat.Health.TakeDamage(int)`）→ 0.5s 后摇 → 回到移动/决策。**可在空中释放（无 IsGrounded 检查、不改重力）**；Boss 死亡立即中断。红线/光束均用 Unity 自带 `LineRenderer`。**未改玩家/玩家子弹/Projectile/卡牌/弹匣/背包/设置/Boss 受击/Boss HP/BossHUD/地面/传送门。** 仅给移动脚本新增最小 `SetMovementLocked/SetCasting`（不改寻路/巡逻逻辑）。

### 新增脚本 (Assets/Scripts/Boss/，命名空间 Cardwin.Boss)
| 文件 | 类 | 职责 | 关键函数 | 状态 |
|---|---|---|---|---|
| `MirrorAngelTripleBeamSkill.cs` | `MirrorAngelTripleBeamSkill` | 三连镜光束技能：CastRoutine 协程控制前摇(CastMirror+移动锁)/红线预警 1s/3 束光束(每束 `CircleCast(playerLayer)` 命中 `Health.TakeDamage(beamDamage)`，每束最多扣一次)/后摇 0.5s/通知 mover 暂停恢复/死亡中断。内置最小冷却触发(initialDelay1.5/cooldown4.5/minDist2.5/maxDist12/attackChance0.65/retryDelay)，无 Brain，可迁移。红线/光束优先用 FX prefab，prefab 为空则运行时建 LineRenderer。LineRenderer 端点=命中点或 origin+dir*beamRange。`IsCasting` 只读。防重入(_isCasting)/死亡(boss.IsDead)/无玩家 全部 guard。`OnDisable` 兜底 EndCast | `Awake/Start/Update/TryCast/CastRoutine/FireBeam/Aborted/EndCast/AimDirection/GetOrigin/ResolvePlayer/SpawnLine/UpdateLine/DestroyLine` | 新增 |

### 修改脚本
| 文件 | 变更 | 说明 |
|---|---|---|
| `MirrorAngelBossGravityMover.cs` | 新增 `SetMovementLocked(bool)` / `SetCasting(bool)` + `_movementLocked`/`_externalCasting` 字段；`IsCasting` 改为 `Time.time<_castEnd \|\| _externalCasting`；FixedUpdate 在 `!boss.CanMove` 之后新增 movement-lock 分支（冻结水平速度，**保留 Y 速度/重力，无 grounded 检查 → 支持空中施法**） | 最小改动，不动巡逻/Dash/Fly/寻路逻辑 |
| `MirrorAngel_CastMirror.anim` | `loopTime` True→**False**（单帧，视觉等价；遵从需求）；Idle/Walk/Dash/Fly/Death 不变 | 资产改动 |

### 新增资产
| 路径 | 说明 |
|---|---|
| `Assets/Materials/Boss/MirrorAngel/M_BossBeamWarning.mat` | Sprites/Default，红色，红线材质 |
| `Assets/Materials/Boss/MirrorAngel/M_BossBeam.mat` | Sprites/Default，紫白，光束材质 |
| `Assets/Prefabs/Boss/MirrorAngel/FX/BossBeamWarning.prefab` | LineRenderer 红线：宽 0.06、红、sortingOrder 120、useWorldSpace、2 点 |
| `Assets/Prefabs/Boss/MirrorAngel/FX/BossBeam.prefab` | LineRenderer 光束：宽 0.22、紫白、sortingOrder 120、useWorldSpace、2 点 |

### CastMirror 动作图（沿用 Stage 39）
- 已导入：`Assets/Art/Gothic/Boss/MirrorAngel/States/MirrorAngel_CastMirror_0.png`（源 `CastMirror_0   .png`，Sprite/Single/PPU100/FullRect/AlphaTrans/Bilinear/Uncompressed/noMips）。
- 动画 Clip：`Assets/Animations/Boss/MirrorAngel/MirrorAngel_CastMirror.anim`（绑定 Body 的 m_Sprite，本轮 loopTime=false）。
- Animator：`MirrorAngelBoss.controller`（Stage40 参数驱动）已含 `IsCasting` 参数 + CastMirror 状态。技能 `mover.SetCasting(true)` → `MirrorAngelBossAnimatorBridge` 写 Animator `IsCasting=true` → 播放 CastMirror（无需新建/重建 Animator）。

### Prefab `MirrorAngelBoss.prefab` 变更 (Stage 44)
- 新增子物体 `BeamOrigin`（空 Transform，localPosition (-0.8,0.8,0)，靠近镜子上方，不参与碰撞）。最终 Hierarchy = `MirrorAngelBoss / Body / BeamOrigin`。
- root 新增 `MirrorAngelTripleBeamSkill`，已 wire：boss/mover/beamOrigin、warningLinePrefab=BossBeamWarning、beamLinePrefab=BossBeam、warningMaterial/beamMaterial、playerLayer=Player(1<<9=512)。
- Body 受击/Hurtbox/IDamageable、Rigidbody2D、CapsuleCollider、Animator、GravityMover、AnimatorBridge、CollisionFilter、SortingGroup、EffectReceiver 全部保留不变。

### 默认参数
`beamRange=14 / beamDamage=10 / beamHitRadius=0.18 / firstWarningTime=1 / beamVisibleTime=0.15 / intervalBetweenBeams=0.25 / recoveryTime=0.5 / beamCount=3 / shortWarningTimeLaterBeams=0(可调) / warningWidth=0.06 / beamWidth=0.22 / sortingOrder=120`。

### 测试（BossRoom Play + 确定性步进，0 红色错误 / 0 warning）
- 静态：场景实例 `MirrorAngelBoss` 含 skill 组件 + BeamOrigin(localPos -0.8,0.8) + 全部引用已 wire（playerLayer=512）。
- 前摇/红线：CastRoutine 启动后 `skill.IsCasting=True`、`mover.IsCasting=True`（移动锁），场景出现 1 条红色 warning LineRenderer。
- 命中/伤害：origin→玩家 `CircleCast(0.18, playerLayer)` 三束命中 Player，每束 `Health.TakeDamage(10)`，HP 50→40→30→20（每束仅一次）。
- 空中释放：`mover.IsGrounded=False`（boss 浮空 y=-2.38）仍成功施法 → 无 grounded 限制成立。
- 死亡中断：致命后 `boss.IsDead=True`，`skill.TryCast()` 返回 False（不再起新技能）；进行中协程每帧 `Aborted()`→`EndCast` 自动解锁（代码确认）。
  - 回归：Body 受击/HP/Phase2/Death、BossHUD、Walk/Idle/Dash/Fly/Death 动画、MainGround 承托、6 区美术排序均不变；Console 0 红错。

### Stage 45 — 朝向修复 + BeamOrigin 镜像 + 第2/3束固定 ±15°
- **朝向 bug 真因**：美术默认面向**右**，但 `MirrorAngelBossGravityMover.UpdateFacing` 的 `artFacesRight=false` → `!artFacesRight` 把朝向取反 → 玩家在右时反而镜像成朝左（"正好弄反"）。修正：`artFacesRight` 默认改 `true`（prefab+实例序列化值同步为 true）。
- **镜像方式**：`SpriteRenderer.flipX` 未用；统一用 `visualRoot(Body).localScale.x = |baseScale.x| * facingSign`（只镜像 Body，**根/Rigidbody2D/Collider 不翻**，root scale 恒 (1,1,1)、rotZ=0）。`facingSign`：玩家在右=+1(自然/朝右)，在左=-1(镜像/朝左)。
- **BeamOrigin 镜像**：mover 新增 `[SerializeField] Transform beamOrigin`（wire 到 `BeamOrigin` 子物体）+ Awake 记录 `_beamOriginBaseLocalPos(-0.8,0.8,0)`；`ApplyFacing(sign)` 同时设 `beamOrigin.localPosition.x = |baseX| * facingSign` → BeamOrigin 永远在**朝向（玩家）一侧**：玩家右 worldX≈8.82(右)、玩家左 worldX≈7.22(左)。仍只 1 个 BeamOrigin，无 FirePoint，不参与碰撞。
- **mover 新增公开 API**：`ComputeFacingSignToward(Vector3)`、`ApplyFacing(float)`、`CurrentFacingSign`；`UpdateFacing` 改为调用二者。移动锁定（施法中）时 mover 不跑 UpdateFacing，故施法期朝向冻结。
- **三连光束角度（skill）**：第 1 束保持原逻辑（红线锁定玩家方向 1s 后沿 baseDir 发射）。施法开始锁定 `castFacingSign`（`mover.ApplyFacing` 设 Body+BeamOrigin），整段不再翻转。第 2/3 束**不再重新瞄准玩家**：`baseDir` 旋转固定 `beamSpreadAngle=15°`——`spreadSign = player.y>=origin.y ? +1 : -1`；`dir2=Rotate(baseDir, spreadSign*15)`、`dir3=Rotate(baseDir, -spreadSign*15)`（玩家在上→第2束上侧/第3束下侧；在下→反序）。新增 `Rotate(v,deg)`（+ 为 CCW）。命中/伤害/范围/可见时长/受伤接口全不变（FireBeam 未改，beamDamage=10）。
- **测试（Play 同步验证，0 红错）**：朝向 player右→Body.x=+0.42/BeamOrigin右、player左→Body.x=-0.42/BeamOrigin左，root 恒 (1,1,1)/rotZ0；baseDir 对玩家 0°、dir2/dir3=±15°、dir2/dir3 距玩家 15°(未re-aim)、玩家上下翻转 spread 顺序；致命后 TryCast=False；boss HP 400→击杀正常。

### Stage 46 — 朝向污染修复：统一 MirrorAngelFacingController + 朝移动方向 + 锁定/解锁
- **反着走真因**：朝向 = 玩家位置（mover `ComputeFacingSignToward(_player)`），移动 = `ComputeWalkDir`/`_patrolDir`/dash → 二者在巡逻/边界/施法后恢复巡逻或接近时**方向不一致** → "向左走脸朝右"。且当时有**两个朝向写入者**（mover.ApplyFacing + skill 调 mover.ApplyFacing）。动画 Clip 检查：6 个 clip 仅 `Body SpriteRenderer.m_Sprite`，**无 flipX/localScale.x/localPosition.x 曲线**（不是动画污染）。
- **新增唯一朝向源** `Assets/Scripts/Boss/MirrorAngelFacingController.cs`（Cardwin.Boss）：唯一控制 Body 视觉(默认 `SpriteRenderer.flipX`，artDefaultFacesRight=true) + BeamOrigin 镜像(`localPosition.x=|baseX|*sign`)。**绝不翻根/Rigidbody/Collider**（root scale 恒 (1,1,1)、Body.localScale.x 恒 +0.42）。API：`FaceMoveDirection(moveX)`/`FaceTarget(target)`/`GetFacingToTarget`/`SetFacing(sign)`/`LockFacing(sign)`/`UnlockFacing()`/`IsFacingLocked`/`CurrentFacingSign`。绝对赋值无累积；invertVisualFacing/invertBeamOriginSide 兜底参数。
- **mover 改造**：移除自身 `ApplyFacing/ComputeFacingSignToward/CurrentFacingSign/artFacesRight/visualRoot/beamOrigin/_visualBaseScale/_beamOriginBaseLocalPos`；改持 `facing` 引用；`UpdateFacing` 改为：未锁时 `|vx|>0.05 → FaceMoveDirection(vx)`（走/冲/飞=朝移动方向，永不反），否则 `FaceTarget(player)`（站立朝玩家）。
- **skill 改造**：cast 开始 `facing.LockFacing(facing.GetFacingToTarget(player))`（替代旧 mover.ApplyFacing）；`EndCast()` 首行 `facing.UnlockFacing()`。EndCast 由正常结束 / Aborted(死亡) / OnDisable 调用 → 死亡/中断/禁用都解锁。skill 只读 `beamOrigin.position`，不再镜像 BeamOrigin。
- **第1束红线1s / 第2·3束±15° / 伤害 / 受伤接口 未改**（仅改 cast 开始的朝向锁定写法）。
- **Prefab**：root 加 `MirrorAngelFacingController`（wire visualRoot=Body/bodySpriteRenderer=Body.SR/beamOrigin=BeamOrigin/useSpriteFlipX=true/artDefaultFacesRight=true）；mover.facing + skill.facing → controller。场景实例自动继承。Hierarchy 仍 `MirrorAngelBoss/Body/BeamOrigin`。
- **测试（Play 同步，0 红错）**：MoveDir+1→flipX=false/BeamOrigin右、MoveDir-1→flipX=true/BeamOrigin左；**巡逻不匹配**（玩家右但移动左）→朝向跟随移动(左)，根除反向；锁定中 FaceMoveDirection 被忽略、解锁后恢复；连续 5 次 Lock/Unlock 无累积漂移；skill.enabled=false→OnDisable 解锁；死亡 TryCast=False 且 IsFacingLocked=False；root 恒 (1,1,1)/rotZ0、Body.localScale.x 恒正。

#### Stage 46.1 — 视觉朝向反向修正
- 实测一开始走路视觉就反（美术自然朝向实为**左**，Stage46 的 artDefaultFacesRight=true 假设反了）。**仅**把 prefab 上 `MirrorAngelFacingController.invertVisualFacing` 设 `false→true`（只影响 Body flipX，不影响 BeamOrigin/root）。其余字段/脚本未动。实测：MoveDir+1→flipX=true、MoveDir-1→flipX=false（与上一版相反）；BeamOrigin 仍按 sign 正确镜像(R/L)；root (1,1,1)/rotZ0、Body.localScale.x +0.42 不变；0 红错。改动文件仅 `MirrorAngelBoss.prefab`（场景实例继承）。

#### Stage 46.2 — CastMirror 攻击图镜像
- Stage46.1 后走路正确但**攻击(CastMirror)又反**：根因=`MirrorAngel_CastMirror_0.png` 的美术自然朝向与 Walk/Idle 相反（同一 flipX 映射下走路对则施法反）。修复：**仅**把该 PNG 像素水平镜像（1254×1254，导入设置/Sprite Single 不变），使其自然朝向与其它帧一致 → 沿用同一 flipX 即正确。未改任何脚本/prefab/其它 sprite。改动文件仅 `Assets/Art/Gothic/Boss/MirrorAngel/States/MirrorAngel_CastMirror_0.png`。

#### Stage 46.3 — Boss AI 设计优化 V1：距离判断 + 决策间隔 + 行为状态机 + 技能候选池 + 攻击概率 + 前摇/释放/后摇 + 重新站位

- **新增 `MirrorAngelBossBrain.cs`**（`Cardwin.Boss`，`RequireComponent(MirrorSaintessBoss)`）：Boss AI 大脑，替代旧的"技能 CD 好了立刻释放"逻辑。
- **AI 状态枚举** `MirrorAngelBossBrainState`：`Idle`(短暂停顿) / `Approach`(玩家太远靠近) / `KeepDistance`(保持理想距离) / `Reposition`(玩家太近后撤) / `Windup`(技能前摇) / `Casting`(技能释放) / `Recovery`(技能后摇) / `Dead`(死亡停止所有 AI)。
- **距离区间参数**：`tooCloseDistance=2.5`(后撤) / `preferredMinDistance=4` / `preferredMaxDistance=7`(理想攻击距离) / `farDistance=10`(优先接近)。Boss 不再永远贴玩家脸。
- **决策间隔**：`decisionIntervalMin=0.5` / `decisionIntervalMax=1.2`，每 0.5~1.2 秒随机间隔才做一次决策；移动执行仍每帧更新。
- **技能候选池** `MirrorAngelBossSkillOption`（`System.Serializable`）：`skillId`/`cooldown`/`lastUseTime`/`minRange`/`maxRange`/`baseWeight`/`repeatPenalty`。当前含 `MirrorTripleBeam`（cooldown=4.5, minRange=4, maxRange=12, baseWeight=10, repeatPenalty=3）和 **`MirrorAngelGroundRay`（Stage 47 新增：cooldown=8, minRange=0, maxRange=100, baseWeight=8, repeatPenalty=4）**。
- **技能评分**：`ScoreSkill` = baseWeight + rangeScore(距离越接近理想中位分数越高) × 5 - repeatPenalty(连续同技能惩罚) + Random(-1,1)。选最高分技能。
- **攻击概率** `attackChance=0.65`：技能可用也不一定释放，Boss 有时会保持距离/停顿/重新站位。
- **移动行为**：Approach→向玩家移动(不贴脸到 preferredMaxDistance 停止)；KeepDistance→停止水平移动；Reposition→后撤 0.4~0.8s(repositionDurationMin/Max, repositionSpeedMultiplier=1.0)；Casting→锁定移动(SetMovementLocked)；Recovery→恢复后解锁。
- **接入 MirrorTripleBeam**：`MirrorAngelTripleBeamSkill.autoCast` 默认改为 `false`，停用 Update 自触发；Brain 通过 `beamSkill.TryCast()` 触发，协程 `CastSkillRoutine` 管理 Windup→Casting→Recovery→Idle 全流程。
- **朝向兼容**：复用现有 `MirrorAngelFacingController`；Approach/Reposition 按移动方向朝向；KeepDistance/Idle 面向玩家；Cast 期间 locked 由 skill 管理；Dead 不再更新。
- **Animator 兼容**：复用现有 `MirrorAngelBossAnimatorBridge`；Brain 通过 mover.SetCasting/SetMovementLocked 间接控制 Animator 参数（Approach/Reposition→Walk，KeepDistance/Idle→Idle，Windup/Casting→CastMirror，Recovery→Idle，Dead→Death）。
- **死亡优先级**：Update 首行检查 `boss.IsDead` → `currentState=Dead` + `StopAllBossActions`（停协程/解锁移动/解锁朝向/Reset 状态），不再决策/移动/释放技能。
- **修改**：`MirrorAngelTripleBeamSkill.cs`（autoCast=false）、`MirrorAngelBossGravityMover.cs`（增加 brain 引用+脑控移动分支）、`MirrorAngelBoss.prefab`（新增 Brain 组件+autoCast=false）。
- **未改**：玩家/卡牌/弹匣/背包/Boss HP 受击/BossHUD/地面/传送门/普通敌人/PlayerController/Projectile。只改 Boss AI 决策逻辑。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| MirrorAngelBossBrain.cs | `MirrorAngelBossBrain` (Cardwin.Boss) | **[Stage 46.3]** Boss AI 大脑：距离判断+决策间隔+行为状态机(Idle/Approach/KeepDistance/Reposition/Windup/Casting/Recovery/Dead)+技能候选池+评分选技+攻击概率+前摇/释放/后摇+重新站位 | `Awake/Start/Update/DecideNextAction/TryUseSkill/StartSkill/CastSkillRoutine/StartReposition/StopAllBossActions/IsSkillUsable/ScoreSkill/ChooseBestSkill/DistanceToPlayer/FindPlayer` | 每帧 Update（死亡优先/Reposition timer/Approach 持续/决策间隔/DecideNextAction）；CastSkillRoutine 管理 Windup→Casting→Recovery→Idle 全流程 | (自身) | 新增 V1 |
| MirrorAngelBossBrain.cs | `MirrorAngelBossSkillOption` (Cardwin.Boss) | 技能候选池数据结构：skillId/cooldown/lastUseTime/minRange/maxRange/baseWeight/repeatPenalty | — | — | MirrorAngelBossBrain | 新增 |
| MirrorAngelBossBrain.cs | `MirrorAngelBossBrainState` (enum) | Boss AI 行为状态：Idle/Approach/KeepDistance/Reposition/Windup/Casting/Recovery/Dead | — | — | MirrorAngelBossBrain | 新增 |

#### Stage 47 — Boss 第二技能：大范围蓄力地面光柱 MirrorAngelGroundRay

- **素材**：从 `C:\Users\86189\Desktop\base\attack1` 导入 3 张 PNG（qianyao/shiangzhong/houyao）→ 按语义对应 Windup/Active/Recovery 三阶段。
- **动画**：新建 `MirrorAngel_Attack1_GroundRay.anim`，3 帧非循环，仅切 Body Sprite（无 flipX/localScale.x/BeamOrigin.localPosition 曲线）。Windup=0.9s/Active=0.8s/Recovery=0.5s。
- **Animator**：`MirrorAngelBoss.controller` 新增 `Attack1_GroundRay` 状态，绑定 GroundRay clip；旧状态/参数/过渡不改。
- **脚本**：新增 `MirrorAngelGroundRaySkill.cs`（Cardwin.Boss）——Windup/Active/Recovery 三阶段协程；Active 在 Boss 朝向一侧 `OverlapBox`（X=100/H=8）命中 Player → `Health.TakeDamage(damage=18)`，每次技能最多扣一次；无地面预警无红圈红线；FacingController LockFacing/UnlockFacing；支持空中释放不检查 IsGrounded；Gizmos 显示攻击范围。TryCast/IsCasting 供 Brain 调用。
- **Brain 接入**：`MirrorAngelBossBrain` 新增 `groundRaySkill` 引用；Start 默认技能池增加 MirrorAngelGroundRay；`CastSkillRoutine` 按 `skillId` 分发到 beamSkill/groundRaySkill。
- **朝向**：GroundRay 从 `facing.CurrentFacingSign` 决定攻击侧（右=+1 右侧 X100、左=-1 左侧 X100），不按玩家位置，不翻 Body/root。
- **FX**：无 prefab 时自动创建运行时 SpriteRenderer（半透明紫白），覆盖攻击矩形；有 prefab 时实例化并 SetActive(true)。
- **未改**：玩家/卡牌/弹匣/背包/Boss HP 受击/BossHUD/地面/传送门/MirrorTripleBeam 光束逻辑/第1束红线1s/±15°逻辑/FacingController 朝向修复/Walk/Dash/Fly/Death 动画。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| MirrorAngelGroundRaySkill.cs | `MirrorAngelGroundRaySkill` (Cardwin.Boss) | **[Stage 47]** 大范围蓄力地面光柱：Windup/Active/Recovery 三阶段+朝向一侧 X=100 范围 OverlapBox 命中 Player 扣血+无地面预警+空中支持+FacingController Lock/Unlock | `Awake/TryCast/CastRoutine/DealDamageOnce/SpawnActiveFx/PositionFx/CreateRuntimeFx/DespawnFx/Aborted/EndCast/ResolvePlayer/OnDisable/OnDrawGizmosSelected` | Brain 通过 TryCast 触发 → 三阶段协程 | MirrorAngelBossBrain | 新增 V1 |
| MirrorAngelBossBrain.cs | `MirrorAngelBossBrain` | **[Stage 47 更新]** 技能池增加 MirrorAngelGroundRay；CastSkillRoutine 按 skillId 分发 | `Start/CastSkillRoutine`（更新） | 新增 groundRaySkill 引用+默认技能+分发 | (自身) | 更新 |
| MirrorAngelBossAnimatorBridge.cs | `MirrorAngelBossAnimatorBridge` | **[Stage 47 更新]** 新增公开 `Animator` 属性供 GroundRaySkill Play 动画 | `Animator` (property) | GroundRaySkill.Play("Attack1_GroundRay") | MirrorAngelGroundRaySkill | 更新 |
| MirrorAngelBoss.controller | `MirrorAngelBossAnimator` | **[Stage 47/47.1/47.2/48 更新]** AttackType 参数分流：0=无,1=CastMirror(TripleBeam),2=Attack1_GroundRay(GroundRay),**3=Attack2_DoubleSlash,4=Attack2_DoubleSlashDash(Stage48)** | — | — | Animator / Brain | 更新 |

#### Stage 48 — Boss 近战技能：二连横劈 + 二连横劈突刺（基于 attack2 素材）
- **素材**：从 `C:\Users\86189\Desktop\base\attack2` 导入 6 张 PNG（2.1qianyao/gongji, 2.2qinyao/gongji, 2.3chongciqianyao/chongci）。
- **动画**：`MirrorAngel_Attack2_DoubleSlash.anim`（0.25s windup1 / 0.10s slash1 / 0.15s windup2 / 0.10s slash2 / 0.30s recovery）和 `MirrorAngel_Attack2_DoubleSlashDash.anim`（同上 + 0.16s dash windup + 0.28s dash + 0.35s recovery）。均只切 Body.m_Sprite。
- **Animator**：新增 `Attack2_DoubleSlash`(AttackType=3) 和 `Attack2_DoubleSlashDash`(AttackType=4) 状态 + 对应 AnyState 过渡。
- **脚本**：
  - `MirrorAngelDoubleSlashSkill.cs` — 两段 OverlapBox 近战命中（slashRangeX=2.5/Y=1.8/offsetX=1.4/Y=1.0），每段 12 伤害，朝向前方自动镜像。无位移。
  - `MirrorAngelDoubleSlashDashSkill.cs` — 两段横劈同上 + 0.16s 冲刺前摇 + dashDistance=3.5 位移(speedMultiplier=3x walkSpeed) + 冲刺长条命中盒(dashHitboxWidth=3.5/H=2.0, dashDamage=20)。突刺方向自动镜像；移动锁在冲刺期间临时解除。
  - `MirrorAngelBossGravityMover` 新增 `Rigidbody`/`IsMovementLocked` 公开属性供 Dash 技能使用。
- **Brain 接入**：技能池新增 DoubleSlash(cooldown=3/range=0.8~2.8/weight=9/penalty=2) 和 DoubleSlashDash(cooldown=5/range=2.0~5.0/weight=7/penalty=3)。CastSkillRoutine 按 skillId 分发到对应 TryCast。
- **朝向**：复用 FacingController LockFacing/UnlockFacing + flipX 机制；所有近战判定按 Boss 朝向自动镜像。
- **调试**：ContextMenu Debug/Play；Gizmos 显示斜劈命中框（橙色）、突刺命中框（红色）、突刺位移终点（红色球）。
- **未改**：玩家/卡牌/弹匣/HP/BossHUD/地面/传送门/三连光束/地面光柱。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 状态 |
|--------|------|----------|--------|----------|------|
| MirrorAngelDoubleSlashSkill.cs | `MirrorAngelDoubleSlashSkill` (Cardwin.Boss) | **[Stage 48]** 二连横劈：两段 OverlapBox 近战+前摇+后摇+朝向镜像+AttackType=3 | `Awake/TryCast/CastRoutine/TryDealDamage/Aborted/EndCast/ResolvePlayer/OnDrawGizmosSelected/DebugPlay` | Brain dispatch | 新增 V1 |
| MirrorAngelDoubleSlashDashSkill.cs | `MirrorAngelDoubleSlashDashSkill` (Cardwin.Boss) | **[Stage 48]** 二连横劈+突刺：两段横劈+冲刺位移+冲刺命中盒+朝向镜像+MovementLock 临时解除+AttackType=4 | `Awake/TryCast/CastRoutine/DealSlashHit/DealDashHit/ApplyDamage/Aborted/EndCast/ResolvePlayer/OnDrawGizmosSelected/DebugPlay` | Brain dispatch | 新增 V1 |
| MirrorAngelBossBrain.cs | `MirrorAngelBossBrain` | **[Stage 48 更新]** 技能池增加 DoubleSlash + DoubleSlashDash；CastSkillRoutine 分发+IsSkillRunning 检查 | `Start/CastSkillRoutine`(更新) | 新增 2 技能引用+默认池+分发 | 更新 |
| MirrorAngelBossGravityMover.cs | `MirrorAngelBossGravityMover` | **[Stage 48 更新]** 新增 `Rigidbody`/`IsMovementLocked` 公开属性供 DashSkill 使用 | `Rigidbody`(property) `IsMovementLocked`(property) | DoubleSlashDashSkill dash phase | 更新 |

#### Stage 49 — 动作仲裁系统：ActionController 统一技能锁 + token 防抢占
- **问题**：Boss 多个技能（TripleBeam/GroundRay/DoubleSlash/DoubleSlashDash）之间互相抢占动作；AnimatorBridge 每帧写 Idle/Walk 覆盖攻击动画；Brain 在攻击期间仍可决策→启动新技能。
- **新增** `MirrorAngelBossActionController.cs`（Cardwin.Boss，RequireComponent(MirrorSaintessBoss)）：
  - `MirrorAngelActionType` 枚举：None(0), TripleBeam(1), GroundRay(2), DoubleSlash(3), DoubleSlashDash(4)。
  - `BeginAction(type)` → 检查 IsActionLocked/IsDead → 递增 token → 设 IsCasting/AttackType/movementLock/facingLock → 返回 token。锁已占→返回 -1。
  - `EndAction(token)` → token 不匹配则 no-op（防旧协程 finally 清掉新动作）→ 解锁全部状态（IsCasting=false/AttackType=0/movementUnlock/facingUnlock/清 externalVelocity）。
  - `ForceCancelAction()` → 递增 token + 无条件解锁（死亡/场景卸载用）。
  - `AllowSkillMotion(bool)` → Dash 技能临时允许位移。
- **Brain 集成**：`Update()` 新增 `actionController.IsActionLocked` 检查（锁定时跳过 DecideNextAction）。`CastSkillRoutine` 改为 `BeginAction → dispatch → wait → EndAction`（token 机制）。`StopAllBossActions` 调用 `ForceCancelAction`。
- **AnimatorBridge 集成**：`Update()` 新增 `actionController.IsActionLocked` 检查（锁定时返回，不写 Idle/Walk 参数）。Death 仍优先写入。
- **技能脚本**：TripleBeam/GroundRay/DoubleSlash/DoubleSlashDash 保持各自的 try/finally + EndCast，但不再直接设置 IsCasting/AttackType/movementLock/facingLock（交由 ActionController）。
- **Prefab**：新增 `MirrorAngelBossActionController` 组件。

| 文件名 | 类名 | 主要职责 | 函数名 | 状态 |
|--------|------|----------|--------|------|
| MirrorAngelBossActionController.cs | `MirrorAngelBossActionController` | **[Stage 49]** 统一动作锁+token：BeginAction/EndAction/ForceCancelAction/AllowSkillMotion；独占 IsCasting+AttackType+movementLock+facingLock | `Awake/BeginAction/EndAction/ForceCancelAction/AllowSkillMotion` | 新增 V1 |
| MirrorAngelBossBrain.cs | `MirrorAngelBossBrain` | **[Stage 49 更新]** 集成 ActionController：Update 检查 IsActionLocked；CastSkillRoutine 用 BeginAction/EndAction+token | `Update/CastSkillRoutine/StopAllBossActions` | 更新 |
| MirrorAngelBossAnimatorBridge.cs | `MirrorAngelBossAnimatorBridge` | **[Stage 49 更新]** 集成 ActionController：IsActionLocked 时跳过 Idle/Walk 写入 | `Update` | 更新 |

#### Stage 50 — 远距离冲刺接近 + 飞天悬停激光
- **FarDashApproach (AttackType=5)**：距离 > 9m 时 35% 概率触发。停止距离 4.5m，最大冲刺 6m，时长 0.35s，CD 5s。`DoFarDashCoroutine` 使用 `rb.MovePosition`+SmoothStep。Dash 动画(Dash state)。
- **AirLaserMode (AttackType=6)**：距离 4~12m 时 25% 概率触发。上升 3.5m(0.45s)→悬停 3.0s→发射 3 次激光(interval 0.65s, 伤害 10, Range 16)→下降 0.4s。CD 9s。`MirrorAngelAirLaserSkill` 管理上升/悬停/激光/降落。Fly 动画(Fly state)。激光复用 LineRenderer 逻辑。
- **Brain 集成**：技能池新增两个选项。CastSkillRoutine 分发：FarDash→StartCoroutine(DoFarDash)内联；AirLaser→airLaserSkill.TryCast。
- **ActionCounter**：AttackType 5/6 走 BeginAction/EndAction，锁定期不可插入其他技能。
- **未改**：所有旧技能、朝向、HP、玩家、卡牌。

| 文件名 | 类名 | 主要职责 | 状态 |
|--------|------|----------|------|
| MirrorAngelAirLaserSkill.cs | `MirrorAngelAirLaserSkill` | **[Stage 50]** 飞天悬停激光：上升/悬停/发射3次激光/降落，rb.MovePosition 控制高度，LineRenderer 复用 | 新增 V1 |
| MirrorAngelBossBrain.cs | `MirrorAngelBossBrain` | **[Stage 50 更新]** +FarDashApproach 参数/DoFarDash+AirLaserSkill 引用+池+分发 | 更新 |
| MirrorAngelBossActionController.cs | `MirrorAngelBossActionController` | **[Stage 50 更新]** ActionType +FarDashApproach(5) +AirLaserMode(6) | 更新 |
| MirrorAngelBoss.controller | `MirrorAngelBossAnimator` | **[Stage 50 更新]** +AnyState→Dash(AttackType=5) +AnyState→Fly(AttackType=6) | 更新 |

---

## 26. Confession Night Rhythm Module — 第三个特殊模组（Stage 54）

> 第三个特殊模组：玩家在 `Demo_Combat` 与道具交互（F）后，普通战斗 UI 碎裂消失、播放《告白の夜》、进入**整首歌循环音游**模式。左键判定红色音符（命中→追踪弹打最近普通敌人 3% 最大生命），右键判定蓝色音符（命中→回血 5% 最大生命），Miss/点错→损失 10% 最大生命。音乐 `loop=true`，播完自动从头；谱面按 `audioSource.time` 驱动，**音乐循环时谱面同步从头（OnMusicLooped：nextNoteIndex=0 + 清空残留音符 + 复用同一份 chart）**；普通 UI 与普通射击在循环期间**不自动恢复**（仅玩家死亡/场景切换时结束）。命名空间 `Cardwin.Modules`，全部**纯新增**，未改 Boss/普通敌人/玩家移动跳跃碰撞/CursedEight/BlessedEight/Retry/Settings；仅在 `PlayerController2D` 两个鼠标射击入口追加 `&& !RhythmGameController.IsRhythmModeActive` 守卫。

**音频**：`Assets/Audio/`（文件夹已建，供放入真实 `Ayasa_Confession_Night.mp3`）。本环境无源 mp3（`/mnt/data` 不存在、桌面无匹配），controller 在未指定 clip 时**自动生成 290s(≈4:50) 程序化占位 clip**（11025Hz 单声道 93BPM 点击轨）驱动整首歌时间线与循环，可立即测试；放入真 mp3 并在 `PlayerConfessionNightModuleState.confessionNightClip` 赋值即用真歌（chart 按 `clip.length` 生成）。曲参数：BPM=93、beat≈0.645s、bar≈2.58s、noteTravelTime=2.0s。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| ConfessionNightModulePickup.cs | `ConfessionNightModulePickup` (Cardwin.Modules) | 场景道具：玩家进触发范围显示提示，F 激活。AddComponent `PlayerConfessionNightModuleState` 若缺失→Activate→消耗自毁 | `OnTriggerEnter2D/OnTriggerExit2D/Update/ActivateModule` | 玩家进出范围/F 键激活 | 场景（Demo_Combat (28,-1,0)） | 新增 V1 |
| PlayerConfessionNightModuleState.cs | `PlayerConfessionNightModuleState` (Cardwin.Modules) | 玩家侧状态：Activate→停用 Cursed/Blessed→破坏普通 UI（CombatUIBreakController）→创建 RhythmGameController 并 BeginRhythmMode。持续直到死亡/场景切换。可选 `confessionNightClip` 字段 | `Awake/ResolveHealth/Activate/DeactivateOtherModules/Deactivate` | 由 Pickup 调用 | Pickup | 新增 V1 |
| RhythmGameController.cs | `RhythmGameController` (Cardwin.Modules) | 音游核心：运行时自建 RhythmGameCanvas(Overlay/ConstantPixelSize sortingOrder30)+HitCircle(灰白环, (0.78w,0.18h))+NoteTrack；AudioSource(loop/vol0.8)；`GenerateFullSongChart(clip.length)`(BPM93/4/4/seed9301/intro6s/end2s/密度按歌曲百分比/红蓝比例随段落/高潮加半拍)；`SpawnNotesByAudioTime`(按 audioSource.time 提前 travel 生成)；判定(左=红/右=蓝，hitWindow110/perfect55/miss150 px)；红命中→追踪弹/蓝命中→回血5%/Miss/Wrong→扣血10%；`OnMusicLooped`(loopCount++/nextNoteIndex=0/清空音符/复用 chart)。静态 `IsRhythmModeActive`/`Instance`。Debug：ForceMusicLoop/SeekAudio/ForceRed/Blue/Miss/Wrong/NearestEnemy | `BeginRhythmMode/Update/GenerateFullSongChart/ShouldSpawnNote/ChooseNoteType/SpawnNotesByAudioTime/UpdateNotePositions/UpdateInput/Judge/OnHit/OnWrong/UpdateMissCheck/HealPlayer/ApplyPenalty/SpawnHomingBullet/FindNearestNormalEnemy/IsBoss/OnMusicLooped/ClearAllActiveNotes/EndRhythmMode/BuildCanvas/SetupAudio/GetPlaceholderClip` | PlayerConfessionNightModuleState / PlayerController2D(读 IsRhythmModeActive) | 新增 V1 |
| RhythmNote.cs | `RhythmNoteType` (enum) | 音符颜色/所需输入：Red(左键)/Blue(右键) | — | — | RhythmGameController | 新增 V1 |
| RhythmNote.cs | `RhythmNoteData` (class) | 谱面条目：hitTime+type | — | — | RhythmGameController.chart | 新增 V1 |
| RhythmNote.cs | `RhythmNote` (MonoBehaviour) | 音符视图：持 RectTransform/Image/type/hitTime/judged；位置/颜色/透明度由 controller 驱动 | `Setup/SetAnchoredPosition/CurrentX/SetAlpha` | RhythmGameController | 新增 V1 |
| RhythmHomingBullet.cs | `RhythmHomingBullet` (Cardwin.Modules) | 红命中追踪弹：锁定最近**普通敌人**(Health，排除 Player/Boss)，homingSpeed12/lifeTime4，接触造成 `target.maxHealth*3%`(Health.TakeDamage，可致死)，运行时生成红色圆点 sprite | `Init/Update/DealDamage/EnsureVisual/GetSharedSprite` | RhythmGameController.SpawnHomingBullet | 新增 V1 |
| CombatUIBreakController.cs | `CombatUIBreakController` (Cardwin.Modules) | UI 碎裂：定位主 HUD Canvas(CombatHUD 父级)，对非菜单 HUD 子物体生成飞散碎片(白方块，重力+旋转+淡出)+CanvasGroup 淡出后 SetActive(false)；跳过 Pause/GameOver/Setting/Bag/Rhythm；`RestoreNormalCombatUI` 仅死亡/退出时调用（循环期不恢复） | `BreakNormalCombatUI/ShatterElement/SpawnFragment/Update/FadeAndHide/RestoreNormalCombatUI/ResolveHudCanvas` | PlayerConfessionNightModuleState | 新增 V1 |

### Demo_Combat 场景新增（Stage 54）
- root `ConfessionNightModulePickup` pos=(28,-1,0) scale=(1.4,1.4,1)：`SpriteRenderer`(紫白发光圆盘 `Assets/Art/Modules/ConfessionNightModule.png`, order50)+`CircleCollider2D`(isTrigger, r0.9)+`ConfessionNightModulePickup`(promptText/visualRenderer 已 wire)。
  - 子 `Prompt`(TextMesh "Press F: Confession Night", localPos(0,1,0), 默认 inactive)。
  - 位置远离前两个模组道具（Cursed≈16 / Blessed≈-12），触发范围无重叠。
- 运行时（非场景持久化）：`ConfessionNight_UIBreak`(CombatUIBreakController) / `ConfessionNight_RhythmController`(RhythmGameController + RhythmGameCanvas/HitCircle/NoteTrack + AudioSource)。

### 场景对象恢复 (Stage 54.1 — 三个模组道具恢复)
- **真实原因**：Cursed/Blessed pickup **从未真正保存进 `Demo_Combat.unity`**（Stage 54 调查时按组件/名称搜索即为 0；本轮 .unity 文件内仅含 `ConfessionNightModulePickup` 的 m_Name）。即上一阶段（或更早）创建后未保存/被覆盖丢失；并非本次删除，也非 Confession 把它们顶掉（Confession pickup 一直正常持久化，证明保存通道有效）。**不是 Play Mode 创建未保存（本轮全程 Edit Mode），不是对象 inactive/无 sprite/坐标错误（对象根本不在场景文件里）。**
- **附带修复**：发现 `manage_gameobject create` 的 `component_properties` 对 `SpriteRenderer.sprite`(对象引用)、`Collider2D.isTrigger`、`sortingOrder`、`CircleCollider2D.radius` **未生效**（连 Stage 54 的 Confession pickup 也是 sprite=NULL / isTrigger=False，之前仅靠代码直调 Activate 测试故未暴露）。改用独立 `manage_components set_property` 逐项设置后全部生效。
- **三个独立脚本**（不共用易错的通用 enum pickup）：Cursed=`CursedEightModulePickup`(moduleType=Cursed 显式) / Blessed=`BlessedEightModulePickup`(独立) / Confession=`ConfessionNightModulePickup`(独立)。
- **最终三道具**（Demo_Combat，Edit Mode 创建并保存）：
  - `CursedEightModulePickup` (16,-1,0) scale1：SpriteRenderer(CursedModule.png 红紫发光圆盘, order50)+CircleCollider2D(isTrigger, r0.9)+脚本(moduleType=Cursed)+Prompt 子物体(“Press F: Cursed Module”, inactive)；wire promptText/visualRenderer。
  - `BlessedEightModulePickup` (-12,-1,0) scale1：SpriteRenderer(BlessedModule.png 金白发光圆盘, order50)+CircleCollider2D(isTrigger, r0.9)+脚本+Prompt(“Press F: Blessed Module”, inactive)；wire。
  - `ConfessionNightModulePickup` (28,-1,0) scale1.4：SpriteRenderer(ConfessionNightModule.png 紫白圆盘, order50)+CircleCollider2D(isTrigger, r0.9)+脚本+Prompt(“Press F: Confession Night”, inactive)；wire（本轮补修其 isTrigger/sprite）。
  - 三个位置互不重叠（-12 / 16 / 28）。新增 sprite：`Assets/Art/Modules/CursedModule.png`、`BlessedModule.png`。
- **备份**：`Assets/Scenes/Demo_Combat_BACKUP_BEFORE_MODULE_RESTORE.unity`（AssetDatabase.CopyAsset，修改前）。
- **保存+重开验证**：MarkSceneDirty+SaveScene+SaveAssets；OpenScene 重载后三道具仍在（active/sprite/order50/isTrigger/r0.9/0 missing/Prompt 齐全），.unity 文件含三个 m_Name。
- **Play 实测（每个模组单独重启 Play，teleport 玩家触发 + 反射调用真实激活方法，0 红错）**：三道具开局可见(SR enabled/sprite!=null)；Cursed→playerInRange+prompt→Good=0/Evil=8/8 攻击卡；Blessed→prompt→Good=8/Evil=0/8 卡/move×0.5/fire×0.5；Confession→prompt→IsRhythmModeActive=True/chart474/RhythmGameCanvas/UI 碎裂(CardwinHUDRoot 隐藏)，音频 clip 已配置(占位 290s/loop/vol0.8，step 驱动下 isPlaying=false 为编辑器音频 DSP 限制)。退出 Play 后 IsRhythmModeActive=False（无残留射击锁）。
- **未改任何脚本逻辑**（仅场景对象 + 2 个新 sprite）；Boss/玩家移动战斗/CursedState/BlessedState/ConfessionState/Retry/Settings 全未改。

### 真实音乐导入 (Stage 54.2 — 播放真实《告白の夜》，禁用占位点击轨)
- **原因**：之前只有“波波音”是因为正式启动时静默回退到程序化占位点击轨 `Ayasa_Confession_Night_Placeholder`（真实 mp3 从未导入项目；且 `PlayerConfessionNightModuleState` 在交互时动态 AddComponent，Inspector 的 `confessionNightClip` 永远为空，旧逻辑 clip==null 即生成 placeholder）。
- **修复**：从 `C:\CloudMusic\Ayasa - 告白の夜.mp3`（实测存在，13.2MB）复制到 `Assets/Resources/Audio/Ayasa_Confession_Night.mp3` 并导入（AudioClip：length=290.5s/44100Hz/2ch）。
- **RhythmGameController.cs** 改动（仅音频解析，不动判定/音符/UI/谱面算法）：
  - 新增 `resourceClipPath="Audio/Ayasa_Confession_Night"` + `allowPlaceholderWhenMissing=false`(默认)。
  - 新增 `ResolveRealClip()`：序列化 clip 为空时 `Resources.Load<AudioClip>(resourceClipPath)` 自动加载真实歌（解决动态 AddComponent 不绑定问题），并回填缓存。
  - `SetupAudio()` 重写：解析真实 clip → 绑定+`Using AudioClip` 日志；clip 仍为空时——`allowPlaceholderWhenMissing` 才用 placeholder，否则 **LogError 且不启动音频（不再静默播放波波音）**。
  - `BeginRhythmMode` 仅在 `clip!=null` 时 `Play()`；否则 LogError，时间线冻结。
  - 谱面仍 `GenerateFullSongChart(clip.length)` 用真实长度（实测 songLength=290.5、notes=476、last=287.29）。
- **未改**：`PlayerConfessionNightModuleState.cs`/`ConfessionNightModulePickup.cs`（controller 端 Resources 兜底已足够）、Demo_Combat 场景与三个 pickup、RhythmNote 判定、RhythmHomingBullet、UI 碎裂、Boss、玩家。
- **测试（Play + teleport 触发真实 pickup 路径，0 红错）**：Console `[RhythmGame] Using AudioClip: Ayasa_Confession_Night, length=290.5, frequency=44100, channels=2, placeholder=False`（无 Placeholder 字样）；`clip.name=Ayasa_Confession_Night`(非 placeholder)、length=290.5、loop=True、vol=0.8；谱面 476/覆盖整首；同步循环 0→1 + 真 Update 循环 1→2（nextNoteIndex→0、清空、chart 复用、IsRhythmModeActive 仍 True、UI 不恢复）；红命中弹/蓝回血(+3)/Miss(-5) 回归正常；玩家死亡时 EndRhythmMode(restoreUI) 正确恢复 UI。退出 Play 后 IsRhythmModeActive=False。
- **注**：MCP step 驱动下编辑器音频 DSP 不前进（isPlaying 可能读 false），需用户在真实聚焦 Play 中实听；clip 绑定/长度/loop 已证明为真实歌。

### 音游 UI 左移 + 拾取卡顿优化 (Stage 54.3)
- **UI 左移**：`RhythmGameController` 新增 `hitCircleScreenX=0.25`、`hitCircleScreenY=0.18`（替代写死的 0.78/0.18），`BuildCanvas` 用 `Screen.width*hitCircleScreenX`。HitCircle 固定屏幕左侧（中心左 25% 宽度，1920 下 x=480、y=194）。**判定/音符目标/Miss 线/红蓝闪烁全部本就基于同一 `_hitCircleX`/HitCircle 对象**，无需改判定逻辑——音符仍从右侧(`spawnX=1.05W`)飞向新 HitCircle，命中/Miss 自动按新位置。实测 note@6.00:x480(到达新圈)、note@7.29:x1471(右侧进入)。
- **拾取卡顿真因**：按 F 同一帧同步做了全部重活——最大头是**同步 `Resources.Load` 13MB mp3（DecompressOnLoad）**，外加 canvas/2 张圆 sprite 纹理生成 + ~75 碎片 Instantiate + chart 生成，全挤一帧。
- **优化**（仅 ConfessionNight 相关脚本 + 该 mp3 导入设置）：
  1. **音频预加载**：`ConfessionNightModulePickup.Start()`（场景对象，开场即跑）`Resources.Load + LoadAudioData` 缓存，激活时把已加载 clip 经 `Activate(AudioClip)` 传入，F 帧零 `Resources.Load`。实测 `ResolveAudioClip cost=1.7ms`（原可达数十~上百 ms）。
  2. **mp3 导入改 Streaming**：`Assets/Resources/Audio/Ayasa_Confession_Night.mp3` loadType=Streaming/preload=false/loadInBackground=true → 加载与 Play 近零开销。
  3. **分帧激活**：`BeginRhythmMode` 仅置位 + `IsRhythmModeActive=true`(立即锁射击) + 启动 `BeginRhythmModeRoutine` 协程（先 `yield` 让 F 帧空转，再逐帧 BuildCanvas→SetupAudio→GenerateFullSongChart→Play，`_ready` 就绪后才跑 spawn/judge 管线）。每步独占一帧。
  4. **UI 碎裂限量+分帧**：`CombatUIBreakController` 新增 `maxTotalFragments=40`/`fragmentsPerElement=6`，`BreakRoutine` 每帧一个 UI 元素（先 yield，不在 F 帧生成），淡出永远执行（即便碎片预算用尽元素也隐藏）。
- **实测各步耗时（每步独占一帧，均 <20ms）**：CreateRhythmCanvas+HitCircle 7.0ms（首次圆 sprite 纹理生成，之后静态缓存）/ ResolveAudioClip 1.7ms / GenerateFullSongChart 2.0ms(476 notes,纯数据) / StartAudio 0.9ms / SpawnFragments 首批 6.5ms(含首次白方块 sprite 生成) 其余 0.9~1.1ms。F 帧(`ACTIVATED (staged)`)无重活。
- **回归（Play+teleport+Step，0 红错）**：clip=Ayasa_Confession_Night(290.5/loop)、chart476、HitCircle(480,194)、红命中弹/蓝回血(+3)/Miss(-5)、loop(0→1 nextIdx→0 清空 chart 复用)、射击锁定 IsRhythmModeActive=True。退出 Play IsRhythmModeActive=False。
- **未改**：Demo_Combat 场景与三 pickup 位置、RhythmNote 判定、RhythmHomingBullet、奖惩数值、音乐/谱面 loop 逻辑、谱面生成算法、Boss、普通敌人、玩家、CursedEightModule、BlessedEightModule。

### 红色追踪弹 视觉×5 + 重新锁定 (Stage 54.4)
- **需求**：音游红色音符命中发射的追踪弹——视觉放大 5 倍、追踪最近普通敌人、命中仍造成目标 maxHealth*3%、不影响普通玩家子弹/Boss/Cursed/Blessed。
- **`RhythmHomingBullet.cs` 重写**（仅此弹，纯新增/局部）：
  - **视觉×5 放到子物体 `Visual`**：root scale 恒 (1,1,1)（命中用距离 `hitDistance`，无 Collider2D，故视觉缩放绝不改命中范围）；`Visual.localScale = baseVisualScale(0.35) * visualScaleMultiplier(5) = 1.75`；SpriteRenderer 移到 `Visual` 上，root 无 SR（防双精灵）。Hierarchy：`RhythmHomingBullet/Visual`。
  - **重新锁定**：`retargetInterval=0.2s`；`Update` 中当 `_target==null||IsDead()` 时按间隔 `FindNearestEnemy()` 重新找最近活着的普通敌人；找不到则沿 `_lastDirection` 继续飞，`lifeTime=4s` 超时自毁。
  - **目标筛选** `FindNearestEnemy`：遍历 `Health`，排除 `IsDead()/currentHealth<=0`、排除 `tag==Player`、排除 Boss（`GetComponentsInParent` 名含 "Boss" 或物体名含 "Boss"；且 Boss 本就无 `Health`，双保险），取离子弹最近者。
  - **伤害不变**：`Mathf.Max(1, CeilToInt(target.maxHealth*0.03))` → `Health.TakeDamage`（3%，视觉放大不改伤害）。
- **`RhythmGameController.SpawnHomingBullet`**：生成对象去掉 `typeof(SpriteRenderer)`（`new GameObject("RhythmHomingBullet", typeof(RhythmHomingBullet))`），由弹自建 `Visual` 子物体；初始目标仍由 `FindNearestNormalEnemy` 给出，飞行中可重新锁定。生成位置不变（玩家上方）。
- **未改**：玩家普通子弹/`Projectile.cs`/普通敌人 AI/Boss/音游判定/Miss·Wrong 扣血/蓝色回血/音乐·谱面 loop/HitCircle 位置/CursedEightModule/BlessedEightModule。
- **测试（Play+teleport+反射触发，编译 0 红错）**：Test A 视觉×5——root scale(1,1,1)、`Visual` 子物体 scale(1.75)、SR 在 Visual、root 无 SR ✓；Test B 最近目标——bulletTarget=MeleeEnemy_01=最近普通敌人、targetIsPlayer=False ✓；Test D 伤害 3%——逻辑与 Stage54.2(30→29) 逐字相同且命中检测(hitDistance/root scale1)未变，故 3% 不变；Test C 重新锁定——逻辑就绪(_target 失效→0.2s 内重选)。普通玩家子弹未改、Cursed/Blessed pickup 未改。
  > 注：本轮 Play 测试时一次 `EditorApplication.Step()` ×200 的脚本驱动把编辑器卡在 Step 暂停态（MCP 桥重连循环）——属测试手法问题，非代码缺陷；编辑器需手动 Stop/聚焦恢复。视觉×5 与最近目标已在卡死前实测通过。

### PlayerController2D 改动（Stage 54，仅射击入口）
- 左键 `if (Input.GetMouseButtonDown(0) && !Cardwin.Modules.RhythmGameController.IsRhythmModeActive)` / 右键 `if (Input.GetMouseButtonDown(1) && !Cardwin.Modules.RhythmGameController.IsRhythmModeActive)` —— 音游模式中普通射击被锁定；移动/跳跃/冲刺/换弹不受影响。

### 测试（Play + EditorApplication.Step 确定性步进，0 红色错误）
- 激活：IsRhythmModeActive=True、chart=474、first=6.00、last=287.29（覆盖到 songLen290-2.71，**非 90s**）、red310/blue164(≈65/35)、audioLoop=True、audioPlaying=True、BPM93/beat0.645/travel2/hitWin110。
- UI 碎裂：HP_Text/MagazinePreview/State_Text/PlayerStatusHUD/BulletPreviewHUD/ComboRankHUD/CardwinHUDRoot 全部碎片+淡出→off；菜单(Pause/GameOver/Setting/Bag)跳过保留；75 碎片飞散。HitCircle 出现 (1498,194)=(0.78w,0.18h)。
- 音符管线：audioTime 驱动从右生成，到 hitTime 恰好抵达 HitCircle(x≈1498)，随时间左移(1832→1651)，越线自动 Miss 移除；红/蓝并存。
- 效果：Miss−5/Wrong−5(10% of50)、Blue+3(5%)、Red→追踪弹命中 MeleeEnemy_01 HP30→29(3% of30)且弹自毁。
- 循环（真 Update 检测）：audioTime 9→0.2 触发 OnMusicLooped，loopCount 0→1、nextNoteIndex→0、active→0、chart 474→474/first/last 不变(复用)、IsRhythmModeActive 仍 True、HUD 仍隐藏(不恢复)。
- 退出 Play：IsRhythmModeActive=False、Instance=null（OnDestroy 复位，不残留射击锁）。
- 未影响 Boss（无 Boss 脚本改动；追踪弹 IsBoss 过滤排除 Boss）、未破坏 CursedEight/BlessedEight（脚本未改，激活时单向停用之）。Console 红色错误 0。

---

## 27. MirrorAngel Boss — AI 自动机审计 + 运行时状态监控 + 作品集文档 (Stage 55)

> 本轮 **不重写 Boss、不改技能数值/CD、不破坏 BossRoom、不改玩家/三模组/Retry/打包**。仅：审计 Boss AI 自动机、新增可视化状态枚举 `BossAIState`、新增运行时监控组件 `MirrorAngelBossDebugState`、把 Brain 既有转换点镜像给监控（无逻辑改动）、输出两份作品集文档。`MirrorAngelBossActionController` 已全 public（`IsActionLocked`/`CurrentAction`/`CurrentToken`），仅被监控读取，**未修改**。

### 新增脚本 (Assets/Scripts/Boss/，命名空间 Cardwin.Boss)

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| BossAIState.cs | `BossAIState` (enum) | **[Stage 55]** 可视化专用 Boss AI 自动机状态：Idle/Decide/Approach/KeepDistance/Reposition/Windup/Casting/Recovery/AirMode/Dead。不替换 Brain 内部 `MirrorAngelBossBrainState`，不改决策逻辑 | — | — | MirrorAngelBossDebugState / MirrorAngelBossBrain(镜像) | 新增 |
| MirrorAngelBossDebugState.cs | `MirrorAngelBossDebugState` (Cardwin.Boss) | **[Stage 55]** 运行时状态监控(仅可视化)：Inspector 显示 CurrentState/CurrentSkillName/DistanceToPlayer/ActionLocked/CurrentActionToken/IsDead/IsAirMode；状态变化**低频日志** `[BossAI] State: X -> Y, reason=...`(仅变化时,非每帧)；可选 Scene 视图文字(Handles.Label, `#if UNITY_EDITOR`)。距离/锁/token/dead/air 每 0.1s 自刷新(读 ActionController/Mover/Boss 公开成员)；State/Skill 由 Brain 推送。无任何战斗逻辑 | `Awake/Update/RefreshRuntimeInfo/SetState/SetSkill/ClearSkill/UpdateRuntimeInfo/FindPlayer/OnDrawGizmos` | 挂 Boss 根 `MirrorAngelBoss`(RequireComponent MirrorSaintessBoss) | MirrorAngelBossBrain | 新增 |

### 小改脚本

| 文件名 | 类名 | 变更 | 说明 |
|--------|------|------|------|
| MirrorAngelBossBrain.cs | `MirrorAngelBossBrain` | **[Stage 55]** 新增 `[SerializeField] debugState` + Awake `GetComponent` 自动解析；新增私有 `PushDebug/PushSkill/ClearDebugSkill` 助手；在**既有**转换点(Dead/Decide/Approach×2/KeepDistance×2/Reposition/Windup+SetSkill/Casting或AirMode/Recovery+ClearSkill)调用助手镜像状态。**决策/评分/技能/数值/`MirrorAngelBossBrainState` 全未改** | 纯可视化镜像，不影响行为 |

### 接入的状态（Brain → DebugState）
`Decide`(DecideNextAction 入口) / `Approach`(player far & beyond preferred range) / `KeepDistance`(attackChance roll & safe distance) / `Reposition`(player too close) / `Windup`(+SetSkill skillId) / `Casting`(AirLaserMode→`AirMode`) / `Recovery`(+ClearSkill, 技能结束) / `Dead`(hp<=0)。

### Prefab `MirrorAngelBoss.prefab` 变更 (Stage 55)
- root 新增组件 `MirrorAngelBossDebugState`（headless `modify_contents` 加在根；BossRoom 场景实例自动继承）。Brain 的 `debugState` 运行时由 Awake `GetComponent` 自动解析（已实测 wire 到该组件）。
- 未改其它任何组件、Body、Collider、Animator、技能、HP。

### ActionController 暴露的调试信息（已有，未改）
`IsActionLocked`(bool) / `CurrentAction`(MirrorAngelActionType) / `CurrentToken`(int) / `IsSkillMotionAllowed`(bool)。DebugState 读取这些得到 ActionLocked / Token / AirMode(CurrentAction==AirLaserMode 或 mover.IsFlying)。

### 新增文档
- `Docs/BossAIStateMachine.md`：总体目标 / 模块职责 / 10 状态表 / 每状态进入退出/移动/技能/打断/Animator / Mermaid 状态图 / 当前 vs 理想差距 / 后续计划。
- `Docs/BossSkillFlow.md`：统一技能流程(Decide→BeginAction→Windup→Active→Recovery→EndAction→Death ForceCancel) / 为什么要 ActionLock / 为什么要 actionToken / 常见 Bug / Mermaid 流程图。

### 测试（Play BossRoom + 确定性 API 驱动，0 红色错误）
- 编译：强制全量刷新 + 重编译后 Console 红色错误 = **0**。
- 监控 live：进入 BossRoom Play，`MirrorAngelBossDebugState` 在 Boss 根实例上，`DistanceToPlayer` 自刷新(16.03)、refs 全 wire(boss/actionController/mover)、Brain.debugState→该组件。
- 状态机+低频日志：确定性 API 驱动 12 次 SetState（含 1 次重复 Decide）→ Console 恰好 **9** 条 `[BossAI] State: X -> Y, reason=...`（重复 Decide **不**重复打印 → 仅变化时打印），序列 Idle→Decide→Approach→Decide→Windup→Casting→Recovery→Decide→AirMode→Dead 全部覆盖。
- Inspector 字段：CurrentState/CurrentSkillName/DistanceToPlayer/ActionLocked/CurrentActionToken/IsDead/IsAirMode 均可读。
- 注：纯 MCP 驱动时编辑器无焦点，Play 主循环不前进（distance 跨 12s 字节级不变），故 Boss 自然行走/施法需在真实聚焦 Play 观察；行为逻辑未改、编译 0 错、监控接入已确定性验证。
- 回归：Boss 行为/技能/死亡逻辑代码未改（仅加可视化推送）；BossRoom 布局/Animator/数值/玩家/三模组/Retry/打包未动。

---

## 28. Lua 子弹系统（最小试点）(Stage 57)

> 增量试点：用 Lua 数据表增删改查子弹，自动进入背包与敌人掉落，统一由通用宿主 `LuaBulletHost` 承载行为。**未重写旧 Projectile / EnemyProjectile / RhythmHomingBullet / Boss / 玩家移动射击主逻辑 / Cursed·Blessed·Confession / Retry / RewardManager**。命名空间 `Cardwin.Lua`，目录 `Assets/Scripts/Lua/`。**项目未接入 xLua/tolua**：注册表由自研「简化 Lua 表解析器」真实解析（CRUD/背包/掉落数据驱动）；行为脚本由按 `behavior` 字符串映射的 C# 桥接执行，`.lua` 行为文件作为规范格式+未来真热更目标保留。全部运行时代码不使用 UnityEditor-only API，可打包。

### 新增脚本 (Assets/Scripts/Lua/，命名空间 Cardwin.Lua)

| 文件名 | 类名 | 主要职责 | 当前状态 |
|--------|------|----------|----------|
| SimpleLuaTable.cs | `LuaTable` / `SimpleLuaTableParser` | 简化 Lua 表解析器：tokenizer+递归下降，解析 `return { key=value, 嵌套表, 字符串数组, 数字, 布尔, nil, -- 注释 }` → `LuaTable`(Map+Array)。仅解析数据表，不执行 Lua 逻辑 | 新增 |
| LuaBulletDefinition.cs | `LuaBulletDefinition` | 一条 Lua 子弹的运行时数据 POCO（Id/Enabled/display/card/bullet/inventory/drop 全字段）+ `CanDropFor(enemyType)` | 新增 |
| LuaBulletDatabase.cs | `LuaBulletDatabase` | 单例：读取 StreamingAssets/Lua/Bullets/BulletRegistry.lua → 解析 → 缓存 Dictionary。提供 `GetBullet/ListAll/ListEnabled/ListInventoryBullets/ListDropBullets/Reload/ReloadLuaBullets`。过滤 enabled=false。打印 `Loaded Lua bullets: N` | 新增 |
| ILuaBulletBehavior.cs | `ILuaBulletBehavior` (interface) | 行为契约 `OnSpawn/OnUpdate/OnHit/OnRecycle`（镜像 Lua 回调形态） | 新增 |
| LuaBulletBehaviorRegistry.cs | `LuaBulletBehaviorRegistry` (static) | `behavior` 字符串 → ILuaBulletBehavior 映射；默认注册 Pierce/Homing；`Fallback` 直线弹。未来接 Lua VM 只需注册 LuaBackedBehavior | 新增 |
| PierceBulletBehavior.cs | `PierceBulletBehavior` | C# 桥接「Bullets.PierceBullet」：直线飞 + 穿透 pierceCount 个敌人各扣血一次，耗尽回收 | 新增 |
| HomingBulletBehavior.cs | `HomingBulletBehavior` | C# 桥接「Bullets.HomingBullet」：按 turnSpeed 转向最近敌人，命中一次扣目标 maxHP×percent 后回收 | 新增 |
| LuaBulletDamage.cs | `LuaBulletDamage` (static internal) | 按 damageMode 施加伤害（Flat→Damage / PercentTargetMaxHp→DamagePercentOfMaxHp），统一经 LuaBattleAPI | 新增 |
| LuaBattleAPI.cs | `LuaBattleAPI` (static) | 给行为调用的安全接口：FindNearestEnemy/FindNearestBoss/IsDead/Damage/DamagePercentOfMaxHp/HealPlayer/Move/MoveToward/PlayEffect/RecycleBullet/ResolveDamageableOwner。伤害一律经 Health.TakeDamage 或 IDamageable.TakeHit | 新增 |
| LuaBulletHost.cs | `LuaBulletHost` (+ `LuaBulletVisual`) | 通用 Unity 宿主：运行时自建 SpriteRenderer+Kinematic Rigidbody2D(g0)+trigger CircleCollider2D；持 Definition/Direction/Context/RemainingPierce/CurrentTarget；`Spawn()` 静态工厂；Update 计寿命+驱动 OnUpdate；OnTriggerEnter2D 过滤(Player/自身/Projectile/Trigger/Ground)+去重后调 OnHit；Recycle→OnRecycle+Destroy。无需 prefab 资源 | 新增 |
| LuaBulletCardBridge.cs | `LuaBulletCardBridge` (static) | 为每个 inventory 子弹创建运行时 CardData(isLuaBullet=true/luaBulletId/cardName=display.name)，缓存；`AddInventoryBulletsToBackpack(inv)` 按 defaultCount 加入背包（幂等）。运行时 CardData 不写盘 | 新增 |
| LuaBulletDropBridge.cs | `LuaBulletDropBridge` (static) | `GetDropCandidates(enemyType)/RollDrop(enemyType)`(加权随机)/`TryDropToInventory(...)`。enabled=false / drop.enabled=false / enemyType 不匹配 均不掉落；掉落进背包 | 新增 |
| LuaBulletRuntimeManager.cs | `LuaBulletRuntimeManager` | RuntimeInitializeOnLoadMethod 自举(DontDestroyOnLoad)：startup 载入注册表；每次场景加载把掉落 roll 订阅到 MeleeEnemy/RangedEnemy 的 Health.OnDeath →死亡掉落进背包。不改敌人 AI/Prefab/RewardManager | 新增 |

### 小改脚本（仅追加，旧逻辑不变）

| 文件名 | 变更 |
|--------|------|
| Cards/CardData.cs | 新增 `bool isLuaBullet` / `string luaBulletId`（旧 asset 不受影响） |
| Cards/CardEffectExecutor.cs | `ExecuteLeft` 顶部加 Lua 分支：`isLuaBullet`→`SpawnLuaBullet`(查定义/Enabled→`LuaBulletHost.Spawn`+FireRed 视觉)+return；普通子弹路径逐字不变。禁用子弹不发射也不回退普通弹 |
| Inventory/InventorySystem.cs | 新增 `AddRuntimeCard(CardData,int)`（运行时卡加入背包入口） |
| Combat/PlayerController2D.cs | `InitializeInventoryAndLoadout()` 在 `InitializeForRun` 后加 1 行 `LuaBulletCardBridge.AddInventoryBulletsToBackpack(inventorySystem)` |

### Lua 资源

| 路径 | 说明 |
|------|------|
| `Assets/StreamingAssets/Lua/Bullets/BulletRegistry.lua` | 注册表（被 C# 真实解析）：`lua_pierce_001`(穿透3, Flat 8, 背包8, 掉落 Melee/Ranged w20) + `lua_homing_001`(追踪, turnSpeed720, 目标maxHP×3%, 背包4, 掉落 Melee/Ranged/BossRoom w10) |
| `Assets/StreamingAssets/Lua/Bullets/PierceBullet.lua` | 穿透弹行为（格式参考 / 未来热更目标） |
| `Assets/StreamingAssets/Lua/Bullets/HomingBullet.lua` | 追踪弹行为（格式参考 / 未来热更目标） |

### 链路

```
CardData(isLuaBullet) / 背包 → CardEffectExecutor.ExecuteLeft 发现 isLuaBullet
→ SpawnLuaBullet(luaBulletId) → LuaBulletHost.Spawn(def,...) → 行为 OnSpawn/OnUpdate/OnHit/OnRecycle
→ LuaBattleAPI 查敌/伤害/回收（伤害仍走 Health.TakeDamage / IDamageable.TakeHit）
```

### 文档
- `Docs/LuaBulletSpec.md`：注册表格式 / 字段含义 / 增删改查 / 背包接入 / 掉落接入 / 行为脚本格式 / 当前限制 / 后续对象池·Addressables·真热更计划。

### 测试（编辑器 execute_code 确定性验证，编译 0 红色错误）
- A 读取：`db.Reload()` 打印 `Loaded Lua bullets: 2`；RegistryPath 存在。
- B 查询：ListAll=2 / ListEnabled=2 / ListInventory=2 / ListDrop(Melee)=2 / ListDrop(Ranged)=2 / ListDrop(BossRoomEnemy)=1 / ListDrop(Unknown)=0。
- C 数据/卡：pierce(pierceCount3/speed12/8 Flat/defCount8/w20) + homing(turnSpeed720/0.03 PercentTargetMaxHp/defCount4/tags 解析)；运行时 CardData isLuaBullet=True/luaId/rarity=Rare；行为均可 Resolve。
- D 发射(结构)：`LuaBulletHost.Spawn` 生成宿主：Kinematic RB g0、trigger CircleCollider r0.35、scale 0.375(0.25×1.5)、RemainingPierce3、Direction(1,0)。
- G 删除：`enabled=false`+Reload → Enabled=False/ListEnabled=1/ListInventory=1/ListDrop(Melee)=1；GetBullet 仍返回(不崩)。已还原。
- H 修改：homing speed 10→25+Reload → Speed=25。已还原。
- E Pierce 穿透 / F Homing 追踪：代码完成（PierceBehavior 每命中 RemainingPierce-- 至 0 回收；HomingBehavior MoveTowardsAngle 转向 + DamagePercentOfMaxHp），需真实聚焦 Play 观察物理帧。
- 影响旧 Projectile / Cursed/Blessed/Confession / Boss / 玩家：**无**。UnityEditor-only API：**无**。
