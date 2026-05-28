# AGENTS.md — Cardwin Unity Demo 开发约束

## 强制规则

1. **开始实施前**：必须先读取 `SYSTEM_INDEX.md` 了解当前系统状态。
2. **代码修改前**：必须先说明本次修改将影响哪些系统、文件、类和函数。
3. **代码修改后**：必须更新 `SYSTEM_INDEX.md`（新增/变更/删除项）。
4. **代码修改后**：必须更新 `DEVELOPMENT_LOG.md`（按日志模板追加记录）。
5. **逻辑不得堆放**：不允许把所有逻辑堆到 `PlayerController2D` 类中。
6. **卡牌数据**：所有卡牌数据必须使用 `ScriptableObject` 定义。
7. **系统拆分**：按以下子系统组织代码：
   - `Core` — 游戏入口、全局状态、事件总控
   - `Combat` — 伤害计算、格挡、治疗、命中等
   - `Cards` — ScriptableObject 卡牌定义、卡牌效果接口
   - `Magazine` — 弹夹、换弹、预览
   - `Inventory` — 背包存储
   - `Shop` — 商店、购买、出售、刷新
   - `UI` — HUD、预览条、血条、商店界面
   - `Analytics` — 战斗数据统计
8. **Demo 优先**：优先保证 Demo 可玩，不要过度架构。

## 文件命名规范

- 脚本文件与其主类同名。例如 `PlayerController2D.cs` 中只包含 `PlayerController2D` 类。
- ScriptableObject 数据文件命名：`CardData_<卡片名>.asset`。
- 命名空间建议：`Cardwin.<子系统名>`。

## 代码风格

- 使用 C# UTF-8 编码，BOM 可选。
- 缩进使用 4 空格。
- 类内成员的排列顺序：字段 → 属性 → Unity 生命周期方法 → 公开方法 → 私有方法。
