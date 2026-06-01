# CARD_SYSTEM_AUDIT.md — 卡牌系统唯一性审计

> 生成时间：2026-06-01 | Stage 11A

---

## 审计问题与结论

| # | 问题 | 当前结论 | 风险 | 建议 |
|---|---|---|---|---|
| 1 | CardData 是否是唯一卡牌数据源？ | **是**。所有卡牌数据通过 `CardData` ScriptableObject 定义。16 个 asset 文件 + CardDatabase 索引。 | 低 | 保持。旧 `CardAssetCreator` 可能生成无 CSV 字段的旧版卡，建议不主动使用。 |
| 2 | CardDatabase 是否是运行时获取卡牌的唯一入口？ | **是**。MagazineSystem、RewardManager、InventorySystem 均通过 CardDatabase 获取卡牌。另有 `Resources.FindObjectsOfTypeAll<CardDatabase>()` 作为 fallback。 | 低 | 保持。fallback 逻辑分散在多处（InventorySystem/RewardManager），可统一抽取。 |
| 3 | CardEffectExecutor 是否是卡牌效果执行的唯一入口？ | **是**。`ExecuteLeft()`/`ExecuteRight()`/`ApplyEffectToTarget()` 是所有卡牌效果的唯一执行路径。Projectile 命中后也调用 `ApplyEffectToTarget()`。 | 低 | 保持。注意仅实现 Damage/Block/Heal/Focus 四种效果。 |
| 4 | Projectile 命中后是否统一进入 CardEffectExecutor？ | **是**。`Projectile.HandleHit()` → `CardEffectExecutor.ApplyEffectToTarget()` | 低 | 保持。 |
| 5 | 左键/右键规则是否只有一套？ | **是**。`PlayerController2D.Update()` 中：左键→`cardExecutor.ExecuteLeft()`，右键→`cardExecutor.ExecuteRight()`。MagazineSystem 存在时不会 fallback。 | 低 | 保持。 |
| 6 | Combo 是否只监听成功用卡，不重复结算？ | **是**。`ComboRatingSystem.RegisterCardUse()` 仅在 `UseLeft()/UseRight()` 返回 true 时调用（PlayerController2D 中）。 | 低 | 保持。 |
| 7 | Good/Evil 装填规则是否只在 Apply 时校验？ | **是**。`MagazineEditUI.Apply()` 中校验 offensive cards 数量 == evil。AutoFill 优先补 evil 用攻击弹。 | 低 | 保持。 |
| 8 | Reward 是否只向 InventorySystem.AddCard 添加卡？ | **是**。`RewardManager.SelectCard()` → `_inventory.AddCard(card)`。 | 低 | 保持。 |
| 9 | 是否还存在旧 CardAssetCreator 生成旧卡？ | **是**。`CardAssetCreator.CreateBasicCards()` 仍在菜单中，会生成 Strike/Guard/Heal/Focus.asset（无 C0xx 前缀，无 CSV 扩展字段）。 | 中 | 建议隐藏菜单或标记 Legacy。 |
| 10 | 是否有旧的 Strike/Guard/Heal/Focus asset 与 C001~C012 重复？ | **是**。`Strike.asset` / `Guard.asset` / `Heal.asset` / `Focus.asset` 与 `C001_Strike.asset` / `C004_Guard.asset` / `C005_Heal.asset` / `C006_Focus.asset` 语义重复。CardDatabase 中均被引用（17 条）。 | 中 | 旧资产 cardId 不同（Strike_001 vs C001），不会导致 ID 冲突但占用数据库槽位。下阶段可考虑从 CardDatabase 移除。 |
| 11 | 是否有直接在其他脚本里硬编码伤害/治疗/护盾，绕过 CardEffectExecutor？ | **分析中**：`EnemyController.cs`（legacy）和 `MeleeEnemyController.TryDamagePlayer()` 调用 `Health.TakeDamage(int)`，是直接伤害不涉及卡牌效果——这是正确的敌人接触伤害。`EnemyProjectile.HandleHit()` 同样直接调用 `Health.TakeDamage()`，不经过 CardEffectExecutor——也是合理的敌人子弹伤害。两者都不需要经过卡牌系统。**未发现硬编码绕过 CardEffectExecutor 的卡牌效果**。 | 低 | 保持现状。 |

---

## 卡牌效果实现状态

| CardEffectType | 是否实现 | 脚本位置 | 备注 |
|---|---|---|---|
| `None` | Yes | CardEffectExecutor.ApplyEffectToTarget | 空操作 |
| `Damage` | **Yes** | CardEffectExecutor.ApplyEffectToTarget | 调用 Health.TakeDamage(focusAdjustedDamage) |
| `Block` | **Yes** | CardEffectExecutor.ApplyEffectToTarget | 调用 Health.GainBlock(card.block) |
| `Heal` | **Yes** | CardEffectExecutor.ApplyEffectToTarget | 调用 Health.Heal(card.heal) |
| `Focus` | **Yes** | CardEffectExecutor.ApplyEffectToTarget | 调用 context.AddFocus(card.focusGain) |
| `WeaknessMark` | **No** | — | enum 已定义，CardEffectExecutor 有 case 分支但空 |
| `QuickReload` | **No** | — | enum 已定义，case 分支空 |
| `ComboSpark` | **No** | — | enum 已定义，case 分支空 |
| `AerialMark` | **No** | — | enum 已定义，case 分支空 |

4 个未实现效果的对应卡牌（C009~C012）均有 `implemented=true`，使用它们会产生空操作，但不会报错或崩溃。

---

## 卡牌数据流

```
CSV (bullets.csv)
  → CardCsvImporter.Import()
    → 创建/更新 CardData .asset (C001~C012)
    → CardDatabaseEditorUtility.RebuildCardDatabase()
      → CardDatabase.asset (allCards list)

运行时：
  CardDatabase
    → MagazineSystem.ResolveSourcePool() → loadoutCards 随机装弹
    → RewardManager.OnEnemyKilled() → GetRandomCards(3)
    → InventorySystem.ResetToTestStock() → 每种正式卡 20 发

  PlayerController2D 输入
    → MagazineSystem.UseLeft/UseRight()
      → CardEffectExecutor.ExecuteLeft/ExecuteRight()
        → ExecuteLeft: 生成 Projectile (携带 card+effect+context)
          → Projectile 命中 → HandleHit()
            → CardEffectExecutor.ApplyEffectToTarget()
              → Health.TakeDamage / Health.Heal / etc.
        → ExecuteRight: 直接对 Player 调 CardEffectExecutor.ApplyEffectToTarget()

  PlayerController2D
    → ComboRatingSystem.RegisterCardUse(card, usedLeft, succeeded)
```

---

## 卡牌资产清单

### 正式卡 (C001~C012)

| CardID | 名称 | useTarget | IsOffensive | implemented |
|---|---|---|---|---|
| C001 | Strike | Enemy | Yes | Yes |
| C002 | Pierce | Enemy | Yes | Yes |
| C003 | Burst | Enemy | Yes | Yes |
| C004 | Guard | Self | No | Yes |
| C005 | Heal | Self | No | Yes |
| C006 | Focus | Self | No | Yes |
| C007 | Evil Shot | Enemy | Yes | Yes |
| C008 | Mercy Shield | Self | No | Yes |
| C009 | Combo Spark | Self | No | Yes |
| C010 | Quick Reload | Self | No | Yes |
| C011 | Weakness Mark | Enemy | No | Yes |
| C012 | Aerial Mark | Enemy | Yes | Yes |

### 旧资产 (无 C0xx 前缀)

| CardID | 名称 | 备注 |
|---|---|---|
| Strike_001 | Strike | 与 C001 重复 |
| Guard_001 | Guard | 与 C004 重复 |
| Heal_001 | Heal | 与 C005 重复 |
| Focus_001 | Focus | 与 C006 重复 |
