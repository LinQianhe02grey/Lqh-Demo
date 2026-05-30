# SYSTEM_INDEX.md — 系统索引

> 最后更新：2026-05-30 (Stage 7A.6 — Fix BagPanel Owned Cards UI Invisible)

---

## 1. Core System
游戏入口、全局状态管理、场景加载、事件总控。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| GameState.cs | `GameState` (enum) | 游戏状态枚举 (MainMenu/Playing/Paused/GameOver/Victory) | — | — | GameManager, GameStateMachine | 骨架完成 |
| GameManager.cs | `GameManager` | 全局单例，场景加载，状态管理 | `Awake()`, `SetState()`, `LoadScene()`, `RestartCurrentLevel()`, `QuitGame()` | 初始化单例 / 状态切换 / 场景切换 / 重载关卡 / 退出 | 全局 | 骨架完成 |
| GameStateMachine.cs | `GameStateMachine` | 状态机，注册处理器，状态切换通知 | `TransitionTo()`, `RegisterHandler<T>()` | 切换状态并通知所有处理器 | GameManager | 骨架完成 |
| GameStateMachine.cs | `IGameStateHandler` (interface) | 状态变化监听接口 | `OnStateChanged(GameState)` | 响应状态切换 | GameStateMachine | 骨架完成 |
| DemoSceneRuntimeBootstrapper.cs | `DemoSceneRuntimeBootstrapper` | 运行时自动配置场景对象：Camera跟随/Player Layer/Enemy Trigger/碰撞层忽略 | `Awake()`, `ResolveLayers()`, `FindCoreObjects()`, `ConfigureCamera()`, `ConfigurePlayer()`, `ConfigureGroundAndPlatforms()`, `ConfigureEnemy()`, `DisableBlockingPlaceholders()`, `IgnorePlayerEnemyCollision()`, `PrintColliderReport()` | 分层解析 / 查找核心对象 / 摄像机绑定CameraFollow2D / Player Tag+Layer+GroundCheck+groundLayer / Ground/Platform Layer+Collider / Enemy Trigger+Kinematic / 占位物Collider禁用 / Player-Enemy忽略碰撞 / 场景Collider报告 | Play模式启动时(−1000执行顺序) | 已完成 |

---

## 2. Combat System
伤害计算、格挡、治疗、命中判定、死亡处理。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| PlayerController2D.cs | `PlayerController2D` | 玩家控制：移动/跳跃/二段跳/冲刺/卡牌系统(Magazine优先→testCard fallback→Shoot fallback)/R键换弹/inputLocked输入锁定。**Stage 7A.5: Awake不再AddComponent，改为GetComponent+Error日志** | `Awake()`, `Update()`, `FixedUpdate()`, `Jump()`, `StartDash()`, `SetInputLocked()`, `IsGrounded()`, `FindGroundCheckIfMissing()`, `OnDrawGizmosSelected()`, `FlipSprite()`, `Shoot()` | Awake只查找已有组件(GetComponent/FindObjectOfType)+缺失时LogError / 不再动态AddComponent创建核心系统(CardEffectExecutor/InventorySystem/MagazineEditUI) / 核心系统必须场景预挂载 | Input Manager / Update Loop / MagazineEditUI | 已完成 |
| Health.cs | `Health` | 通用血量：血量/格挡/受击/治疗/死亡(自毁)/无敌 | `Awake()`, `SetInvincible()`, `TakeDamage()`, `Heal()`, `GainBlock()`, `IsDead()`, `Die()` | 初始化 / 无敌标记 / 受击(无敌检查+格挡先吸收+死亡) / 治疗(上限保护) / 格挡 / 死亡判定 / 死亡+Destroy(gameObject) | PlayerController2D / EnemyController / Projectile | 已完成 |
| EnemyController.cs | `EnemyController` | 敌人：Kinematic追逐/纯Trigger接触伤害/冷却/Health管理 | `Awake()`, `Start()`, `Update()`, `OnTriggerStay2D()`, `TryDamagePlayer()` | 组件缓存+Kinematic+freezeRotation / 查找Player / Kinematic MovePosition移动+死亡检查 / Trigger接触伤害(仅Trigger) / 统一冷却伤害 | Combat System | 已完成 |
| Projectile.cs | `Projectile` | 子弹：运行时视觉兜底/支持CardData效果投射/swift移动/命中过滤 | `Awake()`, `EnsureVisibleDebugSprite()`, `CreateRuntimeSprite()`, `Init(damage)`, `Init(card+effect+context)`, `Update()`, `OnTriggerEnter2D()` | 运行时sprite兜底 / 旧fallback伤害Init / 新卡牌效果Init(携带CardData+CardEffectType+PlayerCardContext) / 命中→调用CardEffectExecutor.ApplyEffectToTarget / 过滤非战斗目标 | CardEffectExecutor.ExecuteLeft / PlayerController2D.Shoot | 已完成 |
| DamageInfo.cs | `DamageInfo` (struct) | 伤害数据结构：基础伤害+Focus加成+来源 | `TotalDamage` (property) | 计算最终伤害值 | Combat 系统 | 骨架完成 |
| SceneCollisionReporter.cs | `SceneCollisionReporter` | 运行时 Debug：输出场景所有 Collider 信息 | `Start()`, `Update()`, `ReportSceneColliders()` | 启动时/F1键输出 / 打印Collider名/Layer/Trigger/Rigidbody类型 | 开发者调试 | 已完成 |

---

## 3. Camera System
摄像机跟随、边界限制。命名空间：`Cardwin.Cameras`（避免与 `UnityEngine.Camera` 冲突）。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| CameraFollow2D.cs | `CameraFollow2D` | 平滑跟随玩家，边界钳制(默认关闭) | `Awake()`, `LateUpdate()`, `FindTargetIfMissing()` | 缓存Camera / 跟随+边界Clamp(useBounds=false默认) / 按Tag查找Player并警告(仅一次) | Camera Update Loop | 已完成 |

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
| MagazineSystem.cs | `MagazineSystem` | 8发弹夹：随机装弹(从loadoutCards)/UseLeft/UseRight/ManualReload/自动换弹/预览/事件；_hasUserLoadoutInit后禁止fallback initialCards；Loadout空→loadedCards空 | `Start()`, `Update()`, `InitializeDefaultLoadoutIfEmpty()`, `SetLoadoutCards()`, `GetLoadoutCards()`, `GetLoadedCards()`, `BuildRandomMagazine()`, `BuildRandomMagazineFallback()`, `ResolveSourcePool()`, `GetCurrentCard()`, `GetPreviewCards()`, `UseCurrentCardLeft()`, `UseCurrentCardRight()`, `ManualReload()`, `AdvanceIndex()`, `StartReload()`, `FinishReload()` | SetLoadoutCards设_hasUserLoadoutInit=true/ResolveSourcePool=_hasUserLoadoutInit且Loadout空→返回空列表不fallback/BuildRandomMagazineFallback仅Start未init时用initialCards | PlayerController2D / MagazinePreviewUI / MagazineEditUI | 已完成 |
| MagazineSlot.cs | `MagazineSlot` | 弹夹预览槽位数据结构 | `SetCard()`, `Clear()` | 设置预览内容 / 清空 | MagazinePreviewUI | 骨架完成 |

---

## 6. Inventory System
背包存储、卡牌增删查、上场/下场。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| InventorySystem.cs | `InventorySystem` | 玩家拥有卡牌列表，增删查，测试库存(4种x20)，从CardDatabase强制初始化ResetToTestStock | `ResetToTestStock()`, `AddCard()`, `AddCards()`, `RemoveCard()`, `GetOwnedCards()`, `GetCount()`, `GetCardCounts()`, `HasCard()`, `EnsureTestStockIfEmpty()` | ResetToTestStock强制清空+自查找CardDatabase+4种x20+_testStockReset=true / RemoveCard返回bool / GetCardCounts返回List<InventoryEntry>聚合 | MagazineEditUI / PlayerController2D | 已完成 |

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
| MagazineEditUI.cs | `MagazineEditUI` | 背包/弹夹编辑界面：Open时强制ResetToTestStock+InitializeDefaultLoadoutIfEmpty/左侧聚合显示/右侧8格Loadout/点击扣除返还Inventory/修改后SetLoadoutCards(+flag)/B+Esc/InputLock+timeScale=0 | `Awake()`, `Start()`, `Update()`, `Toggle()`, `Open()`, `Close()`, `Refresh()`, `FindCardDatabase()`, `RefreshOwnedCards()`, `RefreshLoadoutSlots()`, `OnOwnedCardClicked()`, `OnLoadoutSlotClicked()`, `EnsureEventSystem()`, `EnsureUI()` | Open每次强制inventory.ResetToTestStock→magazine.InitializeDefaultLoadoutIfEmpty→显示/锁定/暂停 / FindCardDatabase统一查找逻辑 | PlayerController2D (SetInputLocked) | 已完成 |
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

## 11. Scenes

| 场景名 | 用途 | 当前状态 |
|--------|------|----------|
| `Demo_Combat.unity` | 主要测试场景，Stage 3.5 重建，Stage 4 后锁定（不可重建） | 活跃 — LOCKED |
| `CardwinSceneBuilder` | 备份恢复工具：`Tools/Cardwin/Rebuild Clean Demo Scene`（仅在明确要求时运行） | 备份 |

## 12. Projectile Prefab

| 路径 | 说明 | 状态 |
|------|------|------|
| `Assets/Prefabs/Projectiles/Projectile_Test.prefab` | 测试投射物：SpriteRenderer + Kinematic Rigidbody2D(gravity=0) + CircleCollider2D(isTrigger) + Projectile | 已创建 |
