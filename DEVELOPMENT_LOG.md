# DEVELOPMENT_LOG.md — 开发日志

---

## 日志模板

```
---
### YYYY-MM-DD | Stage X — 阶段名称
- **用户需求**：
- **修改文件**：
- **新增类**：
- **新增函数**：
- **Unity 挂载方式**：
- **测试步骤**：
- **已知问题**：
- **下一步**：
---
```

---

## 日志正文

---
### 2026-05-28 | Stage 0 — Project Governance
- **用户需求**：扫描 UE5 Cardwin 项目，生成 UE5_REFERENCE_INDEX.md；更新 AGENTS.md
- **修改文件**：
  - 新增 `UE5_REFERENCE_INDEX.md`
  - 修改 `AGENTS.md`（增加 UE5 参考规则）
  - 修改 `DEVELOPMENT_LOG.md`（本记录）
- **新增类**：无（文档阶段）
- **新增函数**：无（文档阶段）
- **Unity 挂载方式**：不适用
- **测试步骤**：
  1. 确认 AGENTS.md 包含 9 条强制规则（含 UE5 参考规则）
  2. 确认 UE5_REFERENCE_INDEX.md 包含 8 个章节
  3. 确认 SYSTEM_INDEX.md 8 个子系统表格均已列出
- **已知问题**：UE5 蓝图 Widget（WBP_*）未展开分析，UI 细节基于代码逻辑推测
- **下一步**：Stage 1 — Basic Code Structure（建立目录结构、PlayerController2D、CardData、MagazineSystem）
---

### 2026-05-29 | Stage 1 — Basic Code Structure + Stage 1.5 — Visual Graybox Scene
- **用户需求**：
  1. 创建基础代码结构（目录 + 骨架脚本 + 空函数，保证编译通过）
  2. 创建 Editor 菜单工具自动生成灰盒 Demo 场景
- **修改文件**：
  - 新增 `Assets/Scripts/Core/GameState.cs`
  - 新增 `Assets/Scripts/Core/GameManager.cs`
  - 新增 `Assets/Scripts/Core/GameStateMachine.cs`
  - 新增 `Assets/Scripts/Combat/PlayerController2D.cs`
  - 新增 `Assets/Scripts/Combat/Health.cs`
  - 新增 `Assets/Scripts/Combat/EnemyController.cs`
  - 新增 `Assets/Scripts/Combat/Projectile.cs`
  - 新增 `Assets/Scripts/Combat/DamageInfo.cs`
  - 新增 `Assets/Scripts/Cards/CardData.cs`
  - 新增 `Assets/Scripts/Cards/CardType.cs`
  - 新增 `Assets/Scripts/Cards/CardRarity.cs`
  - 新增 `Assets/Scripts/Cards/CardEffectType.cs`
  - 新增 `Assets/Scripts/Cards/CardRuntimeInstance.cs`
  - 新增 `Assets/Scripts/Cards/CardEffectExecutor.cs`
  - 新增 `Assets/Scripts/Cards/PlayerCardContext.cs`
  - 新增 `Assets/Scripts/Magazine/MagazineSystem.cs`
  - 新增 `Assets/Scripts/Magazine/MagazineSlot.cs`
  - 新增 `Assets/Scripts/Inventory/InventorySystem.cs`
  - 新增 `Assets/Scripts/Shop/ShopManager.cs`
  - 新增 `Assets/Scripts/Shop/EconomySystem.cs`
  - 新增 `Assets/Scripts/UI/CombatHUD.cs`
  - 新增 `Assets/Scripts/UI/MagazinePreviewUI.cs`
  - 新增 `Assets/Scripts/UI/CardSlotUI.cs`
  - 新增 `Assets/Scripts/UI/ShopUI.cs`
  - 新增 `Assets/Scripts/UI/InventoryUI.cs`
  - 新增 `Assets/Scripts/Analytics/BattleLogger.cs`
  - 新增 `Assets/Editor/Cardwin/CardwinSceneBuilder.cs`
  - 新增 `Assets/Art/Player/player_placeholder.png`
  - 修改 `SYSTEM_INDEX.md`（从"未建立"更新到"骨架完成"）
  - 修改 `DEVELOPMENT_LOG.md`（本记录）
  - 修改 `TODO.md`（Stage 1 + Stage 1.5 打勾）
- **新增类**：27 个类/枚举/结构体/接口
  - Core: GameState (enum), GameManager, GameStateMachine, IGameStateHandler (interface)
  - Combat: PlayerController2D, Health, EnemyController, Projectile, DamageInfo (struct)
  - Cards: CardData (ScriptableObject), TargetType (enum), CardEffectEntry (struct), CardType (enum), CardRarity (enum), CardEffectType (enum), CardRuntimeInstance, CardEffectExecutor, PlayerCardContext (ScriptableObject)
  - Magazine: MagazineSystem, MagazineSlot
  - Inventory: InventorySystem, InventorySlot
  - Shop: ShopManager, EconomySystem
  - UI: CombatHUD, MagazinePreviewUI, CardSlotUI, ShopUI, InventoryUI
  - Analytics: BattleLogger, BattleEntry (struct)
  - Editor: CardwinSceneBuilder
- **新增函数**：约 70+ 空函数骨架，详见 SYSTEM_INDEX.md
- **Unity 挂载方式**：
  - 所有 MonoBehaviour 脚本通过 `RequireComponent` 或 Inspector 手动挂载
  - CardData、PlayerCardContext 通过 `CreateAssetMenu` 菜单创建 ScriptableObject
  - 场景通过菜单 `Tools/Cardwin/Build Demo Scene` 自动生成
- **测试步骤**：
  1. 在 Unity 中打开项目，等待编译完成
  2. 检查 Console 窗口无红色编译错误
  3. 点击菜单 `Tools/Cardwin/Build Demo Scene`
  4. 确认 `Assets/Scenes/Demo_Combat.unity` 已生成
  5. 确认场景中有 Ground、3 个平台、Player（蓝色）、MainCamera、Canvas HUD、标记点
- **已知问题**：
  - 所有函数均为空实现，无实际玩法逻辑
  - PlayerController2D 中的 PlayerCardContext 未在脚本中 import（Cards 命名空间未显式使用）
  - 玩家精灵使用程序化生成的占位图
- **下一步**：Stage 2 — Player Movement（实现 Move/Jump/Dash 逻辑）
---

### 2026-05-29 | Stage 1 Fix — Compile Error Resolution
- **用户需求**：修复 3 个编译错误/警告，不新增玩法功能
- **修改文件**：
  - `Assets/Editor/Cardwin/CardwinSceneBuilder.cs`（3 处修复）
  - `Assets/Scripts/Combat/PlayerController2D.cs`（1 处修复）
- **新增类**：无
- **新增函数**：无
- **Unity 挂载方式**：不适用
- **测试步骤**：
  1. 在 Unity 中打开项目，等待编译完成
  2. 确认 Console 窗口无红色 Error
  3. 菜单栏应出现 Tools > Cardwin > Build Demo Scene
  4. 点击菜单生成 Demo_Combat.unity 场景
- **已知问题**：无（编译错误已全部修复）
- **下一步**：Stage 2 — Player Movement（实现 Move/Jump/Dash 逻辑）
---

### 2026-05-29 | Stage 1.5 Fix — Scene Save & Open Logic
- **用户需求**：修复场景生成后停留在 Untitled 的问题；Ground/Platforms 改用 SpriteRenderer；Player 添加 GroundCheck
- **修改文件**：
  - `Assets/Editor/Cardwin/CardwinSceneBuilder.cs`（4 处修复）
- **新增类**：无
- **新增函数**：无
- **Unity 挂载方式**：不适用
- **测试步骤**：
  1. Unity 中点击 Tools > Cardwin > Build Demo Scene
  2. 确认 Hierarchy 顶部显示 Demo_Combat（非 Untitled）
  3. 确认 Ground 和 Platforms 使用 SpriteRenderer + BoxCollider2D
  4. 确认 Player 下有 GroundCheck 子对象
  5. Console 无红色 Error
- **已知问题**：无
- **下一步**：Stage 2 — Player Movement
---

### 2026-05-29 | Stage 1.5 Fix — Arial.ttf & Sprite Tiling Warnings
- **用户需求**：修复 Arial.ttf 红错；清除 Sprite Tiling 警告
- **修改文件**：
  - `Assets/Editor/Cardwin/CardwinSceneBuilder.cs`（6 处修复）
- **新增类**：无
- **新增函数**：无
- **Unity 挂载方式**：不适用
- **测试步骤**：
  1. Unity 中点击 Tools > Cardwin > Build Demo Scene
  2. Console 无红色 Error（Arial.ttf 已修复）
  3. 无 Sprite Tiling 黄色 Warning（全部改用 Simple + localScale）
  4. 场景正常打开 Demo_Combat.unity
- **已知问题**：无
- **下一步**：Stage 2 — Player Movement
---

### 2026-05-29 | Stage 2 — Player Movement
- **用户需求**：实现左右移动、跳跃、二段跳、冲刺、冲刺无敌、精灵翻转
- **修改文件**：
  - `Assets/Scripts/Combat/PlayerController2D.cs`（完整重写：Move/Jump/Dash/FlipSprite/IsGrounded）
  - `Assets/Scripts/Combat/Health.cs`（新增 IsInvincible + 完整 TakeDamage/Heal/GainBlock/Die）
  - `Assets/Editor/Cardwin/CardwinSceneBuilder.cs`（新增 Ground Layer 创建/分配、Player gravityScale=3）
- **新增类**：无
- **新增函数**：
  - PlayerController2D: `Update()`, `FixedUpdate()`, `Move()`, `Jump()`, `StartDash()`, `IsGrounded()`, `FlipSprite()`
  - Health: `SetInvincible()`, `TakeDamage()`(格挡先吸收), `Heal()`(上限保护), `GainBlock()`, `Die()`
  - CardwinSceneBuilder: `EnsureGroundLayer()`, `GetGroundLayer()`
- **新增字段**：
  - PlayerController2D: `dashSpeed`, `groundCheck`, `groundCheckRadius`, `groundLayer`, `invincibleDuringDash`, `_spriteRenderer`, `_health`, `_facingRight`
  - Health: `IsInvincible` (property)
- **Unity 挂载方式**：PlayerController2D 的 `groundCheck` 拖入 GroundCheck 子物体；`groundLayer` 设为 Ground
- **测试步骤**：
  1. `Tools > Cardwin > Build Demo Scene` 重新生成场景
  2. Inspector 中把 Player 的 `groundCheck` 绑定 GroundCheck 子物体，`groundLayer` 设为 Ground
  3. Play：A/D 移动、Space 跳跃/二段跳、LeftShift 冲刺
  4. 冲刺期间 Health.IsInvincible=true
  5. 落地后跳跃次数重置
- **已知问题**：groundCheck 引用和 groundLayer 需手动在 Inspector 绑定（场景生成未自动关联）
- **下一步**：修复场景生成自动绑定 groundCheck 引用，或进入 Stage 3 — Card Effects
---

### 2026-05-29 | Stage 2 Fix — linearVelocity → velocity (Unity 2022)
- **用户需求**：修复 `Rigidbody2D.linearVelocity` 在 Unity 2022.3 不存在的编译错误
- **修改文件**：
  - `Assets/Scripts/Combat/PlayerController2D.cs`（4 处替换）
- **新增类**：无
- **新增函数**：无
- **Unity 挂载方式**：不适用
- **测试步骤**：Unity 打开项目，确认 Safe Mode 退出，Console 无红色 Error
- **已知问题**：无
- **下一步**：Stage 3 — Card Effects（创建卡牌数据资产 + 效果执行器实现）
---

### 2026-05-29 | Doc Update — Scene Lock & Working Rules
- **用户需求**：锁定 Demo_Combat.unity 为主场景，禁止自动重建；新增 Camera 子系统
- **修改文件**：
  - `AGENTS.md`（新增场景规则 10、Camera 子系统）
  - `SYSTEM_INDEX.md`（新增 Camera 系统、Scenes 章节，更新 Combat/Editor 条目）
- **新增类**：无
- **新增函数**：无
- **Unity 挂载方式**：不适用
- **测试步骤**：文档审查
- **已知问题**：无
- **下一步**：Stage 3 — Basic Combat Loop
---

### 2026-05-29 | Stage 3 — Basic Combat Loop + Stage 3.1 — Camera Follow
- **用户需求**：临时射击(鼠标方向)、子弹命中敌人扣血、敌人追逐+接触伤害、摄像机跟随
- **修改文件**：
  - `Assets/Scripts/Combat/Projectile.cs`（完整重写：Init/Update/OnTriggerEnter2D）
  - `Assets/Scripts/Combat/EnemyController.cs`（完整重写：追逐/接触伤害/Health组件）
  - `Assets/Scripts/Combat/PlayerController2D.cs`（新增 Shoot() 鼠标方向射击 + firePoint/projectilePrefab）
  - 新增 `Assets/Scripts/Camera/CameraFollow2D.cs`
  - 修改 `SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`、`TODO.md`
- **新增类**：`CameraFollow2D`
- **新增字段**：
  - PlayerController2D: `firePoint`, `projectilePrefab`, `testProjectileDamage`
  - Projectile: `damage`
  - EnemyController: `moveSpeed`, `contactDamage`, `attackCooldown`, `_rb`, `_health`, `_player`, `_attackTimer`
  - CameraFollow2D: `target`, `offset`, `smoothTime`, `useBounds`, `minBounds`, `maxBounds`
- **新增函数**：
  - PlayerController2D: `Shoot()`（鼠标方向发射子弹）
  - Projectile: `Awake()`, `Init()`, `Update()`, `OnTriggerEnter2D()` — 全部有实现
  - EnemyController: `Awake()`, `Start()`, `Update()`, `OnCollisionStay2D()` — 全部有实现
  - CameraFollow2D: `Awake()`, `LateUpdate()`, `FindTargetIfMissing()`
- **Unity 挂载方式**：
  - MainCamera: 添加 CameraFollow2D，target → Player，offset=(0,1.5,-10)，useBounds=true
  - Player: Tag 设为 Player；FirePoint 子物体(位置在前方约 0.6,0,0)；projectilePrefab 指向 Projectile 预制体
  - Enemy: 需要 SpriteRenderer + Rigidbody2D + BoxCollider2D + Health + EnemyController
  - Projectile Prefab: SpriteRenderer + Rigidbody2D + CircleCollider2D(isTrigger) + Projectile
- **测试步骤**：
  1. 手动创建 Enemy (设 Player tag)，创建 Projectile Prefab
  2. Player Inspector 绑定 firePoint(子物体) 和 projectilePrefab
  3. MainCamera 添加 CameraFollow2D，target → Player
  4. Play：AD移动/跳跃/冲刺正常；左键鼠标方向发射子弹；子弹命中敌人扣血；敌人接触玩家扣血；冲刺无敌不扣血；摄像机跟随
- **已知问题**：
  - 场景中暂无 Enemy 实例，需手动在 Hierarchy 创建
  - 场景中暂无 Projectile Prefab，需手动创建并拖入 PlayerController2D
  - 射击弹药无限，未接入 MagazineSystem（Stage 4 实现）
  - 敌人 AI 为简单追逐，无巡逻/状态机（后续扩展）
- **下一步**：Stage 4 — Magazine + CardData 接入射击系统
---

### 2026-05-29 | Stage 3 Fix — Namespace Collision: Cardwin.Camera vs UnityEngine.Camera
- **用户需求**：修复 Safe Mode 编译错误 CS0234 — `Camera.main` 被解析为 `Cardwin.Camera.main`
- **修改文件**：
  - `Assets/Scripts/Combat/PlayerController2D.cs`（`Camera.main` → `UnityEngine.Camera.main`）
  - `Assets/Scripts/Camera/CameraFollow2D.cs`（`namespace Cardwin.Camera` → `namespace Cardwin.Cameras`）
  - 更新 `SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`、`TODO.md`
- **新增类**：无
- **新增函数**：无
- **Unity 挂载方式**：不适用
- **测试步骤**：
  1. Unity 打开项目，等待编译完成
  2. Console 无红色 Error，Safe Mode 自动退出
  3. Demo_Combat.unity 正常打开
  4. Play：Player 可移动/跳跃/射击，摄像机跟随正常
  5. 摄像机跟随脚本（Cardwin.Cameras.CameraFollow2D）可正常挂载到 MainCamera
- **已知问题**：无（编译错误已修复）
- **下一步**：Stage 4 — Magazine + CardData 接入射击系统
---

### 2026-05-29 | Stage 3.2 — Scene Wiring / Camera / Collision Fix
- **用户需求**：修复 Demo_Combat 场景中摄像机跟随、Player 跳跃检测、Enemy 碰撞等问题，不允许重建场景
- **修改文件**：
  - `Assets/Scripts/Camera/CameraFollow2D.cs`（`useBounds` 默认值 `true` → `false`）
  - `Assets/Scripts/Combat/PlayerController2D.cs`（Awake 增加 freezeRotation + auto-find GroundCheck；IsGrounded 增加 layerMask fallback；新增 `FindGroundCheckIfMissing()`、`OnDrawGizmosSelected()`）
  - `Assets/Scripts/Combat/EnemyController.cs`（bodyType → Kinematic；velocity → MovePosition；新增 `OnTriggerStay2D()`、`TryDamagePlayer()` 统一冷却伤害逻辑）
  - 更新 `SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`、`TODO.md`
- **新增类**：无
- **新增函数**：
  - PlayerController2D: `FindGroundCheckIfMissing()`, `OnDrawGizmosSelected()`
  - EnemyController: `OnTriggerStay2D()`, `TryDamagePlayer()`
- **新增字段**：
  - PlayerController2D: `_warnedMissingGroundCheck`, `_warnedUnsetLayer`
- **Unity 挂载方式**：
  - **MainCamera**：挂载 CameraFollow2D（`Cardwin.Cameras`），target 留空/指向 Player，Projection=Orthographic，Size≈6，useBounds=false
  - **Player**：Layer=Player，Rigidbody2D(Dynamic, freezeRotation=true, gravityScale=3)，子物体 GroundCheck(位置脚底 Y≈-0.85)，PlayerController2D.groundLayer=Ground
  - **Ground/Platform**：Layer=Ground，BoxCollider2D，无需 Rigidbody2D
  - **Enemy**：Layer=Enemy，Rigidbody2D(Kinematic, freezeRotation=true)，BoxCollider2D(推荐 IsTrigger=true 避免阻挡Player)，EnemyController.attackCooldown=1s
- **测试步骤**：
  1. 打开 `Assets/Scenes/Demo_Combat.unity`
  2. Play：A/D 移动，确认 MainCamera 平滑跟随
  3. Space 跳跃 / 空中二段跳，确认落地检测正常
  4. 靠近 Enemy，确认不会被弹飞/卡住/旋转
  5. 接触 Enemy 时按冷却扣血，Health 数字下降（可查看 Inspector 中 Health 组件）
  6. 按 LeftShift 冲刺，确认无敌期间不扣血
- **已知问题**：
  - groundLayer 必须手动在 Inspector 设为 Ground（否则使用 fallback 忽略 Player 层）
  - Enemy 的 Kinematic MovePosition 在复杂地形可能穿透薄地面（当前灰盒地图无此问题）
  - 场景中 Enemy 需手动设置 BoxCollider2D.isTrigger=true 以完全消除物理碰撞
- **下一步**：Stage 4 — Magazine + CardData 接入射击系统

---

### 2026-05-29 | Stage 3.3 — Level Collision Rework
- **用户需求**：修复空气墙阻挡跳跃、Player踩Enemy头、碰撞职责不清；场景对象 Layer/Collider 重新分类
- **修改文件**：
  - `Assets/Scripts/Combat/PlayerController2D.cs`（`IsGrounded()` 移除 layer fallback，只检测 Ground layer；groundLayer 未设置则 warn + 禁用地面检测）
  - `Assets/Scripts/Combat/EnemyController.cs`（移除 `OnCollisionStay2D`，仅保留 `OnTriggerStay2D` — 纯 Trigger 接触伤害方案）
  - 新增 `Assets/Scripts/Combat/SceneCollisionReporter.cs`（运行时 Debug 脚本，F1 输出场景所有 Collider 信息）
  - 更新 `SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`、`TODO.md`
- **新增类**：`SceneCollisionReporter`
- **新增函数**：
  - SceneCollisionReporter: `Start()`, `Update()`, `ReportSceneColliders()`
- **新增字段**：SceneCollisionReporter: `reportOnStart`, `reportKey`
- **Unity 挂载方式**：
  - SceneCollisionReporter 挂载到任意场景对象（建议 Canvas 或空 DebugHolder），Play 自动输出或按 F1
- **场景对象 Layer 规则**：
  - **Ground / Platform_1~3**：Layer=Ground，BoxCollider2D（Solid, IsTrigger=false），无 Rigidbody2D
  - **Player**：Layer=Player，Rigidbody2D(Dynamic, freezeRotation=true, gravityScale=3)，CapsuleCollider2D(Solid)，groundLayer=Ground
  - **Enemy**：Layer=Enemy，Rigidbody2D(Kinematic, freezeRotation=true)，BoxCollider2D(**IsTrigger=true**)
  - **CameraBounds**：删除/禁用 BoxCollider2D（仅作相机边界参考，不阻挡 Player）
  - **SpawnPoint_Player / SpawnPoint_Enemy**：删除任何 Collider（仅作 Transform 标记点）
  - **BossDoor_Placeholder**：BoxCollider2D 设 IsTrigger=true（当前阶段不阻挡 Player）
- **测试步骤**：
  1. 打开 `Assets/Scenes/Demo_Combat.unity`
  2. 按以上规则手动设置每个对象的 Layer 和 Collider
  3. 挂载 SceneCollisionReporter 到 Canvas，Play 查看 Console 输出确认 Collider 配置
  4. Play：A/D 移动 + Space 跳跃 + 二段跳，确认不被空气墙挡住
  5. Player 走到 Enemy 位置，确认穿过去（不踩头、不卡住）
  6. 接触 Enemy 时 Health 按冷却扣血
  7. LeftShift 冲刺无敌期间接触 Enemy 不扣血
- **已知问题**：
  - 所有场景对象 Layer/Collider 需手动在 Inspector 设置（脚本无法修改 .unity 文件）
  - Enemy 的 BoxCollider2D 必须手动设 IsTrigger=true，否则无接触伤害且 Player 会碰上固体碰撞
  - 无空气墙后 Player 可能走出地图边界（后续由 CameraBounds + useBounds=true 限制）
- **下一步**：Stage 4 — Magazine + CardData 接入射击系统

---

### 2026-05-29 | Stage 3.4 — Runtime Scene Wiring Fix
- **用户需求**：之前修复未真正作用到 Demo_Combat.unity 场景对象；摄像机不跟随、空气墙、踩敌人头问题仍存在。创建运行时修复脚本自动配置。
- **修改文件**：
  - 新增 `Assets/Scripts/Core/DemoSceneRuntimeBootstrapper.cs`（运行时场景自动配置脚本，`DefaultExecutionOrder(-1000)` 最优先执行）
  - 更新 `SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`、`TODO.md`
- **新增类**：`DemoSceneRuntimeBootstrapper`
- **新增函数**：
  - `Awake()` — 统一入口，调用所有配置子函数
  - `ResolveLayers()` — 解析 Player/Enemy/Ground Layer 索引，缺失则报错
  - `FindCoreObjects()` — 按名称查找 Player 和 MainCamera
  - `ConfigureCamera()` — 绑定 CameraFollow2D.target=Player，offset=(0,1.5,-10)，smoothTime=0.15，关闭 useBounds，强制 z=-10
  - `ConfigurePlayer()` — Tag=Player，Layer=Player，Rigidbody2D(Dynamic,gravity=3,freezeRotation)，CapsuleCollider(Solid)，自动创建 GroundCheck 子物体，groundLayer=Ground
  - `ConfigureGroundAndPlatforms()` — 遍历所有名称含 Ground/Platform 的对象，Layer=Ground，BoxCollider2D(Solid)，移除 Rigidbody2D
  - `ConfigureEnemy()` — 遍历所有名称含 Enemy 的对象，Layer=Enemy，Collider→Trigger，Rigidbody→Kinematic
  - `DisableBlockingPlaceholders()` — CameraBounds/SpawnPoint 禁用 Collider，BossDoor 设为 Trigger
  - `IgnorePlayerEnemyCollision()` — `Physics2D.IgnoreLayerCollision(Player, Enemy, true)`
  - `PrintColliderReport()` — 输出场景所有 Collider 清单
- **Unity 挂载方式**：
  - 在 Demo_Combat.unity 中创建空物体 `SceneRuntime`
  - 挂载 `DemoSceneRuntimeBootstrapper` 脚本
  - 所有 public 字段保留默认值即可
  - `[DefaultExecutionOrder(-1000)]` 确保它先于所有其他脚本执行
- **测试步骤**：
  1. Demo_Combat.unity → 创建空物体 `SceneRuntime` → 挂载 `DemoSceneRuntimeBootstrapper`
  2. Play → Console 输出 6 条配置日志 + Collider 报告
  3. 确认 `[SceneBootstrapper] CameraFollow2D target assigned: Player`
  4. A/D 移动 → MainCamera 平滑跟随
  5. Space 跳跃/二段跳 → 不被空气墙挡住
  6. Player 走到 Enemy → 穿过不踩头，不卡住
  7. 接触 Enemy 时 Health 按冷却扣血
  8. LeftShift 冲刺无敌期间不扣血
  9. Console 无红色 Error
- **已知问题**：
  - Layer 'Player'/'Enemy'/'Ground' 必须在 Project Settings 中预先创建（SceneBuilder 已创建）
  - 脚本不创建或修改 `.unity` 文件，每次 Play 都需要 SceneRuntime 挂载
  - 无空气墙后 Player 可能走出地图边界
- **下一步**：Stage 4 — Magazine + CardData 接入射击系统

---

### 2026-05-29 | Stage 3.5 — Rebuild Clean Demo_Combat Scene
- **用户需求**：删除旧场景后重建干净 Demo_Combat，严格按新 Layer/Collider/Camera/Player/Enemy 规则生成
- **修改文件**：
  - `Assets/Editor/Cardwin/CardwinSceneBuilder.cs`（完全重写：`RebuildCleanDemoScene()` 替代旧 `BuildDemoScene()`）
  - 更新 `AGENTS.md`、`SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`、`TODO.md`
- **新增类**：无
- **新增函数**：
  - `RebuildCleanDemoScene()` — 新菜单入口，含 Play Mode 检查
  - `EnsureAllLayers()` / `EnsureLayer()` — 自动创建 Ground/Player/Enemy/Trigger 4 层
  - `ResolveLayers()` — 缓存 Layer 索引
  - `CreateEnemy()` — 创建 Enemy_Test（红色）+ Health + EnemyController + Kinematic + Trigger
  - `CreateSpawnPoints()` — 创建 SpawnPoint_Player/Enemy（半透明标记，无 Collider）
  - `CreateBossDoor()` — 创建 BossDoor_Placeholder（Layer=Trigger, IsTrigger=true）
  - `CreateProjectilePrefab()` — 在 Assets/Prefabs/Projectiles/ 创建 Projectile_Test.prefab
  - `ValidateRebuild()` — 菜单验证：Play Mode 时禁用菜单
- **新增项目**：
  - `Assets/Prefabs/Projectiles/Projectile_Test.prefab`（黄色小方块，Kinematic, IsTrigger, Projectile 组件）
- **场景对象 Layer 规则（最终版）**：
  - **Ground / Platform_1/2/3**：Layer=Ground，BoxCollider2D(Solid)，无 Rigidbody2D
  - **Player**：Layer=Player，Dynamic Rigidbody2D(freezeRotation)，CapsuleCollider2D(Solid)，groundLayer=Ground，GroundCheck + FirePoint 子物体
  - **Enemy_Test**：Layer=Enemy，Kinematic Rigidbody2D，BoxCollider2D(IsTrigger=true)
  - **CameraBounds**：无 Collider（仅半透明黄色可视化参考）
  - **SpawnPoint_Player / SpawnPoint_Enemy**：无 Collider（半透绿/红标记）
  - **BossDoor_Placeholder**：Layer=Trigger，BoxCollider2D(IsTrigger=true)
  - **MainCamera**：CameraFollow2D 已挂载，target 留空（运行时自动查找 Player），useBounds=false
- **测试步骤**：
  1. Unity 中点击 `Tools > Cardwin > Rebuild Clean Demo Scene`
  2. 确认 Console 无红色 Error，输出 `[Cardwin] Clean Demo_Combat scene rebuilt successfully.`
  3. Hierarchy 显示 Demo_Combat 场景，含所有对象
  4. Play：MainCamera 跟随 Player，A/D 移动/跳跃/二段跳/冲刺正常
  5. Player 不被空气墙挡住，只在 Ground/Platform 上站立
  6. Enemy 为 Trigger，Player 穿过不踩头，接触时按冷却扣血
  7. SpawnPoints/CameraBounds/BossDoor 不阻挡 Player
- **已知问题**：
  - CameraFollow2D.target 需要运行时通过 Tag "Player" 自动查找（或手动在 Inspector 拖入）
  - Projectile_Test.prefab 已创建，但 Player 的 projectilePrefab 需在 Play 后自动绑定或手动验证
  - 场景边界无限制（Player 可走出地图），后续需要 CameraBounds + useBounds=true
  - 不可反复运行此工具覆盖场景（仅在明确要求时使用）
- **下一步**：Stage 4 — Basic Combat Loop

---

### 2026-05-29 | Stage 4 — Basic Combat Loop
- **用户需求**：实现鼠标方向发射子弹，子弹命中 Enemy 扣血，Enemy HP<=0 死亡销毁
- **修改文件**：
  - `Assets/Scripts/Combat/Projectile.cs`（`_rb.velocity` → `_rb.MovePosition` 兼容 Kinematic 预制体）
  - `Assets/Scripts/Combat/Health.cs`（`Die()` 添加 `Destroy(gameObject, 0.1f)`）
  - 更新 `AGENTS.md`（锁定场景规则）、`SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`、`TODO.md`
- **新增类**：无
- **新增函数**：无
- **新增字段**：无
- **Unity 挂载方式**：不适用（脚本逻辑修复，无需新挂载）
- **测试步骤**：
  1. 打开 `Assets/Scenes/Demo_Combat.unity`（确认场景由 SceneBuilder 生成）
  2. Play：A/D 移动 + Space 跳跃 + Shift 冲刺正常
  3. 鼠标放在 Player 右侧，左键 → 子弹向右飞行
  4. 鼠标放在 Player 左侧，左键 → 子弹向左飞行
  5. 鼠标放在斜上方，左键 → 子弹斜上飞行
  6. 子弹命中 Enemy_Test（红色方块）→ Enemy Health 减少
  7. 连续射击 3 次（Enemy HP=30，每次10）→ Enemy 消失
  8. Enemy 靠近 Player 时按冷却扣血
  9. Console 无红色 Error
- **已知问题**：
  - 射击弹药无限（未接入 MagazineSystem）
  - Player 死亡也会 Destroy（后续需改为重载关卡）
  - Projectile 是 Kinematic，MovePosition 在复杂碰撞场景可能需要调整
- **下一步**：Stage 5 — Magazine System

---

### 2026-05-29 | Stage 4.1 — Projectile Visibility Fix
- **用户需求**：修复测试子弹不可见问题 — 子弹发射后看不到
- **修改文件**：
  - `Assets/Scripts/Combat/Projectile.cs`（Awake 强制 sortingOrder=20/color=Yellow/scale=0.35；transform.position 移动替代 MovePosition；忽略 Player 和 Projectile 命中；添加 Init 和 Hit Debug）
  - `Assets/Scripts/Combat/PlayerController2D.cs`（Shoot 新增 FirePoint fallback、spawn 偏移 0.3f、Debug.Log+Debug.DrawRay、scale 强制设置）
  - `Assets/Editor/Cardwin/CardwinSceneBuilder.cs`（新增 `Tools/Cardwin/Fix Projectile Prefab` 菜单、`CreateProjectileSpriteAsset()` 生成黄色圆点 PNG、`UpdateProjectilePrefab()` 修复 prefab 属性）
  - 更新 `SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`、`TODO.md`
- **新增类**：无
- **新增函数**：
  - Projectile: `Awake()`（强制可见性设置）
  - CardwinSceneBuilder: `FixProjectilePrefab()`, `CreateProjectileSpriteAsset()`, `UpdateProjectilePrefab()`
- **新增资源**：`Assets/Art/Projectiles/projectile_test.png`（黄色圆点 32x32）
- **Unity 挂载方式**：
  - 打开项目后执行一次 `Tools > Cardwin > Fix Projectile Prefab` 修复 prefab 的 sprite 和 sortingOrder
  - PlayerController2D.projectilePrefab 确认指向 Projectile_Test.prefab
  - FirePoint 子物体确认存在于 Player 下（SceneBuilder 已自动创建）
- **测试步骤**：
  1. 执行 `Tools > Cardwin > Fix Projectile Prefab`（如果 prefab 不存在，先执行 Rebuild）
  2. Play → 左键点击 → Console 输出 `[PlayerShoot] Fire projectile. Direction=(x,y)`
  3. Console 输出 `[Projectile] Init direction=(x,y), damage=10`
  4. Game 视图可见黄色子弹从 Player 身边飞出
  5. Scene 视图可见黄色 Debug.DrawRay 发射线
  6. 子弹命中 Enemy → Console 输出 `[Projectile] Hit Enemy_Test`
  7. Enemy HP 减少
- **已知问题**：
  - 如果仍看不到子弹，检查 Console 的 Debug 输出确认发射和初始化
  - 如果 Projectile Awake 的 sortingOrder 设置与 prefab 冲突，以 Awake 为准
- **下一步**：Stage 5 — Magazine System

---

### 2026-05-29 | Stage 4.2 — Projectile Visual + Hit Filter
- **用户需求**：子弹在 Game 视图肉眼可见；子弹不命中 BossDoor/SpawnPoint/CameraBounds 等非战斗对象
- **修改文件**：
  - `Assets/Scripts/Combat/Projectile.cs`（Awake: sortingOrder=50/scale=0.5；speed=8/lifetime=3 便于观察；OnTriggerEnter2D: 过滤 Player/Projectile/BossDoor/SpawnPoint/CameraBounds/Trigger layer；仅 Health 目标扣血 + Ground 自毁）
  - `Assets/Scripts/Combat/PlayerController2D.Shoot()`（spawn 偏移 0.2f(firePoint)/0.7f(body)；force scale=0.5）
  - 更新 `SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`、`TODO.md`
- **新增类**：无
- **Unity 挂载方式**：不适用
- **测试步骤**：
  1. Play → 左键 → 可见黄色子弹从 Player 飞出
  2. 鼠标不同方向 → 子弹朝对应方向飞行
  3. 子弹命中 Enemy_Test → Console: `[Projectile] Hit damage target: Enemy_Test` → Enemy 扣血
  4. 子弹命中 Ground → Console: `[Projectile] Hit ground: Ground` → 子弹消失
  5. 子弹穿过 BossDoor → 无日志，子弹继续飞行
- **已知问题**：
  - 子弹速度 8f 为调试值，正式版可调回 12f
  - BossDoor/SpawnPoint/CameraBounds 过滤基于名称匹配，若改名需同步更新
- **下一步**：Stage 5 — Magazine System

---

### 2026-05-29 | Stage 4.3 — Force Runtime Projectile Visual
- **用户需求**：Console 已证明子弹生成/Init/命中正常，但 Game 视图不可见。需要在 Projectile.cs 运行时强制创建可见 sprite。
- **修改文件**：
  - `Assets/Scripts/Combat/Projectile.cs`（Awake 调用 `EnsureVisibleDebugSprite()` — 无 SpriteRenderer 则 AddComponent，无 sprite 则 `CreateRuntimeSprite()` 生成 32x32 黄色圆点；sortingOrder=100, scale=0.8, speed=4/lifetime=5）
  - `Assets/Scripts/Combat/PlayerController2D.Shoot()`（新增实例 Debug 日志 + force scale=0.8）
  - 更新 `SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`、`TODO.md`
- **新增函数**：
  - `EnsureVisibleDebugSprite()` — 确保 SpriteRenderer 存在 + sprite 存在 + sortingOrder=100/scale=0.8/z=0
  - `CreateRuntimeSprite()` — 运行时生成 32x32 黄色圆点 Texture2D → Sprite.Create
- **Unity 挂载方式**：不适用（运行时自动修复）
- **测试步骤**：
  1. Play → 左键 → Console 输出 `[ProjectileVisual] SpriteRenderer ready. spriteNull=False, scale=(0.8,0.8,1), sorting=100`
  2. Console 输出 `[PlayerShoot] Spawned projectile instance=Projectile_Test(Clone), active=True, pos=(...), scale=(0.8,0.8,1)`
  3. Hierarchy 展开可看到 `Projectile_Test(Clone)` 实例
  4. Game 视图可见明显黄色子弹从 Player 飞出（scale=0.8, sortingOrder=100 在最前层）
  5. 子弹速度 4f，肉眼方便跟踪全程
- **已知问题**：
  - CreateRuntimeSprite 每次生成新 Texture2D，大量发射时内存增长（测试阶段可接受）
  - 速度 4f 为调试值，正式阶段调回 10~12
- **下一步**：Stage 5 — CardData + CardEffectExecutor

---

### 2026-05-29 | Stage 5 — CardData + CardEffectExecutor
- **用户需求**：将"临时普通射击"升级为 CardData 驱动的卡牌效果系统，实现 Damage/Block/Heal/Focus 四种效果，创建 4 张基础卡牌
- **修改文件**：
  - `Assets/Scripts/Cards/CardData.cs`（重写：flat 字段，移除 CardEffectEntry/TargetType 旧结构）
  - `Assets/Scripts/Cards/CardType.cs`（Attack/Skill/Power → Attack/Defense/Heal/Utility）
  - `Assets/Scripts/Cards/CardRarity.cs`（Common/Uncommon/Rare/Legendary → Common/Rare/Epic）
  - `Assets/Scripts/Cards/CardEffectType.cs`（11种 → None/Damage/Block/Heal/Focus 5种）
  - `Assets/Scripts/Cards/PlayerCardContext.cs`（ScriptableObject → 普通运行时类，AddFocus/ConsumeFocusMultiplier/GetShootDirectionToMouse）
  - `Assets/Scripts/Cards/CardEffectExecutor.cs`（骨架 → 完整实现：ExecuteLeft/ExecuteRight/Damage(Projectile+Focus倍率)/Block(GainBlock)/Heal(Heal)/Focus(AddFocus)）
  - `Assets/Scripts/Cards/CardRuntimeInstance.cs`（适配新字段名）
  - `Assets/Scripts/Combat/PlayerController2D.cs`（Awake 创建 CardEffectExecutor+PlayerCardContext；Input 左键→ExecuteLeft/右键→ExecuteRight；testCard 为空时 fallback Shoot()）
  - `Assets/Editor/Cardwin/CardwinSceneBuilder.cs`（新增 `Create 4 Demo Card Assets` 菜单 + `CreateCardAsset` 方法）
  - 更新 `SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`、`TODO.md`
- **新增函数**：
  - CardEffectExecutor: `ExecuteLeft()`, `ExecuteRight()`, `ExecuteEffect()`, `ExecuteDamageEffect()`, `ExecuteBlockEffect()`, `ExecuteHealEffect()`, `ExecuteFocusEffect()`
  - PlayerCardContext: `AddFocus()`, `ConsumeFocusMultiplier()`, `GetShootDirectionToMouse()`
  - CardwinSceneBuilder: `CreateDemoCards()`, `CreateCardAsset()`
- **新增资产**：4 张 ScriptableObject 卡牌（菜单 Tools > Cardwin > Create 4 Demo Card Assets）
- **测试步骤**：
  1. 执行 `Tools > Cardwin > Create 4 Demo Card Assets`
  2. PlayerController2D.testCard = Strike → 左键发射 10 伤害子弹
  3. testCard = Guard → 左键 5 伤害子弹，右键 +10 护盾
  4. testCard = Heal → 左键 5 伤害子弹，右键 +12 HP
  5. testCard = Focus → 左键/右键 +1 Focus；下次 Damage +50% 伤害
- **下一步**：Stage 6 — Magazine System

---

### 2026-05-29 | Stage 5 Fix — SceneBuilder Disabled
- **用户需求**：修复 CardwinSceneBuilder 引用 CardType/CardRarity/CardEffectType 导致的 4 个编译错误。Editor 工具不应依赖运行时代码。
- **修改文件**：
  - `Assets/Editor/Cardwin/CardwinSceneBuilder.cs`（612 行 → 19 行：替换为安全占位 stub，仅显示禁用提示弹窗）
  - 更新 `AGENTS.md`（SceneBuilder is stubbed disabled tool）
  - 更新 `SYSTEM_INDEX.md`（CardwinSceneBuilder 标记为 DISABLED）
  - 更新 `DEVELOPMENT_LOG.md`（本记录）
- **新增函数**：无
- **删除内容**：
  - 所有场景创建逻辑（`CreateMainCamera`, `CreateGround`, `CreatePlayer`, `CreateEnemy` 等 20+ 方法）
  - 所有卡牌创建逻辑（`CreateDemoCards`, `CreateCardAsset`）
  - 所有 prefab 修复逻辑（`FixProjectilePrefab`, `CreateProjectileSpriteAsset`, `UpdateProjectilePrefab`）
  - 所有层管理逻辑（`EnsureAllLayers`, `EnsureLayer`, `ResolveLayers`）
  - 所有 `using Cardwin.Combat` / `using Cardwin.Cameras` / `using Cardwin.Cards` 引用
- **保留**：`Tools/Cardwin/Rebuild Clean Demo Scene` 菜单项 → 点击弹窗 "SceneBuilder is disabled"
- **已知问题**：弹窗提示后如需重建场景，需要重新开发重建工具
- **下一步**：Stage 6 — Magazine System

---

### 2026-05-29 | Stage 5A — Create Basic Card Assets
- **用户需求**：Assets/Data/Cards 文件夹存在但无 Strike/Guard/Heal/Focus 资产。创建独立 Editor 工具生成 4 张卡牌。
- **修改文件**：
  - 新增 `Assets/Editor/Cardwin/CardAssetCreator.cs`（独立卡牌创建工具，不依赖 SceneBuilder）
  - 更新 `SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`、`TODO.md`
- **新增类**：`CardAssetCreator`
- **新增函数**：`CreateBasicCards()`, `EnsureDirectory()`, `FindProjectilePrefab()`, `CreateOrUpdateCard()`
- **新增资产**（菜单 Tools > Cardwin > Create Basic Card Assets）：
  - `Assets/Data/Cards/Strike.asset` — Attack, damage=10, Damage/Damage
  - `Assets/Data/Cards/Guard.asset` — Defense, damage=5/block=10, Damage/Block
  - `Assets/Data/Cards/Heal.asset` — Heal, damage=5/heal=12, Damage/Heal
  - `Assets/Data/Cards/Focus.asset` — Utility, focusGain=1, Focus/Focus
- **测试步骤**：点击菜单 → Console 输出 → Project 窗口确认 4 个 .asset
- **下一步**：Stage 6 — Magazine System

---

### 2026-05-29 | Stage 5B — Card Target Rule Correction
- **用户需求**：修正卡牌目标规则——左键=对外发射子弹命中谁效果就作用到谁，右键=对自己使用。效果不区分好坏对象。
- **修改文件**：
  - `Assets/Scripts/Combat/Projectile.cs`（新增 `Init(card+effect+context)` 重载；命中→调用 `CardEffectExecutor.ApplyEffectToTarget`）
  - `Assets/Scripts/Cards/CardEffectExecutor.cs`（重写：`ExecuteLeft` 生成 Projectile；`ExecuteRight` 对 Player 施效；`ApplyEffectToTarget` 统一施加 Damage/Block/Heal/Focus）
  - `Assets/Editor/Cardwin/CardAssetCreator.cs`（Guard left=Block, Heal left=Heal）
  - 更新 `SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`、`TODO.md`
- **设计规则**：左键=对外(Projectile→命中对象)，右键=对自己(Player)；效果不区分好坏对象；Focus 仅 Player
- **下一步**：Stage 6 — Magazine System

---

### 2026-05-29 | Stage 6A — MagazineSystem Core
- **用户需求**：将 testCard 单卡模式升级为 MagazineSystem 驱动的 8 发弹夹系统
- **修改文件**：
  - `Assets/Scripts/Magazine/MagazineSystem.cs`（完全重写：8发弹夹/SetMagazineCards/UseLeft/UseRight/Advance/Reload/Preview/事件）
  - `Assets/Scripts/Combat/PlayerController2D.cs`（MagazineSystem 集成 + R 键换弹 + testCard fallback）
  - 更新 `SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`、`TODO.md`
- **Unity 挂载**：Player 挂载 MagazineSystem → initialCards 拖入 8 张
- **测试**：左键/右键消耗卡 → 8 发用完自动 Reload → R 键手动 Reload → 换弹期间禁用
- **下一步**：Stage 6B — Magazine Preview UI

---

### 2026-05-29 | Stage 6B — Magazine Preview HUD
- **用户需求**：Game 视图中显示弹夹最近 3 发预览 + HP/Shield/Focus + Reload 进度
- **修改文件**：
  - `Assets/Scripts/UI/MagazinePreviewUI.cs`（重写：自动创建 3 个 CardSlotUI + HorizontalLayout，订阅 MagazineSystem 事件刷新）
  - `Assets/Scripts/UI/CardSlotUI.cs`（重写：SetCard 显示卡名+效果类型，当前卡 >Name< 高亮，SetEmpty/SetReloading）
  - `Assets/Scripts/UI/CombatHUD.cs`（重写：AutoBind 自动查找 Player+MagazineSystem，运行时创建 HP/Shield/Focus/Reload 文本，每帧刷新 + MagazinePreviewUI 自动绑定）
  - `Assets/Scripts/Magazine/MagazineSystem.cs`（新增 `Capacity` / `ReloadProgress` 只读属性）
  - 更新 `SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`、`TODO.md`
- **新增函数**：
  - MagazinePreviewUI: `Bind()`, `RefreshPreview()`, `OnReloadStarted()`, `OnReloadFinished()`, `EnsureSlotsExist()`, `CreateSlotObject()`
  - CardSlotUI: `SetCard()`, `SetEmpty()`, `SetReloading()`
  - CombatHUD: `AutoBind()`, `EnsureTextElements()`, `EnsureTextChild()`, `RefreshHUD()`, `RefreshReloadProgress()`
- **Canvas 设置**：Canvas 挂载 CombatHUD → 自动创建所有子 UI（HP/Shield/Focus/Reload 文本 + MagazinePreview + 3 个 CardSlot）
- **测试步骤**：
  1. Canvas 挂载 CombatHUD → Play
  2. 左上角显示 HP: 50/50, Shield(有盾时), Focus(有层时)
  3. 下方显示 3 个卡槽：> Strike < / Guard / Heal
  4. 左键→预览前进到 > Guard < / Heal / Focus
  5. 8 发用完→所有卡槽显示 "Reloading" + 百分比
  6. Reload 完成→回到 > Strike < / Guard / Heal
  7. R 键→"Reloading... xx%"
- **下一步**：Stage 7 — Inventory System

---

### 2026-05-29 | Stage 6C — Full Magazine Debug HUD
- **用户需求**：显示完整 8 格弹夹，当前 index 高亮，已用卡灰色区分
- **修改文件**：
  - 新增 `Assets/Scripts/UI/MagazineFullBarUI.cs`（自动创建 8 个 CardSlotUI + HorizontalLayout，订阅 MagazineSystem 事件，显示 Index/当前卡/已用/未用/Reload）
  - `Assets/Scripts/UI/CardSlotUI.cs`（新增 `SetCard(card, current, used)` 三态重载：当前白+scale1.1，已用灰+[Used]）
  - `Assets/Scripts/UI/CombatHUD.cs`（新增 `magazineFullBarUI` 字段 + AutoBind 自动创建）
  - 更新 `SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`、`TODO.md`
- **新增类**：`MagazineFullBarUI`
- **新增函数**：
  - MagazineFullBarUI: `Bind()`, `RefreshFullBar()`, `HandleReloadStarted()`, `HandleReloadFinished()`, `EnsureSlotsExist()`
  - CardSlotUI: `SetCard(card, current, used)` overload
- **Canvas**：挂载 CombatHUD → 自动创建 3 行：HP/Shield/Focus（左上）、8格FullBar（中偏下）、3发Preview（下方）+ Reload进度（底部）
- **测试**：Play → 8格显示 [>Strike<][Guard][Heal][Focus][Strike][Strike][Guard][Strike]；左键后 index 前进，已用变为灰色+[Used]；Reload后全部重置
- **下一步**：Stage 7 — Inventory System

---

### 2026-05-29 | Stage 6B.1 — HUD Mount & Visibility Fix
- **用户需求**：修复 HUD 完全不显示问题 — Game 视图中看不到弹夹预览/8格弹夹/HP/Shield/Focus
- **修改文件**：
  - 新增 `Assets/Scripts/UI/HUDRuntimeBootstrapper.cs`（运行时自动挂载 CombatHUD 到 Canvas）
  - 重写 `Assets/Scripts/UI/CombatHUD.cs`（Awake 创建完整 UI 层级 + 正确锚点 + CanvasScaler + 绑定日志）
  - 修改 `Assets/Scripts/UI/MagazinePreviewUI.cs`（slot 120x60 / fontSize 18 / raycastTarget=false / Start 防重复绑定）
  - 修改 `Assets/Scripts/UI/MagazineFullBarUI.cs`（slot 90x45 / fontSize 14 / raycastTarget=false / Start 防重复绑定）
  - 修改 `Assets/Scripts/UI/CardSlotUI.cs`（全部 Text+Image raycastTarget=false / 当前卡背景亮黄）
  - 修改 `Assets/Scripts/Magazine/MagazineSystem.cs`（empty initialCards 警告日志）
  - 更新 `SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`、`TODO.md`
- **新增类**：`HUDRuntimeBootstrapper`
- **新增函数**：
  - HUDRuntimeBootstrapper: `Awake()`
  - CombatHUD: `Awake()`, `EnsureCanvas()`, `EnsureHUDRoot()`, `EnsureTopLeftStats()`, `EnsureMagazinePreview()`, `EnsureMagazineFullBar()`, `EnsureReloadText()`, `EnsureTextInParent()`, `BindSystems()`
- **新增字段**：CombatHUD: `_hudRoot`, `_warnedMagazineMissing`, `_loggedFirstRefresh`
- **Unity 挂载方式**：
  - **HUDRuntimeBootstrapper** 挂载到场景中的 `SceneRuntime` GameObject（与 DemoSceneRuntimeBootstrapper 同一对象）
  - Play 后 Console 应输出 `[HUDBootstrapper] CombatHUD attached to Canvas.`
- **UI 层级结构**：
  ```
  Canvas
    CombatHUD_Root (full-screen stretch)
      TopLeftStats (anchor top-left, pos=20,-20)
        HP_Text (fontSize=24, white)
        Shield_Text (fontSize=24, light blue)
        Focus_Text (fontSize=24, gold)
      BottomMagazinePreview (anchor bottom-center, pos=0,100) [+MagazinePreviewUI]
        MagazineSlots [HorizontalLayout]
          Slot_0 (120x60)
          Slot_1 (120x60)
          Slot_2 (120x60)
      BottomFullMagazine (anchor bottom-center, pos=0,20) [+MagazineFullBarUI]
        FullBarSlots [HorizontalLayout]
          Slot_0..7 (90x45 each)
      Reload_Text (anchor center, pos=0,120, orange)
  ```
- **UI 锚点方案**：
  - TopLeftStats: anchorMin=(0,1), anchorMax=(0,1), pivot=(0,1), anchoredPosition=(20,-20)
  - BottomMagazinePreview: anchorMin=(0.5,0), anchorMax=(0.5,0), pivot=(0.5,0), anchoredPosition=(0,100)
  - BottomFullMagazine: anchorMin=(0.5,0), anchorMax=(0.5,0), pivot=(0.5,0), anchoredPosition=(0,20)
  - Reload_Text: anchorMin=(0.5,0.5), anchorMax=(0.5,0.5), pivot=(0.5,0.5), anchoredPosition=(0,120)
- **绑定逻辑**：CombatHUD.Start() → BindSystems() → GameObject.FindWithTag("Player") → GetComponent<MagazineSystem>() → 调用 magazinePreviewUI.Bind() + magazineFullBarUI.Bind()；日志 `[CombatHUD] Bound MagazineSystem. Cards=8`
- **测试步骤**：
  1. 将 `HUDRuntimeBootstrapper` 挂载到 SceneRuntime GameObject
  2. 确保 Player 挂载 MagazineSystem + initialCards 有 8 张卡牌
  3. Play → Console 输出绑定日志
  4. Game 视图左上角可见 HP/Shield/Focus
  5. Game 视图下方可见 3 卡预览 + 8 格完整弹夹
  6. 左键/右键使用卡牌 → UI 同步推进
  7. 8 发用完 → 自动 Reload → UI 显示 Reloading
  8. R 键 → 手动 Reload → UI 显示 Reloading 进度
- **下一步**：Stage 7 — Inventory System

---

### 2026-05-29 | Stage 6B.4 — HUD Layout Cleanup
- **用户需求**：修复 UI 文本重叠；清理旧占位 UI (HP_Text, MagazinePreview_Placeholder, State_Text)；统一 HUD 根节点为 CardwinHUDRoot
- **修改文件**：
  - `Assets/Scripts/UI/CombatHUD.cs`（重写：DisableLegacyPlaceholders 禁用旧对象 / CardwinHUDRoot 统一根节点 / PreviewPanel+FullMagazinePanel 替代旧容器 / 精确锚点+HLG 布局）
  - `Assets/Scripts/UI/MagazinePreviewUI.cs`（槽位重命名 PreviewSlot_0~2 / HLG spacing=12 childControl=false / slot 150x60 / NameText+EffectText 分区域锚点防重叠）
  - `Assets/Scripts/UI/MagazineFullBarUI.cs`（槽位重命名 FullSlot_0~7 / HLG spacing=8 childControl=false / slot 95x50 / NameText+EffectText 分区域锚点防重叠）
  - `Assets/Scripts/UI/CardSlotUI.cs`（新增 EffectToShort 缩写函数 Dmg/Blk/Heal/Fcs / 效果文本 L:Dmg R:Dmg 格式 / Reloading 状态显示全称）
  - 更新 `SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`、`TODO.md`
- **新增函数**：`CardSlotUI.EffectToShort()` (static)
- **新增方法**：`CombatHUD.DisableLegacyPlaceholders()`
- **被禁用的旧对象**：
  - HP_Text → SetActive(false)
  - MagazinePreview_Placeholder → SetActive(false)
  - State_Text → SetActive(false)
- **CardwinHUDRoot 层级**：
  ```
  Canvas
    CardwinHUDRoot (anchor 0,0 - 1,1 full stretch)
      TopLeftStats (anchor top-left, pos 20,-20, size 320x120)
        HP_Text_Runtime
        Shield_Text_Runtime
        Focus_Text_Runtime
      PreviewPanel (anchor bottom-center, pos 0,135, size 520x85) [+MagazinePreviewUI]
        PreviewSlot_0, PreviewSlot_1, PreviewSlot_2 (150x60 each)
      FullMagazinePanel (anchor bottom-center, pos 0,35, size 900x70) [+MagazineFullBarUI]
        FullSlot_0..FullSlot_7 (95x50 each)
      ReloadText (anchor center, pos 0,120, size 300x50)
  ```
- **PreviewPanel / FullMagazinePanel 锚点**：
  - PreviewPanel: anchor=(0.5,0), pivot=(0.5,0), pos=(0,135), size=(520,85)
  - FullMagazinePanel: anchor=(0.5,0), pivot=(0.5,0), pos=(0,35), size=(900,70)
- **槽位内部布局**：
  - NameText: anchorMin=(0,0.45), anchorMax=(1,1), offsetMin=(4,0), offsetMax=(-4,-4)
  - EffectText: anchorMin=(0,0), anchorMax=(1,0.45), offsetMin=(4,4), offsetMax=(-4,0)
  - 上下分区防重叠，NameText 占上 55%，EffectText 占下 45%
- **视觉规则**：
  - 当前卡：背景黄(1,1,0.5,0.5)，scale=1.1，name=>Name<
  - 未使用卡：背景深灰(0.3,0.3,0.3,0.25)，scale=1.0
  - 已使用卡：背景深灰(0.2,0.2,0.2,0.3)，effect=[Used]
  - Reloading：背景橙(1,0.5,0,0.2)，name=Reloading
- **数据绑定日志**：
  - Cards>0: `[CombatHUD] Bound MagazineSystem. Cards=8`
  - Cards=0: `[CombatHUD] MagazineSystem has no cards. Check Player initialCards.`
- **测试步骤**：
  1. 确保 HUDRuntimeBootstrapper 挂载到 SceneRuntime
  2. Play → 旧 [1] --- | [2] --- | [3] --- 不再显示
  3. Game 视图仅显示一套清晰 HUD
  4. 左上角 HP/Shield/Focus 分开显示不重叠
  5. 底部上方 3 发预览间距均匀
  6. 底部最下方 8 格弹夹间距均匀
  7. 文字不重叠（Name 和 Effect 分上下区域）
  8. 左键/右键推进→两套 UI 同步
- **下一步**：Stage 7 — Inventory System

---

### 2026-05-29 | Stage 6D — Combat HUD Simplify + Random Reload
- **用户需求**：战斗HUD只显示3发预览（不显示完整8格弹夹）；MagazineSystem改为随机装弹（Fisher-Yates洗牌）
- **修改文件**：
  - `Assets/Scripts/UI/CombatHUD.cs`（移除 EnsureFullMagazinePanel 调用 / 新增 DisableFullBarIfExists 禁用旧FullBar / 移除 MagazineFullBarUI 字段和绑定 / PreviewPanel 下移至 y=35）
  - `Assets/Scripts/Magazine/MagazineSystem.cs`（新增 BuildRandomMagazine + Fisher-Yates洗牌 / Start调用随机装弹 / FinishReload调用随机装弹 / 新增 shuffleOnReload + allowRepeatWhenNotEnoughCards 字段）
  - 更新 `SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`、`TODO.md`
- **新增函数**：`MagazineSystem.BuildRandomMagazine()`, `CombatHUD.DisableFullBarIfExists()`
- **新增字段**：MagazineSystem: `shuffleOnReload` (bool), `allowRepeatWhenNotEnoughCards` (bool)
- **移除**：CombatHUD: `_magazineFullBarUI` 字段, `EnsureFullMagazinePanel()` 方法
- **战斗HUD当前显示内容**：
  - 左上角：HP / Shield / Focus（TopLeftStats + VerticalLayout）
  - 底部：3发预览（PreviewPanel, y=35, size=520x80, 3x 150x60 slots）
  - 屏幕中央：Reloading 状态（ReloadText, y=120）
  - 不再创建或显示完整8格弹夹
- **MagazineSystem随机装弹逻辑**：
  - `BuildRandomMagazine()` 从 `initialCards`（卡池）中随机抽取 `capacity=8` 张
  - 卡池 >=8 张：Fisher-Yates洗牌取前8张
  - 卡池 <8 张 + allowRepeatWhenNotEnoughCards=true：允许重复随机抽取直到8张
  - 卡池 <8 张 + allowRepeatWhenNotEnoughCards=false：只装入已有数量并Warning
  - 日志输出：`[Magazine] Random loaded: Strike, Guard, Heal, Focus, Strike, ...`
- **sourceCards / initialCards 配置**：
  - `initialCards` 是用于随机装弹的卡池（source pool）
  - Inspector 中拖入4张卡牌资产（Strike, Guard, Heal, Focus）
  - system随机生成8发不同顺序
  - 未来背包系统接管 sourceCards 编辑
- **测试随机装弹**：
  1. Player Inspector → MagazineSystem → initialCards 拖入4张卡牌
  2. Play → Console 查看 `[Magazine] Random loaded: ...` 顺序
  3. 打空8发（或R键手动Reload）
  4. 再次查看Console — 顺序应与上次不同
  5. 多次Reload验证随机性
- **下一步**：Stage 7 — Inventory System

---

### 2026-05-29 | Stage 6E — CardDatabase / Bullet Function Registry
- **用户需求**：创建统一的 CardDatabase ScriptableObject 作为卡牌/子弹功能总表；提供按 ID/名称/类型/稀有度/效果查询；支持随机抽取；提供 Editor 重建工具；MagazineSystem 可选接入 CardDatabase 作为卡池
- **修改文件**：
  - 新增 `Assets/Scripts/Cards/CardDatabase.cs`（ScriptableObject：allCards列表 + Dictionary缓存 + 8个查询/抽取方法 + ValidateDatabase）
  - 新增 `Assets/Editor/Cardwin/CardDatabaseEditorUtility.cs`（Editor菜单 Tools/Cardwin/Rebuild Card Database / 扫描Cards文件夹 / 创建CardDatabase.asset）
  - 修改 `Assets/Scripts/Magazine/MagazineSystem.cs`（新增 cardDatabase + useDatabaseAsSource 字段 + ResolveSourcePool() + BuildRandomMagazine 优先用CardDatabase）
  - 更新 `SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`、`TODO.md`
- **新增类**：`CardDatabase` (ScriptableObject), `CardDatabaseEditorUtility` (static editor)
- **新增函数**：
  - CardDatabase: `Initialize()`, `GetById()`, `GetByName()`, `GetByType()`, `GetByRarity()`, `GetByEffect()`, `GetRandomCard()`, `GetRandomCards(count, allowDuplicate)`, `ValidateDatabase()`
  - CardDatabaseEditorUtility: `RebuildCardDatabase()`, `EnsureCardsFolder()`
  - MagazineSystem: `ResolveSourcePool()`
- **新增字段**：CardDatabase: `allCards`, `_cardById`, `_cardByName`；MagazineSystem: `cardDatabase`, `useDatabaseAsSource`
- **CardDatabase.asset 路径**：`Assets/Data/Cards/CardDatabase.asset`
- **重建菜单**：`Tools > Cardwin > Rebuild Card Database`
  - 扫描 `Assets/Data/Cards` 下所有 CardData 资产（排除 CardDatabase 自身）
  - 创建或更新 CardDatabase.asset
  - 调用 ValidateDatabase 输出校验日志
  - Console: `[CardDatabaseEditor] Rebuilt CardDatabase. Count=4`
- **查询接口**：
  - `GetById("Strike0")` → CardData
  - `GetByName("Strike")` → CardData
  - `GetByType(CardType.Attack)` → List\<CardData\>
  - `GetByRarity(CardRarity.Common)` → List\<CardData\>
  - `GetByEffect(CardEffectType.Heal)` → List\<CardData\>
  - `GetRandomCard()` → 随机一张
  - `GetRandomCards(8, false)` → 8张不重复随机
- **ValidateDatabase 检查项**：null卡 / 空cardId / 重复cardId / 空cardName / 重复cardName / 无description / Damage卡damage<=0 / Block卡block<=0 / Heal卡heal<=0 / Focus卡focusGain<=0
- **MagazineSystem 可选接入**：
  - `useDatabaseAsSource = true` → Reload时从 `cardDatabase.allCards` 随机装弹
  - `useDatabaseAsSource = false` (默认) → 继续使用 `initialCards`
  - fallback: 如果CardDatabase无卡则退回 initialCards
- **测试步骤**：
  1. 点击 `Tools > Cardwin > Rebuild Card Database`
  2. Console: `[CardDatabaseEditor] Rebuilt CardDatabase. Count=4`
  3. Console: `[CardDatabase] Validated. Total=4, Errors=0, Warnings=0`
  4. Play → MagazineSystem 正常随机装弹
  5. 战斗HUD显示3发预览正常
  6. （可选）Inspector中设置 MagazineSystem.useDatabaseAsSource=true + cardDatabase=CardDatabase.asset → 从数据库随机装弹
- **下一步**：Stage 7 — Inventory System
---
### 2026-05-29 | Stage 7A — Inventory + Magazine Edit Panel
- **用户需求**：创建背包/弹夹编辑面板，按B键打开，左边显示拥有的卡牌，右边显示8格Loadout，点击加卡/移除，Loadout修改后Reload从新装弹池随机生成
- **修改文件**：
  - 重写 `Assets/Scripts/Inventory/InventorySystem.cs`（string-based item → CardData-based ownedCards；新增 AddCard/RemoveCard/RemoveCardAt/GetOwnedCards/GetOwnedCount/HasCard/InitializeDefaultCards/AddDefaultCardsFromDatabase）
  - 修改 `Assets/Scripts/Magazine/MagazineSystem.cs`（新增 LoadoutCards + SetLoadoutCards/GetLoadoutCards / Start从initialCards初始化loadout / ResolveSourcePool优先loadoutCards→cardDatabase→initialCards / SetLoadoutCards后立即BuildRandomMagazine）
  - 新增 `Assets/Scripts/UI/MagazineEditUI.cs`（BagPanel：B键Toggle/Open暂停Close恢复/左侧OwnedCardsPanel+右侧LoadoutGrid 2x4/点击OwnedCard加入Loadout/点击LoadoutSlot移除/运行时EnsureUI自动创建）
  - 修改 `Assets/Scripts/UI/CardSlotUI.cs`（EffectToShort→EffectToShortPublic / 新增 SetCardForInventory/SetCardForLoadout/SetEmptyLoadoutSlot 带点击回调）
  - 修改 `Assets/Scripts/Combat/PlayerController2D.cs`（新增 inventorySystem+magazineEditUI字段 / Awake自动创建InventorySystem+初始化默认卡+创建MagazineEditUI / B键Toggle背包 / using Cardwin.Inventory+Cardwin.UI）
  - 更新 `SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`、`TODO.md`
- **新增类**：`MagazineEditUI`
- **新增函数**：
  - InventorySystem: `AddCard()`, `RemoveCard()`, `RemoveCardAt()`, `GetOwnedCards()`, `GetOwnedCount()`, `HasCard()`, `InitializeDefaultCards()`, `AddDefaultCardsFromDatabase()`
  - MagazineSystem: `SetLoadoutCards()`, `GetLoadoutCards()`
  - MagazineEditUI: `Toggle()`, `Open()`, `Close()`, `Refresh()`, `RefreshOwnedCards()`, `RefreshLoadoutSlots()`, `OnOwnedCardClicked()`, `OnLoadoutSlotClicked()`, `EnsureUI()`, `CreateSlot()`, `EnsureText()`
  - CardSlotUI: `EffectToShortPublic()`, `SetCardForInventory()`, `SetCardForLoadout()`, `SetEmptyLoadoutSlot()`
  - PlayerController2D: (B key handler in Update)
- **新增字段**：
  - InventorySystem: `ownedCards` (List\<CardData\>), `defaultDatabase` (CardDatabase)
  - MagazineSystem: `LoadoutCards` (List\<CardData\>)
  - PlayerController2D: `inventorySystem` (InventorySystem), `magazineEditUI` (MagazineEditUI)
- **Unity 挂载方式**：
  - 无需手动挂载：PlayerController2D.Awake 自动 AddComponent<InventorySystem>() + 自动 AddComponent<MagazineEditUI>() 到 Canvas
  - CardDatabase 通过 MagazineSystem.cardDatabase 自动传入 InventorySystem 初始化默认卡牌
  - 如无 CardDatabase 则 ownedCards 为空（需运行 Rebuild Card Database）
- **UI 结构**：
  ```
  Canvas
    CardwinHUDRoot (战斗HUD，不变)
    BagPanel (背包面板，居中700x500，半透明深色背景)
      Title: "Inventory / Magazine Edit"
      OwnedCardsPanel (左侧280x400)
        OwnedLabel: "Owned Cards"
        OwnedSlot_N: 卡名 + 数量，点击加入Loadout
      LoadoutPanel (右侧350x400)
        LoadoutLabel: "Loadout (8 Slots)"
        LoadoutGrid (2x4 GridLayout, cell=100x100)
          LoadoutSlot_0..7: 卡名+效果缩写，点击移除
      HintText: "Click owned card to add. Click loadout slot to remove. Press B to close."
  ```
- **设计规则**：
  - Inventory = 玩家拥有的全部卡牌（允许重复）
  - Loadout = 从Inventory选出的8张装弹池
  - loadedCards = 每次Reload从Loadout随机生成的战斗弹夹
  - 战斗HUD只显示3发预览（不变）
  - B键打开背包→暂停游戏；再按B关闭→恢复
  - Loadout最多8张
  - SetLoadoutCards后立即BuildRandomMagazine重新随机装弹
- **默认数据**：
  - ownedCards: Strike x3, Guard x2, Heal x2, Focus x1（从CardDatabase自动加载）
  - Loadout: 从initialCards初始化（首个Play或MagazineSystem initialCards不为空时）
  - 如果initialCards为空且CardDatabase存在 → Loadout为空，需在BagPanel手动添加
- **已知问题**：
  - 背包打开时Time.timeScale=0，所有Update暂停（包括Reload timer），关闭后恢复
  - MagazineEditUI 按B键逻辑在自身Update中处理，PlayerController2D也有一份B键检测（优先级：EditorUI先Toggle暂停→PlayerController再检测时已关闭）
  - ownedCards 基于 CardData 引用去重统计数量，若同一 CardData 引用多次出现则显示数量叠加
  - InventorySystem.RemoveCardAt(0) 会删除第一个匹配项，而非指定索引的所有匹配
- **下一步**：BagPanel 可见性修复 + 输入锁定

---
### 2026-05-30 | Stage 7A.1 — Bag UI Visibility + Menu Input Lock
- **用户需求**：修复背包打开后看不到界面/Player仍可移动射击的问题；重建清晰BagPanel；B/Esc开关；InputLock阻止战斗输入
- **修改文件**：
  - 修改 `Assets/Scripts/Combat/PlayerController2D.cs`（新增 `_inputLocked` + `SetInputLocked()` / Update中所有战斗输入包在 `if(!_inputLocked)` 内 / FixedUpdate locked时velocity归零 / B键移除由MagazineEditUI接管）
  - 重写 `Assets/Scripts/UI/MagazineEditUI.cs`（完全重写：EnsureEventSystem兜底EventSystem+StandaloneInputModule / BagPanel 900x520居中半透明 / OwnedCardsPanel左侧ScrollRect+GridLayout 3列cell=110x60 / LoadoutPanel右侧GridLayout 2列 / CanvasGroup控制raycast+interact / B打开Esc/B关闭 / SetInputLocked(true/false)+timeScale=0/1+Cursor / 关闭按钮CloseButton / 全部明细日志）
  - 更新 `SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`、`TODO.md`
- **新增函数**：
  - PlayerController2D: `SetInputLocked(bool)`
  - MagazineEditUI: `CloseQuick()`, `EnsureEventSystem()`, `CreateTextSlot()`, `CreateTextChild()`
- **新增字段**：
  - PlayerController2D: `_inputLocked` (bool)
  - MagazineEditUI: `_canvasGroup` (CanvasGroup), `_playerController` (PlayerController2D), `_ownedGridRoot` / `_loadoutGridRoot` (Transform)
- **删除**：PlayerController2D 中 B 键检测（由 MagazineEditUI.Update 接管）；MagazineEditUI 旧 EnsureUI/CreateSlot/EnsureText
- **BagPanel 层级**：
  ```
  Canvas
    BagPanel (900x520 centered, inactive default)
      CanvasGroup (alpha=0 interact=off)
      TitleText: "Inventory / Magazine Edit"
      OwnedTitle: "Owned Cards"
      OwnedCardsPanel (ScrollRect)
        Viewport (Mask)
          Content (GridLayoutGroup 3col, cell=110x60)
            OwnedSlot (Button) — cardName xN + Dmg/Blk
      LoadoutTitle: "Loadout (8 Slots)"
      LoadoutPanel (GridLayoutGroup 2col, cell=110x60, spacing=12)
        LoadoutSlot_0..7 (Button fill/empty) — cardName + [index]
      HintText: "Click owned card to add. Click loadout slot to remove. B / Esc to close."
      CloseButton: "Close" (top-right)
  ```
- **输入锁定规则**：
  - Open: playerController.SetInputLocked(true) + Time.timeScale=0 + Cursor.visible=true
  - Close: SetInputLocked(false) + Time.timeScale=1
  - locked=true 时：A/D/Space/Shift/左键/右键/R 全部不响应
  - B/Esc 由 MagazineEditUI.Update() 独立处理（timeScale=0 不影响 Update 执行）
- **测试步骤**：
  1. Play → 战斗HUD正常3发预览 → A/D移动/跳跃/射击正常
  2. 按 B → 屏幕中央出现 BagPanel (900x520, 深色背景)
  3. 左侧OwnedCards可见 (Strike x3, Guard x2, Heal x2, Focus x1)
  4. 右侧Loadout 8 slots 可见
  5. A/D/Space/Shift/左键/右键 → Player 无反应
  6. 点击OwnedCard → Loadout 对应 slot 填充
  7. 点击LoadoutSlot → 卡牌移除
  8. Loadout 满 8 张后再点 OwnedCard → Console: "Loadout full."
  9. B 或 Esc → 面板关闭，Player 恢复控制
  10. 关闭后战斗HUD 3发预览正常
- **下一步**：修复库存数量 + Apply Loadout 实战

---
### 2026-05-30 | Stage 7A.2 — Inventory Test Stock + Apply Loadout To Combat
- **用户需求**：背包显示聚合数量(Strike x20)；点击加卡扣除库存/移除返还库存；Loadout修改后立即BuildRandomMagazine影响实际射击；测试库存4种x20
- **修改文件**：
  - 重写 `Assets/Scripts/Inventory/InventorySystem.cs`（新增 InventoryEntry struct / `InitializeTestStock(database)`清空+Strike/Guard/Heal/Focus各20 / `AddCards(card,count)`批量添加 / `RemoveCard(card)`返回bool找首个匹配移除 / `GetCount(card)`按引用计数 / `GetCardCounts()`返回List<InventoryEntry>聚合 / `EnsureTestStockIfEmpty()`兜底 / 移除旧的InitializeDefaultCards/AddDefaultCardsFromDatabase/RemoveCardAt/GetOwnedCount）
  - 修改 `Assets/Scripts/Magazine/MagazineSystem.cs`（SetLoadoutCards输出Loadout名字列表+"Loadout updated:" / BuildRandomMagazine日志改为"Random loaded from loadout:" / SetLoadoutCards后显式CurrentIndex=0+IsReloading=false+OnMagazineChanged）
  - 修改 `Assets/Scripts/UI/MagazineEditUI.cs`（RefreshOwnedCards改用`inventorySystem.GetCardCounts()`聚合/OnOwnedCardClicked先检查库存>0→inventory.RemoveCard→loadout.Add→SetLoadoutCards→日志"Inventory left=N"/OnLoadoutSlotClicked移除后inventory.AddCard返还→日志"Inventory now=N"）
  - 修改 `Assets/Scripts/Combat/PlayerController2D.cs`（库存初始化改用`InitializeTestStock` / 从CardDatabase查4张卡→default loadout[Strike,Guard,Heal,Focus,Strike,Strike,Guard,Strike]→每张inventory.RemoveCard扣库存→SetLoadoutCards→日志"Default loadout initialized"）
  - 更新 `SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`、`TODO.md`
- **新增类**：`InventoryEntry` (struct: CardData card + int count)
- **新增函数**：
  - InventorySystem: `InitializeTestStock()`, `AddCards()`, `GetCardCounts()`, `EnsureTestStockIfEmpty()`
- **修改函数签名**：
  - InventorySystem.RemoveCard: 返回 bool（找首个匹配，找到移除返回true，否则false）
  - PlayerController2D 中 Inventory 初始化: `InitializeDefaultCards()` → `InitializeTestStock()`
- **删除**：InventorySystem.RemoveCardAt, GetOwnedCount, InitializeDefaultCards, AddDefaultCardsFromDatabase
- **数据流**：
  1. PlayerController2D.Awake → InventorySystem.InitializeTestStock(Strike x20/Guard x20/Heal x20/Focus x20)
  2. 从库存扣8张形成默认Loadout → MagazineSystem.SetLoadoutCards() → BuildRandomMagazine()
  3. 用户B键打开背包 → 左侧显示聚合: Strike x16 / Guard x17 / Heal x19 / Focus x19
  4. 点击Strike → inventory.RemoveCard(Strike) → 库存 Strike 15 → loadout.Add(Strike) → SetLoadoutCards → BuildRandomMagazine
  5. 点击LoadoutSlot → loadout移除 → inventory.AddCard(removed) → 库存 +1 → SetLoadoutCards → BuildRandomMagazine
  6. 关闭背包后左键/右键使用loadedCards当前卡（来自新Loadout随机装弹）
- **测试步骤**：
  1. Play → Console: `[Inventory] Test stock initialized: Strike=20, Guard=20, Heal=20, Focus=20`
  2. Console: `[Magazine] Default loadout initialized. Count=8` + `[Magazine] Random loaded from loadout: ...`
  3. 战斗HUD 3发预览显示新Loadout随机出来的卡
  4. 按B → 左侧: Strike x16, Guard x17, Heal x19, Focus x19；右侧: 8格Loadout
  5. 点击左侧Strike → 库存Strike变15，Loadout新增Strike，Console: `Add Strike to loadout. Inventory left=15`
  6. 点击右侧Loadout中Strike → 库存Strike变16，Loadout移除该格，Console: `Remove Strike from loadout. Inventory now=16`
  7. 关闭背包 → 战斗HUD 3发预览已刷新为新Loadout
  8. 左键/右键使用卡牌来自新Loadout随机装弹
- **下一步**：Continue Stage 7A — Inventory / Magazine Editing stabilization

---
### 2026-05-30 | Stage 7A.3 — Inventory Stock + Loadout Binding Real Fix
- **用户需求**：修复 OwnedCards=8, Loadout=0 问题；确保测试库存4种x20正确初始化；Loadout默认8格初始化；Open时作为单点初始化入口；修复CardDatabase查找路径
- **修改文件**：
  - 修改 `Assets/Scripts/Inventory/InventorySystem.cs`（InitializeTestStock增加每张卡非空检查+错误日志+Total=N输出 / EnsureTestStockIfEmpty增加自查找CardDatabase逻辑 FindObjectOfType+Resources.FindObjectsOfTypeAll）
  - 修改 `Assets/Scripts/Magazine/MagazineSystem.cs`（新增 `InitializeDefaultLoadoutIfEmpty(db)` — 从CardDatabase取4基础卡→默认8格Loadout→BuildRandomMagazine / 新增 `GetLoadedCards()` 公开属性）
  - 修改 `Assets/Scripts/UI/MagazineEditUI.cs`（Open()作为单点初始化入口：先 inventory.EnsureTestStockIfEmpty 再 magazine.InitializeDefaultLoadoutIfEmpty 再显示/暂停 / Refresh日志改为 OwnedTotal=N, OwnedEntries=N, Loadout=N）
  - 修改 `Assets/Scripts/Combat/PlayerController2D.cs`（简化Awake：仅创建+连接 inventorySystem/magazineEditUI/magazineSystem 引用，不做库存或Loadout初始化）
  - 更新 `SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`、`TODO.md`
- **新增函数**：
  - MagazineSystem: `InitializeDefaultLoadoutIfEmpty(CardDatabase)`, `GetLoadedCards()`
- **修改函数**：
  - InventorySystem.EnsureTestStockIfEmpty: 增加自查找CardDatabase回退逻辑
  - MagazineEditUI.Open: 增加初始化门 (EnsureTestStockIfEmpty + InitializeDefaultLoadoutIfEmpty)
  - MagazineEditUI.Refresh: 日志改为 OwnedTotal/OwnedEntries/Loadout 三字段
- **删除**：PlayerController2D 中库存初始化+默认Loadout初始化逻辑（移至MagazineEditUI.Open）
- **架构变更**：
  - 初始化入口：PlayerController2D.Awake → 仅连接组件引用
  - 首次按B → MagazineEditUI.Open() → EnsureTestStockIfEmpty + InitializeDefaultLoadoutIfEmpty → 显示
  - 后续Open时，库存和Loadout已存在，跳过初始化直接Refresh
- **预期Console日志**（首次按B）：
  ```
  [Inventory] Test stock initialized: Strike=20, Guard=20, Heal=20, Focus=20, Total=80
  [Magazine] Default loadout initialized. Count=8
  [Magazine] Random loaded from loadout: Strike, Guard, Heal, ...
  [MagazineEditUI] Refresh. OwnedTotal=80, OwnedEntries=4, Loadout=8
  [MagazineEditUI] Open bag panel.
  ```
- **测试步骤**：
  1. `Tools > Cardwin > Rebuild Card Database` → Console: Count=4
  2. Play → 战斗HUD 3发预览
  3. 按B → 左侧: Strike x20, Guard x20, Heal x20, Focus x20；右侧: 8格默认Loadout
  4. Console: Total=80, Entries=4, Loadout=8（不再是 OwnedCards=8, Loadout=0）
  5. 点击加减卡正常
  6. 关闭背包后射击正常
- **下一步**：Continue Stage 7A — Inventory / Magazine Editing stabilization

---
- **用户需求**：修复 Safe Mode 编译错误 CS0246 — PlayerController2D.cs 缺少 `using System.Collections.Generic`（`List<CardData>` 无法解析）
- **修改文件**：
  - `Assets/Scripts/Combat/PlayerController2D.cs`（顶部添加 `using System.Collections.Generic;`）
  - 更新 `DEVELOPMENT_LOG.md`（本记录）
- **新增类**：无
- **新增函数**：无
- **Unity 挂载方式**：不适用
- **测试步骤**：
  1. Unity 打开项目，确认 Safe Mode 退出
  2. Console 无红色 Error
  3. Demo_Combat.unity 正常打开
- **已知问题**：无
- **下一步**：Continue Stage 7A — Inventory / Magazine Editing stabilization

---
### 2026-05-30 | Stage 7A.4 — Force Test Stock + Remove InitialCards Fallback
- **用户需求**：修复 OwnedTotal=8 不是 80；修复 ResolveSourcePool: using initialCards fallback；强制每次 Open 重置测试库存；禁止 initialCards 覆盖用户 Loadout
- **修改文件**：
  - 重写 `Assets/Scripts/Inventory/InventorySystem.cs`（新增 `ResetToTestStock(database)` 强制清空ownedCards+自查找CardDatabase+4种x20+`_testStockReset=true`+GetCount日志 / 移除InitializeTestStock / EnsureTestStockIfEmpty简化为if(!_testStockReset || count==0)→ResetToTestStock）
  - 修改 `Assets/Scripts/Magazine/MagazineSystem.cs`（新增 `_hasUserLoadoutInit` bool / `SetLoadoutCards`设flag=true / `InitializeDefaultLoadoutIfEmpty`成功时设flag=true / `ResolveSourcePool`：_hasUserLoadoutInit=true且Loadout空→返回空列表(不fallback) / `Start`拆分两种初始化路径(loadout vs initialCards回退) / 新增`BuildRandomMagazineFallback`仅Start未init时用initialCards）
  - 修改 `Assets/Scripts/UI/MagazineEditUI.cs`（Open每次强制`inventory.ResetToTestStock(db)`+`magazine.InitializeDefaultLoadoutIfEmpty(db)` / 新增 `FindCardDatabase()` 统一查找逻辑 / 移除逐个db查找重复代码）
  - 更新 `SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`
- **新增函数**：
  - InventorySystem: `ResetToTestStock(CardDatabase)`
  - MagazineSystem: `BuildRandomMagazineFallback()`
  - MagazineEditUI: `FindCardDatabase()`
- **新增字段**：
  - InventorySystem: `_testStockReset` (bool)
  - MagazineSystem: `_hasUserLoadoutInit` (bool)
- **删除**：InventorySystem.InitializeTestStock
- **关键修复**：
  - Open() 不再检查 `ownedCards.Count == 0`，改为无条件 `ResetToTestStock` — 每次打开背包强制重置为 80 张
  - `ResolveSourcePool` 在 `_hasUserLoadoutInit=true` 时：Loadout空→返回空列表→loadedCards清空→3发预览显示Empty；再也不会 fallback 到 initialCards
  - Start 中 initialCards 初始化只在未进入 loadout 模式时生效（兼容旧 Inspector 设置）
- **预期Console日志（首次按B）**：
  ```
  [Inventory] Test stock reset: Strike=20, Guard=20, Heal=20, Focus=20, Total=80
  [Magazine] Default loadout initialized. Count=8
  [Magazine] Random loaded from loadout: Strike, Guard, Heal, ...
  [MagazineEditUI] Refresh. OwnedTotal=80, OwnedEntries=4, Loadout=8
  [MagazineEditUI] Open bag panel.
  ```
- **不再出现的错误日志**：
  - `ResolveSourcePool: using initialCards. Count=8`
  - `OwnedTotal=8`
  - `Loadout=0`
- **下一步**：Continue Stage 7A — Inventory / Magazine Editing stabilization

---
### 2026-05-30 | Stage 7A.5 — Scene Component Pre-mount + Remove Runtime AddComponent
- **用户需求**：
  1. 修复 Demo_Combat 场景中 Player 缺少 InventorySystem/CardEffectExecutor、Canvas 缺少 MagazineEditUI 的问题
  2. 绑定所有 CardDatabase 和系统间引用
  3. 删除 PlayerController2D.Awake 中动态 AddComponent 创建核心系统的逻辑
- **修改文件**：
  - `Assets/Scripts/Combat/PlayerController2D.cs` — Awake() 重构
  - `Assets/Scenes/Demo_Combat.unity` — 组件挂载+引用绑定（通过 MCP）
  - `SYSTEM_INDEX.md` — 更新 PlayerController2D 条目
  - `DEVELOPMENT_LOG.md` — 本记录
  - `TODO.md` — 更新任务清单
- **新增类**：无
- **新增函数**：无
- **场景修改**：
  - Player 新增 `InventorySystem` + `CardEffectExecutor` 组件
  - Canvas 新增 `MagazineEditUI` 组件
  - 绑定:`InventorySystem.defaultDatabase`→`CardDatabase.asset`、`MagazineEditUI.cardDatabase`→`CardDatabase.asset`
  - 绑定:`PlayerController2D.magazineSystem`→Player.MagazineSystem、`PlayerController2D.inventorySystem`→Player.InventorySystem、`PlayerController2D.magazineEditUI`→Canvas.MagazineEditUI
  - 绑定:`MagazineEditUI.inventorySystem`→Player.InventorySystem、`MagazineEditUI.magazineSystem`→Player.MagazineSystem
  - 绑定:`MagazineSystem.cardExecutor`→Player.CardEffectExecutor
  - 未绑定:`MagazineEditUI._playerController`（private 字段，非序列化，由 Awake FindObjectOfType 运行时赋值）
- **代码修改要点**：
  - 删除 `Awake()` 中三处 `AddComponent`：CardEffectExecutor、InventorySystem、MagazineEditUI
  - 改为纯 `GetComponent`/`FindObjectOfType` 查找，缺失时 `Debug.LogError` 明确报错
  - 错误日志：
    - `[PlayerController2D] Missing CardEffectExecutor on Player.`
    - `[PlayerController2D] Missing MagazineSystem on Player.`
    - `[PlayerController2D] Missing InventorySystem on Player.`
    - `[PlayerController2D] Missing MagazineEditUI on Canvas.`
- **测试步骤**：
  1. 场景中确认 Player 有 PlayerController2D/Health/CardEffectExecutor/InventorySystem/MagazineSystem
  2. Canvas 有 CombatHUD/MagazineEditUI
  3. Inspector 引用全部绑定
  4. Play 模式无 Error（组件引用正确）
- **已知问题**：`MagazineEditUI._playerController` 为 private 非序列化字段，无法 Inspector 预绑定，由 `Awake() FindObjectOfType<PlayerController2D>()` 运行时赋值
- **下一步**：Continue Stage 7A — Inventory / Magazine Editing stabilization

---
### 2026-05-30 | Stage 7A.6 — Fix BagPanel Owned Cards UI Invisible
- **用户需求**：修复背包左侧 Owned Cards 区域不显示的问题
- **修改文件**：
  - `Assets/Scripts/UI/MagazineEditUI.cs` — 修复所有 Canvas 子物体创建方式
  - `SYSTEM_INDEX.md` — 更新标题
  - `DEVELOPMENT_LOG.md` — 本记录
- **根本原因**：`new GameObject("name")` 创建的是普通 Transform，在 Canvas 层级下不参与 UI 布局。`AddComponent<RectTransform>()` 在已有 Transform 的 GameObject 上可能静默失败。
- **修复方案**：
  - 所有 Canvas 子物体改为 `new GameObject("name", typeof(RectTransform))` — 直接从 RectTransform 创建
  - 影响 11 处：BagPanel、TitleText、OwnedTitle、OwnedCardsPanel、LoadoutTitle、LoadoutPanel、HintText、CloseButton、OwnedCardSlot_*、LoadoutSlot_*、EmptyHint
  - `EnsureUI` 中 OwnedCardsPanel 布局简化：移除 ScrollRect/Viewport/Content/Mask/ContentSizeFitter 复杂结构，GridLayoutGroup 直接挂在 OwnedCardsPanel 上
  - GridLayoutGroup 参数：cellSize=(150,60)、spacing=(10,10)、constraintCount=2、childAlignment=UpperLeft
  - OwnedCardsPanel anchors：anchorMin=(0,0.5)、anchorMax=(0,0.5)、pivot=(0,0.5)、anchoredPosition=(40,0)、sizeDelta=(360,360)
  - RefreshOwnedCards 新增 Debug.Log：`[MagazineEditUI] Created owned slot: Strike x20` 等 4 条
  - Slot 命名改为 `OwnedCardSlot_0` ~ `OwnedCardSlot_3`
- **验收标准**：
  1. Console 无红色 Error
  2. 按 B 打开背包，左侧显示 4 个按钮：Strike x20 / Guard x20 / Heal x20 / Focus x20
  3. Console 出现 4 条 `Created owned slot` 日志
  4. 点击卡牌 → Loadout 增加，库存减少
  5. 右侧 Loadout 显示不受影响
   6. 战斗 HUD 三发预览不受影响
- **下一步**：Continue Stage 7A — Inventory / Magazine Editing stabilization

---
### 2026-05-30 | Stage 7B — Loadout Edit Polish
- **用户需求**：打磨 Loadout 编辑体验：Apply/Cancel/Clear/AutoFill 按钮、编辑层独立于战斗、Loadout 数量显示、未 Apply 不影响战斗弹夹
- **修改文件**：
  - `Assets/Scripts/UI/MagazineEditUI.cs` — 编辑层重构 + 4 按钮 + Loadout 计数
  - `Assets/Scripts/Magazine/MagazineSystem.cs` — SetLoadoutCards 增加 rebuildImmediately 参数
  - `Assets/Scripts/Inventory/InventorySystem.cs` — 新增 SetOwnedCardsFromCounts
  - `SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`、`TODO.md`
- **新增字段**：
  - MagazineEditUI: `_editingLoadout` (List<CardData>)、`_editingOwnedCounts` (Dictionary<CardData,int>)、`_hasPendingChanges` (bool)、`_loadoutCountText` (Text)
- **新增函数**：
  - InventorySystem: `SetOwnedCardsFromCounts(Dictionary<CardData,int>)`
  - MagazineEditUI: `Apply()`、`CancelEdit()`、`ClearLoadout()`、`AutoFill()`、`CreateActionButton()`
  - MagazineSystem.SetLoadoutCards 新增 `bool rebuildImmediately = true` 参数
- **删除**：MagazineEditUI.FindCardDatabase()（不再需要，编辑层不再调用 ResetToTestStock）
- **关键规则**：
  - Open 时从 MagazineSystem.GetLoadoutCards() 拷贝 → _editingLoadout，从 InventorySystem.GetCardCounts() 拷贝 → _editingOwnedCounts
  - 点击 Owned Card / Loadout Slot 只修改编辑层，不立刻影响战斗
  - Apply：SetLoadoutCards(_editingLoadout, true) + SetOwnedCardsFromCounts → Close
  - Cancel / B / Esc：丢弃编辑层，不写回 → Close
  - Clear：_editingLoadout 全部返还 _editingOwnedCounts → Clear()
  - AutoFill：从 _editingOwnedCounts 随机抽卡补满 8 张
  - Loadout 为空 Apply 后：loadedCards 清空，战斗 HUD Empty，不 fallback
  - Loadout 标题 "Loadout N/8"，未保存追加 " *"
- **测试步骤**：
  1. Play → B 打开背包 → 4 按钮可见 + Loadout 计数
  2. 点击 Owned Card → 仅编辑态改变，战斗 HUD 不变
  3. Apply → 战斗 HUD 刷新；Cancel → 保持原态
  4. Clear → Apply → 战斗 HUD Empty
  5. Auto Fill → 随机补满 → Apply → 战斗生效
- **下一步**：Continue Stage 7 — Shop System or Polish

---
### 2026-05-30 | Stage 7B.1 — Reloading / Empty 禁止发射
- **用户需求**：修复换弹中或弹夹为空时仍可通过 fallback 发射测试卡牌的问题
- **修改范围**：记录同步型补录；对应代码已体现在 `PlayerController2D.cs` 与 `MagazineSystem.cs`
- **当前状态**：
  - `MagazineSystem` 提供 `HasUsableCurrentCard()` / `LoadedCount`
  - `MagazineSystem` 存在时，左键/右键不会 fallback 到 `testCard`
  - `Reloading` / `Empty` 状态禁止使用当前卡牌
- **验收状态**：核心功能已在代码中体现；本记录仅补齐 DEVELOPMENT_LOG 阶段链
- **后续注意事项**：后续若新增备用射击逻辑，必须保持 Reloading/Empty 阻断规则

---
### 2026-05-30 | Stage 7C — Large Bag Panel + Tabbed Inventory Framework
- **用户需求**：扩大背包面板、增加分页系统、修复按钮 CAAC 重叠、为未来翻页/融合/装备/预览预留结构
- **修改文件**：
  - `Assets/Scripts/UI/MagazineEditUI.cs` — 全面重构 EnsureUI + 分页系统
  - `SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`、`TODO.md`
- **新增类型**：`BagTab` 枚举 (Magazine/Inventory/Fusion/Equipment/Preview)
- **新增字段**：`_currentTab`、`_magazinePage`~`_previewPage` (5个)、`_bottomButtonRow`、`_tabButtons` (Dictionary)
- **新增函数**：`SwitchTab()`、`RefreshCurrentTab()`、`RefreshTabButtons()`、`RefreshPreviewPage()`、`CreateBagPanelBackground()`、`CreateTitleText()`、`CreateTabRow()`、`CreateTabButton()`、`CreateContentRoot()`、`CreateMagazinePage()`、`CreateInventoryPage()`、`CreateFusionPage()`、`CreateEquipmentPage()`、`CreatePreviewPage()`、`CreatePagePlaceholder()`、`CreateBottomButtonRow()`、`CreateHintText()`、`CreateReadOnlyCardSlot()`
- **UI 结构变更**：
  - BagPanel 尺寸：900x520 → 1180x680
  - 新增 Background（最底层黑底 alpha≥0.96）
  - 新增 TabRow（5 个分页按钮：Magazine/Inventory/Fusion/Equipment/Preview，当前高亮蓝青色）
  - ContentRoot (1100x500) 包含 5 个 Page，仅显示当前 Tab
  - MagazinePage：左侧 OwnedCardsPanel(500x460, cellSize 200x70) + 右侧 LoadoutPanel(500x460, cellSize 160x70) + LoadoutCount
  - InventoryPage：只读聚合显示所有 Owned Cards (Grid 3列)
  - FusionPage/EquipmentPage：占位文字 "coming later"
  - PreviewPage：只读显示当前 Loadout + loadedCards 前3发
  - BottomButtonRow：720x48, HorizontalLayoutGroup, spacing=18, childControlWidth/Height=false, childForceExpandWidth/Height=false — 彻底修复 CAAC 重叠
  - ButtonRow 仅在 Magazine 页显示，其他页隐藏
  - HintText 在按钮上方 24px 处，不覆盖按钮
  - 按钮 sizeDelta 120x36 → 150x40, fontSize 14 → 16
  - EnsureUI 改为 destroy-rebuild 模式（复用 BagPanel root）
- **按钮日志**：Clear/Auto Fill/Apply/Cancel 点击时输出对应日志
- **下一步**：Continue Stage 8 — Shop System

---
### 2026-05-30 | Stage 7C.1 — Fix Magazine Page Content Missing
- **用户需求**：修复 Stage 7C 后 Magazine 页内容区域空白的 Bug
- **修改文件**：`Assets/Scripts/UI/MagazineEditUI.cs`
- **根本原因**：`Destroy()` 在 Unity 中延迟执行（帧末销毁）。`EnsureUI()` 先用 `Destroy()` 清除旧子物体，然后立即创建同名新子物体。`GetContentRoot()` 使用 `Find("ContentRoot")` 找到了仍存活但已标记销毁的旧 ContentRoot，新建的 MagazinePage 等被挂载到这个即将销毁的旧节点下，帧末被连带销毁
- **修复方案**：
  1. `Destroy()` → `DestroyImmediate()`：子物体立即销毁，不存在新旧冲突
  2. `_contentRoot` 改为直接字段引用（`Transform _contentRoot`），不再使用 `Find("ContentRoot")`
  3. MagazinePage 添加 "Owned Cards" 和 "Loadout" 标题标签
  4. `SwitchTab()` 仅在 `_isOpen=true` 时刷新内容（避免 Start 时重复空刷新）
  5. `Open()` 中 `EnsureTestStockIfEmpty` 移到 `EnsureUI()` 之前，确保 InventoryPage 创建时能看到测试库存
   6. `Open()` 中 `Refresh()` 改为 `SwitchTab(BagTab.Magazine)`，确保 Magazine 页激活并刷新
- **下一步**：Continue Stage 8 — Shop System

---
### 2026-05-30 | Stage 7C.2 — Large Bag Panel Size Update
- **用户需求**：继续放大背包/弹夹编辑面板，提升 5 分页界面的可读性和按钮可点性
- **修改范围**：记录同步型补录；对应 UI 尺寸已体现在 `MagazineEditUI.cs`
- **当前状态**：
  - BagPanel 当前代码尺寸为 `1380x820`
  - ContentRoot 当前代码尺寸为 `1260x610`
  - Magazine 页左右面板当前代码尺寸为 `540x500`
  - Owned / Loadout cellSize 当前分别为 `210x80` / `190x80`
  - 底部按钮当前为 `170x42`
- **验收状态**：核心布局调整已在代码和 TODO 中体现；本记录补齐 DEVELOPMENT_LOG 阶段链
- **后续注意事项**：如 UI 尺寸继续变化，`SYSTEM_INDEX.md` 只记录当前稳定口径，具体数值以 `MagazineEditUI.cs` 为准

---
### 2026-05-30 | Stage 7D — Inventory Persistence During Play
- **用户需求**：背包在同一 Play 会话中保持库存编辑结果，打开背包不再反复重置测试库存
- **修改范围**：记录同步型补录；对应逻辑已体现在 `InventorySystem.cs`、`PlayerController2D.cs`、`MagazineEditUI.cs`
- **当前状态**：
  - `InitializeForRun()` 一次 Play 会话只初始化一次
  - `Open()` 读取当前库存，不再每次重置
  - `useTestStock` / `resetTestStockOnStart` 作为测试库存配置保留
  - `GetOwnedTotalCount()` 用于显示和调试库存数量
- **验收状态**：核心功能已在代码中体现；本记录补齐 DEVELOPMENT_LOG 阶段链
- **后续注意事项**：当前仍是 Play 会话内持久化，不等同于跨游戏启动的存档系统

---

### 2026-05-31 | Stage 8.0.1 — Safe Mode Compilation Fix (EnemyProjectile type mismatch)
- **用户需求**：修复 Unity Safe Mode 编译错误 CS1503 — Argument 1: cannot convert from 'float' to 'int' at `EnemyProjectile.cs:41`
- **修改文件**：
  - `Assets/Scripts/Enemies/EnemyProjectile.cs` — line 41: `TakeDamage(_damage)` → `TakeDamage(Mathf.RoundToInt(_damage))`
  - `Assets/Scripts/Combat/EnemyController.cs` — line 25: `public float projectileDamage = 6f` → `public int projectileDamage = 6`
- **根本原因**：`EnemyProjectile._damage` 和 `EnemyController.projectileDamage` 均为 `float`，但 `Health.TakeDamage()` 参数为 `int`，类型不匹配
- **修复方式**：最小改动 — `Mathf.RoundToInt` 转换 float→int；同步将 `projectileDamage` 改为 `int`（与 `contactDamage: int` 一致）
- **未修改文件**：Health.cs / PlayerController2D.cs / MagazineSystem.cs / InventorySystem.cs / 背包 UI / Demo_Combat.unity
- **下一步**：Continue Stage 8 — Shop System

---

### 2026-05-31 | Stage 8A.1 — Enemy Placement, Prefab Management, Collision and AI Fix
- **用户需求**：修复敌人管理/摆放/碰撞/AI/Prefab化，解决敌人与Player重合、远程敌人不攻击/不移动、敌人编辑模式不可见等问题
- **修改文件**：
  - 新增 `Assets/Scripts/Enemies/MeleeEnemyController.cs` + .meta
  - 新增 `Assets/Scripts/Enemies/RangedEnemyController.cs` + .meta
  - 修改 `Assets/Scripts/Enemies/EnemyProjectile.cs`（Init签名改为int damage + Rigidbody2D.velocity）
  - 修改 `Assets/Scripts/Core/DemoSceneRuntimeBootstrapper.cs`（支持新控制器）
  - 新增 `Assets/Prefabs/Enemies/MeleeEnemy.prefab` + .meta
  - 新增 `Assets/Prefabs/Enemies/RangedEnemy.prefab` + .meta
  - 新增 `Assets/Prefabs/Enemies/EnemyProjectile.prefab` + .meta
  - 修改 `Assets/Scenes/Demo_Combat.unity`（新增LevelRoot/Enemies + 6敌人）
- **新增类**：
  - `MeleeEnemyController`：patrol/chase/stopDistance(0.75)/attackRange(0.9)
  - `RangedEnemyController`：horizontal patrol/shoot/prefab ref with fallback
- **防重合方案**：stopDistance停止追击 + velocity.x=0 + attackRange攻击 + Player-Enemy层忽略碰撞
- **敌人布置**：Melee(18,-2.2)(42,-2.2)(58,-2.2) / Ranged(32,3.2)(45,3.0)(63,3.0)
- **下一步**：测试验证 → Stage 8B Shop System

---

### 2026-05-31 | Stage 8A.1b — Static Level Authoring Fix + Missing using / Argument Order
- **用户需求**：修复场景层级/敌人位置在地面上/编辑模式可见/LevelRoot完整结构，并修复 Safe Mode 编译错误
- **修改文件**：
  - 修改 `Assets/Scenes/Demo_Combat.unity` —
    - Ground 从 (30,1)@(0,-3) 扩展到 (90,1)@(35,-3)，覆盖 x=-10 到 x=80
    - MeleeEnemy y 从 -2.2 修正到 -2.0（站在地面顶部 y=-2.5）
    - 新增 LevelRoot/Platforms（含 Platform_Z4/5/6_High 灰色5x0.4平台）
    - 新增 LevelRoot/Props（空容器）
    - 新增 LevelRoot/FinishGate（空容器）
  - 修改 `Assets/Scripts/Enemies/EnemyProjectile.cs` — 添加 `using Cardwin.Combat;`
  - 修改 `Assets/Scripts/Enemies/MeleeEnemyController.cs` — 添加 `using Cardwin.Combat;`
  - 修改 `Assets/Scripts/Enemies/RangedEnemyController.cs` — 添加 `using Cardwin.Combat;`
  - 修改 `Assets/Scripts/Combat/EnemyController.cs` — Init 参数顺序修正
- **场景层级**：
  - LevelRoot → Platforms (Platform_Z4/5/6_High), Enemies (6 enemies), Props, FinishGate
  - Enemy_Test_OLD 保留为禁用状态
- **敌人位置**：
  - MeleeEnemy_01: (18, -2, 0) ✓ 在地面顶部
  - MeleeEnemy_02: (42, -2, 0) ✓ 在地面顶部
  - MeleeEnemy_03: (58, -2, 0) ✓ 在地面顶部
  - RangedEnemy_01: (32, 3.2, 0) ✓ 在Z4高台上方
  - RangedEnemy_02: (45, 3.0, 0) ✓ 在Z5高台附近
  - RangedEnemy_03: (63, 3.0, 0) ✓ 在Z6高台上方
- **所有远程敌人 enemyProjectilePrefab 已绑定**
- **根本原因**：新脚本缺失 Health namespace 引用；EnemyController 调用 EnemyProjectile.Init 参数顺序不匹配新签名
- **验证状态**：当前已通过编辑器空闲 / 无 C# 编译错误状态检查；Console 中一条 Error 为 MCP 工具读取根目录文档路径导致，不是项目脚本编译错误
- **下一步**：Stage 8A.2 — Level and Enemy Runtime Validation

---
### 2026-05-31 | Stage 8A.1c — Project Records Synchronization / 文档记录同步修复
- **用户需求**：只同步项目记录链，不修改游戏逻辑；明确当前场景、敌人、Prefab、卡牌资产命名、背包 UI 尺寸和后续验证任务
- **修改文件**：
  - `AGENTS.md` — 增加 Enemies 子系统；修正卡牌资产命名规则，保留当前 `Focus.asset` / `Guard.asset` / `Heal.asset` / `Strike.asset`
  - `SYSTEM_INDEX.md` — 更新到 Stage 8A.1c；新增 Enemies System / Combat Enemies 小节；记录 LevelRoot/Enemies、敌人脚本与 Prefab；修正 MagazineEditUI BagPanel 1380x820
  - `DEVELOPMENT_LOG.md` — 补录 Stage 7B.1 / 7C.2 / 7D；整理 Stage 8A.1b；追加本记录
  - `TODO.md` — 对齐已完成阶段链；下一阶段改为 Stage 8A.2 — Level and Enemy Runtime Validation
  - `UE5_REFERENCE_INDEX.md` — 将 `CardData_Strike0.asset` 等保留为 UE5 参考命名，并补充当前 Unity 实际资产名
- **未修改文件**：未修改 `Demo_Combat.unity`、任何 C# 脚本、Prefab、CardData / CardDatabase 资产，未重命名卡牌资产
- **当前状态**：
  - 文档记录链已对齐到 Stage 8A.1c
  - `SYSTEM_INDEX.md` 不再使用过期 1180x680 背包 UI 尺寸
  - Enemies 系统归属明确为 Combat 大系统下的敌人实现，实际目录为 `Assets/Scripts/Enemies/`
  - 当前基础卡牌资产名继续保留为 `Focus.asset` / `Guard.asset` / `Heal.asset` / `Strike.asset`
- **验收状态**：文档同步完成；未执行 PlayMode 行为测试，敌人攻击/远程射击/地形通路仍待 Stage 8A.2 验证
- **后续注意事项**：下一步应继续 Stage 8A.2 — Level and Enemy Runtime Validation，而不是 Shop System

---
### 2026-05-31 | Stage 8A.3 — Interrupt Recovery, Player Spawn/Jump, Enemy Attack and Projectile Visibility Fix
- **用户需求**：中断恢复检查；修复 Player 出生点、跳跃一直飞、近战敌人攻击/防重合、远程敌人射击、敌方子弹可见与命中；不做 Shop/Reward/Fusion/Equipment/存档，不重建 `Demo_Combat.unity`
- **修改文件**：
  - `Assets/Scripts/Combat/PlayerController2D.cs` — Rigidbody2D 安全恢复：Dynamic、gravityScale=3、移除 FreezePositionY；解锁和跳跃前兜底
  - `Assets/Scripts/Core/DemoSceneRuntimeBootstrapper.cs` — 新增 SpawnPoint_Player 放置逻辑；挂载到 `LevelRoot`；修复 GroundCheck 被误判为 Ground 的问题
  - `Assets/Scripts/Enemies/MeleeEnemyController.cs` — patrolDistance 默认 2.5；攻击范围按 `attackRange` 判定
  - `Assets/Scripts/Enemies/RangedEnemyController.cs` — prefab 为空时自动尝试绑定 `Assets/Prefabs/Enemies/EnemyProjectile.prefab`；发射日志带 direction；fallback 子弹改为 Dynamic
  - `Assets/Scripts/Enemies/EnemyProjectile.cs` — 可见 sprite 兜底、sortingOrder=150、Dynamic Rigidbody2D(gravity=0, Continuous)、Trigger+Overlap 双路径命中 Player/Ground
  - `Assets/Prefabs/Enemies/EnemyProjectile.prefab` — 保存可见 sprite、紫色、sortingOrder=150、scale=(0.45,0.20,1)、Dynamic Rigidbody2D
  - `Assets/Scenes/Demo_Combat.unity` — Player 与 SpawnPoint_Player 对齐到安全出生点；Player gravityScale=3；`LevelRoot` 挂载 `DemoSceneRuntimeBootstrapper`
  - `SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`、`TODO.md` — 更新 Stage 8A.3 记录
- **新增类**：无
- **新增函数**：
  - `PlayerController2D.EnsureRigidbodySetup()`
  - `DemoSceneRuntimeBootstrapper.PlacePlayerAtSpawn()`, `ResolveSpawnY()`
  - `RangedEnemyController.ResolveProjectilePrefab()`
  - `EnemyProjectile.CheckManualHit()`, `HandleHit()`, `EnsureVisibleProjectile()`, `CreateRuntimeSprite()`
- **Unity 挂载方式**：
  - `LevelRoot` 挂载 `DemoSceneRuntimeBootstrapper`
  - `Player` 保持 PlayerController2D/Health/MagazineSystem/InventorySystem/CardEffectExecutor，Rigidbody2D=Dynamic、gravityScale=3、FreezeRotation
  - `LevelRoot/Enemies` 下 3 个近战 + 3 个远程敌人保持编辑模式可见、可选中
  - 远程敌人 `enemyProjectilePrefab` 指向 `Assets/Prefabs/Enemies/EnemyProjectile.prefab`
- **测试步骤**：
  1. 中断恢复检查：Console 0 红色 C# Error；活动场景为 `Assets/Scenes/Demo_Combat.unity`；Play Mode 初始为 false
  2. Play Mode 验证：Player 从 SpawnPoint 附近出生，Rigidbody2D gravityScale=3，GroundCheck 无 Collider 且不再被设为 Ground
  3. 跳跃验证：调用 `Jump()` 后初速 `(0,13)`，脚本物理模拟中上升到 `y≈0.81` 后回落到 `y≈-1.60`，`IsGrounded()==true`
  4. 近战验证：`MeleeEnemy_01/02/03` 进入攻击距离后均把 Player HP 从 50 扣到 42
  5. 远程验证：`RangedEnemy_01/02/03` 均生成可见 EnemyProjectile，sortingOrder=150，scale=(0.45,0.20,1)，velocity=(-6,0)
  6. 敌方子弹验证：3 个远程敌人的 EnemyProjectile 均可通过命中路径把 Player HP 从 50 扣到 44
  7. 玩家子弹验证：`Projectile_Test.prefab` 可将近战敌人 HP 30→20、远程敌人 HP 20→10
- **已知问题**：
  - 本阶段验证以 MCP Play Mode + 运行时代码调用为主；Unity 编辑器在 MCP 下 Play frame 计时有时不连续推进，因此跳跃落地使用 `Physics2D.Simulate` 辅助确认
  - 关卡完整路线节奏、相机边界和终点闭环仍需下一阶段打磨
- **下一步**：Stage 8A.4 — Level Polish / Enemy Tuning

---

### 2026-05-31 | Stage 8A.4 — Bug Sweep: CardData, EnemyHP UI, Bootstrapper
- **遍历发现**：
  - Guard.asset/Heal.asset leftClickEffect=Damage → Block/Heal不生效
  - Bootstrapper 不在场景中 → 无Player/Enemy配置+空气墙未清理
  - 无敌人HP/Shield UI
- **修改**：
  - Guard.asset: leftClickEffect 1→2(Block)
  - Heal.asset: leftClickEffect 1→3(Heal), damage 5→0
  - 新增 EnemyHealthBarUI.cs (OnGUI: HP绿/黄/红条 + SH蓝条)
  - 场景重建：Ground(90宽) + LevelRoot(Bootstrapper) + 6敌人(EnemyHealthBarUI) + Platforms(3高台) + Props + FinishGate
- **下一步**：Play验证

---

### 2026-05-31 | Stage 8A.5 — Simplify Level & Enemy Architecture
- **用户需求**：去繁就简，地图/敌人统一为编辑模式静态实例；不依赖运行时生成/SceneBuilder/Spawner；修复玩家与近战敌人重合。
- **审计结果**：正式敌人实现保留 `MeleeEnemyController` / `RangedEnemyController` / `EnemyProjectile`；`EnemyController` 仅被旧 `Enemy_Test` 引用；未发现 EnemySpawner/LevelBuilder/SpawnManager 运行时生成正式敌人的脚本。
- **修改文件**：
  - `Assets/Scripts/Enemies/MeleeEnemyController.cs`：默认参数改为 patrolSpeed=1.4、chaseSpeed=2.0、attackRange=1.2、stopDistance=1.0。
  - `Assets/Scripts/Combat/Projectile.cs`：新增 `OnCollisionEnter2D`，Trigger/Collision 共同调用同一命中逻辑。
  - `Assets/Scenes/Demo_Combat.unity`：禁用 `Enemy_Test_OLD`，移除 `LevelRoot` 上的 `DemoSceneRuntimeBootstrapper` 组件引用；正式敌人 Collider 改为 Trigger，近战 Rigidbody2D 改为 Kinematic；修正正式敌人序列化参数。
  - `Assets/Prefabs/Enemies/MeleeEnemy.prefab` / `RangedEnemy.prefab`：同步 Kinematic/Trigger 默认值和 EnemyHealthBarUI。
- **空气墙处理**：CameraBounds/SpawnPoint 当前无 Collider；BossDoor_Placeholder Collider 为 Trigger；旧测试敌人禁用。
- **下一步**：Play Mode 实测与 Level Polish / Enemy Tuning。

---

### 2026-05-31 | Stage 8A.8 — Convert Enemy Placeholders To Real Edit-Mode Entities
- **用户需求**：敌人必须在编辑模式下是完整实体，不是运行时补出来的空壳。Scene 视图可见、Inspector 可编辑。
- **审计发现**：6 个正式敌人和 prefab 的 `SpriteRenderer.sprite` 均为 null、`sortingOrder=0`，编辑模式不可见。`MeleeEnemyController.EnsureVisual()` / `RangedEnemyController.EnsureVisual()` 和 `EnemyProjectile.EnsureVisibleProjectile()` 在 Awake 时运行时创建 sprite/AddComponent，违背编辑态可见原则。
- **修改文件**：
  - `Assets/Scripts/Enemies/MeleeEnemyController.cs` — 删除 `EnsureVisual()` 方法及 `Awake` 中的调用、`_spriteRenderer` 字段、`col.isTrigger = true` 赋值；flipX 改用局部 `GetComponent<SpriteRenderer>()`
  - `Assets/Scripts/Enemies/RangedEnemyController.cs` — 删除 `EnsureVisual()` / `FireFallback()` 方法、`_spriteRenderer` 字段、`Awake` 中 `EnsureVisual()` 调用；flipX 改用局部 `GetComponent<SpriteRenderer>()`；prefab 缺失时只报错不 fallback
  - `Assets/Scripts/Enemies/EnemyProjectile.cs` — 删除 `CreateRuntimeSprite()` 和运行时 AddComponent/Sprite.Create 兜底逻辑；缺失组件改为 Error Log 并 return；RB 空指针保护
  - `Assets/Scenes/Demo_Combat.unity` — 6 个敌人 `SpriteRenderer.sprite` 从 null 改为玩家 placeholder sprite (`6e68677f...`); `sortingOrder` 从 0 改为 20; 近战 `m_IsTrigger` 改为 0
  - `Assets/Prefabs/Enemies/MeleeEnemy.prefab` / `Assets/Prefabs/Enemies/RangedEnemy.prefab` — 同步 sprite/sortingOrder/IsTrigger
- **结果**：6 个敌人编辑模式即具备完整 SpriteRenderer(红色/紫色)+Rigidbody2D+BoxCollider2D+Health+AI Controller+EnemyHealthBarUI，不再依赖运行时造图
- **下一步**：Play Mode 实测验证

---

### 2026-05-31 | Stage 8A.9 — Flying Ranged Enemy Hitbox & Detection Tuning
- **用户需求**：修复玩家子弹打空中远程敌人无效（穿透），扩大远程敌人索敌/射击范围。
- **根因分析**：
  - `Projectile.HandleHit` 只查 `GetComponent<Health>()` 不查 parent，若 collider 在子物体则漏掉
  - 玩家子弹 `collisionDetectionMode=Discrete`、速度=9、scale=0.3→collider radius=0.12，高速小体极易穿透
  - RangedEnemy `shootRange=10`，但最近敌人距离≈15，始终不进入射击状态
- **修改文件**：
  - `Assets/Scripts/Combat/Projectile.cs` — Health 查找添加 `GetComponentInParent<Health>()` 回退；命中日志带上效果类型；无 Health 时输出调试日志
  - `Assets/Prefabs/Projectiles/Projectile_Test.prefab` — `m_CollisionDetection`: 0→1(Continuous); `m_Radius`: 0.4→0.5
  - `Assets/Scripts/Enemies/RangedEnemyController.cs` — `shootRange`: 10→16; 添加 `OnDrawGizmosSelected` (黄色=射击范围, 青色=巡逻范围)
  - `Assets/Scenes/Demo_Combat.unity` — 3 个 RangedEnemy `shootRange` 从 10→16
  - `Assets/Prefabs/Enemies/RangedEnemy.prefab` — 同步 `shootRange` 16
- **下一步**：Play Mode 验证

---

### 2026-05-31 | Stage 8A.10 — Remove Invisible Platforms Under Flying Enemies
- **用户需求**：Platform_Z4/5/6_High 不可视、有 solid collider、在 Ground 层，挡住玩家子弹打空中敌人。
- **审计结果**：三个平台均有 `SpriteRenderer.sprite=null`(不可见)、`BoxCollider2D.isTrigger=0`(实体阻挡)、`Layer=8`(Ground)。玩家子弹命中 Ground 层即销毁，无法穿过。
- **修改文件**：
  - `Assets/Scenes/Demo_Combat.unity` — `Platform_Z4_High/Z5_High/Z6_High` 的 `m_IsActive: 1→0`（禁用）
- **结果**：空中远程敌人下方不再有无形阻挡，玩家子弹可直达敌人。RangedEnemy 依赖 Kinematic/g=0 悬浮，不受影响。
- **下一步**：Play Mode 验证

---

### 2026-05-31 | Stage 9A — Player Good / Evil Attribute + Loadout Composition Rule
- **用户需求**：玩家加入善恶属性(Good=4 Evil=4)，Loadout 搭配时攻击性子弹数量必须等于 Evil。
- **修改文件**：
  - 新增 `Assets/Scripts/Combat/PlayerAlignment.cs` — Good/Evil 属性组件
  - 修改 `Assets/Scripts/Cards/CardData.cs` — 新增 `IsOffensive` 属性
  - 修改 `Assets/Scripts/UI/MagazineEditUI.cs` — Alignment 显示 + Apply 校验 + AutoFill 优先补攻击弹
  - `Assets/Scenes/Demo_Combat.unity` — Player 挂载 PlayerAlignment (Good=4, Evil=4)
- **规则**：Strike=攻击性；Guard/Heal/Focus=非攻击性；Apply 拦截 offensiveCount≠Evil 的 Loadout。
- **下一步**：Play Mode 验证

---

### 2026-05-31 | Stage 9B — Combo Rating System
- **用户需求**：屏幕右上角展示连击数和 D/C/B/A 评分；攻击弹左键/增益弹右键算正确使用加连击；5 秒超时清零。
- **修改文件**：
  - 新增 `Assets/Scripts/Combat/ComboRatingSystem.cs` — comboCount/comboTimer/rank 逻辑
  - 修改 `Assets/Scripts/Magazine/MagazineSystem.cs` — UseCurrentCardLeft/Right 返回 bool
  - 修改 `Assets/Scripts/Combat/PlayerController2D.cs` — 注册 combo; `_comboRating` 字段
  - 修改 `Assets/Scripts/UI/CombatHUD.cs` — 右上角 ComboText 显示
  - `Assets/Scenes/Demo_Combat.unity` — Player 挂载 ComboRatingSystem
- **规则**：Strike 左键=正确→+combo; Guard/Heal/Focus 右键=正确→+combo; 错误不加不重置; 5s 超时清零; 1-2→D, 3-5→C, 6-9→B, 10+→A
- **下一步**：Play Mode 验证

---
### 2026-06-01 | Stage 10C — Card Config Validator
- **用户需求**：新增卡牌配置合法性检查器，扫描所有 CardData 和 CardDatabase 是否有配置错误
- **修改文件**：
  - 新增 `Assets/Editor/Cardwin/CardConfigValidator.cs`
  - 修改 `SYSTEM_INDEX.md`（新增 Editor 条目）
  - 修改 `DEVELOPMENT_LOG.md`（本记录）
  - 修改 `TODO.md`（Stage 10C 完成）
- **新增类**：`CardConfigValidator` (static editor)
- **新增函数**：
  - `Validate()` — 菜单入口，执行全部检查
  - `ScanCardDataAssets()` — AssetDatabase.FindAssets 扫描所有 CardData
  - `CheckBasicFields()` — CardID/CardName/描述/Icon/GoodCost/EvilCost/Cooldown 检查
  - `CheckTypeAndUseTarget()` — Heal/Guard/Focus→Self, Strike/Pierce/Burst→Enemy 验证
  - `CheckGoodEvilCost()` — Self卡goodCost>0, Enemy卡evilCost>0, 混合/零消耗警告
  - `CheckIsOffensive()` — Damage效果与IsOffensive一致性, Self卡IsOffensive异常
  - `CheckEffectImplementation()` — 未实现效果(WknsM/QuickR/ComboS/AerialM)警告
  - `CheckNumericValues()` — Damage>50/Heal>50/Block>80/finalValue>3.0/百分比值/负数检查
  - `CheckCardDatabase()` — null/重复CardID/disabled卡/遗漏正式卡/旧重复资产
  - `CheckRewardPool()` — disabled/unimplemented/null卡进入奖励池检查
  - `CheckInventoryTestStock()` — 正式卡x20 预期库存检查
  - `GenerateReport()`, `SaveReport()`
- **菜单路径**：`Tools > Cardwin > Validate Card Configs`
- **报告输出**：`Assets/Data/CardImport/CardValidationReport.txt`
- **检查范围**：Assets/Data/Cards/ 下所有 CardData + CardDatabase.asset
- **限制**：本阶段只报告不自动修复；不修改场景/卡牌资产/游戏逻辑
- **下一步**：Stage 10C.1 — Auto-Fix Console (如有需要)

---
### 2026-06-01 | Stage 11A — Project Architecture Audit / 项目架构审计与脚本总表整理
- **用户需求**：完整扫描项目脚本、资产、场景；输出审计文档；标记 Active/Legacy/Stub；修复 Validate Card Configs 打不开问题（确认功能正常，非 bug 而是 UX 认知差异）
- **修改文件**：
  - 新增 PROJECT_SCRIPT_INDEX.md — 46 脚本总表
  - 新增 PROJECT_FUNCTION_INDEX.md — 函数级索引
  - 新增 CARDWIN_TOOLS_AUDIT.md — 6 菜单项审计
  - 新增 CARD_SYSTEM_AUDIT.md — 卡牌系统唯一性
  - 新增 ACTOR_ARCHITECTURE_AUDIT.md — 角色属性架构
  - 新增 ENEMY_SYSTEM_AUDIT.md — 敌人系统冗余
  - 新增 UI_SYSTEM_AUDIT.md — UI 系统审计
  - 新增 SCENE_STRUCTURE_AUDIT.md — 场景对象审计
  - 新增 CLEANUP_PLAN.md — 清理计划
  - 新增 README_PROJECT_OVERVIEW.md — 新人入门文档
  - 修改 SYSTEM_INDEX.md、DEVELOPMENT_LOG.md、TODO.md
- **审计结论**：
  - **脚本总数**：46 C# 文件（38 Runtime + 6 Editor + 2 data）
  - **Active**: 30 | **Stub**: 7 | **Legacy/Deprecated**: 5 | **Retained**: 2 | **Data Only**: 3
  - **Validate Card Configs**: 功能正常，MCP 执行有效，报告文件生成成功。用户"打不开"的感知是因为无窗口弹出（纯 Console + 文件输出）
  - **玩家和敌人**：不需要共同父类，组件组合方案正确（Health 共用组件）
  - **卡牌效果实现**：唯一入口（CardEffectExecutor），4 种已实现 + 4 种待实现
  - **最大冗余风险**：4 张旧卡资产与正式卡重复；EnemyController.cs / DemoSceneBootstrapper.cs 为 Legacy
  - **建议保留 Tools 菜单**：Card Library / Import CSV / Rebuild Database / Validate Configs
  - **建议废弃**：Rebuild Clean Demo Scene（已 stub） / Create Basic Card Assets（标记 Legacy）
  - **P0 清理建议**：从 CardDatabase 移除旧资产引用 / 删除 Enemy_Test_OLD / 删除禁用高台平台
- **限制**：本阶段未删除任何脚本和资产；未修改场景；未修改运行时逻辑
- **下一步**：根据 CLEANUP_PLAN.md 的 P0 优先级执行清理

---
### 2026-06-01 | Stage 11B — Safe Cleanup Pass
- **用户需求**：执行低风险清理：CardDatabase移除旧卡引用 / 删除Enemy_Test_OLD / 删除禁用高台平台 / 高风险Tools菜单移入Legacy
- **修改文件**：
  - Assets/Data/Cards/CardDatabase.asset — allCards 从 17→12（仅保留 C001~C012）
  - Assets/Scenes/Demo_Combat.unity — 删除 Enemy_Test_OLD / Platform_Z4_High / Platform_Z5_High / Platform_Z6_High
  - Assets/Editor/Cardwin/CardwinSceneBuilder.cs — MenuItem 从 Tools/Cardwin/ 移到 Tools/Cardwin/Legacy/
  - Assets/Editor/Cardwin/CardAssetCreator.cs — MenuItem 移到 Legacy 子菜单 + 添加二次确认弹窗
- **清理结果**：
  - CardDatabase: 12 正式卡 (C001~C012) — 移除 4 旧引用 + 1 null
  - 旧资产文件 (Strike/Guard/Heal/Focus.asset) 保留在磁盘但不进入 CardDatabase
  - 场景根对象: 13→12 (删除 Enemy_Test_OLD)
  - LevelRoot/Platforms: 3→0 (删除 3 个禁用高台平台)
  - Tools > Cardwin 主菜单: 4 安全工具保留 (Card Library / Import CSV / Rebuild DB / Validate Configs)
  - Tools > Cardwin > Legacy: 2 个旧工具移入 (Rebuild Scene / Create Basic Cards)
- **未删除**：EnemyController.cs / DemoSceneRuntimeBootstrapper.cs / 旧 card asset 文件 / Stub脚本 / 核心脚本
- **下一步**：PlayerController2D 组件重构 或 卡牌效果实现

---

### 2026-06-01 | Stage 11C — Post-Cleanup Regression Test
- **用户需求**：Stage 11B 清理后全功能回归测试，确认项目稳定可用
- **修改文件**：
  - 新增 `REGRESSION_TEST_REPORT.md`（回归测试报告）
  - 更新 `DEVELOPMENT_LOG.md`（本文档）
  - 更新 `TODO.md`
  - 更新 `SYSTEM_INDEX.md`
- **测试范围**：11 大项回归检查
  1. Unity 状态检查（MCP/Scene/Console/GameObjects）
  2. CardDatabase 回归（12 张 C001~C012，0 null，0 重复）
  3. Card Library / Validate 工具（Validate 执行通过，报告生成）
  4. 背包库存（12 种 × 20 张 = 240 总计）
  5. Good/Evil 装填规则（Loadout offensive=4=Evil → Apply 通过）
  6. 射击/卡牌效果（MagazineSystem 正常，Reload/Empty 阻挡）
  7. Combo 系统（ComboCount=0 初始，CalculateRank 存在）
  8. 敌人（6 个敌人，3 Melee + 3 Ranged，组件完整）
  9. Reward 三选一（Melee/Ranged 击杀均触发，TimeScale=0，选择后 +1 背包）
  10. 地图/空气墙（Spawn→Ground→Platforms→FinishGate 路径完整）
  11. Tools 菜单（4 安全主菜单 + 2 Legacy 子菜单）
- **测试结果**：
  - Console 红色 Error = 0
  - 所有 11 项 PASS
  - 66 个 cosmetic warning（缺描述/图标）— 不影响功能
  - 核心玩法闭环可运行（背包/卡牌/敌人/奖励/Combo）
- **未测试**：需手动交互的 UI 操作（B 键开背包/滚轮/点击/Combo UI 视觉/敌人动画）
- **结论**：项目可作为后续功能开发基线
- **下一步**：新功能开发或 PlayerController2D 重构

---

### 2026-06-01 | Stage 11D — Archive Legacy Card Assets
- **用户需求**：将旧 Strike/Guard/Heal/Focus.asset 从 Cards 根目录归档到 Legacy，避免误导用户
- **修改文件**：
  - 移动资产：Strike.asset → Assets/Data/Cards/Legacy/Strike.asset
  - 移动资产：Guard.asset → Assets/Data/Cards/Legacy/Guard.asset
  - 移动资产：Heal.asset → Assets/Data/Cards/Legacy/Heal.asset
  - 移动资产：Focus.asset → Assets/Data/Cards/Legacy/Focus.asset
  - 修改 `Assets/Editor/Cardwin/CardLibraryWindow.cs` — 增加 _legacyCards 列表 / _showLegacy 开关（默认 false）/ [Legacy] 标签 / Legacy 禁止编辑 / SyncCardDatabase 排除 Legacy
  - 修改 `Assets/Editor/Cardwin/CardConfigValidator.cs` — ScanCardDataAssets 排除 Legacy / 新增 ScanLegacyCardAssets / 新增 CheckLegacyAssets / Validate 调用新方法
  - 更新 `CLEANUP_PLAN.md` — 记录 Stage 11D 完成
  - 更新 `DEVELOPMENT_LOG.md`（本文档）
  - 更新 `SYSTEM_INDEX.md`
  - 更新 `TODO.md`
- **新增类**：无（仅修改现有类）
- **新增函数**：CardLibraryWindow（Refresh 重构 / FilteredCards 改为 getter / DrawLeftPanel+DrawRightPanel+DrawBottomBar+SyncCardDatabase 适配 Legacy） CardConfigValidator（ScanLegacyCardAssets / CheckLegacyAssets）
- **Unity 挂载方式**：无新挂载
- **测试步骤**：
  1. 验证 Assets/Data/Cards/ 根目录只剩 C001~C012 + CardDatabase
  2. 验证 Assets/Data/Cards/Legacy/ 有 4 个旧资产
  3. 运行 Validate Card Configs → scanned=12 legacy=4 errors=0
  4. CardDatabase 12 张正式卡，无 Legacy 引用
- **已知问题**：无
---

### 2026-06-01 | Stage 12B.1 — Fix Player Death State (Hide + Disable)
- **用户需求**：修复 Player 死亡后仍可行动/可见的问题
- **根因**：GameOverController 仅调用 SetInputLocked，未隐藏 Sprite、禁用 Collider、停止 Rigidbody
- **修改文件**：
  - `Assets/Scripts/Combat/PlayerController2D.cs` — 新增 `_isDead` 字段 / `SetDead(bool)` / Update 中 `_isDead` early return / FixedUpdate 中 `_isDead` zero velocity guard
  - `Assets/Scripts/UI/GameOverController.cs` — 新增 `Instance` 静态引用 + `HandlePlayerDeath()` 静态入口 + `TriggerGameOver()` / `OnPlayerDeath` 中 `SetDead(true)` 替代 `SetInputLocked(true)`
  - `Assets/Scripts/Combat/Health.cs` — `Die()` 中 Player 死亡直接调用 `GameOverController.HandlePlayerDeath()`
- **Player 死亡后禁用项**：SpriteRenderer.enabled=false / Collider2D.enabled=false / Rigidbody2D.simulated=false / Rigidbody2D.velocity=0 / _inputLocked=true / _isDead=true / Update/FixedUpdate early return
- **健康检查更新**：移除了 Health.cs 中的 AnyDeath 静态事件（改为直接调用），移除了 PlayerController2D 中的 ShowGameOver 辅助方法
- **测试**：Play Mode 中 TakeDamage(999) → [Health] Death target=Player → GameOverPanel 出现 → Sprite 隐藏 → Collider 禁用 → 无法移动
- **已知问题**：MCP execute_code 使用 cached 编译程序集，验证时需确保场景已保存并刷新
- **下一步**：新功能开发

---

### 2026-06-01 | Stage 12B — Player Death / Game Over / Retry Flow
- **用户需求**：Player HP<=0 → GameOverPanel / Retry / Load Save / Main Menu / Quit
- **修改文件**：
  - 新增 `Assets/Scripts/UI/GameOverController.cs` — 监听 Health.OnDeath / IsGameOver 静态标记 / Retry/LoadSave/MainMenu/Quit 逻辑
  - 修改 `Assets/Scripts/Combat/Health.cs` — Die() 中 Player 不 Destroy (检查 tag==Player)
  - 修改 `Assets/Scripts/Core/GameFlowManager.cs` — 新增 RetryCurrentScene()
  - 修改 `Assets/Scripts/UI/PauseMenuController.cs` — Update 中检查 GameOverController.IsGameOver 阻止 Esc
  - 修改 `Assets/Scenes/Demo_Combat.unity` — Canvas 新增 GameOverPanel (Retry/LoadSave/MainMenu/Quit) + GameOverController
- **新增类**：GameOverController
- **新增函数**：GameOverController.Start/OnPlayerDeath/ShowGameOverPanel/UpdateLoadSaveButton/OnRetry/OnLoadSave, GameFlowManager.RetryCurrentScene
- **测试结果**：
  - Player HP<=0 → [Health] Death target=Player → [GameOver] Player died. → Show panel.
  - Death后: 输入锁定 / B键不打开背包 / Esc不打开Pause
  - Retry → Demo_Combat重载 / HP=50 / IsGameOver=False
  - Save(HP=42,Pos=20,2) → Kill → LoadSave → 恢复HP=42, Pos=(20,2) ✓
  - GameOver/Pause互斥 ✓
  - Console Error = 0
- **已知问题**：无
- **下一步**：新功能开发

---

### 2026-06-01 | Stage 12A (v3) — MainMenu Two-Panel + Save Select + Confirm
- **用户需求**：主界面精简为 New Game/Continue/Quit，Continue 跳转 SaveSelectPanel，新增 ConfirmPanel 确认删除/覆盖
- **修改文件**：
  - 重写 `Assets/Scripts/UI/MainMenuController.cs` — 两面板管理 + SaveSelectMode 枚举 + RequestConfirm 确认流程 + SetupSlot 按模式显示按钮
  - 重建 `Assets/Scenes/MainMenu.unity` — MainPanel (NewGame/Continue/Quit) + SaveSelectPanel (3槽+Back) + ConfirmPanel (Message/Confirm/Cancel)
- **MainMenu 结构**：
  - MainPanel：Cardwin标题 / New Game / Continue / Quit
  - SaveSelectPanel：Select Save Slot标题 / Slot1-3行(Continue/Overwrite/NewGame+Delete) / Back
  - ConfirmPanel：MessageText / Confirm / Cancel
- **存档槽按钮规则**：
  - Continue模式：空槽Continue禁用 / 有存档Continue+Delete可用
  - NewGame模式：空槽New Game可用 / 有存档Overwrite+Delete可用(Overwrite需确认)
- **测试结果**：
  - MainPanel 显示 New Game/Continue/Quit，不直接显示 3 个槽
  - Continue 无存档时置灰
  - Continue → SaveSelectPanel，显示存档摘要
  - New Game → SaveSelectPanel(NewGame模式)，空槽显示 New Game
  - Back 返回主界面
  - Delete 弹 ConfirmPanel 确认
  - Overwrite 弹 ConfirmPanel 确认
  - PausePanel 显示 Current Slot: X
  - Console Error = 0
- **已知问题**：无
- **下一步**：新功能开发

---

### 2026-06-01 | Stage 12A (v2) — MainMenu Scene + 3 Save Slots + Pause Save System
- **用户需求**：升级单存档为三存档槽，每槽支持 New/Continue/Overwrite/Delete，互不覆盖
- **修改文件**：
  - 修改 `Assets/Scripts/Save/GameSaveData.cs` — 新增 slotIndex / savedAt / gameVersion
  - 新增 `Assets/Scripts/Save/SaveSlotInfo.cs` — 存档摘要数据结构
  - 重写 `Assets/Scripts/Save/SaveSystem.cs` — 多槽位支持，接口改为5个（GetSavePath/HasSave/Save/TryLoad/DeleteSave/GetAllSlotInfos），路径改为 cardwin_save_slot_X.json
  - 重写 `Assets/Scripts/Core/GameFlowManager.cs` — 新增 currentSlotIndex / NewGame(slot) / ContinueGame(slot) / OverwriteGame(slot) / DeleteSaveSlot(slot)
  - 重写 `Assets/Scripts/UI/MainMenuController.cs` — 3 槽 UI 管理 / Continue/Overwrite/Delete 按钮显隐 / 确认对话框
  - 修改 `Assets/Scripts/UI/PauseMenuController.cs` — 新增 currentSlotText / Save 显示 "Saved to Slot X"
  - 重建 `Assets/Scenes/MainMenu.unity` — 3 个槽行 + New/Continue/Overwrite/Delete 按钮 + Quit
  - 修改 `Assets/Scenes/Demo_Combat.unity` — PausePanel 新增 CurrentSlotText
- **存档路径**：`Application.persistentDataPath/cardwin_save_slot_1.json` ~ `slot_3.json`
- **测试结果**：
  - 3 空槽显示 Empty，New Game 可用，Continue/Overwrite/Delete 隐藏
  - Slot 1 New Game → Save → 文件 cardwin_save_slot_1.json 生成，Slot 2 不受影响
  - Slot 2 New Game → Save → cardwin_save_slot_2.json 独立
  - Continue Slot 1 → 恢复位置(15,0) + HP=40/50 + Inventory=240
  - Delete 有 EditorUtility.DisplayDialog 确认
  - Overwrite 有确认
  - Console Error = 0
- **已知问题**：无
- **下一步**：新功能开发

---

### 2026-06-01 | Stage 12A — MainMenu Scene + Pause Menu + Save & Continue
- **用户需求**：新增游戏外壳（主菜单/暂停菜单/单存档/保存继续）
- **修改文件**：
  - 新增 `Assets/Scripts/Save/GameSaveData.cs` — 存档数据结构
  - 新增 `Assets/Scripts/Save/SaveSystem.cs` — JSON 存档读写系统
  - 新增 `Assets/Scripts/Core/GameFlowManager.cs` — 全局流程管理（NewGame/Continue/Save/ReturnToMainMenu/Quit/ApplySave）
  - 新增 `Assets/Scripts/UI/MainMenuController.cs` — 主菜单 UI 控制
  - 新增 `Assets/Scripts/UI/PauseMenuController.cs` — 暂停菜单（Esc/PausePanel/Resume/Save/MainMenu/Quit）
  - 修改 `Assets/Scripts/UI/MagazineEditUI.cs` — 新增 public IsOpen 属性
  - 新增 `Assets/Scenes/MainMenu.unity` — 主菜单场景
  - 修改 `Assets/Scenes/Demo_Combat.unity` — Canvas 下新增 PausePanel + PauseMenuController
- **新增类**：GameSaveData, CardStackSaveData, SaveSystem, GameFlowManager, MainMenuController, PauseMenuController
- **新增函数**：SaveSystem.HasSave/Save/TryLoad/DeleteSave/GetSavePath, GameFlowManager.Instance/NewGame/ContinueGame/SaveCurrentGame/ReturnToMainMenu/QuitGame/ApplySaveAfterSceneLoaded, PauseMenuController.TogglePause/OnResume/OnSave/HidePausePanel
- **Unity 挂载方式**：
  - MainMenu.unity: Canvas 挂载 MainMenuController + GameFlowManager
  - Demo_Combat.unity: Canvas 挂载 PauseMenuController，PausePanel 子物体
- **测试步骤**：
  1. MainMenu 场景显示标题+3 按钮，Continue 无存档时置灰
  2. New Game → 进入 Demo_Combat
  3. 按 Esc → PausePanel 出现，TimeScale=0，玩家输入锁定
  4. Save → hintText 显示 "Saved"，生成 cardwin_save.json
  5. Main Menu → 返回主菜单，Continue 可点击
  6. Continue → 进入 Demo_Combat，恢复位置/HP/Inventory/Loadout
  7. Quit → Editor 停止 Play
- **已知问题**：无
- **下一步**：新功能开发
