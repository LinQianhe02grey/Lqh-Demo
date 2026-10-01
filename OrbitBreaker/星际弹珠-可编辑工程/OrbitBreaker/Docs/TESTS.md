# 验证记录

## TEST-20260926-001 — T00 环境与编译基线

- 环境：Windows；Unity 2022.3.62f2 Editor；2026-09-26 14:29–14:33 Asia/Shanghai；MCP 已连接。
- 操作步骤：
  1. 在工程根目录读取 ProjectSettings/ProjectVersion.txt、GraphicsSettings.asset、QualitySettings.asset、ProjectSettings.asset、EditorBuildSettings.asset 及 Packages/manifest.json；列出 Assets 文件。
  2. 运行 `git rev-parse --show-toplevel`、`git status --short`、`git branch --show-current` 和 `git log -1`。
  3. 经 Unity MCP 读取 instances，选择 OrbitBreaker@e58ab18932d5ddc5，读取 project/info、editor/state；manage_scene 的 get_active 与 get_hierarchy。
  4. read_console(action=get, types=[error,warning], count=100)，另以 types=[error] 再查。未清空 Console。
- 期望：记录真实工程路径、版本、管线、Git、MCP 与当前 Console 基线，并区分未验证项目。
- 实际：
  - 工程为 C:/testjbt/OrbitBreaker；版本 2022.3.62f2（7670c08855a9），MCP 一致。
  - 全局 m_CustomRenderPipeline={fileID: 0}，QualitySettings 无 pipeline 覆盖字段；Built-In。activeInputHandler=2（Both）。
  - Git 各命令报告 `fatal: not a git repository (or any of the parent directories): .git`；不是一个已提交的干净仓库。
  - MCP 连接可用；Editor 非 Play，is_compiling=false，is_domain_reload_pending=false，ready_for_tools=true。
  - OrbitBreakerSampleScene 已保存，isDirty=false，三个根对象；MCP_TestCube 在 (0,0,0)，带 BoxCollider/Rigidbody。
  - Console 错误 0；警告 1：`[WebSocket] Unexpected receive error: WebSocket is not initialised`，位置 MCPForUnity 的 Editor/Helpers/McpLog.cs:45。后续本轮 MCP 查询成功；没有据此宣称警告已修复。
  - Assets 无 .cs 玩法脚本；无 OrbitBreakerBattle/SceneBuilder；Build Settings 的 m_Scenes=[]。
- 结果：PASS（完成 T00 基线记录；不代表战斗玩法完成）。
- 证据：[MCP 状态原始返回](EVIDENCE/T00-mcp-baseline.json)、[本轮工程文件哈希基线](EVIDENCE/T00-files-before.json)；配置来源见步骤 1。
- 未验证项：没有主动重编译，没有 Play 模式试玩，没有 Windows Build/独立启动；本轮仅文档初始化，以上均不标记通过。

## TEST-20260926-002 — 文档与无工程改动核验

- 操作：确认 AGENTS、STATUS、TASKS、DECISIONS、CHANGELOG、TESTS 和交接记录存在；确认 T00–T12 全部入账且状态一致；比较 Assets、Packages、ProjectSettings 的 SHA256 与 T00-files-before.json。
- 期望：T00 DONE、T01 READY、其他 TODO；无领取中的任务；已有工程文件未改变。
- 实际：7 份必需入口/记录文件存在；T00–T12 共 13 行，T00 DONE、T01 READY，其余 TODO；Assets、Packages、ProjectSettings 文件路径与 SHA256 比较无差异。
- 结果：PASS。


## TEST-20260926-003 — T01 编译与构建（PASS）

- 环境：Windows / Unity 2022.3.62f2 / Built-In / MCP 已连接，2026-09-26 15:07–15:50 Asia/Shanghai。
- 先重新读取工程/台账/版本/包和 Git；仍无 Git，原场景为 OrbitBreakerSampleScene，错误 0，既有 MCP WebSocket 警告 1。
- 新增脚本后调用 refresh_unity(force, compile=request)，等待 editor/state ready、非编译/非域重载；read_console(types=error) 返回 0。最后一次编译产物 Assembly-CSharp-Editor.dll 时间晚于修正脚本。
- 读取 menu-items 确認 Build Demo Scene / Validate T01 已注册；执行 Build Demo Scene 两次，均生成/保存 OrbitBreakerBattle；active scene 为 OrbitBreakerBattle、isDirty=false、rootCount=1、buildIndex=0。
- 生成 7 个 GameObject，背景无碰撞；相机正交沿 +Z、包含 AudioListener；配置引用落盘、八参数默认值可读；Build Settings 仅一个启用的 OrbitBreakerBattle 条目。
- 初次最终核验发现监听器遗漏；修复 Ensure 的 Unity null 比较并增加测试，重新编译、构建和验收通过。第一轮 PASS 不作为最终版本依据。

## TEST-20260926-004 — T01 可重复验收（PASS）

- 复现：保存修改中的场景 → Tools/Orbit Breaker/Build Demo Scene → Tools/Orbit Breaker/Validate T01 → 等待自动退出 Play → 查看 Docs/EVIDENCE/T01-validation.json。
- 实际最终报告：PASS，observedAtUtc=2026-09-26T07:49:45.5308959Z，Unity 2022.3.62f2。
- Edit 检查：一份 OrbitBreakerGameManager、正交 XY 相机、Directional Light、唯一 AudioListener、配置引用和恰好八个字段；临时改变 gravityStrength 并保存、增加专属测试子对象后重复构建，所有对象 ID、配置/场景 GUID、调参值与测试子对象保持不变；测试 finally 恢复参数并移除测试对象；保存重开后引用/参数/监听器保持，场景不脏。
- Play 检查：Ready 拒绝结局/重开；StartGame 仅接受一次；Playing 拒绝调试重开；Victory/Defeat 为不可覆写的终态；只有两次有效转换发出两次事件；Victory 和 Defeat 后均真正 LoadScene，获取新的 OrbitBreakerGameManager 且恢复 Ready/Config。测试自动退出 Play。
- 证据：[自动报告](EVIDENCE/T01-validation.json)、[最终 Editor 快照](EVIDENCE/T01-editor-final.json)、可复跑脚本 Assets/Scripts/Editor/OrbitBreakerT01Validation.cs。
- 限制：使用状态方法调用，未模拟 Enter/R 实际键盘；未测试移动、物理或真实计分导致的胜败；未做 Windows Build。

## TEST-20260926-005 — T01 范围和收尾核验（PASS）

- 将 T00-files-before.json 的每个已有文件重新做 SHA256 比较：仅 EditorBuildSettings.asset 改变（加入 OrbitBreakerBattle），OrbitBreakerSampleScene、OrbitBreakerMCP_TestCube_Red.mat、Packages、其他 ProjectSettings 和既有资源原内容不变。
- 最终 OrbitBreakerBattle 已保存、rootCount=1，无测试探针残留；AudioListener 已落盘；配置 gravityStrength 已还原 12。
- 最终 Console：错误 0；1 条既有 MCP WebSocket is not initialised 警告再次出现（McpLog.cs:45），本轮后续 MCP 读取成功，未修改包或宣称警告消失。
- Editor 已退出 Play，编译完成；T01 DONE，T02 READY，其余未开始；所有交接和状态记录已落盘，无 Git 提交。


## TEST-20260926-006 — T02 编译和自动物理（PASS，保留失败历史）

- 环境：Windows / Unity 2022.3.62f2 / Built-In / Input Both / Unity MCP；2026-09-26 16:07–16:22 Asia/Shanghai；未升级包、无 Git。
- 新增运行脚本和测试后 refresh_unity(force, compile=request)，核对 Console 无编译错误并确认菜单注册，Build Demo Scene 保存 OrbitBreakerBattle；运行 Tools/Orbit Breaker/Validate T02。
- 首轮冻结轴断言 FAIL；细化记录后确认 z=0，但强扭矩使 rotation=(0.30,89.70,89.70)。修复 Motor 每步旋转稳定后重新编译、完整重跑，PASS。
- 最终报告 T02-physics.json：2026-09-26T08:19:06.4502057Z。自然下落从y=4到3.347、vy=-3.84；约0.3秒右/左速度x=±6，W竖速+2.4，S竖速-9.6；Z保持0，强外力后旋转和角速度0。
- 真实墙碰撞：左右停在x=±5.50；顶部碰撞后y=8.95；左右护栏上停在y=-9.50。所有这些用例仍 Playing。
- OrbitBreakerKillZone 忽略非玩家刚体；从出生点不输入自然漏球至y=-10.414，只触发一次 Defeat，球保持静止；重载后 Ready、出生点、输入清空；WinGame 状态同样冻结球。
- 证据：[最终物理报告](EVIDENCE/T02-physics.json)、[首次失败](EVIDENCE/T02-physics-first-attempt.json)、[旋转诊断失败](EVIDENCE/T02-physics-rotation-failure.json)。
- 自动模式通过 Motor.SetMoveInput 提交方向，初始条件由 Editor 测试安排；不是键盘模拟，也不是完整玩法测试。

## TEST-20260926-007 — T02 真实键盘链路（PASS）

- 运行 Check T02 Keyboard，使用 computer-use 技能的 node_repl/@oai/sky 定位唯一 Unity 2022.3.62f2 工程窗口，截图确认 Game 视图并点击聚焦。
- 依次 press_key(w/a/s/d)，验证报告实际记录方向(0,1)、(-1,0)、(0,-1)、(1,0)，mask=15；Ready 球保持静止。
- 实际 press_key(Return)：OrbitBreakerGameManager 进入 Playing，蓝球自然漏球至 Defeat/kinematic；实际 press_key(r)：重新加载为 Ready，验收自动退出 Play。
- 结果 PASS：2026-09-26T08:21:00.0109840Z；证据 [T02-keyboard.json](EVIDENCE/T02-keyboard.json)。
- 限制：WASD 的键位识别在 Ready 检查，运动加速度另由 006 检查；未声称长时间连续按键游玩或技能/战斗已完成。

## TEST-20260926-008 — T01 回归和范围核验（PASS）

- 再跑既有 Validate T01；2026-09-26T08:21:35.2428915Z PASS，重复生成保留对象身份、配置/GUID/调参、自定义子对象，保存重开保留组件引用与 AudioListener，两种终局重载仍可用。
- 回归结果单独保存：[T02-T01-regression.json](EVIDENCE/T02-T01-regression.json)；最后 Editor 快照：[T02-editor-final.json](EVIDENCE/T02-editor-final.json)。
- 比较 T02-files-before.json 中已有文件：仅 OrbitBreakerBattle.unity 与 OrbitBreakerSceneBuilder.cs 改变；Core、Config、OrbitBreakerSampleScene、已有材质、Packages、ProjectSettings 均保持原内容。新增文件见 CHANGELOG。
- 最终 OrbitBreakerBattle 已保存、rootCount=1，总15个GameObject；玩家 useGravity=0、isKinematic=1、constraints=120，OrbitBreakerKillZone isTrigger=1；无测试探针落盘。
- Console 最终错误0，既有MCP WebSocket警告1；退出Play、编译结束。无Windows Build、敌人/计分/技能/完整胜利试玩。


## TEST-20260926-009 — T02V 视角修正与回归（PASS）

- 环境：Unity 2022.3.62f2 Built-In / Windows / MCP已连接，2026-09-26 17:08–17:42 Asia/Shanghai。
- 编译：refresh_unity请求脚本编译；成功后Build Demo Scene保存Battle。旧T01相机断言改为Perspective且相机位于漏球近端、朝向沿台面远端。
- 视觉：直接捕获Main Camera，检查T02V-table.png（初版FOV38）后扩大取景至42，最终[T02V-table-1.png](EVIDENCE/T02V-table-1.png)显示近宽远窄台面、立体墙/台体、蓝球高光与投影。截图为实际相机输出，无合成。没有模拟最终敌人/翻板美术。
- API记录：manage_camera set_lens返回Target CinemachineCamera not found；此工程未安装Cinemachine，随后通过manage_components设置真实Camera.fieldOfView=42成功并保存；未增加任何包。
- T02物理回归PASS：2026-09-26T09:40:22Z，自然下落、四方向加速度、Z/旋转约束、墙与护栏、非玩家触发忽略、自然漏球仅一次Defeat、冻结、重开回出生点并清空输入、Victory冻结。[独立证据](EVIDENCE/T02V-physics-regression.json)。
- T01最终回归PASS：2026-09-26T09:40:47Z，重复生成/保存重开、配置和自定义子对象保留、GUID/对象身份稳定、监听器、状态事件及Victory/Defeat实际场景重载。[独立证据](EVIDENCE/T02V-T01-regression.json)。相机改FOV42后再次运行并保存通过。
- 范围核验：对T02V-files-before.json逐项比较，既有Assets/Packages/ProjectSettings内容仅Battle.unity、OrbitBreakerSceneBuilder.cs、OrbitBreakerT01Validation.cs改变。新增3个Table材质及meta；既有运行时脚本/参数/其他场景/项目设置未变。
- 最终场景Battle已保存isDirty=false、rootCount1、buildIndex0；Editor非Play、非编译。Console错误0、警告0；未修改MCP插件，不能据此声称既有连接警告已修复。[最终状态](EVIDENCE/T02V-editor-final.json)。
- 未验证：本轮未重复真实键盘按键（T02旧证据保留）、没有Windows Build、完整战斗或自由三轴滚动物理验证。


## TEST-20260926-010 — T03 自动冲刺和撞击（PASS，保留失败历史）

- 环境：Windows / Unity2022.3.62f2 Built-In / Unity MCP；17:47–18:01 Asia/Shanghai。修改后刷新编译，Console无编译错误，Build Demo Scene保存组件。
- 复现：保存场景 → Tools/Orbit Breaker/Validate T03 → 等待自动退出Play → 查看T03-physics.json。
- 最终PASS：2026-09-26T09:57:38.9494576Z。配置初速度18、冷却.75秒；默认向上、左右/下/归一化斜向(12.73,12.73)、无输入最近方向、请求方向捕获通过。
- 重复排队/活动期间/冷却期间请求被拒绝；冷却结束可重新冲刺；反向输入不覆盖窗口内速度，重力仍持续，窗口后普通加速度恢复；Z约束有效。
- 真实PhysX撞墙停止冲刺，Impact保留wasDashing资格，暖色闪.12秒后恢复；持续接触不重复闪；普通快速撞墙也反馈。冲刺青色、撞击暖色和属性块清除均由运行时读取断言。
- 禁用Dash取消排队；Defeat取消待处理请求并冻结，真实重载重置冷却与默认方向，重开可立即冲刺；Victory中断活动冲刺并恢复蓝色。
- 截图已查看：[冲刺](EVIDENCE/T03-dash.png)、[撞击](EVIDENCE/T03-impact.png)，均ScreenCapture实际Play画面，保留斜俯视3D台面。
- 历史：首次EditorApplication.update采样错过短窗口，保留T03-physics-first-attempt.json；Editor目录MonoBehaviour不能挂载，改成Assets/Tests中UNITY_EDITOR限定观察器，保留T03-observer-setup-attempt.json（中止记录）；空属性块恢复后覆盖标记残留，改成原块为空时SetPropertyBlock(null)，保留T03-feedback-restore-attempt.json。
- 稳定采样：测试探针LateUpdate晚于反馈更新；仅测试会话临时把maximumDeltaTime限为fixedDeltaTime，退出恢复，不修改TimeManager.asset。物理用例直接设置测试初始条件，不当作真实键盘试玩。[最终报告](EVIDENCE/T03-physics.json)。

## TEST-20260926-011 — T03 真实键盘链路（PASS）

- 运行Check T03 Keyboard，通过computer-use的node_repl/@oai/sky定位唯一实际Unity窗口，聚焦Game，真实press_key(Return)、press_key(space)。
- 保留正常PlayerInput，测试工具从不调用TryDash。为等待工具操作，在开始后且Space尚未触发时将球维持在出生点；收到实际DashStarted后停止干预，检查初速/向上位移和未重复触发。
- 最终PASS：2026-09-26T09:58:56.9056093Z，真实Enter启动，Space沿默认方向冲刺并产生位移，无新Space按下时不再次冲刺。[报告](EVIDENCE/T03-keyboard.json)。
- 限制：本轮真实键盘验证默认向上链路；其他方向/冷却/撞击由010独立检查。未声称长时间持续按住或完整战斗试玩。

## TEST-20260926-012 — T03 回归与收尾（PASS）

- T02自动物理回归PASS：2026-09-26T09:59:24.7687920Z。原有重力/WASD加速度/轴约束/墙和护栏/漏球/冻结/重开均通过。[报告](EVIDENCE/T03-T02-regression.json)。
- T01回归PASS：2026-09-26T10:00:09.2307050Z。重复生成不重复对象、配置/GUID/调参/自定义子对象保留，保存重开及两种终态真实重载通过。[报告](EVIDENCE/T03-T01-regression.json)。
- 范围：对T03-files-before.json每项比SHA256，原工程文件只改变Battle.unity、SceneBuilder、OrbitBreakerPlayerInput、OrbitBreakerPlayerMotor；新增Dash/Feedback、T03验证与Editor限定Probe及meta。原Core、Config、材质、OrbitBreakerSampleScene、Packages、ProjectSettings未改。
- 最终Battle已保存isDirty=false、buildIndex0/rootCount1，无T03_ValidationProbe落盘；相机orthographic=0、FOV42；Editor非Play/非编译；Console错误0/警告0。[最终状态](EVIDENCE/T03-editor-final.json)。
- 未执行Windows Build；没有敌人/得分/踏板/吸附/受击/HUD或完整胜利流程；Git未初始化，无commit。

## TEST-20260926-013 — T04 自动踏板与弹柱（PASS）

- 环境：Windows / Unity2022.3.62f2 Built-In / Unity MCP；2026-09-26 23:05–23:22 Asia/Shanghai。复现：保存场景 → Tools/Orbit Breaker/Validate T04 → 等待自动退出Play → 查看报告。
- 首次物理PASS 23:11:51；视觉修正后最终PASS 2026-09-26T15:16:55.5213643Z。[最终报告](EVIDENCE/T04-physics.json)。实际检查两踏板/两弹柱持久化、Ready拒绝、双区外/球心刚越边界/错侧拒绝、冷却/重复排队拒绝、留在区域不会自动施力、冷却后新请求可再次触发。
- 从vy=-24下落、质量2左侧救球，实际速度(10.01,15.33,0)且位置向上；右侧(-10.01,15.33,0)。保持Z约束；禁用移动输入仍能救球；活动向下冲刺被救球打断，后发冲刺不能覆盖待执行救球。
- 两弹柱由真实PhysX接触触发，一次碰撞一次冲量，向外速度分别(0,-12.48,0)/(0,11.52,0)；非玩家球不获得主动增强。不是直接调用Bumper私有回调或仅检查布尔状态。
- Defeat取消已排队冲量并冻结；真实重载回Ready，引用和两踏板冷却恢复，重开可立即救球；Victory拒绝请求。
- 用既有UNITY_EDITOR探针LateUpdate采样，仅测试会话临时限制maximumDeltaTime，退出恢复。初始位置/速度/质量通过测试夹具设置，不能当作真实键盘或Stunned测试。
- 视觉：初版踏板遮挡球中部，且截图后立即复位影响捕获；将PaddlePivot中心Z=.25、厚.5，Scene/Builder同步；截图后等待一帧再推进夹具。最终[左救球](EVIDENCE/T04-left-rescue.png)、[右救球](EVIDENCE/T04-right-rescue.png)、[台面](EVIDENCE/T04-board-final.png)为实际Unity画面；首张留作历史，无测试失败冒充PASS。

## TEST-20260926-014 — T04 真实键盘链路（PASS）

- Tools/Orbit Breaker/Check T04 Keyboard，通过computer-use技能的node_repl/@oai/sky定位实际Unity窗口，点击Game聚焦，真实press_key(Return)、k(左区错侧)、j、k(右区)。
- 测试不直接调用TryActivate；等待工具按键时将球维持在对应区域，收到Activated后停止干预并检查实际上升；左区K确实无激活，J和右区K能救球，松开后不重复触发。
- PASS 2026-09-26T15:14:13.1387936Z；[报告](EVIDENCE/T04-keyboard.json)。之后只调整踏板显示厚度/截图等待，输入代码未改，物理重跑通过。
- 限制：属于夹具辅助的真实键位链路验证，不是长时间完整战斗试玩；Stunned/Attached尚未实现。旧WASD/Space真实键盘证据保留，本轮未重复全部键位。

## TEST-20260926-015 — T04 回归和收尾（PASS）

- T03自动回归PASS 2026-09-26T15:15:03.5072770Z：方向/冷却/窗口/重力/撞墙/闪色恢复/终局重开。[报告](EVIDENCE/T04-T03-regression.json)。
- T02自动回归PASS 2026-09-26T15:20:00.9604266Z：重力/四向加速度/Z和旋转约束/墙护栏/中央漏球一次失败/冻结/重开。[报告](EVIDENCE/T04-T02-regression.json)。
- T01回归PASS 2026-09-26T15:20:34.2299935Z：重复构建不复制对象、对象身份/配置/GUID/调参/自定义子对象保留、保存重开、状态和两种终局实际重载。[报告](EVIDENCE/T04-T01-regression.json)。
- 对T04-files-before.json逐项SHA256核验，既有Assets/Packages/ProjectSettings只有Battle.unity、OrbitBreakerSceneBuilder.cs、OrbitBreakerPlayerMotor.cs改变；新建FlipperController/OrbitBreakerBumper/T04Validation和三材质及meta。[改动清单](EVIDENCE/T04-file-changes.json)。Core/Config/旧材质/OrbitBreakerSampleScene/Packages/ProjectSettings未改。
- 最终Battle已保存isDirty=false、buildIndex0/rootCount1；两踏板/两弹柱存在、相机Perspective/FOV42、无ValidationProbe落盘；退出Play、编译结束。Console错误0/警告0；开始时既有MCP连接警告1，未修改插件。[最终状态](EVIDENCE/T04-editor-final.json)。
- 未执行Windows Build；尚无敌人/得分目标/受击/吸附/HUD/完整战斗。Git未初始化，无commit。T04释放领取，T05 READY。


## TEST-20260927-001 — T05 目标计分与真实碰撞（PASS）

- 环境：Windows / Unity2022.3.62f2 Built-In/Input Both / MCP；工程C:/testjbt/OrbitBreaker。开始时MCP本地8080服务无响应，恢复后唯一实例与工程匹配。没有更换包或项目设置。
- 复现：保存场景 → Tools/Orbit Breaker/Validate T05 → 等待自动退出Play → 检查T05-physics.json。新脚本曾未被scripts刷新导入、菜单不可用，all刷新后注册；不是玩法测试失败。
- PASS：2026-09-27T01:45:41.5877947Z；[报告](EVIDENCE/T05-physics.json)。两50分普通目标、一100分高位目标、OrbitBreakerScoreManager/ScoreDisplay引用持久化；Ready分数0，读取既有targetScore500，拒绝Ready/无来源/非正分数/无效事件号。
- 实际PhysX接触：下落入射vy=-10.24不计分；低速上碰vy=3.52不计分；高速横碰/非玩家球不计分；目标上方静止停留不刷分。真实向上入射vy=9.76获50分，解算后vy=0，证实使用入射速度而不是解算后速度。
- 同次事件号/旧事件号在统一入口拒绝；持续向上顶住超过冷却、同次接触中改变速度不刷分；完全离开并过冷却后新撞击计分；快速离开重入被冷却拒绝，随后停留到冷却结束不会补分。
- 后续高位目标真实碰撞每次100分，累计500触发Victory并冻结球/后续计分；真实场景重载恢复Ready、零分、目标接触/事件号/冷却、踏板冷却、玩家出生点和冲刺状态。重开右目标可立即得分；再经中央漏球Defeat保留当局50分且禁止加分；再次真实重载清零。
- 成功目标闪绿.18秒并恢复空MaterialPropertyBlock，无共享材质修改；[命中图](EVIDENCE/T05-hit.png)、[胜利图](EVIDENCE/T05-victory.png)已查看，保留斜俯视3D厚台面，分数/结局文本可读。
- 限制：测试夹具安排初始位置/速度，真实PhysX计算接触和分数；不当作自由操作完整试玩。复用既有UNITY_EDITOR探针；会话内临时限制maximumDeltaTime并退出恢复，未改时间设置。

## TEST-20260927-002 — T05 真实键盘与胜负重开（PASS）

- 复现：Check T05 Keyboard，通过computer-use技能的node_repl/@oai/sky选择唯一Unity窗口，聚焦Game；真实Enter → Space → Victory后R → Enter → Defeat后R。
- 首次Space通过正常PlayerInput/PlayerDash冲刺并真实撞击高分目标，获得100；后续夹具安排四次向上真实撞击补齐500，未直接注入分数或调用WinGame；胜利后真实R完整重载。第二局真实Enter后安排普通目标碰撞得50，球从中央通道漏出进入Defeat，真实R再次重置。
- PASS 2026-09-27T01:47:52.3204240Z；[报告](EVIDENCE/T05-keyboard.json)、[键盘胜利图](EVIDENCE/T05-keyboard-victory.png)、[漏球图](EVIDENCE/T05-defeat.png)。胜负画面及分数复位已实际观察。
- 限制：等待首次按键时夹具固定目标下方球位；后续达标/漏球起点由夹具安排。验证真实键盘链路与真实碰撞/场景重载，不是3–5分钟无干预试玩。尚未实现敌人/受击/吸附/完整HUD或Windows构建。

## TEST-20260927-003 — T05 回归、范围与收尾（PASS）

- T04自动回归PASS 2026-09-27T01:48:26.8456278Z：左右区域/冷却/快速下落救球、冲刺与台面冲量优先级、真实弹柱碰撞、非玩家/终局/重开。[报告](EVIDENCE/T05-T04-regression.json)。
- T03自动回归PASS 2026-09-27T01:49:37.4419358Z：冲刺方向、冷却、重力/窗口、墙碰撞和颜色恢复、终局重开。[报告](EVIDENCE/T05-T03-regression.json)。
- T02自动回归PASS 2026-09-27T01:50:11.8379750Z：四向加速度、重力、轴约束、墙护栏、漏球失败与冻结重开。[报告](EVIDENCE/T05-T02-regression.json)。
- T01回归PASS 2026-09-27T01:50:44.9492639Z：重复构建不复制对象、身份/GUID/自定义子对象和调参保留、保存重开、状态与两种终局真实重载。[报告](EVIDENCE/T05-T01-regression.json)。
- SHA256与本轮T05-files-before.json对比：既有Assets/Packages/ProjectSettings仅Battle.unity、OrbitBreakerSceneBuilder.cs改变；新增ScoreManager、OrbitBreakerScoreTarget、OrbitBreakerScoreDisplay、T05Validation和两材质及meta。既有运行时脚本、配置资产、OrbitBreakerSampleScene、旧材质、Packages、ProjectSettings内容不变。[改动清单](EVIDENCE/T05-file-changes.json)。
- 最终Battle已保存isDirty=false/buildIndex0/rootCount1，无测试探针落盘，Perspective/FOV42不变；Editor退出Play、编译结束，Console0错误0警告。[最终状态](EVIDENCE/T05-editor-final.json)。
- 开始时MCP本地服务暂不可达，恢复后连接成功；编译重载曾出现WebSocket未初始化警告，最终未出现，不声称修复插件。无新增包或外部素材。
- 本轮T05 DONE，T06 READY；Git未初始化，无提交；未运行Windows Build，完整敌人战斗/受击/吸附/完整HUD仍待后续任务。

## TEST-20260927-004 — T06 随机刷新、追击和实际冲刺击杀（PASS）

- 环境：Windows / Unity2022.3.62f2 Built-In/Input Both / Unity MCP，工程C:/testjbt/OrbitBreaker。复现：保存Battle → Tools/Orbit Breaker/Validate T06 → 等待自动退出Play → 查看报告。
- PASS 2026-09-27T06:47:28.5971747Z；[报告](EVIDENCE/T06-physics.json)。红色Prefab、8个出生点、空Enemies容器及启用的Spawner引用持久化；Ready不生成；Playing等待既有2秒间隔才生成一个；实际移动使敌人与玩家距离缩短且Z保持0。
- 真实生成6个敌人后第7个被拒绝，跨完整自动间隔仍不超限/不追赶积压；销毁释放名额。出生点均在安全边缘，多个点被使用；8点全部放实体阻挡物时拒绝生成；将点移至漏球区仍拒绝。测试的立即TrySpawn请求负责准备上限/碰撞夹具，自动间隔另有独立断言。
- 普通高速实体接触不击杀/不计分，持续接触不能刷分。向上冲刺实际PhysX碰撞击杀一次+20，Motor已结束冲刺时仍能正确消费wasDashing快照；重复同来源死亡事件被ScoreManager拒绝；敌人实体/Collider移除且名额恢复。侧向冲刺同样击杀，敌人规则不套用目标仅向上计分限制。出台面清理不得分。
- 实际漏球Defeat冻结敌人/计分/刷怪，持续一个完整间隔仍静止；真实场景重载清空敌人/分数并恢复引用。4次高分目标真实碰撞400分+5次实际冲刺击杀100分达到Victory，存活敌人与玩家冻结、禁止继续刷怪；胜利重载后重新等待完整间隔才刷第一只。
- 已检查实际[敌人画面](EVIDENCE/T06-enemies.png)、[击杀画面](EVIDENCE/T06-keyboard-kill.png)、[混合计分胜利](EVIDENCE/T06-victory.png)，保持近端斜俯视3D透视台面，红Cube和立体阴影可见。
- 限制：Editor夹具设置碰撞初始位置/速度和采样，调用真实生成/冲刺请求，未直接注入击杀分数或WinGame；不是3–5分钟自由试玩。复用UNITY_EDITOR探针；只在测试会话临时限制maximumDeltaTime，退出恢复。普通接触导致Stunned留T07，未验证吸附/完整HUD或Windows构建。

## TEST-20260927-005 — T06 真实按键击杀和重开（PASS）

- 复现：Tools/Orbit Breaker/Check T06 Keyboard，通过computer-use的node_repl/@oai/sky选择当前Unity窗口、聚焦Game，依次真实Return → Space → 漏球后r。
- 正常PlayerInput/PlayerDash收到Space后，球实际碰撞红Cube，触发一次死亡和20分；目标消失且重复事件被拒绝。之后夹具安排中央漏球，Defeat显示保留20分；真实R重载到Ready、零分、空敌人容器，Spawner引用正常。
- PASS 2026-09-27T06:50:59.6941509Z；[报告](EVIDENCE/T06-keyboard.json)、[实际20分画面](EVIDENCE/T06-keyboard-kill.png)。按键过程中直接观察到20分失败画面及重开，不以API调用替代真实按键。
- 限制：等待Space时夹具维持球和敌人位置；首次冲刺后停止该固定，漏球起点由夹具安排。此为真实输入链路与物理接触/场景重载验证，不宣称自由完整战斗试玩。本轮未重复旧WASD/J/K键盘用例，旧记录保留。

## TEST-20260927-006 — T06 回归、范围和收尾（PASS）

- T05回归PASS 2026-09-27T06:51:29.7791474Z：[报告](EVIDENCE/T06-T05-regression.json)，真实向上计分/低速下落拒绝/重复防护/胜利漏球/重载。
- T04回归PASS 2026-09-27T06:51:58.0309944Z：[报告](EVIDENCE/T06-T04-regression.json)，两踏板区域/冷却/快速下落与质量补偿救球/冲刺优先级/弹柱/重开。
- T03回归PASS 2026-09-27T06:52:46.3804736Z：[报告](EVIDENCE/T06-T03-regression.json)，方向/冷却/重力窗口/墙碰撞/反馈恢复/终局重开。
- T02回归PASS 2026-09-27T06:53:31.3943488Z：[报告](EVIDENCE/T06-T02-regression.json)，四向加速度/重力/轴约束/墙护栏/漏球/冻结重开。
- T01回归PASS 2026-09-27T06:53:54.1903972Z：[报告](EVIDENCE/T06-T01-regression.json)，重复构建保持所有对象身份（含新刷怪节点）、配置/GUID/调参/自定义子节点保留、保存重开及两种终态真实重载。
- 隔离范围：T02–T05固定物理用例在测试Play中临时禁用Spawner，防止无关随机碰撞污染旧断言；T01不改。T06独立覆盖真实自动刷新、追击、实际碰撞和目标/敌人混合胜利。不是宣称所有旧夹具在随机战斗下原样运行。
- 对T06-files-before.json逐项SHA256比较，既有工程只改Battle、SceneBuilder及T02–T05验收隔离；新增EnemyController/Spawner、OrbitBreakerEnemy.prefab、OrbitBreakerEnemyRed.mat、T06Validation和meta。无删除；旧运行时脚本/原八参数资产/旧材质/OrbitBreakerSampleScene/Packages/ProjectSettings内容不变。[改动清单](EVIDENCE/T06-file-changes.json)。
- 14:54:30直接查询Editor：Battle保存isDirty=false/buildIndex0/rootCount1，非Play/非编译；Spawner启用、8点引用、持久化Prefab、无运行时敌人/测试探针。相机Perspective/FOV42、位置(0,-20,-16)；Console最终0错误0警告。[快照](EVIDENCE/T06-editor-final.json)。editor/state缓存曾报stale_status，因此当前判断使用直接Editor读取。
- 开始时既有MCP连接警告1，最终未出现，不声称修复插件。无新包/外部素材；本地生成几何/纯色Standard材质。Git未初始化，无提交；未运行Windows Build，Stunned/吸附/完整HUD/长期自由试玩留后续。

## TEST-20260927-007 — T07 受击、危险机关与加宽台面（PASS）

- 环境：Unity2022.3.62f2 / Built-In/Input Both / Windows Editor / Unity MCP。工程C:/testjbt/OrbitBreaker，起始Console错误0/既有WebSocket连接警告2，非Play、Battle已保存，Git未初始化。
- 复现：Tools/Orbit Breaker/Validate T07；自动进入/退出Play。PASS 2026-09-27T12:01:30.7041487Z，[报告](EVIDENCE/T07-physics.json)。使用原配置stunDuration=.65；没有增加配置字段或修改资产。
- 台面实体内宽由12改15（+25%），中央漏口仍4，球体比例/3D透视FOV42不变。两个危险机关实际BoxCollider持久化；不通过缩放整个根节点加宽。
- Ready拒绝受击；接受命中当帧进入Stunned/保护，取消排队冲刺，同帧重复命中和冲刺请求拒绝；下一物理步产生一次向下速度变化。失控球保持dynamic和重力，无移动输入加速度，红色MaterialPropertyBlock显示；.65秒结束后恢复移动，额外.35秒保护仍拒绝伤害，之后恢复原蓝色。
- 失控球实际撞加宽墙仍被阻挡且Z约束有效；真实弹柱碰撞依然弹回。左右踏板在失控时均实际向上救球，保持失控；左侧测试先排队救球再受击，右侧先受击再救球，两种顺序都不会被下坠覆盖。
- 真实普通敌人碰撞触发一次失控、敌人存活、0分，攻击冷却1.2秒；玩家保护结束而敌人冷却未结束时重新接触仍无受击；双方冷却结束后新接触可再次命中。持续实体接触跨过冷却仍不重复刷新失控。
- 实际冲刺撞敌人击杀+20且玩家不受击；实际冲刺撞黄黑机关则被打断并失控、不得分。另一机关在玩家保护期间不能叠加命中，保护结束后的新接触可命中。
- 受击后从中央真实漏球进入Defeat，优先清除失控/保护并冻结、拒绝后续受击。真实重载恢复Ready、分数0/冷却0/无失控；另直接调用WinGame验证终态覆盖排队命中（该项是状态边界测试，不冒充实际得分胜利，实际计分胜利由T06回归覆盖）。
- 已检查[失控救球及加宽台面截图](EVIDENCE/T07-stun-rescue.png)，黄黑危险机关、红色受击球、体积/阴影和完整台面可见；另保存T07-stunned.png、T07-hazard-hit.png。
- 夹具限制：设置测试起点/速度、调用状态请求和准备真实敌人；稳定持续接触用临时静止敌人和位置维护，不是自由战斗试玩。复用UNITY_EDITOR探针，maximumDeltaTime仅测试会话限制并退出恢复。T08吸附尚未实现；只提供CanControl状态门禁，未声称验证吸附中断。

## TEST-20260927-008 — T07 真实键盘失控救球与重开（PASS，保留首轮失败）

- 使用computer-use技能的node_repl/@oai/sky定位实际Unity窗口，聚焦Game。正常PlayerInput/FlipperController接收真实Return、Space、j、k、r。
- 正常敌人真实碰撞使球失控；Space被拒绝，J和K分别使失控球实际向上，且不解除失控。最后夹具安排真实中央漏球，R真实场景重载恢复Ready、清空失控/保护，并恢复原配置。
- PASS 2026-09-27T12:05:41.9582368Z，[报告](EVIDENCE/T07-keyboard.json)、[真实J救球图](EVIDENCE/T07-keyboard-rescue.png)。K后采样速度(-4.37,16.74,0)，仍有失控时间，验证真实输入不会被状态锁错误拦截。
- 为允许逐步工具操作，仅键盘测试克隆CombatConfig到运行时（DontSave），把失控窗口临时放大至120秒；未改磁盘资产。原.65秒时长与恢复顺序由007测试。等待按键时夹具固定球在对应踏板区，收到Activated后停止固定、检查真实速度；没有调用TryActivate替代真实J/K。
- 首轮临时30秒窗口不足以完成逐步按键，K断言FAIL（20:03:14）；Space/J已过。保留[T07-keyboard-first-attempt.json](EVIDENCE/T07-keyboard-first-attempt.json)。只延长测试克隆窗口并增加诊断后重测通过，生产受击/踏板实现未因该失败修改。
- 本轮未重复实际WASD输入，失控时移动禁止与恢复用007物理用例验证；不是3–5分钟无干预试玩，不等同Windows独立构建验证。

## TEST-20260927-009 — T07 回归、加宽幂等与收尾（PASS）

- T06自动回归PASS 2026-09-27T12:06:29.8734576Z：[报告](EVIDENCE/T07-T06-regression.json)。宽台面安全刷怪/追击/上限、普通接触不刷分、真实冲刺击杀、目标400+击杀100到Victory、终局冻结/重开。普通接触现在会失控，因此夹具等待恢复后再进行独立冲刺用例；没有禁用生产伤害。
- T05 PASS 2026-09-27T12:07:33.2670205Z：[报告](EVIDENCE/T07-T05-regression.json)，目标真实方向/速度/冷却/去重、实际计分胜利与漏球重开。
- T04 PASS 2026-09-27T12:08:08.0698425Z：[报告](EVIDENCE/T07-T04-regression.json)，区域/冷却/快速下落与质量补偿/冲刺救球/弹柱/终局重开。
- T03 PASS 2026-09-27T12:08:55.7980401Z：[报告](EVIDENCE/T07-T03-regression.json)，方向/冷却/重力窗口/新墙边界真实碰撞/反馈/重开。
- T02 PASS 2026-09-27T12:09:31.5265396Z：[报告](EVIDENCE/T07-T02-regression.json)，移动/重力/旋转Z约束/加宽墙体/原中央漏口/冻结重开。
- T01 PASS 2026-09-27T12:09:55.9757293Z：[报告](EVIDENCE/T07-T01-regression.json)，重复构建保留对象身份/配置/GUID/用户子节点、保存重开与胜负状态重载。最终宽15、加宽标记1份，证明没有重复扩宽或额外机关。
- T02–T05沿用仅测试Play期间暂停随机Spawner的隔离；T06/T07独立真实敌人/受击/计分整合；未宣称旧固定用例在随机战斗中运行。
- 已查看[T07-board-final.png](EVIDENCE/T07-board-final.png)：从本轮T06回归真实Game捕获，包含加宽台面、6红Cube、球体、两黄黑机关与完整3D阴影。不是概念图。
- SHA256对比本轮基线：既有文件改9个（OrbitBreakerBattle、Builder、T02/T03/T06Validation、OrbitBreakerEnemyController/Spawner、Motor/Feedback）；新文件8个含Hazard、T07Validation、2材质及meta，无删除。原Config资产/其他旧材质/OrbitBreakerEnemy Prefab/OrbitBreakerSampleScene/Core/Packages/ProjectSettings不变。[清单](EVIDENCE/T07-file-changes.json)。
- 最终20:10:21直接Editor查询：[快照](EVIDENCE/T07-editor-final.json)。OrbitBreakerBattle isDirty=false/buildIndex0/rootCount1，非Play/非编译；内宽15、Hazard2、加宽标记1、Spawner启用、无敌人/测试探针。stunDuration=.65/8字段，Time.maximumDeltaTime=.333333已恢复；相机Perspective/FOV42/位置(0,-20,-16)。Console错误0/警告0。
- 初始既有MCP连接警告2、首轮键盘夹具失败如008保存；最终无错误，不声称修复插件。无Git仓库/提交，无外部素材或新增包。T07 DONE/T08 READY，未进行Windows Build、吸附或完整自由战斗试玩。

## TEST-20260928-001 — T08 自动吸附/鼠标投影/状态协调（PASS，保留首轮失败）

- 环境：Unity2022.3.62f2 Built-In Editor Play，Unity MCP连接当前PinballCombat；菜单Tools/Orbit Breaker/Validate T08。正式attachDuration=1.5、stunDuration=.65，未改配置资产。
- PASS 2026-09-27T17:19:40.4385179Z（本地09-28 01:19:40），[报告](EVIDENCE/T08-physics.json)。两个持久化吸附点；Ready/远处拒绝，近处接受；静止动态悬停/屏蔽WASD且不蓄力，1.5秒自动脱离下坠、不自动冲刺；同区超冷却仍不可重入，离区后才重新接受。
- 真正物理绕转后.6秒有效角运动蓄满，半径1.35、Z锁定；满蓄力不自动发射，Space请求朝鼠标发射。无蓄力速度18，满蓄力24且真实敌人碰撞击杀+20，不同时受击；无效/视口外鼠标拒绝发射；透视射线投影往返误差<.001。
- 真实敌人接触使吸附球失控并脱离，禁止再次吸附/冲刺；受击与救球同一步仍保留向上冲量，不解除失控。点被禁用后释放；实体障碍阻断轨道并释放；E请求释放后重力恢复；胜利清除状态/冻结，真实重载恢复，位移离点后实际中央漏球失败。
- 首轮FAIL 01:18:29，报告[T08-physics-first-attempt.json](EVIDENCE/T08-physics-first-attempt.json)。已确认真实接触脱离/失控通过，但测试仍把敌人放在球下方，实体接触阻止自由下坠，导致速度断言失败。命中后禁用该运行时敌人再采样自由运动，实测vy=-5.38，通过；生产受击代码未因该夹具问题改变。
- 固定用例暂禁自动Spawner，以正式Prefab/TrySpawn准备敌人；除起点/等待准备外，绕转/碰撞/发射由正式组件驱动。胜利边界直接WinGame，不声称该条是实际得分胜利（T06回归另覆盖）。测试探针仅UNITY_EDITOR，退出恢复maximumDeltaTime。

- 001补充复验：收尾发现禁用PlayerMagnet后PlayerInput仍可能显式调用公开方法，补isActiveAndEnabled检查；新增模块禁用释放/拒绝、吸附覆盖排队冲刺、打断活动冲刺但保留冷却、吸附发射不能绕过冷却。最终完整T08物理再次PASS 2026-09-27T17:29:11.4957559Z，当前T08-physics.json为该最终报告。键盘证据仍适用于保持启用的正式输入路径，无改动。

## TEST-20260928-002 — T08 真实E/鼠标/Space/R（PASS）

- 使用computer-use技能的node_repl/@oai/sky，在实际Unity窗口聚焦Game，真实Enter开始、E吸附；鼠标依次移到吸附点右/上/左，球实际绕转，蓄力3%→43%→90%→100%，满蓄力仍保持附着。
- 真实Space朝左侧鼠标发射，DashStarted实测速度24、方向(-1,0)；再按E重新吸附，再次E脱离且未触发冲刺；夹具安排实际漏球，真实R重载恢复Ready、清空附着、正式1.5秒配置。
- PASS 2026-09-27T17:22:47.5707204Z（本地01:22:47），[报告](EVIDENCE/T08-keyboard.json)、[真实鼠标蓄满画面](EVIDENCE/T08-keyboard-charged.png)。自动测试截图T08-charged.png在同帧发射后捕获，因此用于台面视觉而非附着HUD证据。
- 输入测试仅克隆运行时配置把attachDuration设120秒等待逐个工具调用，生产1.5秒由001覆盖；准备时固定起点，不调用ToggleAttachment/TryRequestDash替代真实E/Space。鼠标通过真实指针定位（点击仅用于移动指针；游戏不读取鼠标按钮）分段绕转。不是长时间自由战斗试玩，也不是Windows Build验证。



## TEST-20260928-003 — T08 旧功能回归与最终保存（PASS）

- 按顺序执行Validate T07/T06/T05/T04/T03/T02/T01并逐项核验报告；独立存档T08-Txx-regression.json。旧验收代码未因吸附点而改动。以下UTC对应本地次日+8小时：
- T07 PASS 09/27/2026 17:23:43，[报告](EVIDENCE/T08-T07-regression.json)。
- T06 PASS 09/27/2026 17:25:33，[报告](EVIDENCE/T08-T06-regression.json)。
- T05 PASS 09/27/2026 17:26:21，[报告](EVIDENCE/T08-T05-regression.json)。
- T04 PASS 09/27/2026 17:27:37，[报告](EVIDENCE/T08-T04-regression.json)。
- T03 PASS 09/27/2026 17:29:46，[报告](EVIDENCE/T08-T03-regression.json)。
- T02 PASS 09/27/2026 17:30:29，[报告](EVIDENCE/T08-T02-regression.json)。
- T01 PASS 09/27/2026 17:31:25，[报告](EVIDENCE/T08-T01-regression.json)。
- 覆盖旧受击/攻击冷却/危险机关/失控J/K救球、刷怪/追击/上限/实际冲刺击杀与混合达500胜利、目标方向/速度/反刷分、踏板区域/冷却/质量补偿、普通冲刺、WASD运动请求/重力/墙/中央漏球、两终局真实场景重载、Builder重复构建/保存重开/配置GUID与自定义子对象保留。
- 最终Editor快照01:32:21：[T08-editor-final.json](EVIDENCE/T08-editor-final.json)。Battle已保存isDirty=false/buildIndex0/rootCount1，Editor非Play非编译；绿色点2、Hazard2、Spawner启用、无敌人或测试探针落盘；Ready/无吸附或失控/蓄力0/冲刺冷却0。宽15、Perspective FOV42/原位置保持。八参数，attachDuration=1.5、stunDuration=.65，maximumDeltaTime退出恢复.333333。
- Console最终错误0/警告0；首轮夹具错误已保存，不冒充从未失败。Play切换时MCP曾暂时ping未就绪，后续重连/执行正常，未改插件。
- [SHA256范围核验](EVIDENCE/T08-file-changes.json)：既有文件修改6（OrbitBreakerBattle、Builder、Motor、Input、Feedback、OrbitBreakerScoreDisplay），新增8含AttachmentPoint/OrbitBreakerPlayerMagnet/T08Validation/绿色材质及对应meta；无删除。原Config/其他场景/旧材质/敌人Prefab/Packages/ProjectSettings未变。
- 实际画面检查保留近端斜俯视3D台面，两个绿色柱/轨道可辨识，球有体积/阴影。附着变绿、满蓄变白，计时和蓄力可见。
- 未进行Windows独立构建、长时间无干预自由战斗试玩和完整T09 HUD；Git未初始化，无commit。未创建外部通信或上传。

## TEST-20260928-004 — T09 吸附敌人防护/真实击退与HUD物理验收

- 环境：Unity2022.3.62f2/Built-In/MCP，正式配置attachDuration=1.5/stunDuration=.65。菜单Tools/Orbit Breaker/Validate T09。
- 首轮PASS 10:16:08。已存在敌人在E附着时立即忽略玩家物理配对；检测到实际接近后向外真实位移、速度明显大于追击，玩家不受击/不脱离/不偏离轨道且可继续蓄力，敌人不死且分数0。
- 附着中生成第二个敌人同样忽略碰撞并击退；手动脱离、1.5秒超时、终局、敌人禁用均恢复配对，敌人重新启用正确恢复防护关系。退出吸附后普通真实敌人碰撞仍失控；真实危险机关依然打断附着并失控。
- 准备/游玩/冲刺/冷却/吸附/失控/胜负HUD与真实状态一致；中文字体存在，真实冲刺击杀使分数+20且显示一次得分反馈；临时提示自动消失。真实场景重载清空HUD/分数/状态，实际漏球显示失败。
- 胜利边界用WinGame调用检查终局配对/UI，不冒充实际达分胜利；实际混合计分胜利由T06回归。固定夹具暂停随机刷怪，调用正式TrySpawn安排真实敌人，其他运动/碰撞由生产代码处理；不是完整自由试玩。
- 已逐张查看T09-ready.png、T09-repel.png、T09-stunned.png，中文无缺字/文字重叠，3D台面与中央战区保留，击退闪色/受击红边/技能锁定可见。另保存orbit/cooldown/score/victory/defeat图；终局技能文案修正后已启动最终完整复验，结果见T09-physics.json。
- 最终完整物理复验PASS 2026-09-28T02:19:39.2944379Z（10:19:39），正式1.5秒配置，报告EVIDENCE/T09-physics.json；所有截图已随最终复验重拍。

## TEST-20260928-005 — T09 真实键鼠与窗口恢复（PASS）

- 首次真实输入尝试因Windows未枚举到Unity可操作窗口而NOT_RUN；10:20 MCP HTTP短暂断开。保留T09-keyboard-window-unavailable.json。用户恢复窗口后MCP与Windows窗口均恢复，未改插件或安装依赖。
- 使用computer-use技能的node_repl/@oai/sky，真实Enter开局、E吸附；实际敌人靠近被击退，玩家保持吸附、不失控、分数0。鼠标从柱下移到右、上、左，HUD蓄力3%→47%→93%→100%，满蓄不自动发射。
- 真实Space发射测得初速24、方向(-1.00,-0.09)；随后E再次吸附、第二次E脱离且不产生额外冲刺；真实漏球进入Defeat，R重载恢复Ready与正式1.5秒配置。
- PASS 2026-09-28T02:28:14.7951701Z（10:28:14），EVIDENCE/T09-keyboard.json。截图T09-keyboard-repel.png、T09-keyboard-charged.png。中文HUD在实际Game中无缺字、状态/倒计时/蓄力正确。
- 测试夹具仅在Play克隆attachDuration=120秒以等待工具输入，生产参数未改；按键真实经过PlayerInput，鼠标通过Windows定位，未用API替代输入；不是3–5分钟自由战斗试玩或Windows Build。

## TEST-20260928-006 — T09 旧功能回归与最终保存（PASS）

- 本轮依次运行Validate T08/T07/T06/T05/T04/T03/T02/T01，均PASS；报告独立归档EVIDENCE/T09-Txx-regression.json。UTC时间依次02:28:53、02:29:41、02:30:57、02:31:34、02:32:15、02:32:50、02:33:47、02:34:24（本地+8小时）。
- 覆盖吸附时限/离区重入/动态碰撞/状态优先级、受击保护/危险机关/失控踏板救球、敌人刷新上限/真实击杀和达分胜利、目标反刷分、左右踏板、基础冲刺/重力/XY/墙/漏球、Builder幂等/配置和自定义子对象保留/两结局真实重载。T08仅按D017新用户规则替换敌人碰撞预期，其他历史报告不覆盖归档。
- 最终保存和快照10:35:03：EVIDENCE/T09-editor-final.json。OrbitBreakerBattle isDirty=false、buildIndex0、rootCount1；Editor非Play非编译、HUD1、吸附点2、Hazard2、Spawner启用、敌人0/探针0、Ready/无附着/失控/冷却。正式八参数与Perspective/FOV42/相机位置不变，maximumDeltaTime=.333333。
- 最终read_console错误0/警告0。Play切换期间一次ping not ready，重试恢复；没有未解决编译错误。
- SHA256范围：4个既有脚本修改、T09验收与meta共2个新增、无删除；OrbitBreakerBattle/Config/Prefab/材质/其他场景/Packages/ProjectSettings内容未改。证据T09-file-changes.json。
- 尚未执行完整3–5分钟自由战斗或Windows Build；Git未初始化，无提交。

## TEST-20260928-007 — T10 首轮时限/特效验证与修正

- 正式配置7.5秒、过旧1.5秒后仍可真实绕转蓄满，能量弧/实际轨迹可见；精确时限PASS。首轮T10-integration-first-attempt.json在拖尾清理断言失败（14:20:03），未进入完整胜败测试。
- 原实现只停止emitting并Clear，TrailRenderer可能仍保留一个非绘制锚点；修正为脱离/禁用时同时关闭renderer，验证不可见且最多一个不构成线段的锚点。没有放宽时限/状态清理要求。并将尾迹限制台面方向、增加转角细分/最小采样距离以消除密集锯齿。
- 第一张实际截图已检查，中文版面正常且原3D台面不变；最终截图和完整验收待重测。

- 第二次时限/特效检查通过，但夹具在Playing直接调用RestartGame，正式代码按规则拒绝，仅测试流程等待重载。已停止并保留T10-integration-fixture-restart.json；修正为先结束隔离特效夹具，再重载正式整局，生产GameManager未改。后续胜败回合不注入结束或分数。

## TEST-20260928-008 — T10 正式时限/特效/连续完整胜败与真实键鼠（PASS）

- T10-integration.json最终PASS 2026-09-28T06:24:31.2383205Z（14:24:31）。正式配置7.5秒：超过原1.5秒继续动态绕转且蓄满，能量弧/尾迹真实可见；7.5秒超时脱离，弧线和尾迹renderer关闭，真实重载不残留运行时特效。
- 隔离特效夹具仅安排起点、关闭刷怪；测试结束先LoseGame再重载。之后两条完整回合从磁盘场景出生点启动，正式参数/正常刷怪，不移动任何物体起点、不写Rigidbody速度/位置、不直接注分或调用WinGame/LoseGame。
- 完整胜利回合仅通过SetMoveInput/TryRequestDash发正常移动冲刺请求，五次真实金靶碰撞累计500分、自动触发Victory，敌人正常生成（最高1），5次冲刺，无受击，实测游戏时间3.70秒。随后真实场景重载清零并保持Spawner启用。
- 完整失败回合不给输入，从保存出生点自然下坠，0.93秒漏球进入Defeat，真实重载恢复Ready、7.5秒、清空状态。T10-full-victory.png / T10-full-defeat.png保存。
- **节奏观察**：理想脚本连续瞄准金靶可极快通关；无输入出生点到漏口很近。这是可复现的平衡限制，不能把这个自动驱动结果写成3–5分钟人工试玩或已证明普通玩家平均时长。用户本轮明确数值调整仅吸附×5，其余七参数保持；T12真人试玩需记录可理解性/节奏后再决定目标布局或得分调优。
- 真实键鼠PASS 2026-09-28T06:30:18.7682681Z（14:30:18），EVIDENCE/T10-keyboard.json：实际Enter/E/鼠标3%→47%→93%→100%/Space初速24、E再附着再释放、漏球后R全部通过。游戏画面能量弧由绿变金，发射/脱离后不残留；重载正式配置7.5秒。
- 输入测试使用computer-use技能的node_repl/@oai/sky和实际Unity窗口；沿用运行时120秒等待夹具，正式7.5秒由T10自动计时验证。没有用脚本替代真实键盘事件。此处真实输入为夹具辅助，完整胜利是自动输入请求，明确区分。
- T10-orbit-vfx.png与T10-keyboard-charged.png实图已检查，保留3D台面、中文字形/HUD可读，能量弧可见。使用Unity内置着色器，无新增外部素材。

## TEST-20260928-009 — T10 全回归、保存与范围（PASS）

- 新7.5秒下依次执行T09/T08/T07/T06/T05/T04/T03/T02/T01，全部PASS。各报告归档EVIDENCE/T10-Txx-regression.json，UTC依次06:25:48、06:26:45、06:31:30、06:32:34、06:33:12、06:33:58、06:34:45、06:35:42、06:36:52（本地+8）。旧菜单默认报告为最新运行，查T09阶段原参数证据须结合TESTS004–006与T09-editor-final；本轮归档明确7.5秒。
- 自动覆盖全部移动/冲刺/受击/救球/吸附/刷怪/计分/胜负/重载/幂等保存，真实键鼠见008；没有引入第二物理写入者。T03原有闪色与恢复回归通过。
- 最终14:37:49：Battle已保存isDirty=false、buildIndex0/rootCount1，Editor非Play非编译；Ready、HUD1、吸附点2、Hazard2、Spawner启用，敌人0/测试探针0/运行时能量弧与尾迹0。8字段，attachDuration7.5，其余12/20/18/.75/.65/2/500不变；透视FOV42原位置保持，maximumDeltaTime恢复.333333。
- Console最终0错误0警告。早期夹具失败已保留，Play切换时偶发MCP ping not ready后恢复，不算编译错误。
- SHA256范围T10-file-changes.json：既有5文件修改（Config资产/default、OrbitBreakerPlayerFeedback、T08/T09验证），新增T10验证+meta共2，无删除；场景/Prefab/材质/其他场景/Packages/ProjectSettings未改。
- Git未初始化、无commit。Windows Build未执行；理想输入过快通关及3–5分钟人工节奏未验证见008，不作独立交付/真人长试玩声明。

## TEST-20260928-010 — T13 宇宙盘与战斗集成（最后复核中）
- 环境：Unity 2022.3.62f2 Built-In，当前Editor/MCP；未进行T11 Windows构建。
- 新增可复现菜单Validate T13（OrbitBreakerT13Validation.cs），报告Docs/EVIDENCE/T13-integration.json。先用明确的隔离摆球/速度夹具检查真实Collider接触，最后重载原生产场景，仅发送正常移动/冲刺请求，保留自动刷敌完成整局。
- 首轮PASS（20:40）：敌100→60→20→0、仅死亡80基础分；左右弹板真实速度与免伤；两次连弹1.5倍、弹射真实碰撞60伤害；真实撞柱/摆锤/旋转挡板继续连弹；大奖洞捕获、300×倍率、0.65秒弹出、防停留/重入；实际旋转扣血/击退、防推、蓄力/特效/Space；普通受击重置、8秒过期、真实危险碰撞与冲刺免伤；3000分胜利/重载；无敌进入黑洞仍死亡。
- 原生产场景完整自动胜局28.65秒3025分、最高6敌、18次得分；无操作0.95秒漏球。不能称人工无辅助通关或已达到3–5分钟节奏。
- 验证迭代：击退瞬态需记录事件发生而不是0.45秒后状态；黑洞保护需在实际触发死亡前采样。金靶移至y8.7避免与大奖洞捕获区域叠加；黑洞视觉深度调整不改变Collider。
- 最后审查修复同一物理步吸附发射被板冲量替代时清理magnetLaunchQueued，追加“只计一次、之后普通冲刺不计连弹”断言。自动夹具移除临时运行时PlayerInput（禁用的MonoBehaviour仍接收失焦回调），防用户/工具切窗口取消脚本请求；生产输入仍保留失焦取消机制。以最后JSON为准。

## TEST-20260928-011 — T13 真实键鼠 PASS
- computer-use技能与@oai/sky操作当前Unity窗口；真实Enter、W/A/S/D、J、K、E、鼠标绕点、Space、R均由PlayerInput/FlipperController实际消费。
- 20:51:08 PASS，证据T13-keyboard.json、T13-real-orbit.png。Space实测满蓄发射速度24、保护生效，R恢复Ready/0分/0连弹/正式7.5秒。
- 键鼠夹具禁用自动刷敌、摆球等待按键，临时克隆配置吸附120秒以容纳工具操作延迟；未保存到生产资产。真实7.5秒时限由生产配置/既有T10时限和本轮物理状态验证区分记录。
- 初次键盘夹具按“combo比前次大”等待K，工具停顿超过8秒会正常断连导致等待；改为观测真实Activated事件后通过，未修改游戏窗口/连弹时长。
- 实际画面人工查看了3D宽台面、星云、星球、底部黑洞/弹板、双机器/大奖洞、HUD和旋转蓄力效果。Shader无错误。
- 完整自动驾驶复核曾在2850分被敌人逼入上角超时（T13-driver-timeout-before.json）；夹具当时只在y<7.1时冲刺，无法主动退出上角。修正为上角向场内/下方冲刺，并固定随机种子1301以复现。未改动生产敌人/物理/得分，也不把自动驾驶失败隐藏为游戏通过。
- 固定种子的首次自动回合在1950分正常漏球（T13-driver-defeat-before.json）；再次完善测试操作：大奖洞本身向下弹出时不额外按下后退，金靶离开即可返身，底部调用正常J/K弹板请求。仍无摆球/加分/无敌作弊；生产代码未改。

## TEST-20260928-012 — T13 最终复核 PASS（21:12）
- T13-integration.json于21:09:11 PASS，55项，包含新增的同帧板冲量覆盖吸附发射只记一次，以及取消请求不泄漏到后续普通冲刺。
- 原生产场景固定种子1301、自动刷敌、仅移动/冲刺/救球输入请求：19.69秒3050分胜利，最多6敌、19次计分。实际物理得分胜利、无敌落黑洞失败、两次重载、无输入0.95秒失败均通过。
- T13-keyboard.json真实键鼠PASS20:51；此后生产改动仅旧请求清理（集成复核覆盖）及提示文案，未改变输入映射。
- SceneBuilder最终重复运行122→122，Battle字节完全一致，重新打开场景仍完整。输入/刷敌启用，7.5秒吸附/3000分目标，0敌/0测试探针落盘；maximumDeltaTime恢复0.3333333。
- Console 0错误/0警告，Missing Script 0，两个自定义Shader无编译错误。证据T13-editor-final.json；最终1600×1000实际Camera渲染T13-cosmic-board.png已目视检查，3D透视/星球/星云/黑洞/机关可见。
- 范围核验：8个既有文件修改、45个新增文件（含材质/脚本/Shader及meta）；Packages/ProjectSettings/原Enemy.prefab/其他场景未修改。Git未初始化，无提交；Windows构建明确NOT_RUN（用户暂缓T11）。

## TEST-20260928-013 — T14 定向物理回归 PASS（21:32）
- Unity2022.3.62f2，真实当前Battle，菜单Validate T14，独立证据EVIDENCE/T14-integration.json，共35项。
- 实测敌人从撞柱一侧到另一侧（最终x=-.4087）、穿越运动旋转器、穿过运行时新建实体；查询所有场景实体忽略配对与新敌人之间配对，未吸附玩家碰撞保留。
- 真实普通接触失控、三次冲刺100→60→20→0并仅奖励80；实际吸附旋转扫掠伤害/击退且不断吸附；双踏板真实冲量与连弹；偏置大奖洞真实捕获计分/弹出；正式Spawner相位碰撞初始化；无敌进黑洞失败，真实重载清敌/分/连弹。
- 夹具仅Play摆球、禁用刷敌/移除临时PlayerInput以避免工具失焦影响脚本；不以此声称新一轮真实键鼠/人工通关。正式场景保留原输入/Spawner。

## TEST-20260928-014 — T14 最终显示与保存 PASS（21:37）
- EVIDENCE/T14-play-final.png：1920×1080实际Game截图，目视确认近端接近屏宽、轻边框、非对称摆设、半透明红敌与血条、中文HUD、无底部文字重叠。视觉夹具Time.timeScale=0、三个固定位置敌人，退出已恢复1，未保存测试对象。
- 最终相机(0,-28,-20)，FOV27.314；16:9/16:10/4:3近端光轨跨度94.89%，远端/底部归一化Y均在0–1内，见T14-editor-final.json。响应宽高比通过数学投影核验，完整Game截图只实测1920×1080。
- 重建127→127且Battle逐字节不变；Edit、非编译、非dirty，Missing Script0，自定义敌人Shader无错误，Console0错误0警告，输入/刷敌启用，场景0敌人。timeScale=1、maximumDeltaTime=.3333333。
- 与T14基线哈希比较：5修改、10新增（含meta）、0删除；Packages/ProjectSettings/其他场景不变。T13旧报告保留，没有以旧自动通关结果冒充新布局通关。本轮未重测完整胜利/真实键鼠/Windows构建。

## TEST-20260929-001 — T15 定向物理与持续回合 PASS
- Unity2022.3.62f2，最终22柱/5浮动板Battle，菜单Validate T15，T15-integration.json 40项PASS。真实碰撞充能4→1、无资源拒绝、无效鼠标/取消不扣次、鼠标右向覆盖WASD左向、实体击杀补次、3上限/1秒冷却且冷却不补次、运动板反弹、吸附旋转伤敌/击退/蓄力/消耗1次速度>22、黑洞外光环安全/核心无敌死亡、真实重开资源清零。
- 最后真实生产场景种子1501，只用输入等价请求，不注入分数/资源/位置、不禁用刷敌：45.01秒121有效碰撞2910分仍Playing，峰值6敌/3冲刺储备。4板中间版42.95秒99碰撞3065 Victory，最终统计明确分开。
- 早期失败均保留：T15-contact-fixture-before（密集位置干扰单次断言，换隔离上部柱后4次逐次验证）；T15-enemy-fixture-before（敌人在等待时实际被击杀，修夹具空引用）；T15-board-fixture-before / T15-dense-rebound-observation-before（运动板后立即撞第二根柱，末端速度不代表板发力；改为接触+冲量事件之后的物理帧观察，因为AddForce事件当下速度尚未积分）；T15-core-fixture-before（.04秒观察时已进入可见核心，起点上移再验证免伤入洞死亡）。未修改生产规则去掩盖这些夹具问题。
- 夹具仅Play内摆球/敌人、移除临时PlayerInput避免失焦取消请求；生产阶段重新加载后不摆球且自动刷敌。最终输入组件、物理时间参数均恢复。

## TEST-20260929-002 — T15 真实鼠标/键盘 PASS
- computer-use技能/@oai/sky操作Unity窗口：真实Enter，自动物理4次接触赚1次，鼠标点击右上Game画面后真实Space。T15-keyboard.json 5项PASS；实际XY方向(0.70,0.72)、速度>17、次数1→0，T15-real-mouse.png。
- 准备和等待用摆球夹具，未直接注入冲刺资源；没有把此夹具当无辅助真人游玩。该输入验证在追加底部板前完成，此后仅场景救球板和顶部提示外观修改，输入/资源代码不变。

## TEST-20260929-003 — 无输入开局与底部容错
- 两次使用种子1502，正常重力与自动刷敌，移除运行时输入确保无操作，正常StartGame开局17速度发球；仅记录状态/峰值，无摆球/资源注入。
- 加底部板前2.62秒2碰撞后Defeat（T15-passive-before-shuttle.json）；加RescueShuttle后7.52秒12碰撞、峰值9连弹后Defeat（T15-passive-rally.json）。改善了初期获得冲刺的机会，仍要求主动救球，不提供隐藏免死。

## TEST-20260929-004 — 最终编辑器/保存/范围 PASS
- T15-editor-final.json：Battle非Play/dirty/编译，重复Build225→225且字节不变，22Bumper/5FloatingBoard、Missing Script0、运行时敌0、输入/刷敌启用，gravity7.5/cooldown1/attach7.5/target3000，timeScale1/maxDelta.3333333。Console0错误0警告。
- 核心位置(0,-10.5,0)，尺寸2.8×2.0，ContainsCore中心真、x边外1.41假；真实Trigger穿越另在集成验证。T15-play-final.png为最后5板版本自动生产回合实际1920×1080截图，目视確認层次/密度/鼠标箭头/右栏移除/黑色核心与外光环区别。
- T15-file-changes.json：10既有文件修改，8新增含meta，0删除；Packages/ProjectSettings/其他场景不变。未做Windows构建，Git未初始化、无提交；3–5分钟人工节奏未证明。


## TEST-20260929-005 — T16 随机场景与护壳集成（PASS）
- 环境：Unity2022.3.62f2/Built-In/OrbitBreakerBattle，MCP运行 Tools/Orbit Breaker/Validate T16。
- 35项PASS，报告Docs/EVIDENCE/T16-integration.json；跨区180/-180仍Playing、25块回收/同seed重访，球居中/鼠标投影、.8缩放、4物理碰撞资源、正常/吸附发射与免伤、真实敌HP击杀、浮动板真实反弹、旋转伤害、三壳与黑洞外光环安全/核心扣1/2秒去抖/冲量弹出、0壳失败/场景重开。
- 最后生产输入请求回合28.36秒/21碰撞/峰值4敌/累计195块/常驻25/0分/三壳耗尽Defeat，居中误差9.54e-7。截图T16-play-final.png；没有对这段回合摆球/加分/资源注入。
- 首次测试重访断言在精确y=0边界被真实重力推进负块；修正测试停放在(1,1)，未改生产生成算法。原失败证据T16-chunk-fixture-before.json保留；并非生产地形随机不稳定。测试所设maximumDeltaTime结束恢复.3333333。

## TEST-20260929-006 — T16 真实鼠标键盘（PASS）
- 菜单Check T16 Mouse Keyboard，使用computer-use的@oai/sky对Unity Game输入真实Enter/鼠标右上/Space。
- 5项PASS，实际方向(.78,.63)、速度>17、次数1→0。报告T16-keyboard.json，截图T16-real-mouse.png。
- 充能使用4次真实碰撞夹具，等待时稳住球，生产PlayerInput未移除；此记录仅验证真实输入链，不代表无辅助真人通关。

## TEST-20260929-007 — T16 远处敌人与计分闭环（PASS）
- 菜单Validate T16 Combat；31项PASS，报告T16-combat.json。
- 在(180,180)六次相对玩家11–15单位刷敌、上限、实例.64缩放；228环境碰撞配对忽略且玩家实体对保留；真实追击后移到(360,360)回收旧敌。
- 克隆随机靶真实向上碰撞得分；来自正式大奖洞模板的6次实际触发测试夹具达到3000分Victory，捕获/延迟/弹出保护完整，冻结与重载Ready/三壳/清分/新seed正常。并非自然随机布局的无辅助通关路线。

## TEST-20260929-008 — T16 保存/编译/幂等（PASS，2026-09-29 10:42）
- Battle已保存、Edit、非dirty、无编译，233保存Transform含停用模板；重复Build字节完全相同，缺脚本0，无运行时敌人/探针/生成块保存。旧Arena停用、9模板引用都有效且不活跃、3材质引用有效、seed0表示下次Play随机；新跟随启用/旧构图禁用，输入刷敌启用。
- Console读取0错误0警告；证据T16-editor-final.json。场景备份和哈希T16-baseline.json/T16-OrbitBreakerBattle-before.unity.txt，变更T16-file-changes.json：9修改14新增0删除，Config/Packages/ProjectSettings/其他场景未改。
- 未进行：Windows构建、长期性能压力、人工3–5分钟手感验收；T11用户继续暂缓。


## TEST-20260929-009 — T17 主集成（PASS，2026-09-29 21:56）
- Unity2022.3.62f2/MCP菜单Validate T17；52项PASS，T17-integration.json。测试使用一次性运行时Config克隆，未把测试值写回正式资产；场景只在迁移时保存。
- 修改血量160/容量2/速度12/冷却.5确实改变生成敌/胶囊/实速/计时，恢复正式值继续测；三柱真实反弹分别约13.55/16.57/20.70（已扣采样时重力），证明颜色对应不同真实冲量。
- 2.2距离E等价请求吸附成功，静止鼠标自动满蓄，黑敌不会打断/减速吸附，Space朝鼠标>22速度消耗1次；范围内移动实测vx1.3、冲刺约11.7，离开恢复。
- 真击杀配置0不掉/1掉、拾取补盾不超3；奖励洞实际触发>=600分、7秒增益、弹射20→27/冲刺18→24.3、红尾迹和到期恢复；12敌/4黑敌，更快机关、普通三壳失败和重开。
- 最终随机生产输入请求回合30秒13碰撞/峰值6敌/220分/2壳Playing；先前同逻辑局30秒24碰撞/峰值8敌/1738分/1壳Playing。测试段不摆球/注入分数资源，但不是人工试玩。
- 第一次测试发现Bumper碰撞入口漏接Variant而仍用16速，已修复，原失败T17-bumper-before.json保留。最终重跑在单次拾取防重复与概率端点处理后PASS。

## TEST-20260929-010 — T17 其余参数/音频/胜利（PASS）
- 菜单Validate T17 Config Audio；12项PASS，T17-config-audio.json。
- 运行时克隆Config改刷新.4秒→1.3秒实际3敌；吸附1.2秒→1秒仍吸/1.3秒脱离无自动冲刺；单个补盾在1壳时只恢复到2，触发多回调不多补。
- 程序音频经AudioSource/AudioListener实际输出，峰值RMS .004938201，isPlaying true；音量/Listener/Editor mute正常。首个单点采样恰为0，换为最多1.2秒窗口轮询读到有效样本；原T17-audio-sample-before.json保留。没有声称人工听感评价。
- 奖励洞金色星标存在，改目标500后真实入洞600分立即Victory/冻结，重载Ready/三壳/正式3000目标/7.5秒吸附。

## TEST-20260929-011 — T17 真实E/Space（NOT_RUN）
- Check T17 Mouse Keyboard菜单已实现，实际运行待Unity窗口恢复；T17-keyboard.json记录限制。
- computer-use通过返回窗口标识5835726获取窗口，activate_window超时，get_window_state提示not a usable app window，再次list_windows没有Unity可用窗口。已用异步问题请用户恢复，不通过其他输入后门绕过。
- 后台MCP与Editor Play验收正常；物理自动绕转/Space请求及原PlayerInput映射已检查，不把这些说成真实按键通过。

## TEST-20260929-012 — T17 最终保存与视觉（PASS）
- T17-editor-final.json：Battle与Config保存，234→234重复Build、场景/配置字节不变，8可见核心参数和中文Editor正确绑定；缺脚本0，Console0错误警告，非Play/非dirty/非compile，时间1/.3333333，输入/刷敌/胶囊/音效启用；无敌/拾取物/测试夹具保存。
- T17-production.png为真实随机生产输入请求画面；T17-reward.png为奖励验证夹具画面（测试摆球可能拉长尾迹）；T17-slime-pickup-preview.png为明确暂停摆放的视觉夹具，黑球/紫圈/绿色+/金星奖励洞可辨识。预览退出且恢复时间，无场景写入。
- 基线T17-baseline.json/T17-OrbitBreakerBattle-before.unity.txt；T17-file-changes.json列出15修改18新增含meta0删除，Packages/ProjectSettings/其他场景不变。无Git提交/无Windows构建，T11用户暂缓。

## TEST-20260930-T18 — 速度战斗与模式闭环（2026-09-30 03:38）
- 环境：Unity2022.3.62f2 Editor、Built-In、Unity MCP已连接；Windows桌面真实输入使用computer-use。非Windows Build。
- 自动复现：Edit打开Battle，Tools/Orbit Breaker/Validate T18；报告EVIDENCE/T18-integration.json，48项PASS。测试包含实际刚体低速敌接触、高速伤害、两敌贯穿、实际绕转9.06→16.26→24、锁定发射、柱碰撞补次数、奖励32.4、Boss实体触发伤害及反向速度、危险预警/激活、毁生成柱、Boss死亡胜利/同模式重开/三壳失败。
- 隔离说明：测试会停刷怪/地形并布置专用碰撞；Boss激光/触手定向验证临时禁用自主攻击和移动，避免不同攻击互相污染结果。Boss最后归零通过公开伤害入口重复触发，单次真实物理撞击和反向弹开另行验证。不能据此声称真人已完整通关。
- 真实输入：Tools/Orbit Breaker/Check T18 Mouse Keyboard；EVIDENCE/T18-keyboard.json，7条PASS。实际点击Free、点击右侧柱、按E从2.2m吸附、自动加速、不绕鼠标、真实Space以24速冲向锁定目标，胶囊1→0、锁定清除。7条包含夹具设置说明，非7种独立操作。
- 输入时间限制：为处理桌面工具延迟，只在测试运行时克隆配置将吸附7.5改20秒；蓄满后Time.timeScale=.1等待真实Space，完成恢复1。正式配置未改。早期超时/空按键与夹具假阴性报告已保留，不算生产通过证据。
- 正常生产GUI：真实点击图鉴、标签、滚轮看到Boss/触手数值；Esc回Ready；真实点击Battle，出现章鱼与HP条；正常时钟观察自动触手2次/激光1次/毁物件1次/受撞1次。真实Esc回Ready且Boss0，再Esc停止Play。见T18-ui-checks.json、T18-production-schedule.json、T18-main-menu.png、T18-catalog-rules.png、T18-battle-production.png。
- 插件：MCP打开Tools/Orbit Breaker/战斗参数编辑器，实际点击速度页，八项中文参数可见；以同一SerializedObject机制把HP2400改2500，Undo还原2400再保存，PASS（T18-plugin-undo.json）。字段实时影响另有集成测试。
- 最终Edit：T18-editor-final.json；Battle已保存/不脏，235Transform、Missing Script0、测试/Boss/生成块残留0，正式输入与Spawner启用，Perspective保持，吸附7.5/timeScale1/BossHP2400。Build两次场景与两个配置内容不变；最终Console0错误0警告。
- 修复追溯：外部真实高速曾被普通移动8速上限吞掉，Motor现识别已有外部动量。接触夹具距离、锁定后碰撞补资源、危险攻击隔离、真实输入等待固定步等假阴性均在T18-before-*或T18-keyboard-before-*证据中保留。
- 范围：T18-file-changes.json，12修改/22新增含meta/0删除；Packages/ProjectSettings及其他场景未变。
- NOT_RUN：独立exe构建/退出、3–5分钟真人满意度、完整正常时钟真人Boss通关、长距离毁地形重载压力测试。T11用户暂缓。

## TEST-DOC01 — 2026-09-30 03:57
- 两份DOCX由python-docx生成；默认render_docx.py因无LibreOffice失败，改用Word后台导出PDF及内置Poppler生成PNG。
- 初版分页溢出与标题样式线已修正；最终运行4页/设计4页全部目检，中文正常、无空白页、无裁切或表格跨断。
- 桌面复制SHA256校验一致，验证见Docs/Deliverables/verification.json。无新增游戏功能验证声明。

## TEST-20260930-T19 — 手感与慢动作（PASS）
- Unity2022.3.62f2 Editor/MCP；菜单Validate T19产出T19-integration.json，55条PASS（含19个图示纹理检查）。真实轨道镜头偏移0、24速上射.2秒保速、结束恢复重力；18速实际撞击主/邻/远敌HP27/79/100；贯穿两敌只爆炸一次；Boss额外爆炸仍反弹。
- 黑洞实际扣1壳与消耗、奖励洞消耗后仍弹出、吸附柱离开消耗、分块卸载重载不复活、源模板保留；Focus全局.2和物理步长.004、真实2秒、不续时、无次数失败不取消、菜单/终局恢复。
- Check T19 Mouse Keyboard产出T19-keyboard.json，4条PASS：真实点击Free与右键生效/2秒自动恢复/真实Space速度21.00002、胶囊3→2且恢复时钟。第二阶段为工具输入间隔专用夹具，每帧延长focusEnd等待Space；第一阶段2秒为生产规则，正式资产不变。之前工具迟到和续接超时FAIL保留在before文件，不计通过。
- T19-T18-regression.json：在T19代码上48项旧速度/Boss回归PASS，原T18报告已恢复以保留历史。
- 正常生产菜单真实点击图鉴三标签/滚轮：19图示、长说明、最后一项与返回键可读，截图T19-catalog-*.png。实际冲刺爆炸视觉夹具截图T19-actual-impact.png；加粗外圈后可辨识，临时定帧便于检查，非整局人工通关。
- 仅最后VFX线宽改动后做视觉和48回归，无重复全套55；不影响伤害逻辑。最终Edit报告T19-editor-final.json，235→235/两次Build场景双配置字节相同、Missing0、probe0/Boss0，Console0错误警告。无T19新增场景或配置变化。
- NOT_RUN：真人满意度/长局平衡；独立构建此时由新授权T11接续。

## TEST-20260930-T11 — 构建与首次启动
- 新Editor/PinballWindowsBuild菜单构建Battle为Windows x64 Mono非Development版本，窗口模式1280×720；构建后恢复工程原显示设置。
- 初版构建脚本日志计数类型uint不匹配Unity int，已修正；之后编译通过。
- EVIDENCE/T11-build.json：2026-09-30T06:19:11 UTC，Succeeded，约31.76秒、92,252,215字节，0错误、2警告。警告来自MCP websocket在重载时中断，构建报告为成功；工具连接返回空失败不代表Build失败。
- 通过computer-use真实启动01_Demo/OrbitBreaker.exe，看到正常中文主菜单、自由/战斗与图鉴按钮。Player.log已留存。
- 用户此时明确“不用检查了，直接后续的输出文档”，所以停止；独立版输入/胜败/重开/退出、其他电脑启动NOT_RUN。
- 更正T19“配置无变化”的简写：最终重复Build前后字节幂等成立；但相对于T19基线，OrbitBreakerCombatConfig.asset dashSpeed由18变为24，已保留当前值，来源未推测。文件清单已列出该资产。18速集成属于调整前版本。
- 当前文档依用户指示直接输出，未运行本轮DOCX渲染QA；无虚报分页检查。


## UPLOAD01 — 2026-10-01 断点
- 命名迁移完成；115 个自有资产与 75 个脚本主类静态核对通过，GUID 保留。
- Unity 重新构建权限请求被拒绝，编译和游戏运行 NOT_RUN。Demo 是原有构建，只改 exe 与 Data 目录名。
- 当前分支 orbit-breaker；推送待完成。主界面追加文字和位置待用户回复。
- 证据 Docs/UPLOAD01-static-verification.json；最新交接 Docs/HANDOFFS/2026-10-01_Codex_Upload.md。


## UPLOAD01 上传结果
- 2026-10-01：orbit-breaker 分支已推送至 https://github.com/LinQianhe02grey/Lqh-Demo/tree/orbit-breaker ，首提交 0556cd1。视频 Git LFS 240 MB 上传成功。
- 上传完成；命名迁移静态检查通过，Unity 编译仍 NOT_RUN。主界面追加文字及位置待用户确认。
- 下一步：收到文字与位置后完成该单项修改；如需重新构建，须获得运行 Unity 的授权。桌面原件保留。

