# PROJECT_FUNCTION_INDEX.md — 函数级索引

> 生成时间：2026-06-01 | Stage 11A

---

## PlayerController2D (`Assets/Scripts/Combat/PlayerController2D.cs`)

| 函数名 | 访问级别 | 调用时机 | 功能说明 | 核心逻辑 | 可能风险 |
|---|---|---|---|---|---|
| `Awake()` | private | Awake | 初始化：Rigidbody2D/Health/CardEffectExecutor/MagazineSystem/InventorySystem/MagazineEditUI/ComboRatingSystem 引用获取 | Yes | 组件缺失时 Error Log，不 AddComponent |
| `Update()` | private | Update | 处理输入(A/D移动/Space跳跃/Shift冲刺/左键右键射击/B键背包) | Yes | _inputLocked 时跳过 |
| `FixedUpdate()` | private | FixedUpdate | 物理移动应用 velocity | Yes | locked 时 velocity 归零 |
| `Move(float)` | public | Update | 水平移动 + 精灵翻转 | Yes | — |
| `Jump()` | public | Update | 跳跃/二段跳，velocity 设置 | Yes | groundCheck 需正确绑定 |
| `StartDash()` | private | Update | 冲刺 + 无敌标记 | Yes | dashDuration/dashCooldown 控制 |
| `IsGrounded()` | public | Update/FixedUpdate | GroundCheck 检测 | Yes | 仅检测 Ground layer |
| `Shoot()` | private | Update | 发射测试子弹（magazineSystem 不存在时 fallback） | No | magazine 存在时永不执行 |
| `SetInputLocked(bool)` | public | External | 锁定/解锁战斗输入 | Yes | 背包打开时调用 |
| `EnsureRigidbodySetup()` | private | Awake/Jump | 确保 Rigidbody2D 正确设置 | Yes | Dynamic+gravityScale=3 |
| `FindGroundCheckIfMissing()` | private | Awake | 运行时创建 GroundCheck 子物体 | Yes | 一次性 |
| `OnDrawGizmosSelected()` | private | Editor | 绘制地面检测范围 | No | 仅编辑模式 |

---

## Health (`Assets/Scripts/Combat/Health.cs`)

| 函数名 | 访问级别 | 调用时机 | 功能说明 | 核心逻辑 | 可能风险 |
|---|---|---|---|---|---|
| `Awake()` | private | Awake | 初始化 currentHealth=maxHealth | Yes | — |
| `SetInvincible(bool)` | public | External | 设置无敌状态 | Yes | — |
| `TakeDamage(int)` | public | External | 受击：无敌检查→格挡先吸收→死亡触发 | Yes | 格挡=0 时直接扣血 |
| `Heal(int)` | public | External | 治疗：上限保护 (maxHealth) | Yes | — |
| `GainBlock(int)` | public | External | 格挡叠加 | Yes | — |
| `IsDead()` | public | External | 死亡判定 | Yes | — |
| `Die()` | private | Internal | 死亡触发：OnDeath.Invoke() + Destroy(gameObject, 0.1f) | Yes | 0.1s 延迟销毁 |

---

## Projectile (`Assets/Scripts/Combat/Projectile.cs`)

| 函数名 | 访问级别 | 调用时机 | 功能说明 | 核心逻辑 | 可能风险 |
|---|---|---|---|---|---|
| `Awake()` | private | Awake | 确保可见性：SortingOrder/scale/SpriteRenderer | Yes | — |
| `Init(Vector2, int)` | public | External | 旧 fallback 伤害初始化 | No | Legacy |
| `Init(Vector2, CardData, CardEffectType, PlayerCardContext)` | public | External | 新卡牌效果初始化（携带完整上下文） | Yes | — |
| `Update()` | private | Update | 飞行 + 超时自毁 | Yes | speed=4 为调试值 |
| `OnTriggerEnter2D(Collider2D)` | private | Unity | Trigger 命中处理 | Yes | 过滤 Player/Projectile/非战斗对象 |
| `OnCollisionEnter2D(Collision2D)` | private | Unity | Collision 命中处理 | Yes | 与 Trigger 共享 HandleHit |
| `HandleHit(GameObject)` | private | Internal | 统一命中处理：Health→CardEffectExecutor.ApplyEffectToTarget | Yes | GetComponentInParent<Health> 回退 |
| `EnsureVisibleDebugSprite()` | private | Awake | 运行时 sprite 兜底 | Yes | — |
| `CreateRuntimeSprite()` | private | Awake | 32x32 黄色圆点 Texture2D | No | 每次新 Texture，内存增长（测试可接受） |

---

## CardEffectExecutor (`Assets/Scripts/Cards/CardEffectExecutor.cs`)

| 函数名 | 访问级别 | 调用时机 | 功能说明 | 核心逻辑 | 可能风险 |
|---|---|---|---|---|---|
| `Initialize(PlayerCardContext)` | public | Awake | 设置运行时上下文 | Yes | — |
| `ExecuteLeft(CardData, PlayerCardContext)` | public | Input | 左键：生成 Projectile (携带 card+effect+context) | Yes | Damage 效果→发射 Projectile；非 Damage→对 Player 自用 |
| `ExecuteRight(CardData, PlayerCardContext)` | public | Input | 右键：对 Player 自己施效 | Yes | — |
| `ApplyEffectToTarget(CardData, CardEffectType, GameObject, PlayerCardContext)` | public | Projectile 命中 | 统一施加 Damage/Block/Heal/Focus | Yes | 仅实现了4种；其他 effect 有 if 分支但空 |

---

## CardData (`Assets/Scripts/Cards/CardData.cs`) — ScriptableObject

| 属性/字段 | 访问级别 | 类型 | 功能说明 | 核心逻辑 |
|---|---|---|---|---|
| `cardId` | public | string | 唯一标识 | Yes |
| `cardName` | public | string | 显示名称 | Yes |
| `cardType` | public | CardType | 卡牌类型 | Yes |
| `rarity` | public | CardRarity | 稀有度 | Yes |
| `icon` | public | Sprite | 图标 | — |
| `damage` | public | int | 伤害值 | Yes |
| `block` | public | int | 格挡值 | Yes |
| `heal` | public | int | 治疗值 | Yes |
| `focusGain` | public | int | Focus 增益 | Yes |
| `leftClickEffect` | public | CardEffectType | 左键效果类型 | Yes |
| `rightClickEffect` | public | CardEffectType | 右键效果类型 | Yes |
| `useTarget` | public | CardUseTarget | 使用目标 (Self/Enemy/Both) | Yes |
| `goodCost` | public | int | Good 消耗 | Yes |
| `evilCost` | public | int | Evil 消耗 | Yes |
| `finalValue` | public | float | CSV 导入数值 | No |
| `enabled` | public | bool | 是否启用 | Yes |
| `implemented` | public | bool | 是否已实现 | Yes |
| `IsOffensive` | public (computed) | bool | 是否攻击性卡（影响 Good/Evil 装填） | Yes |

---

## CardDatabase (`Assets/Scripts/Cards/CardDatabase.cs`) — ScriptableObject

| 函数名 | 访问级别 | 调用时机 | 功能说明 | 核心逻辑 |
|---|---|---|---|---|
| `OnEnable()` | private | Unity | 自动 Initialize | Yes |
| `Initialize()` | public | OnEnable/手动 | 构建 _cardById / _cardByName 字典 | Yes |
| `GetById(string)` | public | External | 按 cardId 查询 | Yes |
| `GetByName(string)` | public | External | 按 cardName 查询 | Yes |
| `GetByType(CardType)` | public | External | 按类型筛选 | Yes |
| `GetByRarity(CardRarity)` | public | External | 按稀有度筛选 | Yes |
| `GetByEffect(CardEffectType)` | public | External | 按效果筛选 | Yes |
| `GetRandomCard()` | public | External | 随机一张 | Yes |
| `GetRandomCards(int, bool)` | public | External | 随机 N 张（可重复/不重复） | Yes |
| `ValidateDatabase()` | public | Editor | 校验 null/空Id/重复/效果数值 | Yes |

---

## MagazineSystem (`Assets/Scripts/Magazine/MagazineSystem.cs`)

| 函数名 | 访问级别 | 调用时机 | 功能说明 | 核心逻辑 | 可能风险 |
|---|---|---|---|---|---|
| `Start()` | private | Start | 初始化 default loadout / BuildRandomMagazine | Yes | _hasUserLoadoutInit 屏蔽 fallback |
| `Update()` | private | Update | 换弹 timer | Yes | — |
| `HasUsableCurrentCard()` | public | Every use | 检查 !IsReloading + loadedCards>0 + index 有效 | Yes | 阻止 Reloading/Empty 时使用 |
| `GetCurrentCard()` | public | Every use | 返回当前槽位 CardData | Yes | 通过 HasUsableCurrentCard 检查 |
| `UseCurrentCardLeft()` | public | Input | 左键使用当前卡→调用 cardExecutor.ExecuteLeft | Yes | 返回 bool（是否成功） |
| `UseCurrentCardRight()` | public | Input | 右键使用当前卡→调用 cardExecutor.ExecuteRight | Yes | 返回 bool |
| `ManualReload()` | public | R键 | 手动换弹 | Yes | — |
| `StartReload()` | private | Internal | 开始换弹 timer | Yes | 暂停 loadedCards |
| `FinishReload()` | private | Internal | 完成换弹→BuildRandomMagazine | Yes | — |
| `BuildRandomMagazine()` | public | Reload/Init | Fisher-Yates 洗牌 + 随机装弹 | Yes | 从 loadoutCards 抽取 |
| `SetLoadoutCards(List<CardData>, bool)` | public | MagazineEditUI | 设置 Loadout 池 + 可选立即 rebuild | Yes | _hasUserLoadoutInit 标记 |
| `GetLoadoutCards()` | public | External | 返回当前 Loadout | Yes | — |
| `GetLoadedCards()` | public | External | 返回 loadedCards（当前弹夹） | Yes | — |
| `InitializeDefaultLoadoutIfEmpty(CardDatabase)` | public | MagazineEditUI | 首次默认8格 Loadout | Yes | 仅执行一次 |
| `ResolveSourcePool()` | private | Internal | loadoutCards → cardDatabase → initialCards 优先级 | Yes | _hasUserLoadoutInit=true 时 loadout 空→返回空不 fallback |
| `GetPreviewCards(int)` | public | UI | 获取下 N 发预览 | Yes | — |
| `Advance()` | private | Internal | index 前移 | Yes | — |

---

## InventorySystem (`Assets/Scripts/Inventory/InventorySystem.cs`)

| 函数名 | 访问级别 | 调用时机 | 功能说明 | 核心逻辑 |
|---|---|---|---|---|
| `InitializeForRun(CardDatabase)` | public | Player.Awake | 一次 Play 会话只初始化一次 | Yes |
| `GetOwnedTotalCount()` | public | UI | 返回 ownedCards.Count | Yes |
| `ResetToTestStock(CardDatabase)` | public | Internal | 每种 enabled 卡 20 发 | Yes |
| `AddCard(CardData)` | public | Reward/UI | 添加单张卡 | Yes |
| `AddCards(CardData, int)` | public | Internal | 批量添加 | Yes |
| `RemoveCard(CardData)` | public | UI | 移除首个匹配卡→返回 bool | Yes |
| `GetCardCounts()` | public | UI | 聚合返回 List<InventoryEntry> | Yes |
| `GetCount(CardData)` | public | UI | 按引用计数 | Yes |
| `HasCard(CardData)` | public | UI | 是否拥有 | Yes |
| `SetOwnedCardsFromCounts(Dictionary)` | public | MagazineEditUI.Apply | 从聚合字典写回 | Yes |
| `EnsureTestStockIfEmpty(CardDatabase)` | public | Internal | 库存为空时兜底 | Yes |

---

## MagazineEditUI (`Assets/Scripts/UI/MagazineEditUI.cs`)

| 函数名 | 访问级别 | 调用时机 | 功能说明 | 核心逻辑 |
|---|---|---|---|---|
| `Awake()` | private | Awake | 获取 PlayerController2D 引用 | Yes |
| `Start()` | private | Start | 创建 BagPanel UI | Yes |
| `Update()` | private | Update | B/Esc 键 Toggle | Yes |
| `Toggle()` | public | Input | 开关背包 | Yes |
| `Open()` | public | Toggle | 打开背包：初始化库存+Loadout+暂停时间 | Yes |
| `Close()` | public | Toggle/Esc | 关闭背包：恢复时间+解锁输入 | Yes |
| `SwitchTab(BagTab)` | public | Tab click | 切换5分页 | Yes |
| `Refresh()` | public | Open/Edit | 刷新当前 Tab 内容 | Yes |
| `RefreshOwnedCards()` | private | Refresh | 刷新左侧 Owned Cards 列表 | Yes |
| `RefreshLoadoutSlots()` | private | Refresh | 刷新右侧 Loadout 8格 | Yes |
| `OnOwnedCardClicked(CardData)` | private | Button | 点击 Owned Card→编辑层扣库存加 Loadout | Yes |
| `OnLoadoutSlotClicked(int)` | private | Button | 点击 Loadout Slot→编辑层移除返还库存 | Yes |
| `Apply()` | public | Button | 写回编辑层→SetLoadoutCards + SetOwnedCardsFromCounts | Yes |
| `CancelEdit()` | public | Button | 丢弃编辑层 | Yes |
| `ClearLoadout()` | public | Button | 清空编辑 Loadout | Yes |
| `AutoFill()` | public | Button | 从编辑库存随机补满8格 | Yes |
| `EnsureUI()` | private | Start | 完整重建 BagPanel UI 层级 | Yes |
| `FindCardDatabase()` | private | ResetToTestStock | 查找 CardDatabase | Yes |
| `CreateReadOnlyCardSlot(CardData)` | private | PreviewPage | 只读卡槽 | — |
| `CreateActionButton(string, Action)` | private | EnsureUI | 创建操作按钮 | — |

---

## CombatHUD (`Assets/Scripts/UI/CombatHUD.cs`)

| 函数名 | 访问级别 | 调用时机 | 功能说明 | 核心逻辑 |
|---|---|---|---|---|
| `Awake()` | private | Awake | 禁用旧占位 UI + 创建 CardwinHUDRoot | Yes |
| `Start()` | private | Start | BindSystems() + Combo text | Yes |
| `Update()` | private | Update | 每帧 RefreshHUD | Yes |
| `BindSystems()` | private | Start | 绑定 Player/MagazineSystem/PreviewUI | Yes |
| `RefreshHUD()` | private | Update | 刷新 HP/Shield/Focus/Combo 显示 | Yes |
| `RefreshReloadProgress()` | private | Update | 刷新 Reloading 进度 | Yes |
| `DisableLegacyPlaceholders()` | private | Awake | 禁用 HP_Text/MagazinePreview_Placeholder/State_Text | Yes |

---

## ComboRatingSystem (`Assets/Scripts/Combat/ComboRatingSystem.cs`)

| 函数名 | 访问级别 | 调用时机 | 功能说明 | 核心逻辑 |
|---|---|---|---|---|
| `RegisterCardUse(CardData, bool, bool)` | public | PlayerController2D | 注册使用卡牌→判断 combo | Yes |
| `ResetCombo(string)` | public | Internal | 清零 combo | Yes |

---

## MeleeEnemyController (`Assets/Scripts/Enemies/MeleeEnemyController.cs`)

| 函数名 | 访问级别 | 调用时机 | 功能说明 | 核心逻辑 |
|---|---|---|---|---|
| `Awake()` | private | Awake | 初始化 Kinematic RB + gravityScale=0 | Yes |
| `Start()` | private | Start | 记录初始X | Yes |
| `Update()` | private | Update | 状态机：Patrol/Chase/Attack/Return | Yes |
| `ChaseAndAttack()` | private | Update | 追击+攻击逻辑 | Yes |
| `Patrol()` | private | Update | 巡逻逻辑 | Yes |
| `TryDamagePlayer()` | private | Attack state | 攻击判定+扣血 | Yes |
| `FindPlayer()` | private | Internal | 查找 Player | Yes |
| `OnDrawGizmosSelected()` | private | Editor | 绘制攻击/索敌/巡逻范围 | No |

---

## RangedEnemyController (`Assets/Scripts/Enemies/RangedEnemyController.cs`)

| 函数名 | 访问级别 | 调用时机 | 功能说明 | 核心逻辑 |
|---|---|---|---|---|
| `Awake()` | private | Awake | 初始化 Kinematic RB + gravityScale=0 | Yes |
| `Start()` | private | Start | 记录初始X | Yes |
| `Update()` | private | Update | 巡逻 + 索敌判定 + 射击 | Yes |
| `HorizontalPatrol()` | private | Update | 水平悬浮巡逻 | Yes |
| `FireAtPlayer()` | private | Internal | prefab 发射 EnemyProjectile | Yes |
| `FindPlayer()` | private | Internal | 查找 Player | Yes |
| `OnDrawGizmosSelected()` | private | Editor | 绘制射击/巡逻范围 | No |

---

## EnemyProjectile (`Assets/Scripts/Enemies/EnemyProjectile.cs`)

| 函数名 | 访问级别 | 调用时机 | 功能说明 | 核心逻辑 |
|---|---|---|---|---|
| `Awake()` | private | Awake | 校验 prefab 完整性 | Yes |
| `Init(Vector2, int, float)` | public | RangedEnemy | 初始化方向/伤害/速度 | Yes |
| `Update()` | private | Update | 飞行+超时自毁 | Yes |
| `OnTriggerEnter2D(Collider2D)` | private | Unity | Trigger 命中 Player→扣血 / Ground→销毁 | Yes |
| `CheckManualHit()` | private | Update | Overlap 手动检测 | Yes |
| `HandleHit(GameObject, string)` | private | Internal | 统一命中：Player 扣血 + 销毁 | Yes |

---

## RewardManager (`Assets/Scripts/Combat/RewardManager.cs`)

| 函数名 | 访问级别 | 调用时机 | 功能说明 | 核心逻辑 |
|---|---|---|---|---|
| `Start()` | private | Start | 查找 CardDatabase / Inventory / Player / 订阅敌人 OnDeath | Yes |
| `OnEnemyKilled()` | private | Event Callback | 暂停→随机3张→显示 OnGUI | Yes |
| `OnGUI()` | private | Unity | 三选一按钮面板 | Yes |
| `SelectCard(CardData)` | private | Button click | 添加卡到 Inventory→恢复游戏 | Yes |

---

## PlayerAlignment (`Assets/Scripts/Combat/PlayerAlignment.cs`)

| 函数名 | 访问级别 | 调用时机 | 功能说明 | 核心逻辑 |
|---|---|---|---|---|
| `SetGood(int)` | public | External | 设置 Good 值 | Yes |
| `SetEvil(int)` | public | External | 设置 Evil 值 | Yes |
| `SetValues(int, int)` | public | External | 同时设置 Good/Evil | Yes |

---

## 编辑器和辅助脚本

| 脚本 | 函数名 | 访问级别 | 调用时机 | 功能说明 |
|---|---|---|---|---|
| CardwinSceneBuilder | `RebuildCleanDemoScene()` | public static | Editor Menu | 弹窗提示已禁用 |
| CardAssetCreator | `CreateBasicCards()` | public static | Editor Menu | 创建 Strike/Guard/Heal/Focus asset |
| CardDatabaseEditorUtility | `RebuildCardDatabase()` | public static | Editor Menu | 扫描Cards→创建CardDatabase |
| CardCsvImporter | `Import()` | public static | Editor Menu | 从 bullets.csv 批量导入卡牌 |
| CardLibraryWindow | `ShowWindow()` | public static | Editor Menu | 打开卡牌管理窗口 |
| CardConfigValidator | `Validate()` | public static | Editor Menu | 完整卡牌配置检查+报告 |
