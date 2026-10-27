# Reference Repositories and Reuse Policy

## Inspirations

- **LayoutFixer** — https://github.com/sina-developer/LayoutFixer (Swift/macOS, MIT): bidirectional English/Persian wrong-layout detection, physical keycode translation, dictionary confidence and Tab-confirmed popup. We intend a Windows-native independent implementation.
- **Filee** — https://github.com/KnifeLemon/Filee (C#/.NET/Avalonia, MIT): no-focus transparent floating window patterns, tray, themes, user settings, Windows UI drawing. Its file-conversion engine and visual assets are not part of LAYFIX.

## Licensing warning

LayoutFixer states that **Resources/en_50k.txt** and **Resources/fa_50k.txt** come from hermitdave/FrequencyWords and are licensed **Creative Commons Attribution-ShareAlike 4.0**, separate from LayoutFixer's MIT source license. These resources are not approved to be copied into LAYFIX simply because source files have MIT headers.

GitHub source is public but that does not waive attribution, trademark or dependency conditions. For actual copied source, retain license notices and document exact upstream revision/path. For dictionaries/assets, separately review compatible license/distribution requirements before committing.

## Provenance record (to complete when reusing material)

For every reused artifact record: upstream repository, upstream commit hash and file path; content included/modified; license SPDX identifier and license text; required attribution and notices; security review; date/owner approval. Third-party material must be auditable.

## Current bootstrap state

No external source files, fonts, images, trademarks, code snippets or word lists have been copied. This document records design inspiration only.

## Phase 1 inspection

- LayoutFixer commit `12e0085b1347d8e3bd09e00ed42f04203f06d882`: reviewed `Sources/main.swift` layout enumeration/selection and `Popup` nonactivating NSPanel. Carbon/TIS semantics are not portable to Windows; the keyboard tap, dictionaries, text deletion and retyping are outside Phase 1 and were not reused.
- Filee commit `3f790fb2460e44a449caf7c416ffb7a74362421c`: reviewed `src/Filee.App/Views/RadialWindow.axaml`, `Services/TransparentWindows.cs`, `Services/ThemeService.cs`, `Services/SoftPalette.cs`, `src/Filee.Platform.Windows/WindowsPlatformServices.cs`, its native declarations, and app project packaging. LAYFIX independently uses supported Avalonia window styles, platform colors, tray and backdrop APIs. No Filee source, conversion engines, dependencies unique to Filee, or artwork was imported.
