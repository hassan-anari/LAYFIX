# Phase 1 implementation and acceptance record

Scope: PRODUCT_SPEC P-01, essential P-04 preferences, and P-05 privacy/lifecycle. Phase 2–4 correction, suggestion, installer and release work is excluded.

## Implemented

- Real `Layfix.slnx` with App, Core, Windows and two test projects, SDK 10.0.403, locked dependencies and advisory warnings as errors.
- Focused-thread HKL detection; explicit FA/EN, Unknown, Unsupported, Unavailable and Paused states; loaded-layout discovery and exact preferred variants; external-change reconciliation; single-request switch verification and stale-context rejection.
- Small non-activating draggable HWND, bounded DPI/monitor positioning, persistent geometry/scale, tray, single-instance activation and orderly shutdown.
- System appearance/accent, manual overrides, contrast-aware tokens, opaque/accessibility fallback, English/Persian Settings with RTL and a color picker.
- Schema-v1 local JSON, validated defaults, atomic replacement, corrupt-file recovery, future-schema preservation and visible save failures.
- Mandatory Windows CI: documentation contract, restore, Release build, tests with nonzero counts/no skips, read-only Win32 smoke, self-contained publish, output-file verification, packaged application lifecycle smoke, TRX and portable ZIP artifacts.

## Evidence

Local Linux validation: Release build passed with zero warnings/errors; 52 core tests and 17 fake-boundary adapter tests passed (69 total, no failures/skips). The separately classified real Windows native smoke is excluded locally, not counted as passed. A self-contained win-x64 directory was cross-published successfully; Linux cannot launch its executable.

Windows CI run [40540054160](https://github.com/hassan-anari/LAYFIX/actions/runs/40540054160) passed on implementation commit `8f2c4985dea6e236a943936a4a5cde2d45708f96`: 52 core and 18 Windows tests (including one real read-only native smoke), zero failures/skips, self-contained publish and packaged startup/shutdown. The runner was Windows Server 2025 build 26100, not Windows 10/11 desktop validation. Final artifact provenance and superseding exact-head CI are recorded in Draft PR #2; delivery must inspect the final run after any subsequent change.

The independent native UI source review found and prompted fixes for shutdown ordering, cross-monitor drag recovery and surface/text opacity. Source review does not establish visual or interactive approval. Six contrast/fallback regression cases exercise the token contract; custom-accent cases additionally require visible 3:1 thin marks. CI also renders all five Settings sections in English/light and Persian/dark to inspect real templates and localization without generating keyboard input. Every interactive row in [Phase 1 validation](PHASE1_VALIDATION.md) remains **NOT RUN** without a real Windows desktop. CPU/memory/latency benchmarks are also NOT RUN.

## Important limits and decisions

Readback verifies conventional keyboard HKLs, not every foreign TSF service. No TSF manager on LAYFIX's thread is used to fabricate a foreign-profile result. IMEs, conflicting focused/foreground layouts, missing focus, elevated/inaccessible targets, remote sessions and secure desktops are rejected. Native requests have a bounded timeout, can be ignored, and cannot be atomically retracted after delivery; no repeat/focus/input-injection fallback exists.

Widget text and the active segment remain opaque; only an available glass surface uses the opacity preference. Disabled transparency, unavailable effects and high contrast remain opaque. Fullscreen overlay behavior follows Windows composition; tray hide/restore is always the recovery path.

Launch-at-login is explicitly deferred. Original project licensing remains undecided; the owner-requested portable preview is unsigned and grants no redistribution rights to original LAYFIX code. Dependency notices are included. No code/assets/dictionaries from the references were copied.
