# Architecture Decision Log

Decisions below define the current proposed baseline. If later evidence invalidates one, submit an ADR update and tests in a Draft PR before changing product behavior.

| ID | Decision | Status | Rationale |
| --- | --- | --- | --- |
| ADR-001 | Windows 10/11 x64 first; C# / .NET 10 / Avalonia, native Windows interop | Accepted for initial implementation | Native Windows integration and Filee-inspired UI without requiring user SDK |
| ADR-002 | Separate UI, pure core and Win32/UIA/TSF adapters | Accepted | Testable logic and minimized native coupling |
| ADR-003 | Focused target application's verified layout is authoritative | Accepted | Windows may maintain per-thread/app input layout |
| ADR-004 | Conservative, explicit correction; no auto-rewrite by default | Accepted | Prevent text loss and shortcut hijacking |
| ADR-005 | Offline, no keystroke or user-text persistence, unknown sensitive context fails closed | Accepted | Privacy boundary of keyboard utility |
| ADR-006 | Windows accent/theme inheritance with override and opaque fallback | Accepted | Consistent Fluent feel and accessibility |
| ADR-007 | Codex Draft PR + GitHub Actions, no direct main merge/release | Accepted | Reproducibility and owner review |
| ADR-008 | Phase 1 portable self-contained x64 only; installer/signing later | Accepted | Early binary validation without user .NET dependency |
| ADR-009 | No project license selected during bootstrap | Open | Owner must approve licensing before redistribution |
| ADR-010 | Keyboard hook/TSF adapters and clipboard strategy require prototype evidence | Open | App behavior varies; prefer no clipboard fallback |

## Pending technical verification

- Which Windows API combination yields reliable switching for common TSF-aware apps and both Persian layout variants?
- What focused-editable and sensitive-field classification is safe for browser/Electron/desktop editors? Unsupported defaults to no observation.
- Does the non-activating floating widget preserve typing focus when clicked in Chrome, VS Code, Notepad and Telegram Desktop across Windows versions?
- Which selected-text replacement adapter is demonstrably non-destructive for each supported editor?
- Should full app UI be shipped Persian-first or English-first? Ship both locales in final UX without blocking Phase 1 engine development.

Open verification points are not license to implement risky guessed behavior.

## Phase 1 implementation decisions

- ADR-011: Use WinEvent foreground/focus notifications plus a 750 ms reconciliation interval. Windows does not broadcast another application's WM_INPUTLANGCHANGE to LAYFIX; no keyboard hook or injection is used. Pause stops native observation. Session/desktop transitions invalidate state.
- ADR-012: Resolve GetGUIThreadInfo's focused HWND and its owning thread, capture foreground/focus/process identity, and revalidate before one bounded WM_INPUTLANGCHANGEREQUEST. Independently read back the same context. No optimistic language or retry loop. A transition away and back invalidates an outstanding operation.
- ADR-013: Phase 1 supports conventional loaded English/Persian HKLs, including Persian Standard. A foreign TSF text-service profile cannot be verified using LAYFIX's own ITfInputProcessorProfiles or thread manager. IME/unsupported profiles, conflicting focus/top-level HKLs, inaccessible/elevated/remote/secure contexts fail closed. No universal TSF claim; interactive compatibility remains a manual gate.
- ADR-014: Use Avalonia's supported platform color and transparency APIs, native system accessibility/composition checks, centralized contrast-aware tokens, and a compact non-activating HWND. Native backdrop requests always include an opaque fallback. Settings are local schema-v1 JSON with validated values and atomic replacement.
- ADR-015: Launch-at-login is deferred to Phase 4. No registry startup changes, installer, signing, update service, text capture or correction in Phase 1. The owner's explicit Phase 1 portable-preview request authorizes CI preview artifacts; the original project license remains undecided and no redistribution license is invented.
