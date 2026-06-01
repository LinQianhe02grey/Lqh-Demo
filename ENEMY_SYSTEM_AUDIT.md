# ENEMY_SYSTEM_AUDIT.md — 敌人系统冗余审计

> 生成时间：2026-06-01 | Stage 11A

---

## 敌人实现概览

| 项目 | 当前状态 | 是否冗余 | 建议 |
|---|---|---|---|
| `EnemyController.cs` | Legacy，仅挂在 `Enemy_Test_OLD`（disabled） | **是** | 保留不删除，已被取代 |
| `MeleeEnemyController.cs` | 正式近战 AI 实现，场景中 3 个实例 | **否**（正式版） | 保持 |
| `RangedEnemyController.cs` | 正式远程 AI 实现，场景中 3 个实例 | **否**（正式版） | 保持 |
| `EnemyProjectile.cs` | 正式敌人子弹实现 | **否**（唯一版本） | 保持 |
| `EnemyHealthBarUI.cs` | 正式敌人 HP/Shield UI | **否**（唯一版本） | 保持 |
| `Enemy_Test_OLD` | 场景中 disabled 旧测试敌人 | **是** | 下阶段可安全删除 |
| `DemoSceneRuntimeBootstrapper` | Legacy 运行时配置脚本，不再挂载到 LevelRoot | **是** | 保留不删除 |
| 运行时生成敌人代码 | `DemoSceneRuntimeBootstrapper.ConfigureEnemy()` 按名称查找 + 配置 | **是**（但已不挂载） | 保留不删除 |
| 编辑模式可见性 | 6 个正式敌人均有完整 SpriteRenderer/Collider/Rigidbody，编辑模式可见 | **否** | ✅ 已验证 |

---

## 敌人实例清单（Demo_Combat.unity）

| 路径 | 名称 | 控制器 | HP | 状态 |
|---|---|---|---|---|
| `LevelRoot/Enemies/` | MeleeEnemy_01 | MeleeEnemyController | 30 | Active |
| `LevelRoot/Enemies/` | MeleeEnemy_02 | MeleeEnemyController | 30 | Active |
| `LevelRoot/Enemies/` | MeleeEnemy_03 | MeleeEnemyController | 30 | Active |
| `LevelRoot/Enemies/` | RangedEnemy_01 | RangedEnemyController | 20 | Active |
| `LevelRoot/Enemies/` | RangedEnemy_02 | RangedEnemyController | 20 | Active |
| `LevelRoot/Enemies/` | RangedEnemy_03 | RangedEnemyController | 20 | Active |
| (root level) | Enemy_Test_OLD | EnemyController (legacy) | 50 | **Disabled** |

---

## 敌人 Prefab

| Prefab | 引用 | 状态 |
|---|---|---|
| `MeleeEnemy.prefab` | SpriteRenderer(red) + Rigidbody2D(Kinematic) + BoxCollider2D + Health(30) + MeleeEnemyController + EnemyHealthBarUI | Active |
| `RangedEnemy.prefab` | SpriteRenderer(purple) + Rigidbody2D(Kinematic,g=0) + BoxCollider2D + Health(20) + RangedEnemyController(binds EnemyProjectile)+ EnemyHealthBarUI | Active |
| `EnemyProjectile.prefab` | SpriteRenderer(purple,sortingOrder=150) + Dynamic Rigidbody2D(g=0,Continuous) + CircleCollider2D(Trigger) + EnemyProjectile | Active |

---

## EnemyController.cs (Legacy) 引用分析

| 引用者 | 方式 | 影响 |
|---|---|---|
| `Enemy_Test_OLD` | 场景挂载 | 无（对象 disabled） |
| `DemoSceneRuntimeBootstrapper.cs` | 代码引用（`Cardwin.Enemies`） | 无（脚本不再挂载） |
| 无其他正式对象挂载此脚本 | — | — |

**结论**：`EnemyController.cs` 对正式游戏逻辑无影响，可以安全保留为历史参考。

---

## 敌人系统正确性检查

| 检查项 | 结果 |
|---|---|
| 正式敌人是否只使用 MeleeEnemyController / RangedEnemyController | ✅ |
| 是否仅一套 EnemyProjectile | ✅ |
| 是否仅一套 EnemyHealthBarUI | ✅ |
| 敌人是否编辑模式可见 | ✅ |
| 敌人是否静态实例（非运行时生成） | ✅ |
| 是否有运行时 Spawner | ❌ 无 |
| 是否有多套 AI 并行 | ❌ 无（legacy 已被动禁用） |
| Layer 是否正确（Enemy=10, Player=9） | ✅ |
| 碰撞是否使用 Physics2D.IgnoreLayerCollision | ✅（Bootstrapper 中，但 Bootstrapper 已不挂载） |

---

## 冗余清理建议

| 对象 | 安全删除？ | 影响 | 建议 |
|---|---|---|---|
| `Enemy_Test_OLD` | **是** | 无。已 disabled，使用 legacy EnemyController | 下阶段删除 |
| `EnemyController.cs` | **谨慎** | Enemy_Test_OLD 引用它；DemoSceneRuntimeBootstrapper 代码引用 | 保留。如删除 Enemy_Test_OLD，可一起删除此脚本。 |
| `DemoSceneRuntimeBootstrapper.cs` | **谨慎** | 不在任何正式对象上挂载，但包含场景配置逻辑历史 | 保留为备份。如确认不再需要运行时配置，可删除。 |
| MeleeEnemy prefab/controller | **否** | 3 个正式近战敌人 | 保留 |
| RangedEnemy prefab/controller | **否** | 3 个正式远程敌人 | 保留 |
