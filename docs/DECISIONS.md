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
