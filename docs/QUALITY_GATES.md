# Test Strategy and Quality Gates

CI restores/builds the actual upstream-derived Windows source and runs two dependency-free regression executables. They invoke linked production source, return nonzero on any failed case and emit JUnit XML. They are real assertions, not placeholder `dotnet test` successes. There is no test discovery dependency; the harness also fails on an empty suite. Required checks cannot be skipped just because this is an audit build.

`tests/LayoutFixer.Tests` covers actual dictionary data/normalization/classification, both conversion directions, short-word and consecutive-word rules, false positives, Unicode, injectable physical mapping with Shift/Caps, multiple layout variants, focused-thread identity, safe/unknown/password policy facts, context invalidation, visible Tab acceptance, normal Tab/Esc, pause/timeout/reset/disposal, exact-span rejection, races, failed/partial edits, switch outcome and hook rollback. These tests establish coordinator contracts using fakes; they do not certify secure-field detection or a real editor.

`tests/LayoutFixer.NativeTests` runs only on Windows x64. It checks actual Win32 ABI sizes, installed layout enumeration, hook registration/cleanup without inspecting any event data, native US Shift/Caps/punctuation and Persian/Persian Standard physical mapping. Layouts are temporarily loaded in the CI session without activation or input injection and unloaded if newly added. A failure to load/map is a test failure, not a silent skip.

CI on `windows-2022` uses SDK 10.0.100, locked restore, Release build, both suites, self-contained win-x64 non-single-file publish, package content validation, SHA-256 and artifact upload. Source provenance is checked separately against the dedicated verbatim import commit. All test results and actual counts must be reported from the final head run. A self-contained package does not prove fresh-PC launch or interactive correctness.

## Interactive acceptance — NOT RUN

| Scenario | Required evidence before affected feature is enabled |
| --- | --- |
| Fresh supported Windows 10/11, no .NET installed | Extract audit package, launch tray, no global collection, quit cleanly |
| Native standard editor / supported future adapter | Exact span/caret/selection, correct preview, explicit Tab acceptance, undo and clipboard unchanged |
| Passwords in native/browser controls, uncertain UIA providers | No buffering or correction; no private diagnostic output |
| Notepad, Chrome/Edge, VS Code/Electron, Telegram, IME/terminals | Explicit adapter capability matrix; unknown means blocked |
| Focus/caret/selection/layout change before Tab | No stale edit; normal Tab navigation preserved |
| English variants, Persian and Persian Standard | Both directions, Shift/Caps, punctuation, combining characters and surrogate pairs |
| Secure desktop, lock/unlock, elevated targets, RDP | Fail closed, no elevation/UIPI bypass or hidden observation |
| Partial/failed edit and target layout switch | No retry, no false success, no unintended target switch |
| DPI/multiple monitors, timeout/dismiss, shutdown | Correct popup placement, no revival, no leaked hooks/forms/timers |

Current automated native tests generate no real input and are not substitutes for these journeys. No performance, memory, compatibility, privacy-detector or production-readiness claim follows from a green run. Missing safe adapters and real desktop evidence leave Phase 1 incomplete.
