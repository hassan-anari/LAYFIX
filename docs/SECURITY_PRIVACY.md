# Security and Privacy Contract

LAYFIX can observe keyboard events, a uniquely sensitive privilege. Privacy-by-design and text integrity are release blockers, not optional polish.

## Default privacy guarantees

- Fully offline keyboard observation, layout conversion, and widget functionality.
- No stored/reported raw key events, plain text typed by the user, selected content, clipboard contents, passwords or rich window content.
- No key stream analytics, cloud detection, telemetry SDK, hidden background uploads or network fallback.
- Volatile bounded buffers only when observation is enabled and focused control is confirmed safe; wipe on pause, app change, desktop lock, error, stale context or shutdown.
- Sanitized diagnostic events are local-only, opt-in and capped with a clear erase control. Include only status codes, non-sensitive supported-app identifiers (if opted in) and performance summaries.
- Disable correction in protected/secure/password contexts and whenever safe-text eligibility cannot be established. Global keyboard hook presence alone is not a safety signal.
- Do not run as administrator or bypass the integrity level boundary to modify elevated or protected applications.

## Threat model

- **Password/private text exposure** through global hooks, UIA, logs, crash dumps or clipboard. Mitigation: conservative observation gate, volatile buffer, no content logs, exclusion list, sanitized crash output.
- **Destructive edits** from stale focus, caret movement, app-specific editors, race with async popup, or missed events. Mitigation: context token, readback validation, capability checks, abort rather than guess.
- **Synthetic input loops** and stolen shortcuts. Mitigation: recognized injection tagging, Tab only in eligible suggestion state, hook timing bounds, feature pause.
- **Clipboard replacement/leak**. Mitigation: no implicit clipboard fallback. Future optional adapter requires consent and bounded tested restore.
- **Untrusted dependencies/CI artifacts**. Mitigation: least-privilege jobs, pinned/approved dependencies, code review, no credential-bearing artifact, reproducible release metadata.
- **Misleading OS language status**. Mitigation: state derived from foreground target and verified after switch; Unknown and Failure visible.

## Storage

A versioned preferences file may store widget geometry, accent preference, shortcuts, selected language profile, per-app exclusion identifiers and local diagnostics consent. It must not store textual input. File is written atomically under per-user application data. Sensitive exception traces are never emitted.

## Communications

Core app makes zero network calls. Optional update check belongs to later phase, is disabled by default until approved, uses only HTTPS and publishes an understandable privacy notice. No automatic self-update or undisclosed remote commands.

## Dependency and license policy

- New packages require pinned/versioned dependency entry, security review, license compatibility and explicit rationale.
- LayoutFixer and Filee repositories are MIT, but LayoutFixer's bundled FrequencyWords dictionaries are **CC BY-SA 4.0**, not MIT; do not copy/bundle them until license/attribution/distribution obligations are analyzed and accepted.
- Do not use code, logos, screenshots or resource files from references without proper license attribution.
- Repository owner must choose LAYFIX's own license before public distribution; third-party MIT licenses do not automatically license original LAYFIX code.

## Disclosure and vulnerability handling

Do not put security-sensitive input traces in public GitHub issues. Submit sanitized reproduction steps and technical conditions only; coordinate any sensitive report privately with the repository maintainer.

## Required security acceptance

Test excluded/password fields, blocked/elevated foreground, stale focus/selection, synthetic reentrancy, runtime pause and resume, hook teardown, disabled network, corrupted preferences, accidental log leakage, and artifact provenance. Any confirmed text loss or sensitive recording is a stop-ship P0.
