# 镜狱圣女 Boss 设计摘要

## Boss 名称
镜狱圣女·塞拉菲娜

## 阶段
- Phase 1：圣女审判，红蓝双枪轮流攻击。
- Phase 2：镜狱降临，攻击频率提升，核心能量暴走。

## 可破坏部位
1. ChestCore：胸口核心，破坏后 Stun。
2. BlueGun：蓝枪，破坏后禁用蓝枪技能。
3. RedGun：红枪，破坏后禁用红枪技能。

## 作品集亮点
- 状态机：Idle / Cast / Hurt / Phase2 / Death。
- 部位破坏：部件 HP 独立，破坏后影响技能池。
- 工程落地：Prefab、Animator、Collider、脚本解耦。
- 策划表达：双阶段、输出窗口、风险收益。
