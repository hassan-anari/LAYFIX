# LAYFIX

**Status: specification and repository bootstrap; no application has been implemented yet.**

LAYFIX is a planned privacy-first, Windows-native English/Persian keyboard layout assistant. It combines (1) a floating input-language indicator with click-to-switch and (2) conservative, user-confirmed recovery of text typed under the wrong keyboard layout.

## Product principles

- Works offline for all core keyboard functionality; no cloud AI, typing telemetry, or raw keystroke logging.
- Reads the effective input language of the **target/focused Windows application**, not merely LAYFIX's own thread.
- Displays a draggable, non-activating floating FA/EN switcher without stealing typing focus.
- Offers exact, layout-aware, reversible-in-principle correction only when safe; never silently destroys text.
- Supports Windows 11 Mica/Acrylic where possible and graceful Windows 10/disabled-transparency fallback.
- Inherits the system accent and light/dark preference by default; allows user overrides.
- Exposes controls, application exclusions, accessibility, diagnostics, startup, and pause/exit in the app.
- Uses Codex + Draft PRs + GitHub Actions for changes, tests, and self-contained Windows builds.

## Planned stack

C# / .NET 10 / Avalonia UI, with Windows APIs (Win32, UI Automation, TSF as needed) behind testable abstractions. The planned solution is **Layfix.slnx**, with projects in **src/** and tests in **tests/**. No application source files exist yet.

## Specifications and operating rules

- [Codex rules](AGENTS.md)
- [Product specification](docs/PRODUCT_SPEC.md)
- [Architecture](docs/ARCHITECTURE.md)
- [Visual design and customization](docs/DESIGN_SYSTEM.md)
- [Roadmap and phase gates](docs/ROADMAP.md)
- [Tests and quality gates](docs/QUALITY_GATES.md)
- [Security and privacy](docs/SECURITY_PRIVACY.md)
- [Development workflow](docs/DEVELOPMENT_WORKFLOW.md)
- [Release process](docs/RELEASE_PROCESS.md)
- [Technical decisions](docs/DECISIONS.md)
- [Reference projects and third-party licenses](docs/REFERENCE_REPOS.md)

## Repository status and CI

The repository bootstrap CI currently checks required documentation. **When Layfix.slnx is added in Phase 1**, the Windows job will restore, build, run tests, and, when the app project exists, publish a self-contained Windows x64 portable ZIP as a GitHub Actions artifact. A successful bootstrap check is **not** a claim that an executable was built or tested.

Installers, signing, tagged release automation, and in-app updates are later-phase deliverables. Nothing is automatically merged, deployed, or publicly released.

## External inspiration

- [LayoutFixer](https://github.com/sina-developer/LayoutFixer) — macOS keyboard layout mistake detection and Tab-confirmation concept.
- [Filee](https://github.com/KnifeLemon/Filee) — C# / Avalonia interface, native-feeling floating windows, theming and tray patterns.

See [references and licensing](docs/REFERENCE_REPOS.md). No third-party code, dictionaries, or visual assets are imported during bootstrap.

## License

**Undecided for LAYFIX.** Public repository visibility does not grant unrestricted reuse rights. Choose an explicit project license before distributing or accepting external contributions; preserve all applicable third-party notices.
