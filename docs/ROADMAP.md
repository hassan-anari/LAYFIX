# Roadmap and Phase Gates

## Phase 0 — specifications (baseline)

Completed baseline `main`: `27de06e738cd585173a30419dd993eb427a5bd74`. The original Avalonia-first implementation plan is superseded. Draft PR #2 must not be merged or used as the development base.

## Phase 1 reset — upstream Windows import and safety audit (current)

Adopt LayoutFixer Windows at `969814d2dbb6819eaef984525ef11db4c3f48dca`. Preserve a verbatim import commit with checksums, then make separate hardening/test/specification changes. Retain the actual dictionary detector, physical mapping, consecutive-word rules, tray, caret popup, Tab/Esc semantics and command-line detection mode. No independently recreated algorithm.

Required delivery: pinned .NET 10 Windows CI build, meaningful automated tests, self-contained unsigned x64 portable ZIP with datasets/dependencies/notices, SHA-256 and machine-readable test results. No upstream executables or unrelated macOS/promotional files.

The current audit build intentionally blocks global observation and destructive editing. To complete Phase 1, implement and independently verify an editor adapter that proves secure-field exclusion and atomically validates/replaces the exact span; wire its eligible input stream to the retained engine without blocking keyboard callbacks. Complete the real Windows acceptance matrix. Never enable unsafe generic Backspace replacement to satisfy a demo.

Phase 1 acceptance is incomplete while that adapter and interactive evidence are missing, even if the CI package builds. See [Security Audit](SECURITY_AUDIT.md) for individual blockers.

## Next approved development stages (do not begin in this task)

After owner review of the functioning safe upstream engine, design the final language switcher and extensive settings/localization UI around it. Installer/signing, release publication, richer editor adapters and additional platforms require their own scope and review. They are not implicit authorization to proceed to Phase 2 here.

## Release decision

A CI artifact is a downloadable audit build. Do not merge, close previous PRs, publish a release, claim production readiness or infer interactive compatibility from green API tests.
