# Architecture Decision Log

| ID | Decision | Status / reason |
| --- | --- | --- |
| ADR-001 | Windows 10/11 x64, C# / .NET 10 | Retained; actual compatibility acceptance outstanding |
| ADR-002 | Adopt actual LayoutFixer Windows at 969814d2dbb6819eaef984525ef11db4c3f48dca | Approved reset; supersedes Avalonia-first projects/PR #2 |
| ADR-003 | Keep upstream Windows Forms tray/popup and physical/dictionary correction rules | Approved; no independent algorithm recreation or final UI work |
| ADR-004 | Exact focused-thread HKL, retain variants, reject ambiguous opposite selection | Required for correctness; explicit profile UI deferred |
| ADR-005 | Offline/no content persistence; unknown sensitivity fails closed | Release blocker; no cloud/telemetry/clipboard fallback |
| ADR-006 | No generic Backspace replacement | Unsafe upstream path removed; atomic-span adapter required |
| ADR-007 | Global observation and cross-app editing blocked in audit binary | No real secure-field/exact-span adapter is yet accepted; no hooks installed, no settings/CLI bypass |
| ADR-008 | Retain engine behind injected boundaries; dependency-free real regression executables | Test upstream behavior and safety contracts without representing fake adapters as native guarantees |
| ADR-009 | Verbatim import first, then hardening; SHA-256/blob manifest | Reviewable provenance with separate immutable import commit |
| ADR-010 | Separate MIT application and CC BY-SA 4.0 datasets, full attribution/legal text in artifact | Migration explicitly authorizes this import with redistribution obligations documented |
| ADR-011 | Pinned SDK/Actions, locked dependency-free restore, Windows self-contained folder ZIP | Inspectable unsigned audit artifact; no upstream binaries, single-file, release or installer |
| ADR-012 | Overall license for original LAYFIX additions | Still open; do not remove upstream grants/notices or claim a production licensing decision |
| ADR-013 | Final switcher and extensive settings UI | Subsequent development stages; Phase 2 is not started |

Remaining verification: real sensitive-control detection, exact editor revision/span replacement, layout-switch readback, bounded hook/event processing and interactive Windows acceptance. A disabled feature is an explicit blocker and must never be described as a completed safe adapter.
