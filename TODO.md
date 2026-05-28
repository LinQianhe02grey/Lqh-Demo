# TODO.md — 任务清单

---

## 当前阶段：Stage 1 — Basic Code Structure (进行中)

- [x] 建立目录结构（Core / Combat / Cards / Magazine / Inventory / Shop / UI / Analytics）
- [x] 创建基础脚本骨架（Core: 3 个类 / Combat: 5 个类 / Cards: 7 个类 / Magazine: 2 个类 / Inventory: 1 个类 / Shop: 2 个类 / UI: 5 个类 / Analytics: 1 个类）
- [x] 创建 ScriptableObject 卡牌数据基类（CardData）
- [x] 创建弹夹系统骨架（MagazineSystem）
- [x] 创建基础 UI 骨架（CombatHUD, MagazinePreviewUI, CardSlotUI, ShopUI, InventoryUI）
- [x] 创建基础敌人骨架（EnemyController）
- [x] Stage 1.5：Visual Graybox Scene（Editor 场景生成工具）

---

## 下一阶段：Stage 2 — Player Movement

- [ ] 实现 Input System 绑定（Move / Jump / Dash / Fire / UseSelfCard / Reload）
- [ ] 实现 PlayerController2D.Move() 水平移动 + 精灵翻转
- [ ] 实现 PlayerController2D.Jump() 跳跃 + 二段跳
- [ ] 实现 PlayerController2D.StartDash() 冲刺 + 无敌帧 + CD
- [ ] 在灰盒场景中测试移动/跳跃/冲刺

---

## 未来阶段（概要）

| Stage | 名称 | 目标 |
|-------|------|------|
| 2 | Card Effects | 实现 Damage / Block / Heal / Focus 四种效果 |
| 3 | Combat Loop | 完整战斗循环、敌人 AI |
| 4 | Inventory | 背包系统 |
| 5 | Shop | 商店系统 |
| 6 | Polish & Analytics | UI 打磨、数据统计 |
