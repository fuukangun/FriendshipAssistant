# Gift Prompt Storage and Snooze Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [x]`) syntax for tracking.

**Goal:** Add per-NPC daily prompt suppression and an opt-in storage-device gift page that can send gifts from supported containers.

**Architecture:** Extend the existing save-backed `GiftHistoryService` with daily suppression records. Keep storage enumeration and source-aware item consumption in a dedicated service, while the controller combines backpack and storage candidates and the menu renders separate source pages. UI selections carry the actual source item/container so duplicate item IDs cannot cause an incorrect inventory deduction.

**Tech Stack:** C# / .NET 6, Stardew Valley 1.6 APIs, SMAPI 4.x, MonoGame UI, xUnit.

## Global Constraints

- `ShowStorageItems` defaults to `false`.
- Supported storage devices are `Chest`, `Stone Chest`, `Big Chest`, `Stone Big Chest`, `Fridge`, `Mini-Fridge`, and `Junimo Chest`.
- Dressers, auto-grabbers, shipping bins, trash cans, and unknown mod containers are excluded.
- Storage scanning is opt-in and has no distance restriction once enabled.
- Storage UI text must state that the feature may affect game balance.
- Ordinary close remains different from “today do not remind me”.
- Existing auto-gift behavior remains unchanged.

---

### Task 1: Add configuration, localization, and daily suppression state

**Files:**
- Modify: `Config/ModConfig.cs`
- Modify: `Models/GiftHistoryData.cs`
- Modify: `Services/GiftHistoryService.cs`
- Modify: `ModEntry.cs`
- Modify: `i18n/default.json`
- Modify: `i18n/zh.json`
- Test: `Tests/GiftHistoryServiceTests.cs`

**Interfaces:**
- Produce `ModConfig.ShowStorageItems: bool` with default `false`.
- Produce `GiftHistoryService.IsPromptSuppressed(string npcName, string season, int day): bool`.
- Produce `GiftHistoryService.SuppressPrompt(string npcName, string season, int day): void`.
- Existing save import/export remains the single persistence path.

- [x] **Step 1: Add failing suppression tests**

Add tests for same-day matches, date/season expiry, NPC isolation, duplicate overwrite, and null import.

- [x] **Step 2: Run the focused tests and confirm failure**

Run `dotnet test FriendshipAssistant.Tests.csproj --no-restore --filter FullyQualifiedName~GiftHistoryServiceTests`.
Expected: compilation failure because suppression APIs and model fields do not exist.

- [x] **Step 3: Implement the state and configuration**

Add a serializable dictionary of NPC names to a `PromptSuppressionEntry` containing `Season` and `Day`. Validate NPC names and seasons consistently with existing history methods. Register `ShowStorageItems` with `gmcm.AddBoolOption` and add localized name/tooltip plus `ui.dismissToday`.

- [x] **Step 4: Wire save lifecycle compatibility**

Keep existing `gift-history` save data key. Ensure missing new JSON properties deserialize to empty dictionaries and `OnReturnedToTitle` clears imported state through the existing `Import(null)` path.

- [x] **Step 5: Run focused tests**

Run the focused test command again and expect all `GiftHistoryServiceTests` to pass.

---

### Task 2: Add source-aware gift candidates and storage enumeration

**Files:**
- Create: `Models/GiftItemSource.cs`
- Create: `Models/StorageGiftItem.cs`
- Create: `Services/StorageItemService.cs`
- Modify: `Services/StardewGiftCandidateFactory.cs`
- Modify: `Models/GiftMenuRow.cs`
- Modify: `Models/GiftMenuSlot.cs`
- Modify: `UI/GiftMenuModel.cs`
- Modify: `UI/GiftGroupedGridLayout.cs`
- Test: `Tests/StardewGiftCandidateFactoryTests.cs`
- Test: `Tests/StorageItemServiceTests.cs`

**Interfaces:**
- `GiftItemSource` identifies `Backpack` or `Storage`.
- `StorageGiftItem` carries a `GiftCandidate`, its live `Item`, and a source container reference.
- `StorageItemService.GetGiftItems(Farmer farmer, NPC npc): IReadOnlyList<StorageGiftItem>` returns only supported storage candidates.
- Menu rows/slots preserve a source-aware `GiftMenuItem` rather than only `GiftCandidate`.

- [x] **Step 1: Define pure source models and red tests**

Add tests for supported type-name classification, blocked type names, and source-aware row hit testing. Use small fake item-container abstractions in tests where possible; do not require a running game UI.

- [x] **Step 2: Implement supported-container discovery**

Create a service that scans player save locations and supported container objects, including farm, farmhouse, sheds, cabins, and other loaded game locations. Treat `JunimoChest` as one shared logical source. Read only supported container item lists and skip unavailable or incompatible objects.

- [x] **Step 3: Create storage candidates through existing gift rules**

Reuse `StardewGiftCandidateFactory.Create` so storage items receive the same `canBeGivenAsGift`, tool filtering, native taste mapping, quality, and sale-price behavior as backpack items.

- [x] **Step 4: Carry source identity through menu models and layout**

Update menu row/slot models and `GiftGroupedGridLayout.HitTest` to return the selected source-aware entry. Keep existing candidate-only tests passing through a backpack-source adapter where needed.

- [x] **Step 5: Run focused tests**

Run `dotnet test FriendshipAssistant.Tests.csproj --no-restore --filter FullyQualifiedName~StorageItemServiceTests|FullyQualifiedName~StardewGiftCandidateFactoryTests|FullyQualifiedName~GiftGroupedGridLayoutTests`.
Expected: all focused storage, factory, and layout tests pass.

---

### Task 3: Make gift delivery consume the selected source safely

**Files:**
- Modify: `Services/GiftGiver.cs`
- Modify: `Services/GiftInventoryConsumption.cs`
- Create or modify: `Services/StorageItemService.cs`
- Modify: `Services/GiftPromptController.cs`
- Test: `Tests/GiftInventoryConsumptionTests.cs`
- Test: `Tests/StorageItemServiceTests.cs`

**Interfaces:**
- Backpack and storage delivery share one source-aware delivery callback.
- Delivery validates the selected source item is still present and has a positive stack before calling `NPC.receiveGift`.
- Delivery returns `false` when a stale storage reference cannot be safely consumed.

- [x] **Step 1: Add red consumption tests**

Test decrementing a storage list item, removing a one-item stack, and rejecting a stale or missing source without touching another source with the same item ID.

- [x] **Step 2: Implement source-specific consumption**

Generalize the existing stack decrement helper to operate on an `IList<Item?>` plus the exact selected `Item` reference. Preserve current farmer inventory behavior and add container-list behavior without falling back to item-ID lookup.

- [x] **Step 3: Update `GiftGiver` delivery**

Accept a source-aware item and consume from the matching list after `NPC.receiveGift`. Record the same last-gift history regardless of source. Keep the existing public path for backpack callers or adapt it through a backpack source.

- [x] **Step 4: Run focused delivery tests**

Run `dotnet test FriendshipAssistant.Tests.csproj --no-restore --filter FullyQualifiedName~GiftInventoryConsumptionTests|FullyQualifiedName~StorageItemServiceTests`.
Expected: all focused delivery tests pass.

---

### Task 4: Integrate suppression and storage candidates in the controller

**Files:**
- Modify: `Services/GiftPromptController.cs`
- Modify: `ModEntry.cs`
- Test: `Tests/GiftPromptControllerTests.cs` or the smallest existing controller-test seam

**Interfaces:**
- `TryPrompt` first returns without opening a menu when the current NPC/date is suppressed.
- With `ShowStorageItems=false`, candidate analysis and UI remain backpack-only.
- With `ShowStorageItems=true`, the controller builds separate backpack/storage page models and passes source-aware callbacks.

- [x] **Step 1: Add controller behavior tests or seams**

Cover suppression short-circuit, storage setting off, and storage setting on. Keep game-global calls behind existing constructor delegates or a narrow seam so tests remain deterministic.

- [x] **Step 2: Implement suppression short-circuit**

Check `GiftHistoryService.IsPromptSuppressed` after eligibility and before candidate analysis. The callback passed to the menu calls `SuppressPrompt` with the current NPC and game date.

- [x] **Step 3: Build separate source pages**

Always build the backpack page. Build the storage page only when enabled, and pass the storage source items to the menu. Do not let storage candidates change auto-gift selection unless the approved behavior explicitly requires it; preserve auto-gift as backpack-only.

- [x] **Step 4: Run controller-focused tests**

Run `dotnet test FriendshipAssistant.Tests.csproj --no-restore --filter FullyQualifiedName~GiftPromptControllerTests` and expect all tests to pass.

---

### Task 5: Implement the two-page suggestion menu and verification

**Files:**
- Modify: `UI/GiftSuggestionMenu.cs`
- Create or modify: `UI/GiftMenuChrome.cs`
- Modify: `UI/GiftScrollBarLayout.cs`
- Modify: `i18n/default.json`
- Modify: `i18n/zh.json`
- Test: `Tests/GiftMenuChromeTests.cs`
- Test: `Tests/GiftScrollBarLayoutTests.cs`

**Interfaces:**
- Menu pages are `Backpack` and `Storage`; storage page exists only when enabled.
- Left/right controls switch pages and preserve independent scroll offsets.
- Selecting a slot invokes the source-aware delivery callback; “today do not remind me” invokes its callback and exits.

- [x] **Step 1: Add layout tests**

Test tab/control rectangles, bottom dismissal button placement, and non-overlap with close and scrollbar regions. Test page switching and independent scroll clamping through pure layout helpers.

- [x] **Step 2: Implement page state and controls**

Add a stable page enum, page labels, arrow bounds, and per-page scroll offsets. Keep the existing close button and item grid geometry stable where possible; reserve a bottom action area for dismissal text.

- [x] **Step 3: Implement rendering and interactions**

Draw localized page labels, arrow buttons, the localized balance-warning storage setting only in GMCM (not as a repeated in-game warning), the empty state, and the “今日不再提醒” action. Handle arrow clicks before grid hit testing and preserve normal close behavior.

- [x] **Step 4: Run the full test suite**

Run `dotnet test FriendshipAssistant.Tests.csproj --no-restore -v minimal`.
Expected: exit code 0 with zero failed tests.

- [x] **Step 5: Build the mod**

Run `dotnet build FriendshipAssistant.csproj -c Release --no-restore`.
Expected: exit code 0 and a rebuilt `bin/Release/net6.0/FriendshipAssistant.dll`.

- [x] **Step 6: Inspect the final diff and status**

Run `git diff --check`, `git status --short`, and `git diff --stat`. Confirm only the approved implementation, tests, localization, and plan files changed.

