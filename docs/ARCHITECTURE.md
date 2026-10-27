# Technical Architecture

## Technology and system boundaries

- C# and .NET 10; Avalonia Desktop for application UI and composition.
- Native Windows interoperability isolated behind interfaces; use Win32, UI Automation and Text Services Framework (TSF) selectively.
- Single resident user-session process, unprivileged by default; no driver, service, system-wide DLL injection or external daemon.
- Avoid unnecessary third-party code; maintain small auditable dependencies.

## Planned solution structure

~~~text
Layfix.slnx
src/
  Layfix.App/        Avalonia app, floating widget, settings, tray, popups, composition
  Layfix.Core/       Pure models, mappings, detection rules, state machines, settings schema
  Layfix.Windows/    Window focus, layout provider, input hook, UIA, TSF, Win32 interop
tests/
  Layfix.Core.Tests/
  Layfix.Windows.Tests/
docs/
.github/workflows/
~~~

Do not introduce empty or fake buildable projects in bootstrap; create them in Phase 1 with real tests.

## Components and responsibilities

**LanguageStateService**: retrieve foreground HWND and target thread; derive active input locale/layout from target thread where possible, reconcile state after external shortcuts, expose Unknown explicitly. Switching is a request to the appropriate target, then an independently verified readback. Calls such as GetForegroundWindow, GetWindowThreadProcessId, GetKeyboardLayout, and WM_INPUTLANGCHANGEREQUEST are candidate mechanisms. TSF-aware apps may require a more specific adapter; unsupported cases must be reported, not forced.

**InputObservationService**: optional low-level keyboard monitoring, strictly bounded buffer, cancellation on unsafe context, reentrancy-tagged synthetic events, minimal work inside hook callback. Pure events are delivered to a state machine without logging source text. Use WH_KEYBOARD_LL only with bounded callback latency and explicit lifecycle management.

**LayoutMappingService**: translate physical key events under source/target active layouts including modifiers, variant IDs, punctuation, dead keys and Unicode. Windows ToUnicodeEx is a candidate, but dead-key/state side effects must be mitigated and verified. Never assume every layout pair is invertible.

**CorrectionEngine**: deterministic pure input-to-output conversion and dictionary/context scoring. It returns a *proposed* correction and confidence; it cannot perform I/O or mutate the user document.

**EditSessionCoordinator**: safe commit boundary. Snapshot target identity, layout, selection/caret and a limited observable text span. Just before editing, recheck preconditions; reject changed/unsupported context. An adapter performs the smallest supported edit via verified platform surface and reports success/failure. Revalidate outcome where possible; do not blindly send N backspaces based solely on a stale keyboard buffer.

**SuggestionStateMachine**: deterministic states Idle -> Collecting -> Suggested -> Applying/Dismissed -> Idle, with stale-context, timeout, manual interruption and focus-change guards. In particular, Tab is only consumed while Suggested and the context token still matches.

**FloatingWidgetService**: a non-activating overlay plus tray settings. Ownership and pointer activation/focus behavior must be tested on real Windows. Multi-display position normalization, per-monitor DPI and accessibility controls are mandatory.

**PreferencesService**: versioned schema, migrations, atomic persistence under per-user app data, strongly validated values and recovery from malformed files. Does not persist observed text.

**DiagnosticsService**: sanitized opt-in technical events, privacy filters, capped local retention, easy reset/disable.

## Correctness and privacy boundaries

- Never treat a global key hook as evidence that the foreground control is editable or non-sensitive. Require an independent focused-control safety verdict; Unknown => no monitoring/correction.
- IsPassword and UIA support are imperfect: protect web password fields, OS secure desktops and hidden input using conservative gates and exclusions; only extend support when verified.
- Key observation must ignore self-injected events. Avoid hook reentrancy, input races and UI thread dispatch in hook callback.
- Synthetic input is constrained by Windows security boundaries and foreground ownership; do not bypass UIPI.
- No clipboard replacement by default. A future explicit clipboard strategy would require consent, atomic snapshot/restore, conflict protection and independent tests.
- Separate lookup, suggestion and mutation flows so a failed read or permission gap never escalates to unsafe deletion.

## Async/concurrency design

- UI binds to a serialized language/suggestion state stream; render on UI dispatcher.
- Hook callbacks enqueue compact events and return immediately; no file I/O, UIA calls, dictionaries loading or synchronous render in callback.
- Long operations are cancellable; focus/layout changes invalidate outstanding proposals by monotonically increasing context generation.
- System state is authoritative; the floating UI never optimistically displays FA/EN as confirmed before readback.

## UI/OS design

- Default theme/accent inherited from Windows with user override. Mica for appropriate app surfaces on Windows 11; Acrylic/translucent popup only where supported; solid readable fallback otherwise.
- Never assert Windows 10 Mica support. Respect transparency/high contrast/reduced motion and remote desktop composition limitations.
- Window HWND ex-styles (e.g., no-activate) and Avalonia ShowActivated=false behavior must be verified empirically for pointer click without focus loss.
- No window stealing foreground focus for input switching, tooltip, popup or notification.

## Risks and mitigations

1. **Per-thread language state**: target HWND-thread lookup, readback, Unknown status.
2. **Unsafe global edits**: fail-closed context validation, no blind destructive fallback, abort on race.
3. **Sensitive input**: conservative focused-control policy and explicit exclusions.
4. **Cross-app variance**: adapter capability matrix and honest unsupported outcomes.
5. **Hook performance**: constant-time callback, bounded buffers, timing instrumentation without sensitive content.
6. **Transparent surface compatibility**: solid fallback, native smoke tests, reduced effects.

Use [Quality Gates](QUALITY_GATES.md) for test strategy and [Decisions](DECISIONS.md) for unresolved choices.

## Implemented Phase 1 boundary

The solution now implements the three production projects and two test projects above. Later-phase components remain descriptions, not implemented features.

- `LanguageStateService` owns serialized observations and a bounded switch state machine. The App dispatches notifications to the UI thread. `LanguageObservationWorker` coalesces foreground/focus events into a capacity-one channel; a 750 ms timer reconciles foreign language changes that do not produce a cross-process notification. It never sends requests on a timer.
- `WindowsLanguagePlatform` uses an injectable `IWindowsInputApi`. `Win32InputApi` resolves foreground/focused HWNDs, their processes and threads, compares cross-thread HKLs, checks desktop/session/integrity eligibility, and sends a single timeout-bounded language request. The platform observer has its own STA message loop, three out-of-context WinEvent registrations and a hidden session/power/appearance broadcast window.
- Context identity includes a native event generation; a foreground/focus event invalidates a captured switch even when the user returns to the same target. Immediately before the request the adapter rechecks target identity and source HKL. Readback checks the same identity for at most 800 ms. Acknowledged message delivery is never success. There is no AttachThreadInput, SetForegroundWindow, ActivateKeyboardLayout, SendInput or keyboard hook.
- Standard keyboard HKLs are the observable contract. ImmIsIME/E-prefixed layouts are unsupported; conflicting focus/foreground HKLs are Unknown. Transient/unrecognized languages are unsupported. LAYFIX's own TSF profile cannot establish a foreign TSF profile's effective state, so no TSF manager is substituted for foreign readback. A third-party TSF service can disagree with its host's HKL: such behavior is outside verified coverage, and the UI describes thread-layout verification. Owner desktop validation must establish supported applications; this is not universal effective-profile support.
- A native message cannot be transacted atomically with foreground changes. Final HWND/thread/process/generation checks narrow races; requests remain addressed to the captured original HWND and never follow another foreground target. A timed-out target may process a delivered message later. No retries, synthetic input, or focus-restoration tricks are used.
- Widget styles include WS_EX_NOACTIVATE/TOOLWINDOW and WM_MOUSEACTIVATE returns MA_NOACTIVATE. The physical-pixel native region matches the pill; drag uses screen coordinates and working-area clamps. Native focus-style failure hides the widget and exposes recovery guidance in Settings.
- Settings schema v1 is under `%LOCALAPPDATA%/LAYFIX/settings.json`; I/O runs outside the UI thread. Writes serialize through a capacity-one latest-settings channel and SettingsStore's write gate, flush to a unique temporary file, and replace atomically. Future schema files are preserved read-only. No observed input state is persisted.
- Shutdown pauses requests, stops and joins observation, cancels/joins switching, releases native subscriptions on their registration thread, drains settings writes, removes tray resources, and exits the explicit application lifetime. There is no external worker process.

See [Phase 1 validation](PHASE1_VALIDATION.md) for actual evidence and limitations.
