# Test Strategy and Quality Gates

## Truthfulness

Green GitHub Actions means *only* that the checks actually run passed. Windows GitHub-hosted runners do not prove real interactive keyboard behavior, non-activating overlays, TSF switching, multi-display composition or editor compatibility.

Before Phase 1 contains projects, CI is **bootstrap/docs-only**; do not refer to it as a passing .NET build. Once Layfix.slnx exists, a failure in restore/build/test or publish is a blocker.

## Automated test layers

**Core unit tests (cross-platform):**
- Layout-key mapping, word boundaries, short/ambiguous words, Persian normalization, Unicode and punctuation.
- Suggestion lifecycle, Tab/Esc routing, timeout/focus/layout invalidation and stale generation prevention.
- Settings validation, version migrations, bad/corrupt file recovery, theming tokens, clamping/recovery of monitor coordinates.
- No-network behavior and privacy-safe log formatting.

**Windows native adapter tests:**
- Mocked thread/HWND layouts, switch/readback state, timeouts and errors; safe handle disposal.
- Hook subscription lifecycle, synthetic event identification and concurrency bounds.
- Editing preconditions, permission failure, unknown/sensitive controls and focus/caret race aborts.
- Integrations that require a real desktop should be classified separately from normal headless CI tests.

**CI integration & packaging:**
- .NET restore, release build, test, Windows win-x64 self-contained publish and portable artifact upload.
- Fresh-package launch smoke testing when a suitable interactive environment exists.
- Security scan/dependency/license audit and release signature/checksum verification added before public release.

## Human-run Windows acceptance matrix

| Scenario | Minimum expected verification |
| --- | --- |
| Win11 23H2+ and currently supported Win11 | FA/EN readback, focus-preserving click switch, external switching, glass or fallback |
| Win10 22H2 | Same language behavior, opaque/Acrylic supported fallback, no Win11-only Mica assumption |
| Notepad and typical Win32 edit | Keyboard switching, manual correction in later phase |
| Chrome/Edge web textareas and contenteditable | Focus, caret, password/secure-exclusion tests |
| VS Code and Electron-based chat editors | No destructive edits when context cannot be verified |
| Telegram Desktop | Language/widget test, later correction capability classification |
| Keyboard English US / Persian / Persian Standard | Variant-specific physical conversion verification |
| 100%, 150%, 200% scaling and multi-monitor | Visible/usable widget on screen, no cursor-position jumps |
| fullscreen/high contrast/RDP | Graceful suppression/fallback, never unintended focus change |
| Elevated app/UAC/secure desktop | Explicit unsupported/blocked behavior, no privilege bypass |
| Mixed scripts, shortcuts, IME, dead keys | No broken Tab, no lost keystrokes, safe cancellation |
| Unplug/plug monitor, lock/unlock, sleep/resume | Reconcile language state and restore safe widget position |

## Non-functional targets (to benchmark, not claim)

- Input hooks must return promptly; target typical callback time below 2 ms with no blocking UIA/network/I/O calls.
- Idle memory/CPU must be measured on target machines, with an initial goal of low single-digit CPU use (near zero idle) and no growing buffers/leaks.
- Widget responsiveness: fast state display following observed layout change; avoid aggressive polling.
- All measurements must report hardware, OS, build, methodology, sample size and result. No fabricated baselines.

## PR completion gate

A task is not complete unless:
1. Changed behavior and limitations are documented with tests and a compatibility note.
2. All required CI jobs finish green at exact final head SHA (or failures are explicitly stated).
3. Test totals, skips, Windows interactive tests *not run*, and artifact links are provided.
4. No security/privacy violation and no stale background hook remains.
5. An independent review can inspect a still-unmerged Draft PR.

## Manual test protocol

Use a disposable test document. Check expected foreground HWND and language before/after, input focus, original/converted text, undo, clipboard unchanged, secure field disabled, cancellation, and correct UI status after failure. Do not reproduce a bug by capturing or exporting private text. Log only non-sensitive outcomes.
