# SCENE_STRUCTURE_AUDIT.md — 场景对象审计

> 生成时间：2026-06-01 | Stage 11A
> 场景：`Assets/Scenes/Demo_Combat.unity`

---

## 场景根对象总表

| # | 路径 | 对象名 | 组件 | Layer | Tag | Active | 是否运行时必须 | 风险 | 建议 |
|---|---|---|---|---|---|---|---|---|---|
| 1 | / | MainCamera | Transform, Camera, CameraFollow2D | 0 (Default) | MainCamera | Yes | **必须** | 无 | 保持 |
| 2 | / | Ground | Transform, SpriteRenderer, BoxCollider2D | 8 (Ground) | Untagged | Yes | **必须** | 无 | 保持 |
| 3 | / | Platform_1 | Transform, SpriteRenderer, BoxCollider2D | 8 (Ground) | Untagged | Yes | **必须** | 无 | 保持 |
| 4 | / | Platform_2 | Transform, SpriteRenderer, BoxCollider2D | 8 (Ground) | Untagged | Yes | **必须** | 无 | 保持 |
| 5 | / | Platform_3 | Transform, SpriteRenderer, BoxCollider2D | 8 (Ground) | Untagged | Yes | **必须** | 无 | 保持 |
| 6 | / | CameraBounds | Transform, SpriteRenderer | 0 | Untagged | Yes | 可选 (参考) | 无 Collider | 可删除或保留为视觉参考 |
| 7 | / | Player | Transform, SpriteRenderer, Rigidbody2D, CapsuleCollider2D, PlayerController2D, Health, MagazineSystem, InventorySystem, CardEffectExecutor, PlayerAlignment, ComboRatingSystem, RewardManager | 9 (Player) | Player | Yes | **必须** | 无 | 保持 |
| 8 | / | Enemy_Test_OLD | Transform, SpriteRenderer, Rigidbody2D, BoxCollider2D, Health, EnemyController | 10 (Enemy) | Untagged | **No (disabled)** | 否 | 无 | **下阶段安全删除** |
| 9 | / | SpawnPoint_Player | Transform, SpriteRenderer | 0 | Untagged | Yes | 可选 (视觉参考) | 无 | 可保留或删除 |
| 10 | / | SpawnPoint_Enemy | Transform, SpriteRenderer | 0 | Untagged | Yes | 可选 (视觉参考) | 无 | 可保留或删除 |
| 11 | / | BossDoor_Placeholder | Transform, SpriteRenderer, BoxCollider2D | 11 (Trigger) | Untagged | Yes | 可选 (展位) | 无 | 可保留 |
| 12 | / | Canvas | RectTransform, Canvas, CanvasScaler, GraphicRaycaster, CombatHUD, MagazineEditUI | 0 | Untagged | Yes | **必须** | 无 | 保持 |
| 13 | / | LevelRoot | Transform | 0 | Untagged | Yes | 可选 (容器) | 无 | 保持 |

---

## LevelRoot 子层级

| 路径 | 子级 | 说明 |
|---|---|---|
| LevelRoot/Platforms | Platform_Z4_High, Platform_Z5_High, Platform_Z6_High | 3 个高台平台（**已禁用** — Stage 8A.10） |
| LevelRoot/Enemies | 3x MeleeEnemy + 3x RangedEnemy | 6 个正式敌人，编辑模式可见 |
| LevelRoot/Props | (空容器) | 预留给未来场景物件 |
| LevelRoot/FinishGate | FinishGateTrigger | 关卡终点触发 (FinishGateTrigger.cs) |

---

## Player 子物体

| 路径 | 组件 | 说明 |
|---|---|---|
| Player/FirePoint | Transform | 子弹发射点位置 |
| Player/GroundCheck | Transform | 地面检测锚点 |

---

## Layer 配置

| Layer ID | Name | 用途 |
|---|---|---|
| 8 | Ground | 地面/平台，Solid Collider |
| 9 | Player | Player，与 Enemy 层忽略碰撞 (Physics2D.IgnoreLayerCollision) |
| 10 | Enemy | 敌人，Trigger Collider |
| 11 | Trigger | BossDoor / 触发区 |

---

## 正式 vs 旧对象

| 类型 | 对象 | 状态 |
|---|---|---|
| **正式** | Player, MainCamera, Canvas, Ground, Platform_1/2/3, LevelRoot (Enemies ×6, FinishGate), BossDoor_Placeholder | Active |
| **旧/禁用** | Enemy_Test_OLD | Disabled |
| **旧/禁用** | Platform_Z4_High, Platform_Z5_High, Platform_Z6_High | Disabled (Stage 8A.10) |
| **参考** | CameraBounds, SpawnPoint_Player, SpawnPoint_Enemy | Active (仅视觉) |

---

## 可安全删除的对象

| 对象 | 原因 | 风险 |
|---|---|---|
| Enemy_Test_OLD | 已被正式 6 敌人取代，disabled | 无 |
| Platform_Z4/5/6_High | 已被 Stage 8A.10 禁用，不可视，遮挡子弹 | 无 |
| CameraBounds | 仅 SpriteRenderer 视觉参考，无 Collider | 极低 |
| SpawnPoint_Player / SpawnPoint_Enemy | 仅 SpriteRenderer 视觉参考 | 极低 |

---

## 绝对不能删除的对象

| 对象 | 原因 |
|---|---|
| Player | 核心 Gameplay |
| Canvas | UI 容器 |
| MainCamera | 渲染 + 摄像机跟随 |
| Ground / Platform_1/2/3 | 地形 |
| LevelRoot/Enemies (6个) | 战斗内容 |
| BossDoor_Placeholder | 关卡进度标记 |

---

## 场景正确性检查

| 检查项 | 结果 |
|---|---|
| Player 是否有完整组件 | ✅ 12 个组件 |
| Player Tag = "Player" | ✅ |
| Player Layer = 9 (Player) | ✅ |
| Enemy Layer = 10 | ✅ |
| Ground Layer = 8 | ✅ |
| 无空气墙阻挡跳跃 | ✅ |
| 摄像机跟随 | ✅ CameraFollow2D 挂载 |
| HUD 可见 | ✅ CombatHUD + HUDRuntimeBootstrapper |
| 无 C# Error | ✅ (Console 0 errors) |
