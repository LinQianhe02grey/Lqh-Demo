# Mirror Saintess Boss Pack（镜狱圣女 Boss 原型资产包）

这是一个可以直接复制到 Unity `Assets/` 下的 Boss 原型资产包。

它包含：

- 女性哥特 Boss 立绘；
- 三个可破坏部位：胸口核心、蓝枪、红枪；
- 每个部位的完整/破损贴图；
- 6 组序列帧动画：Idle、CastBlue、CastRed、Hurt、Phase2、Death；
- 独立 C# Boss 原型脚本；
- Unity Editor 一键生成 Prefab 工具。

## 使用方式

1. 解压整个文件夹到你的 Unity 项目：

```text
Assets/MirrorSaintessBossPack/
```

2. 等 Unity 自动导入资源。

3. 点击菜单：

```text
Tools / Mirror Saintess Boss / Build Prototype Prefab
```

4. 生成 Prefab：

```text
Assets/Prefabs/Boss/MirrorSaintessBoss_Prototype.prefab
```

5. 把这个 Prefab 拖进：

```text
Assets/Scenes/BossRoom.unity
```

建议放到 `BossSpawnPoint` 附近。

## 当前资产定位

这是“可运行原型版”，不是最终 Spine/Live2D 拆骨骼版。

它适合你现在阶段先验证：

- Boss 能出现；
- Boss 有 Idle / 施法 / 受击 / 转阶段 / 死亡动画；
- 玩家能攻击 Boss；
- 玩家能攻击可破坏部位；
- 破坏蓝枪 / 红枪 / 胸口核心后，Boss 行为能变化。

## 可破坏部位设计

| 部位 | 作用 | 被破坏后 |
|---|---|---|
| ChestCore | 核心弱点 | 触发短暂 Stun，二阶段弱化入口 |
| BlueGun | 蓝色增益枪 | 禁用蓝枪技能 |
| RedGun | 红色诅咒枪 | 禁用红枪技能 |

## 动画说明

当前动画是基于立绘生成的原型序列帧：

```text
Art/Animations/Frames/Idle
Art/Animations/Frames/CastBlue
Art/Animations/Frames/CastRed
Art/Animations/Frames/Hurt
Art/Animations/Frames/Phase2
Art/Animations/Frames/Death
```

Unity Editor 工具会自动用这些帧生成 `.anim` 和 AnimatorController。

## 接入你的项目伤害系统

当前脚本提供通用接口：

```csharp
TakeDamage(float damage)
```

可以被你现有 Projectile 调用。

建议接入方式：

- 子弹命中 Boss 根对象：调用 `MirrorSaintessBoss.TakeDamage(damage)`；
- 子弹命中可破坏部位：调用 `MirrorSaintessBossPart.TakeDamage(damage)`；
- 如果你的项目已有 `Health` 或 `IDamageable`，可以加一个小适配器，不要重写 Boss。

## 注意

- 当前 Prefab 的碰撞、位置、缩放是原型值；
- 导入后需要根据你的 BossRoom 调整 Sorting Layer、Layer、Collider 和 Scale；
- 不要直接把这个 Boss 放进 GlobalRuntimeRoot，它是场景敌人，应该属于 BossRoom。
