# Third-party notices

LAYFIX Phase 1 uses pinned NuGet packages and a self-contained .NET runtime. Full resolved package versions and integrity hashes are recorded in each project's `packages.lock.json`. License texts for distributed dependencies are in [docs/licenses](docs/licenses/) and are copied into the portable package's `ThirdPartyLicenses` directory. Original LAYFIX licensing remains undecided; this notice grants no license to original LAYFIX code.

| Dependency | Version | License / purpose |
| --- | --- | --- |
| Avalonia, Desktop, Themes.Fluent, Controls.ColorPicker | 11.3.22 | MIT; application UI, tray, platform colors, native backdrop negotiation and accessible color picker |
| Avalonia Win32, Skia, Native, X11, FreeDesktop, Remote.Protocol and related assemblies | 11.3.22 | MIT; transitive Avalonia platform/rendering support. Cross-platform managed assemblies may be present but LAYFIX executes only on Windows |
| Avalonia.BuildServices | 11.3.2 | MIT; transitive build-only dependency. All assets/targets explicitly excluded to disable its build telemetry; not a runtime service |
| Avalonia.Angle.Windows.Natives | 2.1.25547.20250602 | BSD and bundled third-party terms in package LICENSE; Windows graphics |
| SkiaSharp / NativeAssets.Win32 | 2.88.9 | MIT wrapper; bundled graphics licenses in THIRD-PARTY-NOTICES; rendering |
| HarfBuzzSharp / NativeAssets.Win32 | 8.3.1.1 | MIT wrapper; bundled shaping licenses in THIRD-PARTY-NOTICES; Persian/Latin shaping |
| MicroCom.Runtime | 0.11.0 | MIT; Avalonia COM interoperability |
| Tmds.DBus.Protocol | 0.21.3 | MIT; transitive non-Windows Avalonia support, unused by LAYFIX on Windows |
| .NET runtime | 10.0.14 (SDK 10.0.403) | MIT and component notices; self-contained Windows runtime |
| Microsoft.NET.Test.Sdk | 17.14.1 | MIT; build/test only, not packaged |
| xunit / xunit.runner.visualstudio | 2.9.3 / 3.1.4 | Apache-2.0; test only, not packaged; retain upstream notices when redistributing test tools |

Sources: [Avalonia](https://github.com/AvaloniaUI/Avalonia), [ANGLE](https://github.com/google/angle), [SkiaSharp](https://github.com/mono/SkiaSharp), [HarfBuzzSharp](https://github.com/mono/SkiaSharp), [MicroCom](https://github.com/kekekeks/MicroCom), [Tmds.DBus](https://github.com/tmds/Tmds.DBus), [.NET](https://github.com/dotnet/runtime), [xUnit](https://github.com/xunit/xunit), [VSTest](https://github.com/microsoft/vstest).

The package-embedded native licenses/notices are preserved verbatim. Avalonia's MIT text was read from its 11.3.22 tag (`licence.md`); MicroCom's MIT text from upstream `LICENSE` (copyright 2021 Nikita Tsukanov); Tmds.DBus's COPYING from package-declared commit `8cb04f66c330b64244e996ff68f685f27f7381a0`. Runtime license/notices came from the exact Microsoft.NETCore.App.Runtime.win-x64 10.0.14 NuGet runtime pack. Avalonia build services share the Avalonia project's MIT terms (package copyright 2023–2025 The AvaloniaUI Project).

## Review and privacy

Dependencies serve UI/rendering or tests, not keyboard data collection. Runtime app code has no network client, keyboard hook, clipboard API, UI Automation text reader, remote process injection or correction engine. The sole explicit URL action opens the repository in the user's browser. CI opts out of SDK/Avalonia build telemetry. NuGet advisory warnings are errors; Avalonia 11.3.8 was rejected because its DBus 0.21.2 transitive dependency triggered GHSA-xrw6-gwf8-vvr9; 11.3.22 resolves 0.21.3. Restore's current advisory result is evidence, not a promise of future vulnerability absence.

## Inspiration, without imported material

- LayoutFixer by Sina Gholizadeh — https://github.com/sina-developer/LayoutFixer — MIT source, inspected at `12e0085b1347d8e3bd09e00ed42f04203f06d882`.
- Filee by Myeonggu Kim and contributors — https://github.com/KnifeLemon/Filee — MIT source, inspected at `3f790fb2460e44a449caf7c416ffb7a74362421c`.

No reference source, fonts, artwork, dictionaries or identity assets were copied. LAYFIX's small arrow icon is original geometry. LayoutFixer's FrequencyWords files are separately CC BY-SA 4.0 and are not bundled.
