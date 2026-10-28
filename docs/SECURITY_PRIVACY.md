# Security and Privacy Contract

LAYFIX includes an approved, privacy-restricted keyboard observation and correction component derived from LayoutFixer Windows. This component remains disabled in the audit build until real secure-control and atomic-span editor adapters pass acceptance. See [Security Audit](SECURITY_AUDIT.md) for findings, mitigations and unresolved blockers. Do not describe a policy predicate or mocked adapter as Windows secure-field detection.

- Core behavior is offline. No telemetry, cloud model/API, key analytics, content logs, clipboard fallback or undisclosed network activity.
- Never persist/transmit typed text, passwords, selected text, clipboard contents, document/window titles or keystroke runs. Caller-supplied `--test` samples are explicit diagnostic input and may be printed; do not use private data there.
- Independently establish control sensitivity/editability, focused HWND/thread, foreground identity, default desktop, integrity and session status before accepting any textual event. Password/sensitive/unknown/unsupported/remote contexts fail closed. Windows does not universally hide password keystrokes from low-level hooks.
- Keep eligible text only in bounded volatile buffers, clear references on pause, failure, timeout, focus/layout/caret change and shutdown. Managed strings are not guaranteed to be overwritten in memory.
- Destructive edits require a visible exact-context suggestion, explicit confirmation and an adapter that atomically validates the actual span/revision at mutation. No generic Backspace deletion, privileged injection or implicit clipboard strategy.
- Validate input/edit results; no blind retry or layout switch after failure. Partial uncertainty disables the session. Layout switching must target the same verified editor and independently verify readback.
- Do not call unbounded UIA providers or perform dictionary/I/O/UI work in keyboard callbacks. Pending events and async work must carry invalidatable context generations. Dispose timers/forms/subscriptions and roll back partially registered hooks.
- Current shipping policy installs no keyboard/mouse/foreground hooks, starts no text engine and makes no UIA provider calls. It cannot intercept Tab/Esc, observe secure-field typing or modify another application. This is a feature blocker, not proof that an unimplemented detector is safe.

## Storage and diagnostics

Only the existing explicit Start with Windows registry preference is retained. No observed-text storage, dictionary learning, diagnostics traces, new settings database or exclusion-profile persistence is introduced. No error trace may include content. Adding diagnostics later requires sanitized status codes and opt-in bounded retention.

## Dependencies and licenses

All imported code/data/assets require traceable provenance. Preserve the upstream MIT notice; FrequencyWords English/Persian datasets are separately CC BY-SA 4.0 and remain unchanged, with credit to Hermit Dave/OpenSubtitles, legal text, source URLs, change status and no added restrictions/DRM. Redistributed adapted datasets require a compatible share-alike license. Webster data is public domain according to the pinned upstream notice; document that reliance and jurisdiction limits. See [third-party notices](../THIRD_PARTY_NOTICES.md).

The migration request explicitly authorizes importing these datasets and building a review artifact after documenting obligations. It does not choose an overall production license for original LAYFIX additions. No copied macOS/promotional assets or upstream binaries are permitted.

## Disclosure and release gate

Do not post sensitive input traces to issues or PRs. Use sanitized reproduction conditions. Any actual sensitive collection or text loss is a stop-ship P0. Before enabling observation, complete secure/unknown/elevated/remote/lock tests, exact-span/caret/focus races, failed/partial edit tests, normal navigation, cleanup and packaged offline validation. All interactive checks remain **NOT RUN** until evidence is recorded.
