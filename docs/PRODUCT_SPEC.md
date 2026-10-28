# Product Requirements — Upstream Windows Migration

## Approved Phase 1 scope

LAYFIX is an offline English/Persian Windows keyboard-layout correction utility derived directly from the real LayoutFixer Windows C# source at `969814d2dbb6819eaef984525ef11db4c3f48dca`. It is not a semantic translator or cloud writing assistant. Windows 10/11 x64 is the target; compatibility must be measured on actual desktops.

P-01: Preserve upstream frequency dictionary detection and conservative English/Persian confidence rules, including short ambiguous words, Webster protection, wrong-layout false positives and consecutive mistaken runs. Reuse the implementation; do not independently recreate it.

P-02: Preserve physical-key conversion in both directions. Handle Shift/Caps Lock, punctuation, valid Unicode sequences, focused thread identity and installed layout variants. Refuse unmappable/dead keys, unknown focus and ambiguous target selection rather than guessing.

P-03: Retain the upstream tray and non-activating correction popup near the caret. Tab may accept only a visible suggestion whose exact editor/span/context is valid; Esc dismisses and remains available to the app. Normal Tab must pass through otherwise. Cancel on focus/layout/caret/selection change, timeout, shortcut, mouse movement affecting context, pause or failure.

P-04: Preserve explicit offline `--test` detection using caller-supplied sample text and installed layouts. It must not observe or persist actual user typing. Retain opt-in Start with Windows and Quit controls without changing existing upstream preference keys silently.

P-05: Include an approved, privacy-restricted keyboard observation/correction component. Never buffer secure/password/private or uncertain contexts. No typed-text persistence, telemetry, network API/model, implicit clipboard use, elevation or privilege bypass. Destructive edits require atomic exact-span validation and explicit acceptance. A generic Backspace count is insufficient.

P-06: Build from C# source in our Windows GitHub Actions with .NET 10. Provide a self-contained unsigned x64 portable folder/ZIP, SHA-256, test XML, data/license notices and commit/run provenance. No separate end-user .NET installation; no installer/release in this task.

## Current capability and limitations

The audit build runs the retained tray and caller-invoked detector. Actual global input observation, live suggestions, Tab acceptance, cross-app replacement and post-correction layout switching are **BLOCKED** until a safe editor adapter is verified. The classifier, physical conversion and suggestion/acceptance engine remain testable production source. This is an explicit Phase 1 gap, not a completed feature claim. The shipping process registers no global hooks or UIA callbacks.

## Acceptance journeys (all interactive journeys NOT RUN)

1. Fresh Windows PC without .NET: extract portable ZIP, launch tray, verify notices, quit cleanly. Check no hooks/input collection while blocked.
2. Explicit test mode: one EN plus one FA layout; confirm deterministic detection for both directions and conservative short words. Multiple variants must report ambiguity.
3. Before enabling a future adapter: known secure/password field, browser password, lock/UAC/elevated/remote/unsupported contexts must produce no textual buffer or correction. Unknown means disabled.
4. Safe supported editor: type a disposable mistaken run, inspect preview, accept only its exact span, verify layout switch/readback, unchanged clipboard and undo. Move caret/focus/layout before Tab: abort without loss and preserve navigation.
5. Partial/failed edit: no blind retry, no mistaken success or layout switch; partial uncertainty disables the session.

## Deferred UI

The final language switcher, full settings, extensive personalization, English/Persian localized UI and accessibility polish are subsequent approved stages to be designed around the safe upstream engine. No rejected Avalonia interface is recreated in Phase 1. No Phase 2 development is authorized by this migration.
