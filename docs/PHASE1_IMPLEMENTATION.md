# Phase 1 implementation and acceptance record

Scope: PRODUCT_SPEC P-01, the essential P-04 preferences, and P-05 privacy/lifecycle. Phase 2–4 correction, suggestion, installer and release work is excluded.

## Acceptance checklist

- [ ] Real .NET 10 / Avalonia application and isolated Win32 adapter
- [ ] Foreground/focused-thread language observation and verified, race-guarded switching
- [ ] Non-activating draggable widget, tray, single instance and clean shutdown
- [ ] System appearance, accessibility fallback, English/Persian settings and local persistence
- [ ] Deterministic core/adapter tests, Windows CI and self-contained portable ZIP
- [ ] Manual Windows matrix recorded honestly, with NOT RUN where no desktop is available

Implementation and evidence will be updated in this Draft PR. An artifact or headless build does not establish interactive Windows compatibility.
