# TODO.md — 任务清单

---

## 当前阶段：Stage 3 — Basic Combat Loop (完成)

- [x] Shot: 鼠标方向发射子弹 (Projectile.Init + Shoot)
- [x] Enemy: 追逐玩家 + 接触伤害 + Health
- [x] Camera: 平滑跟随 + 边界限制 (CameraFollow2D)
- [x] 文档：锁定 Demo_Combat.unity 为主场景

---

## 当前待办：场景手动挂载

- [ ] 创建 Projectile Prefab（SpriteRenderer + Rigidbody2D(gravity=0) + CircleCollider2D(isTrigger) + Projectile）
- [ ] Player FirePoint 子物体创建 + PlayerController2D 绑定 firePoint / projectilePrefab
- [ ] 创建测试 Enemy（手动或在 Play 时自动生成）
- [ ] MainCamera 添加 CameraFollow2D

---

## 下一阶段：Stage 4 — Magazine + CardData

- [ ] 创建 4 张 ScriptableObject 卡牌数据资产
- [ ] 实现 MagazineSystem 弹药池/弹夹/换弹/预览
- [ ] PlayerController2D 接入 MagazineSystem（替换临时射击）
- [ ] MagazinePreviewUI 显示下 3 发预览

---

## 未来阶段（概要）

| Stage | 名称 | 目标 |
|-------|------|------|
| 2 | Card Effects | 实现 Damage / Block / Heal / Focus 四种效果 |
| 3 | Combat Loop | 完整战斗循环、敌人 AI |
| 4 | Inventory | 背包系统 |
| 5 | Shop | 商店系统 |
| 6 | Polish & Analytics | UI 打磨、数据统计 |
