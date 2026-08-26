# FriendshipAssistant Gamepad Support Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add complete native-style gamepad navigation to the gift suggestion menu while preserving mouse/keyboard behavior and fixing year-aware prompt suppression.

**Architecture:** Keep gift selection and inventory mutation in the existing services. Extend `GiftSuggestionMenu` with a focused, source-aware `ClickableComponent` graph whose visible grid is rebuilt when the page or scroll offset changes. Route gamepad confirmation and cancellation through the same callbacks used by mouse clicks; keep tooltip rendering separate for mouse and gamepad focus.

**Input update:** Only gifts participate in the gamepad focus chain. The left stick and D-pad use native Snappy navigation, right-stick horizontal movement synchronizes the pointer to the selected gift, right-stick vertical movement scrolls without moving the pointer, X dismisses recommendations for today, B closes, LB/RB switch pages, and LT/RT scroll by a visible page.

**Tech Stack:** C#/.NET 6, SMAPI 4.x, Stardew Valley `IClickableMenu`, `ClickableComponent`, MonoGame input, xUnit.

**Status:** Implementation and automated verification complete; in-game controller smoke test pending.

## Global Constraints

- Use Stardew Valley / MonoGame logical gamepad input; do not identify controller brands or override in-game custom mappings.
- Keep mouse clicks, wheel scrolling, scrollbar dragging, keyboard behavior, gift sorting, item consumption, history, and storage gifting behavior backward compatible.
- `AutoGift` continues to select only from the farmer backpack; `ShowStorageItems` affects manual suggestions only.
- A stale suppression record without `Year` is treated as `Year = 0` and never matches the current game date.
- Do not add controller-brand button icons or a permanent instruction panel.

### Task 1: Make prompt suppression year-aware

**Files:**
- Modify: `Models/GiftHistoryData.cs`
- Modify: `Services/GiftHistoryService.cs`
- Modify: `Services/GiftPromptController.cs`
- Modify: `ModEntry.cs`
- Test: `Tests/GiftHistoryServiceTests.cs`

**Interfaces:**
- `PromptSuppressionEntry.Year: int` defaults to `0` for old save data.
- `GiftHistoryService.IsPromptSuppressed(string npcName, GameDate date): bool` compares all four date dimensions.
- `GiftHistoryService.SuppressPrompt(string npcName, GameDate date): void` records all four date dimensions.

- [x] Add `Year` to `PromptSuppressionEntry`, leaving the default value `0` so missing JSON remains loadable.
- [x] Update `IsPromptSuppressed` and `SuppressPrompt` signatures and validation to include a positive year.
- [x] Update `GiftPromptController` call sites through its existing callbacks and current `Game1` date values (`Game1.year`, `Game1.currentSeason`, `Game1.dayOfMonth`).
- [x] Add tests for same-year match, next-year mismatch, `Year == 0` legacy mismatch, and invocation-time date capture.

### Task 2: Extract focused menu navigation state

**Files:**
- Create: `UI/GiftMenuFocusState.cs`
- Create: `Tests/GiftMenuFocusStateTests.cs`
- Modify: `UI/GiftSuggestionMenu.cs`

**Interfaces:**
- The state object tracks page, focused item identity, focused row/column, scroll offset per page, and the last focused gift for returning from the dismiss action.
- Pure navigation helpers accept the current `GiftGridDisplayRow` list and return a valid `GiftMenuSlot` or a named chrome target; they never return headers or empty slots.

- [x] Define stable gift focus IDs; close, page, and dismiss controls remain outside the gamepad focus chain.
- [x] Implement row/column navigation over the current page's real gift slots, including right/left clamping on short rows.
- [x] Implement the distinction between visible-bottom row and page-final row: down scrolls while later rows exist, and only page-final row down enters dismiss.
- [x] Implement column clamping after LT/RT page-sized scrolls and page-empty fallback to navigation chrome or dismiss.
- [x] Add pure tests for identity restoration, nearest fallback, short rows, header boundaries, visible-edge scrolling, final-row dismiss transition, and column clamping.

### Task 3: Register Stardew Snappy components and preserve mouse paths

**Files:**
- Modify: `UI/GiftSuggestionMenu.cs`
- Modify: `UI/GiftSuggestionMenu.Drawing.cs`
- Modify: `UI/GiftMenuChrome.cs`
- Test: `Tests/GiftMenuChromeTests.cs`

**Interfaces:**
- `GiftSuggestionMenu.populateClickableComponentList()` rebuilds the current page's component graph.
- `GiftSuggestionMenu.snapToDefaultClickableComponent()` selects the configured initial target.
- Component IDs and neighbor IDs are stable within a rebuilt page.

- [x] Create components for close, enabled page buttons, visible gift slots, and dismiss; exclude headers, empty slots, disabled buttons, and invalid gifts.
- [x] Keep directional neighbors inside the gift grid, use `ID_ignore` at outer edges instead of self-neighbors, and prevent focus from entering top or bottom controls.
- [x] Rebuild components after page switches, scroll changes, item invalidation, and focus restoration.
- [x] Keep `receiveLeftClick`, `receiveScrollWheelAction`, `leftClickHeld`, and `releaseLeftClick` behavior intact; synchronize focus when the mouse target changes.
- [x] Run the grid, grouped-grid, chrome, focus navigator, and focus resolver test groups.

### Task 4: Add gamepad input, page switching, and scrolling

**Files:**
- Modify: `UI/GiftSuggestionMenu.cs`
- Modify: `UI/GiftSuggestionMenu.Drawing.cs`
- Test: `Tests/GiftSuggestionMenuGamepadTests.cs`

**Interfaces:**
- Override Stardew's gamepad input hook (`receiveGamePadButton`) for logical confirm/cancel and shoulder/trigger actions.
- Reuse the existing `onGiftSelected`, `onDismissToday`, and `exitThisMenu` callbacks.

- [x] Handle A as focused gift activation with live-source validation; X dismisses recommendations for today.
- [x] Handle B as unconditional menu close on both pages.
- [x] Handle LB/RB as backpack/storage page selection only when storage is enabled.
- [x] Keep native left-stick and D-pad Snappy navigation inside the gift grid.
- [x] Handle right-stick horizontal movement as visible-grid focus navigation and synchronize the pointer to the selected gift center.
- [x] Keep right-stick vertical scrolling from moving the pointer.
- [x] Remove the top operation area from the gamepad focus chain while keeping LB/RB page switching and B close behavior.
- [x] Remove the bottom dismiss action from the gamepad focus chain and map X directly to dismiss-today.
- [x] Handle LT/RT as one visible-area scroll per press, with held-input repeat and boundary clamping.
- [x] Open on backpack's first gift when available, otherwise open storage directly when only storage has gifts; empty pages have no gamepad focus.
- [x] Reset the target page to the top on every page switch, focus its first gift, and move the pointer to that gift center.
- [x] On initial menu open, focus the current page's first gift and move the pointer to that gift center.
- [x] Add pure command dispatcher, repeat gate, focus resolver, empty-page, and long-list scrolling tests; keep the actual Stardew input hook in the in-game smoke checklist.

### Task 5: Handle invalid gifts and focused tooltip rendering

**Files:**
- Modify: `UI/GiftSuggestionMenu.cs`
- Modify: `UI/GiftSuggestionMenu.Drawing.cs`
- Create: `UI/GiftMenuTooltipLayout.cs`
- Test: `Tests/GiftMenuTooltipLayoutTests.cs`, `Tests/GiftSuggestionMenuInvalidationTests.cs`

**Interfaces:**
- `GiftMenuTooltipLayout` computes a tooltip rectangle from a focused slot rectangle, menu bounds, screen bounds, and measured tooltip size.
- Menu refresh logic removes stale `GiftMenuItem` entries and returns a valid focus target.

- [x] Revalidate source accessibility, container membership, stack count, and `CanAppearInGiftPrompt` before gamepad confirmation.
- [x] On invalidation, rebuild the current page, keep the same gift identity or closest valid item, or fall back to page chrome/dismiss; never record history or close the menu.
- [x] Track whether the active input is mouse or gamepad; mouse tooltip remains mouse-position based and takes precedence when hovering.
- [x] Render gamepad tooltip anchored to the focused gift slot, choose left/right based on available space, align vertically, and clamp inside menu/screen bounds.
- [x] Hide gift tooltip when focus is on close, page, or dismiss controls.
- [x] Add pure tooltip layout, source accessibility, identity restoration, and invalidation fallback tests.

### Task 6: Update GMCM copy and regression coverage

**Files:**
- Modify: `i18n/default.json`
- Modify: `i18n/zh.json`
- Modify: `Tests/GmcmApiTests.cs`
- Create: `Tests/GiftPromptControllerTests.cs`

**Interfaces:**
- Existing translation keys remain stable; only tooltip copy changes to state the backpack-only AutoGift boundary.

- [x] Update English and Chinese `gmcm.autoGift.tooltip` strings to explicitly say AutoGift never takes items from storage.
- [x] Keep `gmcm.showStorageItems.tooltip` scoped to manual suggestions and its balance warning.
- [x] Add assertions for both localized strings in `GmcmApiTests` and the registered GMCM option callbacks.
- [x] Run the full suite: `dotnet test FriendshipAssistant.Tests.csproj --no-restore -v minimal` (73/73 passed).
- [x] Run the release build: `dotnet build FriendshipAssistant.csproj -c Release --no-restore` (0 errors; 4 pre-existing architecture warnings).

## Completion Checklist

- [x] All implementation tasks and focused tests pass.
- [x] Full test suite and release build pass.
- [x] `git diff --check` is clean.
- [ ] Manual in-game smoke test covers controller focus, gift selection, both pages, long lists, dismiss, empty states, and mouse/keyboard non-regression.
- [x] Review the final diff against `docs/superpowers/specs/2026-08-24-support-gamepad-design.md` before claiming completion.
