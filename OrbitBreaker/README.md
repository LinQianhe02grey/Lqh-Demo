# 星际弹珠 · Orbit Breaker

宇宙主题的 3D 弹珠战斗原型，包含自由模式、章鱼 Boss 战斗模式、随机机关与速度伤害。

## 下载与启动

本目录位于仓库的 `orbit-breaker` 分支，与根目录的 Cardwin 工程独立。

Windows 64 位：完整下载 `星际弹珠-WindowsDemo` 文件夹，双击 `OrbitBreaker.exe`。保留旁边的 `OrbitBreaker_Data`、`UnityPlayer.dll` 和 `MonoBleedingEdge`，不要只下载 exe。

试玩视频通过 Git LFS 保存。使用安装了 Git LFS 的 Git 获取完整文件：

```sh
git clone --branch orbit-breaker https://github.com/LinQianhe02grey/Lqh-Demo.git
cd Lqh-Demo
git lfs pull
```

## 操作

- WASD 移动，鼠标瞄准，左键锁定目标。
- E 靠近绿色柱子后自动绕转；Space 消耗一次胶囊发射。
- 每 4 次有效碰撞或击杀 1 个敌人补充一次发射机会，上限 3 次。
- 右键慢动作 2 秒，期间 Space 可保速改向。
- Esc 返回主菜单，主菜单 Esc 退出。

## 工程与资料

- `星际弹珠-可编辑工程/OrbitBreaker`：Unity 2022.3.62f2 LTS 工程。Unity Hub 添加此目录，打开 `Assets/Scenes/OrbitBreakerBattle.unity`，点击 Play。
- `星际弹珠-文档`：原有四份 PDF，保留正文，仅调整文件名。
- `星际弹珠-试玩演示.mp4`：原有演示视频。
- `星际弹珠-重命名映射.json`：自有代码和资产迁移映射。

## 本次上传的验证范围

2026-10-01：自有源码、类名、命名空间与资产名称已统一为 OrbitBreaker；Unity 资产 GUID 保持不变。Unity 必需目录、第三方库与许可证保留原名。

本轮重新构建请求未获授权，改名后的源码尚未经过 Unity 编译或运行验证。Windows Demo 是原有已构建版本，仅将启动文件及配套 Data 目录改名；不是改名源码重新编译的产物。原有记录只证明该版本成功构建并启动至主菜单，不代表完整独立版操作验收。

原 PDF 和视频保留原文，因此部分旧工程名称或路径仍可能出现；工程位置和新版资产路径以本说明及重命名映射为准。游戏菜单追加文字尚待确认，本次未添加未知文字。
