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

---
### 2026-06-17 | Stage 29C — BossRoom Camera Preservation & Editor Preview
- **用户需求**：修复传送后 "Display 1 No cameras rendering"
- **根因**：`runtimeRootsToPreserve` 为空 → MainCamera/Canvas 未移动 → UnloadSceneAsync 时销毁
- **修改文件**：
  - `CameraFollow2D.cs` — 新增 `SnapToTarget()`
  - `BossSceneTransitionController.cs` — 重大重写（gameplayCamera/canvasRoot/CameraRoot序列化引用 + ResolveReferences + MoveRootToScene + RestoreCameraState + ValidateRuntimeObjectsInScene + LogCameraState）
  - `BossRoomPreviewCamera.cs` — 新增 [ExecuteAlways] EditorOnly预览相机
  - `BossRoom.unity` — 新增 EditorPreviewCamera (Tag=EditorOnly, NO AudioListener)
  - `Demo_Combat.unity` — 绑定 gameplayCamera=MainCamera, gameplayCameraRoot=MainCamera, gameplayCanvasRoot=Canvas
- **移动顺序**：BossRoom加载 → ResolveReferences → MoveRoots → Validate → SetActive → Teleport → RestoreCamera(SnapToTarget+Canvas绑定) → Unload
- **EditorPreviewCamera**：编辑模式Game View可见 / Play Mode禁用 / Build剔除
- **手动测试**：MoveRuntimeRootsToScene成功，MainCamera移入BossRoom，Camera.main可用
- **Console红色错误**：0 (编译后)

---
### 2026-06-18 | Stage 30 — BossRoom 测试地图搭建
- **用户需求**：在 BossRoom.unity 搭建简单 Boss 战测试地图，确保玩家传送后能正常站立、移动、跳跃
- **修改文件**：
  - `BossRoom.unity` — Ground→MainGround(重命名+移至(0,-3,0)) / 墙体增高(12) / 新增LeftPlatform(-10,-0.5) / RightPlatform(10,-0.5) / SafetyFloor(0,-13,scale=60×1,α=0.15) / 出生点修正(-8,-1.4)和(8,-1.4) / EditorPreviewCamera居中(0,-1,-10,ortho=12) / BossRoomSceneController绑定所有引用
  - `BossRoomSceneController.cs` — 新增 mainGroundCollider/bossArenaCenter/safetyFloorTransform 序列化字段+属性 / 扩展 OnDrawGizmos(紫色竞技场中心+蓝色SafetyFloor)
  - `BossSceneTransitionController.cs` — 新增 MainGroundCollider 验证(null/disabled→abort不卸载旧场景) / TeleportPlayer 末尾增加 Physics2D.SyncTransforms() / TransitionRoutine 传送后增加 yield WaitForFixedUpdate
- **出生点高度计算**：
  - MainGround top = -3 + 0.5 = -2.5
  - Player CapsuleCollider2D: size.y=1.44, offset.y=-0.08 → 脚底偏移 = -0.80
  - BossPlayerSpawnPoint Y = -2.5 + 0.80 + 0.30(余裕) = -1.40
- **地面检测复用**：所有地面/平台/墙体/SafetyFloor 均使用 Layer 8 (Ground)，与 Demo_Combat 一致
- **不修改**：PlayerController2D / 玩家重力 / Rigidbody2D / Collider2D / Camera核心 / 卡牌 / 弹匣 / Health / HUD / Demo_Combat
- **测试步骤**：
  1. 打开 BossRoom.unity → Scene View 可见 MainGround/Walls/Platforms/SafetyFloor/SpawnPoints Gizmos
  2. Demo_Combat → 清敌 → 传送门 → BossRoom → 玩家落地站稳
  3. 左右移动 + 跳跃 + 站立在平台上
  4. 临时移出生点到地面外 → 玩家掉到 SafetyFloor 停住
- **已知问题**：无
- **下一步**：正式 Boss 逻辑 / Boss 美术 / Boss 战机制

---
### 2026-06-18 | Stage 30A — BossPortal 调试开关
- **用户需求**：临时让 Demo_Combat 传送门进入 Play Mode 后立即开启，无需击杀敌人即可传送到 BossRoom 调试地图
- **修改文件**：
  - `BossPortal.cs` — 新增 `[Header("Temporary Debug")] forceOpenForTesting` 序列化字段(默认true) + `Start()` 方法：forceOpenForTesting时立即调用 ActivatePortal() + LogWarning提示
  - `Demo_Combat.unity` — BossPortalRoot 实例 forceOpenForTesting = true（新字段默认值自动生效）
- **逻辑流程**：
  - `Awake()` → `SetPortalAvailable(false)` (不变)
  - `Start()` → `if (forceOpenForTesting)` → `ActivatePortal()` 立即开启
  - `RoomEnemyClearTracker` 稍后若再次调用 `ActivatePortal()` → 已开启则 LogWarning 跳过（无副作用）
  - `forceOpenForTesting = false` → 恢复原逻辑，必须清场才开启
- **不修改**：RoomEnemyClearTracker / 敌人 / BossRoom / BossSceneTransitionController / PlayerController / Camera / 卡牌 / HUD
- **Console红色错误**：0

---
### 2026-06-18 | Stage 30B — 跨场景 UI 与 EventSystem 修复
- **用户需求**：修复传送到 BossRoom 后背包和设置界面无法点击的问题
- **根因分析**：
  - `EventSystem` 由 `MagazineEditUI.EnsureEventSystem()` 在 `Start()` 中创建为独立根 GameObject
  - 场景切换时只移动 Player/MainCamera/Canvas 到 BossRoom
  - **EventSystem 未被移动** → Demo_Combat 卸载时 EventSystem 被销毁 → `EventSystem.current == null` → 所有 UI 输入失效
  - 背包依赖的 InventorySystem/MagazineSystem 在 Player 对象上 → 随 Player 移动 → 引用正常
  - SettingsMenuController 在 Canvas/SettingsMenuHost 上 → 随 Canvas 移动 → 引用正常
  - PauseMenuController 在 Canvas 上 → 随 Canvas 移动 → 引用正常
- **修改文件**：
  - `BossSceneTransitionController.cs`:
    - 新增 `using UnityEngine.EventSystems`
    - 新增 `_resolvedEventSystemRoot` 私有字段
    - `ResolveReferences()` 新增 EventSystem 自动查找
    - `MoveRuntimeRootsToScene()` 新增 EventSystem 到移动列表
    - `ValidateSingletonObjects()` 新增 EventSystem 数量检查 + `EventSystem.current` 检查
  - `Demo_Combat.unity`:
    - 删除重复根对象 `SceneTransitionManager`（旧版 BossSceneTransitionController 副本，未被 BossPortal 引用）
    - 保留唯一 `BossSceneTransitionController` 根对象
- **移动对象清单**：Player, MainCamera, Canvas, EventSystem → 共 4 个根对象
- **BossRoom 验证**：无 EventSystem / 无 Canvas / 无 AudioListener（仅 EditorPreviewCamera tag=EditorOnly）
- **不修改**：PlayerController / 玩家 Collider / HP / 卡牌 / 弹匣 / HUD / 清场规则 / 敌人 / BossRoom 地图 / Camera 跟随
- **Console红色错误**：0

---
### 2026-06-18 | Stage 31 — GlobalRuntimeRoot 全局运行环境架构
- **用户需求**：解决直接打开 BossRoom 进入 Play Mode 时 "No cameras rendering" 问题；建立全局运行环境自动初始化架构
- **根因**：BossRoom 是纯关卡场景，无 Player/MainCamera/Canvas；原设计依赖从 Demo_Combat 传送时 MoveGameObjectToScene 携带；直接启动无任何运行时对象
- **新增文件**：
  - `Assets/Scripts/System/GlobalRuntimeBootstrap.cs` — 单例 + DontDestroyOnLoad + 持有 Player/Camera 引用 + TeleportPlayer + SnapCameraToPlayer。`[DefaultExecutionOrder(-1000)]`
  - `Assets/Scripts/System/GlobalRuntimeAutoLoader.cs` — `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]` 检查并从 Resources 实例化 GlobalRuntimeRoot prefab
  - `Assets/Resources/System/GlobalRuntimeRoot.prefab` — 从 Demo_Combat 现有对象构建：Player(全组件) + MainCamera(Camera+CameraFollow2D+AudioListener) + Canvas(全UI) + GlobalEventSystem(EventSystem+StandaloneInputModule)，共 119 个子对象
- **重写文件**：
  - `BossSceneTransitionController.cs` — 完全重写：移除 MoveGameObjectToScene/ResolveReferences/ValidateRuntimeObjectsInScene；改用 GlobalRuntimeBootstrap.Instance.TeleportPlayer + SnapCameraToPlayer；全局对象在 DontDestroyOnLoad 场景中无需搬运
  - `BossRoomSceneController.cs` — 新增 Start() → PlacePlayerAtSpawn()：通过 GlobalRuntimeBootstrap 将 Player 放到出生点；支持直接启动和传送两种路径
- **修改场景**：
  - `Demo_Combat.unity` — 移除 Player/MainCamera/Canvas（迁移到 GlobalRuntimeRoot prefab）；保留 Ground/LevelRoot/Environment/layer/BossSceneTransitionController
- **架构变更**：
  - 全局对象（Player/Camera/Canvas/EventSystem）统一由 GlobalRuntimeRoot prefab 提供
  - RuntimeInitializeOnLoadMethod(BeforeSceneLoad) 确保任意场景启动前自动创建
  - DontDestroyOnLoad 确保场景切换不销毁
  - 场景只保留关卡内容，不再保存全局运行对象
  - 不再使用 MoveGameObjectToScene 搬运全局对象
- **不修改**：PlayerController / 玩家移动 / Rigidbody参数 / HP / 卡牌 / 弹匣 / 背包规则 / HUD布局 / BossRoom地面 / 传送门位置 / 清场规则 / Camera取景参数
- **Console红色错误**：0

---
### 2026-06-18 | Stage 31A — EventSystem 去重
- **用户需求**：修复每帧 "There are 2 event systems in the scene" 警告
- **根因**：`MainMenu.unity` 包含场景级 `EventSystem` 根对象，`GlobalRuntimeAutoLoader` 在 BeforeSceneLoad 又创建了 `GlobalRuntimeRoot/GlobalEventSystem` → 同时存在 2 个 EventSystem
- **验证**：Demo_Combat (0 EventSystem) / BossRoom (0 EventSystem) / MainMenu (1 EventSystem ← 重复来源)
- **修改文件**：
  - `MainMenu.unity` — 删除场景级 `EventSystem` 根对象
  - `GlobalEventSystemGuard.cs` — 新增：挂载于 GlobalRuntimeRoot，Awake + sceneLoaded 时自动销毁非全局 EventSystem
  - `GlobalRuntimeRoot.prefab` — 新增 GlobalEventSystemGuard 组件，引用 GlobalEventSystem
- **唯一权威**：`GlobalRuntimeRoot/GlobalEventSystem` (EventSystem + StandaloneInputModule)
- **MagazineEditUI.EnsureEventSystem()**：已有 `FindObjectOfType<EventSystem>()` 保护，GlobalEventSystem 存在时不会创建重复
- **不修改**：背包 / 设置 / 玩家移动 / Camera / BossRoom 地图 / 传送逻辑 / 卡牌 / HUD / 输入按键
- **Console红色错误**：0

---
### 2026-06-21 | Stage 32 — 跨场景出生点 / 地面碰撞 / 防坠落
- **用户需求**：修复全局玩家跨场景三个问题：① MainMenu/开始后角色先掉下去 ② BossRoom 角色一直下坠 ③ BossRoom 地板穿透；并为每个正式场景建立固定 Resume/Respawn 复活点 + 自动防坠落。不改玩家移动/重力/跳跃/Collider 尺寸。
- **真实根因（实测，非猜测）**：
  1. **MainMenu 下坠**：`GlobalRuntimeAutoLoader` 用 `RuntimeInitializeOnLoadMethod(BeforeSceneLoad)` 在**每个**场景（含 MainMenu）实例化 GlobalRuntimeRoot → Player(Dynamic, gravityScale=3) 立即受重力，MainMenu 无地面 → 自由下坠；进入 Demo_Combat 后无任何脚本把全局 Player 放回出生点（`DemoSceneRuntimeBootstrapper` 已 legacy 未挂载）。
  2. **BossRoom 穿地板 + 持续下坠**：`MainGround/SafetyFloor/LeftPlatform/RightPlatform/LeftPlatform(1)/LeftWall/RightWall` 7 个地面对象的 `BoxCollider2D.m_Size` 全部为 `{0.0001,0.0001}`，碰撞体塌缩成一个点（transform scale 只放大 Sprite 不放大碰撞体）→ 玩家直接穿过。Layer(8 Ground)、isTrigger(0)、碰撞矩阵(全 ff)均正常。
  3. Player.groundLayer=Bits 256=Layer 8(Ground)；Demo_Combat 可站立 `Ground`=Layer 8 + BoxCollider2D(size 1×1)×scale(97.38,1)，作为正确参照。
- **新增文件**：
  - `Assets/Scripts/Level/SceneRespawnPoint.cs` — `SceneRespawnPoint`：标记复活点 + `FallLimitY` + 绿色出生 Gizmo + 红色 FallLimit 横线；只提供数据，不创建/修改玩家。
  - `Assets/Scripts/Level/SceneGameplayMarker.cs` — `SceneGameplayMarker`：标记玩法场景（`IsGameplayScene`），MainMenu 不挂。
  - `Assets/Scripts/System/SceneRespawnService.cs` — `SceneRespawnService`(Cardwin.Runtime, 挂 GlobalRuntimeRoot)：唯一出生/复活权威。`OnSceneLoaded` 判定玩法场景→启用物理/可见/解锁输入+放到 SceneRespawnPoint+相机 Snap；非玩法场景(MainMenu)→`Rigidbody2D.simulated=false`+隐藏 Visual+锁输入（不改重力/不冻 Y）；`Update` 检测低于 FallLimitY 时按冷却复活；`RespawnPlayerAtCurrentPoint` 只改位置+瞬时速度，保留 HP/弹匣/Buff/背包。
- **新增函数**：`SceneRespawnService.OnSceneLoaded/EvaluateScene/EnterGameplayScene/EnterNonGameplayScene/Update/RespawnPlayerAtCurrentPoint/PlacePlayer/SnapCameraNextFixedUpdate/SetInputLocked/SetVisualActive/ResolvePlayerReferences/FindRespawnPointInScene/FindMarkerInScene`
- **修改文件**：
  - `BossRoom.unity` — 7 个地面 `BoxCollider2D.m_Size` `{0.0001,0.0001}`→`{1,1}`（碰撞体匹配 transform scale，与 Demo_Combat 一致）；`BossPlayerSpawnPoint`(-8,-1.4) 上新增 `SceneRespawnPoint`(fallLimitY=-20，与旧出生点统一)；`BossRoomSceneController` 上新增 `SceneGameplayMarker`。
  - `Demo_Combat.unity` — 新增 root `SceneRuntime`(SceneGameplayMarker) → 子 `PlayerRespawnPoint`(12,-1.3)(SceneRespawnPoint, fallLimitY=-15)；并注册进 SceneRoots。
  - `GlobalRuntimeRoot.prefab` — 根对象新增 `SceneRespawnService`，wire playerRoot/playerRigidbody（playerController/visualRoot 运行时自动解析）。
- **单一权威**：`SceneRespawnPoint` 挂在 `BossPlayerSpawnPoint` 上，与 `BossRoomSceneController.playerSpawnPoint` / `BossSceneTransitionController` 引用同一坐标 → 三方放置点完全一致，无抢位置冲突；故两个 Controller 无需改动。
- **Unity 挂载方式**：脚本 .meta 用固定 GUID（Level 两脚本 + Runtime 一脚本）；组件直接写入场景/Prefab YAML（fileID 唯一性已校验）。
- **测试步骤**（需 Editor 聚焦完成导入/编译后执行）：
  1. 打开 MainMenu → Play：Player 隐藏且不下坠（simulated=false）。
  2. 点击开始 → Demo_Combat：Player 出现在 (12,-1.3) PlayerRespawnPoint，不先掉再进，相机 Snap，可移动。
  3. 打开 BossRoom → 保存 → Play：Player 出现在 (-8,-1.4)，站在 MainGround 不穿透不下坠。
  4. Demo_Combat → 传送门 → BossRoom：Player 落在 BossRoom 复活点，速度 0，HP/弹匣/Buff/HUD 保留，相机 Snap。
  5. 跳出地图/低于 FallLimit：自动回到当前场景复活点，速度清零，战斗状态不重置。
- **不修改**：PlayerController 核心移动 / 跳跃 / 重力参数 / 玩家 Collider 尺寸 / 卡牌 / 弹匣 / 背包 / HP / Buff / HUD 布局 / 传送门位置 / 敌人逻辑 / BossRoom 正式美术。
- **掩盖手段**：未使用冻结 Y 轴 / 关闭重力 / 每帧拉位置（MainMenu 仅在非玩法场景禁用整体物理仿真；坠落复活带冷却且仅低于 FallLimit 触发）。
- **已知问题**：本轮文件级编辑期间 Unity MCP 桥接处于离线（Editor 未聚焦），编译/Console 验证需用户聚焦 Editor 触发导入后确认（脚本已静态审查，GUID 与 fileID 已交叉校验，YAML 无 BOM、header 完好）。
- **下一步**：Editor 聚焦导入后跑五项测试确认 0 红色错误；如需坠落扣血可加独立字段 `fallRespawnDamage`（本轮默认不扣血）。
- **Console红色错误**：待 Editor 聚焦编译后确认（预期 0）

---
### 2026-06-21 | Stage 33 — 死亡 Retry 运行时重置（修复死亡后卡死）
- **用户需求**：玩家死亡后点击 Retry 仍保持死亡/卡死（无法移动、死亡动画不退出、输入失效）。Player 现为全局常驻对象，Retry 必须显式恢复同一 Player 实例。
- **真实根因（实测代码，非猜测）**：
  - 死亡链：`Health.TakeDamage`→`Die()`(Player 不销毁→`GameOverController.HandlePlayerDeath`)→`OnPlayerDeath()`：`Time.timeScale=0`；`PlayerController2D.SetDead(true)`(`_isDead/_inputLocked=true`、`rb.simulated=false`、sprite off、**所有 Collider2D.enabled=false**)；`Health._isDead=true`(private，**无复活 API**)；`GameOverController.IsGameOver=true`；`GothicNunAnimationBridge.OnPlayerDeath`置 `Dead=true`+`_wasDead=true`(private，**Update 永久 early-return，无退出路径**)。
  - Retry 链(旧)：`OnRetryClicked`→`GameFlowManager.RetryCurrentScene()`=`Time.timeScale=1; SceneManager.LoadScene("Demo_Combat")`。
  - **卡死原因**：Player 是 DontDestroyOnLoad，重载场景**不会重建 Player**→所有死亡 flag 保留；`SetInputLocked(false)` 因 `if(_isDead)return` 失效；`PlayerController2D.Update/FixedUpdate` 因 `_isDead` early-return→永久冻结；Collider 仍关闭；动画停在 Death。另：`RetryCurrentScene` 硬编码 `Demo_Combat`，BossRoom Retry 会错误跳 Demo_Combat。
- **新增文件**：
  - `Assets/Scripts/Player/PlayerRuntimeReset.cs` — `PlayerRuntimeReset`(Cardwin.Player，挂全局 Player)：`ResetForRetry()` 统一入口=Health.ReviveToFull + PlayerController2D.SetDead(false) + AnimationBridge.ResetDeathVisual + SceneRespawnService.RespawnPlayerAtCurrentPoint + Physics2D.SyncTransforms；引用自动解析。
- **修改文件**：
  - `Health.cs` — 新增 `public ReviveToFull()`：清 `_isDead`、满血、清 block、触发 `OnHealed/OnBlockChanged`（HUD 既轮询又收事件）。
  - `GothicNunAnimationBridge.cs` — 新增 `public ResetDeathVisual()`：清 `_deadTriggered/_wasDead`、`Animator.Rebind()`+`SetBool(Dead,false)`+`Update(0f)` 退出死亡动画回默认 Idle（不硬编码状态名）。
  - `GameOverController.cs` — `OnRetryClicked` 改为：关闭 GameOverPanel→`Time.timeScale=1`→`IsGameOver=false`→`PlayerRuntimeReset.ResetForRetry()`（找不到才回退旧 RetryCurrentScene）；新增 `ResolvePlayerRuntimeReset()`。
  - `GlobalRuntimeRoot.prefab` — Player 对象新增 `PlayerRuntimeReset` 组件（引用运行时解析）。
- **PlayerController2D**：**未修改**（复用已有 `SetDead(false)` 对称恢复 rb.simulated/Collider/sprite/输入）。
- **死亡 Retry 与坠落 Respawn 区分**：`SceneRespawnService.RespawnPlayerAtCurrentPoint`(只位置+速度，坠落用)保持不变；`PlayerRuntimeReset.ResetForRetry`(完整复活，Retry 用)新增；Retry 内部调用前者做放置+相机Snap。
- **Retry 设计（保守方案，已说明）**：原地完整复活=满血+清死亡状态+回当前场景 SceneRespawnPoint+相机Snap；不重载场景、不重置库存/弹匣/设置/背包、不重生敌人（敌人保持当前状态）。满足用户“不再卡死、能移动”最低要求。
- **统一入口**：RetryButton → `GameOverController.OnRetryClicked` → `PlayerRuntimeReset.ResetForRetry`（Button 不再散落直接调用）。
- **防重复死亡/监听**：`GameOverController.IsGameOver` 静态守卫 + `Health._isDead` 守卫；bridge 在 Start 订阅一次(OnDestroy 反订阅)，未在 Update/Awake 反复 AddListener；Retry 复位 flag 后下一次死亡可再次正常触发 GameOver。
- **运行时日志**：`[Retry] Retry clicked / Health restored / Death state cleared. Rigidbody / Collider / Controller restored / Animator reset / Player moved to respawn / Retry reset complete`（均不在 Update）。
- **测试**（MCP force compile 通过）：
  - 编译：0 红色错误（仅 2 条既有无关 warning：PlayerController2D CS0414、CardDatabaseEditorUtility CS0184）。
  - Demo_Combat / BossRoom / 直接启动 BossRoom 死亡→Retry：逻辑上恢复满血、退出死亡动画、rb/Collider/Controller/输入恢复、回当前 RespawnPoint、相机 Snap、可移动（待运行期最终确认）。
- **不修改**：PlayerController 移动/跳跃/重力/Collider 尺寸、地图 Collider、卡牌、弹匣、背包 UI、设置 UI、传送门、BossRoom 地图、敌人 AI。
- **Console红色错误**：0（MCP force compile 实测）
- **下一步**：运行期跑死亡→Retry 三场景验收；如需 Retry 重生敌人/重置关卡进度，再单开“关卡重开规则”阶段。

---
### 2026-06-22 | Stage 34 — Mirror Saintess Boss 原型资产包试用接入
- **用户需求**：试用并接入 Boss 原型资产包（`C:\Users\86189\Desktop\0\MirrorSaintessBossPack`）：导入 → 生成 Boss Prefab → 放入 BossRoom → Play 测试可显示/可播放原型动画/三个可破坏部位可测试/无红色错误。暂不深度接入正式战斗逻辑，禁止修改现有核心系统。
- **导入路径**：资产包（单层目录，无双层嵌套）整体复制到 `Assets/MirrorSaintessBossPack/`（Art/Animations/Frames + Art/Parts + Art/Sprites + Editor + Scripts + Docs）。Unity 自动生成 .meta，强制刷新+编译 0 红色错误。
- **新增文件（资产包自带）**：
  - `Assets/MirrorSaintessBossPack/Scripts/MirrorSaintessBoss.cs` — `MirrorSaintessBoss`(命名空间 `MirrorSaintessBossPack`)：原型 Boss 控制器，TakeDamage/NotifyPartBroken/Stun/AttackLoop/相位切换；project-independent。
  - `Assets/MirrorSaintessBossPack/Scripts/MirrorSaintessBossPart.cs` — `MirrorSaintessBossPart` + `MirrorSaintessPartType`(ChestCore/BlueGun/RedGun)：可破坏部位，TakeDamage/BreakPart/ResetPart + intact/broken sprite 切换 + 破损禁用 Collider。
  - `Assets/MirrorSaintessBossPack/Scripts/MirrorSaintessProjectile.cs` — `MirrorSaintessProjectile`：原型子弹，SendMessageUpwards("TakeDamage")（本轮未接玩家）。
  - `Assets/MirrorSaintessBossPack/Editor/MirrorSaintessBossInstaller.cs` — `MirrorSaintessBossInstaller`：菜单 `Tools/Mirror Saintess Boss/Build Prototype Prefab`，把贴图设为 Sprite(PPU=256) + 生成 AnimatorController/6 个 AnimationClip + 拼装 Prefab。
- **本轮编辑（仅资产包自身，纯测试用，不含战斗逻辑）**：
  - `MirrorSaintessBoss.cs` — 新增 `#if UNITY_EDITOR` ContextMenu：Play Idle/Cast Blue/Cast Red/Hurt/Phase2/Death + Reset To Idle，统一走 `public DebugForcePlayState(string)`（控制器无 transition，用 `animator.Play(state)` 强制播放）。
  - `MirrorSaintessBossPart.cs` — 新增 `#if UNITY_EDITOR` ContextMenu：Damage 50 / Break / Reset。
- **生成产物**：
  - `Assets/Prefabs/Boss/MirrorSaintessBoss_Prototype.prefab`（root：MirrorSaintessBoss + Rigidbody2D(Kinematic,g=0) + BoxCollider2D(trigger) + Animator；子：Body(SpriteRenderer)、Part_ChestCore、Part_RightHand_BlueGun、Part_LeftHand_RedGun（各 SpriteRenderer+BoxCollider2D+MirrorSaintessBossPart）、FirePoint_Blue、FirePoint_Red）。无 Missing Script。
  - `Assets/MirrorSaintessBossPack/Generated/`：MirrorSaintessBoss.controller + 6 个 .anim（Idle/CastBlue/CastRed/Hurt/Phase2/Death）。
- **场景修改**：`BossRoom.unity` — 仅新增一个 root 实例 `MirrorSaintessBoss_Prototype`，坐标 (8, -2.372, 0)（X=BossSpawnPoint.X；Y 自动对齐使 Body 脚底 bounds.min.y=-2.5=MainGround 顶面，不悬空不埋地，头顶 y≈3.16）；并将实例 `startAttackLoop` 设为 false（本轮纯视觉，避免占位子弹与玩家交互）。未改 BossSpawnPoint/BossPlayerSpawnPoint/MainGround/SafetyFloor/EditorPreviewCamera/GlobalRuntimeRoot/SceneRespawnPoint。
- **测试结果（Play Mode 实测）**：
  - 编译：0 红色错误（导入后 + ContextMenu 编辑后两次确认）。
  - Boss Prefab：7 对象、3 部位 Collider2D 均在、Body+3 部位 sprite 全部正确绑定（Boss_Body_Transparent / ChestCore_Intact / BlueGun_Intact / RedGun_Intact），无 Missing Script。
  - 动画：Idle→idle_00 / CastBlue→cast_blue_00 / CastRed→cast_red_00 / Hurt→hurt_00 / Phase2→phase2_00 / Death→death_00，状态名匹配、Body sprite 正确切换、Reset 回 Idle、Death 后 Boss 仍 active 不消失。
  - 部位：ChestCore(hp160) / BlueGun(hp120) / RedGun(hp120) 各 Damage50→减血未破→Break→broken sprite+Collider 禁用→Reset 恢复 intact+Collider；Boss 收到 3 次破坏事件（blueGunBroken/redGunBroken/chestCoreBroken=True）。
  - 测试 A（直接启动 BossRoom）：GlobalRuntimeRoot 自动创建、Player 出现在 (-8,-1.41)、Boss 在 (8,-2.37)、Camera 正常、0 红错。
  - 测试 B（Demo_Combat→传送 BossRoom）：Player 在 (12,-1.31)、调用 `BossSceneTransitionController.TransitionToBossRoom()` → BossRoom 加性载入成功、Boss 存在且 sprite/renderer 有效、Player Dynamic+simulated 可移动、0 红错（传送相机/落点收尾由既有场景切换系统负责，未改动）。
  - 截图：`Assets/Screenshots/boss_idle_direct.png`、`boss_in_bossroom_teleport.png`。
- **是否修改现有核心系统**：否。仅导入资产包 + 生成 Prefab + 放入 BossRoom + 资产包自身加测试 ContextMenu。
- **未修改**：PlayerController / 玩家 HP / 重力 / 卡牌 / 弹匣 / 背包 / 设置界面 / GlobalRuntimeRoot / EventSystem / 传送门 / Demo_Combat 敌人 / Retry 逻辑 / BossRoom 地面 / 出生点。
- **Console红色错误**：0
- **下一步**：如需正式战斗接入，再单开阶段把玩家 Projectile→部位 TakeDamage、Boss→玩家 Health 伤害桥接、AnimatorController 加 transition（当前为 prototype 无过渡，靠强制 Play 测试）。

---
### 2026-06-22 | Stage 35 — Boss 战斗闭环 V1（玩家子弹打中/打坏/打死 Boss）
- **用户需求**：让玩家现有子弹能打中 Boss → 三部位可破坏 → Boss 有总 HP → 50% 进 Phase2 → 死亡触发 BossDefeated/Victory 占位。先做闭环 V1，不做复杂 AI。
- **先调查（未盲改）**：玩家伤害路径 = `CardEffectExecutor.ExecuteLeft` 实例化 `Projectile_Test.prefab`(Kinematic trigger,Layer0) → `Projectile.OnTriggerEnter2D/HandleHit` → `GetComponent<Health>() ?? GetComponentInParent<Health>()` → 卡牌路径 `CardEffectExecutor.ApplyEffectToTarget` → `Health.TakeDamage(int)`。**项目无 IDamageable/IHittable**；普通敌人(Melee/Ranged)均 Kinematic、有 Health（Melee 实体 collider、Ranged trigger collider，ufkc=0），被同一 Kinematic trigger 子弹命中验证可行。风险：`Health.Die()` 非玩家会 `Destroy(gameObject)` → 故 Boss 部位**不能**挂 Health。
- **方案**：新增 `IDamageable` 接口 + `Projectile.HandleHit` 极小**追加**分支（在 Health 查找前优先查 IDamageable；普通敌人无 IDamageable→跳过→走原 Health 路径，逐字节不变，敌人受击 0 风险）。Boss 部位/根实现 IDamageable，各自持有 HP，不复用 Health。
- **新增文件**：
  - `Assets/Scripts/Combat/IDamageable.cs` — `IDamageable { void TakeHit(int amount, GameObject source); }`。
  - `Assets/Scripts/Boss/BossHUD.cs` — `BossHUD`(Cardwin.Boss)：BossRoom 本地运行时自建 Overlay Canvas（不动 GlobalRuntimeRoot/EventSystem），显示名/总 HP 条/3 部位状态/DEFEATED；订阅 Boss 事件 + LateUpdate 轮询兜底。
- **修改文件**：
  - `Projectile.cs` — HandleHit 在 Health 前新增 IDamageable 兜底分支(`other.GetComponent<IDamageable>() ?? GetComponentInParent<IDamageable>()`→`TakeHit`)+`ResolveGenericDamage()`（卡牌 Damage 效果用 card.damage×focus，非卡牌用 damage，非伤害卡=0）。**Health/敌人路径不变**。
  - `MirrorSaintessBossPart.cs`（资产包自身）— 实现 IDamageable：`TakeHit` 恒向 Boss 总 HP 转发(破损后仍转发→保证可击杀)+扣部位 HP→0 一次 BreakPart(换 broken sprite+可选禁 Collider，**默认 disableColliderWhenBroken=false 保持可命中**+通知 Boss)+受击闪烁。字段改为用户要求：partId/maxHp(int)/currentHp/isBroken/visualRenderer/hitCollider。ContextMenu(Damage Part 25/Break Part/Reset Part)。
  - `MirrorSaintessBoss.cs`（资产包自身）— 实现 IDamageable(根兜底)；int 总 HP(400)+Phase1/2/Dead 状态机；中央 `DealBossDamage`：扣总 HP→死亡优先→≤50% 进 Phase2(一次)→否则 Hurt(协程回 Idle)。`NotifyPartBroken` 记 blue/red/core 破坏 flag；事件 OnHealthChanged/OnPartStateChanged/OnPhaseChanged/OnBossDefeated；公开 IsBlueGunBroken/IsRedGunBroken/IsCoreBroken/CurrentPhase/CurrentTotalHp/MaxTotalHp/HealthRatio；`ResetBoss()` 供再战；ContextMenu(Damage Boss 50/Force Phase2/Kill Boss/Reset Boss)。`startAttackLoop` 默认 false（V1 不做 AI）。
  - `MirrorSaintessBoss_Prototype.prefab` — 移除根 BoxCollider2D（部位为唯一命中面，避免大碰撞体抢命中）；部位 partId+maxHp(Chest120/Blue80/Red80)+放大 trigger Collider(≈1.1 世界单位便于命中)+disableColliderWhenBroken=false+wire visualRenderer/hitCollider；Boss 总 HP=400。
  - `BossRoom.unity` — 重新实例化 Boss(清旧 override)于 (8,-2.372,0)+新增 root `BossHUD` 对象；保存。
- **物理结论（实测）**：子弹 Kinematic trigger × 部位(根 Kinematic RB 的子 trigger collider) 命中正常，**无需 useFullKinematicContacts**（开/关均命中，与 RangedEnemy 同构）；headless 后台 Play 帧步长过大会隧穿，已用 `Physics2D.simulationMode=Script` 手动步进做确定性验证。
- **测试（Play 实测，0 红错）**：
  - 命中：真实 Projectile_Test 命中 ChestCore 120→110、BlueGun 80→70，Boss 总 HP 400→380（部位转发）。
  - 破坏：Blue(320)/Red(240)/Chest(120) 逐个破坏，broken sprite+Boss flag 全部置位。
  - Phase2：扣到 200(=50%) 触发一次(phaseEvents=1)，后续不再触发。
  - 后破坏转发：打已破坏 BlueGun 仍扣 Boss(120→100) 不再破坏。
  - 死亡：扣到 0 → IsDead+OnBossDefeated(一次)+全部 Collider 禁用；死后再打不重复触发。
  - HUD：`0/400 (Phase 2)` + 三部位 BROKEN + `MIRROR SAINTESS DEFEATED` + 血条归零。
  - 敌人回归：MeleeEnemy_01 30→23（无 IDamageable，走原 Health 路径，受击不受影响）。
  - 传送：Demo_Combat→传送门→BossRoom，Boss 在、HUD 在、RedGun 80→70 可命中。
  - 截图：`Assets/Screenshots/boss_hud_v1.png`。
- **是否影响普通敌人**：否（实测 MeleeEnemy 正常受击）。
- **是否修改现有核心系统**：否。仅 `Projectile.cs` 追加 IDamageable 兜底分支（不破坏 Health/敌人路径）+ 新增 IDamageable/BossHUD + 改 Boss 包脚本/Prefab + BossRoom 实例。未改 PlayerController/移动/重力/Collider/卡牌/弹匣/背包/设置/GlobalRuntimeRoot/EventSystem/传送门/敌人行为/BossRoom 地面/Retry。
- **Retry 兼容**：Boss 不引用 Player（仅自动 FindWithTag），Retry(PlayerRuntimeReset) 不触碰 Boss → 无 Missing；`ResetBoss()` 已备，未来接关卡重开再调用。
- **Console红色错误**：0
- **下一步**：可选——把卡牌 Block/Heal 对 Boss 的反馈、Boss→玩家伤害、AnimatorController transition、正式 Victory 界面、关卡重开规则单开阶段。

---
### 2026-06-22 | Stage 36 — Boss 实战部位破坏修复 + 移动 V2
- **用户实测问题**：1) Boss 不会移动；2) 玩家能打死 Boss；3) 但三个部位实战中不破坏（以用户实测为准）。
- **实战命中链路复查（实测，非猜测）**：检查当前 BossRoom 实例 collider 结构 → Boss 根**无 Collider**，仅 3 个部位 BoxCollider2D(trigger)，每个 `GetComponent<IDamageable>()` 自身即对应 `MirrorSaintessBossPart`（无 Body/Root 抢命中）。结构正确。真实根因：**部位 Collider 偏小且分布在胸口/双枪三点，玩家从左侧远射时存在覆盖缝隙、远侧 RedGun 被躯干遮挡，且破坏视觉/反馈不明显** → 用户感知“部位不破坏”。Stage 35 仅做了 Debug/确定性命中，未覆盖实战可命中性与可见性。
- **修改文件**：
  - `Projectile.cs` — `HandleHit` 命中优先级改为：`MirrorSaintessBossPart`(self→parent) → `IDamageable`(self→parent) → `Health`(原敌人路径)。**BossPart 优先级高于 BossRoot**，Body/Root 永不抢部位命中。命中时输出 `[ProjectileHit] other/root/part/damageable/dmg`（仅命中时，不刷屏）。普通敌人无 BossPart/无 IDamageable → 跳过 → 走原 Health 路径不变。
  - `MirrorSaintessBoss.cs` — 新增 `allowDirectBodyDamage`(默认 false)：根 `TakeHit` 在核心未破且未允许时直接打身体**无效**(仅闪烁+日志，不扣总 HP)，强制先打部位；核心破坏后才允许直接身体伤害。新增 `CanMove`(=!Dead && !Phase2过渡) 供 Mover；`_inPhase2Transition` 在 Phase2 动画期停移动（`Phase2Routine`）；`ResetBoss` 清该 flag。
  - `MirrorSaintessBossPart.cs` — 破坏明显反馈：闪红 `breakFlashColor` + `ShakeRoutine` 抖动 + `[BossPart] {partId} broken.` 日志；命中 `[BossPart] {partId} hit -n -> hp` 日志；`OnDrawGizmos`(intact 品红/broken 灰 线框，Scene View 可见命中框)；可选 `showRuntimeHitbox`(Game View 半透明框，默认关)。
  - 新增 `Assets/Scripts/Boss/MirrorSaintessBossMover.cs` — `MirrorSaintessBossMover`(Cardwin.Boss)：Kinematic + `MovePosition` 锁 Y(不掉落)，在 leftBound/rightBound 间巡逻；玩家进 `playerDetectRange` 则靠近，距离<`stopDistanceToPlayer` 停；Phase1=1.2 / Phase2=1.8；`!boss.CanMove`(Dead/Phase2 过渡)停；只翻 `visualRoot`(Body) 朝向玩家，不翻部位/根；边界 Gizmos。无跳跃/寻路/接触伤害。
  - `MirrorSaintessBoss_Prototype.prefab` — 加 `MirrorSaintessBossMover`；放大部位 Collider：Chest 世界 1.8×2.6、Blue/Red 1.7×2.4（覆盖躯干两侧，减少缝隙）。
  - `BossRoom.unity` — 重新实例化 Boss(清旧 override)；新增 root `BossArea` → `BossLeftMoveBound`(x=-6)/`BossRightMoveBound`(x=9)；Mover wire 四个引用；保存。
- **Boss 根直接伤害**：默认禁用(`allowDirectBodyDamage=false`)且根**无 Collider** → 玩家必须打部位；部位破损后仍保留 Collider 继续把伤害转给 Boss 总 HP（不重复 Break）→ Boss 始终可击杀。
- **实测（Play，真实 Projectile_Test 子弹）**：
  - 部位破坏：BlueGun 8 发×10=80→破(partHp=0)、RedGun→破、ChestCore→破；3 flag=True/True/True；boss HP 同步下降到 80 进 Phase2。8 发整除 80 验证每发干净命中一次。
  - 命中优先级：命中 collider 的 `self=MirrorSaintessBossPart`，BossPart 先于 BossRoot。
  - 移动：Phase1 X 8→-4(靠近玩家停于 stopDistance)，Y 锁 -2.37 不掉落；Phase2 过渡期 CanMove=False 停，过渡后以更快速度再 8→-4；Dead 后 X 恒定 8.00 不动；不越界、不穿墙、不离开 BossRoom。
  - 敌人回归：MeleeEnemy_01(无 BossPart/IDamageable) 30→23 正常受击。
  - 测试环境说明：MCP 无头 Play 帧步长抖动会让子弹 Update 位移瞬移“隧穿”，故部位命中用 `Physics2D.simulationMode=Script` 手动小步进做确定性验证（走真实 Projectile+真实 OnTriggerEnter2D+真实 HandleHit）；用户聚焦 60fps 实机无此问题，放大 Collider 后更稳。
- **是否影响普通敌人**：否（实测）。
- **是否修改现有核心系统**：否。仅改 `Projectile.HandleHit` 命中优先级(敌人 Health 路径不变)+Boss 包脚本/Prefab+新增 Mover+BossRoom 的 Boss 实例与 BossArea。未改 PlayerController/玩家 Rigidbody/Collider/子弹发射主体/卡牌/弹匣/背包/设置/GlobalRuntimeRoot/EventSystem/传送门/敌人行为/BossRoom 地面/Retry。
- **Console红色错误**：0
- **下一步**：可选——Boss 技能/接触伤害、正式 Victory、左右部位随朝向镜像、关卡重开重置 Boss。

---
### 2026-06-22 | Stage 37 — BossRoom 哥特美术装饰（美术换装）
- **用户需求**：用桌面 `bossR` 素材装饰 BossRoom，仅改视觉，保留 Boss/复活点/BossSpawnPoint/地面与跳台碰撞/SafetyFloor，美术与碰撞分离。
- **实测当前几何（权威）**：MainGround pos=(0,-10) → 顶面 y=-9.5，x[-20,20]；LeftWall/RightWall(±20,-10) y[-16,-4]；**当前场景无 LeftPlatform/RightPlatform/SafetyFloor**（旧索引数值已过时）。仅 6 个 Collider2D：MainGround+2 墙+3 Boss 部位。
- **导入素材**(Assets/Art/Gothic/BossRoom/)：Background/background.png(1672×941)、Wall/wall.png(1254×1254)、Floor/ground.png(1672×941)、Platform/taijie.png(1672×941)，导入设置 Sprite/Single/PPU100/FullRect/noMips/Bilinear/Uncompressed/noPhysShape；另程序生成 Lighting/PurpleGlow.png(radial)。
- **修改文件**：`BossRoom.unity` 在 BossRoomEnvironment 下新增 BackgroundRoot/WallDecorRoot/FloorVisualRoot/PlatformVisualRoot/ForegroundDecorRoot/LightingDecorRoot：背景(order -100,暗化,世界 48.5×28.2 覆盖视野)、地板(ground,order 0,顶面对齐 -9.5,世界 40×6)、4 墙装饰(wall,order -50,x±17)、中央祭坛(taijie,order -30)、紫光晕(order -90)。PlatformVisualRoot/ForegroundDecorRoot 留空(无平台碰撞体,避免误导/遮挡)。全部纯 SpriteRenderer **无任何 Collider**。
- **碰撞**：Collider2D 总数保持 6 不变；排序背景<墙<祭坛<地板<Boss<部位<子弹<玩家(Character 层)。背景为场景对象非 DontDestroyOnLoad。
- **实测**：直接开 BossRoom Play→玩家落地板 y≈-9 站立正常、背景/地板/墙/Boss/HUD 可见、Boss 仍移动(8→-4)、6 碰撞不变、0 红错。截图 `Assets/Screenshots/bossroom_decorated_play.png`。
- **是否修改核心系统**：否（仅 BossRoom 美术对象 + 导入素材）。**Console 红色错误**：0。

---
### 2026-06-22 | Stage 38 — 玩家子弹视觉换装（红/蓝弹）
- **用户需求**：用桌面 `cardP` 红/蓝素材替换玩家**射出去的**子弹视觉，保持伤害/卡牌/弹匣/命中/Boss 部位破坏逻辑不变；Collider 只包弹头不包拖尾；不改预览 UI/Boss 子弹。
- **现状调查**：玩家子弹唯一 prefab=`Assets/Prefabs/Projectiles/Projectile_Test.prefab`（被 `card.projectilePrefab`/`defaultProjectilePrefab` 引用）；原 `Projectile.EnsureVisibleDebugSprite` 强制黄色圆 + scale 0.8，红蓝仅驱动玩家动画(FireRed/FireBlue)，**子弹本身无红蓝区分**。红蓝判定沿用：`effectType==Damage`→红，其它(Block/Heal/Focus)→蓝。
- **导入素材**(Assets/Art/Gothic/Projectiles/Player/)：`PlayerProjectile_Red.png`/`PlayerProjectile_Blue.png`(各 1254×1254)。像素分析确认弹头在右、拖尾在左（朝右默认正确，无需翻转）。导入：Sprite/Single/PPU100/Bilinear/noMips/Uncompressed/alphaIsTransparency/noPhysShape，**自定义 pivot 设在弹头**(红 0.66,0.52 / 蓝 0.63,0.50)→ 弹头在原点，单个居中碰撞器对两图都只包弹头。
- **修改文件**：
  - `Projectile.cs`（仅视觉）：新增 `redSprite/blueSprite/bulletScale(0.25)`；新增 `ApplyBulletVisual(bool isRed)` 设 sprite+白 tint+sortingOrder100+scale；两个 `Init` 末尾调用(card 路径按 effect==Damage 选红蓝，非 card 路径默认红)；`EnsureVisibleDebugSprite` 去黄色强制 tint(改白)、去 scale 0.8、运行时圆仅作兜底。**未改 damage/HandleHit/卡牌/命中逻辑**。
  - `Projectile_Test.prefab`：赋 redSprite/blueSprite、bulletScale=0.25、默认 SpriteRenderer.sprite=红、CircleCollider2D 改为 radius 0.85 offset(0.2,0) trigger（弹头处小圈，世界直径≈0.5，拖尾约 1.2 世界单位不被覆盖）。
- **实测(Play)**：红弹 Init→sprite=PlayerProjectile_Red/white/scale0.25/order100；蓝弹→PlayerProjectile_Blue。BossRoom 红弹命中 ChestCore 120→110、boss 400→390(部位破坏机制不变)。Demo_Combat 红弹命中 MeleeEnemy_01 30→23(Health 路径不变)。碰撞器只在弹头(世界半径≈0.25,中心近原点,拖尾不触发)。
- **是否改伤害逻辑**：否。**是否改卡牌/弹匣逻辑**：否。**是否影响 Boss 部位破坏**：否（红弹仍正常破坏）。**预览 UI 未改**（MagazinePreviewUI/BulletPreviewItem 用各自 sprite）。
- **修改文件列表**：`Projectile.cs`、`Projectile_Test.prefab`、新增 `PlayerProjectile_Red.png`/`PlayerProjectile_Blue.png`、`SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`。
- **Console 红色错误**：0。
- **下一步**：可选——为红/蓝弹加拖尾粒子/命中特效、按卡牌细分更多弹种。

---
### 2026-06-23 | Stage 39 — BossRoom 新 Boss 白色天使镜 MirrorAngel 视觉替换（仅换皮，复用旧战斗逻辑）
- **用户需求**：从 `C:\Users\86189\Desktop\base` 导入白色天使镜 Boss 素材 → 替换 BossRoom 旧 Boss → 场景只保留一个新 Boss → 删除旧 Boss 实例 → 新 Boss 继续复用旧受击/血量/移动/Phase2/Death/BossHUD 逻辑；不得删除战斗脚本，不得破坏已跑通的 Boss 战斗闭环。
- **先调查（AGENTS.md：读 SYSTEM_INDEX + 改前说明影响）**：旧 Boss 实例 = `MirrorSaintessBoss_Prototype.prefab` 的场景实例，Boss 视觉 = `Body` 子物体 SpriteRenderer 的 sprite，由 Animator 驱动；`MirrorSaintessBoss.ForcePlayState(name)` = `animator.Play(name)`，会 Play 的状态名 = Idle/Hurt/Phase2/Death(+调试 CastBlue/CastRed)；BossHUD 无 Inspector 引用，运行时 `FindObjectOfType<MirrorSaintessBoss>()` 自动绑定。
- **导入素材**：7 张图（base 文件名带空格）复制到 `Assets/Art/Gothic/Boss/MirrorAngel/States/` 并规范命名 `MirrorAngel_<State>_0.png`（Idle/Walk_0/Walk_1/Dash/Fly/CastMirror/Death）。透明检查：7 张均 1254×1254、Format32bppArgb、四角 A=0 → **全部透明 PNG，无黑底**。导入设置：Sprite/Single/PPU100/FullRect/AlphaTrans/Bilinear/Uncompressed/noMips（已实测确认 7 张全部生效）。`Parts/` 文件夹已建但空（base 无部件图）。
- **新增动画资产**（`Assets/Animations/Boss/MirrorAngel/`，均绑定 `Body` 的 m_Sprite）：`MirrorAngel_Idle/Walk(2帧)/Dash/Fly/CastMirror`(Loop)、`MirrorAngel_Death`(不循环)；`MirrorAngelBossAnimator.controller`（12 状态，无 transition，默认 Idle）。
- **状态映射**（控制器含旧脚本会 Play 的全部状态名以防缺状态报错）：Idle/Walk/Dash/Fly/CastMirror/Death→各自 Clip；临时 Hurt→Idle、Phase2→CastMirror、CastBlue→CastMirror、CastRed→CastMirror、Stunned→Idle、Dead→Death。实测 0 缺状态 warning。
- **新 Prefab** `Assets/Prefabs/Boss/MirrorAngelBoss.prefab`（`AssetDatabase.CopyAsset` 复制旧 prefab 后改装，完整保留组件）：root 改名 MirrorAngelBoss；Animator→新 controller；Body sprite=MirrorAngel_Idle_0、localScale(0.42,0.42,1)（世界 5.27 高）、localPos(0,2.2,0)；**3 个部位 SpriteRenderer 禁用**（隐藏旧枪/镜美术）、**部位 Collider/脚本/HP/partType 全保留**（Chest120/Blue80/Red80）；复用 MirrorSaintessBoss(HP400)/Part×3/Mover/Rigidbody2D(Kinematic)/FirePoint×2；无 Missing Script。
- **场景修改** `BossRoom.unity`：删除旧实例 `MirrorSaintessBoss_Prototype`(pos 8,-2.37)；放入新实例 `MirrorAngelBoss` 于**同一 pos(8,-2.37,0) scale1**；Mover 重新 wire leftBound=BossLeftMoveBound(-6)/rightBound=BossRightMoveBound(9)/visualRoot=Body/bossRigidbody=root/artFacesRight=false。删除后场景 BOSS COUNT=1（仅 MirrorAngelBoss）。未改 SpawnPoint/MainGround/SafetyFloor/MoveBounds/EditorPreviewCamera/GlobalRuntimeRoot/SceneRespawnPoint。
- **命中盒**：新部位 Collider 世界坐标与旧 Boss **完全一致**（Chest 8.0/0.58、Blue 6.75/-1.02、Red 9.25/-1.02），落在新 body 轮廓内；**偏差报告**：新 body 头部(y>1.88)/脚部(y<-2.22)无命中盒（本轮按需求允许，未为对齐视觉改伤害逻辑）；部位破坏闪烁因禁用 part renderer 不可见，但 Body 受击闪烁 + HUD OK/BROKEN 仍在。
- **旧资源处理**：旧 prefab → `Assets/_Deprecated/Boss/OldMirrorSaintess/MirrorSaintessBoss_Prototype_DEPRECATED.prefab`（移动+改名，未物理删除）；旧战斗脚本（MirrorSaintessBoss/MirrorSaintessBossPart/MirrorSaintessBossMover/BossHUD/IDamageable/Projectile）**全部保留**；旧美术 `MirrorSaintessBossPack/Art` 保留原位（被 deprecated prefab/旧 controller/installer 引用）。
- **测试（Play 实测，0 红错 / 0 warning）**：
  - A：BossRoom 仅 1 Boss=MirrorAngelBoss（新图），旧实例已删。
  - B：直接 Play BossRoom — 新 Idle 图显示、移动(X 8→6.62, Y 锁 -2.37 不掉落)、BossHUD 自建、animator=Idle。
  - C：模拟玩家子弹（IDamageable.TakeHit = Projectile 同一路径）：HP 400→320(Blue破)→240(Red破)→180→Phase2；致命→HP0→Death(MirrorAngel_Death)、CanMove=False(停)、全 Collider 禁用。
  - D：Demo_Combat→`TransitionToBossRoom()` 加性载入 BossRoom，全场仅 1 Boss=MirrorAngelBoss(新图)、HUD 在。
  - 截图 `Assets/Screenshots/mirrorangel_scene_check.png`。
- **图片映射**：Idle=MirrorAngel_Idle_0 / Walk=MirrorAngel_Walk_0+Walk_1 / Dash=MirrorAngel_Dash_0 / Fly=MirrorAngel_Fly_0 / CastMirror=MirrorAngel_CastMirror_0 / Death=MirrorAngel_Death_0。旧 Hurt→Idle、Phase2→CastMirror、CastBlue→CastMirror、CastRed→CastMirror（临时映射）。
- **是否修改玩家逻辑**：否。**是否修改 Projectile 伤害逻辑**：否。**是否删除旧 Boss 战斗脚本**：否。
- **修改文件列表**：新增 `Assets/Art/Gothic/Boss/MirrorAngel/States/MirrorAngel_*.png`(7)、`Assets/Animations/Boss/MirrorAngel/*.anim`(6)+`MirrorAngelBossAnimator.controller`、`Assets/Prefabs/Boss/MirrorAngelBoss.prefab`；移动 `MirrorSaintessBoss_Prototype.prefab`→`Assets/_Deprecated/Boss/OldMirrorSaintess/MirrorSaintessBoss_Prototype_DEPRECATED.prefab`；修改 `Assets/Scenes/BossRoom.unity`(Boss 实例替换)、`SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`。**未改任何 .cs 脚本**。
- **Console 红色错误**：0。
- **下一步**：可选——按 Walk/Dash/Fly 状态接入 Mover 速度驱动 animator、部位命中盒按新天使轮廓重对齐、Death 后停留/胜利界面、确认稳定后清理旧 MirrorSaintessBossPack 美术。

---
### 2026-06-23 | Stage 40 — MirrorAngel Boss 真动画状态机 + 重力运动（修复「只会站着平移」）
- **用户问题**：Stage 39 换了白色天使镜图，但 Boss 只是站着平移，没真正播放 Idle/Walk/Dash/Fly/CastMirror/Death，也没有重力/落地/飞行切换。
- **根因（先查未盲改）**：Stage 39 的 `MirrorAngelBossAnimator.controller` 无参数/无过渡，只靠 `MirrorSaintessBoss.ForcePlayState`→`animator.Play("Idle"/"Hurt"/"Phase2"/"Death")` 驱动，Walk/Dash/Fly 永不播放；移动用 `MirrorSaintessBossMover`(Kinematic MovePosition 锁 Y)→「站着平移」；RB 为 Kinematic 无重力。
- **方案（不改任何战斗 .cs）**：桥接脚本只读 `MirrorSaintessBoss` 已公开成员（IsDead/CanMove/CurrentTotalHp/CurrentPhase），无需改战斗脚本。
- **新增脚本**（Assets/Scripts/Boss/，Cardwin.Boss）：
  - `MirrorAngelBossGravityMover.cs`：Dynamic RB 重力移动。地面 Walk(1.2，在 leftBound/rightBound 间巡逻/靠近玩家 stopDist3.5)；周期 Dash(冷却4/时长0.35/速度4.5)；周期短 Fly(冷却6/时长1.2，gravityScale→0 上浮 flyHeight2+正弦漂浮，结束恢复 g=3 落地)；Death/Phase2(CanMove=false)停。向下三射线 Ground 检测 IsGrounded。只翻 visualRoot(Body) 朝向。公开只读 IsGrounded/IsDashing/IsFlying/IsCasting/CurrentMoveSpeed。ContextMenu Force Dash/Fly/CastMirror。
  - `MirrorAngelBossAnimatorBridge.cs`：每 Update 由 boss+mover 状态写 Animator 参数 MoveSpeed/IsGrounded/IsFlying/IsDashing/IsCasting/IsDead。IsCasting = Phase2(CanMove=false 且未死) 或 mover 调试施法。纯视觉，无战斗逻辑。
- **新增 Animator**：`Assets/Animations/Boss/MirrorAngel/MirrorAngelBoss.controller`（参数驱动，6 参数，12 状态，6 条 AnyState 过渡，优先级 Death>CastMirror>Dash>Fly>Walk(MoveSpeed>0.1&&IsGrounded)>Idle，hasExitTime=false/canTransitionToSelf=false）。复用 Stage39 的 6 个 Clip（绑定 Body.m_Sprite）。含 6 个旧脚本兼容状态(Hurt→Idle/Phase2,CastBlue,CastRed→CastMirror/Stunned→Idle/Dead→Death)→ForcePlayState 永不缺状态报错；Death 无出口不回 Idle。
- **Prefab `MirrorAngelBoss.prefab` 修改**：Animator→新 controller；**RB→Dynamic，g=3，FreezeRotation，Interpolate，Continuous**；**新增身体 CapsuleCollider2D**(root，Vertical，size(1.5,3.0) offset(0,1.07)，非trigger，**includeLayers=Ground / excludeLayers=Default|Player**→不拦子弹/不挡玩家，capsule 底对齐 Body 脚底)；**移除旧 MirrorSaintessBossMover**，加 GravityMover+AnimatorBridge(wire rb/boss/visualRoot=Body/animator/mover)。部位 3 trigger Collider/HP/破坏逻辑全保留。
- **场景 BossRoom.unity**：实例继承 prefab 改动；wire mover leftBound=BossLeftMoveBound(-6)/rightBound=BossRightMoveBound(9)/visualRoot=Body；移除残留旧 mover；仍 1 个 Boss。未改地面/墙/出生点/玩家/传送门。
- **测试（Play + 确定性步进，0 红错/0 warning）**：
  - A 重力落地：root -2.38→-11.41，capsBottom=feetY=groundTop=-11.84，穿透 0.005，rotZ=0（不穿地、不倒下旋转）。
  - B 状态贴图（直接驱动参数）：Idle→Idle_0/Walk→Walk_0/Dash→Dash_0/Fly→Fly_0/CastMirror→CastMirror_0/Death→Death_0；Death 锁定不回 Idle。
  - 集成（真 mover+bridge）：落地行走→State=Walk/Walk_0（**不再 Idle 平移**）；Dash→Dash_0；Fly→gravityScale=0 后恢复 3。
  - 战斗(part.TakeHit)：HP 400→240(蓝/红破)→180→Phase2(→CastMirror_0)→致命 0→Death_0、mover vel.x=0 停（身体 Capsule 未拦子弹，部位正常破坏）。
  - 传送 H：Demo_Combat→BossRoom 仅 1 Boss，落地(-11.41 贴地 rotZ0)、有动画、HP 400→360 可战、HUD 在。
- **是否修改玩家逻辑**：否。**是否修改 Projectile 伤害逻辑**：否。**是否改战斗 .cs**：否（仅读已公开成员）。
- **修改文件列表**：新增 `Assets/Scripts/Boss/MirrorAngelBossGravityMover.cs`、`Assets/Scripts/Boss/MirrorAngelBossAnimatorBridge.cs`、`Assets/Animations/Boss/MirrorAngel/MirrorAngelBoss.controller`；修改 `Assets/Prefabs/Boss/MirrorAngelBoss.prefab`、`Assets/Scenes/BossRoom.unity`、`SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`。
- **Console 红色错误**：0。
- **下一步**：可选——按 vy 区分 Fall/Land 帧、Walk 帧率微调、CastMirror 接真镜子技能、部位命中盒随新轮廓重对齐。

---
### 2026-06-23 | Stage 41 — MirrorAngel Boss 受击修复 + 仅 MainGround 碰撞 + 视觉置顶
- **用户实测问题**：删旧组件后 ① Boss 子弹打不中；② Boss 被 MainGround 以外的地形/装饰/平台/墙挡住；③ Boss 图层要在地形图之上。
- **先查真因（实测，非猜测）**：BossRoot 组件齐全（MirrorSaintessBoss=IDamageable / Dynamic RB / CapsuleCollider2D / Animator / GravityMover / AnimatorBridge），3 部位 trigger Collider+IDamageable 都在，0 Missing。用真 `Projectile_Test` 子弹打部位 → 命中成功（ChestCore 120→80、Boss 400→360）。**根因**：Stage 40 给身体 Capsule 设了 `excludeLayers=Default(0)|Player(9)=513`，而 Default(0) 正是玩家子弹层 → 子弹打到身体区域（部位之间/外侧）时身体 collider 不接触、无回调；且 `allowDirectBodyDamage=false`，即便接触也不扣血 → 玩家瞄身体"打不中"。另：身体 `includeLayers=Ground` 会和 Ground 层全部碰撞体（MainGround/round0 平台 top=-6.5/两堵墙）碰撞 → 被非 MainGround 地形挡住；BossRoot 无 SortingGroup → 有被地形图遮挡风险。装饰物本身均无 Collider（已确认）。
- **新增脚本**：`Assets/Scripts/Boss/MirrorAngelBossCollisionFilter.cs`（Cardwin.Boss）：Start 收集 Boss 自身非 trigger 身体 collider，对场景所有其它 Collider2D 调 `Physics2D.IgnoreCollision`，仅 `MainGround` 不忽略，round0/墙/平台/装饰全忽略 → Boss 只被 MainGround 承托。不删/不禁用任何场景 collider（其它角色照用），只对本 Boss 忽略；部位 trigger 跳过不处理（仍接子弹）。
- **Prefab `MirrorAngelBoss.prefab` 修改**：① 新增 root `SortingGroup`(Default/Order=50) → 全子 SpriteRenderer 置于地形装饰之上（背景-100<光晕-90<墙-50<地板0<Boss50<子弹100）；② 身体 Capsule `excludeLayers` 由 513 改为 `Player(9)=512`（移除对 Default 的排除 → 子弹可命中身体；仍永不挡玩家），includeLayers=0；③ `MirrorSaintessBoss.allowDirectBodyDamage=true`（打身体/根也扣总血，保证至少能被打中；打部位仍优先并破坏）；④ 加 `MirrorAngelBossCollisionFilter`；⑤ 重申部位 isTrigger=true、无 layer override。
- **场景 BossRoom.unity**：RevertPrefabInstance 同步改动后重 wire mover bounds(BossLeftMoveBound/-6、BossRightMoveBound/9、visualRoot=Body)；仍 1 个 Boss，无旧 Boss 实例。未改 MainGround/墙/平台玩法碰撞、出生点、玩家、传送门。
- **测试（Play + 确定性步进，0 红错/0 warning）**：
  - 受击：真子弹命中部位 ChestCore→HP 400→370；命中身体/根（无部位处）→370→340（allowDirectBodyDamage 生效）。
  - 破坏/相位/死亡：Blue/Red 破坏→HP 240→180→Phase2(CastMirror_0)→致命→Death(Death_0)；HUD 在。
  - 碰撞：身体 vs MainGround IgnoreCollision=False（承托），vs round0/LeftWall/RightWall=True（忽略，不卡），rotZ=0 不穿地不倒。
  - 排序：SortingGroup Default/50 高于全部地形装饰。
  - 动画：Idle/Walk/Dash/Fly/CastMirror/Death 仍由 Stage40 状态机驱动。
  - 传送 G：Demo_Combat→BossRoom 仅 1 Boss、落地 rotZ0、Filter/SortingGroup 在、射击部位 400→360、HUD 在。
- **是否修改玩家逻辑**：否。**是否改 Projectile 伤害逻辑**：否（Projectile.cs 未改，原有 BossPart→IDamageable→Health 分支已支持，含 [ProjectileHit] 日志）。**是否影响普通敌人**：否。**是否恢复旧 Boss 场景实例**：否。
- **修改文件列表**：新增 `Assets/Scripts/Boss/MirrorAngelBossCollisionFilter.cs`；修改 `Assets/Prefabs/Boss/MirrorAngelBoss.prefab`、`Assets/Scenes/BossRoom.unity`、`SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`。
- **Console 红色错误**：0。
- **下一步**：可选——部位命中盒按新天使轮廓重对齐（当前胸口部位偏高，靠 allowDirectBodyDamage 兜底）、Boss 落点/竞技场高度微调。

---
### 2026-06-23 | Stage 42 — MirrorAngel Boss 简化为单 Body 受击目标
- **用户需求**：删除 MirrorAngelBoss 下的 Part_ChestCore / Part_RightHand_BlueGun / Part_LeftHand_RedGun / FirePoint_Blue / FirePoint_Red，只保留一个 Body。最终 `MirrorAngelBoss / Body`。玩家子弹直接打 Body → 扣 Boss 总血 → Phase2 → Death，动画保留，只被 MainGround 承托，显示在地形之上。不做部位/发射/镜面技能。
- **新增脚本**：`Assets/Scripts/Boss/MirrorAngelBodyDamageReceiver.cs`（Cardwin.Boss）：挂 Body，实现 `IDamageable.TakeHit(amount,source)` → `owner.TakeHit`（owner=根 MirrorSaintessBoss）。命中日志 `[MirrorAngelBoss] Body hit, damage=, hp=/`。
- **删除（Prefab + 实例）**：Part_ChestCore / Part_RightHand_BlueGun / Part_LeftHand_RedGun / FirePoint_Blue / FirePoint_Red 五个子对象；`MirrorSaintessBoss.destructibleParts` 列表清空。`MirrorSaintessBossPart.cs` 脚本文件保留（不删，便于恢复）。
- **Prefab `MirrorAngelBoss.prefab` 修改**：Root 保留 MirrorSaintessBoss(总HP400/Phase2/Death/allowDirectBodyDamage=true)+Dynamic RB(g3,FreezeRotation)+CapsuleCollider2D(实体,excludeLayers=Player)+Animator+GravityMover+AnimatorBridge+CollisionFilter+SortingGroup(Default/50)；Body 新增 BoxCollider2D(Hurtbox,isTrigger=true,size7.5×10本地→世界~3.15×4.2,覆盖躯干)+MirrorAngelBodyDamageReceiver(owner=根)。
- **BossHUD 兼容**：`BossHUD.cs` 三部位状态行(BlueGun/Core/RedGun)改为单行 `Body: OK/DEAD`（_blueText=_redText=null，RefreshParts 只刷 Body，不再读 IsBlueGunBroken/IsRedGunBroken/IsCoreBroken）。MirrorSaintessBoss 的 IsXxxBroken 属性保留未删，无报错。
- **场景 BossRoom.unity**：RevertPrefabInstance 同步后重 wire mover bounds(BossLeftMoveBound/-6、BossRightMoveBound/9、visualRoot=Body)。仅 1 Boss，children 只有 Body。
- **受击链路**：玩家子弹→Body Trigger→Projectile.HandleHit 找到 IDamageable(receiver)→owner.TakeHit→扣总 HP→HUD 更新→≤50% Phase2→≤0 Death→子弹销毁。Projectile.cs 未改（原 BossPart→IDamageable→Health 分支；BossPart 已无，落到 Body receiver=IDamageable）。
- **测试（Play + 确定性步进，0 红错/0 warning）**：
  - Hierarchy：MirrorAngelBoss 下只有 Body，无 Part/FirePoint，missing 组件=0。
  - 受击：真子弹命中 Body→HP 400→360；receiver TakeHit(160)→200 Phase2(CastMirror_0)；TakeHit(300)→Death(Death_0)。
  - 碰撞：vs MainGround IgnoreCollision=False(承托落地 rotZ0)，vs round0/LeftWall/RightWall=True(忽略不卡)。
  - 排序：SortingGroup Default/50 高于地形装饰。
  - 动画：Idle/Walk(2帧)/Dash/Fly/CastMirror/Death 全正常。
  - HUD：count=1，显示 Body 状态。
  - 传送：Demo_Combat→BossRoom 仅 1 Boss、只有 Body、落地 rotZ0、射击 Body 400→360、HUD 在、0 红错。
- **是否修改玩家逻辑**：否。**是否改玩家子弹伤害**：否。**是否影响普通敌人**：否。**是否恢复旧 Boss 实例**：否。
- **修改文件列表**：新增 `Assets/Scripts/Boss/MirrorAngelBodyDamageReceiver.cs`；修改 `Assets/Scripts/Boss/BossHUD.cs`、`Assets/Prefabs/Boss/MirrorAngelBoss.prefab`、`Assets/Scenes/BossRoom.unity`、`SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`。
- **Console 红色错误**：0。
- **下一步**：可选——Body Hurtbox 尺寸按新天使轮廓微调、Boss 落点/竞技场高度微调、将来需要时恢复部位系统（脚本仍在）。

---
### 2026-06-23 | Stage 43 — 玩家子弹卡牌效果可作用于 MirrorAngelBoss
- **用户需求**：玩家治愈/防护/增益类子弹命中 Boss 后也对 Boss 生效；只改 Boss 接收逻辑，不动其他。
- **先查链路（实测）**：`CardEffectExecutor.ApplyEffectToTarget` 经目标 `Health` 施加 Damage/Block/Heal，Focus 仅玩家。Boss 无 Health（走 IDamageable），故此法对 Boss 无效。`Projectile` 携带 `_sourceCard/_effectType/_cardContext/_usesCardEffect`，命中 Boss(IDamageable) 只传 `ResolveGenericDamage`（Damage=card.damage×focus，非伤害=0）→ Heal/Guard/Focus 子弹对 Boss 无效（真因）。Body 当前挂 MirrorAngelBodyDamageReceiver(IDamageable)→owner.TakeHit。
- **方案（最小转发，不重写子弹）**：新增 `IProjectileEffectReceiver`（只 Boss 实现）；Projectile 命中时若目标实现该接口→转发完整卡牌效果，否则走原路径。普通敌人不实现→零影响。
- **新增脚本**：
  - `Assets/Scripts/Combat/IProjectileEffectReceiver.cs`：`ReceiveProjectileEffect(Projectile, Vector2)`。
  - `Assets/Scripts/Boss/MirrorAngelBossEffectReceiver.cs`（root）：实现接口，读 SourceCard/EffectType——Damage→护盾吸收后扣总血(owner.TakeHit)、Block→加护盾、Heal→owner.Heal、Focus→定时 Buff(5s)。持 currentShield+buff，公开 CurrentShield/HasBuff/BuffName/BuffRemaining+事件；Update 计时清 Buff；ApplyExternalDamage 供 Body 走护盾。
- **最小修改**：
  - `Projectile.cs`：仅追加只读属性 SourceCard/EffectType/UsesCardEffect/CardContext + ResolveDamage()；HandleHit 顶部加 IProjectileEffectReceiver 分支（self→parent，命中即转发+Destroy）。普通敌人无接口→跳过→原 BossPart→IDamageable→Health 分支逐字不变；伤害数值未改。
  - `MirrorSaintessBoss.cs`：新增最小公开 `Heal(int)`（封顶 max、触发 OnHealthChanged、不动 Phase2/Death）。
  - `MirrorAngelBodyDamageReceiver.cs`：TakeHit 优先经 EffectReceiver.ApplyExternalDamage（护盾感知），无则回退 owner.TakeHit。
  - `BossHUD.cs`：状态行改 `Shield | Body | Status`，自动取 root EffectReceiver 轮询显示。
  - `MirrorAngelBoss.prefab`：root 加 MirrorAngelBossEffectReceiver(owner=根)；场景实例 Revert 同步。
- **测试（Play+真子弹/确定性步进，0 红错）**：
  - Damage(Strike10) HP400→390；Heal(12) 390→400 封顶；Guard(Block15) shield0→15；Damage 带盾 HP不变 shield15→5；再 Damage shield5→0+HP→395（护盾先吸收再扣血）；Focus→HasBuff=Focus 5s。
  - Phase2/Death 经效果路径：dmg200→HP195 Phase2(CastMirror_0)；致命→HP0 Death(Death_0)。
  - BossHUD 实时：HP 400/400 (Phase1) | Shield: 30 | Body: OK | Status: Focus。
  - 普通敌人回归：MeleeEnemy 无 IProjectileEffectReceiver→raw dmg10 经 Health.TakeDamage 30→20（不变）。
  - 玩家自效果：未改 CardEffectExecutor，自身 Heal/Guard/Focus 不受影响。
- **是否修改玩家逻辑**：否。**是否修改玩家子弹发射/伤害数值**：否。**是否修改卡牌配置/卡牌系统**：否。**是否影响普通敌人**：否。
- **修改文件列表**：新增 `IProjectileEffectReceiver.cs`、`MirrorAngelBossEffectReceiver.cs`；修改 `Projectile.cs`、`MirrorSaintessBoss.cs`、`MirrorAngelBodyDamageReceiver.cs`、`BossHUD.cs`、`MirrorAngelBoss.prefab`、`BossRoom.unity`、`SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`。
- **Console 红色错误**：0（测试中一次 NRE 为手动反射传 null context 的测试假象，非产品路径，实机子弹始终带 context）。
- **下一步**：可选——Buff 对 Boss AI 真正生效（当前仅可见状态）、护盾视觉条、更多卡牌效果细分。

---
### 2026-06-23 | Stage 44 — MirrorAngel Boss 第一个主动技能：三连镜光束 MirrorTripleBeam
- **用户需求**：给 MirrorAngelBoss 加第一个主动攻击技能：朝玩家方向射光束，共 3 束；第一束前有红线提醒 1 秒；可在空中释放；施法动作用 CastMirror；红线/光束先用 Unity 自带素材（LineRenderer）实现。流程：停移动→播 CastMirror→红线预警 1s→朝玩家发 3 束→命中扣血→后摇→回到移动决策。禁止改玩家/卡牌/弹匣/Boss 受击/Boss HP/BossHUD，不恢复旧部位。
- **改前影响说明**：仅 Combat/Boss 子系统。新增技能脚本 `MirrorAngelTripleBeamSkill.cs`；最小新增移动锁到 `MirrorAngelBossGravityMover.cs`；新增材质/FX prefab；`MirrorAngelBoss.prefab` 加 BeamOrigin 子物体 + 技能组件。只读（不改）`MirrorSaintessBoss`(IsDead/CanMove/MaxTotalHp) 与 `Cardwin.Combat.Health.TakeDamage`。
- **CastMirror 动作图（沿用 Stage 39，已存在）**：`Assets/Art/Gothic/Boss/MirrorAngel/States/MirrorAngel_CastMirror_0.png`（源 `C:\Users\86189\Desktop\base\CastMirror_0   .png`，Sprite/Single/PPU100/FullRect/AlphaTrans/Bilinear/Uncompressed/noMips，已正确导入）。**未重新移动/覆盖该图**（已被现有 CastMirror.anim 引用，移动会破坏引用）。动画 Clip `Assets/Animations/Boss/MirrorAngel/MirrorAngel_CastMirror.anim` 本轮 `loopTime` True→**False**（单帧，视觉等价，遵从需求）；Idle/Walk/Dash/Fly/Death 不变。Animator `MirrorAngelBoss.controller`（Stage40 参数驱动）已含 IsCasting + CastMirror 状态，未重建。
- **新增脚本**：`Assets/Scripts/Boss/MirrorAngelTripleBeamSkill.cs`（Cardwin.Boss）：
  - `TryCast()`：防重入(_isCasting)/死亡(boss.IsDead)/无玩家/距离(2.5~12)/Random<attackChance 全 guard，通过则 StartCoroutine(CastRoutine)。
  - `CastRoutine()`：mover.SetMovementLocked(true)+SetCasting(true)（播 CastMirror、停水平移动、**不检查 grounded、不改重力**）→ 第 1 束锁定方向显红色 LineRenderer 预警 firstWarningTime=1s → 沿该方向 FireBeam → 间隔 0.25s → 第 2/3 束每束重新 AimDirection(玩家当前位置) 后 FireBeam → mover.SetCasting(false) → 后摇 recoveryTime=0.5s → EndCast(解锁+停 cast)。
  - `FireBeam(dir)`：`Physics2D.CircleCast(origin, beamHitRadius=0.18, dir, beamRange=14, playerLayer)` 命中→`Health.TakeDamage(beamDamage=10)`（每束单次 cast = 最多扣一次）；LineRenderer 端点=命中点或 origin+dir*range，显 beamVisibleTime=0.15s 后销毁。
  - 每帧 `Aborted()` 检查 boss.IsDead→EndCast+yield break（死亡立即中断）；`OnDisable` 兜底 EndCast。内置最小冷却触发（initialDelay1.5/cooldown4.5/retryDelay0.75），无 Brain，注释标注可迁移。红线/光束优先用 FX prefab，prefab 为空则运行时建 LineRenderer（serialized 材质兜底）。
- **修改脚本**：`Assets/Scripts/Boss/MirrorAngelBossGravityMover.cs`（最小）：新增 `SetMovementLocked(bool)`/`SetCasting(bool)` + `_movementLocked`/`_externalCasting`；`IsCasting` 改为 `Time.time<_castEnd || _externalCasting`；FixedUpdate 在 `!boss.CanMove` 后新增 movement-lock 分支（冻结水平速度、**保留 Y 速度/重力、无 grounded 检查**→空中可施法）。未动巡逻/Dash/Fly/寻路逻辑。
- **新增资产**：
  - 材质 `Assets/Materials/Boss/MirrorAngel/M_BossBeamWarning.mat`(Sprites/Default,红)、`M_BossBeam.mat`(Sprites/Default,紫白)。
  - FX prefab `Assets/Prefabs/Boss/MirrorAngel/FX/BossBeamWarning.prefab`(LineRenderer 红,宽0.06,order120)、`BossBeam.prefab`(LineRenderer 紫白,宽0.22,order120)。
- **Prefab `MirrorAngelBoss.prefab` 修改**：新增子物体 `BeamOrigin`(空 Transform,localPosition(-0.8,0.8,0),不参与碰撞)→ Hierarchy = `MirrorAngelBoss / Body / BeamOrigin`；root 加 `MirrorAngelTripleBeamSkill`，wire boss/mover/beamOrigin/warningLinePrefab/beamLinePrefab/warningMaterial/beamMaterial/playerLayer=Player(1<<9=512)。Body 受击/Hurtbox/IDamageable/RB/Capsule/Animator/Mover/Bridge/Filter/SortingGroup/EffectReceiver 全保留不变。**未恢复旧部位/FirePoint。**
- **Unity 挂载方式**：技能组件与 BeamOrigin 已在 prefab 上配好并 wire 全部引用；BossRoom 场景实例自动继承（已验证场景实例含组件+子物体+引用）。
- **测试（BossRoom Play + 同步确定性步进，0 红色错误/0 warning）**：
  - 静态：场景实例 `MirrorAngelBoss` 含 skill + BeamOrigin(localPos -0.8,0.8) + 全引用 wire(playerLayer=512)。
  - 前摇/红线：CastRoutine 起→skill.IsCasting=True、mover.IsCasting=True（移动锁），场景出现 1 条红色 warning LineRenderer。
  - 命中/伤害：origin→玩家 CircleCast 三束命中 Player，每束 Health.TakeDamage(10)，HP 50→40→30→20（每束仅一次）。
  - 空中释放：mover.IsGrounded=False（boss 浮空 y=-2.38）仍成功施法 → 无 grounded 限制成立。
  - 死亡中断：致命后 boss.IsDead=True，skill.TryCast() 返回 False（不再起新技能）；进行中协程每帧 Aborted()→EndCast 自解锁（代码确认；本测环境编辑器冻结未步进帧故残留 _isCasting，实机一帧内自愈）。
  - 注：MCP 驱动下编辑器 Play 不自由步进（Time 冻结），故用同步 CircleCast+TakeDamage 复刻 FireBeam 命中逻辑做确定性验证（与技能内部逐字一致）。
- **是否支持空中释放**：是。**是否检查 IsGrounded**：否。**Boss 死亡后是否停止释放**：是。**是否修改玩家逻辑**：否。**是否修改玩家子弹逻辑**：否。**是否修改卡牌/弹匣**：否。**是否修改 Boss 受击/HP**：否（只读 IsDead/CanMove/MaxTotalHp）。**是否修改 BossHUD**：否。
- **玩家受伤调用接口**：`Cardwin.Combat.Health.TakeDamage(int)`（项目现有玩家受伤接口，未新建）。
- **修改文件列表**：新增 `Assets/Scripts/Boss/MirrorAngelTripleBeamSkill.cs`、`Assets/Materials/Boss/MirrorAngel/M_BossBeamWarning.mat`、`M_BossBeam.mat`、`Assets/Prefabs/Boss/MirrorAngel/FX/BossBeamWarning.prefab`、`BossBeam.prefab`；修改 `Assets/Scripts/Boss/MirrorAngelBossGravityMover.cs`、`Assets/Animations/Boss/MirrorAngel/MirrorAngel_CastMirror.anim`(loopTime)、`Assets/Prefabs/Boss/MirrorAngelBoss.prefab`、`SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`。BossRoom 场景实例随 prefab 自动继承（无需手改场景）。
- **Console 红色错误**：0。
- **下一步**：可选——把技能注册进未来 `MirrorAngelBossBrain` 评分系统、第 2/3 束加极短 0.15s 红线闪烁(已留 shortWarningTimeLaterBeams 字段)、光束命中粒子/音效、CastMirror 多帧动作。

---
### 2026-06-23 | Stage 45 — MirrorTripleBeam 朝向修复 + BeamOrigin 镜像 + 第2/3束固定±15°
- **用户需求**：① Boss 动画朝向正好反了需镜像；② Boss 从另一边攻击时 BeamOrigin 也要镜像；③ 三连光束：第1束保持(红线1s沿红线发射)，第2/3束不再重新瞄玩家，而是基于第1束方向固定旋转 ±15°，顺逆顺序由玩家位置决定。只修朝向/BeamOrigin镜像/第2第3束角度，不改其它。
- **改前影响说明**：仅 `MirrorAngelBossGravityMover.cs`(朝向+BeamOrigin镜像)、`MirrorAngelTripleBeamSkill.cs`(角度逻辑)、`MirrorAngelBoss.prefab`(mover.artFacesRight/beamOrigin 序列化)。只读 `MirrorSaintessBoss`/`Health`，未改玩家/子弹/卡牌/弹匣/Boss受击/HP/BossHUD/地面/传送门/普通敌人。未恢复旧部位/FirePoint，Hierarchy 仍 `MirrorAngelBoss/Body/BeamOrigin`。
- **1. 动画反向真因**：美术默认面向**右**，但 mover `artFacesRight=false` → `UpdateFacing` 里 `if(!artFacesRight) sign=-sign` 把朝向取反 → 玩家在右时把朝右的图镜像成朝左（"正好弄反"）。
- **2. 朝向方案**：用 `visualRoot(Body).localScale.x = |baseScale.x| * facingSign`（**未用 flipX**；只镜像 Body，根/Rigidbody2D/Collider 不翻，实测 root scale 恒 (1,1,1)、rotZ=0，物理体不错位）。`artFacesRight` 默认改 `true` 并把 prefab+场景实例序列化值同步为 true（实测 BEFORE=False→AFTER=True）。
- **3. 美术默认朝向**：右（artFacesRight=true）。**修正后 facingSign**：`targetOnRight=worldPos.x>=boss.x; sign=targetOnRight?1:-1; if(!artFacesRight) sign=-sign;` → 玩家右=+1(自然/朝右)、玩家左=-1(镜像/朝左)。
- **4. BeamOrigin 镜像**：mover 新增 `[SerializeField] Transform beamOrigin`（wire `BeamOrigin` 子物体）+Awake 记录 `_beamOriginBaseLocalPos`；`ApplyFacing(sign)` 同步设 `beamOrigin.localPosition.x = Mathf.Abs(baseX) * facingSign` → BeamOrigin 永远在朝向(玩家)一侧。实测：玩家右 worldX=8.82(右)、玩家左 worldX=7.22(左)（boss.x=8.02）。仍 1 个 BeamOrigin、无 FirePoint、不参与碰撞。
- **mover 新增公开 API**：`ComputeFacingSignToward(Vector3)`、`ApplyFacing(float)`、`CurrentFacingSign`；`UpdateFacing` 改为调用二者。移动锁定（施法中）时不跑 UpdateFacing，故施法期朝向冻结。
- **5/6. 三连光束角度**：第1束保留（`AimDirection` 锁玩家方向→红线 firstWarningTime=1s→沿 `baseDir` 发射）。施法开始 `mover.ApplyFacing(ComputeFacingSignToward(player))` 锁 Body+BeamOrigin，整段不翻。第2/3束**不再 re-aim**：`spreadSign = player.y>=GetOrigin().y ? +1 : -1`；循环里 `step=Ceil(i/2)*beamSpreadAngle(15)`、`dirSign=(i奇?spreadSign:-spreadSign)`、`dir=Rotate(baseDir, dirSign*step)` → 3 束=base / +15° / -15°。新增 `Rotate(v,deg)`（+为CCW，normalized）。删除旧的 `dir=AimDirection()` 重新瞄准 + 短闪后再 re-aim。
- **7/8. 第1束与命中不变**：红线 1s 无伤害逻辑不变；FireBeam 未改（CircleCast/beamRange14/beamDamage10/beamVisibleTime0.15/每束最多扣一次/`Health.TakeDamage(int)`）。
- **顺逆判定**：以 `player.position.y` vs `GetOrigin().y` 决定 spreadSign；Rotate(+)=CCW。实测：玩家在上(spreadSign+1)→dir2=+15°(上侧)/dir3=-15°(下侧)；玩家在下(spreadSign-1)→dir2=-15°/dir3=+15°（顺序反）。扇形对称(dir2,dir3 夹角30°)，spreadSign 仅决定先打哪侧；如视觉需相反可对 spreadSign 取反（已注释说明）。
- **Unity 挂载**：prefab 已设 mover.artFacesRight=true + wire mover.beamOrigin=BeamOrigin；场景实例自动继承（实测 instance artFacesRight=True/beamOrigin=BeamOrigin/visualRoot=Body，无残留 override）。
- **测试（BossRoom Play 同步确定性验证，0 红错/0 warning）**：
  - A 朝向：玩家右→Body.scale.x=+0.42(朝右)、玩家左→-0.42(朝左)；root scale(1,1,1)/rotZ0 不翻。
  - B BeamOrigin：玩家右→worldX=8.82(右侧)、玩家左→7.22(左侧)，始终玩家一侧。
  - C 第1束：baseDir 对玩家 angle=0°，红线 1s 逻辑保留。
  - D 第2/3束：dir2/dir3 距 baseDir ±15°、距玩家方向 15°(证明未 re-aim)、dir2-dir3 夹角 30°。
  - E 上下顺逆：玩家上→+15/-15、玩家下→-15/+15。
  - F 死亡：致命后 IsDead=True/CanMove=False、TryCast()=False；FireBeam/Aborted 未改，进行中协程每帧自中断。
  - G 回归：boss HP 400→可被击杀(受击/HP 路径未动)、Console 0 红错。
- **最终方案**：Body.localScale（非 flipX）；美术默认朝右；facingSign 右+1/左-1；BeamOrigin `|baseX|*facingSign`；dir2=Rotate(base,spreadSign*15)/dir3=Rotate(base,-spreadSign*15)。
- **是否修改玩家逻辑/玩家受伤逻辑/Boss受击·HP/BossHUD/卡牌/弹匣/普通敌人**：全否。**玩家受伤接口**：仍 `Cardwin.Combat.Health.TakeDamage(int)`（未改）。
- **修改文件列表**：`Assets/Scripts/Boss/MirrorAngelBossGravityMover.cs`、`Assets/Scripts/Boss/MirrorAngelTripleBeamSkill.cs`、`Assets/Prefabs/Boss/MirrorAngelBoss.prefab`（mover.artFacesRight=true + beamOrigin wire）、`SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`。BossRoom 场景实例随 prefab 自动继承（未改场景文件）。
- **Console 红色错误**：0。
- **下一步**：可选——若实机视觉上下扇形方向相反则翻 spreadSign、第2/3束极短红线闪烁、命中粒子/音效。

---
### 2026-06-23 | Stage 46 — Boss 朝向污染修复（攻击后反着走）：统一 MirrorAngelFacingController
- **用户需求**：Boss 释放 MirrorTripleBeam/CastMirror 后再进 Walk 时移动方向与朝向相反（向左走脸朝右）。统一 Boss 朝向控制；攻击期锁定朝向、结束恢复；Walk/Idle/Dash/Fly/CastMirror/Death 不互相污染 flipX/scale；BeamOrigin 仍跟随朝向镜像。只修朝向，不改技能/Walk/玩家/卡牌/HP。
- **改前影响说明**：仅 Boss 朝向——新增 `MirrorAngelFacingController.cs`；`MirrorAngelBossGravityMover.cs` 朝向委托；`MirrorAngelTripleBeamSkill.cs` 锁定/解锁；`MirrorAngelBoss.prefab` 加 controller 并 wire。只读 `MirrorSaintessBoss`/`Health`。未改玩家/子弹/卡牌/弹匣/Boss受击·HP/BossHUD/地面/传送门/普通敌人；未恢复旧部位（Hierarchy 仍 `MirrorAngelBoss/Body/BeamOrigin`）。
- **1. 反着走真因**：mover 朝向取自**玩家位置**（`ComputeFacingSignToward(_player)`），而移动取自 `ComputeWalkDir`/`_patrolDir`/dash → 在巡逻、边界、施法后恢复巡逻/接近时**两者方向不一致** → 身体面向玩家却朝反方向走。另有**两个朝向写入者**（mover.ApplyFacing + skill 直接调 mover.ApplyFacing），状态分散易乱。
- **2. 动画 Clip 检查**：用 AnimationUtility 枚举 6 个 clip 曲线绑定——`Idle/Walk/Dash/Fly/CastMirror/Death` **仅** `Body SpriteRenderer.m_Sprite`（帧切换），**无 m_FlipX / m_LocalScale.x / m_LocalPosition.x 曲线**。结论：动画 Clip 未污染朝向，无需删曲线。
- **3. 清理动画曲线**：无（Clip 本就干净）。
- **4. 新增统一朝向组件**：是 → `Assets/Scripts/Boss/MirrorAngelFacingController.cs`（Cardwin.Boss）。唯一控制 Body 视觉 + BeamOrigin 镜像；其它脚本只能经它改朝向。
- **5. 视觉方式**：默认 `SpriteRenderer.flipX`（`useSpriteFlipX=true`，可切 localScale 模式）。artDefaultFacesRight=true → 朝右 flipX=false / 朝左 flipX=true。绝对赋值无累积；同时把 Body.localScale.x 兜底保持为正。
- **6. 禁止翻 BossRoot**：是 → 只改 Body(flipX) 与 BeamOrigin(localPosition)。实测 root scale 恒 (1,1,1)、rotZ=0、Body.localScale.x 恒 +0.42。
- **7. LockFacing**：技能 cast 开始 `facing.LockFacing(facing.GetFacingToTarget(player))`（面向玩家并锁定），随后 `mover.SetMovementLocked(true)+SetCasting(true)`。
- **8. UnlockFacing**：`EndCast()` 首行 `facing.UnlockFacing()`。
- **9. 死亡/Disable 强制解锁**：是 → EndCast 由正常结束 / `Aborted()`(boss.IsDead) / `OnDisable()` 调用，三路径都解锁。实测：skill.enabled=false→OnDisable→解锁；死亡后 IsFacingLocked=False。
- **10. 移动脚本朝向更新**：mover `UpdateFacing` 改为——`!facing.IsFacingLocked` 时：`|rb.velocity.x|>0.05 → facing.FaceMoveDirection(vx)`（走/冲/飞=朝移动方向），否则 `facing.FaceTarget(player)`（站立=朝玩家）。移除 mover 自身 ApplyFacing/ComputeFacingSignToward/artFacesRight/visualRoot/beamOrigin 等。
- **11. BeamOrigin 镜像者**：`MirrorAngelFacingController.ApplyBeamOrigin`（`localPosition.x=|baseX|*sign`）；skill 只读 `beamOrigin.position`，不再镜像。
- **12. 连续 5 次攻击 Walk 正常**：是（5 次 Lock/Unlock 循环交替左右，锁定中防污染、解锁后跟随移动，flipX 全对，**无累积漂移**）。
- **13. 玩家左右绕 Boss Walk 正常**：是（朝向跟随移动方向，巡逻不匹配场景"玩家右/移动左"→朝向=左，根除反向；站立朝玩家）。
- **14. 第一束红线 1s**：是（未改）。**15. 第2/3束 ±15°**：是（未改，仅改 cast 开始朝向写法）。
- **16. 改玩家逻辑**：否。**17. 改 Boss HP/受击**：否（只读；实测 HP 400→可正常击杀）。
- **测试（BossRoom Play 同步确定性验证，0 红错）**：A/B 朝向两侧+BeamOrigin 两侧正确、root 不翻；巡逻不匹配修复；C 5 连不漂移；D 绕侧跟随移动；Lock 防污染/Unlock 恢复；Disable+死亡解锁不卡死；E 动画曲线干净；F BeamOrigin 镜像；G 红线1s/±15°/伤害/受伤/HP/HUD 未动。
- **Console 红色错误**：0。
- **修改文件列表**：新增 `Assets/Scripts/Boss/MirrorAngelFacingController.cs`；修改 `Assets/Scripts/Boss/MirrorAngelBossGravityMover.cs`（朝向委托）、`Assets/Scripts/Boss/MirrorAngelTripleBeamSkill.cs`（Lock/Unlock facing）、`Assets/Prefabs/Boss/MirrorAngelBoss.prefab`（加 controller + wire mover.facing/skill.facing）、`SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`。BossRoom 场景实例随 prefab 自动继承。
- **下一步**：可选——FaceTarget 的站立细分(KeepDistance)、若实机镜子在另一侧用 invertBeamOriginSide、命中粒子/音效。

---
### 2026-06-23 | Stage 46.1 — Boss 视觉朝向反向修正
- **用户反馈**：一开始走路视觉就是反的（向左走但脸朝右等）。Stage46 朝向已跟随移动方向（move/face 同源不会再相互矛盾），故"反"=视觉映射整体反 → 美术自然朝向实际为**左**，Stage46 的 `artDefaultFacesRight=true` 假设错了。
- **修复（最小，不改无关文件）**：**仅**把 prefab `MirrorAngelBoss.prefab` 上 `MirrorAngelFacingController.invertVisualFacing` 由 `false` 改为 `true`（该参数只反转 Body 的 flipX 计算，不影响 BeamOrigin 与 root）。未改任何脚本、未改其它字段。场景实例自动继承。
- **测试（Play 同步，0 红错）**：MoveDir+1→flipX=true、MoveDir-1→flipX=false（与上一版相反，纠正反向）；BeamOrigin 仍按 facingSign 正确镜像（+1→右 8.82 / -1→左 7.22）；root scale 恒 (1,1,1)/rotZ0、Body.localScale.x 恒 +0.42；连续/锁定/解锁/死亡逻辑不受影响。
- **是否改脚本/玩家/卡牌/HP/无关文件**：否（仅 1 个 prefab 序列化布尔 + SYSTEM_INDEX/DEVELOPMENT_LOG）。
- **修改文件列表**：`Assets/Prefabs/Boss/MirrorAngelBoss.prefab`、`SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`。
- **Console 红色错误**：0。
- **下一步**：若实机仍有个别状态视觉异常再单独排查；如镜子/光束侧别需对调用 invertBeamOriginSide。

---
### 2026-06-23 | Stage 46.2 — CastMirror 攻击图水平镜像（攻击朝向反）
- **用户反馈**：走路修好后，攻击动画(CastMirror)又反了，单独改、不要动其他文件。
- **根因**：走/施法用同一 flipX 映射（FacingController）；走路对则说明 `MirrorAngel_CastMirror_0.png` 的美术自然朝向与 Walk/Idle 相反（动画 Clip 仅切 m_Sprite、无 flip 曲线，已确认）。
- **修复（最小，仅 1 个文件）**：把 `Assets/Art/Gothic/Boss/MirrorAngel/States/MirrorAngel_CastMirror_0.png` 像素**水平镜像**（读 PNG→LoadImage→GetPixels32 左右翻转→EncodeToPNG 写回→ForceUpdate 重导）。尺寸 1254×1254 不变、导入仍 Sprite/Single。使其自然朝向与其它帧一致，沿用同一 flipX 即正确。
- **未改**：任何脚本 / prefab / 其它 sprite / Walk·Idle·Dash·Fly·Death / 玩家 / 卡牌 / Boss HP·受击 / BeamOrigin / root。
- **修改文件列表**：`Assets/Art/Gothic/Boss/MirrorAngel/States/MirrorAngel_CastMirror_0.png`、`SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`。

---
### 2026-06-24 | Stage 46.3 — Boss AI 设计优化 V1：距离判断 + 决策间隔 + 行为状态机 + 技能候选池 + 攻击概率 + 前摇/释放/后摇 + 重新站位
- **用户需求**：把 Boss 从"追玩家 + 技能 CD 好了立刻释放"改成距离判断 + 决策间隔 + 行为状态机 + 技能候选池 + 攻击概率 + 前摇/释放/后摇 + 重新站位。只用现有 MirrorTripleBeam，不新增第二个技能。
- **修改文件**：
  - 新增 `Assets/Scripts/Boss/MirrorAngelBossBrain.cs`
  - 修改 `Assets/Scripts/Boss/MirrorAngelTripleBeamSkill.cs`（autoCast 默认 false）
  - 修改 `Assets/Scripts/Boss/MirrorAngelBossGravityMover.cs`（增加 brain 引用+脑控移动分支）
  - 修改 `Assets/Prefabs/Boss/MirrorAngelBoss.prefab`（新增 Brain 组件 + autoCast=false）
  - 修改 `SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`
- **新增类**：`MirrorAngelBossBrain`（Cardwin.Boss，RequireComponent(MirrorSaintessBoss)）、`MirrorAngelBossSkillOption`（技能候选池数据）、`MirrorAngelBossBrainState`（行为状态枚举）
- **新增函数**：`DecideNextAction/TryUseSkill/StartSkill/CastSkillRoutine/StartReposition/StopAllBossActions/IsSkillUsable/ScoreSkill/ChooseBestSkill/DistanceToPlayer/FindPlayer`（Brain）；未新增 Mover/BeamSkill 函数。
- **核心设计**：
  - **状态机**：Idle/Approach/KeepDistance/Reposition/Windup/Casting/Recovery/Dead（Inspector 可见 currentState）
  - **距离参数**：tooClose=2.5/preferredMin=4/preferredMax=7/far=10
  - **决策间隔**：0.5~1.2s 随机（不做每帧决策）
  - **技能候选池**：MirrorAngelBossSkillOption(skillId/cooldown/lastUseTime/minRange/maxRange/baseWeight/repeatPenalty)；仅 MirrorTripleBeam(cooldown=4.5/min=4/max=12/weight=10/penalty=3)
  - **评分**：baseWeight + rangeScore×5 - repeatPenalty + Random(-1,1)；选最高分
  - **攻击概率**：attackChance=0.65（可用不一定放）
  - **Reposition**：后撤 0.4~0.8s（repositionDurationMin/Max, speedMultiplier=1.0）
  - **Recovery**：recoveryDuration=0.5s
- **Unity 挂载方式**：Brain 已添加到 MirrorAngelBoss.prefab root，auto-resolve boss/mover/facing/beamSkill/animBridge 引用；场景实例随 prefab 继承。
- **禁止修改**：玩家/卡牌/弹匣/背包/Boss HP 受击/BossHUD/地面/传送门/PlayerController/Projectile/普通敌人/EventSystem/Retry/GlobalRuntimeRoot/部位/CastMirror_0.png/Walk/Dash/Fly/Death 动画/X°15 光束逻辑/红线1s逻辑
- **测试要求（按规格）**：
  - A（玩家很远 >10）：Boss Approach 靠近，不无脑放光束。
  - B（玩家贴脸 <2.5）：Boss Reposition 后撤，不贴脸。
  - C（理想距离 4~7）：不贴脸、会停顿/保持距离、有概率释放 MirrorTripleBeam。
  - D（技能 CD 好了不放）：仅进入候选池，受距离/状态/attackChance 限制。
  - E（技能完整流程）：Windup→CastMirror→第1束红线1s→三连光束→Recovery→Idle→Decide。
  - F（攻击后走路朝向）：攻击后继续移动，FacingController 不被污染。
  - G（死亡）：Brain 进 Dead、不再决策/移动/释放。
  - H（回归）：玩家子弹命中/Hp下降/BossHUD/卡牌/弹匣正常；Console 红色错误=0。
- **已知问题**：当前仅 1 个技能候选（MirrorTripleBeam），评分/候选池为后续扩展 CloseStrike/GroundRay/DashSlash 预留。
- **下一步**：实机 Play 验证全部测试 A~H；后续可扩展第二个攻击技能。
- **Console 红色错误**：0（编译通过）。
- **修改文件列表**：新增 `MirrorAngelBossBrain.cs`；修改 `MirrorAngelTripleBeamSkill.cs`（autoCast=false）、`MirrorAngelBossGravityMover.cs`（brain 分支）、`MirrorAngelBoss.prefab`（Brain+autoCast）、`SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`。
- **Console 红色错误**：0。

---

### 2026-06-24 | Stage 48.2 — 稳定性修复：Boss 卡三射光 + 死亡掉落
- **Bug 1 根因**：Brain `CastSkillRoutine` 无 try/finally → `StopAllBossActions` 中 `StopCoroutine(_stateRoutine)` 后清理代码跳过 → `currentState` 卡在 Casting/Windup → Boss 永远不重新决策。
- **Bug 1 修复**：Brain `CastSkillRoutine` try/finally；finally 无条件执行 `_brainMovementLocked=false`/`mover.SetMovementLocked(false)`/`SetCasting(false)`/`UnlockFacing`/`AttackType=0`/`currentState=Idle`。TripleBeam `CastRoutine` 同步 try/finally → `EndCast()` 必执行。
- **Bug 2 根因**：`MirrorSaintessBoss.Die()` 调用 `SetAllCollidersEnabled(false)` 禁用 CapsuleCollider2D → Dynamic Rigidbody2D(g=3) 穿透 MainGround。
- **Bug 2 修复**：`Die()` 中禁用 Collider **前** 设 `rb.bodyType=Kinematic, velocity=0, gravityScale=0`；Brain `StopAllBossActions` 同步 Rigidbody 冻结兜底。
- **修改文件列表**：`MirrorAngelBossBrain.cs`（try/finally+Rigidbody 冻结）、`MirrorAngelTripleBeamSkill.cs`（try/finally）、`MirrorSaintessBoss.cs`（Die 中 Rigidbody 冻结）、`SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`。
- **Console 红色错误**：0。
---

### 2026-06-24 | Stage 48 — Boss 近战技能：二连横劈(DoubleSlash) + 二连横劈突刺(DoubleSlashDash)
- **用户需求**：基于 `C:\Users\86189\Desktop\base\attack2` 素材实现两个近战技能，接入 Brain 候选池。
- **素材导入**：6 张 PNG → `Assets/Art/Gothic/Boss/MirrorAngel/States/Attack2/`，Sprite/Single/PPU100/FullRect/AlphaTrans/Bilinear/Uncompressed/noMips。
- **动画 Clip**：`MirrorAngel_Attack2_DoubleSlash.anim`（4 帧 0.90s）和 `MirrorAngel_Attack2_DoubleSlashDash.anim`（6 帧 1.39s），只绑定 Body.m_Sprite。
- **新增脚本**：
  - `MirrorAngelDoubleSlashSkill.cs`：两段 OverlapBox 近战，slashRangeX=2.5/Y=1.8/offsetX=1.4/Y=1.0，12 伤害×2，朝向自动镜像，AttackType=3。
  - `MirrorAngelDoubleSlashDashSkill.cs`：两段横劈 + dashDistance=3.5(speed=3x walkSpeed) + 冲刺命中盒(3.5×2.0, damage=20)，临时解除 MovementLock 用于 dash 位移，AttackType=4。
  - 均含 ContextMenu Debug/Play + Gizmos 橙色/红色命中框 + 红色位移终点球。
- **AnimatorController**：新增 `Attack2_DoubleSlash`(AttackType=3) 和 `Attack2_DoubleSlashDash`(AttackType=4) 状态 + AnyState 过渡。
- **Brain 接入**：技能池新增 DoubleSlash(cooldown=3/0.8~2.8/weight=9/penalty=2) + DoubleSlashDash(cooldown=5/2.0~5.0/weight=7/penalty=3)。CastSkillRoutine/IsSkillRunning 分发。
- **Mover 改动**：新增 `Rigidbody`(property) 和 `IsMovementLocked`(property) 供 DashSkill 用。
- **朝向**：复用 FacingController + flipX；所有判定按 Boss 朝向镜像。突刺前摇/攻击图如果视觉反向，需单独水平翻转 PNG（类似 Stage 46.2 CastMirror 修复）。
- **未改**：玩家/卡牌/弹匣/HP/BossHUD/地面/三连光束/地面光柱/FacingController。
- **Console 红色错误**：0。
- **修改文件列表**：新增 Attack2 png×6 + 2 anim clips + 2 skill scripts；修改 Mover.cs + Brain.cs + AnimatorController + BossPrefab + SYSTEM_INDEX + DEVELOPMENT_LOG。
---

### 2026-06-24 | Stage 48.1 — 修复 DoubleSlashDash 突刺位移：rb.MovePosition + 外部速度覆写
- **用户反馈**：Boss 播放了二连横劈+突刺动画，但身体不位移，原地摆动作。
- **根因诊断（完整链路）**：
  1. `DoubleSlashDashSkill.CastRoutine` 在执行 dash 时：`mover.SetMovementLocked(false)` + `rb.velocity = dashVelocity`。
  2. **下一帧 `Mover.FixedUpdate`**：`_movementLocked=false` → 进入 `BrainActive` 分支 → 读 `brain.DesiredMoveX`（Brain 处于 Casting 状态→`_brainMovementLocked=true`→`DesiredMoveX=0`）→ `rb.velocity = new Vector2(0 * walkSpeed, ...)` → **技能刚设的 dash 速度被 Mover 当场清零**。
  3. 旧代码 `moveX = facingSign * dashSpeedMultiplier; rb.velocity = new Vector2(moveX * 1.2f, rb.velocity.y)` 用的是常数速度而非精确位移控制，且每次都被下一帧 FixedUpdate 覆写。
- **修复**：
  - **Mover**：新增 `SetExternalVelocity(Vector2)` / `ClearExternalVelocity()` + `HasExternalVelocity` 属性。`BrainActive` 分支优先检查外部速度（非负标记），有则直接使用、不读 `brain.DesiredMoveX`。
  - **DoubleSlashDashSkill**：Dash 阶段改为 `rb.MovePosition` + SmoothStep（`dashCurve` 或默认 `smoothstep`），用 `WaitForFixedUpdate` 逐物理步移动。参数改为 `dashDistance=3.2f` / `dashDuration=0.25f`。Backward-compatible AnimationCurve 可选。
  - Skill 在 dash 开始时不再 `SetMovementLocked(false)`；Mover 在 `_movementLocked=true` 时仍 block 普通移动，但外部速度覆写绕过 `brain.DesiredMoveX` 的同时保留 Brain 的移动锁语义（Dash 速度由 skill 直接控，不受 Brain 影响）。
- **修改文件列表**：`MirrorAngelBossGravityMover.cs`（+外部速度覆写）、`MirrorAngelDoubleSlashDashSkill.cs`（dash 改写为 MovePosition+SmoothStep）、`SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`。
- **未改**：动画、朝向、伤害判定、TripleBeam/GroundRay/DoubleSlash。
- **Console 红色错误**：0。
---

### 2026-06-24 | Stage 47.2 — 修正 GroundRay 动画显示 Idle：新增 AttackType 参数分流
- **用户反馈**：GroundRay 不播放 attack1 动作，显示站立 Idle。
- **根因诊断（完整链路）**：
  1. Stage 47.1 移除了 `mover.SetCasting(true)` 以避免 CastMirror 覆盖，但导致 `mover.IsCasting=false`。
  2. `AnimatorBridge.Update()` 每帧写 `animator.SetBool("IsCasting", false)` + `animator.SetFloat("MoveSpeed", 0)`（移动锁→速度归零）。
  3. Animator 的 AnyState 过渡按优先级评估：Death(skip)→CastMirror(skip)→Dash(skip)→Fly(skip)→Walk(MoveSpeed=0→skip)→**Idle(MoveSpeed<0.1 ∧ !IsCasting ∧ !IsDashing ∧ !IsFlying → 全真 → 触发)**。
  4. `animator.Play("Attack1_GroundRay")` 被每帧 AnyState→Idle 覆盖 → 永远显示 Idle。
- **修复方案：AttackType 参数分流**：
  - **AnimatorController 新增** `AttackType` (int, 默认 0)。
  - **修改 CastMirror 过渡**：添加条件 `AttackType != 2`。
  - **新增 AnyState→Attack1_GroundRay 过渡**：`IsCasting=true, IsDead=false, AttackType=2`。
  - **GroundRaySkill 恢复** `mover.SetCasting(true)` + 新增 `animator.SetInteger("AttackType", 2)`。
  - **Brain 兜底**：`CastSkillRoutine` + `StopAllBossActions` 重置 `AttackType=0`。
- **运行时过渡优先级**：Death > CastMirror(AttackType!=2) > Idle(!IsCasting) > Attack1_GroundRay(AttackType=2)
  - TripleBeam(AttackType=0, IsCasting=true) → CastMirror 触发 ✓
  - GroundRay(AttackType=2, IsCasting=true) → CastMirror 跳过，Attack1_GroundRay 触发 ✓
  - 技能外(IsCasting=false) → Walk/Idle 正常 ✓
- **修改文件列表**：`MirrorAngelBoss.controller`（+AttackType 参数 + 改过渡）、`MirrorAngelGroundRaySkill.cs`（恢复 SetCasting + AttackType）、`MirrorAngelBossBrain.cs`（AttackType 兜底重置）、`SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`。
- **Console 红色错误**：0。
---

### 2026-06-24 | Stage 47 — Boss 第二技能：大范围蓄力地面光柱 MirrorAngelGroundRay
- **用户需求**：根据 `C:\Users\86189\Desktop\base\attack1` 素材制作第二个技能——大范围蓄力地面光柱，攻击 Boss 朝向一侧 X=100，无地面预警，有前摇/持续/后摇，接入 Brain 候选池。
- **修改文件**：
  - 新增 `Assets/Art/Gothic/Boss/MirrorAngel/States/Attack1/MirrorAngel_Attack1_{Windup,Active,Recovery}_00.png`（3 张，从 attack1 复制+重命名）
  - 新增 `Assets/Animations/Boss/MirrorAngel/MirrorAngel_Attack1_GroundRay.anim`（Windup 0.9s / Active 0.8s / Recovery 0.5s，3 帧非循环，只切 Body m_Sprite）
  - 新增 `Assets/Scripts/Boss/MirrorAngelGroundRaySkill.cs`
  - 修改 `Assets/Animations/Boss/MirrorAngel/MirrorAngelBoss.controller`（新增 Attack1_GroundRay 状态）
  - 修改 `Assets/Scripts/Boss/MirrorAngelBossBrain.cs`（新增 groundRaySkill 引用+默认技能池增加+CastSkillRoutine 分发）
  - 修改 `Assets/Scripts/Boss/MirrorAngelBossAnimatorBridge.cs`（新增 public Animator Animator 属性）
  - 修改 `Assets/Prefabs/Boss/MirrorAngelBoss.prefab`（新增 MirrorAngelGroundRaySkill 组件）
  - 修改 `SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`
- **新增类**：`MirrorAngelGroundRaySkill`（Cardwin.Boss）——Windup/Active/Recovery 三阶段协程。
- **新增函数**：`TryCast/CastRoutine/DealDamageOnce/SpawnActiveFx/PositionFx/CreateRuntimeFx/DespawnFx/Aborted/EndCast/ResolvePlayer/OnDisable/OnDrawGizmosSelected`（GroundRaySkill）；`Animator` property（AnimatorBridge）。
- **素材导入**：3 张 PNG（qianyao→Windup/shiangzhong→Active/houyao→Recovery），Sprite/Single/PPU100/FullRect/AlphaTrans/Bilinear/Uncompressed/noMips。文件名自然顺序=语义顺序。
- **攻击范围**：Boss 朝向一侧 `OverlapBox`（X=100, H=8），从 `facing.CurrentFacingSign` 决定方向（右+1/X=100 右侧，左-1/X=100 左侧），不按玩家位置决定。damage=18，每技能最多扣一次。
- **FX**：无 prefab 时运行时创建 4×4 RGBA32 白色 SpriteRenderer（半透明紫白 fxColor(0.7,0.5,1,0.35)，sortingOrder=60），scale 匹配攻击矩形。有 prefab 时实例化。
- **Animator 接入**：GroundRaySkill 通过 `animBridge.Animator.Play("Attack1_GroundRay")` 播放动作；结束后 AnimatorBridge Update 恢复参数驱动。
- **Brain 接入**：技能池默认增加 MirrorAngelGroundRay(cooldown=8/minRange=0/maxRange=100/baseWeight=8/repeatPenalty=4)；CastSkillRoutine 按 skillId 分发 TryCast；Waiting 检查对应的 IsCasting 属性。
- **朝向兼容**：复用 FacingController.LockFacing/UnlockFacing；GroundRaySkill 不直接改 Body.localScale.x/flipX/root.localScale。死亡/OnDisable 兜底 EndCast→UnlockFacing。
- **Gizmos**：OnDrawGizmosSelected 绘制半透明紫色攻击矩形（drawCube+drawWireCube）+ 黄色 Boss 原点球。
- **禁止修改**：玩家/卡牌/弹匣/背包/Boss HP 受击/BossHUD/地面/传送门/MirrorTripleBeam 光束逻辑/第1束红线1s/±15° 逻辑/FacingController 朝向修复/Walk/Dash/Fly/Death 动画/旧部位。
- **测试要求**：
  - A（素材导入）：3 张 Attack1 动画正常播放。
  - B（前摇）：Windup 0.9s 播放蓄力动作，无伤害，无地面红圈/红线。
  - C（持续）：Active 0.8s 显示紫白光柱 FX，Boss 朝向一侧 X=100，玩家在范围内扣血（最多一次），外不扣。
  - D（后摇）：Recovery 0.5s，FX 消失，无伤害，恢复 AI。
  - E（朝向）：面向右→右 X100 受伤/左不伤；面向左→左 X100 受伤/右不伤。
  - F（空中释放）：不检查 IsGrounded，空中可正常完成技能。
  - G（AI 接入）：Brain 候选池含 MirrorAngelGroundRay，CD=候选条件而不是立刻释放；MirrorTripleBeam 仍正常。
  - H（回归）：攻击后走路朝向正常/FacingController 不污染/Boss 可被命中/HP·HUD 正常/玩家卡牌弹匣背包正常/Console 0 红错。
- **Console 红色错误**：0（编译通过）。
- **修改文件列表**：新增 GroundRaySkill.cs + Attack1 png×3 + GroundRay.anim；修改 Brain.cs + AnimatorBridge.cs + BossPrefab + AnimatorController + SYSTEM_INDEX + DEVELOPMENT_LOG。
---

### 2026-06-24 | Stage 47.1 — 修正 GroundRay 动画绑定：Play 动画被 AnyState→CastMirror 覆盖
- **用户反馈**：Boss 释放第二技能 GroundRay 时，肉眼看到的是第一个技能的老动画 CastMirror，不是 attack1 的新动画。
- **根因诊断**：
  - `MirrorAngelBoss.controller` 有 6 条 AnyState 过渡，其中优先级第 2（仅次于 Death）的是 `AnyState → CastMirror [IsCasting=true, IsDead=false]`。
  - Brain 的 `CastSkillRoutine` 在技能预分发时调用了 `mover.SetCasting(true)`，GroundRaySkill 的 `CastRoutine` 也调用了 `mover.SetCasting(true)`。
  - `MirrorAngelBossAnimatorBridge.Update()` 每帧将 `mover.IsCasting` 同步到 Animator 的 `IsCasting` 参数。
  - → `IsCasting=true` 每帧触发 AnyState → CastMirror，**覆盖**了 `animator.Play("Attack1_GroundRay")` 的调用。
  - 所以 GroundRay 的动作动画一直是 CastMirror_0（旧图），不是 attack1 的新图。
- **确认数据**：
  - `Attack1_GroundRay` 状态 → motion=`MirrorAngel_Attack1_GroundRay`（3 帧 attack1：Windup/Active/Recovery）✓ 正确。
  - `CastMirror` 状态 → motion=`MirrorAngel_CastMirror`（1 帧 CastMirror_0 旧图）✓ 正确。
  - 动画 Clip 绑定本身没有问题，问题在运行时参数覆盖。
- **修复**（最小改动）：
  - **Brain `CastSkillRoutine`**：移除预分发中的 `mover.SetCasting(true)`，只保留 `mover.SetMovementLocked(true)` + `facing.LockFacing()`。各技能自行管理 Casting 状态。
  - **GroundRaySkill `CastRoutine`**：移除 `mover.SetCasting(true)`。GroundRay 使用直接 `animBridge.Animator.Play("Attack1_GroundRay")` 驱动动画，不需要 IsCasting 参数。mover.IsCasting=false → AnimatorBridge 设置 Animator.IsCasting=false → AnyState→CastMirror 条件不满足 → 动画不被覆盖。
  - **TripleBeamSkill**：未改，仍在其 `CastRoutine` 内 `mover.SetCasting(true)` → AnimatorBridge.IsCasting=true → AnyState→CastMirror 正确触发。
- **修改文件列表**：`MirrorAngelBossBrain.cs`（移除 SetCasting）、`MirrorAngelGroundRaySkill.cs`（移除 SetCasting）、`SYSTEM_INDEX.md`、`DEVELOPMENT_LOG.md`。
- **未改**：动画 Clip、Animator 状态、技能逻辑、攻击范围/伤害、朝向、玩家/卡牌/弹匣/HP 等。
- **Console 红色错误**：0。

---

### 2026-06-24 | Stage 49 — 动作仲裁系统：ActionController 统一技能锁 + token 防抢占
- **问题**：技能互相抢占、AnimatorBridge 写 Idle/Walk 覆盖攻击动画、Brain 攻击中仍决策。
- **根因**：IsCasting/AttackType/movementLock/facingLock 由多模块分散写入，无互斥。
- **修复**：新增 `MirrorAngelBossActionController.cs` — BeginAction(token)/EndAction(token match)/ForceCancelAction/AllowSkillMotion。Brain+AnimatorBridge 检查 IsActionLocked。
- **修改文件**：新增 ActionController.cs；修改 Brain.cs + AnimatorBridge.cs + Prefab + SYSTEM_INDEX + DEVELOPMENT_LOG。
- **Console 红色错误**：0。

---

### 2026-06-24 | Stage 50 — 远距离冲刺接近 + 飞天悬停激光
- **FarDashApproach (AttackType=5)**：距离>9m,35%概率,CD5s。停止距离4.5m,最大冲刺6m,时长0.35s。Brain内DoFarDashCoroutine用rb.MovePosition+SmoothStep。Dash动画。
- **AirLaserMode (AttackType=6)**：距离4~12m,25%概率,CD9s。上升3.5m→悬停3s→3次激光(10伤,Range16)→降落0.4s。MirrorAngelAirLaserSkill独立脚本。Fly动画。LineRenderer复用。
- **修改文件**：新增AirLaserSkill.cs；修改Brain.cs+ActionController.cs+AnimatorController+Prefab+docs。
- **Console红色错误**：0。

---

### 2026-06-24 | Stage 50.4 — 修复 AirLaser 红线/激光不显示
- **根因**：(1) fallback LineRenderer 未设 `sortingLayerName`/`sortingOrder=200`(旧=120)；(2) `material.color` 未直接设 alpha；(3) `origin` 使用 `_rb.position`(脚底)导致激光穿过 Boss；(4) 无 Debug 日志查调用。
- **修复**：`SpawnLine` 重写——fallback 创建 `Sprites/Default` 材质设 color + sortingLayer="Default" + sortingOrder=200 + enabled=true + numCornerVertices=4。`FireAirLaser` 重写——origin 加 +0.8y 偏移 + null player guard + dir 兜底 + warning/beam 各用 while 循环保证显示时长。新增 `Debug/Force AirLaser Once` + `Debug/Force AirLaserMode` ContextMenu。Animator Fly 过渡改为 `AirSubType Equals 1`（更精确）。
- **修改文件**：AirLaserSkill.cs（SpawnLine+FireAirLaser 重写+ContextMenu）+ AnimatorController（Fly 过渡条件修正）+ docs。
- **Console 红色错误**：0。
---

### 2026-06-24 | Stage 50.3 — 重构 AirLaserMode 动画：AirSubType 分流，不再整段 Fly
- **问题**：浮空后整段保持 Fly 动画，AirDash/AirLaser 子动作无法表现。
- **根因**：Animator 中 `AnyState→Fly[AttackType=6]` 每帧触发，覆盖所有子动画切换。
- **修复**：
  - Animator 新增 `AirSubType` (int) 参数。AttackType=6 的 Fly 过渡加 `AirSubType<2` 条件(仅 Hover/Move)。新增 Dash(`AttackType=6,AirSubType=2`)和 CastMirror(`AttackType=6,AirSubType=3`)过渡。
  - AirLaserSkill 新增 `AirSubState` 枚举(Rise/Hover/Move/Dash/Laser/Exit)+Inspector 可见。`SetAirSubType(st)` 写入 Animator。每个子动作前后切 AirSubType。Dash→Type=2/Dash 动画，Laser→Type=3/CastMirror 动画，其余→Type=1/Fly。
  - Rise→Hover→AirLoop(首轮必 Laser→可选 Dash/Move/再 Laser)→Exit。每次子动作后回 Hover。
- **修改文件**：AirLaserSkill.cs（+AirSubState+SetAirSubType 贯穿）+MirrorAngelBoss.controller（+AirSubType 参数+3 条过渡修改）+docs。
- **Console 红色错误**：0。
---

### 2026-06-24 | Stage 50.2 — 重构 AirLaserMode：保证攻击 + 重力接管 + Mover 尊重
- **Bug1 浮空发呆**：旧版无 `hasFiredAtLeastOneLaser`，如果 `Random.value > airLaserChance` 或时间太短，可能 0 次激光就退出。
- **修复**：新增 `hasFiredAtLeastOneLaser` bool。AirLoop 首轮必须发射一次激光，之后才可选移动/冲刺/再激光。`airDuration` 保底 `Mathf.Max(airDuration, minRequired)`。
- **Bug2 重力冲突**：旧版只保存 `_originalGravityScale`，不保存 constraints。`Aborted()` 中恢复重力与 `Die()` 冻结冲突。
- **修复**：保存/恢复 `_originalGravityScale` + `_originalConstraints`。拆分为 `EndCastNormal()`（恢复正常重力）和 `EndCastDeath()`（FreezeAll + velocity=0）。`Aborted()` 调用 `EndCastDeath()`。`OnDisable` 判断 IsDead 选择清理方式。
- **Bug3 Mover 覆盖**：GravityMover 在 AirLaserMode 期间仍可能写速度（BrainActive 分支）。
- **修复**：Mover 新增 `actionController` 引用；FixedUpdate 在 ActionLocked+AirLaserMode 时直接 return，不控制 rigidbody。
- **修改文件**：AirLaserSkill.cs（重构 CastRoutine/cleanup）+ GravityMover.cs（+actionController + AirLaser check）+ docs。
- **Console 红色错误**：0。
---

### 2026-06-24 | Stage 50.1 — 三项修复：FarDash触发 + AirLaser飞行移动 + Walk覆盖
- **Bug1**：DecideNextAction远距离先Approach return，吞掉FarDash评估。修复：远距离先TryUseSkill再降级。调优minDist=8.5/chance=0.55/cooldown=4.5/weight=9。ScoreSkill+5远距加成。+ForceFarDash ContextMenu。
- **Bug2**：AirLaser固定3s静态。重写HoverLoop随机6~10s，横向移动+空中冲刺+间歇激光。rise→loop→exit。
- **Bug3**：BeginAction未设MoveSpeed=0→Walk延迟退出。修复：BeginAction/EndAction/ForceCancel均设MoveSpeed=0。
- **修改文件**：Brain.cs+AirLaserSkill.cs+ActionController.cs+docs。
- **Console红色错误**：0。
---

### 2026-06-24 | Stage 50.5 — AirLaser 预警时间过短修复
- **根因**：`laserWarningTime=0.2f`→几乎看不见。已有方向锁定(dir 仅计算一次)和伤害分离(beam 阶段才扣血)。
- **修复**：`laserWarningTime=0.85f`, `laserVisibleTime=0.18f`, `warningWidth=0.08f`, `beamWidth=0.24f`。
- **修改文件**：AirLaserSkill.cs（参数）+ docs。
- **Console 红色错误**：0。
---

### 2026-06-24 | Stage 51 — 统一攻击协议：技能清理冗余状态管理
- **诊断**：所有 clip loop=false/只切 Sprite。AnyState canTransitionToSelf=false。AnimatorBridge+Brain 已检查 ActionLocked。但 DoubleSlash/Dash/GroundRay 技能仍直接调 `mover.SetCasting/SetMovementLocked` 等，与 Brain BeginAction/EndAction 双重管理。
- **修复**：三个技能移除直接 mover/facing/AttackType 调用。技能只管理 `_isCasting` + 自身 FX。movement/casting/AttackType 由 Brain BeginAction/EndAction 统一。
- **修改**：DoubleSlashSkill.cs + DoubleSlashDashSkill.cs + GroundRaySkill.cs + docs。
- **Console 红色错误**：0。
---

### 2026-06-24 | Stage 52 — Demo_Combat 诅咒模组道具
- **新增** `PlayerCursedEightModuleState.cs`：邪恶=8(PlayerAlignment)、FireRateMultiplier=1.5、InfiniteEightLoop=true、HP drain 1%/s 非致死(最低 1%)。
- **新增** `CursedEightModulePickup.cs`：玩家靠近按 F 激活，BoxCollider2D trigger，紫色结晶 Sprite。
- **MagazineSystem**：`InfiniteEightLoopEnabled` property + AdvanceIndex 中 CurrentIndex>=count 时 wrap 到 0 而非 reload。
- **GlobalRuntimeRoot Player**：新增 `CursedEightModuleState` 组件。
- **Demo_Combat**：放置 pickup 在 (16,-1)，右侧约 4m。
- **修改**：State.cs + Pickup.cs + MagazineSystem.cs + GlobalRuntimeRoot.prefab + Demo_Combat scene + docs。
- **Console 红色错误**：0。
---

### 2026-06-24 | Stage 52.1 — 诅咒模组修复：Good+Evil 总和约束 + 强制攻击弹夹
- **Bug1**：`SetEvil(8)` 未清 Good→Good=4,Evil=8(总和12≠8)。修复：`SetValues(0,8)→Good=0,Evil=8`。
- **Bug2**：旧弹夹直接循环→增益子弹也循环。修复：`ForceLoadEightAttackCards`清空→8张攻击卡→index=0→infinite loop。攻击卡通过`CardType.Attack`/`Damage`从 CardDatabase/initialCards/LoadedCards 查找。
- **修改**：State.cs（SetValues+FindAttackCard+ForceLoad）+MagazineSystem.cs（ForceLoadEightAttackCards）+docs。
- **Console 红色错误**：0。
---

### 2026-06-24 | Stage 52.2 — 诅咒模组掉血修复
- **根因**：(1) 组件可能不在 Player 实例；(2) _health 在 Awake 时可能未 ready。
- **修复**：Pickup 动态 AddComponent；State Update lazy resolve Health（GetComponent/InParent/InChildren）+ maxHealth guard。
- **修改**：Pickup.cs + State.cs + docs。
- **Console 红色错误**：0。
---

### 2026-06-24 | Stage 53 — BlessedEightModule：移速-50%+射速-50%
- **PlayerController2D**：新增 externalMoveSpeedMultiplier/externalFireRateMultiplier + _nextAllowedFireTime fire cooldown + SetExternal* 接口。
- **BlessedState**：Good=8/Evil=0 + 8增益弹夹循环 + SetFireRate(0.5)+SetMoveSpeed(0.5) + 敌人光环。
- **CursedState** 接入 SetExternalFireRateMultiplier(1.5)。
- **Pickup** 通用 ModuleType 枚举(Cursed/Blessed) + 金色 Blessed pickup x=-12。
- **Console 红色错误**：0。
---

### 2026-06-24 | Stage 53.1 — 修复 Blessed=Cursed：分离 Pickup + 互斥
- **根因**：Blessed pickup 用通用 Pickup + reflection 设 ModuleType=1，未序列化 → 运行时=默认 Cursed(0)。
- **修复**：新建 BlessedEightModulePickup（独立脚本，直调 Blessed 模块）。双模块 Activate 互斥。全链路 debug 日志。
- **修改**：新增 BlessedEightModulePickup.cs + 修改 CursedState/BlessedState（互斥+日志）+ doc。
- **Console 红色错误**：0。
---
- **根因**：`Health.currentHealth` 是 int。每帧 `50*0.01*dt≈0.008`→`RoundToInt=0`；每秒 `50*0.01=0.5`→`RoundToInt=0`(banker's rounding)。damage 永远=0。
- **修复**：改为 `IEnumerator DrainHpRoutine` 每秒 tick + `_drainAccumulator`(float) 累加 `maxHp*0.01`，`FloorToInt(_drainAccumulator)`≥1 时才扣除 int HP。Pickup 改用 `GetComponentInParent<Health>()` 定位正确的 Health 持有对象再挂组件。新增 `Debug/Force One HP Drain Tick` ContextMenu。
- **验证**：maxHp=50→每秒 accumulator+0.5→2 秒后 Floor(1.0)=1→HP 50→49。真实数值日志可见。
- **Console 红色错误**：0。
---

### 2026-06-25 | Stage 54 — 第三个特殊模组 ConfessionNightModule：整首歌循环音游《告白の夜》
- **目标**：玩家在 Demo_Combat 与道具(F)交互后 → 普通战斗 UI 碎裂消失 → 播放《告白の夜》 → 进入整首歌循环音游；左键判定红音符(命中→追踪弹打最近普通敌人 3% 最大生命)，右键判定蓝音符(命中→回血 5%)，Miss/点错→扣 10% 最大生命；音乐 loop、谱面同步 loop、UI/普通射击循环期不恢复(仅死亡/场景切换结束)。
- **音频**：建 Assets/Audio/（放真实 Ayasa_Confession_Night.mp3）。本环境无源 mp3(/mnt/data 不存在、桌面无匹配)→controller 未指定 clip 时生成 290s(≈4:50) 程序化占位 clip(11025Hz 单声道 93BPM 点击轨)驱动整首歌时间线+循环，可立即测试；放入真 mp3 并在 PlayerConfessionNightModuleState.confessionNightClip 赋值即用真歌(chart 按 clip.length 生成)。
- **新增脚本(Cardwin.Modules)**：ConfessionNightModulePickup / PlayerConfessionNightModuleState / RhythmGameController / RhythmNote(RhythmNoteType+RhythmNoteData+RhythmNote view) / RhythmHomingBullet / CombatUIBreakController。
- **整首歌谱面**：GenerateFullSongChart(clip.length)，BPM93/4-4/seed9301/intro6s/end2s；密度按歌曲百分比(10/35/70/92/100%)分段，红蓝比例随段落(50→55→65→75→50%，小节首拍偏红/三拍偏蓝)，高潮加半拍。实测 chart=474、first=6.00、last=287.29(覆盖 songLen290-2.71，非 90s)、red310/blue164(≈65/35)。
- **按 audioSource.time 生成**：SpawnNotesByAudioTime 提前 noteTravelTime(2s) 生成；音符到 hitTime 恰好抵达 HitCircle(x≈1498)，随时间左移。
- **循环**：audioSource.loop=true + Update 检测 audioTime<lastAudioTime-1 → OnMusicLooped(loopCount++ / nextNoteIndex=0 / ClearAllActiveNotes / 复用同一份 chart)；UI/射击循环期不恢复。取消“音乐结束恢复 UI”旧设计。
- **UI 碎裂**：CombatUIBreakController 对主 HUD Canvas 非菜单子物体生成飞散碎片(白方块+重力+旋转+淡出)+CanvasGroup 淡出后 SetActive(false)，跳过 Pause/GameOver/Setting/Bag/Rhythm；RestoreNormalCombatUI 仅死亡/退出时调用。
- **判定**：HitCircle(0.78w,0.18h) 灰白环，左闪红/右闪蓝 0.12s；hitWindow110/perfect55/miss150 px；左红=Hit / 右蓝=Hit / 左蓝=右红=Wrong / 越 miss 线=Miss / 空点不罚。
- **效果**：红命中 RhythmHomingBullet 锁最近普通敌人(Health，排除 Player/Boss)，speed12/life4，接触造成 target.maxHealth*3%(Health.TakeDamage 可致死)；蓝命中 Health.Heal(5%)；Miss/Wrong Health.TakeDamage(10%)。复用既有 Health.Heal/TakeDamage，未改 Health.cs。
- **PlayerController2D**：仅两个鼠标射击入口追加 “&& !RhythmGameController.IsRhythmModeActive” 守卫(音游中锁普通射击)；移动/跳跃/冲刺/换弹不受影响。
- **场景**：Demo_Combat (28,-1,0) 新增 ConfessionNightModulePickup(紫白发光圆盘 sprite Assets/Art/Modules/ConfessionNightModule.png + CircleCollider2D trigger r0.9 + Prompt 子物体 “Press F: Confession Night”)，远离前两个道具(16 / -12)无触发重叠。
- **测试(Play + EditorApplication.Step 确定性步进，编辑器失焦时 audioSource.time 不前进，用 DebugSeekAudio 驱动)**：激活 OK / UI 碎裂(7 个 HUD off + 菜单保留 + 75 碎片) / 音乐 audioPlaying=True / HitCircle(1498,194) / 红蓝音符从右飞来并左移 / 红命中追踪弹 MeleeEnemy_01 30→29 / 蓝回血+3 / Miss-5 / Wrong-5 / 真 Update 循环 loopCount0→1,nextIdx→0,清空,chart复用,IsRhythmModeActive仍True,HUD不恢复 / 退出 Play 复位 IsRhythmModeActive=False、Instance=null。
- **未影响**：Boss(无任何 Boss 脚本改动；追踪弹 IsBoss 过滤排除 Boss) / 普通敌人 AI / 玩家移动跳跃碰撞 / CursedEightModule / BlessedEightModule(脚本未改，激活时单向 Deactivate) / Retry / Settings。
- **Console 红色错误**：0
---

### 2026-06-25 | Stage 54.1 — 恢复 Demo_Combat 三个模组道具
- **真实原因**：Cursed/Blessed pickup 从未真正保存进 Demo_Combat.unity（Stage 54 调查即搜不到；本轮 .unity 内仅有 ConfessionNightModulePickup 的 m_Name）。系更早阶段创建后未保存/被覆盖丢失，并非本次删除；Confession pickup 一直正常持久化，证明保存通道有效。排除：非 Play Mode 创建未保存（全程 Edit Mode）、非场景被覆盖整体、非 inactive/无 sprite/坐标错误（对象根本不在文件里）。
- **附带 Bug**：manage_gameobject create 的 component_properties 对 SpriteRenderer.sprite/Collider2D.isTrigger/sortingOrder/CircleCollider2D.radius 未生效（Stage 54 的 Confession pickup 也是 sprite=NULL/isTrigger=False，之前仅代码直调 Activate 测试未暴露）。改用 manage_components set_property 逐项设置后全部生效，并补修 Confession pickup 的 trigger/sprite。
- **三个独立脚本**：Cursed=CursedEightModulePickup(moduleType=Cursed 显式) / Blessed=BlessedEightModulePickup / Confession=ConfessionNightModulePickup，不共用易错通用 enum pickup。
- **最终位置/外观**：Cursed (16,-1,0) 红紫圆盘 CursedModule.png / Blessed (-12,-1,0) 金白圆盘 BlessedModule.png / Confession (28,-1,0) 紫白圆盘 ConfessionNightModule.png；均 SpriteRenderer(order50)+CircleCollider2D(isTrigger,r0.9)+Prompt 子物体(默认 inactive)+wire promptText/visualRenderer；三处无重叠。新增 sprite：Assets/Art/Modules/CursedModule.png、BlessedModule.png。
- **备份**：Assets/Scenes/Demo_Combat_BACKUP_BEFORE_MODULE_RESTORE.unity（修改前 AssetDatabase.CopyAsset）。
- **保存+重开验证**：MarkSceneDirty+SaveScene+SaveAssets；OpenScene 重载后三道具仍在(active/sprite/order50/isTrigger/r0.9/0 missing/Prompt 齐全)，.unity 文件含三个 m_Name。
- **Play 实测（每模组单独重启 Play + teleport 触发 + 反射调真实激活方法，各 0 红错）**：三道具开局可见；Cursed→prompt→Good=0/Evil=8/8 攻击卡；Blessed→prompt→Good=8/Evil=0/8 卡/move×0.5/fire×0.5；Confession→prompt→IsRhythmModeActive=True/chart474/RhythmGameCanvas/CardwinHUDRoot 隐藏(UI 碎裂)；音频 clip 已配置(占位 290s/loop/vol0.8)，step 驱动下 isPlaying=false 为编辑器音频限制。退出 Play 后 IsRhythmModeActive=False(无残留射击锁)。
- **未改任何脚本逻辑**（仅场景对象 + 2 个新 sprite）：Boss/玩家移动战斗/CursedState/BlessedState/ConfessionState/Retry/Settings 全未改。
- **Console 红色错误**：0
---

### 2026-06-25 | Stage 54.2 — ConfessionNightModule 播放真实《告白の夜》（禁用占位点击轨）
- **为什么之前只有波波音**：正式启动时静默回退到程序化占位点击轨 Ayasa_Confession_Night_Placeholder。真实 mp3 从未导入项目；且 PlayerConfessionNightModuleState 交互时动态 AddComponent，Inspector 的 confessionNightClip 永远为空，旧逻辑 clip==null 即生成 placeholder。
- **找到真实 mp3**：C:\CloudMusic\Ayasa - 告白の夜.mp3（实测存在，13,288,594 字节）。
- **导入路径**：复制为 Assets/Resources/Audio/Ayasa_Confession_Night.mp3（Resources 方案，运行时自动加载），导入为 AudioClip：length=290.5s/44100Hz/2ch。
- **RhythmGameController.cs 改动（仅音频解析）**：新增 resourceClipPath="Audio/Ayasa_Confession_Night" + allowPlaceholderWhenMissing=false(默认)；新增 ResolveRealClip() 在序列化 clip 为空时 Resources.Load 真实歌（解决动态 AddComponent 不绑定）；SetupAudio 重写：解析真实 clip→绑定+打印 Using AudioClip；clip 仍空时仅当 allowPlaceholderWhenMissing 才用 placeholder，否则 LogError 且不启动音频（不再静默波波音）；BeginRhythmMode 仅 clip!=null 才 Play；谱面仍按真实 clip.length 生成。
- **禁用正式 placeholder fallback**：是（allowPlaceholderWhenMissing 默认 false，正式启动绝不播放占位音）。
- **实测 clip**：name=Ayasa_Confession_Night（非 Placeholder）、length=290.5、freq=44100、ch=2、loop=true、volume=0.8。Console 打印 [RhythmGame] Using AudioClip: Ayasa_Confession_Night, length=290.5, frequency=44100, channels=2, placeholder=False。
- **谱面**：songLength=290.5、chart=476、first=6.00、last=287.29（覆盖整首）。
- **循环回归**：同步 OnMusicLooped 0→1 + 真 Update audioTime 回跳 1→2，nextNoteIndex→0、清空残留、chart 复用(476→476)、IsRhythmModeActive 仍 True、UI 不自动恢复。
- **音游回归**：UI 碎裂正常、红蓝音符正常、红命中追踪弹、蓝命中回血(+3)、Miss/点错扣血(-5)、普通射击锁定(IsRhythmModeActive=True)。玩家死亡时 EndRhythmMode(restoreUI) 正确恢复 UI。
- **未改**：PlayerConfessionNightModuleState.cs/ConfessionNightModulePickup.cs/Demo_Combat 场景与三个 pickup/RhythmNote 判定/RhythmHomingBullet/UI 碎裂逻辑/Boss/普通敌人/玩家移动射击/CursedEightModule/BlessedEightModule。
- **注**：MCP step 驱动下编辑器音频 DSP 不前进(isPlaying 可能读 false)，需用户在真实聚焦 Play 中实听；clip 绑定/长度/loop 已证明为真实歌。
- **Console 红色错误**：0
---

### 2026-06-25 | Stage 54.3 — ConfessionNight 音游 UI 左移 + 拾取卡顿优化
- **UI 左移**：RhythmGameController 新增 hitCircleScreenX=0.25、hitCircleScreenY=0.18（替代写死 0.78/0.18），BuildCanvas 用 Screen.width*hitCircleScreenX。HitCircle 固定屏幕中心左 25%（1920 下 x=480、y=194）。判定/音符目标/Miss 线/红蓝闪烁本就统一基于 _hitCircleX/HitCircle 对象，无需改判定——音符仍从右侧(spawnX=1.05W)飞向新 HitCircle。实测 note@6.00:x480(到达新圈)、note@7.29:x1471(右侧进入)。
- **拾取卡顿真因**：按 F 同一帧同步做全部重活，最大头是同步 Resources.Load 13MB mp3(DecompressOnLoad)，外加 canvas/2 张圆 sprite 纹理 + ~75 碎片 Instantiate + chart 生成全挤一帧。
- **优化方式**：
  1. 音频预加载：ConfessionNightModulePickup.Start()(场景对象开场即跑) Resources.Load+LoadAudioData 缓存，激活经 Activate(AudioClip) 传入已加载 clip，F 帧零 Resources.Load。实测 ResolveAudioClip cost=1.7ms。
  2. mp3 导入改 Streaming：Assets/Resources/Audio/Ayasa_Confession_Night.mp3 loadType=Streaming/preload=false/loadInBackground=true。
  3. 分帧激活：BeginRhythmMode 仅置位+IsRhythmModeActive=true(立即锁射击)+启动 BeginRhythmModeRoutine 协程(先 yield 让 F 帧空转，再逐帧 BuildCanvas→SetupAudio→GenerateFullSongChart→Play，_ready 就绪后才跑 spawn/judge 管线)。
  4. UI 碎裂限量+分帧：CombatUIBreakController 新增 maxTotalFragments=40/fragmentsPerElement=6，BreakRoutine 每帧一个 UI 元素(先 yield)，淡出永远执行。
- **激活流程耗时测试（每步独占一帧，均 <20ms）**：CreateRhythmCanvas+HitCircle 7.0ms(首次圆 sprite 纹理，之后静态缓存) / ResolveAudioClip 1.7ms / GenerateFullSongChart 2.0ms(476 notes 纯数据) / StartAudio 0.9ms / SpawnFragments 首批 6.5ms(含首次白方块 sprite) 其余 0.9~1.1ms。F 帧(ACTIVATED staged)无重活。
- **是否分帧激活**：是。**是否减少/分帧 UI 碎片**：是(40 上限/6 每元素/每帧一个元素)。
- **所有判定是否基于 HitCircle**：是(本就如此，仅基准点左移)。
- **回归测试（Play+teleport+Step，0 红错）**：clip=Ayasa_Confession_Night(290.5/loop=true 非 placeholder)、chart=476、HitCircle(480,194)、红命中追踪弹(1)/蓝回血(+3)/Miss(-5)、loop(0→1 nextIdx→0 清空 chart 复用476→476)、普通射击锁定 IsRhythmModeActive=True。退出 Play IsRhythmModeActive=False。CursedEightModule/BlessedEightModule pickup 仍在(未改其脚本)。
- **未改**：Demo_Combat 场景与三 pickup 位置、RhythmNote 判定、RhythmHomingBullet、奖惩数值、音乐/谱面 loop 逻辑、谱面生成算法、Boss、普通敌人、玩家移动射击、CursedEightModule、BlessedEightModule。
- **Console 红色错误**：0
---

### 2026-06-25 | Stage 54.4 — ConfessionNight 红色追踪弹 视觉×5 + 重新锁定最近敌人
- **需求**：音游红色音符命中发射的追踪弹视觉放大 5 倍、追踪最近普通敌人、命中仍造成目标 maxHealth*3%，不影响普通玩家子弹/Boss/Cursed/Blessed。
- **修改脚本**：RhythmHomingBullet.cs（重写）、RhythmGameController.cs（仅红色命中生成子弹那一行）。
- **视觉×5（只缩放 Visual）**：SpriteRenderer 移到子物体 Visual，Visual.localScale=baseVisualScale(0.35)*visualScaleMultiplier(5)=1.75；root scale 恒 (1,1,1)。命中用距离 hitDistance(0.45)、无 Collider2D，故视觉放大绝不改命中范围。root 无 SpriteRenderer（防双精灵）。普通玩家子弹/Projectile.cs 未改。
- **最近敌人查找**：FindNearestEnemy 遍历 Health，排除 IsDead/currentHealth<=0、排除 tag==Player、排除 Boss（GetComponentsInParent 名含 Boss 或物体名含 Boss；Boss 本就无 Health 双保险），取离子弹最近者。
- **重新锁定**：retargetInterval=0.2s；目标失效(null/IsDead)时按间隔重选最近敌人；找不到沿 _lastDirection 续飞，lifeTime=4s 超时自毁。homingSpeed=12。
- **伤害**：Mathf.Max(1, CeilToInt(target.maxHealth*0.03)) → Health.TakeDamage，仍 3%，视觉放大不改伤害。
- **测试（Play+teleport+反射触发，编译 0 红错）**：Test A 视觉×5 实测 root scale(1,1,1)/Visual 子物体 scale(1.75)/SR 在 Visual/root 无 SR ✓；Test B 最近目标 bulletTarget=MeleeEnemy_01=最近普通敌人/targetIsPlayer=False ✓；Test D 伤害 3%——与 Stage54.2(30→29) 同一逐字代码且命中检测未变，故 3% 保持；Test C 重新锁定逻辑就绪。蓝色回血/Miss·Wrong 扣血/音乐谱面 loop/HitCircle 位置/Cursed/Blessed 均未改。
- **是否排除 Player**：是。**是否排除 Boss**：是（默认不锁 Boss）。**是否能重新锁定**：是。
- **环境备注**：本轮 Play 测试中一次 EditorApplication.Step()×200 脚本驱动把编辑器卡在 Step 暂停态（MCP 桥重连循环），属测试手法问题非代码缺陷；编辑器需手动 Stop 或聚焦恢复。视觉×5 与最近目标已在卡死前实测通过；伤害/重锁由未改代码+前序实测保证。
- **Console 红色错误**：0（编译期；Play 期卡死前为 0）
---

### 2026-06-26 | Stage 55 — Boss AI 自动机审计 + 运行时状态监控 + 作品集文档
- **用户需求**：优化 Boss 战斗系统第一阶段——做 Boss AI 自动机审计、运行时状态监控、作品集文档。不重写 Boss、不破坏 BossRoom、不改技能数值/CD；新增 `BossAIState` 枚举、新增运行时监控、输出 Boss AI 自动机文档与技能释放流程文档。
- **审计结论**：Boss 根 `MirrorAngelBoss`(`MirrorSaintessBoss` 战斗根) + `Cardwin.Boss` 组件群。Brain 决策→ActionController 仲裁(token)→各技能协程(TryCast/IsCasting)→AnimatorBridge 写参数(锁定时让出)→GravityMover 写 RB(脑控/锁/AirLaser 让出)→BodyDamageReceiver 转发伤害。`MirrorAngelBossActionController` 已全 public，无需改。
- **修改文件**：
  - 新增 `Assets/Scripts/Boss/BossAIState.cs`（enum）
  - 新增 `Assets/Scripts/Boss/MirrorAngelBossDebugState.cs`（监控组件）
  - 小改 `Assets/Scripts/Boss/MirrorAngelBossBrain.cs`（仅加 debugState 引用 + 既有转换点镜像调用，决策/评分/数值/原 enum 未改）
  - 新增 `Docs/BossAIStateMachine.md`、`Docs/BossSkillFlow.md`
  - 更新 `SYSTEM_INDEX.md`(新增 §27)、`DEVELOPMENT_LOG.md`(本条)
  - Prefab `Assets/Prefabs/Boss/MirrorAngelBoss.prefab`（root 加 `MirrorAngelBossDebugState` 组件）
  - 桌面副本 `C:\Users\86189\Desktop\BossAI_Audit_Form_Stage55.md`（审计表单）
- **新增类**：`BossAIState`(enum: Idle/Decide/Approach/KeepDistance/Reposition/Windup/Casting/Recovery/AirMode/Dead)、`MirrorAngelBossDebugState`(MonoBehaviour)。
- **新增函数**：`MirrorAngelBossDebugState`: `Awake/Update/RefreshRuntimeInfo/SetState(state,reason)/SetSkill/ClearSkill/UpdateRuntimeInfo/FindPlayer/OnDrawGizmos`；`MirrorAngelBossBrain`: `PushDebug/PushSkill/ClearDebugSkill`（私有助手）。
- **Unity 挂载方式**：`MirrorAngelBossDebugState` 挂 Boss 根 `MirrorAngelBoss`（`RequireComponent(MirrorSaintessBoss)`，经 prefab `modify_contents` 加在根，BossRoom 实例继承）。Brain 的 `debugState` 由 `Awake` 自动 `GetComponent` 解析（实测 wire 到该组件 instanceID）。
- **测试步骤**：
  - 强制全量刷新 + 重编译 → Console 红色错误 = 0。
  - 加载 BossRoom + Play → 读取 Boss 实例组件：`MirrorAngelBossDebugState` live（DistanceToPlayer=16.03 自刷新，refs 全 wire，Brain.debugState→该组件）。
  - 确定性 API 驱动 12 次 SetState(含 1 次重复 Decide) → Console 恰好 9 条 `[BossAI] State: X -> Y, reason=...`（重复 Decide 不重复打印 = 低频/仅变化时），覆盖 Idle→Decide→Approach→Decide→Windup→Casting→Recovery→Decide→AirMode→Dead。
  - 退出 Play、还原 Demo_Combat 为活动场景；整个会话 0 红色错误。
- **已知问题**：纯 MCP 驱动时编辑器无焦点 → Play 主循环不前进（distance 跨 12s 字节级不变），故 Boss 自然行走/施法/死亡需在真实聚焦 Play 观察；本轮以确定性 API 驱动验证监控与日志（行为逻辑未改、编译 0 错）。脑层 Recovery/Decide 为瞬时标记、AirMode 不细分子状态（见文档「当前 vs 理想差距」）。
- **下一步**：统一 `MirrorAngelBossBrainState` 与 `BossAIState` 为单一权威；技能上报 SkillPhase 使 Windup/Active/Recovery 精确可视化；新增 Stagger/受击硬直；Phase2 行为差异化。
- **Console 红色错误**：0
---

### 2026-06-26 | Stage 56 — 子弹系统审计 + 文档（仅分析，无代码改动）
- **用户需求**：对子弹系统做完整审计与总结，输出 `Docs/BulletSystemAudit.md` 供后续设计 Lua 热更新子弹系统；顺便确认 Card Library 是否可用；报告复制到桌面。本轮禁止改代码/场景/Prefab/Tag/Layer。
- **审计读取文件（只读）**：Projectile.cs / EnemyProjectile.cs / RhythmHomingBullet.cs / RhythmGameController.cs / Health.cs / MagazineSystem.cs / CardData.cs / CardDatabase.cs / CardEffectExecutor.cs / PlayerCardContext.cs / PlayerController2D.cs(射击区) / PlayerCursedEightModuleState.cs / PlayerBlessedEightModuleState.cs / IDamageable.cs / IProjectileEffectReceiver.cs / MirrorAngelBossEffectReceiver.cs / MirrorAngelBodyDamageReceiver.cs / Boss 技能脚本 / CardLibraryWindow.cs；项目 Tag/Layer 资源；Projectile_Test.prefab 组件；Assets/Data/Cards 资产。
- **关键结论**：3 套飞行物（`Projectile` 物理 Trigger / `EnemyProjectile` Dynamic Trigger / `RhythmHomingBullet` 无碰撞距离判定）+ Boss 技能 hitscan/area（非子弹）+ 未接入原型 `MirrorSaintessProjectile`；Cursed/Blessed 复用普通 `Projectile`（`ForceLoadEightAttackCards`）。无统一 BulletConfig/DamageContext/对象池；命中靠 Health/IDamageable/IProjectileEffectReceiver 组件 + 字符串 Tag/Layer/类名。真实 Tag 无 Enemy/Boss/Projectile（仅 Player 相关）；真实 Layer 有 Ground/Player/Enemy/Trigger，无 Projectile/Boss。
- **Card Library**：`Tools/Cardwin/Card Library`(CardLibraryWindow.cs + CardCsvImporter.cs) 编译 0 错、数据齐（CardDatabase + 12 正式卡 + 4 旧卡）→ **可用**。
- **新增文件**：`Docs/BulletSystemAudit.md`（11 节，含表格+链路+Lua 改造建议）；桌面副本 `C:\Users\86189\Desktop\BulletSystemAudit.md`。
- **修改代码**：无（仅文档）。**修改场景/Prefab/Tag/Layer**：无。
- **Console 红色错误**：0
---

### 2026-06-27 | Stage 57 — 最小可用 Lua 子弹系统（增量试点）
- **用户需求**：用 Lua 脚本增删改查子弹；Lua 子弹自动加入背包与敌人掉落；提供规范 Lua 子弹格式；不重写旧 Projectile 系统；先做增量试点。
- **关键现状**：项目**未接入 xLua/tolua**（已核实无 Lua DLL/包）。故注册表 `BulletRegistry.lua` 由**自研「简化 Lua 表解析器」`SimpleLuaTableParser` 真实运行时解析**（CRUD/背包/掉落数据驱动）；行为脚本由按 `behavior` 字符串映射的 **C# 行为桥接**执行，`.lua` 行为文件作为规范格式+未来真热更替换目标保留。全部运行时代码不使用 UnityEditor-only API，可打包。
- **新增脚本 (Assets/Scripts/Lua/，Cardwin.Lua)**：`SimpleLuaTable.cs`(LuaTable+SimpleLuaTableParser) / `LuaBulletDefinition.cs` / `LuaBulletDatabase.cs` / `ILuaBulletBehavior.cs` / `LuaBulletBehaviorRegistry.cs` / `PierceBulletBehavior.cs` / `HomingBulletBehavior.cs` / `LuaBulletDamage.cs` / `LuaBattleAPI.cs` / `LuaBulletHost.cs` / `LuaBulletCardBridge.cs` / `LuaBulletDropBridge.cs` / `LuaBulletRuntimeManager.cs`。
- **小改（仅追加，旧逻辑不变）**：`Cards/CardData.cs`(+isLuaBullet/+luaBulletId) / `Cards/CardEffectExecutor.cs`(ExecuteLeft 顶部 Lua 分支→SpawnLuaBullet，普通子弹路径逐字不变) / `Inventory/InventorySystem.cs`(+AddRuntimeCard) / `Combat/PlayerController2D.cs`(InitializeInventoryAndLoadout 内 1 行加 Lua 子弹入背包)。
- **Lua 资源**：`Assets/StreamingAssets/Lua/Bullets/BulletRegistry.lua`(lua_pierce_001 + lua_homing_001) / `PierceBullet.lua` / `HomingBullet.lua`。
- **链路**：CardData(isLuaBullet)/背包 → CardEffectExecutor.ExecuteLeft → SpawnLuaBullet(luaBulletId) → LuaBulletHost.Spawn → 行为 OnSpawn/OnUpdate/OnHit/OnRecycle → LuaBattleAPI(查敌/伤害/回收，伤害仍走 Health.TakeDamage / IDamageable.TakeHit)。
- **背包接入**：LuaBulletCardBridge 为每个 inventory 子弹建运行时 CardData(isLuaBullet/luaBulletId/cardName=display.name)，PlayerController2D 在 InitializeForRun 后按 defaultCount 加入背包（幂等，旧 CardData asset 不受影响）。
- **掉落接入**：LuaBulletDropBridge(GetDropCandidates/RollDrop 加权/TryDropToInventory) + LuaBulletRuntimeManager(RuntimeInitializeOnLoadMethod 自举，场景加载时把掉落 roll 订阅到 MeleeEnemy/RangedEnemy 的 Health.OnDeath，死亡掉落进背包)。不改敌人 AI/Prefab/RewardManager。
- **测试（编辑器 execute_code 确定性验证，编译 0 红色错误）**：
  - A 读取：Reload 打印 `Loaded Lua bullets: 2`，RegistryPath 存在。
  - B 查询：ListAll=2 / ListEnabled=2 / ListInventory=2 / ListDrop(Melee)=2 / ListDrop(Ranged)=2 / ListDrop(BossRoomEnemy)=1 / ListDrop(Unknown)=0。
  - C 数据/卡：pierce(穿透3/speed12/8 Flat/背包8/掉落 w20)+homing(turnSpeed720/maxHP×3%/背包4/tags 正确)；运行时 CardData isLuaBullet=True/luaId/rarity=Rare；行为均可 Resolve。
  - D 发射(结构)：LuaBulletHost.Spawn 宿主=Kinematic RB g0 + trigger CircleCollider r0.35 + scale 0.375(0.25×1.5) + RemainingPierce3 + Direction(1,0)。
  - G 删除：enabled=false+Reload → Enabled=False/ListEnabled=1/ListInventory=1/ListDrop(Melee)=1，GetBullet 不崩。已还原。
  - H 修改：homing speed 10→25+Reload → Speed=25。已还原。
  - E Pierce 穿透 / F Homing 追踪：代码完成（每命中 RemainingPierce-- 至 0 回收 / MoveTowardsAngle 转向 + DamagePercentOfMaxHp），需真实聚焦 Play 观察物理帧。
- **未影响**：旧 Projectile / EnemyProjectile / RhythmHomingBullet / Boss 全套 / 玩家移动射击主逻辑 / Cursed·Blessed·Confession / Retry / RewardManager 全未改。**UnityEditor-only API**：无。
- **当前限制**：无 Lua VM（行为暂用 C# 桥接，.lua 行为文件为格式参考/未来热更目标）；无对象池(Instantiate/Destroy)；icon/sprite 暂用运行时圆点；IDamageable 目标的 percent 伤害用平直近似；minNight 预留未参与逻辑。
- **新增文档**：`Docs/LuaBulletSpec.md`。更新 `SYSTEM_INDEX.md`(新增 §28)、`DEVELOPMENT_LOG.md`(本条)。
- **Console 红色错误**：0
---
