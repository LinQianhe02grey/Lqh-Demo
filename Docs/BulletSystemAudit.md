# Bullet System Audit

> 项目：Cardwin Unity Demo (Card_G) ／ Unity 2022.3.25f1 ／ 日期：2026-06-26
> 范围：**只做分析与文档，未改任何代码 / 场景 / Prefab / Tag / Layer**。
> 目的：为后续设计 **Lua 热更新子弹系统** 提供权威现状基线。

---

## 1. 总结结论

- 当前项目**没有统一的「子弹系统」**。实际存在 **3 套独立的飞行物实现** + **1 套 Boss 命中扫描（非飞行物）**，它们之间**唯一的共同点**是最终都调用 `Cardwin.Combat.Health.TakeDamage(int)` 或 `Cardwin.Combat.IDamageable.TakeHit(int, GameObject)` 作为伤害汇聚点。
- 三套飞行物：
  1. **玩家普通/卡牌子弹** `Projectile`（Rigidbody2D + Trigger Collider2D，`Instantiate`/`Destroy`）。
  2. **敌人远程子弹** `EnemyProjectile`（Dynamic Rigidbody2D + Trigger Collider2D + 手动 OverlapCircle）。
  3. **音游红色追踪弹** `RhythmHomingBullet`（**无 Collider / 无 Rigidbody**，纯距离判定 + 追踪）。
- **Boss 技能（三连光束 / 地面光柱 / 二连斩 / 突刺 / 飞天激光）不是子弹**，是 `LineRenderer` + `Physics2D.CircleCast / OverlapBox / Raycast` 的**即时命中（hitscan / area）**。仅有一个**未接入**的原型 `MirrorSaintessProjectile.cs`。
- **CursedEightModule / BlessedEightModule 不引入新子弹**，只是用 `MagazineSystem.ForceLoadEightAttackCards(card)` 强制把弹夹塞成 8 张同一 `CardData` 并开启无限循环——发射时复用第 1 套 `Projectile`。
- 子弹类型**硬编码在 C#**；不存在统一 `BulletConfig` / `DamageContext` / 对象池；命中目标主要靠**组件查询（Health / IDamageable / IProjectileEffectReceiver）** 与**字符串 Tag/Layer/类名**判断 → 适合做 Lua 热更前需要先抽出扩展点。
- **Card Library 工具可用**（见 §10 末）。

---

## 2. 当前子弹类型列表

> 注：「Boss 技能」「Cursed/Blessed」严格说不是独立子弹类，但按需求一并列出。

| 类型 | 类名 | Prefab / Sprite | 生成入口 | 命中逻辑 | 伤害/回血 | 走 Health.TakeDamage | 走 Boss DamageReceiver | 可追踪 | 可穿透 | 走弹夹循环 |
|---|---|---|---|---|---|---|---|---|---|---|
| 普通攻击子弹 | `Projectile` | `Projectile_Test.prefab`（SpriteRenderer+Rigidbody2D+CircleCollider2D trigger）；红/蓝 sprite 由效果选 | `CardEffectExecutor.ExecuteLeft` → `Instantiate` | `OnTriggerEnter2D`/`OnCollisionEnter2D` → 组件链 | Damage=card.damage×Focus | 是（普通敌人） | 是（IProjectileEffectReceiver / IDamageable） | 否（直线） | **否**（命中即 Destroy） | 是（来自当前 CardData） |
| 增益/Buff 子弹 | `Projectile`（同上） | 同上（蓝色 sprite） | 同上（leftClickEffect=Block/Heal/Focus） | 同上 | Block/Heal/Focus | 普通敌人：Block/Heal 经 `ApplyEffectToTarget`；Focus 仅对玩家 | 是（Boss EffectReceiver 解析 Block/Heal/Focus） | 否 | 否 | 是 |
| Cursed 强制 8 发攻击 | `Projectile`（复用） | 复用普通子弹 | `PlayerCursedEightModuleState.Activate` → `MagazineSystem.ForceLoadEightAttackCards(attackCard)` | 同普通子弹 | 同普通子弹 | 是 | 是 | 否 | 否 | **是（无限循环 InfiniteEightLoopEnabled）** |
| Blessed 强制 8 发增益 | `Projectile`（复用） | 复用普通子弹 | `PlayerBlessedEightModuleState.Activate` → `ForceLoadEightAttackCards(buffCard)` | 同增益子弹 | Block/Heal/Focus | 视效果 | 是 | 否 | 否 | **是（无限循环）** |
| 音游红色追踪弹 | `RhythmHomingBullet` | 运行时 `new GameObject` + 子物体 `Visual`（程序生成红圆 sprite，**无 Collider/RB**） | `RhythmGameController.OnHit(Red)` → `SpawnHomingBullet` | **距离判定** `hitDistance=0.45`（非物理触发） | 目标 maxHP×3% | 是（普通敌人 `Health.TakeDamage`） | 是（BossRoom 无普通敌人时经 `MirrorAngelBodyDamageReceiver`(IDamageable)） | **是（每 0.2s 重新锁定最近普通敌人）** | 否（命中即 Destroy） | 否（与弹夹无关） |
| 敌人远程子弹 | `EnemyProjectile` | `EnemyProjectile.prefab`（SpriteRenderer+Dynamic Rigidbody2D+CircleCollider2D trigger） | `RangedEnemyController.FireAtPlayer` → `Instantiate` | `OnTriggerEnter2D` + 手动 `OverlapCircleAll` | 固定 int 伤害 | 是（仅打 `Player` tag） | 否 | 否 | 否 | 否 |
| Boss 技能（非子弹） | `MirrorAngelTripleBeamSkill` / `GroundRaySkill` / `DoubleSlashSkill` / `DoubleSlashDashSkill` / `AirLaserSkill` | `LineRenderer`（光束/激光）/ 运行时 FX / 无飞行物 | Boss `MirrorAngelBossBrain` → `skill.TryCast()` | `Physics2D.CircleCast`(beam) / `OverlapBox`(ray/slash) / `Raycast`(air laser)，命中 `playerLayer` | 各技能固定 int | 是（玩家 `Health.TakeDamage`） | N/A | 否 | N/A | 否 |
| Boss 原型子弹（未接入） | `MirrorSaintessProjectile` | 原型 | 无（`startAttackLoop=false`，未接入战斗） | `OnTriggerEnter2D` → `SendMessageUpwards("TakeDamage")` | — | 否（SendMessage） | 否 | 否 | 否 | 否 |

---

## 3. 普通子弹生成链路

实际链路（玩家左键）：

```
玩家左键 Input.GetMouseButtonDown(0)
  └─ 守卫：!RhythmGameController.IsRhythmModeActive（音游模式锁射击）
  └─ 冷却：Time.time >= _nextAllowedFireTime  (fireCooldown = baseFireInterval / externalFireRateMultiplier)
→ PlayerController2D.Update()
  └─ magazineSystem != null:
       IsReloading → 拒绝
       !HasUsableCurrentCard() → 拒绝（弹夹空）
       否则 → magazineSystem.UseCurrentCardLeft()  + _nextAllowedFireTime = now + fireCooldown + ComboRating
  └─ (fallback) magazineSystem==null 且 testCard → cardExecutor.ExecuteLeft(testCard)
  └─ (fallback) 否则 → Shoot()  // 直接 Instantiate projectilePrefab，Init(dir, testProjectileDamage)
→ MagazineSystem.UseCurrentCardLeft()
  └─ card = GetCurrentCard()（= LoadedCards[CurrentIndex]，一个 CardData）
  └─ cardExecutor.ExecuteLeft(card, context)
  └─ OnCardConsumed?.Invoke(card,false)  → AdvanceIndex()
→ CardEffectExecutor.ExecuteLeft(card, context)
  └─ prefab = card.projectilePrefab ?? context.defaultProjectilePrefab
  └─ direction = context.GetShootDirectionToMouse()  (Camera.main 鼠标方向)
  └─ spawnPos = firePoint.position + dir*0.3
  └─ GameObject projObj = Instantiate(prefab, spawnPos, identity)；scale 0.8
  └─ proj.Init(direction, card, effect, context)   // 携带 CardData+EffectType+Context
→ Projectile.Update()  每帧 transform.position += dir*speed*dt；lifeTimer 倒数到 0 → Destroy
→ Projectile.OnTriggerEnter2D / OnCollisionEnter2D → HandleHit(other)
   命中优先级（自上而下，命中即 Destroy）：
     1. other.CompareTag("Player")           → 忽略（不打玩家）
     2. other 有 Projectile                  → 忽略（子弹不打子弹）
     3. name 含 bossdoor/spawnpoint/camerabounds → 忽略
     4. layer 名 == "Trigger"                → 忽略
     5. IProjectileEffectReceiver（Boss）     → ReceiveProjectileEffect → Destroy
     6. MirrorSaintessBossPart(IDamageable)   → TakeHit（旧部位，现 Boss 已无部位，代码保留）
     7. 通用 IDamageable                      → TakeHit → Destroy
     8. Health                                → 携卡：ApplyEffectToTarget；裸弹：TakeDamage(damage) → Destroy
     9. layer 名 == "Ground"                  → Destroy
→ Health.TakeDamage(int)：先扣 block，再扣 currentHealth，<=0 → Die()
```

必答要点：

| 问题 | 答案（真实代码） |
|---|---|
| 左/右键都能发射？ | 左键 `UseCurrentCardLeft → ExecuteLeft`（生成子弹打敌人）；右键 `UseCurrentCardRight → ExecuteRight`（**对自身**施效，不生成子弹）。 |
| 射击冷却在哪 | `PlayerController2D.Update`：`fireCooldown = baseFireInterval / externalFireRateMultiplier`，`_nextAllowedFireTime` 门限。右键无冷却。 |
| 子弹方向怎么算 | `PlayerCardContext.GetShootDirectionToMouse()` = `Camera.main.ScreenToWorldPoint(mouse) - firePoint`。fallback `Shoot()` 同理。 |
| 子弹速度在哪 | `Projectile.speed`（默认 4，prefab 字段；运行时不改）。 |
| 子弹伤害在哪 | 携卡：`Projectile.ResolveGenericDamage()` = `card.damage × Focus 倍率`（Block/Heal/Focus 对 IDamageable 为 0，但 Boss EffectReceiver 单独处理）；普通敌人携卡走 `CardEffectExecutor.ApplyEffectToTarget`；裸弹 `Projectile.damage`。 |
| 生命周期在哪 | `Projectile.lifetime`（默认 5s），`Update` 内 `_lifeTimer` 倒数。 |
| 销毁/回收在哪 | **无对象池**。命中 / 生命周期到 → `Destroy(gameObject)`。每发都 `Instantiate` + `Destroy`。 |

---

## 4. 卡牌 / 弹夹 / 子弹关系

| 问题 | 答案 |
|---|---|
| CardData 是否保存子弹类型？ | **否**。`CardData` 只有 `projectilePrefab`（可选；为空则用 `context.defaultProjectilePrefab`）。所有伤害卡共用同一个 `Projectile` prefab；区别只在 `damage/block/heal/focusGain` + `leftClickEffect/rightClickEffect`。 |
| 攻击/增益/治疗/Focus 如何区分？ | 靠 `CardEffectType`（`None/Damage/Block/Heal/Focus`）的 `leftClickEffect` / `rightClickEffect`；外加 `CardType`（Attack/Defense/Heal/Utility）与 `IsOffensive` 属性。 |
| `MagazineSystem.LoadedCards` 存什么？ | **存 `List<CardData>`，不是 Bullet**。`LoadoutCards` 是来源池，`LoadedCards` 是当前 8 发弹夹。 |
| CurrentIndex 怎么推进？ | 每次 `UseCurrentCardLeft/Right` → `AdvanceIndex()`，`CurrentIndex++`。 |
| 第 9 发怎么回第 1 发？ | `AdvanceIndex` 中 `CurrentIndex >= LoadedCards.Count` 时：若 `InfiniteEightLoopEnabled` → `CurrentIndex=0`（循环）；否则 `StartReload()`（1.2s 后 `BuildRandomMagazine` 重洗，`CurrentIndex=0`）。 |
| Cursed/Blessed 怎么强制改弹夹？ | `ForceLoadEightAttackCards(card)`：清空 → 填 8 张同一 `CardData` → `InfiniteEightLoopEnabled=true`。Cursed 用 `FindAttackCard()`（扫 DB/initialCards/LoadedCards 找 Attack/Damage 卡）；Blessed 用增益卡。**不消耗背包**。 |

**新增一个新子弹到底要改什么？**

| 需求 | 是否需要 | 说明 |
|---|---|---|
| 改数值/效果（如更高伤害、改成治疗） | **只需新增 CardData asset** | 0 行 C# 代码。配 `damage/effect`，由 Card Library `Sync Database` 加入 `CardDatabase`。 |
| 改外观（不同贴图/缩放） | CardData + **新 Prefab** | 新建带 `Projectile` 的 prefab，设 sprite，挂到 `CardData.projectilePrefab`。 |
| 改行为（穿透 / 多重 / 弹道 / 追踪 / 范围） | **必须改 `Projectile.cs`**（或派生新脚本+新 prefab+CardData） | 当前 `Projectile` 是直线、单体、命中即 Destroy，**没有 pierce/spread/homing 字段**。 |
| 让 Cursed/Blessed 能抽到 | 该卡是 Attack/Damage 即会被 `FindAttackCard` 选中；或直接指定卡。 | — |

结论：**多数“配置型”新子弹只要新增 CardData（无需写代码）；“行为型”新子弹必须改 `Projectile.cs`。** 这正是 Lua 热更想解决的痛点。

---

## 5. Tag / Layer / Collider 要求（项目真实值）

### 项目实际 Tag（`ProjectSettings/TagManager`）
`Untagged, Respawn, Finish, EditorOnly, MainCamera, Player, GameController`
→ **只有 `Player` 与战斗相关。没有 `Enemy` tag、没有 `Boss` tag、没有 `Projectile` tag。**

### 项目实际 Layer
`0 Default, 1 TransparentFX, 2 Ignore Raycast, 4 Water, 5 UI, 8 Ground, 9 Player, 10 Enemy, 11 Trigger`
→ 有 `Ground(8)/Player(9)/Enemy(10)/Trigger(11)`；**没有 `Projectile` layer、没有 `Boss` layer。**

### 命中判定到底靠什么？

| 判定方式 | 是否使用 | 细节 |
|---|---|---|
| 靠 **Tag** 判断目标 | 部分 | 只用 `CompareTag("Player")` 来**排除/识别玩家**（`Projectile`、`EnemyProjectile`、Rhythm 弹均如此）。`Enemy`/`Boss` tag **不存在**，相关 `CompareTag("Enemy")` 形同虚设。 |
| 靠 **LayerMask** 判断目标 | 部分 | `Projectile` 用 layer **名字字符串** `"Trigger"`(跳过)/`"Ground"`(销毁)；Boss 技能用 `playerLayer` LayerMask 命中玩家；`EnemyProjectile` 用 `Ground/Default` layer 判墙。 |
| 靠 **Health 组件** 判断目标 | **是（主力）** | 普通敌人/玩家被打都靠 `GetComponent<Health>()`/`GetComponentInParent<Health>()`。Rhythm 弹靠 `FindObjectsOfType<Health>()` 找最近敌人。 |
| 靠 **EnemyController** 判断目标 | 弱 | Rhythm 弹 `IsEnemy()` 用**类名包含 "Enemy" 且不含 "Boss"** 作兜底（因无 Enemy tag）。 |
| 靠 **Boss DamageReceiver** 判断 Boss | **是** | 玩家子弹经 `IProjectileEffectReceiver`(MirrorAngelBossEffectReceiver) / `IDamageable`(MirrorAngelBodyDamageReceiver)；Rhythm 弹经 `MirrorAngelBodyDamageReceiver`(IDamageable)。 |
| Collider2D 必须 isTrigger？ | **是（飞行物 prefab）** | `Projectile`/`EnemyProjectile` prefab 的 Collider 都 `isTrigger=true`（走 `OnTriggerEnter2D`）。`Projectile` 同时有 `OnCollisionEnter2D` 兜底。Rhythm 弹**无 Collider**（距离判定）。 |
| Rigidbody2D 必须存在？ | **看类型** | `Projectile`/`EnemyProjectile` `RequireComponent(Rigidbody2D)`（前者 Kinematic g=0，后者 Dynamic g=0）。Rhythm 弹**无 Rigidbody**（手动移动）。 |

### 对象 → 组件/Tag/Layer 表（项目真实）

| 对象类型 | 实际 Tag | 实际 Layer | 必须组件 | Collider 设置 | 说明 |
|---|---|---|---|---|---|
| Player | `Player` | `Player(9)` | `Health` + `PlayerController2D`（+ Magazine/Inventory/CardExecutor） | Collider2D（被敌弹打） | 子弹靠 tag `Player` 排除/识别 |
| 普通敌人 | **Untagged**（无 Enemy tag） | `Enemy(10)` | **`Health`** + `MeleeEnemyController` / `RangedEnemyController` | Collider2D（非必须 trigger） | 子弹靠 **Health 组件**命中；类名含 "Enemy" 作兜底 |
| Boss | **Untagged**（无 Boss tag） | Body 在 `Default`；root Capsule excludeLayers=Player | `MirrorSaintessBoss` + `MirrorAngelBodyDamageReceiver`(IDamageable) + `MirrorAngelBossEffectReceiver`(IProjectileEffectReceiver) | Body BoxCollider2D **isTrigger=true**；root Capsule 非 trigger（只与 MainGround 碰撞，由 CollisionFilter 保障） | BossRoom 特殊：靠 IDamageable/EffectReceiver，不靠 tag/layer |
| 普通子弹 | Untagged | **Default(0)**（无 Projectile layer） | `Projectile` + `Rigidbody2D`(Kinematic,g0) | CircleCollider2D **isTrigger=true** | 玩家普通/卡牌攻击 |
| 音游追踪弹 | Untagged | Default | `RhythmHomingBullet`（+ 子物体 Visual 的 SpriteRenderer） | **无 Collider2D / 无 Rigidbody2D** | 距离判定 `hitDistance=0.45` |
| 敌人子弹 | Untagged | Default | `EnemyProjectile` + `Rigidbody2D`(Dynamic,g0) | CircleCollider2D **isTrigger=true** | 远程敌人发射，只打 Player |
| 地面 | Untagged | `Ground(8)` | Collider2D | **非 Trigger** | `Projectile` 命中 "Ground" layer 即销毁 |

> 注：用户示例表里的 `Enemy` tag / `Boss` tag / `Projectile` layer / `Boss` layer **在本项目并不存在**，本表按真实工程修正。

---

## 6. 新增一种普通子弹步骤（项目真实流程）

**情况 A — 只是新数值/新效果（最常见，0 代码）**
1. `Tools/Cardwin/Card Library` → `Create New`（或在 `Assets/Data/Cards` 右键 Create → Cardwin/Card Data）。
2. 配置 `cardId / cardName / cardType / rarity`。
3. 配置 `damage / block / heal / focusGain` 与 `leftClickEffect`（Damage/Block/Heal/Focus）、`rightClickEffect`。
4. `projectilePrefab` 留空 → 复用 `defaultProjectilePrefab`（即 `Projectile_Test`）。
5. Card Library → `Sync Database`（写入 `CardDatabase.asset`）。
6. 进入背包/弹夹（`MagazineEditUI`）把卡放进 Loadout，或让 Cursed/Blessed 自动选中（Attack/Damage 卡）。
7. Play 测试：命中普通敌人（`Health` 扣血）；确认不打玩家（tag Player 排除）；BossRoom 中命中 Boss（经 EffectReceiver / IDamageable）。

**情况 B — 需要新外观**
1. 做子弹 Sprite。
2. 复制 `Projectile_Test.prefab` → 新 prefab：保留 `SpriteRenderer` + `Rigidbody2D`(Kinematic,g0) + `CircleCollider2D`(**isTrigger=true**) + `Projectile`；换 sprite。
3. 新 `CardData.projectilePrefab` 指向该 prefab。
4. 其余同情况 A（5~7）。
> Tag/Layer：子弹保持默认即可（系统不依赖子弹自身的 Enemy/Projectile tag/layer；只要不在 Player/Trigger 上）。

**情况 C — 需要新行为（穿透/多重/弹道/追踪）**
1. **必须改 `Projectile.cs`**（加 pierce/spread/homing 字段与逻辑），或新建一个 `Projectile` 派生/替代脚本 + 新 prefab。
2. 走情况 B 接 prefab，情况 A 接 CardData。
> 这是当前架构的主要扩展成本，也是 Lua 热更要消除的点（§10）。

---

## 7. 新增一种音游追踪弹步骤

红色 note 命中入口：`RhythmGameController.UpdateInput → Judge → OnHit(Red) → SpawnHomingBullet()`。

| 关注点 | 真实实现 | 改新弹要动的地方 |
|---|---|---|
| 1. 命中入口 | `RhythmGameController.OnHit(RhythmNoteType.Red, perfect)` | `RhythmGameController.cs` |
| 2. 如何生成 | `SpawnHomingBullet()`：`new GameObject("RhythmHomingBullet", typeof(RhythmHomingBullet))` 于玩家上方 0.6，`bullet.Init(target, fallbackDir, homingSpeed, homingLifeTime, homingDamagePercent)` | `RhythmGameController.cs`（生成）+ `RhythmHomingBullet.cs`（行为） |
| 3. 目标如何找 | `FindNearestNormalEnemy(player)` 种子 + 弹内 `Retarget()` 每 0.2s `FindNearestEnemyInCurrentScene()` | `RhythmHomingBullet.cs` |
| 4. 普通敌人 vs Boss 优先级 | **优先最近普通敌人**（`Health`，排除 Player/Boss）；无普通敌人且在 BossRoom 才锁 Boss（`MirrorAngelBodyDamageReceiver`→`MirrorSaintessBoss`，root 兜底） | `RhythmHomingBullet.TryAcquireBossTarget` |
| 5. 视觉缩放在哪 | 子物体 `Visual.localScale = baseVisualScale(0.35) × visualScaleMultiplier(5) = 1.75`；root 恒 scale 1（不影响命中） | `RhythmHomingBullet.EnsureVisual` |
| 6. 伤害比例在哪 | `damagePercentOfTargetMaxHp = 0.03`（控制器 `homingDamagePercent` 传入）→ `Mathf.CeilToInt(maxHp×3%)` | `RhythmGameController`(值) / `RhythmHomingBullet`(应用) |
| 7. 命中后如何销毁 | `DealDamage()` 后 `Destroy(gameObject)`；`_damaged` 防重复 | `RhythmHomingBullet` |
| 8. 没有目标如何处理 | 沿 `_lastDirection` 继续飞（coast），`lifeTime=4s` 超时 `Destroy` | `RhythmHomingBullet.Update` |

结论：**新增音游弹 = 改 `RhythmHomingBullet.cs` + `RhythmGameController.cs` 两个文件**（值全部硬编码在控制器 SerializeField + 弹脚本）。与普通子弹/CardData **完全无关**。

---

## 8. BossRoom 子弹特殊处理

- **Boss 不被 tag/layer 识别**：无 `Boss` tag；Body 在 `Default` layer，root Capsule `excludeLayers=Player`，由 `MirrorAngelBossCollisionFilter` 让 Boss 身体只与 `MainGround` 碰撞、忽略其它一切 Collider。
- **玩家子弹打 Boss**：`Projectile.HandleHit` 命中 Body 的 Trigger BoxCollider →
  - 先查 `IProjectileEffectReceiver`（`MirrorAngelBossEffectReceiver`，root）→ 解析 Damage/Block/Heal/Focus（护盾先吸收 → `owner.TakeHit` 扣总 HP / Phase2 / Death）；
  - 否则查 `IDamageable`（`MirrorAngelBodyDamageReceiver`，Body）→ `ApplyExternalDamage`（护盾感知）→ `owner.TakeHit`。
  - `allowDirectBodyDamage=true`，root Capsule 也能吃伤害作双保险。
- **音游追踪弹打 Boss**：仅当当前场景无普通敌人且场景名含 "Boss" 时，`TryAcquireBossTarget()` 用 `MirrorAngelBodyDamageReceiver`(IDamageable) → 3% `MaxTotalHp`。
- **Boss 反打玩家**：技能 hitscan/area（`playerLayer` LayerMask）→ 玩家 `Health.TakeDamage`，不产生飞行子弹。
- **结论**：BossRoom 子弹命中是**第三条独立通道**（IDamageable / IProjectileEffectReceiver），与普通敌人的 `Health` 直击通道并行，靠 `Projectile.HandleHit` 的优先级链统一分流。

---

## 9. 当前问题与风险

| # | 问题 | 现状 |
|---|---|---|
| 1 | 普通子弹 vs 音游追踪弹是否两套？ | **是两套**：`Projectile`（物理 Trigger）与 `RhythmHomingBullet`（无碰撞、距离判定、追踪），无共享基类/配置。 |
| 2 | Boss 技能是否第三套？ | **是**（hitscan/area，非飞行物）。还有未接入的原型 `MirrorSaintessProjectile`。敌人 `EnemyProjectile` 实为第四套。 |
| 3 | 子弹类型写死在 C#？ | **是**。行为（直线/单体/Destroy/追踪/范围）全硬编码，无数据驱动行为。 |
| 4 | 新增子弹要改多文件？ | 配置型 0 文件（只加 CardData）；行为型要改 `Projectile.cs`；音游弹要改 2 个 Rhythm 文件。**不统一。** |
| 5 | 命中靠字符串 Tag 易错？ | **是**：`CompareTag("Enemy")` 形同虚设（无 Enemy tag）；靠 layer **名字字符串** `"Trigger"`/`"Ground"`、类名包含 `"Enemy"/"Boss"` 兜底 → 脆弱、易拼写错误、改名即崩。 |
| 6 | 是否无统一 DamageContext？ | **是**。伤害以裸 `int` 传递（`TakeDamage(int)` / `TakeHit(int, GameObject)`），无来源/暴击/元素/效果上下文结构。卡牌效果靠 `Projectile` 携带 `CardData+CardEffectType+Context` 临时拼。 |
| 7 | 是否无统一 BulletConfig？ | **是**。速度/寿命/伤害分散在 prefab 字段、`CardData`、控制器 SerializeField。 |
| 8 | 是否无对象池？ | **是**。三套都 `Instantiate`/`Destroy`，且子弹/敌弹/音游弹常运行时 `new Texture2D` 生成 sprite（音游弹有静态缓存，普通弹每发可能建图）。 |
| 9 | 是否每次 Instantiate/Destroy？ | **是**（音游弹甚至 `new GameObject`）。高频战斗有 GC / 峰值风险。 |
| 10 | 是否适合 Lua 热更？ | **当前不适合直接热更**：行为写死、无扩展点、无对象池、无统一接口。需先抽出 BulletHost + BattleAPI（见 §10）。 |

---

## 10. Lua 热更新改造建议（仅建议，不实现）

目标：**让策划/程序用 Lua 新增子弹行为与配置，无需改 C# / 不重新打包**。建议新增以下扩展点（增量、不动现有 `Projectile`）：

1. **C# 保留 `BulletHost`（宿主组件）**：一个轻量 MonoBehaviour，持有 Rigidbody2D/Collider2D/SpriteRenderer 引用，把 Unity 生命周期（Spawn/Update/Trigger/Recycle）转发给 Lua 回调；不含任何具体子弹逻辑。
2. **C# 提供 `LuaBattleAPI`（安全接口层）**：暴露受控方法给 Lua —— `FindNearestEnemy()` / `FindNearestNormalEnemy()` / `Damage(target, amount, ctx)` / `Heal(target, amount)` / `ApplyEffect(target, effectType, value)` / `PlayEffect(id, pos)` / `Move(host, dir, speed)`。内部仍走现有 `Health.TakeDamage` / `IDamageable.TakeHit`。
3. **Lua 配置 `BulletConfig`**：数据驱动 speed/lifetime/damage/scale/sprite/behaviourTags（pierce/homing/spread…），替代散落的 prefab/CardData/控制器字段。
4. **Lua 实现行为回调**：`OnSpawn(host, cfg)` / `OnUpdate(host, dt)` / `OnHit(host, target, ctx)` / `OnRecycle(host)`。把“穿透/多重/追踪/范围”从 C# 硬编码迁到 Lua。
5. **资源热更**：子弹 Sprite/FX 通过 **Addressables / AssetBundle** 远程更新（当前是运行时程序生成贴图，应改为可热更资源）。
6. **统一 `DamageContext`（建议同时引入）**：把 `int` 伤害升级为 `{ amount, source, effectType, focusMult, isCrit, element }`，让 Lua/卡牌/Boss 共用一套伤害语义。
7. **统一对象池 `BulletPool`**：`BulletHost` 走 Spawn/Recycle，消除 `Instantiate`/`Destroy` 与运行时建图。
8. **目标判定去字符串化**：用稳定的 `enum Faction`（Player/Enemy/Boss/Neutral）或接口标记替代 `CompareTag("Enemy")` / 类名包含 "Boss" 的脆弱判断（先补 `Enemy`/`Boss` tag 或一个 `FactionTag` 组件）。

---

## 11. 后续最小改造路线（增量试点，先不动现有系统）

1. **不动** `Projectile` / `EnemyProjectile` / `RhythmHomingBullet` / Boss 技能（保证现有 Demo 可玩、可打包）。
2. **第 1 步**：补 `Enemy` / `Boss` tag（或新增 `FactionTag` 组件）+ 给普通敌人/Boss 打标，消除字符串/类名判定风险（纯加法，不改逻辑）。
3. **第 2 步**：新增 `DamageContext` 结构 + `Health/IDamageable` 增加 `TakeDamage(DamageContext)` 重载（旧 `int` 重载内部转发），不破坏现有调用。
4. **第 3 步**：新增 `BulletHost` + `BulletPool` + `LuaBattleAPI`（C# 侧），先**只接一种新子弹**做试点（如“Lua 穿透弹”），与现有子弹并存。
5. **第 4 步**：把子弹资源迁到 Addressables，验证热更链路。
6. **第 5 步**：稳定后，逐步把音游弹 / 普通弹的行为可选迁移到 `LuaBulletHost`（增量，不强制一次性替换）。

> 原则：**Projectile 暂时不动，先新增 `LuaBulletHost` 做增量试点**，确认热更链路稳定后再考虑统一。

---

## 附：Card Library 可用性检查

- 工具：`Assets/Editor/Cardwin/CardLibraryWindow.cs`（菜单 `Tools/Cardwin/Card Library`）。依赖 `CardCsvImporter.cs`（存在）。
- 编译：项目当前 **0 红色错误** → 工具可正常打开与使用。
- 数据：`Assets/Data/Cards/CardDatabase.asset` 存在；正式卡 12 张（`C001_Strike … C012_Aerial_Mark`）+ 旧版 4 张（Legacy：Focus/Guard/Heal/Strike）。
- 功能：列表/搜索/筛选（Type/Rarity/Target/Status）/详情/`Create New`/`Sync Database`/`Import CSV`/启用禁用/Ping/删除均在代码中实现且引用有效。
- 结论：**Card Library 仍可用。**
