# TODO.md — 任务清单

---

## 当前阶段：Stage 2 — Player Movement (完成)

- [x] 实现 Input System 绑定（Move / Jump / Dash — 旧版 Input Manager）
- [x] 实现 PlayerController2D.Move() 水平移动 + 精灵翻转
- [x] 实现 PlayerController2D.Jump() 跳跃 + 二段跳
- [x] 实现 PlayerController2D.StartDash() 冲刺 + 无敌帧 + CD
- [x] 在灰盒场景中测试移动/跳跃/冲刺

---

## 下一阶段：Stage 2.5 — Fix Auto-Binding + Stage 3 — Card Effects

- [ ] 修复场景生成时自动绑定 groundCheck 引用和 groundLayer
- [ ] 创建 ScriptableObject 卡牌数据资产（Strike0 / Guard0 / Heal0 / Focus0）
- [ ] 实现 CardEffectExecutor.ExecuteOnEnemy / ExecuteOnSelf

---

## 未来阶段（概要）

| Stage | 名称 | 目标 |
|-------|------|------|
| 2 | Card Effects | 实现 Damage / Block / Heal / Focus 四种效果 |
| 3 | Combat Loop | 完整战斗循环、敌人 AI |
| 4 | Inventory | 背包系统 |
| 5 | Shop | 商店系统 |
| 6 | Polish & Analytics | UI 打磨、数据统计 |
