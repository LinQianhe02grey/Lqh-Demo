# SYSTEM_INDEX.md — 系统索引

> 最后更新：2026-05-29

---

## 1. Core System
游戏入口、全局状态管理、场景加载、事件总控。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| GameState.cs | `GameState` (enum) | 游戏状态枚举 (MainMenu/Playing/Paused/GameOver/Victory) | — | — | GameManager, GameStateMachine | 骨架完成 |
| GameManager.cs | `GameManager` | 全局单例，场景加载，状态管理 | `Awake()`, `SetState()`, `LoadScene()`, `RestartCurrentLevel()`, `QuitGame()` | 初始化单例 / 状态切换 / 场景切换 / 重载关卡 / 退出 | 全局 | 骨架完成 |
| GameStateMachine.cs | `GameStateMachine` | 状态机，注册处理器，状态切换通知 | `TransitionTo()`, `RegisterHandler<T>()` | 切换状态并通知所有处理器 | GameManager | 骨架完成 |
| GameStateMachine.cs | `IGameStateHandler` (interface) | 状态变化监听接口 | `OnStateChanged(GameState)` | 响应状态切换 | GameStateMachine | 骨架完成 |

---

## 2. Combat System
伤害计算、格挡、治疗、命中判定、死亡处理。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| PlayerController2D.cs | `PlayerController2D` | 玩家控制：移动/跳跃/二段跳/冲刺/精灵翻转/鼠标方向射击 | `Awake()`, `Update()`, `FixedUpdate()`, `Jump()`, `StartDash()`, `IsGrounded()`, `FlipSprite()`, `Shoot()` | 组件缓存 / 输入+状态 / 物理速度 / 跳跃(二段跳) / 冲刺+无敌+CD / 地面检测 / A/D翻转 / 鼠标方向发射子弹 | Input Manager / Update Loop | 已完成 |
| Health.cs | `Health` | 通用血量：血量/格挡/受击/治疗/死亡/无敌 | `Awake()`, `SetInvincible()`, `TakeDamage()`, `Heal()`, `GainBlock()`, `IsDead()`, `Die()` | 初始化 / 无敌标记 / 受击(无敌检查+格挡先吸收+死亡) / 治疗(上限保护) / 格挡 / 死亡判定 / 死亡 | PlayerController2D / EnemyController / Projectile | 已完成 |
| EnemyController.cs | `EnemyController` | 敌人：追逐玩家/接触伤害/冷却/Health管理 | `Awake()`, `Start()`, `Update()`, `OnCollisionStay2D()` | 组件缓存 / 查找Player / 追逐移动+死亡检查 / 接触玩家伤害(冷却) | Combat System | 已完成 |
| Projectile.cs | `Projectile` | 子弹：方向飞行/命中伤害/自动销毁/朝向旋转 | `Awake()`, `Init()`, `Update()`, `OnTriggerEnter2D()` | 缓存Rigidbody / 初始化方向速度伤害 / 生命周期销毁 / 命中Health扣血 | PlayerController2D.Shoot | 已完成 |
| DamageInfo.cs | `DamageInfo` (struct) | 伤害数据结构：基础伤害+Focus加成+来源 | `TotalDamage` (property) | 计算最终伤害值 | Combat 系统 | 骨架完成 |

---

## 3. Camera System
摄像机跟随、边界限制。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| CameraFollow2D.cs | `CameraFollow2D` | 平滑跟随玩家，边界钳制 | `Awake()`, `LateUpdate()`, `FindTargetIfMissing()` | 缓存Camera / 跟随+边界Clamp / 按Tag查找Player并警告 | Camera Update Loop | 已完成 |

---

## 4. Card System
ScriptableObject 卡牌数据定义、卡牌效果接口与实现。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| CardData.cs | `CardData` | ScriptableObject 卡牌数据资产定义 | `IsSelfTarget()` | 判定是否自用卡牌 | MagazineSystem / CardEffectExecutor | 骨架完成 |
| CardData.cs | `TargetType` (enum) | 目标类型：Enemy / Self / SelfOrEnemy | — | — | CardData | 骨架完成 |
| CardData.cs | `CardEffectEntry` (struct) | 单个卡牌效果数据（类型/数值/重复次数） | — | — | CardData | 骨架完成 |
| CardType.cs | `CardType` (enum) | 卡牌类型：Attack / Skill / Power | — | — | CardData | 骨架完成 |
| CardRarity.cs | `CardRarity` (enum) | 稀有度：Common / Uncommon / Rare / Legendary | — | — | CardData | 骨架完成 |
| CardEffectType.cs | `CardEffectType` (enum) | 效果操作类型（Damage/Heal/GainBlock/ApplyStatus 等 11 种） | — | — | CardEffectExecutor / Projectile | 骨架完成 |
| CardRuntimeInstance.cs | `CardRuntimeInstance` | 运行时卡牌实例，包装 CardData + 升级等级 | `CardId`, `DisplayName`, `Cost`, `IsSelfTarget` (properties) | 提供运行时只读属性 | MagazineSystem / InventorySystem | 骨架完成 |
| CardEffectExecutor.cs | `CardEffectExecutor` | 卡牌效果执行器，分发到具体效果方法 | `Initialize()`, `ExecuteOnEnemy()`, `ExecuteOnSelf()`, `ExecuteDamage()`, `ExecuteHeal()`, `ExecuteGainBlock()`, `ExecuteApplyStatus()` | 初始化上下文 / 对敌执行 / 对己执行 / 伤害/治疗/格挡/状态 | Projectile / PlayerController2D | 骨架完成 |
| PlayerCardContext.cs | `PlayerCardContext` | ScriptableObject：缓存玩家引用、Focus 层数 | `GetFocusBonus()`, `CacheReferences()` | 获取 Focus 伤害加成 / 缓存玩家组件 | CardEffectExecutor | 骨架完成 |

---

## 5. Magazine System
弹夹管理、弹药消耗、换弹、下 N 发预览。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| MagazineSystem.cs | `MagazineSystem` | 弹药池 CRUD、弹夹洗牌、换弹、预览 | `Initialize()`, `BuildShuffledLoadedMagazine()`, `TryGetCurrentBullet()`, `ConsumeCurrent()`, `StartReload()`, `FinishReload()`, `GetUpcomingBullets()`, `SetBulletPool()`, `AddBullet()`, `RemoveBulletAt()`, `SwapSlots()` | 初始化 / Fisher-Yates洗牌 / 获取当前弹 / 消耗+自动换弹 / 换弹 / 预览 / 弹药池操作 | PlayerController2D / MagazinePreviewUI | 骨架完成 |
| MagazineSlot.cs | `MagazineSlot` | 弹夹预览槽位数据结构 | `SetCard()`, `Clear()` | 设置预览内容 / 清空 | MagazinePreviewUI | 骨架完成 |

---

## 6. Inventory System
背包存储、卡牌增删查、上场/下场。

| 文件名 | 类名 | 主要职责 | 函数名 | 函数用途 | 被谁调用 | 当前状态 |
|--------|------|----------|--------|----------|----------|----------|
| InventorySystem.cs | `InventorySystem` | 24 格网格背包，堆叠，CRUD | `Initialize()`, `AddItem()`, `RemoveItem()`, `SwapSlots()`, `ClearSlot()`, `HasItem()`, `GetItemCount()` | 初始化 / 添加物品 / 移除 / 交换格子 / 清空 / 查询 | InventoryUI / ShopManager | 骨架完成 |
| InventorySystem.cs | `InventorySlot` | 单个格子数据（物品ID/数量/堆叠上限） | `IsEmpty()`, `CanStackWith()` | 判空 / 堆叠检查 | InventorySystem | 骨架完成 |

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
| CombatHUD.cs | `CombatHUD` | 战斗 HUD：血条/格挡/弹药/换弹条/状态文字 | `UpdateHP()`, `UpdateBlock()`, `UpdateAmmo()`, `UpdateReloadProgress()`, `SetStateText()` | 更新血条 / 格挡 / 弹药 / 换弹进度 / 状态 | Health / MagazineSystem | 骨架完成 |
| MagazinePreviewUI.cs | `MagazinePreviewUI` | 最近 N 发卡牌预览 UI | `Bind()`, `RefreshPreview()`, `HighlightCurrentBullet()` | 绑定弹夹系统 / 刷新预览 / 高亮当前弹 | MagazineSystem | 骨架完成 |
| CardSlotUI.cs | `CardSlotUI` | 单张卡牌显示槽（图标/名称/描述/费用/使用按钮） | `SetCard()`, `Clear()`, `OnUseClicked()` | 显示卡牌 / 清空槽位 / 使用按钮回调 | InventoryUI / MagazineEditUI | 骨架完成 |
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
| CardwinSceneBuilder.cs | `CardwinSceneBuilder` | 备份场景生成工具（非日常使用） | `BuildDemoScene()`, `EnsureGroundLayer()`, `GetGroundLayer()`, `CreateMainCamera()`, `CreateGround()`, `CreatePlatforms()`, `CreateCameraBounds()`, `CreatePlayer()`, `CreateTestMarkers()`, `CreateMarker()`, `CreateCanvasHUD()`, `CreateHUDText()`, `CreateWhiteSquareSprite()`, `CreatePlaceholderSprite()` | 菜单入口 / Ground Layer / 各对象创建 | 开发者菜单 Tools/Cardwin/Build Demo Scene | 已完成（备份工具） |

## 11. Scenes

| 场景名 | 用途 | 当前状态 |
|--------|------|----------|
| `Demo_Combat.unity` | 主要测试场景，日常开发唯一使用的场景 | 活跃 |
| `CardwinSceneBuilder` | 备份/重建工具，仅在用户明确要求时运行 | 备份 |
