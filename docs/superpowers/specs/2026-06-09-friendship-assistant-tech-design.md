# 友谊助手 技术方案设计

> 日期：2026-06-09 | 基于需求文档 v2

---

## 一、技术栈

| 层 | 选型 |
|----|------|
| Mod 框架 | SMAPI 4.x |
| 游戏 | Stardew Valley 1.6 |
| 目标平台 | .NET 6 (SMAPI 要求) |
| UI | 自绘 `IClickableMenu`（原版风格） |
| 事件拦截 | Harmony 2.x (SMAPI 内置) |
| 配置 | GMCM (Generic Mod Config Menu) 可选依赖 |
| 国际化 | SMAPI `Helper.Translation` + `i18n/` JSON |
| 数据持久化 | SMAPI `Helper.Data` (save-specific JSON) |

---

## 二、项目结构

```
FriendshipAssistant/
├── FriendshipAssistant.csproj
├── manifest.json
├── ModEntry.cs                    # 入口：注册事件 + Harmony Patch
├── i18n/
│   ├── default.json               # 英文（默认语言，key 命名基准）
│   └── zh.json                    # 中文
├── Config/
│   └── ModConfig.cs               # 配置模型 + GMCM 注册
├── Services/
│   ├── GiftDetector.cs            # 背包物品检测 + 五等级分类
│   ├── GiftGiver.cs               # 执行送礼（含自动送礼策略）
│   ├── GiftHistoryService.cs      # 上次礼物记录（亮点 🎁）
│   └── GiftTasteCache.cs          # NPC 礼物偏好缓存（性能优化）
├── UI/
│   └── GiftSuggestionMenu.cs      # 自定义送礼弹框（IClickableMenu 子类）
├── Patches/
│   └── DialogueEndPatch.cs        # Harmony Postfix：对话结束时触发
└── Helpers/
    └── BirthdayHelper.cs          # 生日/节日判定
```

---

## 三、核心流程

### 3.1 对话结束拦截

```
玩家与NPC对话
    ↓
DialogueBox.CloseDialogue()
    ↓
Harmony Postfix 拦截
    ↓
获取 Game1.player.currentSpeaker → NPC 实例
    ↓
判据检查（不可送礼？今日已送？本周满？）→ 不通过则 return
    ↓
GiftDetector.Analyze(player.Items, npc) → 分类结果
    ↓
  AutoGift=true  → GiftGiver.AutoGift(npc, result) → 静默送出
  AutoGift=false → GiftSuggestionMenu.Open(npc, result)  → 弹框
```

**判据检查细节：**

```
对话结束 → 检查 NPC 是否可接受礼物
            ↓ YES
         检查是否已触发过送礼弹框（本次对话生命周期标记）
            ↓ NOT YET
         检查今日是否已向此 NPC 送礼
          (Game1.player.hasGiftedToday(npc.Name) 或每日重置标记)
            ↓ NO
         检查本周是否已满 2 次
          (使用 NPC.giftsThisWeek 原版数据)
            ↓ NOT FULL
         检查背包是否有相关物品
            ↓ HAS ITEMS
         执行送礼流程 → 标记已触发 = true
```

**本次对话生命周期标记**：防止同一段对话多次关闭时重复弹框。在 `MenuChanged` 事件中，当 DialogueBox 打开时重置标记，弹框触发后设为 true。

### 3.2 物品检测与分类

```
GiftDetector.Analyze(inventory, npc)
    │
    ├─ 遍历背包每个物品
    │     │
    │     └─ npc.getGiftTasteForThisItem(item) → 获取等级
    │         (内部调用 Data\NPCGiftTastes asset)
    │
    ├─ 按等级分组：
    │     LovedItems    ← +80
    │     LikedItems    ← +45
    │     NeutralItems  ← +20
    │     DislikedItems ← -20
    │     HatedItems    ← -40
    │
    ├─ 每组内排序：Quality 降序（铱星=4 > 金星=2 > 银星=1 > 普通=0）
    │
    └─ 返回 GiftAnalysisResult
```

**性能优化**：`GiftTasteCache` 缓存 NPC 的礼物偏好表，避免每次对话都重新解析 `Data/NPCGiftTastes`。缓存 key 为 NPC 的内部名，在进入新的一天时清空（因为理论上偏好表不会变，但为安全起见每天刷新）。

### 3.3 送礼执行

```
GiftGiver.GiveGift(npc, item, player)
    │
    ├─ 调用原版送礼接口
    │     npc.receiveGift(item)
    │     或 player.friendshipData[name].Update(giftTaste)
    │
    ├─ 记录到 GiftHistoryService
    │     { npcName → { itemId, itemDisplayName, date } }
    │
    └─ 触发原版 NPC 反应动画 + 对话
```

### 3.4 自动送礼策略

```
GiftGiver.AutoGift(npc, result)
    │
    ├─ 候选池：LovedItems + LikedItems + NeutralItems（三组合并）
    │
    ├─ 排序 key：
    │     primary:   友谊加成分数（最爱=3 > 喜欢=2 > 普通=1）
    │     secondary: 品质（铱星=4 > 金星=2 > 银星=1 > 普通=0）
    │     tertiary:  售价（买贵的送，玩家更心疼）
    │
    ├─ 取出第一个 → GiveGift()
    │
    └─ 不弹窗，仅在聊天栏输出一行静默提示（可选）
         Helper.Translation.Get("auto.message", new { itemName, npcName })
```

---

## 四、UI 设计 — GiftSuggestionMenu

### 4.1 设计原则

- 风格**完全贴近星露谷原版**：木质背景、手绘感边框、像素字体
- 使用游戏内已有素材，不自带额外贴图资源
- 交互方式符合原版玩家直觉（点击选中、ESC 关闭、滚轮滚动）

### 4.2 布局结构

```
┌──────────────────────────────────────────┐
│  🎂 今天是Abigail的生日！送礼×8加成！     │  ← 生日横幅（若有）
├──────────────────────────────────────────┤
│                                          │
│  ———— ⭐ 最爱 —————————————————————       │  ← 紫色/粉红分组标题
│                                          │
│  ┌────┐                                  │
│  │ 🎃 │ ⭐ 紫水晶  [铱星★]  上次送过 ←    │  ← 物品行（最爱高亮 ⭐）
│  └────┘                                  │
│  ┌────┐                                  │
│  │ 🍫 │ 巧克力蛋糕  [金星★]              │  ← 可点击选中
│  └────┘                                  │
│                                          │
│  ———— 💚 喜欢 —————————————————————       │  ← 绿色分组标题
│                                          │
│  ┌────┐                                  │
│  │ 🌸 │ 黄水仙  [普通]                   │
│  └────┘                                  │
│                                          │
│  ———— 🩶 普通 —————————————————————       │  ← 灰色分组标题
│                                          │
│  ┌────┐                                  │
│  │ 🪨 │ 石头  [普通]                     │
│  └────┘                                  │
│                                          │
│  ———— 🧡 不喜欢 ——————————————————       │  ← 橙色分组标题
│                                          │
│  ┌────┐                                  │
│  │ 🐌 │ 蜗牛  [普通]                     │
│  └────┘                                  │
│                                          │
│  ———— ❤️ 讨厌 ———————————————————       │  ← 红色分组标题
│                                          │
│  ┌────┐                                  │
│  │ 🍄 │ 红蘑菇  [普通]                   │
│  └────┘                                  │
│                                          │
├──────────────────────────────────────────┤
│              [ 关  闭 ]                   │  ← 底部居中关闭按钮
└──────────────────────────────────────────┘
```

### 4.3 UI 尺寸与定位

| 参数 | 值 |
|------|-----|
| 弹框总宽 | 600px |
| 弹框最大高 | 500px（超长时启用滚动） |
| 物品行高 | 60px |
| 物品图标尺寸 | 48×48（游戏标准图标大小） |
| 弹框位置 | 屏幕水平居中、垂直居中 |
| 滚动条 | 右侧，原版样式（Game1.mouseCursors 中的滚动条素材） |
| 字体 | 物品名：`Game1.smallFont`，标题：`Game1.dialogueFont` |

### 4.4 交互行为

| 操作 | 行为 |
|------|------|
| **点击物品行** | 执行送礼 → 播放 NPC 反应 → 关闭弹框 |
| **鼠标悬浮物品行** | 高亮底色（原版浅棕色） + 显示 tooltip（物品全名+描述） |
| **点击 NPC 头像/背景** | 不响应（防止误触） |
| **按 ESC / 点关闭按钮** | 关闭弹框，不送礼。下次对话可再次触发 |
| **滚轮滚动** | 列表上下滚动（当物品超过可视区域时） |
| **按手柄 A 键** | 选中当前高亮物品送出 |

### 4.5 视觉细节

**背景：**
- 使用 `Game1.menuTexture` 木质纹理，透明度 0.95
- 边框使用 `drawTextureBox` 原版方法，保持与其他菜单一致的装饰边框

**分组标题：** 通过 `Helper.Translation.Get("category.loved")` 等 key 动态获取，各等级配色固定：

| 等级 | i18n Key | 颜色 | 
|------|----------|------|
| 最爱 | `category.loved` | 紫/粉 `Color.HotPink` |
| 喜欢 | `category.liked` | 绿 `Color.LimeGreen` |
| 普通 | `category.neutral` | 灰 `Color.Gray` |
| 不喜欢 | `category.disliked` | 橙 `Color.Orange` |
| 讨厌 | `category.hated` | 红 `Color.Red` |

**物品行：**
- 图标使用 `Item.drawInMenu()` 标准方法，尺寸 48×48
- 最爱物品：名称前加 ⭐，名称颜色金色 `Color.Gold`
- 上次送过的物品：行末标注灰色斜体 `Helper.Translation.Get("ui.lastGifted")`
- 品质星标：使用 `Game1.mouseCursors` 中的品质素材（铱紫/金/银）
- 行背景：鼠标悬浮时绘制浅棕色矩形高亮

**生日/节日横幅：**
- 位于弹框顶部，跨整个宽度
- 金色文字，背景微红（喜庆感）
- 只有在当天是该 NPC 生日或当日是送礼节日时才显示

**关闭按钮：**
- 使用 `Game1.mouseCursors` 中的原版按钮素材
- 底部居中，尺寸 64×64 的 clickable area
- 悬停时放大/高亮

### 4.6 滚动逻辑

```
当 物品总行数 × 行高 > 弹框内容高度 时：
  │
  ├─ 右侧显示滚动条
  ├─ currentScroll 追踪当前滚动偏移
  ├─ 只绘制可视区域内的物品行（裁剪优化）
  └─ 滚轮/拖动滚动条改变 currentScroll
```

### 4.7 NPC 头像（可选扩展）

弹框左上角绘制该 NPC 的小头像（32×32，从 NPC 的 Portraits 纹理中截取），让玩家一眼确认在和谁互动。

---

## 五、数据持久化

### 5.1 礼物历史

使用 SMAPI `Helper.Data.WriteSaveData<T>` / `ReadSaveData<T>`，数据存储在每个存档目录下。

```csharp
class GiftHistoryData
{
    // key: NPC internal name
    // value: 最后送出的物品信息
    public Dictionary<string, LastGiftEntry> LastGifts { get; set; }
}

class LastGiftEntry
{
    public string ItemId { get; set; }       // 物品 ID（限定的）
    public string ItemDisplayName { get; set; } // 显示名
    public int Season { get; set; }
    public int Day { get; set; }
}
```

**用途**：在弹框中判断某个物品是否与上次相同，标记 "上次已送过"。

### 5.2 每日/每周状态

不单独持久化，直接读取原版数据：
- 今日已送：`Game1.player.hasGiftedToday(npcName)`
- 本周已送次数：原版 `Friendship.GiftsThisWeek` 字段

---

## 六、GMCM 集成

### 6.1 注册

```csharp
// 在 GameLaunched 事件中注册
var gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
if (gmcm != null)
{
    gmcm.Register(ModManifest, ...);
    gmcm.AddBoolOption(
        ModManifest,
        getValue: () => Config.AutoGift,
        setValue: v => Config.AutoGift = v,
        name: () => Helper.Translation.Get("gmcm.autoGift.name"),
        tooltip: () => Helper.Translation.Get("gmcm.autoGift.tooltip")
    );
}
```

### 6.2 配置模型

```csharp
class ModConfig
{
    public bool AutoGift { get; set; } = false;
}
```

单一开关，GMCM 中一行搞定。

---

## 六-EXT、国际化 (i18n)

### 6-EXT.1 实现方式

使用 SMAPI 内置的 `Helper.Translation` API，通过 `i18n/` 目录下的 JSON 文件提供多语言支持。

- **默认语言**：英文 (`default.json`)
- **追加语言**：中文 (`zh.json`)
- **获取文本**：`Helper.Translation.Get("key")` 或带参数的 `Helper.Translation.Get("key", new { arg })`
- **语言检测**：SMAPI 自动根据游戏语言设置匹配 JSON，无需手动判断

### 6-EXT.2 翻译文件

**`i18n/default.json`** (英文基准)：

```json
{
  "ui.title": "Gift Suggestion",
  "ui.close": "Close",
  "ui.lastGifted": "Last gifted",

  "category.loved": "Loved",
  "category.liked": "Liked",
  "category.neutral": "Neutral",
  "category.disliked": "Disliked",
  "category.hated": "Hated",

  "birthday.banner": "🎂 Today is {{npcName}}'s birthday! Gift friendship ×8!",
  "festival.banner": "🎉 It's a festival today! Don't forget to gift!",

  "auto.message": "[FriendshipAssistant] Automatically gifted {{itemName}} to {{npcName}}.",

  "quality.iridium": "Iridium",
  "quality.gold": "Gold",
  "quality.silver": "Silver",
  "quality.normal": "Normal",

  "gmcm.autoGift.name": "Auto Gift",
  "gmcm.autoGift.tooltip": "When enabled, automatically gifts the best item (Loved > Liked > Neutral) after dialogue without showing the menu."
}
```

**`i18n/zh.json`** (中文)：

```json
{
  "ui.title": "送礼建议",
  "ui.close": "关闭",
  "ui.lastGifted": "上次送过",

  "category.loved": "最爱",
  "category.liked": "喜欢",
  "category.neutral": "普通",
  "category.disliked": "不喜欢",
  "category.hated": "讨厌",

  "birthday.banner": "🎂 今天是 {{npcName}} 的生日！送礼友谊加成 ×8！",
  "festival.banner": "🎉 今天是节日！别忘了送礼！",

  "auto.message": "[友谊助手] 已自动赠送 {{itemName}} 给 {{npcName}}。",

  "quality.iridium": "铱星",
  "quality.gold": "金星",
  "quality.silver": "银星",
  "quality.normal": "普通",

  "gmcm.autoGift.name": "自动交好",
  "gmcm.autoGift.tooltip": "开启后，对话结束自动送出最优物品（最爱>喜欢>普通），不弹窗。"
}
```

### 6-EXT.3 翻译 Key 分类

| Key 前缀 | 用途 | 使用位置 |
|-----------|------|----------|
| `ui.*` | 弹窗 UI 文本 | GiftSuggestionMenu |
| `category.*` | 五等级分组标题 | GiftSuggestionMenu 分组渲染 |
| `birthday.*` / `festival.*` | 特殊日期横幅 | GiftSuggestionMenu 顶部 |
| `auto.*` | 自动送礼聊天提示 | GiftGiver |
| `quality.*` | 品质名称 | GiftSuggestionMenu 物品行 |
| `gmcm.*` | GMCM 配置项标签 | ModConfig |

### 6-EXT.4 代码使用示例

```csharp
// 简单文本
string closeText = Helper.Translation.Get("ui.close");

// 带参数模板 (SMAPI 使用 {{param}} 语法)
string banner = Helper.Translation.Get("birthday.banner", new { npcName = npc.displayName });
// 英文 → "🎂 Today is Abigail's birthday! Gift friendship ×8!"
// 中文 → "🎂 今天是 阿比盖尔 的生日！送礼友谊加成 ×8！"
```

### 6-EXT.5 使用原则

1. **所有面向玩家的文本**（UI文字、聊天消息、按钮、tooltip）**必须**通过 `Helper.Translation` 获取
2. **日志和调试输出**可使用硬编码英文（不面向玩家）
3. **Key 命名**统一 `dot.case` 小写，按功能域前缀分组
4. 缺失的 key 自动 fallback 到 `default.json`

---

## 七、关键 API 依赖

| 需求 | API | 备注 |
|------|-----|------|
| 获取 NPC 对物品的好感等级 | `NPC.getGiftTasteForThisItem(Item)` | 返回 int → `GiftTaste` 枚举转换 |
| 判断 NPC 是否可送礼 | `NPC.CanSocialize` 或检查 NPC data 排除特殊角色 | 排除宠物、婴儿、特定商店 NPC |
| 执行送礼 | `NPC.receiveGift(Item)` 或手动操作用 `Farmer.currentLocation` 方法 | 优先调用原版方法以触发反应动画 |
| 检查今日是否已送 | `Game1.player.hasGiftedToday(npcName)` | SMAPI 提供的方法 |
| 检查本周送礼次数 | `Game1.player.friendshipData[npcName].GiftsThisWeek` | ≥ 2 则跳过 |
| 判断生日 | `NPC.isBirthday(season, day)` | 返回 bool |
| 判断节日 | 检查 `Game1.isFestival()` 或 `Game1.CurrentEvent` | 非 null → 在节日中 |
| 获取 NPC 头像 | NPC 的 Portrait 纹理 | UI 左上角小头像用 |
| 物品品质星 | `Game1.mouseCursors` 中的品质素材位置 | 绘制品质星标 |
| 获取 NPC 礼物偏好 | `Data/NPCGiftTastes` game asset | 缓存优化 |

---

## 八、事件与生命周期

| 生命周期 | 处理 |
|----------|------|
| **Mod.Entry()** | 读取配置、注册 Harmony Patch、注册事件 |
| **GameLaunched** | 注册 GMCM（如果已安装） |
| **DayStarted** | 清空 GiftTasteCache、重置每日标记 |
| **SaveLoaded** | 加载 GiftHistoryData |
| **Saving** | 保存 GiftHistoryData |
| **ReturnToTitle** | 清空运行时缓存 |
| **对话开始 (DialogueBox opened)** | 重置 "本次对话已触发弹框" 标记 |
| **对话结束 (DialogueBox closed)** | 拦截 → 检测 → 弹框/自动送礼 |

---

## 九、模块职责速查

| 模块 | 职责 | 依赖 |
|------|------|------|
| `ModEntry` | 入口，生命周期管理，事件注册 | 所有模块 |
| `ModConfig` | 配置数据类 + GMCM UI 注册逻辑 | GMCM API（可选） |
| `GiftDetector` | 遍历背包 → 五等级分类 → 排序 | NPC.getGiftTasteForThisItem |
| `GiftGiver` | 执行送礼动作，自动送礼策略 | GiftHistoryService |
| `GiftHistoryService` | 上次送了什么、存档读写 | Helper.Data |
| `GiftTasteCache` | NPC 偏好缓存 | Data/NPCGiftTastes asset |
| `GiftSuggestionMenu` | 自绘 UI，交互处理 | 所有游戏 UI 素材 |
| `DialogueEndPatch` | Harmony Postfix on DialogueBox.CloseDialogue | GiftDetector, GiftGiver, GiftSuggestionMenu |
| `BirthdayHelper` | 生日/节日判定 | NPC 属性 |

---

## 十、风险与应对

| 风险 | 影响 | 应对 |
|------|------|------|
| `getGiftTasteForThisItem` 在特定条件下返回异常值 | 分类错误 | 对未知返回值做 fallback，归入 "普通" |
| 其他 mod 也 patch 了 DialogueBox.CloseDialogue | 冲突 | 使用 Harmony `Priority.Low` + 非破坏性 Postfix |
| 玩家在送礼弹框中再次与 NPC 对话 | 弹框叠加 | display 前检查是否已有活动弹框 |
| NPC 不可送礼但 `currentSpeaker` 有值 | 对不可送礼 NPC 弹框 | 先执行 CanSocialize / 角色类型过滤 |
| 弹框打开时玩家传送/切场景 | 弹框残留 | `OnWarped` / `LocationChanged` 事件中关闭弹框 |

---

## 十一、后续可扩展方向（本期不实现）

- NPC 好感度预览条（送礼前显示送完后的好感度变化）
- 背包无物品时提示"去哪里可以获得 NPC 喜欢的物品"
- 手柄深度适配（手柄振动反馈）