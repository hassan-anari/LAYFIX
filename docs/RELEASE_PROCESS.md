# Build Artifacts and Release Process

Phase 1 builds `windows/LayoutFixer.Windows.csproj` from the pinned upstream-derived C# source on a Windows GitHub-hosted runner with SDK 10.0.100. No upstream precompiled executable is used. The unsigned self-contained win-x64 folder is ZIP-packaged with runtime, all dictionaries, MIT/CC BY-SA notices, security blocker disclosure and source/run provenance. A SHA-256 companion file is uploaded with it.

The artifact name is `LAYFIX-win-x64-portable`; JUnit test evidence is `layfix-test-results`. An end-user PC needs no separate .NET install. Extract the complete folder; do not copy only the EXE. This is an audit build: global observation and correction are blocked pending real adapter safety acceptance.

No production release, installer, signing bypass, automatic merge or Phase 2 is authorized. Before any release, the owner must review actual CI evidence, fresh-Windows launch, real editor/privacy acceptance, artifact/license composition, project licensing and signing. Never describe an unsigned PR artifact as a signed production build. Do not silently enable blocked features for release packaging.
