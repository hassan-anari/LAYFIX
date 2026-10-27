# Roadmap and Phase Gates

Development is gated. Each phase has a separate Codex task, reviewable Draft PR, exact SHA, green applicable CI, manual test plan and owner approval before merge. Do not develop later phases early.

## Phase 0 — Repository specification and governance (current)

- Written product contract, architecture, UI system, security/privacy policy, test/release standards, Codex AGENTS.md, GitHub PR template, basic CI.
- No app build or functional UI is claimed.
- Gate: all intended files present, cross-references coherent, bootstrap docs job green, Draft PR not merged.

## Phase 1 — Windows foundation and language switcher

- Create Layfix.slnx, production projects, test projects, versioned settings and dependency management.
- Implement native foreground target/language reader with Unknown state and verified language-switch request.
- Implement a minimalist non-activating, movable FA/EN widget, tray, pause/show/quit, system theme/accent and readable visual fallback; user can change at least size, opacity and position.
- Ensure external Windows layout switching updates the widget; do not steal keyboard focus on click.
- Initial Win10/Win11 compatibility matrix, deterministic tests for state/settings/window abstractions, Windows integration smoke tests where possible.
- GitHub Actions restores, builds, tests and uploads a self-contained unsigned Windows x64 portable ZIP. No .NET runtime needed on the end-user machine.
- Gate: clean-room Windows launch, no false language indicator, verified focus preservation on supported apps, diagnostics show blocked switch accurately, meaningful passing CI.
- Excluded: any typo detection or rewriting; full final settings polish; installer/signing.

## Phase 2 — Safe manual correction

- Source/target keyboard layout mapping, deterministic conversion (FA/EN, variants, Shift, punctuation, Unicode).
- User-invoked correction for verified selection/context with explicit safe edit boundaries and abort logic.
- No blind bulk backspace or default clipboard mutation. App compatibility table and manual verifications.
- Gate: comprehensive corpus unit tests, race/focus validation, no inadvertent edits in secure/unsupported contexts.

## Phase 3 — Smart detection and suggestions

- Local frequency dictionaries with license-approved source, conservative wrong-layout detector, FSM for candidate runs, cursor-near popup.
- Tab to accept only when suggestion context valid; Esc dismiss; no automatic rewriting by default.
- Application exclusions, sensitivity setting, ambiguous-word behavior, performance tests.
- Gate: test matrix for normal navigation, secure controls, code editors, IME and mixed-language cases; no destructive false positives in verified regression set.

## Phase 4 — UI completion and distribution

- Full Fluent glass theme system, optional custom accents, personalization/settings, localization, accessibility and resilient monitor handling.
- Portable x64 plus installer/uninstaller, release workflow, checksums, changelog and upgrade path; signing strategy reviewed separately.
- Extended compatibility, failure telemetry strictly local/opt-in, documentation, performance and soak testing.
- Gate: signed versus unsigned status clearly disclosed; clean-machine install/uninstall, persistence and privacy audit; explicit release approval.

## Post-MVP options (not approved)

ARM64, additional languages, advanced app adapters, optional opt-in update checks, richer diagnostics, automatic fixing, enterprise deployment. None are implicit requirements or permitted scope creep.

## Release decision rule

An artifact existing in CI is only a downloadable build. A production-quality/releasable feature requires all code, tests, Windows hands-on validation, privacy checks and user approval. Missing UI tests must be reported, never assumed.
