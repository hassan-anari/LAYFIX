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
