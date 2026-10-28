# Technical Architecture

Phase 1 adopts the actual upstream Windows C# / .NET 10 application, pinned at `969814d2dbb6819eaef984525ef11db4c3f48dca`. Windows Forms retains the working tray and non-activating popup. Avalonia and the rejected PR #2 implementation are not part of this migration.

```text
Layfix.slnx
windows/LayoutFixer.Windows.csproj
windows/src/Program.cs        tray, startup preference and explicit --test detector
windows/src/Dictionaries.cs   upstream conservative frequency classifier, normalization
windows/src/Fixer.cs          upstream word/run rules, suggestion and acceptance lifecycle
windows/src/Layouts.cs        physical mapping and focused-thread layout boundaries
windows/src/Popup.cs          retained WinForms popup, native caret geometry only
windows/src/Native.cs         Win32 ABI and interop declarations
windows/src/Safety.cs         privacy policy, context/edit contracts, hook ownership gate
Resources/                   unchanged English/Persian frequency datasets
windows/data/words.txt        unchanged Webster data
provenance/                   original hashes and import commit
licenses/                    upstream MIT and CC BY-SA legal text
tests/LayoutFixer.Tests/      production-source deterministic regression executable
tests/LayoutFixer.NativeTests/ Windows native API regression executable
```

`Dictionaries.IsMistake` retains upstream ranks, the 30,000-word Persian cutoff, English/Webster protection, minimum-letter and short-predecessor rules. Normalization now composes Unicode canonically and rejects unknown language tags. Required datasets cannot silently disappear. No independent replacement algorithm or cloud detection is introduced.

`Fixer` retains physical key records, previous word, suggested run and the 12-word extension rule. Its platform dependencies are injected: layout access, popup, timer and `ICorrectionBoundary`. Context identity includes foreground HWND, focused HWND/thread, exact HKL and generation. Unknown, sensitive, stale, changed-layout, changed-focus, timeout, unsupported-mapping, shortcut, injected and Backspace events clear the run. Caps Lock is recorded with Shift per key. Buffers are bounded; pause/disposal immediately drop references to text. Managed strings are not guaranteed to be overwritten in memory.

`Layout.Character` resolves scan codes through the destination HKL and uses `ToUnicodeEx` with the no-state-change flag. Multiple installed variants are retained. An ambiguous opposite-layout choice returns Unknown instead of picking the first. Dead/unmapped/control/invalid UTF-16 outputs cancel conversion. A selected profile UI is deferred; no locale identity is guessed from the process thread when focus lookup fails.

The old Backspace/SendInput replacement implementation is removed. `ICorrectionBoundary.TryValidateSpan` must establish the exact expected span, caret/selection, revision and context without crossing privacy/integrity boundaries; `Replace` must atomically compare and replace that span, without Backspace or clipboard fallback. The coordinator rechecks context immediately before the operation. Rejected edits preserve Tab; failed/partial edits never switch layouts, partial results latch a fault without retry, and switching is only requested after confirmed success in the same context. A real adapter must also revalidate the layout-switch target internally. These contracts are tested using deterministic fakes; no production editor adapter is claimed.

**Shipping gate:** `TrayApp` installs no keyboard, mouse or foreground hooks, creates no Fixer text buffer and makes no UIA calls. Global observation and rewriting remain blocked until an independently reviewed adapter establishes all privacy facts and safe edit semantics. This is a deliberate unresolved Phase 1 blocker. The popup and corrected engine are retained for adapter integration, rather than presented as working cross-app functionality.

`HookLifetime` provides transactional registration and reverse-order idempotent cleanup. It is exercised by native API smoke tests; production does not currently register hooks. Any future callback must do constant bounded work and enqueue only eligible events, never await a provider or edit inline. The old popup background UIA tasks and continuations have been removed; no stale continuations can show a dismissed popup.

No runtime third-party NuGet dependencies are added. SDK, Actions and dependency lock files are pinned. The CI package is a self-contained unsigned win-x64 folder with dictionaries, license notices, original checksums and build provenance. It is not a production release. Final switcher/settings UI and Phase 2 are excluded.
