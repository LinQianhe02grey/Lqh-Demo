# TODO.md — 任务清单

---

## 已修复

- [x] 编译错误 CS0234 ~ SceneBuilder Disabled
- [x] Stage 3.2 ~ 3.5：场景修复和重建
- [x] Stage 4 ~ 4.3：Basic Combat Loop + Projectile 可见性
- [x] Stage 5 ~ 5B：CardData + CardEffectExecutor + 目标规则修正
- [x] Stage 6A：MagazineSystem Core
- [x] Stage 6B：Magazine Preview HUD — 3发预览 + HP/Shield/Focus
- [x] Stage 6C：Full Magazine Debug HUD — 8发完整弹夹显示 + 当前高亮 + 已用区分
- [x] Stage 6B.1：HUD Mount & Visibility Fix — Canvas自动挂载/UI锚点修复/运行时强制创建/绑定日志
- [x] Stage 6B.4：HUD Layout Cleanup — 旧占位禁用/CardwinHUDRoot统一/PreviewPanel+FullMagazinePanel HLG布局/文本防重叠
- [x] Stage 6D：Combat HUD Simplify + Random Reload — 战斗HUD只显示3发预览/随机装弹(Fisher-Yates)/完整8格弹夹保留给未来背包界面
- [x] Stage 6E：CardDatabase / Bullet Function Registry — 统一卡牌总表/按ID名称类型稀有度效果查询/随机抽取/Editor重建工具/MagazineSystem可选接入
- [x] Stage 7A：Inventory + Magazine Edit Panel — 背包/弹夹编辑/B键打开/左侧OwnedCards右侧Loadout 8格/点击加卡移除/Reload从新Loadout随机装弹
- [x] Stage 7A.1：Bag UI Visibility + Menu Input Lock — 修复BagPanel不可见/InputLock阻止战斗输入/B+Esc开关/EventSystem兜底/CanvasGroup遮罩/Cursor显示
- [x] Stage 7A.2：Inventory Test Stock + Apply Loadout To Combat — 测试库存Strike/Guard/Heal/Focus各20/聚合显示/点击扣除返回Inventory/Loadout立即BuildRandomMagazine实战生效
- [x] Stage 7A.3：Inventory Stock + Loadout Binding Real Fix — Open()单点初始化/EnsureTestStock+InitializeDefaultLoadoutIfEmpty/CardDatabase查找修复/Refresh日志OwnedTotal+OwnedEntries+Loadout
- [x] Stage 7A.4：Force Test Stock + Remove InitialCards Fallback — ResetToTestStock强制80张/_hasUserLoadoutInit禁止initialCards fallback/每次Open重置/FindCardDatabase统一查找
- [x] Stage 7A.5：Scene Component Pre-mount + Remove Runtime AddComponent — Player预挂InventorySystem+CardEffectExecutor/Canvas预挂MagazineEditUI/绑定所有引用/删除Awake动态AddComponent/改为GetComponent+Error日志
- [x] Stage 7A.6：Fix BagPanel Owned Cards UI Invisible — 修复Canvas子物体创建方式(Transform→RectTransform)/简化OwnedCardsPanel布局/GridLayoutGroup直接挂载/4条Created owned slot日志

---

## 下一阶段：Continue Stage 7A — Inventory / Magazine Editing stabilization

- [ ] 进一步稳定测试库存
- [ ] UI 布局微调

---

## 未来阶段

| Stage | 名称 |
|-------|------|
| 7 | Inventory System |
| 8 | Shop System |
| 9 | Polish & Analytics |
