# LAYFIX Windows audit build

Derived from LayoutFixer Windows at `969814d2dbb6819eaef984525ef11db4c3f48dca`. The verbatim import is recorded separately in `../provenance/import-commit.txt`; later commits are LAYFIX changes. Build/test/package instructions are in the repository README and development workflow.

The retained tray and explicit `LayoutFixer.exe --test "sample"` detector run offline. Global observation, live popup acceptance, cross-application edits and post-correction layout switching are BLOCKED pending secure-control and atomic-span adapter acceptance. No global hooks are installed by the shipping process. Core engine behavior remains in source and is tested through injected boundaries. This is not production-ready and Phase 1 acceptance remains incomplete.

Portable output contains its .NET runtime, three dictionaries, required icons, provenance and MIT/CC BY-SA notices. No separate .NET install is required on an end-user PC. Extract the complete ZIP. See `../docs/SECURITY_AUDIT.md` and `../THIRD_PARTY_NOTICES.md` before using or redistributing it. No installer, release, automatic merge or Phase 2 is included.
