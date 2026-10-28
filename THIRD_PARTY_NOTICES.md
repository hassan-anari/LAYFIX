# Third-party notices and redistribution obligations

## LayoutFixer Windows source and icons — MIT

Source: https://github.com/sina-developer/LayoutFixer/tree/969814d2dbb6819eaef984525ef11db4c3f48dca

Copyright (c) 2026 Sina Gholizadeh. The six original Windows C# files, project and required app/tray icons are imported under MIT. The complete, unchanged copyright/permission/warranty notice is in `licenses/upstream/LICENSE` and must accompany all copies or substantial portions, including the portable binary. The original `THIRD_PARTY_NOTICES.md` is preserved verbatim in `licenses/upstream/` for provenance; its macOS/promotional font entries describe upstream content that is not bundled here.

LAYFIX modifications are separate from the verbatim import: privacy gate, removal of unsafe generic replacement, injected safety/layout boundaries, Shift/Caps/Unicode fixes, lifecycle changes, tests, build/package configuration and documentation. No upstream EXE or unrelated macOS/promotional source/assets were copied. An overall license for original LAYFIX additions remains an owner decision; upstream grants remain intact. This document does not purport to license all original LAYFIX work under MIT.

## FrequencyWords English and Persian data — CC BY-SA 4.0

Credit: **Hermit Dave**, FrequencyWords, derived from **OpenSubtitles** data. Dataset source: https://github.com/hermitdave/FrequencyWords

- `Resources/en_50k.txt`: `content/2018/en/en_50k.txt`
- `Resources/fa_50k.txt`: `content/2018/fa/fa_50k.txt`

Both files are redistributed **unchanged from the pinned LayoutFixer commit** and copied unchanged into the binary package's `data/` folder. Original-byte SHA-256 checksums appear in `provenance/upstream-import.json`. LAYFIX runtime normalization does not modify the distributed datasets.

License: **Creative Commons Attribution-ShareAlike 4.0 International**, https://creativecommons.org/licenses/by-sa/4.0/ . Full legal text: `licenses/CC-BY-SA-4.0.txt`, copied from Creative Commons' official legalcode repository blob `2d58298e6eda10e7204abb52722efbc840db2390` (`docroot/legalcode/by-sa_4.0.txt`). The MIT code license does not apply to these datasets.

Redistributors must retain reasonable creator/source attribution, copyright/license/disclaimer notices supplied with the material, a link/copy of the CC license and the source URI; indicate if any changes are made and retain prior change indications. Sharing adaptations of the data requires CC BY-SA 4.0 or a permitted compatible adapter license, attribution and change disclosure. Do not impose additional legal restrictions or effective technological measures that prevent recipients from exercising the licensed rights. No endorsement by the data authors is implied. Merely packaging the unchanged datasets alongside software does not relicense the entire software as CC BY-SA. The review artifact includes these notices and full legal text rather than mislabeling all contents MIT.

## Webster word list — upstream public-domain attribution

`windows/data/words.txt` is imported unchanged from the same pinned upstream commit. Upstream identifies it as Webster's Second International Dictionary (1934), the `/usr/share/dict/words` word list, in the public domain. We retain that source statement and its exact hash; no independent chain-of-title or all-jurisdiction public-domain determination is claimed. Review jurisdiction-specific status before a production release. It is not represented as an MIT or FrequencyWords dataset.

## Build/runtime components

No third-party NuGet package is added to application or test projects. The .NET SDK is pinned in `global.json`; self-contained publish redistributes Microsoft's .NET/Windows Desktop runtime components with their own notices in the publish folder. Preserve those generated notices. The CI uses SHA-pinned GitHub Actions. Test/CI tools do not become application runtime dependencies.
