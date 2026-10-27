# LAYFIX

LAYFIX Phase 1 is an unsigned Windows 10/11 x64 preview of an offline English/Persian language indicator and switcher. It uses .NET 10, Avalonia and isolated Win32 APIs. It does not capture keystrokes or typed text, access the clipboard, correct text, or call a cloud service.

The compact floating FA/EN pill observes the foreground/focused Windows thread. Clicking requests one language change for that captured target, then verifies readback. Unknown, unsupported, unavailable and paused states are explicit. The widget uses non-activating native styles, is draggable, and remembers a bounded on-screen position. Interactive editor compatibility remains **NOT RUN** until the [Windows validation matrix](docs/PHASE1_VALIDATION.md) is completed.

## Download the portable preview

Open the [Phase 1 Draft PR](https://github.com/hassan-anari/LAYFIX/pull/2), select its latest successful CI run, and download **LAYFIX-win-x64-portable**. Extract the artifact, then extract `LAYFIX-win-x64-portable.zip` completely and run **Layfix.App.exe**. Keep its DLLs and runtime alongside it. No .NET SDK or runtime installation is required. This is an unsigned development artifact with limited retention, not a production Release or installer.

## Use

- Click the pill to toggle the captured English/Persian target; drag it to move. An exclamation mark means the latest switch failed or was cancelled; hover or open Keyboard settings for the reason.
- Use the tray to open Settings, hide/show the widget, pause/resume observation, or exit. Closing Settings keeps the resident app running. A second launch opens the existing instance's Settings.
- Settings provide English/Persian UI with RTL, System/Light/Dark theme, Windows accent or a custom color, glass/opaque preference, surface opacity, scale, drag lock, topmost behavior, target variants and reset controls.
- Settings live at `%LOCALAPPDATA%\LAYFIX\settings.json`. Malformed preferences recover to safe defaults; newer schemas are not overwritten. No observed language/window/input state is persisted.

Windows/system accessibility settings take precedence over glass/custom appearance. Unsupported native effects use an opaque surface. Launch-at-login is deferred; the preview never changes Windows' configured languages or startup entries.

## Build and test

```powershell
dotnet restore Layfix.slnx --locked-mode
dotnet build Layfix.slnx -c Release --no-restore
dotnet test Layfix.slnx -c Release --no-build --no-restore --logger "trx;LogFileName=test-results.trx"
./build/package.ps1
./build/smoke-portable.ps1
```

Use SDK 10.0.403 (global.json). CI preserves documentation checks, builds all five projects, runs nonzero tests and real read-only Win32 smoke, publishes a self-contained directory, verifies native/runtime files, checks published-process lifecycle, and uploads the ZIP/TRX outputs. On non-Windows exclude `Category=NativeSmoke`; this is an excluded Windows test, not a desktop pass. No headless CI check proves focus preservation in Chrome, VS Code, Notepad or Telegram.

## Architecture and limits

`Layfix.slnx` contains `src/Layfix.App` (UI/composition/tray), `src/Layfix.Core` (state/settings/rules), `src/Layfix.Windows` (Win32/adapters), and matching Core/Windows test projects. See [implementation](docs/PHASE1_IMPLEMENTATION.md), [architecture](docs/ARCHITECTURE.md), [decisions](docs/DECISIONS.md), [design](docs/DESIGN_SYSTEM.md), [quality gates](docs/QUALITY_GATES.md), [privacy](docs/SECURITY_PRIVACY.md), [workflow](docs/DEVELOPMENT_WORKFLOW.md), and [release rules](docs/RELEASE_PROCESS.md).

Conventional focused-thread HKLs are supported. Foreign TSF profiles cannot universally be verified through a different process's thread manager. IMEs, elevated/inaccessible apps, secure desktops, remote sessions and conflicting thread state fail closed. See the validation matrix for focus-race and timeout limits. Exclusive fullscreen overlay visibility is controlled by Windows. Compatibility and resource/latency measurements require real Windows testing.

Phase 2–4 conversion, dictionaries, suggestions, text hooks, installers, signing, updates and production Releases are not implemented. The PR remains Draft and must not be merged or released automatically.

## References and license

[LayoutFixer](https://github.com/sina-developer/LayoutFixer) and [Filee](https://github.com/KnifeLemon/Filee) were technical/design references only. No source, artwork or dictionaries were copied. See [provenance](docs/REFERENCE_REPOS.md) and [third-party notices](THIRD_PARTY_NOTICES.md).

Original LAYFIX license is **undecided**. Public visibility and this owner-requested preview do not grant redistribution rights. Preserve bundled dependency licenses.
