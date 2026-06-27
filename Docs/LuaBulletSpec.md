# Lua Bullet Spec — Card_G (Stage 57)

最小可用的 Lua 子弹系统：用 Lua 数据表增删改查子弹，自动进入玩家背包与敌人掉落，统一由通用宿主 `LuaBulletHost` 承载行为。

> 重要现状：项目**未接入 xLua / tolua**（已核实无 Lua DLL/包）。因此：
> - 注册表 `BulletRegistry.lua` 由自研「简化 Lua 表解析器」`SimpleLuaTableParser` **真实运行时解析**（CRUD / 背包 / 掉落全部数据驱动）。
> - 行为脚本（`PierceBullet.lua` / `HomingBullet.lua`）的 `OnSpawn/OnUpdate/OnHit/OnRecycle` 当前由按 `behavior` 字符串映射的 **C# 行为桥接**执行（`LuaBulletBehaviorRegistry`）。`.lua` 行为文件作为**规范格式 + 未来真热更替换目标**保留。
> - 全部运行时代码不使用 UnityEditor-only API，保证可打包（Windows）。

---

## 1. 文件位置

```
Assets/StreamingAssets/Lua/Bullets/BulletRegistry.lua   注册表（数据，被 C# 解析）
Assets/StreamingAssets/Lua/Bullets/PierceBullet.lua     穿透弹行为（格式参考 / 未来热更）
Assets/StreamingAssets/Lua/Bullets/HomingBullet.lua     追踪弹行为（格式参考 / 未来热更）
```

打包后位于 `<Game>_Data/StreamingAssets/Lua/Bullets/`，可直接替换实现热更（编辑器中需 Reload）。

---

## 2. LuaBulletRegistry 标准格式

`BulletRegistry.lua` 返回一个表：`return { version = N, bullets = { <id> = { ... }, ... } }`。
每个 `<id>`（如 `lua_pierce_001`）是一条子弹定义，结构如下：

```lua
lua_pierce_001 = {
    enabled = true,                 -- 软开关；false = 软删除
    display = {
        name = "Lua Pierce Bullet", -- 卡名（背包显示）
        desc = "...",               -- 描述
        icon = "Icon_LuaPierce",    -- 图标资源名（当前未加载，预留）
        sprite = "Bullet_LuaPierce",-- 子弹图资源名（当前用运行时圆点，预留）
        rarity = "Rare"             -- Common / Rare / Epic（Legendary→Epic）
    },
    card = {
        cardType = "Attack",        -- 运行时 CardData.cardType
        tags = { "Lua", "Attack" }, -- 标签数组
        leftClickEffect = "LuaBullet",  -- 标记用途；实际由 isLuaBullet 拦截
        rightClickEffect = "None"
    },
    bullet = {
        prefab = "LuaBulletHost",   -- 宿主（当前运行时自建，无需 prefab 资源）
        behavior = "Bullets.PierceBullet", -- 映射到 C# 行为 / 未来 Lua 模块
        speed = 12,                 -- 飞行速度
        lifeTime = 4,               -- 寿命（秒）
        damage = 8,                 -- 伤害（Flat=整数 / Percent=比例）
        damageMode = "Flat",        -- "Flat" 或 "PercentTargetMaxHp"
        pierceCount = 3,            -- 可穿透敌人数
        turnSpeed = 720,            -- 追踪转向速度（度/秒，仅 Homing）
        visualScale = 1.5,          -- 视觉缩放（不影响命中半径）
        hitRadius = 0.35            -- 触发碰撞半径
    },
    inventory = {
        enabled = true,             -- 是否纳入背包流程
        defaultCount = 8,           -- 初始数量
        addToBackpack = true        -- 是否加入背包
    },
    drop = {
        enabled = true,             -- 是否可掉落
        weight = 20,                -- 掉落权重（加权随机）
        enemies = { "MeleeEnemy", "RangedEnemy" }, -- 允许掉落的敌人类型（空=任意）
        minNight = 1                -- 预留：最低夜数门槛
    }
}
```

敌人类型字符串：`MeleeEnemy`（近战）/ `RangedEnemy`（远程）/ `BossRoomEnemy`（预留）。

---

## 3. 字段含义

见 §2 注释。映射到 C# 的 `LuaBulletDefinition`：`Id/Enabled/DisplayName/Description/Icon/Sprite/Rarity/CardType/Tags/LeftClickEffect/RightClickEffect/Prefab/Behavior/Speed/LifeTime/Damage/DamageMode/PierceCount/TurnSpeed/VisualScale/HitRadius/AddToBackpack/DefaultCount/AddToDrop/DropWeight/DropEnemies/MinNight`。

---

## 4. 如何新增子弹（Create）

1. 在 `BulletRegistry.lua` 的 `bullets` 表内新增一条 `<id> = { ... }`（建议 id 形如 `lua_xxx_001`）。
2. `behavior` 填写已注册的行为 id（当前可用：`Bullets.PierceBullet` / `Bullets.HomingBullet`；其它行为暂回退为直线弹）。
3. 在 C# 侧调用 `LuaBulletDatabase.ReloadLuaBullets()`（编辑器中），或重启游戏。

---

## 5. 如何修改子弹（Update）

修改任意字段（数值、掉落权重、背包数量、damageMode…），保存文件，调用 `LuaBulletDatabase.ReloadLuaBullets()`。新发射的子弹立即采用新数值（已验证：speed 10→25 重载即生效）。

---

## 6. 如何删除子弹（Delete）

**不要物理删除 id**，使用软删除：将 `enabled = false`。Reload 后：

- 不加入背包（`ListInventoryBullets` 过滤）。
- 不加入掉落（`ListDropBullets` / `CanDropFor` 过滤）。
- 不允许新发射（`CardEffectExecutor.SpawnLuaBullet` 检查 `Enabled`，禁用时不发射、也**不回退**普通 Projectile）。
- 旧存档/旧引用安全：`GetBullet(id)` 仍返回该定义（不崩），仅状态为 Disabled。

---

## 7. 如何查询子弹（Read）

`Cardwin.Lua.LuaBulletDatabase.Instance` 提供：

```csharp
LuaBulletDefinition GetBullet(string id);
IReadOnlyList<LuaBulletDefinition> ListAll();              // 全部（含 disabled）
IReadOnlyList<LuaBulletDefinition> ListEnabled();          // 仅 enabled
IReadOnlyList<LuaBulletDefinition> ListInventoryBullets(); // enabled && addToBackpack
IReadOnlyList<LuaBulletDefinition> ListDropBullets(string enemyType); // 可掉落且敌人匹配
void Reload();                  // 重新读取并重建缓存
static void ReloadLuaBullets(); // 静态便捷重载
```

---

## 8. 如何加入背包

`LuaBulletCardBridge.AddInventoryBulletsToBackpack(InventorySystem)`：为每个 `ListInventoryBullets()` 的定义创建**运行时** `CardData`（`isLuaBullet=true`、`luaBulletId=<id>`、`cardName=display.name`），按 `defaultCount` 调 `InventorySystem.AddRuntimeCard(card,count)`。幂等（已在背包则跳过）。已由 `PlayerController2D.InitializeInventoryAndLoadout()` 在 `InitializeForRun` 后自动调用。运行时 CardData 不写盘，旧 CardData asset 不受影响。

---

## 9. 如何加入敌人掉落

`LuaBulletDropBridge`：

```csharp
List<CardData> GetDropCandidates(string enemyType);
CardData RollDrop(string enemyType);                       // 按 weight 加权随机
bool TryDropToInventory(string enemyType, InventorySystem inv, float chance = 1f);
```

`LuaBulletRuntimeManager`（RuntimeInitializeOnLoadMethod 自举，无需改场景/敌人）在每次场景加载时把掉落 roll 订阅到 `MeleeEnemy/RangedEnemy` 的 `Health.OnDeath`，敌人死亡时把掉落结果加入**背包**（不直接进弹夹）。`dropChance` 可调（默认 1.0 便于演示）。

---

## 10. Lua 行为脚本格式（OnSpawn / OnUpdate / OnHit / OnRecycle）

```lua
local MyBullet = {}
function MyBullet.OnSpawn(host)            end          -- 创建时一次
function MyBullet.OnUpdate(host, dt)       end          -- 每帧
function MyBullet.OnHit(host, target)      end          -- 命中战斗目标
function MyBullet.OnRecycle(host)          end          -- 销毁前一次
return MyBullet
```

`host` 提供：`host.Definition`（定义）、`host.Direction`、`host.RemainingPierce`、`host.CurrentTarget`、`host.Recycle()`。
`BattleAPI`（= `LuaBattleAPI`）提供安全接口：

```
FindNearestEnemy(pos) / FindNearestBoss(pos) / IsDead(target)
Damage(target, amount) / DamagePercentOfMaxHp(target, percent) / HealPlayer(percent)
Move(host, dir, speed, dt) / MoveToward(host, targetPos, speed, dt)
PlayEffect(effectId, pos) / RecycleBullet(host)
```

伤害一律经 `Health.TakeDamage` 或 `IDamageable.TakeHit`；Boss 特殊受击仍走现有 Boss DamageReceiver；Lua/行为**不直接**访问 Health/Boss 内部。

---

## 11. 当前限制

- 无 Lua VM：行为由 C# 桥接执行；`.lua` 行为文件暂不被解释执行（仅注册表数据被真实解析）。
- 无对象池：当前 `Instantiate`/`Destroy`（每发新建宿主，运行时缓存圆点贴图避免重复分配纹理）。
- 图标/精灵（`display.icon/sprite`）暂未加载实际资源，宿主使用按稀有度着色的运行时圆点。
- `DamagePercentOfMaxHp` 对 `Health` 目标按其 `maxHealth` 计算；对仅实现 `IDamageable` 的目标（Boss）无法读取最大 HP，使用文档化的平直近似（`max(1, round(percent*100))`）。
- `minNight` 字段已解析但当前未参与掉落门槛逻辑（预留）。

---

## 12. 后续计划（对象池 / Addressables / 真热更）

1. 接入 xLua/tolua：新增 `LuaBackedBehavior : ILuaBulletBehavior`，加载 `.lua` 行为模块并转发回调；`LuaBulletBehaviorRegistry.Register(behaviorId, luaBacked)` 即可，无需改 `LuaBulletHost`/调用方。
2. 对象池：`LuaBulletHost` 增加 Spawn/Despawn 复用，替换 Instantiate/Destroy。
3. Addressables / 真热更下载：从远端拉取 `BulletRegistry.lua` + 行为 `.lua` + 图标/精灵，写入可写目录后 `Reload()`，实现运行期热更。
4. 图标/精灵资源解析：`display.icon/sprite` → Addressables/Resources 加载真实美术。
```
