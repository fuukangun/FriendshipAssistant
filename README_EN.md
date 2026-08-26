# FriendshipAssistant

[![Stardew Valley](https://img.shields.io/badge/Stardew%20Valley-1.6%2B-brightgreen)](https://www.stardewvalley.net/)
[![SMAPI](https://img.shields.io/badge/SMAPI-4.0%2B-blue)](https://smapi.io/)

[中文](README.md) | English

**FriendshipAssistant** is a Stardew Valley gift assistant mod. After you finish talking to an NPC, it checks your inventory against the game's native gift taste logic and opens an inventory-style gift suggestion menu.

---

## Contents

- [Features](#features)
- [How It Works](#how-it-works)
- [Installation](#installation)
- [Usage](#usage)
- [Gamepad Controls](#gamepad-controls)
- [Configuration](#configuration)
- [Compatibility](#compatibility)
- [Build](#build)

---

## Features

### Core Features

| Feature | Description |
|---------|-------------|
| Post-dialogue suggestions | Opens a gift suggestion menu after NPC dialogue ends. |
| Native gift taste logic | Uses Stardew Valley's `NPC.getGiftTasteForThisItem` result. |
| Grouped display | Groups items by Loved, Liked, Neutral, Disliked, and Hated. |
| Inventory-style UI | Shows item icons in a grid; click an item to gift it. |
| Auto Gift | Can automatically give the best available gift after dialogue. |
| Storage gifts | Optionally shows items from chests, fridges, mini-fridges, and Junimo Chests, and allows gifting them directly. This may affect game balance. |
| Birthday reminder | Shows a birthday bonus banner when the NPC has a birthday today. |

### Gamepad Support in 1.1.0

- Use the left stick and D-pad to select gifts in the gift grid
- Move the right stick left or right to select gifts and move the mouse pointer to the selected gift center
- Move the right stick up or down to scroll the gift grid without moving the mouse pointer
- When the menu opens or you switch between backpack and storage, the first gift on the current page is selected and the mouse pointer is centered on it
- Fixed focus getting stuck at the four corners of the gift grid

### Gift Menu

- Shows gift candidates as item slots, not text rows
- Uses the original item description in hover tooltips
- Slightly enlarges item icons on hover
- Includes a visible draggable scrollbar
- Uses a Stardew-style close button
- Supports switching between backpack and storage items with left/right controls
- Filters out tools, weapons, and non-giftable items

### Gift History

The mod records the most recent gift given to each NPC and marks it in the suggestion menu, so repeated gifts are easier to notice.

---

## How It Works

1. Talk to an NPC as usual
2. When the dialogue box closes, the mod checks whether that NPC can receive a gift
3. It scans your inventory for giftable items
4. It calls the game's native gift taste API for each item
5. It sorts candidates by taste, quality, and price
6. It opens the suggestion menu, or gifts automatically if Auto Gift is enabled

### Gift Taste Mapping

| Game return value | Category |
|-------------------|----------|
| `0` | Loved |
| `2` | Liked |
| `8` | Neutral |
| `4` | Disliked |
| `6` | Hated |

The mod doesn't maintain a custom gift database. It relies on the game, so it should follow vanilla data and other mods that correctly integrate with the native gift logic.

---

## Installation

### Requirements

- [Stardew Valley 1.6+](https://www.stardewvalley.net/)
- [SMAPI 4.0+](https://smapi.io/)
- [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) (optional, for in-game settings)

### Steps

1. Install SMAPI
2. Download the latest `FriendshipAssistant` release
3. Extract it into your `StardewValley/Mods/` folder
4. Launch the game through SMAPI

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

## Usage

### Manual Gift Selection

1. Talk to an NPC
2. Wait for the gift suggestion menu after dialogue closes
3. Browse items grouped by gift taste
4. Click an item to give it

When **Show Storage Items** is enabled, use the left/right controls in the suggestion menu to switch to the storage page and gift directly from supported storage devices.

### Gamepad Controls

| Button | Action |
|--------|--------|
| Left stick / D-pad | Select a gift in the gift grid |
| Right stick left/right | Select gifts horizontally and synchronize the mouse pointer |
| Right stick up/down | Scroll the gift grid without moving the mouse pointer |
| A | Select and give the focused gift |
| B | Close the gift suggestion menu |
| X | Record “do not show today” and close the menu |
| LB / RB | Switch between backpack and storage pages |
| LT / RT | Scroll the gift list by one visible area |

Gamepad input uses the game's logical button semantics. Xbox, PlayStation, and Switch controllers use the same behavior, and the mod does not override in-game custom mappings.

### Auto Gift

When Auto Gift is enabled, the mod automatically gives the best available gift after dialogue and skips the menu. A HUD message with the item icon appears in the lower-left corner.

Auto selection priority:

1. Better gift taste
2. Higher item quality
3. Lower sell price

---

## Configuration

### Through GMCM

In game menu -> **Mod Options** -> **FriendshipAssistant**

| Setting | Default | Description |
|---------|---------|-------------|
| Auto Gift | Off | Automatically selects and gives the best gift from the player's backpack after dialogue without opening the menu. It never takes items from storage. |
| Show Storage Items | Off | Shows giftable items from supported storage devices and allows gifting them directly. This may affect game balance. |

### Edit Config File

Edit `Mods/FriendshipAssistant/config.json`:

```json
{
  "AutoGift": false,
  "ShowStorageItems": false
}
```

---

## Compatibility

- Requires Stardew Valley 1.6+ and SMAPI 4.0+
- GMCM is optional; without it, the mod still works but settings can't be changed in-game
- Gift taste logic comes from the native game API, so it should work with content that modifies gift tastes through the game's data
- Mods that fully replace NPC dialogue flow may affect when the post-dialogue trigger runs

---

## Build

The project targets .NET 6 and expects a local Stardew Valley + SMAPI install. On macOS it defaults to:

```text
$HOME/Library/Application Support/Steam/steamapps/common/Stardew Valley/Contents/MacOS
```

Build Release:

```bash
dotnet build FriendshipAssistant.csproj -c Release
```

Override the game path if needed:

```bash
dotnet build FriendshipAssistant.csproj -c Release -p:GamePath="/path/to/Stardew Valley/Contents/MacOS"
```

Run unit tests:

```bash
dotnet test FriendshipAssistant.Tests.csproj
```
