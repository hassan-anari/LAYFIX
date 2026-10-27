# Product Specification

## Goal and user problem

Windows English/Persian users frequently type text with the wrong active keyboard layout and cannot reliably see or switch the language of the currently focused app. LAYFIX offers a persistent, low-distraction floating language switcher and a safe, optional correction suggestion that reconstructs the intended characters from physical keyboard input.

## Target and boundaries

- Supported initial platform: Windows 11 and Windows 10, x64; no admin permission required for normal usage.
- Languages in MVP: English and Persian, including physical keyboard layout variants configured in Windows.
- Scope: keyboard *layout* correction, not semantic translation, spell-checking or an AI writing assistant.
- No promise that every app, elevated window, secure field, game, IME or remote desktop will support global input interception or rewriting.
- Entire core feature set works offline. Optional update checking, if implemented later, must be opt-in and disclosed.

## Functional requirements

### P-01: Language indicator and switcher

- Shows the **effective active input layout/language in the foreground target**; reads changes made externally, including Windows shortcuts and per-app layout changes.
- Clearly distinguishes FA, EN and Unknown/Unsupported (never display a guessed language as fact).
- A compact, draggable, always-on-top, borderless floating widget with click-to-switch; configurable position, size, opacity, theme and display mode.
- Pointer interaction must not permanently steal focus from the target text field. Language change must be verified against the original target; indicate failure without lying about state.
- Supports multiple monitors, per-monitor DPI, monitor disconnection, taskbar and fullscreen policies; remembers a valid on-screen position.
- Has an independent system tray control to hide/show, pause monitoring, open settings and quit.
- Do not repeatedly post language changes on a timer. Use notifications or bounded observation to reflect external changes.

### P-02: Manual layout correction

- User invokes a configurable shortcut to correct the applicable selection or safely tracked text run without translating meaning.
- Conversion must use actual keyboard mapping/layout pair wherever available, not a naive fixed substitution table. Account for modifier state, keys that do not map one-to-one, punctuation and Unicode.
- Before rewriting, validate foreground HWND, target edit control, selection/caret, recorded layout and text span. Abort safely on unsupported/unverified edit surfaces.
- Explicit preview/confirmation is preferred when an edit may be destructive. Never delete arbitrary characters from a different field or stale caret.
- If the edit fails, no claim of success; provide non-destructive error feedback. Clipboard is not an implicit fallback.
- Support both directions, FA -> EN and EN -> FA.

### P-03: Suggestions for mistaken typing

- Check candidate words/runs after word boundaries, using frequency-aware dictionaries and conservative context rules inspired by LayoutFixer.
- No automatic rewriting by default. Show a small preview near the caret if safely locatable; fallback to an unobtrusive position when caret geometry is unavailable.
- When suggestion is valid and visible, Tab accepts and switches to the target layout; Esc dismisses; continued normal typing may dismiss or extend suggestion according to a tested state machine.
- Otherwise, Tab and Esc retain normal app behavior. Do not trap navigation or shortcuts; clear stale suggestions on focus change, edit/caret movement, mouse clicks, layout change or timeout.
- Handle ambiguous short words, mixed-language text, brand names, code, URLs, email addresses, punctuation and IME carefully. Favor missing a suggestion over damaging valid text.
- Sensitivity and dismissal learning may be added later, without persisting typed content.

### P-04: Preferences and customization

- In-app Settings for appearance, accent/color override, transparency/fallback, widget size/position, animations/reduced motion, theme and RTL language.
- Correction controls: global enable/pause, manual shortcut, suggestion enable, confirmation behavior, sensitivity and exclusion list.
- Keyboard controls: chosen English/Persian layouts and user-definable switch hotkey where safe; detect unsupported layouts.
- App controls: start with Windows (user consent), tray, notifications, update check (optional future), restore defaults, diagnostics.
- Preferences persisted locally and atomically with schema version, validation, recovery from corruption and upgrade migrations. No cloud profile.
- Accessibility: keyboard navigation, usable contrast on all accents/backdrops, screen readers as practical, reduced motion and touch-sized targets.

### P-05: Privacy, reliability and feedback

- No text/key content stored or transmitted, no default telemetry, no accidental clipboard exfiltration.
- Sensitive fields and unknown contexts are excluded by default; user can pause from tray/widget.
- Diagnostics may report sanitized app identity, OS version, effective language, operation outcome/error code and performance totals, but never typed content or selections.
- Duplicate instance prevention, crash resilience, cleanup on exit, and no abandoned global hooks or altered system settings.
- Meaningful indication when target layout switching is blocked by Windows/app-specific behavior. Never claim to repair Windows Text Input Services automatically.

## Out of scope for MVP

- Semantic translation, cloud LLM, word prediction, grammar correction, automatic language switching from intent, mobile/macOS/Linux, elevated process control, kernel drivers, Windows language installation, gaming anti-cheat support, network sync, collecting typing analytics.

## Acceptance journeys

1. **Normal switching**: Focus an ordinary editable field; widget shows current language, clicking changes target language while focus remains usable; external Win+Space updates widget.
2. **Wrong Persian layout**: Type the physical keystrokes for “this instead of writing it in english” while Persian input is active; a preview offers intended English; Tab safely replaces the recorded run and language updates, or aborts without loss.
3. **Wrong English layout**: Type keystrokes whose alternate Persian layout is “سلام”; receive an appropriate suggestion, with safeguards for common English words.
4. **Dismissal**: Normal Tab continues to navigate when no eligible suggestion exists. Esc dismisses without editing.
5. **Unsupported editor**: Cannot verify caret/selection or safety: no edit, no hidden clipboard mutation, diagnostics explain unsupported context.
6. **Privacy**: Secure/password field: no recorded textual run, suggestion or corrective replacement.
7. **Customization**: Changing theme/accent override/opacity or language of UI updates immediately and survives restart.
8. **Offline distribution**: Download the CI portable ZIP on clean supported Windows; run without installing .NET SDK or runtime.

Acceptance journeys are goals, not claims that code exists. Each phase has its own verifiable gates in [Roadmap](ROADMAP.md).
