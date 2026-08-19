# FriendshipAssistant

[![Stardew Valley](https://img.shields.io/badge/Stardew%20Valley-1.6%2B-brightgreen)](https://www.stardewvalley.net/)
[![SMAPI](https://img.shields.io/badge/SMAPI-4.0%2B-blue)](https://smapi.io/)

中文 | [English](README_EN.md)

**FriendshipAssistant** 是一个星露谷物语送礼辅助模组。与 NPC 对话结束后，它会根据你背包里的物品和游戏原生送礼喜好数据，弹出一个物品栏风格的送礼建议界面，帮助你快速选择合适的礼物。

---

## 目录

- [功能特性](#功能特性)
- [工作原理](#工作原理)
- [安装](#安装)
- [使用方法](#使用方法)
- [配置](#配置)
- [兼容性](#兼容性)
- [构建](#构建)

---

## 功能特性

### 核心功能

| 功能 | 说明 |
|------|------|
| 对话后送礼建议 | 与 NPC 对话结束后，自动分析背包并弹出送礼建议界面。 |
| 原生喜好判断 | 使用 Stardew Valley 原生 `NPC.getGiftTasteForThisItem` 判断礼物喜好。 |
| 分组展示 | 按最爱、喜欢、普通、不喜欢、讨厌分类展示物品。 |
| 物品栏风格界面 | 使用物品图标网格展示候选礼物，点击物品即可赠送。 |
| 自动交好 | 可开启自动送礼，对话结束后直接送出当前最优礼物。 |
| 存储设备送礼 | 可选显示木箱、石箱、大箱子、石制大箱子、冰箱、迷你冰箱和朱尼莫箱中的物品，并直接送礼。该功能可能影响游戏平衡性。 |
| 生日提示 | NPC 生日当天会在弹框中显示生日加成提示。 |

### 送礼界面

- 候选礼物以物品格子展示，而不是文本列表
- 鼠标悬停物品时显示原版物品描述
- 鼠标悬停物品图标时会略微放大
- 右侧显示可拖动滚动条
- 使用星露谷风格关闭按钮
- 可在背包物品和存储设备物品之间左右切换
- 自动过滤工具、武器和不可送礼物品

### 送礼记录

模组会记录每个 NPC 最近一次送出的礼物，并在建议界面中标记，避免重复送同一件礼物时没有心理预期。

---

## 工作原理

1. 你与 NPC 正常对话
2. 对话框关闭后，模组检测当前 NPC 是否还能收礼
3. 扫描玩家背包中的可送礼物品
4. 调用游戏原生礼物喜好 API 判断每个物品的喜好等级
5. 根据喜好等级、品质和价格排序
6. 弹出送礼建议界面，或在开启自动交好时直接送出最优礼物

### 喜好等级

| 游戏返回值 | 分类 |
|-----------|------|
| `0` | 最爱 |
| `2` | 喜欢 |
| `8` | 普通 |
| `4` | 不喜欢 |
| `6` | 讨厌 |

模组只负责展示和选择，不维护自定义礼物数据库。因此它会跟随游戏本体和其他正确接入原生礼物逻辑的内容变化。

---

## 安装

### 前置要求

- [Stardew Valley 1.6+](https://www.stardewvalley.net/)
- [SMAPI 4.0+](https://smapi.io/)
- [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098)（可选，用于游戏内设置）

### 步骤

1. 安装 SMAPI
2. 下载 `FriendshipAssistant` 最新版本
3. 解压到 `StardewValley/Mods/` 文件夹
4. 通过 SMAPI 启动游戏

```text
StardewValley/
└── Mods/
    └── FriendshipAssistant/
        ├── FriendshipAssistant.dll
        ├── manifest.json
        └── i18n/
            ├── default.json
            └── zh.json
```

---

## 使用方法

### 手动选择礼物

1. 与 NPC 对话
2. 关闭对话后等待送礼建议弹框
3. 查看按喜好分类的物品
4. 点击想送出的物品

启用“显示存储设备物品”后，可以使用建议界面的左右按钮切换到存储设备页面，从支持的存储设备中直接选择并赠送物品。

### 自动交好

开启自动交好后，模组会在对话结束后自动选择当前最优礼物并送出，不再弹出选择界面。送出后左下角会显示带物品图标的 HUD 提示。

自动选择优先级：

1. 喜好等级更高
2. 品质更高
3. 售价更低

---

## 配置

### 通过 GMCM

游戏内菜单 -> **模组选项** -> **FriendshipAssistant**

| 设置 | 默认值 | 说明 |
|------|--------|------|
| 自动交好 | 关闭 | 开启后，对话结束自动送出最优礼物，不弹出选择界面。 |
| 显示存储设备物品 | 关闭 | 开启后显示支持的存储设备中的礼物，并允许直接送出。此功能可能影响游戏平衡性。 |

### 直接编辑配置文件

编辑 `Mods/FriendshipAssistant/config.json`：

```json
{
  "AutoGift": false,
  "ShowStorageItems": false
}
```

---

## 兼容性

- 需要 Stardew Valley 1.6+ 和 SMAPI 4.0+
- GMCM 是可选依赖；未安装时模组仍可运行，只是不能在游戏内修改设置
- 礼物喜好判断来自游戏原生 API，通常能兼容修改原生礼物喜好的内容包或模组
- 如果其他模组完全替换 NPC 对话流程，可能影响对话结束后的触发时机

---

## 构建

项目目标框架为 .NET 6，并需要本地 Stardew Valley + SMAPI 安装。macOS 默认路径：

```text
$HOME/Library/Application Support/Steam/steamapps/common/Stardew Valley/Contents/MacOS
```

构建 Release：

```bash
dotnet build FriendshipAssistant.csproj -c Release
```

如果游戏路径不同：

```bash
dotnet build FriendshipAssistant.csproj -c Release -p:GamePath="/path/to/Stardew Valley/Contents/MacOS"
```

运行测试：

```bash
dotnet test FriendshipAssistant.Tests.csproj
```
