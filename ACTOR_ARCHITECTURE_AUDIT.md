# ACTOR_ARCHITECTURE_AUDIT.md — 角色属性架构审计

> 生成时间：2026-06-01 | Stage 11A

---

## 玩家组件（Player GameObject）

| 组件 | 类型 | 职责 |
|---|---|---|
| Transform | Unity | 位置/旋转/缩放 |
| SpriteRenderer | Unity | 玩家视觉 (placeholder PNG) |
| Rigidbody2D | Unity | Dynamic 物理 |
| CapsuleCollider2D | Unity | 碰撞检测 |
| PlayerController2D | Cardwin.Combat | 输入/移动/跳跃/冲刺/射击/InputLock |
| Health | Cardwin.Combat | HP/护盾/受击/死亡/无敌 |
| MagazineSystem | Cardwin.Magazine | 8发弹夹/装弹/预览/Loadout |
| InventorySystem | Cardwin.Inventory | 背包存储 |
| CardEffectExecutor | Cardwin.Cards | 卡牌效果执行 |
| PlayerAlignment | Cardwin.Combat | Good/Evil 属性 |
| ComboRatingSystem | Cardwin.Combat | 连击评分 |
| RewardManager | Cardwin.Combat | 击杀奖励三选一 |

**Tag**: `Player` | **Layer**: `9` (Player)

---

## 敌人组件（正式敌人 — MeleeEnemy Prefab）

| 组件 | 类型 | 职责 |
|---|---|---|
| Transform | Unity | 位置/旋转/缩放 |
| SpriteRenderer | Unity | 敌人视觉 (红色 placeholder) |
| Rigidbody2D | Unity | Kinematic 物理 |
| BoxCollider2D | Unity | 碰撞/触发 |
| Health | Cardwin.Combat | HP/护盾/受击/死亡/无敌 |
| MeleeEnemyController | Cardwin.Enemies | 近战AI：Patrol/Chase/Attack/Return |
| EnemyHealthBarUI | Cardwin.Enemies | HP/Shield bar (OnGUI) |

## 敌人组件（正式敌人 — RangedEnemy Prefab）

| 组件 | 类型 | 职责 |
|---|---|---|
| Transform | Unity | |
| SpriteRenderer | Unity | 紫色 placeholder |
| Rigidbody2D | Unity | Kinematic (gravityScale=0) |
| BoxCollider2D | Unity | |
| Health | Cardwin.Combat | |
| RangedEnemyController | Cardwin.Enemies | 远程AI：巡逻/索敌/射弹 |
| EnemyHealthBarUI | Cardwin.Enemies | |

**Tag**: `Untagged` | **Layer**: `10` (Enemy)

---

## 共享能力分析

| 共享能力 | 当前实现 | 是否需要抽象 | 建议方案 |
|---|---|---|---|
| HP/血量 | `Health` 组件（Player + Enemies 共用） | **不需要** | 当前组件组合方案正确。Health 是独立 MonoBehaviour，挂载即可。 |
| Shield/格挡 | `Health.currentBlock` + `Health.GainBlock(int)` | **不需要** | 共用。通过 CardEffectExecutor 统一施加。 |
| Damage/受击 | `Health.TakeDamage(int)` | **不需要** | 共用。Player 受击来自 EnemyProjectile/EnemyController；Enemy 受击来自 Projectile。 |
| Heal/治疗 | `Health.Heal(int)` | **不需要** | 共用。仅 Player 通过 CardEffectExecutor 治疗自己（右键）。 |
| Death/死亡 | `Health.Die()` → Destroy(gameObject) | **不需要** | 共用。Health.OnDeath UnityEvent 供 RewardManager 订阅。 |
| HP/Shield UI | Player: `CombatHUD.TopLeftStats` | **不需要** | Player UI 和 Enemy UI 职责不同，不需要统一。 |
| | Enemy: `EnemyHealthBarUI.OnGUI()` | | |
| Team/Faction | **当前无** | **未来可选** | 如需团队/阵营/友军伤害判断，可新增 `ActorIdentity` 组件。当前 Player/Enemy 通过 Layer 和 Tag 区分。 |
| Invincible/无敌 | `Health.IsInvincible` + `SetInvincible()` | **不需要** | 共用。Player 冲刺时使用；Enemy 暂未使用。 |
| Status/Buff/Debuff | **当前无** | **未来可选** | WeaknessMark/AerialMark 等 unimplemented 效果需要此系统。 |

---

## 独有能力

| 能力 | Player | MeleeEnemy | RangedEnemy |
|---|---|---|---|
| 输入处理 | PlayerController2D | — | — |
| 移动/跳跃/冲刺 | PlayerController2D | — | — |
| 弹夹管理 | MagazineSystem | — | — |
| 背包 | InventorySystem | — | — |
| 卡牌效果执行 | CardEffectExecutor | — | — |
| 连击评分 | ComboRatingSystem | — | — |
| Good/Evil | PlayerAlignment | — | — |
| 背包UI | MagazineEditUI | — | — |
| 战斗HUD | CombatHUD | — | — |
| 巡逻AI | — | MeleeEnemyController | — |
| 追击/攻击AI | — | MeleeEnemyController | — |
| 远程射击AI | — | — | RangedEnemyController |
| 水平飞行巡逻 | — | — | RangedEnemyController |
| 发射子弹 | — | — | RangedEnemyController→EnemyProjectile |
| 死亡奖励 | — | Health.OnDeath（RewardManager 监听） | 同 |

---

## 是否需要共同父类？

**结论：不需要。**

**理由**：
1. Player 和 Enemy 唯一共享的逻辑是 `Health`（HP/Shield/Damage/Heal/Death），它已经是独立组件
2. Player 有 12 个组件，Enemy 有 7 个组件；共享部分仅 1 个（Health）
3. 如果强行让 Player 和 Enemy 继承同一个 MonoBehaviour 父类：
   - 会破坏现有 Prefab 序列化数据
   - Prefab 中的组件引用会丢失
   - 无法在 Inspector 中看到父类字段
   - Unity 组件模型鼓励组合优于继承

**推荐架构**：
```
健康/生存：Health 组件（Player + Enemies 共用）
阵营/身份：ActorIdentity 组件（未来可选，当前不需要）
玩家控制：PlayerController2D 组件
近战AI：MeleeEnemyController 组件
远程AI：RangedEnemyController 组件
UI：CombatHUD (Player) / EnemyHealthBarUI (Enemy)
```

---

## 是否有硬编码绕过 Health？

| 场景 | 代码路径 | 是否正确 |
|---|---|---|
| Player 受敌人伤害 | `MeleeEnemyController.TryDamagePlayer()` → `Health.TakeDamage()` | ✅ |
| Player 被远程弹命中 | `EnemyProjectile.HandleHit()` → `Health.TakeDamage()` | ✅ |
| Player 被旧 Enemy 伤害 | `EnemyController.TryDamagePlayer()` → `Health.TakeDamage()` (legacy) | ✅ (正确调用) |
| Enemy 被玩家子弹命中 | `Projectile.HandleHit()` → `CardEffectExecutor.ApplyEffectToTarget()` → `Health.TakeDamage(damage)` | ✅ |
| Player 治疗/格挡自己 | `CardEffectExecutor.ExecuteRight()` → `ApplyEffectToTarget()` → `Health.Heal/GainBlock` | ✅ |
| Player 死亡 | `Health.Die()` → `Destroy(gameObject)` | ✅ |
| Enemy 死亡 | `Health.Die()` → `Destroy(gameObject)` → `OnDeath.Invoke()` | ✅ |

**未发现硬编码绕过 Health 的伤害/治疗**。
