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
- [x] Stage 7B：Loadout Edit Polish — editingLoadout+editingOwnedCounts编辑层/Apply写回/Cancel放弃/Clear清空/AutoFill随机补满/SetLoadoutCards(cards,rebuildImmediately)/SetOwnedCardsFromCounts/Loadout N/8计数/未保存*标记/Loadout空不fallback
- [x] Stage 7C：Large Bag Panel + Tabbed Framework — 5分页(Magazine/Inventory/Fusion/Equipment/Preview)/1180x680大面板/TabRow高亮/BottomButtonRow修复CAAC/ButtonRow仅Magazine可见/Inventory只读/Fusion+Equipment占位/Preview预览/SwitchTab+日志
- [x] Stage 7C.1：Fix Magazine Page Content Missing — DestroyImmediate替代Destroy/_contentRoot直接引用/SwitchTab仅_open时刷新/Open先初始化库存再EnsureUI
- [x] Stage 7C.2：Enlarge Bag Panel Layout — 1380x820大面板/1260x610内容区/540x500两侧面板/210+190 cellSize/按钮170x42 fontSize17
- [x] Stage 7B.1：Block Fire During Reload/Empty — HasUsableCurrentCard/LoadedCount/MagazineSystem存在时永不fallback testCard/Reloading+Empty禁止左键右键
- [x] Stage 7D：Inventory Persistence During Play — InitializeForRun只初始化一次/Open只读库存不重置/PlayerController2D.Awake统一初始化/库存跨背包会话保持
- [x] Stage 8.0.1：Safe Mode Compilation Fix — EnemyProjectile float→int 类型修复
- [x] Stage 8A.1：Enemy Placement, Prefab Management, Collision and AI Fix — MeleeEnemyController/RangedEnemyController/EnemyProjectile prefab化/LevelRoot:Enemies场景层级/防重合/编辑模式可见
- [x] Stage 8A.1b：Static Level Authoring Fix + Missing using / Argument Order — LevelRoot结构/敌人落点/远程子弹Prefab绑定/Health命名空间与Init参数顺序修复
- [x] Stage 8A.1c：Project Records Synchronization — 同步 AGENTS/SYSTEM_INDEX/DEVELOPMENT_LOG/TODO/UE5_REFERENCE_INDEX 文档口径；未修改场景、脚本、Prefab 或卡牌资产
- [x] Stage 8A.3：Interrupt Recovery + Player Spawn/Jump + Enemy Attack/Projectile Visibility Fix — 修复Player重力/出生点/GroundCheck误判；近战扣血；远程发射可见敌方子弹；敌方子弹可扣Player HP；玩家Projectile可伤害近战和远程敌人
- [x] Stage 8A.5：Simplify Level & Enemy Architecture — 正式敌人统一为静态场景实例；旧 Enemy_Test 禁用；LevelRoot 移除运行时 Bootstrapper 依赖；近战 Kinematic+Trigger+stopDistance=1 防重合；Projectile 支持 Trigger/Collision 双路径
- [x] Stage 8A.8：Convert Enemy Placeholders To Real Edit-Mode Entities — 敌人在编辑模式具备完整 SpriteRenderer/Collider/Rigidbody；移除 EnsureVisual 运行时造图逻辑；移除 RangedEnemy FireFallback 运行时 AddComponent；EnemyProjectile 改为仅校验 prefab 完整性不补缺组件
- [x] Stage 8A.9：Flying Ranged Enemy Hitbox & Detection Tuning — 修复玩家子弹穿透空中敌人；Projectile 支持 GetComponentInParent<Health>；Bullet prefab 开 Continuous detection+增大 Collider；RangedEnemy shootRange 10→16；添加 OnDrawGizmosSelected 显示范围
- [x] Stage 8A.10：Remove Invisible Platforms Under Flying Enemies — 禁用 Platform_Z4/5/6_High 三个不可视实体平台；消除子弹遮挡和空气墙；空中远程敌人依赖 Kinematic/g=0 悬浮
- [x] Stage 9A：Player Good / Evil Attribute + Loadout Composition Rule — PlayerAlignment 组件(Good=4 Evil=4); CardData.IsOffensive; MagazineEditUI 显示善恶+攻击弹计数+Apply 校验; AutoFill 按 Evil 补攻击性子弹
- [x] Stage 9B：Combo Rating System — ComboRatingSystem 连击评分; MagazineSystem UseLeft/Right 返回 bool; PlayerController2D 注册连击; CombatHUD 右上角显示 Combo/Rank/Time; D/C/B/A 评分; 错误输入清零
- [x] Stage 9C：Combat Reward Pick One — RewardManager 订阅敌人 OnDeath; CardDatabase 随机抽 3 张; OnGUI 三选一; InventorySystem.AddCard; Time.timeScale=0 暂停
- [x] Stage 9C.1：Reward Trigger Debug — CardDatabase fallback 修复; Health 死亡日志
- [x] Stage 9C.2：Fix Player Movement Jitter — Rigidbody2D interpolation None→Interpolate; Jump 改用 velocity 替代 AddForce
- [x] Stage 9C.3：Fix Enemy Chase Jitter — 近战敌人状态机 Patrol/Chase/Attack/Return + 迟滞区; Kinematic MovePosition 统一移动
- [x] Stage 10A：CSV Card Import Pipeline — CardType 扩展 Support/Debuff; CardEffectType 扩展; CardData 扩展 CSV 字段; CardCsvImporter Editor 菜单; bullets.csv 12张卡; CardDatabase 自动更新
- [x] Stage 10B：Unity Card Library Manager — CardLibraryWindow (搜索/筛选/禁用/移除/删除/同步); CardData 新增 enabled/implemented
- [x] Stage 10B.1：Card Library Target Type Refactor — CardUseTarget enum (Enemy/Self/Both); CardData.useTarget; Combo 基于 useTarget; Library Target 筛选
- [x] Stage 10B.2：Simplify Card Query + Fix UseTarget + Stock All Cards — 12张卡 useTarget 修正; Library 筛选简化 Self/Enemy; Inventory 每种卡 20 发
- [x] Stage 10B.3：Inventory Card List ScrollView — Owned Cards 区域 ScrollRect+ContentSizeFitter; 按钮显示 Self/Enemy + 善恶消耗
- [x] Stage 10C：Card Config Validator — Tools > Cardwin > Validate Card Configs / 扫描CardData+CardDatabase / 9项检查 / 输出CardValidationReport.txt

---

## 下一阶段：Level Polish / Enemy Tuning

- [ ] 打磨从 SpawnPoint_Player 向右推进的路线节奏
- [ ] 调整平台高度差，避免极限跳跃
- [ ] 微调近战敌人 stopDistance / attackRange / cooldown
- [ ] 微调远程敌人 shootRange / fireCooldown / projectileSpeed
- [ ] 检查相机边界和完整通关路径
- [ ] 补充胜利门 / 终点触发的试玩闭环

---

## 未来阶段

| Stage | 名称 |
|-------|------|
| 7 | Inventory System |
| 8A.4 | Level Polish / Enemy Tuning |
| 8B | Shop System |
| 9 | Polish & Analytics |
- [x] Stage 11A：Project Architecture Audit — 完整扫描46脚本/场景/资产，输出10份审计文档，标记Active/Legacy/Stub，确定清理优先级
- [x] Stage 11B：Safe Cleanup Pass — CardDatabase 17→12正式卡 / 删除Enemy_Test_OLD+3禁用高台 / Tools菜单Legacy化
- [x] Stage 11C：Post-Cleanup Regression Test — 11项回归PASS / 0 Console Error / CardDatabase=12 / Inventory=240 / Good/Evil正常 / Combo正常 / Enemy正常 / Reward正常 / Tools菜单正确 / 报告REGRESSION_TEST_REPORT.md已生成
- [x] Stage 11D：Archive Legacy Card Assets — Strike/Guard/Heal/Focus.asset 归档到 Legacy/ / CardLibrary 增加 Show Legacy 开关 / Validate 排除 Legacy 目录 / CardDatabase 仅 12 张正式卡 / 0 资产删除
- [x] Stage 12B.1：Fix Player Death — SetDead 禁用 Sprite/Collider/Rigidbody / Health.Die 直接调用 GameOverController.HandlePlayerDeath / Update+FixedUpdate _isDead guard
