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


