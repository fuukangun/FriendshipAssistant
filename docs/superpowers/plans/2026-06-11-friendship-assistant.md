# FriendshipAssistant Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a SMAPI mod for Stardew Valley 1.6 that detects NPC giftable items after dialogue, shows a native-style gift picker, and optionally auto-gifts the best item.

**Architecture:** Keep all gift-selection rules in small, testable service classes and keep SMAPI/Harmony code thin. Use a cached dialogue context to avoid relying on unstable state during menu teardown, and persist only the last-gift history that needs to survive save/load.

**Tech Stack:** C# / .NET 6, SMAPI 4.x, Harmony 2.x, Generic Mod Config Menu (optional), xUnit for unit tests.

---

### Task 1: Scaffold the mod and test projects

**Files:**
- Create: `FriendshipAssistant.csproj`
- Create: `FriendshipAssistant.Tests.csproj`
- Create: `manifest.json`
- Create: `Program.cs` or `ModEntry.cs`
- Create: `Config/ModConfig.cs`
- Create: `Services/` and `Helpers/` folders

- [ ] **Step 1: Write the failing test**

Create a tiny pure-logic test project and one red test for a helper that has not been implemented yet, such as `BirthdayHelper.IsFestivalOrBirthday(...)`.

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test FriendshipAssistant.Tests.csproj -v minimal`

Expected: fail because the target helper or assembly does not exist yet.

- [ ] **Step 3: Write minimal implementation**

Add the project skeleton, reference SMAPI/Harmony, and implement only enough code for the test project and mod entry to compile.

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test FriendshipAssistant.Tests.csproj -v minimal`

Expected: pass.

- [ ] **Step 5: Commit**

```bash
git add .
git commit -m "chore: scaffold friendship assistant mod"
```

### Task 2: Build gift analysis and selection logic

**Files:**
- Create: `Services/GiftDetector.cs`
- Create: `Services/GiftSelectionService.cs`
- Create: `Services/GiftTasteCache.cs`
- Create: `Models/GiftAnalysisResult.cs`
- Create: `Models/GiftCandidate.cs`
- Modify: `FriendshipAssistant.Tests.csproj`
- Create: `Tests/GiftDetectorTests.cs`
- Create: `Tests/GiftSelectionServiceTests.cs`

- [ ] **Step 1: Write the failing test**

Cover three behaviors first: items are grouped by taste, candidates are ordered by taste then quality, and auto-gift excludes hated/disliked items.

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test FriendshipAssistant.Tests.csproj -v minimal --filter FullyQualifiedName~GiftDetector`

Expected: fail because the service logic is not implemented yet.

- [ ] **Step 3: Write minimal implementation**

Implement the detector against `NPC.getGiftTasteForThisItem(Item)` and keep the selection rules isolated from SMAPI UI code.

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test FriendshipAssistant.Tests.csproj -v minimal`

Expected: pass.

- [ ] **Step 5: Commit**

```bash
git add Services Models Tests FriendshipAssistant.Tests.csproj
git commit -m "feat: add gift analysis and selection logic"
```

### Task 3: Add gift history persistence and date helpers

**Files:**
- Create: `Services/GiftHistoryService.cs`
- Create: `Helpers/BirthdayHelper.cs`
- Create: `Models/GiftHistoryData.cs`
- Modify: `ModEntry.cs`
- Create: `Tests/GiftHistoryServiceTests.cs`
- Create: `Tests/BirthdayHelperTests.cs`

- [ ] **Step 1: Write the failing test**

Test that last-gift entries can be saved and loaded, and that birthday/festival detection returns the expected booleans for simple cases.

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test FriendshipAssistant.Tests.csproj -v minimal --filter FullyQualifiedName~GiftHistory`

Expected: fail because persistence is not wired yet.

- [ ] **Step 3: Write minimal implementation**

Use SMAPI save data for persistence and keep the helper methods side-effect free.

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test FriendshipAssistant.Tests.csproj -v minimal`

Expected: pass.

- [ ] **Step 5: Commit**

```bash
git add Services Helpers Models Tests ModEntry.cs
git commit -m "feat: persist last gift history"
```

### Task 4: Wire SMAPI events and dialogue tracking

**Files:**
- Modify: `ModEntry.cs`
- Create: `Patches/` or `Listeners/DialogueContextTracker.cs`
- Create: `Tests/DialogueContextTrackerTests.cs`

- [ ] **Step 1: Write the failing test**

Write a test for the dialogue context tracker so it caches the active speaker and clears it on menu transitions.

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test FriendshipAssistant.Tests.csproj -v minimal --filter FullyQualifiedName~DialogueContextTracker`

Expected: fail because the tracker does not exist yet.

- [ ] **Step 3: Write minimal implementation**

Track the current speaker from menu events and only trigger once per dialogue lifecycle.

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test FriendshipAssistant.Tests.csproj -v minimal`

Expected: pass.

- [ ] **Step 5: Commit**

```bash
git add ModEntry.cs Patches Tests
git commit -m "feat: track dialogue context for gift prompts"
```

### Task 5: Implement the gift picker menu

**Files:**
- Create: `UI/GiftSuggestionMenu.cs`
- Create: `UI/GiftSuggestionMenu.draw.cs` if splitting helps keep the file small
- Modify: `FriendshipAssistant.csproj`
- Create: `Tests/GiftSuggestionMenuLayoutTests.cs` if layout math is extracted into a pure helper

- [ ] **Step 1: Write the failing test**

Move layout math into a helper and test scroll bounds, row visibility, and item hit detection.

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test FriendshipAssistant.Tests.csproj -v minimal --filter FullyQualifiedName~Layout`

Expected: fail because the helper does not exist yet.

- [ ] **Step 3: Write minimal implementation**

Implement a native-style `IClickableMenu` with click selection, ESC close, tooltips, and scroll support.

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test FriendshipAssistant.Tests.csproj -v minimal`

Expected: pass.

- [ ] **Step 5: Commit**

```bash
git add UI Tests FriendshipAssistant.csproj
git commit -m "feat: add gift suggestion menu"
```

### Task 6: Integrate auto-gift, GMCM, and translations

**Files:**
- Modify: `ModEntry.cs`
- Modify: `Config/ModConfig.cs`
- Create: `Services/GiftGiver.cs`
- Create: `i18n/default.json`
- Create: `i18n/zh.json`
- Create: `Tests/GiftGiverTests.cs`

- [ ] **Step 1: Write the failing test**

Test that auto-gift chooses the best eligible item and that config defaults to manual mode.

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test FriendshipAssistant.Tests.csproj -v minimal --filter FullyQualifiedName~GiftGiver`

Expected: fail because the giver is not wired yet.

- [ ] **Step 3: Write minimal implementation**

Hook GMCM when present, use `Helper.Translation` for player-facing text, and keep auto-gift to one item per dialogue.

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test FriendshipAssistant.Tests.csproj -v minimal`

Expected: pass.

- [ ] **Step 5: Commit**

```bash
git add Services Config i18n ModEntry.cs Tests
git commit -m "feat: wire auto gift and localization"
```

### Task 7: Verify the mod builds cleanly

**Files:**
- Modify: any files required by compiler errors only

- [ ] **Step 1: Run the full test suite**

Run: `dotnet test FriendshipAssistant.Tests.csproj -v minimal`

Expected: all tests pass.

- [ ] **Step 2: Build the mod**

Run: `dotnet build FriendshipAssistant.csproj -c Release`

Expected: build succeeds with no compiler errors.

- [ ] **Step 3: Manual smoke check**

Load the mod in a Stardew Valley 1.6 + SMAPI 4.x test install and verify:
dialogue closes, prompt appears only once per dialogue, manual selection gifts the chosen item, auto-gift skips hated/disliked items, and save/load preserves last-gift markers.

- [ ] **Step 4: Commit**

```bash
git add .
git commit -m "feat: complete friendship assistant implementation"
```
