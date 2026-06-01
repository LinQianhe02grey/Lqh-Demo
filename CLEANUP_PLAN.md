# CLEANUP_PLAN.md — 冗余和清理建议总表

> 生成时间：2026-06-01 | Stage 11A
> **重要：本阶段不执行任何删除操作。所有清理建议供下阶段参考。**

---

## A. 可以保留（核心系统）

| 对象/脚本 | 当前用途 | 风险 | 建议动作 |
|---|---|---|---|
| PlayerController2D.cs | 玩家输入/移动/射击 | 无 | 保留 |
| Health.cs | 通用血量/格挡/死亡 | 无 | 保留 |
| Projectile.cs | 玩家子弹 | 无 | 保留 |
| CardEffectExecutor.cs | 卡牌效果执行唯一入口 | 无 | 保留 |
| CardData.cs | 卡牌数据 ScriptableObject | 无 | 保留 |
| CardDatabase.cs | 卡牌总表 | 无 | 保留 |
| MagazineSystem.cs | 弹夹/装弹/Loadout | 无 | 保留 |
| InventorySystem.cs | 背包存储 | 无 | 保留 |
| MagazineEditUI.cs | 背包/弹夹编辑 UI | 无 | 保留 |
| CombatHUD.cs | 战斗 HUD | 无 | 保留 |
| CardSlotUI.cs | 卡槽 UI 组件 | 无 | 保留 |
| MagazinePreviewUI.cs | 3发预览 | 无 | 保留 |
| MeleeEnemyController.cs | 近战 AI | 无 | 保留 |
| RangedEnemyController.cs | 远程 AI | 无 | 保留 |
| EnemyProjectile.cs | 敌人子弹 | 无 | 保留 |
| EnemyHealthBarUI.cs | 敌人 HP UI | 无 | 保留 |
| ComboRatingSystem.cs | 连击评分 | 无 | 保留 |
| PlayerAlignment.cs | Good/Evil 属性 | 无 | 保留 |
| RewardManager.cs | 击杀三选一奖励 | 无 | 保留 |
| CameraFollow2D.cs | 摄像机跟随 | 无 | 保留 |
| HUDRuntimeBootstrapper.cs | HUD 自动绑定 | 无 | 保留 |
| CardLibraryWindow.cs | Editor 卡牌管理 | 无 | 保留 |
| CardCsvImporter.cs | CSV 导入 | 无 | 保留 |
| CardDatabaseEditorUtility.cs | Database 重建 | 无 | 保留 |
| CardConfigValidator.cs | 配置检查 | 无 | 保留 |
| Player GameObject | 12 组件完整 | 无 | 保留 |
| Canvas GameObject | UI 容器 | 无 | 保留 |
| 6 正式敌人 | 3近战+3远程 | 无 | 保留 |
| 3 Enemy Prefabs | Melee/Ranged/Projectile | 无 | 保留 |
| Projectile_Test.prefab | 玩家子弹 | 无 | 保留 |

---

## B. 标记 Deprecated / Legacy（本阶段不删）

| 对象/脚本 | 当前用途 | 风险 | 建议动作 | 需要用户确认？ |
|---|---|---|---|---|
| EnemyController.cs | Legacy 旧敌人控制器 | 仅挂在 Disabled Enemy_Test_OLD | 标记 Legacy，下阶段可删除 | 是 |
| DemoSceneRuntimeBootstrapper.cs | Legacy 场景配置 | 不挂载任何正式对象 | 标记 Legacy，下阶段评估是否删除 | 是 |
| CardwinSceneBuilder.cs | Stubbed 禁用工具 | 仅弹窗提示 | 标记 Deprecated，可移除 MenuItem | 是 |
| CardAssetCreator.cs | 创建旧命名卡牌 | 生成与 C001~C012 重复的旧资产 | 隐藏菜单或标记 Legacy | 是 |
| Enemy_Test_OLD | 旧敌人测试对象 | Disabled，无影响 | 安全删除 | 是 |
| Platform_Z4/5/6_High | 不可见高台平台 | Disabled，无影响 | 安全删除 | 是 |
| SampleScene.unity | Unity 默认场景 | 未使用 | 安全删除 | 是 |
| Strike.asset (旧) | 与 C001_Strike 重复 | 占用 CardDatabase 槽位 | 从 CardDatabase 移除（资产保留） | 是 |
| Guard.asset (旧) | 与 C004_Guard 重复 | 同上 | 同上 | 是 |
| Heal.asset (旧) | 与 C005_Heal 重复 | 同上 | 同上 | 是 |
| Focus.asset (旧) | 与 C006_Focus 重复 | 同上 | 同上 | 是 |
| MagazineFullBarUI.cs | 保留给未来用 | 战斗 HUD 不创建 | 标记 Retained | 是 |
| CameraBounds | 视觉参考 | 无 Collider | 可安全删除 | 否 |
| SpawnPoint_Player | 视觉参考 | 无 Collider | 可安全删除 | 否 |
| SpawnPoint_Enemy | 视觉参考 | 无 Collider | 可安全删除 | 否 |

---

## C. 高风险（暂不动）

| 对象/脚本 | 当前用途 | 风险 | 建议动作 |
|---|---|---|---|
| PlayerController2D | 核心玩家逻辑 | 极高 | 不动 |
| MagazineSystem | 弹夹核心 | 极高 | 不动 |
| CardEffectExecutor | 效果唯一入口 | 极高 | 不动 |
| InventorySystem | 背包核心 | 高 | 不动 |
| RewardManager | 奖励核心 | 高 | 不动 |
| Health.cs | 共用组件 | 极高 | 不动 |
| CardData.cs | 数据定义 | 极高 | 不动 |
| Demo_Combat.unity | 主场景 | 极高 | 不动 |
| CardDatabase.asset | 卡牌总表 | 极高 | 保持，但可清理旧资产引用 |

---

## D. Stub 脚本（骨架代码）

| 脚本 | 实现程度 | 建议 |
|---|---|---|
| GameManager.cs | 全 stub | 保留，在需要场景切换/状态管理时实现 |
| GameStateMachine.cs | 全 stub | 保留 |
| ShopManager.cs | 全 stub | 保留，商店系统开发时实现 |
| EconomySystem.cs | CanAfford 有实现，其余 stub | 保留 |
| InventoryUI.cs | 全 stub | **可能不需要**（MagazineEditUI 已替代） |
| ShopUI.cs | 全 stub | 保留，商店系统开发时实现 |
| BattleLogger.cs | 全 stub | 保留，分析系统需要时实现 |

---

## E. 清理优先级

| 优先级 | 对象 | 影响 | 难度 |
|---|---|---|---|
| **P0 (下阶段)** | 从 CardDatabase 移除旧 Strike/Guard/Heal/Focus asset | 减少数据库噪音 | 低（Editor 操作） |
| **P0 (下阶段)** | 删除 Enemy_Test_OLD | 清理场景 | 低 |
| **P0 (下阶段)** | 删除 Platform_Z4/5/6_High | 清理场景 | 低 |
| **P1** | 删除 SampleScene.unity | 清理项目 | 低 |
| **P1** | 隐藏/移除 Rebuild Clean Demo Scene 菜单 | 避免误触 | 低 |
| **P1** | 标记 Create Basic Card Assets 为 Legacy | 减少重复资产风险 | 低 |
| **P2** | 删除 CameraBounds/SpawnPoint_Player/SpawnPoint_Enemy | 清理场景参考对象 | 低 |
| **P2** | 删除 InventoryUI.cs（如确定不需要） | 减少代码噪音 | 低 |
| **P3** | 考虑是否删除 EnemyController.cs / DemoSceneRuntimeBootstrapper.cs | 清理 Legacy 脚本 | 中 |

---

## F. 清理后预期

清理 P0~P1 后：
- 场景根对象从 13 → 10
- CardDatabase 从 17 引用 → 13 引用（12 正式卡 + CardDatabase 自身排除）
- Tools 菜单从 6 → 4 可见项
- CardData 目录从 17 asset → 13 asset（4 旧资产可选择保留或归档）
- 0 功能影响

---

## G. 已完成清理任务

### Stage 11B (Safe Cleanup Pass)
- CardDatabase 17→12 正式卡（C001~C012）
- 删除 Enemy_Test_OLD
- 删除 3 个禁用高台（Platform_Z4/5/6_High）
- Tools 菜单 Legacy 化（Rebuild Scene / Create Basic Cards 移到 Legacy 子菜单）

### Stage 11C (Post-Cleanup Regression Test)
- 全功能回归测试：11 项全部 PASS
- Console Error = 0
- 详见 REGRESSION_TEST_REPORT.md

### Stage 11D (Archive Legacy Card Assets)
- 旧 Strike.asset / Guard.asset / Heal.asset / Focus.asset 移动到 Assets/Data/Cards/Legacy/
- CardDatabase 仅保留 C001~C012 正式卡（12 张）
- CardLibraryWindow 增加 "Show Legacy Cards" 开关（默认 false）
- 旧卡显示时标记为 [Legacy]
- CardConfigValidator 默认排除 Legacy 目录
- Legacy 资产单独报告为 Info（不计入重复 Error）
- 未删除任何资产
