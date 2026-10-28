# Development Workflow and CI Contract

Use the latest `main` Phase 0 baseline and a separate `feat/upstream-layoutfixer-windows` branch with a Draft PR. PR #2's Avalonia implementation is rejected: do not merge/copy it or use that branch as the base. Never automatically merge, close previous PRs, release or begin Phase 2.

1. Read AGENTS.md and current product/security/architecture/test documents.
2. Import only the actual pinned Windows sources, required data/assets and notices, verbatim in the first commit. Record upstream commit, SHA-256 and Git blob identities. Preserve that import in history.
3. Make subsequent hardening/tests/spec changes in separate reviewable commits. Reuse the upstream classifier and run rules. Block unsafe/uncertain functionality explicitly until a real adapter is proven; never use permissive placeholders to turn a test green.
4. Run provenance checks, locked restore, Release build and both regression executables. Observe all Actions jobs at the exact final head. Report failures accurately and fix feasible failures.
5. Produce a self-contained unsigned x64 folder/ZIP with dictionaries, licenses, SHA-256, build provenance and downloadable XML results. Do not publish a GitHub Release or installer.
6. Report exact final SHA, Draft PR URL/unmerged state, changes, commands, actual test totals, skipped/failed checks, run/job/artifact links, limitations and interactive NOT RUN items.

[CI](../.github/workflows/ci.yml) has least-privilege `contents: read`, pinned Actions and pinned SDK 10.0.100. The Windows job runs the two explicit test executables and verifies their JUnit totals. No credentials, untrusted write tokens, prebuilt upstream EXEs or package failure bypasses are used. Self-contained publish includes .NET runtime; no end-user SDK/runtime install is required. `LAYFIX-win-x64-portable` contains the portable ZIP and checksum; `layfix-test-results` contains the JUnit reports. Artifacts expire after 14 days and are audit evidence, not production releases.

Download from the final PR Actions run, extract fully, inspect `BUILD_PROVENANCE.json`, `SECURITY_AUDIT.md` and notices, and launch `LayoutFixer.exe` on a disposable supported Windows desktop. Global observation and edits remain blocked. Real interactive acceptance must be recorded separately from the native API tests. No final switcher/settings UI is part of this migration.
