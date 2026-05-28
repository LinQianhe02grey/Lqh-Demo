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


