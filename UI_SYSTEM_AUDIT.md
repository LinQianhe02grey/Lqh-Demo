# UI_SYSTEM_AUDIT.md — UI 系统审计

> 生成时间：2026-06-01 | Stage 11A

---

## UI 脚本清单

| 脚本 | 类型 | 当前状态 | 挂载位置 | 是否运行时创建 UI |
|---|---|---|---|---|
| `CombatHUD.cs` | Runtime MB | Active | Canvas | Yes (Awake) |
| `HUDRuntimeBootstrapper.cs` | Runtime MB | Active | Canvas (ExecuteBefore) | Yes (确保 CombatHUD) |
| `MagazinePreviewUI.cs` | Runtime MB | Active | Canvas/CardwinHUDRoot/PreviewPanel | No（CombatHUD 创建容器后绑定） |
| `MagazineFullBarUI.cs` | Runtime MB | Retained | 不挂载（战斗HUD不创建） | No |
| `MagazineEditUI.cs` | Runtime MB | Active | Canvas | Yes (Start, EnsureUI) |
| `CardSlotUI.cs` | Runtime MB | Active | PreviewPanel/InventoryPage/MagazinePage | No（被其他 UI 创建） |
| `EnemyHealthBarUI.cs` | Runtime MB | Active | Each Enemy prefab | No (OnGUI) |
| `InventoryUI.cs` | Runtime MB | Stub | 不挂载 | — |
| `ShopUI.cs` | Runtime MB | Stub | 不挂载 | — |
| `CardLibraryWindow.cs` | Editor Tool | Active | Editor Window | N/A (Editor Window) |

---

## UI 层级结构（运行时）

```
Canvas (CombatHUD + MagazineEditUI)
├── EventSystem (MagazineEditUI 创建)
├── CardwinHUDRoot (CombatHUD.Awake 创建, anchor full-stretch)
│   ├── TopLeftStats (anchor top-left)
│   │   ├── HP_Text_Runtime
│   │   ├── Shield_Text_Runtime
│   │   └── Focus_Text_Runtime
│   ├── PreviewPanel (anchor bottom-center, y=35) [+MagazinePreviewUI]
│   │   └── PreviewSlot_0/1/2 (150x60 each)
│   ├── Combo_Text (anchor top-right)  // Combo rank
│   └── ReloadText (anchor center, y=120)
└── BagPanel (MagazineEditUI.Start 创建, 1380x820, default inactive)
    ├── Background
    ├── TitleText
    ├── TabRow (Magazine/Inventory/Fusion/Equipment/Preview)
    ├── ContentRoot (1260x610)
    │   ├── MagazinePage
    │   │   ├── OwnedCardsPanel (540x500)
    │   │   └── LoadoutPanel (540x500)
    │   ├── InventoryPage (只读聚合)
    │   ├── FusionPage (占位)
    │   ├── EquipmentPage (占位)
    │   └── PreviewPage (只读预览)
    ├── BottomButtonRow (Apply/Cancel/Clear/AutoFill, 仅在 Magazine 页)
    └── HintText
```

---

## UI 审计问题

| # | 问题 | 结论 |
|---|---|---|
| 1 | 是否有重复 UI 组件？ | **无**。CombatHUD 和 MagazineEditUI 职责分离清晰。CardSlotUI 被多个 UI 复用，这是设计意图。 |
| 2 | 8 格弹夹是否只在背包显示？ | **是**。`MagazineFullBarUI` 保留但不被战斗 HUD 创建。8 格弹夹显示在 MagazineEditUI.MagazinePage 的 LoadoutPanel（2x4 Grid）。 |
| 3 | 战斗 HUD 是否只显示 3 发预览？ | **是**。CombatHUD 只创建 PreviewPanel（3 个 PreviewSlot），不创建 FullMagazinePanel。 |
| 4 | RewardPanel 是否独立？ | **是**。`RewardManager.OnGUI()` 使用 Unity 立即模式 GUI（`GUI.Box/GUI.Button`），不依赖任何 Canvas UI 系统。 |
| 5 | Card Library 是否 Editor-only？ | **是**。`CardLibraryWindow` 是 `EditorWindow`，只在 Editor 模式可用。 |
| 6 | 旧的 Placeholder Text 是否还在？ | **是但已禁用**。CombatHUD.Awake 中 `DisableLegacyPlaceholders()` 禁用 `HP_Text/MagazinePreview_Placeholder/State_Text`。这些对象仍在场景层级中但 inactive。 |
| 7 | UI 运行时创建是否过多？ | CombatHUD 和 MagazineEditUI 都在 Awake/Start 中大量创建 UI 元素。这是当前设计选择——但增加了启动时间和运行时 GC。建议未来改为 Prefab 化。 |
| 8 | 是否有旧 UI 未使用？ | **有**。`InventoryUI.cs`、`ShopUI.cs` 均为 stub，功能已被 MagazineEditUI 替代或尚未开发。`MagazineFullBarUI.cs` 保留未使用。 |

---

## UI 依赖关系

| UI | 依赖系统 |
|---|---|
| CombatHUD | Health, PlayerCardContext, MagazineSystem, MagazinePreviewUI, ComboRatingSystem, PlayerController2D |
| MagazineEditUI | InventorySystem, MagazineSystem, CardDatabase, PlayerController2D, PlayerAlignment, CardSlotUI |
| MagazinePreviewUI | MagazineSystem (订阅事件), CardSlotUI |
| EnemyHealthBarUI | Health |

---

## UI 建议

| 建议 | 优先级 |
|---|---|
| 清理旧 Placeholder 对象（HP_Text/MagazinePreview_Placeholder/State_Text） | 低 |
| MagazineEditUI 13 个 `Create*()` 方法可以抽取到 UI 工厂类 | 中 |
| CombatHUD 中的 UI 创建逻辑可以考虑 Prefab 化 | 中 |
| `InventoryUI.cs` / `ShopUI.cs` 如果长期不使用，考虑删除 | 低 |
| `MagazineFullBarUI.cs` 如果确定未来不使用，考虑删除 | 低 |
