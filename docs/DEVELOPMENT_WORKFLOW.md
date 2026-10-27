# Codex + GitHub Development Workflow

## Branch/PR protocol

1. Read AGENTS.md, product/architecture/quality/privacy specs, and current main.
2. Interpret the current phase only; write acceptance checklist and identify missing assumptions as bounded documented defaults, not arbitrary scope expansion.
3. Create a task branch from latest main; use a Draft PR throughout implementation.
4. Add production code, unit/integration tests, documentation updates and relevant negative tests.
5. Push small traceable commits; wait for all applicable GitHub Actions jobs at the final head.
6. Report exact SHA, Draft PR URL, job statuses, test counts/skips, artifact names, risk and remaining manual tests.
7. **Do not merge or release** without explicit owner authorization; update PR after review comments on the same branch.

## CI contract

**Bootstrap:** docs-contract check and Windows job explicitly report no solution yet; no claims of a successful C# build/test.

**After Phase 1 skeleton:** .github/workflows/ci.yml performs Windows-hosted .NET 10 restore, Release build, unit tests and self-contained win-x64 publish, then uploads a portable zip artifact. The Windows job must fail if a solution exists but tests/build/publish fail. Tests requiring interactive foreground windows are not assumed covered.

**Later:** add explicit release workflow for signed/tagged releases, Installer verification, checksums, SBOM and updater requirements. Never publish from PR events; never expose credentials to untrusted changes.

## How a user with no .NET installed uses builds

- Open the PR's GitHub Actions CI run, download the **LAYFIX-win-x64-portable** artifact ZIP after its Windows job succeeds.
- Extract on a supported Windows machine. In Phase 1 and later, the app package is expected to contain the .NET runtime (self-contained).
- Report actual end-to-end Windows results; do not assume click-to-switch is correct just because dotnet publish succeeds.
- First phase creates a portable build; Setup.exe and signed production release are scheduled for Phase 4.

## Change management

- Significant API, storage or security decisions: update docs/DECISIONS.md before implementation.
- New configuration keys: schema migration + negative tests + validation + preview.
- New external library: record provenance/license/security review in THIRD_PARTY_NOTICES.md as needed.
- Bug fixes: add reproduction/regression test, assess other apps/layouts, never conceal skips or override checks.
- When no UI automation can be run in CI: explicitly list a reproducible manual verification procedure with pass/fail results marked Not Run until actually performed.

## Review checklist

No sensitive keyboard data? No destructive action without context validation? Correct per-target layout semantics? No non-target Tab interception? No unverified native handles left? No hard-coded accent/default glass dependency? All tests actually run? Artifact self-contained? No direct main mutation/automatic merge?
