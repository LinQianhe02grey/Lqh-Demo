# UE5 Reference Index for Cardwin Unity Demo

> 生成日期：2026-05-28
> 基于 UE5 Cardwin 项目：`C:\cardwin\Cardwin\`
> UE 版本：5.7

---

## 1. UE5 Project Summary

UE5 Cardwin 是一个 2D 横版动作卡牌游戏原型，核心机制如下：

- **2D 横版移动**：ACharacter 平台角色，支持水平移动、跳跃（含二段跳）、Dash 冲刺（含无敌帧）
- **弹夹卡牌系统**：玩家不直接打手牌，而是将卡牌作为"弹药"装入弹夹（8 发弹夹容量），左键发射（投射物命中敌人触发效果），右键自用（效果直接作用于玩家自身）
- **换弹机制**：弹夹打空后自动换弹（Fisher-Yates 洗牌），1.5 秒换弹时间
- **4 种基础卡牌效果**：Damage（伤害）、GainBlock（格挡）、Heal（治疗）、ApplyStatus.Focus（专注，叠加伤害加成）
- **卡牌数据**：通过 UDataTable（DT_Cards）定义，行结构为 FCardData，包含 CardId、DisplayName、Description、Cost、Effects[]、TargetTag 等
- **弹夹编辑**：UBulletEditComponent 管理 BulletPool（弹药池，最多 8 种）和 LoadedMagazine（当前弹夹），支持 CRUD 操作
- **背包系统**：UInventoryComponent 管理 24 格网格背包，支持堆叠、增删、交换
- **装备系统**：UEquipmentComponent 管理 Weapon / Armor / Accessory / Core 四个装备槽
- **AI 卡牌生成**：UBulletSynthesisComponent 通过外部 Python 进程（Stable Diffusion）根据敌人名称生成新卡牌图片和数据
- **敌人系统**：ADemoEnemyActor 支持近战 / 远程两种攻击模式，PatrolChase / KeepDistance 两种移动 AI，血量 / 格挡 / 受击 / 死亡
- **输入**：Enhanced Input System，绑定了 Move、Jump、Dash、PlayCard（左键）、UseSelfCard（右键）
- **UI**：UMG 蓝图 Widget（WBP_* 蓝图资源，未展开分析），涉及 HUD、弹夹预览、背包、弹夹编辑界面

---

## 2. UE5 File Scan Result

### 2.1 项目定义与编译文件

| 文件路径 | 类型 | 主要职责 | 与 Unity 迁移的关系 |
|----------|------|----------|---------------------|
| `Cardwin.uproject` | .uproject | UE5 项目定义，模块声明 | 参考项目结构 |
| `Source/Cardwin/Cardwin.Build.cs` | Build.cs | 模块依赖声明：EnhancedInput、Paper2D、Niagara、UMG、GameplayTags 等 | 确认 Unity 需用的 Package（Input System、2D Sprite、UI） |
| `Source/Cardwin/Cardwin.h` | .h | 模块头文件 | 无需迁移 |
| `Source/Cardwin/Cardwin.cpp` | .cpp | 模块实现 | 无需迁移 |
| `Source/Cardwin.Target.cs` | Target.cs | 构建目标 | 无需迁移 |
| `Source/CardwinEditor.Target.cs` | Target.cs | 编辑器构建目标 | 无需迁移 |

### 2.2 核心数据类型

| 文件路径 | 类型 | 主要职责 | 与 Unity 迁移的关系 |
|----------|------|----------|---------------------|
| `Public/CardGameTypes.h` | .h | 定义 ECardEffectOperation 枚举、FCardEffectData 结构体、FCardData 结构体 | **P0 核心**：CardData ScriptableObject 字段来源 |
| `Private/CardGameTypes.cpp` | .cpp | 类型构造/析构 | 迁移时参考字段初始化逻辑 |

### 2.3 玩家系统

| 文件路径 | 类型 | 主要职责 | 与 Unity 迁移的关系 |
|----------|------|----------|---------------------|
| `Public/CGDemoPlayerCharacter.h` | .h | 玩家角色：移动、跳跃、Dash、射击、自用卡牌、血量、格挡 | **P0 核心**：PlayerController2D 拆分参考 |
| `Private/CGDemoPlayerCharacter.cpp` | .cpp | 玩家全部行为实现 | 必须拆分为多个子系统，不允许堆砌到单个脚本 |

### 2.4 弹夹系统

| 文件路径 | 类型 | 主要职责 | 与 Unity 迁移的关系 |
|----------|------|----------|---------------------|
| `Public/BulletEditComponent.h` | .h | 弹夹编辑组件：BulletPool、LoadedMagazine、换弹、预览 | **P0 核心**：MagazineSystem 直接参考 |
| `Private/BulletEditComponent.cpp` | .cpp | 弹夹 CRUD、洗牌、换弹逻辑 | Fisher-Yates 洗牌 + 预览逻辑复用 |

### 2.5 投射物系统

| 文件路径 | 类型 | 主要职责 | 与 Unity 迁移的关系 |
|----------|------|----------|---------------------|
| `Public/CardProjectileActor.h` | .h | 卡牌投射物：携带 CardId，命中敌人后从 DataTable 查效果执行 | **P0 核心**：Projectile.cs 参考 |
| `Private/CardProjectileActor.cpp` | .cpp | 投射物初始化、碰撞、效果执行 | 命中判定 + 效果分发逻辑参考 |

### 2.6 背包与装备系统

| 文件路径 | 类型 | 主要职责 | 与 Unity 迁移的关系 |
|----------|------|----------|---------------------|
| `Public/InventoryTypes.h` | .h | EInventoryItemType、EEquipmentSlotType、FInventoryItemData、FInventorySlotData、FEquipmentSlotData 定义 | **P1**：InventorySystem 数据结构参考 |
| `Private/InventoryTypes.cpp` | .cpp | 结构体辅助方法 | 字段逻辑参考 |
| `Public/InventoryComponent.h` | .h | 背包管理：24 格、堆叠、增删交换 | **P1**：Inventory 系统参考 |
| `Private/InventoryComponent.cpp` | .cpp | 背包 CRUD、debug 物品添加 | 背包操作逻辑参考 |
| `Public/EquipmentComponent.h` | .h | 装备系统：Weapon/Armor/Accessory/Core 四槽 | **P1 暂缓**：后续阶段实现 |
| `Private/EquipmentComponent.cpp` | .cpp | 装备 CRUD | 暂缓迁移 |

### 2.7 AI 卡牌生成系统

| 文件路径 | 类型 | 主要职责 | 与 Unity 迁移的关系 |
|----------|------|----------|---------------------|
| `Public/BulletSynthesisComponent.h` | .h | AI 生成卡牌：Python 子进程调用 Stable Diffusion | **P2 后置**：Demo 阶段可选 |
| `Private/BulletSynthesisComponent.cpp` | .cpp | Python 调用、JSON 解析、纹理加载 | 暂缓迁移 |

### 2.8 敌人系统

| 文件路径 | 类型 | 主要职责 | 与 Unity 迁移的关系 |
|----------|------|----------|---------------------|
| `Public/DemoEnemyActor.h` | .h | 敌人基类：血量、格挡、攻击/移动模式切换、死亡奖励 | **P0 核心**：EnemyController 参考 |
| `Private/DemoEnemyActor.cpp` | .cpp | 敌人伤害接收、状态、AI Think 循环 | 受击/死亡逻辑复用 |
| `Public/EnemyAttackComponent.h` | .h | 攻击基类：攻击范围、冷却、目标检测 | **P0**：敌人攻击逻辑参考 |
| `Private/EnemyAttackComponent.cpp` | .cpp | 2D 距离判定、攻击冷却 | 2D 距离判定逻辑复用 |
| `Public/EnemyMeleeAttackComponent.h` | .h | 近战攻击：直接伤害 | **P0**：近战敌人 |
| `Private/EnemyMeleeAttackComponent.cpp` | .cpp | 近战执行 | 直接伤害调用逻辑 |
| `Public/EnemyRangedAttackComponent.h` | .h | 远程攻击：生成投射物 | **P0**：远程敌人 |
| `Private/EnemyRangedAttackComponent.cpp` | .cpp | 投射物生成 | 投射物生成逻辑 |
| `Public/EnemyProjectileActor.h` | .h | 敌人投射物：命中玩家 | **P0**：EnemyProjectile |
| `Private/EnemyProjectileActor.cpp` | .cpp | 投射物碰撞、伤害 | 命中玩家逻辑参考 |
| `Public/EnemyMoveComponent.h` | .h | 移动基类：X 轴移动、锁定 Y/Z | **P0**：敌人移动逻辑参考 |
| `Private/EnemyMoveComponent.cpp` | .cpp | 水平移动框架 | X 轴移动逻辑参考 |
| `Public/EnemyPatrolChaseMoveComponent.h` | .h | Patrol→Chase→ReturnHome 状态机 | **P0**：巡逻追击 AI |
| `Private/EnemyPatrolChaseMoveComponent.cpp` | .cpp | 巡逻/追击/回位 状态机逻辑 | 状态机逻辑复用 |
| `Public/EnemyKeepDistanceMoveComponent.h` | .h | Patrol→KeepDistance→ReturnHome 状态机 | **P1**：远程敌人保持距离 AI |
| `Private/EnemyKeepDistanceMoveComponent.cpp` | .cpp | 保持距离状态机逻辑 | 暂缓 |

### 2.9 配置文件

| 文件路径 | 类型 | 主要职责 | 与 Unity 迁移的关系 |
|----------|------|----------|---------------------|
| `Config/DefaultInput.ini` | .ini | EnhancedInput 配置 | Unity Input System 绑定参考 |
| `Config/DefaultEngine.ini` | .ini | GameInstance/GameMode/启动地图/渲染 | 场景/管理器结构参考 |
| `Config/DefaultGameplayTags.ini` | .ini | GameplayTag 注册：Target、Target.light、Target.Self | 标签系统参考 |
| `Config/Tags/Card.ini` | .ini | Card/Status 标签：Card.Attack/Skill/Effect.*、Status.Focus | 标签系统参考 |

---

## 3. UE5 System Map

| UE5 系统/类/蓝图/组件 | UE5 中的职责 | Unity 对应系统 | Unity 对应脚本 | 迁移优先级 | 当前状态 |
|------------------------|-------------|---------------|---------------|-----------|---------|
| Enhanced Input / UInputAction (Move/Jump/Dash/PlayCard/UseSelfCard) | 输入绑定和事件触发 | Input System | PlayerController2D.InputActions | P0 | 未建立 |
| ACGDemoPlayerCharacter — 移动/跳跃/二段跳 | 2D 平台移动（X 轴、Jump Z 700、二段跳） | Core / Combat | PlayerController2D (Movement) | P0 | 未建立 |
| ACGDemoPlayerCharacter — Dash | 冲刺（力度 1200、0.18s 持续、0.8s CD、无敌帧） | Combat | PlayerController2D (Dash) | P0 | 未建立 |
| ACGDemoPlayerCharacter — 血量/格挡/状态 | MaxHealth=50、CurrentBlock、PlayerStatuses (TMap<FGameplayTag,int32>) | Combat | Health.cs / StatusComponent.cs | P0 | 未建立 |
| ACGDemoPlayerCharacter — TryFireCurrentBullet / FireCardProjectileById | 发射当前子弹，生成投射物 | Cards / Magazine | PlayerController2D → MagazineSystem.Fire → Projectile.Spawn | P0 | 未建立 |
| ACGDemoPlayerCharacter — TryUseCurrentBulletOnSelf / ExecuteSelfCardEffect | 右键自用卡牌效果（Heal/GainBlock/ApplyStatus） | Cards | CardEffectExecutor (Self-target) | P0 | 未建立 |
| ACGDemoPlayerCharacter — GetPlayerFocusDamageBonus | 读取 Status.Focus 叠加值作为伤害加成 | Combat | StatusComponent / DamageCalculator | P1 | 未建立 |
| ACGDemoPlayerCharacter — RestartCurrentLevel | 死亡后重载当前关卡 | Core | GameManager | P0 | 未建立 |
| ACardProjectileActor | 携带 CardId 的投射物，命中敌人查 DataTable 执行效果 | Combat / Cards | Projectile.cs | P0 | 未建立 |
| ADemoEnemyActor | 敌人血量/受击/死亡/攻击模式/移动模式切换 | Combat | EnemyController.cs | P0 | 未建立 |
| ADemoEnemyActor — ApplyDamageToEnemy / HealEnemy / GainBlock | 敌人受击逻辑（格挡先吸收） | Combat | EnemyController.cs / Health.cs | P0 | 未建立 |
| ADemoEnemyActor — FindPlayerTarget | 查找玩家目标 | Combat | EnemyController.cs | P0 | 未建立 |
| ADemoEnemyActor — TryGenerateBulletSynthesisReward | AI 卡牌生成奖励 | Cards | 暂缓（P2） | P2 | 未建立 |
| UEnemyAttackComponent (基类) | 攻击基类：范围判定、冷却 | Combat | EnemyAttack.cs | P0 | 未建立 |
| UEnemyMeleeAttackComponent | 近战直接伤害 | Combat | EnemyMeleeAttack.cs | P0 | 未建立 |
| UEnemyRangedAttackComponent | 远程生成敌人投射物 | Combat | EnemyRangedAttack.cs | P1 | 未建立 |
| AEnemyProjectileActor | 敌人投射物，命中玩家 | Combat | EnemyProjectile.cs | P1 | 未建立 |
| UEnemyMoveComponent (基类) | 移动基类：X 轴水平移动 | Combat | EnemyMovement.cs | P0 | 未建立 |
| UEnemyPatrolChaseMoveComponent | Patrol→Chase→ReturnHome 状态机 | Combat | EnemyPatrolChaseAI.cs | P0 | 未建立 |
| UEnemyKeepDistanceMoveComponent | Patrol→KeepDistance→ReturnHome 状态机 | Combat | EnemyKeepDistanceAI.cs | P1 | 未建立 |
| DT_Cards (UDataTable) + FCardData | 卡牌数据表（CardId、DisplayName、Cost、Effects[]、TargetTag） | Cards | CardData (ScriptableObject) | P0 | 未建立 |
| FCardEffectData | 单个卡牌效果（Operation、Value、RepeatCount、EffectTag） | Cards | CardEffect (ScriptableObject sub-asset) | P0 | 未建立 |
| ECardEffectOperation | 效果操作枚举（Damage/Heal/GainBlock/ApplyStatus 等 11 种） | Cards | CardEffectType (enum) | P0 | 未建立 |
| TargetTag / Target.Self | 目标判定标签（self → 右键自用，不发射投射物） | Cards | CardData.targetType | P0 | 未建立 |
| UBulletEditComponent — BulletPool | 玩家编辑的弹药池（TArray<FName>） | Magazine | MagazineSystem.cs | P0 | 未建立 |
| UBulletEditComponent — LoadedMagazine / CurrentBulletIndex | 当前弹夹 + 当前子弹索引 | Magazine | MagazineSystem.cs | P0 | 未建立 |
| UBulletEditComponent — BuildShuffledLoadedMagazine (Fisher-Yates) | 洗牌后装入弹夹 | Magazine | MagazineSystem.cs | P0 | 未建立 |
| UBulletEditComponent — StartReload / FinishReload (1.5s Timer) | 换弹流程 | Magazine | MagazineSystem.cs | P0 | 未建立 |
| UBulletEditComponent — GetUpcomingBulletIds / GetUpcomingBulletPreviewTexts (Count=3) | 下 N 发预览 | Magazine | MagazineSystem.cs → MagazinePreviewUI | P0 | 未建立 |
| UBulletEditComponent — SetBulletPool / AddBulletCard / RemoveBulletAt / SwapBulletPoolSlots | 弹药池 CRUD | Magazine | MagazineSystem.cs → MagazineEditUI | P1 | 未建立 |
| UBulletEditComponent — OnBulletEditorChanged / OnBulletReloadStarted / OnBulletReloadFinished | 事件委托（UI 更新用） | Magazine / UI | UnityEvent | P0 | 未建立 |
| UInventoryComponent | 24 格网格背包、堆叠、CRUD | Inventory | InventorySystem.cs | P1 | 未建立 |
| FInventoryItemData / FInventorySlotData | 库存物品/槽位数据结构 | Inventory | InventoryItem.cs / InventorySlot.cs | P1 | 未建立 |
| EInventoryItemType | None / BulletCard / Equipment / Consumable / Material | Inventory | ItemType enum | P1 | 未建立 |
| UEquipmentComponent | Weapon/Armor/Accessory/Core 四装备槽 | Inventory | 暂缓（P1 后段） | P1 | 未建立 |
| UBulletSynthesisComponent | AI 卡牌生成（Python + Stable Diffusion） | Cards | 暂缓（P2） | P2 | 未建立 |
| WBP_BulletChainHUD（推测） | 战斗中最近 3 发预览 HUD | UI | MagazinePreviewUI.cs | P0 | 未建立 |
| WBP_BulletEditor（推测） | 弹夹编辑界面 | UI | MagazineEditUI.cs | P1 | 未建立 |
| WBP_BagInventory（推测） | 背包界面 | UI | InventoryUI.cs | P1 | 未建立 |
| HUD（推测） | 玩家血条、格挡值、弹药状态 | UI | CombatHUD.cs | P0 | 未建立 |
| Niagara FX（CardPlayEffect） | 卡牌播放粒子特效 | VFX | ParticleSystem / VFX Graph | P2 | 未建立 |
| GameplayTag 系统 | 标签匹配（Target.Self、Status.Focus 等） | Core | TagSystem.cs / string-based tags | P1 | 未建立 |

---

## 4. UE5 Function Index

### 4.1 玩家系统函数

| UE5 文件 | 函数名 | 函数职责 | 输入/输出 | Unity 对应类和函数 | Phase 1 迁移 |
|----------|--------|----------|-----------|-------------------|-------------|
| CGDemoPlayerCharacter.cpp | `Move(const FInputActionValue&)` | 水平移动 + 精灵翻转 | 输入：轴值 (float)；输出：AddMovementInput | PlayerController2D.Move() | **是** |
| CGDemoPlayerCharacter.cpp | `StartJumpInput()` | 开始跳跃输入 | 无 | PlayerController2D.OnJumpPressed() | **是** |
| CGDemoPlayerCharacter.cpp | `StopJumpInput()` | 停止跳跃输入 | 无 | PlayerController2D.OnJumpReleased() | **是** |
| CGDemoPlayerCharacter.cpp | `StartDash()` | 冲刺 + 无敌帧 + CD | 无 | PlayerController2D.StartDash() | **是** |
| CGDemoPlayerCharacter.cpp | `TryFireCurrentBullet()` | 发射当前子弹 | 查 BulletEditComponent → 取 CardId → 生成投射物 | PlayerController2D.Fire() → MagazineSystem.TryGetCurrentBullet() | **是** |
| CGDemoPlayerCharacter.cpp | `FireCardProjectileById(FName)` | 按 CardId 生成投射物 | 输入：CardId；输出：Spawn ACardProjectileActor | Projectile.Spawn(CardData) | **是** |
| CGDemoPlayerCharacter.cpp | `TryUseCurrentBulletOnSelf()` | 右键自用卡牌 | 检查 Target.Self → ExecuteSelfCardEffect() | PlayerController2D.UseSelfCard() | **是** |
| CGDemoPlayerCharacter.cpp | `ExecuteCardEffect(FName, int32, ADemoEnemyActor*)` | 对敌人执行卡牌效果 | 输入：CardId, RepeatCount, Target；输出：伤害/治疗等 | CardEffectExecutor.ExecuteOnEnemy() | **是** |
| CGDemoPlayerCharacter.cpp | `ExecuteSelfCardEffect(FName, int32)` | 对自身执行卡牌效果 | 输入：CardId, RepeatCount；输出：自身 Heal/GainBlock 等 | CardEffectExecutor.ExecuteOnSelf() | **是** |
| CGDemoPlayerCharacter.cpp | `IsSelfTargetCard(const FCardData&)` | 检查 TargetTag 是否包含 "self"/"Self"/"player" | 输入：FCardData；输出：bool | CardData.IsSelfTarget() | **是** |
| CGDemoPlayerCharacter.cpp | `ApplyDamageToPlayer(int32)` | 格挡先吸收，剩余伤害扣血；死亡触发 RestartCurrentLevel | 输入：damage；输出：修改 CurrentHealth | Health.TakeDamage(int) | **是** |
| CGDemoPlayerCharacter.cpp | `HealPlayer(int32)` | 治疗玩家 | 输入：amount；输出：增加 CurrentHealth，上限 MaxHealth | Health.Heal(int) | **是** |
| CGDemoPlayerCharacter.cpp | `GainBlock(int32)` | 增加格挡值 | 输入：amount；输出：CurrentBlock += amount | Health.GainBlock(int) | **是** |
| CGDemoPlayerCharacter.cpp | `ApplyStatusToPlayer(FGameplayTag, int32)` | 叠加状态层数（如 Focus） | 输入：Tag, Stacks | StatusComponent.ApplyStatus() | **是** |
| CGDemoPlayerCharacter.cpp | `GetPlayerFocusDamageBonus()` | 读取 Focus 层数作为伤害加成 | 输出：int32 bonus | StatusComponent.GetFocusBonus() | 否（P1） |
| CGDemoPlayerCharacter.cpp | `RestartCurrentLevel()` | 死亡重载关卡 | 无 | GameManager.RestartLevel() | **是** |
| CGDemoPlayerCharacter.cpp | `FindDemoEnemyInFront()` | 查找前方最近敌人（距离 800，半高 300） | 输出：ADemoEnemyActor* | EnemyController.FindNearestEnemy() | 否（P1） |

### 4.2 投射物系统函数

| UE5 文件 | 函数名 | 函数职责 | 输入/输出 | Unity 对应类和函数 | Phase 1 迁移 |
|----------|--------|----------|-----------|-------------------|-------------|
| CardProjectileActor.cpp | `InitProjectile(UDataTable*, FName, FVector)` | 初始化投射物方向、速度、视觉 | 输入：DataTable, CardId, Direction | Projectile.Init(CardData, Vector2) | **是** |
| CardProjectileActor.cpp | `OnProjectileOverlap()` | 碰撞检测，仅对 ADemoEnemyActor 响应 | 触发：Overlap 事件 | Projectile.OnTriggerEnter2D() | **是** |
| CardProjectileActor.cpp | `ApplyCardEffectsToTarget()` | 查 DataTable 获取 FCardData，遍历 Effects | 输入：Target Actor | Projectile.ApplyEffects() | **是** |
| CardProjectileActor.cpp | `ExecuteEffectOnTarget()` | switch ECardEffectOperation 执行具体效果 | 输入：Effect, Target | CardEffectExecutor.Execute() | **是** |

### 4.3 弹夹系统函数

| UE5 文件 | 函数名 | 函数职责 | 输入/输出 | Unity 对应类和函数 | Phase 1 迁移 |
|----------|--------|----------|-----------|-------------------|-------------|
| BulletEditComponent.cpp | `InitializeBulletEditor()` | 校验容量，构建初始弹夹 | 无 | MagazineSystem.Initialize() | **是** |
| BulletEditComponent.cpp | `BuildShuffledLoadedMagazine()` | Fisher-Yates 洗牌，加载 MagazineBulletCount 发 | 无 | MagazineSystem.ShuffleAndLoad() | **是** |
| BulletEditComponent.cpp | `TryGetCurrentBullet(FName& OutCardId)` | 获取当前子弹 CardId | 输出：CardId、bool 成功 | MagazineSystem.GetCurrentBullet() | **是** |
| BulletEditComponent.cpp | `ConsumeCurrentBulletAndMaybeReload()` | 索引++；弹夹打空则自动换弹 | 无 | MagazineSystem.ConsumeCurrent() | **是** |
| BulletEditComponent.cpp | `StartReload()` | 设置 1.5s 定时器，广播事件 | 无 | MagazineSystem.StartReload() | **是** |
| BulletEditComponent.cpp | `FinishReload()` | BuildShuffledLoadedMagazine() + 重置索引 | 无 | MagazineSystem.FinishReload() | **是** |
| BulletEditComponent.cpp | `GetUpcomingBulletIds(int32 Count)` | 获取接下来 N 发的 CardId 列表 | 输入：Count（默认 3）；输出：TArray<FName> | MagazineSystem.GetUpcomingBullets(int) | **是** |
| BulletEditComponent.cpp | `GetUpcomingBulletPreviewTexts(int32 Count)` | 格式化预览文本 "1. Strike \| Cost 1 \| Damage 5" | 输入：Count；输出：TArray<FText> | MagazineSystem.GetPreviewTexts(int) | **是** |
| BulletEditComponent.cpp | `SetBulletPool(TArray<FName>)` | 设置弹药池 | 输入：CardId 数组 | MagazineSystem.SetBulletPool(CardData[]) | **是** |
| BulletEditComponent.cpp | `AddBulletCard(FName)` | 向弹药池添加卡牌 | 输入：CardId | MagazineSystem.AddBullet(CardData) | 否（P1） |
| BulletEditComponent.cpp | `RemoveBulletAt(int32)` | 移除弹药池指定位置 | 输入：Index | MagazineSystem.RemoveBullet(int) | 否（P1） |
| BulletEditComponent.cpp | `SwapBulletPoolSlots(int32, int32)` | 交换弹药池两个位置 | 输入：IndexA, IndexB | MagazineSystem.SwapSlots(int, int) | 否（P1） |

### 4.4 敌人系统函数

| UE5 文件 | 函数名 | 函数职责 | 输入/输出 | Unity 对应类和函数 | Phase 1 迁移 |
|----------|--------|----------|-----------|-------------------|-------------|
| DemoEnemyActor.cpp | `ApplyDamageToEnemy(int32)` | 格挡先吸收，剩余扣血；触发 OnEnemyDamaged / OnEnemyDead | 输入：damage | EnemyController.TakeDamage(int) | **是** |
| DemoEnemyActor.cpp | `HealEnemy(int32)` | 治疗敌人 | 输入：amount | EnemyController.Heal(int) | **是** |
| DemoEnemyActor.cpp | `GainBlock(int32)` | 敌人增加格挡 | 输入：amount | EnemyController.GainBlock(int) | **是** |
| DemoEnemyActor.cpp | `IsDead()` | 判断是否死亡 | 输出：bool | EnemyController.IsDead() | **是** |
| DemoEnemyActor.cpp | `AttackThink()` | 攻击思考循环（每 0.25s），分发到对应攻击组件 | 无 | EnemyController.AttackUpdate() | **是** |
| DemoEnemyActor.cpp | `MoveThink()` | 移动思考循环（每 0.02s），分发到对应移动组件 | 无 | EnemyController.MoveUpdate() | **是** |
| EnemyAttackComponent.cpp | `TryAttack(AActor* Target)` | 范围+冷却判定 → ExecuteAttack() | 输入：Target；输出：bool 是否成功 | EnemyAttack.TryAttack(GameObject) | **是** |
| EnemyMeleeAttackComponent.cpp | `ExecuteAttack(AActor* Target)` | 直接对玩家造成伤害 | 输入：Target | EnemyMeleeAttack.Execute() | **是** |
| EnemyPatrolChaseMoveComponent.cpp | `UpdateMove(float DeltaTime)` | Patrol→Chase→ReturnHome 状态机 | 输入：DeltaTime | EnemyPatrolChaseAI.UpdateMove() | **是** |

### 4.5 背包系统函数

| UE5 文件 | 函数名 | 函数职责 | 输入/输出 | Unity 对应类和函数 | Phase 1 迁移 |
|----------|--------|----------|-----------|-------------------|-------------|
| InventoryComponent.cpp | `AddItem(FInventoryItemData, int32 Count, int32& OutRemainingCount)` | 添加物品（先堆叠，后空槽） | 输入：Item, Count；输出：剩余数量 | InventorySystem.AddItem() | 否（P1） |
| InventoryComponent.cpp | `RemoveItemById(FName, int32 Count)` | 按 Id 移除物品 | 输入：ItemId, Count | InventorySystem.RemoveItem() | 否（P1） |
| InventoryComponent.cpp | `SwapSlots(int32, int32)` | 交换两个格子 | 输入：SlotA, SlotB | InventorySystem.SwapSlots() | 否（P1） |
| InventoryComponent.cpp | `HasItem(FName)` | 检查是否拥有某物品 | 输入：ItemId；输出：bool | InventorySystem.HasItem() | 否（P1） |

---

## 5. Card Data Reference

### 5.1 FCardData 结构体（DT_Cards 行）

| UE5 字段名 | 字段含义 | Unity CardData 对应字段 | v0.1 必须保留 |
|-----------|----------|------------------------|--------------|
| `CardId` (FName) | 卡牌唯一标识（如 "Strike0", "Guard0"） | `string cardId` | **是** |
| `DisplayName` (FText) | 显示名称 | `string displayName` | **是** |
| `Description` (FText) | 卡牌描述文本 | `string description` | **是** |
| `Cost` (int32, 默认 1) | 费用 | `int cost` | 否（Demo 暂不需要费用） |
| `CardTags` (FGameplayTagContainer) | 卡牌标签（如 "Card.Attack", "Card.Skill"） | `List<string> tags` 或 `CardTag[]` | 否（P1） |
| `TargetTag` (FGameplayTag) | 目标标签（"Target.Self" 表示自用，"Target.light" 表示对敌） | `TargetType targetType` (enum: Self / Enemy) | **是** |
| `Effects` (TArray\<FCardEffectData\>) | 卡牌效果列表 | `List<CardEffect> effects` | **是** |

### 5.2 FCardEffectData 结构体

| UE5 字段名 | 字段含义 | Unity CardData 对应字段 | v0.1 必须保留 |
|-----------|----------|------------------------|--------------|
| `Operation` (ECardEffectOperation) | 效果操作类型 | `CardEffectType effectType` (enum) | **是** |
| `Value` (int32) | 效果数值 | `int value` | **是** |
| `RepeatCount` (int32, 默认 1) | 重复次数 | `int repeatCount` | **是** |
| `EffectTag` (FGameplayTag) | 效果标签 | `string effectTag` | 否（P1） |
| `StatusTag` (FGameplayTag) | 状态标签（ApplyStatus/RemoveStatus 时使用） | `string statusTag` | 否（P1） |
| `CueTag` (FGameplayTag) | CustomCue 标签 | `string cueTag` | 否 |
| `PayloadId` (FName) | 自定义 Payload 标识 | `string payloadId` | 否 |

### 5.3 ECardEffectOperation 枚举（v0.1 只需前 4 个）

| 枚举值 | 含义 | v0.1 实现 |
|--------|------|----------|
| `Damage` | 造成伤害 | **是** |
| `Heal` | 恢复生命 | **是** |
| `GainBlock` | 获得格挡 | **是** |
| `ApplyStatus` | 施加状态（Focus 等） | **是**（先实现 Focus） |
| `DrawCards` | 抽卡 | 否 |
| `DiscardCards` | 弃卡 | 否 |
| `RemoveStatus` | 移除状态 | 否 |
| `MoveUnit` | 移动单位 | 否 |
| `GainEnergy` | 获得能量 | 否 |
| `CreateCard` | 创建卡牌 | 否 |
| `CustomCue` | 自定义触发器 | 否 |

### 5.4 调试卡牌数据（来自 InventoryComponent::AddDefaultDebugItems）

| CardId | 效果 | 参数 | 数量 |
|--------|------|------|------|
| `Strike0` | Damage | Value=5 | 3 |
| `Guard0` | GainBlock | Value=5 | 3 |
| `Heal0` | Heal | Value=5 | 2 |
| `Focus0` | ApplyStatus.Status.Focus | Stacks=1 | 1 |

**Unity 迁移参考命名（仅保留为 UE5 数据映射参考）：**
- `CardData_Strike0.asset`
- `CardData_Guard0.asset`
- `CardData_Heal0.asset`
- `CardData_Focus0.asset`

**当前 Unity 项目实际基础卡牌资产名（Stage 8A.1c 同步口径）：**
- `Strike.asset`
- `Guard.asset`
- `Heal.asset`
- `Focus.asset`

不要在普通功能阶段重命名当前资产；如未来要统一迁移到 `CardData_<Name>.asset`，需单独开资产迁移阶段。

---

## 6. UI Reference

UE5 的 UI 实现在 `.uasset` 蓝图 Widget 中（WBP_*），C++ 代码中未展开具体 UI 布局。以下基于代码逻辑和命名推测 UI 对应关系：

### 6.1 战斗 HUD → CombatHUD

**UE5 来源：** `WBP_BulletChainHUD`（推测，由 BulletEditComponent 的预览函数驱动）

| 功能 | 描述 | Unity 实现 |
|------|------|-----------|
| 玩家血条 | CurrentHealth / MaxHealth | HealthBar.cs (Slider) |
| 格挡值显示 | CurrentBlock 数值 | BlockBar.cs (Text/Slider) |
| 弹药状态 | 剩余子弹数 / 弹夹大小 | AmmoText.cs |
| 换弹进度 | 1.5s 换弹计时 | ReloadBar.cs (Slider) |
| 状态效果图标 | Focus 层数等 | StatusIcon.cs |

### 6.2 最近 3 发预览 → MagazinePreviewUI

**UE5 来源：** `GetUpcomingBulletIds(3)` / `GetUpcomingBulletPreviewTexts(3)`

| 功能 | 描述 | Unity 实现 |
|------|------|-----------|
| 预览队列 | 显示下 3 发子弹的卡牌图标/名称 | MagazinePreviewUI.cs (3 个 Image/Text 槽位) |
| 当前子弹高亮 | 当前膛内子弹高亮显示 | CurrentBulletHighlight.cs |
| 格式文本 | "1. Strike \| Cost 1 \| Damage 5" | PreviewTextFormatter.cs |

### 6.3 背包 UI → InventoryUI

**UE5 来源：** `WBP_BagInventory`（推测，由 UInventoryComponent 驱动）

| 功能 | 描述 | Unity 实现 |
|------|------|-----------|
| 网格视图 | 24 格物品网格 | InventoryUI.cs (GridLayoutGroup) |
| 物品图标 | 卡牌/物品缩略图 | ItemSlot.cs (Image) |
| 数量显示 | 堆叠数量 | ItemSlot.cs (Text) |
| 拖拽交换 | 拖拽交换格子 | DragHandler.cs (IDragHandler) |
| 右键菜单 | 使用/丢弃/装备 | ContextMenu.cs |

### 6.4 弹夹编辑 UI → MagazineEditUI

**UE5 来源：** `WBP_BulletEditor`（推测，由 UBulletEditComponent 的 Set/Add/Remove/Swap 驱动）

| 功能 | 描述 | Unity 实现 |
|------|------|-----------|
| 弹药池列表 | 显示 BulletPool 中所有卡牌 | MagazineEditUI.cs (VerticalLayout) |
| 添加卡牌 | 从背包拖入或选择 | MagazineEditUI.AddBullet() |
| 移除卡牌 | 从弹药池移除 | MagazineEditUI.RemoveBullet() |
| 交换位置 | 拖拽调整弹药池顺序 | MagazineEditUI.SwapSlots() |
| 保存并重建弹夹 | 修改后 BuildShuffledLoadedMagazine() | MagazineEditUI.ApplyChanges() |

### 6.5 商店 UI → ShopUI

**UE5 状态：** UE5 项目中未见商店实现，为 Unity 后续扩展。

| 功能 | 描述 | Unity 实现 |
|------|------|-----------|
| 商品列表 | 展示可购买卡牌 | ShopUI.cs |
| 购买/出售 | 货币交易 | ShopUI.Buy() / ShopUI.Sell() |
| 刷新 | 刷新商品 | ShopUI.Refresh() |

---

## 7. Migration Priority

### P0 — 必须先做（Demo 核心可玩）

| 模块 | 内容 | 依赖 |
|------|------|------|
| Player Movement | 2D 水平移动、跳跃、二段跳、精灵翻转 | Input System |
| Player Dash | 冲刺、无敌帧、CD | Player Movement |
| Health System | 玩家和敌人血量、格挡、受击、死亡重载 | — |
| CardData (ScriptableObject) | 4 张基础卡牌数据资产（Strike0 / Guard0 / Heal0 / Focus0） | — |
| CardEffectType (enum) | Damage / Heal / GainBlock / ApplyStatus 枚举 + 执行逻辑 | CardData |
| Projectile | 卡牌投射物生成、飞行、命中敌人查效果 | CardData |
| Enemy Controller | 敌人血量、受击（格挡先吸收）、死亡 | Health System |
| Enemy AI (基础) | 巡逻追击移动 + 近战攻击 | Enemy Controller |
| MagazineSystem | 弹药池、弹夹、Fisher-Yates 洗牌、换弹、预览 | CardData |
| MagazinePreviewUI | 最近 3 发预览 | MagazineSystem |
| CombatHUD | 玩家血条、弹药状态 | Health System、MagazineSystem |

### P1 — Demo 核心扩展

| 模块 | 内容 |
|------|------|
| Status System (Focus) | Focus 叠加层数、伤害加成 |
| Enemy AI (远程) | KeepDistance 移动 + 远程投射物攻击 |
| InventorySystem | 24 格背包 CRUD |
| InventoryUI | 背包网格视图、物品显示 |
| MagazineEditUI | 弹药池编辑界面 |
| ShopSystem | 商店购买/出售/刷新 |
| ShopUI | 商店界面 |
| Equipment System | 装备槽管理 |

### P2 — 后置表现

| 模块 | 内容 |
|------|------|
| VFX | 卡牌播放粒子特效（投射物飞行、命中） |
| SFX | 音效 |
| BulletSynthesis | AI 卡牌生成（Python Stable Diffusion，Demo 可选） |
| Boss Polish | Boss 战斗打磨 |
| Analytics | 战斗数据统计 |
| Advanced Relics | 遗物/被动效果 |

---

## 8. Migration Rules

1. **不直接复制 UE5 API**：UE5 使用 C++/Blueprint/UMG/Paper2D/Niagara，Unity 使用 C#/MonoBehaviour/ScriptableObject/Canvas UI/SpriteRenderer。系统逻辑可参考，API 调用不可照搬。

2. **Unity 中使用原生方案重建**：
   - ACharacter → MonoBehaviour + Rigidbody2D
   - UDataTable + FTableRowBase → ScriptableObject 资产
   - UActorComponent → MonoBehaviour（挂载到 GameObject）
   - Enhanced Input → Unity Input System Package
   - UMG Widget → Canvas + UI 脚本
   - Paper2D Sprite → SpriteRenderer
   - Niagara → ParticleSystem / VFX Graph
   - GameplayTag → 字符串标签或 ScriptableObject 标签

3. **只参考 UE5 的系统边界、函数职责、数据字段和交互流程**：例如 BulletEditComponent 的函数签名和调用关系可直接参考，但实现时用 Unity C# 方式重写。

4. **每次实现 Unity 功能前，必须先阅读 `SYSTEM_INDEX.md` 和 `UE5_REFERENCE_INDEX.md`**。

5. **每次实现后必须更新 `SYSTEM_INDEX.md`、`UE5_REFERENCE_INDEX.md`（当前状态列）和 `DEVELOPMENT_LOG.md`**。
