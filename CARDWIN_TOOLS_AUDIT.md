# CARDWIN_TOOLS_AUDIT.md — Tools 菜单审计

> 生成时间：2026-06-01 | Stage 11A

---

## Tools > Cardwin 菜单总表

| # | 菜单路径 | 脚本 | 方法 | 功能 | 当前是否需要 | 风险 | 建议 |
|---|---|---|---|---|---|---|---|
| 1 | `Tools/Cardwin/Rebuild Clean Demo Scene` | `CardwinSceneBuilder.cs` | `RebuildCleanDemoScene()` | 弹窗提示 "SceneBuilder is disabled" | **不需要** | 低（已 stubbed，不会误改场景） | **废弃/移除** — 下阶段可隐藏或删除 |
| 2 | `Tools/Cardwin/Create Basic Card Assets` | `CardAssetCreator.cs` | `CreateBasicCards()` | 创建/更新 Strike/Guard/Heal/Focus asset | **谨慎保留** | 中（会创建旧命名资产，与 C001~C012 重复） | **可用但危险** — 建议在菜单中增加提示或隐藏 |
| 3 | `Tools/Cardwin/Rebuild Card Database` | `CardDatabaseEditorUtility.cs` | `RebuildCardDatabase()` | 扫描 Cards 目录→重建 CardDatabase.asset | **需要** | 低（Play Mode 保护） | **保留** |
| 4 | `Tools/Cardwin/Import Cards From CSV` | `CardCsvImporter.cs` | `Import()` | 从 bullets.csv 批量导入→创建 CardData→更新 Database | **需要** | 低（只创建/更新，不删除） | **保留** |
| 5 | `Tools/Cardwin/Card Library` | `CardLibraryWindow.cs` | `ShowWindow()` | 卡牌浏览/搜索/筛选/禁用/删除管理窗口 | **需要** | 低 | **保留** — 主卡牌管理入口 |
| 6 | `Tools/Cardwin/Validate Card Configs` | `CardConfigValidator.cs` | `Validate()` | 扫描 CardData+Database → 输出检查报告 | **需要** | 低 | **保留** — QA 工具 |

---

## 详细分析

### 1. Rebuild Clean Demo Scene — 建议废弃

**当前状态**：点击只弹窗提示 "SceneBuilder is disabled. Use existing Demo_Combat.unity. Scene rebuilding is locked."

**风险**：无。该方法已被完全 stub 化，不包含任何场景生成逻辑。

**建议**：
- 下阶段移除 `[MenuItem]` 属性，从菜单隐藏
- 或保留为 `[MenuItem("Tools/Cardwin/Rebuild Clean Demo Scene (Deprecated)", priority = 100)]`
- 最终可删除整个 `CardwinSceneBuilder.cs`

---

### 2. Create Basic Card Assets — 谨慎保留

**当前状态**：生成 4 张旧命名卡牌（Strike.asset / Guard.asset / Heal.asset / Focus.asset）。

**风险**：
- 会生成无 `C0xx` 前缀的旧资产，与正式 12 张卡重复
- 已存在 16 张卡（4旧+12新），再创建可能引入更多重复
- 旧资产不包含 CSV 导入的附加字段（goodCost/evilCost/finalValue 等）
- 点击即覆盖现有旧资产，不会警告

**建议**：
- 保留脚本本身（有 `CreateOrUpdateCard` 工具方法可能被其他工具引用）
- 但隐藏菜单项，或加 Play Mode + 二次确认
- `[MenuItem("Tools/Cardwin/Create Basic Card Assets (Legacy)"])` 标记 legacy

---

### 3. Rebuild Card Database — 保留

**当前状态**：扫描 `Assets/Data/Cards/` 下所有 `CardData` asset，写入 `CardDatabase.allCards`。

**调用链**：
- CSV Importer 导入完成后自动调用
- 用户手动同步数据库时使用
- Play Mode 保护

**建议**：保留。如果未来改为自动同步，可以改为 `[MenuItem]` 只做手动强制重建。

---

### 4. Import Cards From CSV — 保留

**当前状态**：`EditorWindow`，从 `bullets.csv` 读取12张卡数据，创建/更新对应 `CardData` asset，自动调用 `RebuildCardDatabase()`。

**建议**：保留。核心数据管线入口。可扩展为支持任意 CSV 路径。

---

### 5. Card Library — 保留

**当前状态**：`EditorWindow`，搜索/筛选(按名称/类型/稀有度/目标/效果)/批量操作(禁用/启用/删除)/同步/创建/导入。

**建议**：保留。主卡牌管理入口。

---

### 6. Validate Card Configs — 保留

**当前状态**：`MenuItem` 静态方法，点击后执行完整检查→Console输出+生成 `CardValidationReport.txt`。

**已知行为**：
- 从 MCP Unity 可以正常触发
- 从 Unity Editor GUI 点击有时"感觉打不开"——因为方法是静态、不打开窗口、直接输出到 Console
- 不是 EditorWindow，无需 `ShowWindow()`
- Console 日志和报告文件是主要输出形式

**建议**：保留。可在 log 开头输出 `[CardValidator] Validation started — check Console and Assets/Data/CardImport/CardValidationReport.txt` 告知用户去哪里看结果。

---

## 建议清理计划

### 下阶段可移除/隐藏

| 菜单项 | 动作 |
|---|---|
| Rebuild Clean Demo Scene | 移除 `[MenuItem]` 或删除脚本 |
| Create Basic Card Assets | 添加 "Legacy" 标记或隐藏 |

### 必须保留

| 菜单项 | 原因 |
|---|---|
| Rebuild Card Database | 数据库同步 |
| Import Cards From CSV | 卡牌导入管线 |
| Card Library | 卡牌管理 |
| Validate Card Configs | QA 检查 |

---

## Validate Card Configs 打不开问题分析

**用户报告**：菜单存在但点击"打不开"。

**根因分析**：
1. `Validate()` 是 `public static void`，`[MenuItem]` 正确
2. 方法不需要 EditorWindow，不打开任何窗口
3. MCP 执行确认方法正常工作（输出 Console 日志 + 生成报告文件）
4. **用户感觉"打不开"的原因**：点击后没有任何窗口弹出，Console 日志可能被其他消息淹没
5. 方法内有 `AssetDatabase.FindAssets` 和 `AssetDatabase.LoadAssetAtPath`，如果资源路径错误会静默失败
6. 可能原因：Unity Editor 菜单注册延迟或 GUI 刷新问题

**修复建议**（如需改善 UX）：
1. 在方法开头增加 `EditorUtility.DisplayProgressBar` 显示进度（让用户看到正在执行）
2. 验证完成后 `EditorUtility.ClearProgressBar()`
3. 用 `EditorUtility.DisplayDialog` 弹出摘要信息（Errors / Warnings 数量）
4. 添加 try-catch 并在 catch 中输出 `[CardValidator][Exception] ...`
5. 但这增加了 UI 干扰——当前纯 Console 方案也可接受

**当前结论**：**功能正常**。不是代码 bug，是 UX 认知差异（期望窗口但实际是 Console 输出）。
