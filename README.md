# LAYFIX

LAYFIX adopts the real Windows C# application from [LayoutFixer](https://github.com/sina-developer/LayoutFixer), pinned to **969814d2dbb6819eaef984525ef11db4c3f48dca**. The previous Avalonia Phase 1 direction is rejected. This branch starts from Phase 0 `main`; it does not reuse PR #2.

**Audit build: global observation and corrective edits are BLOCKED.** No editor has yet passed secure-field and atomic-span acceptance. The upstream engine, physical conversion, frequency rules, consecutive-word suggestions and Windows Forms popup remain in source and are exercised through injectable boundaries. The running application currently provides the tray and explicit offline command-line detector. It does not monitor typing, display live suggestions, consume Tab/Esc, rewrite text or switch the target's layout. Phase 1 is not fully accepted and this is not production-ready.

The approved product direction includes a privacy-restricted keyboard observation and correction component. It must never store/transmit typed text, query unsupported sensitive controls or blindly delete characters. See [security findings and blockers](docs/SECURITY_AUDIT.md).

Build and run on a development Windows PC with .NET SDK **10.0.100**:

```powershell
dotnet restore Layfix.slnx --locked-mode
dotnet build Layfix.slnx -c Release --no-restore
dotnet run --project tests/LayoutFixer.Tests -c Release --no-build -- TestResults/core.xml
dotnet run --project tests/LayoutFixer.NativeTests -c Release --no-build -- TestResults/native.xml
dotnet run --project windows/LayoutFixer.Windows.csproj -c Release
```

Explicit detection check, with exactly one English and one Persian layout installed:

```powershell
.\LayoutFixer.exe --test "صشدف لقشذ"
```

Only caller-supplied test text is printed by `--test`; no observed user input is logged. With multiple variants the diagnostic refuses to guess. The executable retains its upstream name for traceability.

[GitHub Actions](.github/workflows/ci.yml) builds from source on Windows, runs regression and native API tests, and creates **LAYFIX-win-x64-portable** (ZIP + SHA-256), plus **layfix-test-results** (JUnit XML). The portable folder includes the .NET runtime; end users need no separate .NET installation. Extract the ZIP before running. This unsigned audit artifact is not a release, installer or evidence of interactive compatibility.

[Provenance](provenance/upstream-import.json) records all 16 imported blobs and SHA-256 checksums. [The import commit](provenance/import-commit.txt) is separate from hardening. MIT application notices and separately licensed CC BY-SA 4.0 word lists are described in [third-party notices](THIRD_PARTY_NOTICES.md) and included in the package. No macOS sources, promotional files or upstream binaries are bundled.

Read [product requirements](docs/PRODUCT_SPEC.md), [architecture](docs/ARCHITECTURE.md), [roadmap](docs/ROADMAP.md), [test gates](docs/QUALITY_GATES.md) and [development workflow](docs/DEVELOPMENT_WORKFLOW.md). All interactive editor acceptance remains **NOT RUN** until recorded on real Windows desktops. Do not merge, publish a release or begin Phase 2 automatically.
