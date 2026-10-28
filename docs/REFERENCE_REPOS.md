# Source References and Provenance

The application now directly derives from [sina-developer/LayoutFixer](https://github.com/sina-developer/LayoutFixer) at `969814d2dbb6819eaef984525ef11db4c3f48dca`. The import contains only the Windows C# app, required icon assets, EN/FA frequency lists, Webster word data and applicable notices. macOS code, promotional assets and prebuilt binaries are excluded.

[The manifest](../provenance/upstream-import.json) records upstream paths, destination paths, byte counts, Git blob SHA-1 and SHA-256. [The import commit](../provenance/import-commit.txt) preserves every imported blob verbatim; later commits contain hardening and LAYFIX changes. Source history can be compared to the pinned upstream commit without conflating modifications with the import.

The former Avalonia/Filee-inspired implementation direction is superseded; PR #2 is not copied or used as a base. Filee remains only a historical design reference, with no imported code/assets/dependency in this migration.

See [third-party notices](../THIRD_PARTY_NOTICES.md) for MIT, CC BY-SA 4.0 frequency data and Webster status. Dataset authorship, source links, license and no-change statements must accompany redistribution. Do not attribute third-party dictionary data to LAYFIX or apply the code's MIT notice to those datasets.
