# Phase 1 Upstream Security Audit

Upstream: `sina-developer/LayoutFixer@969814d2dbb6819eaef984525ef11db4c3f48dca`. This is a source audit and automated-contract test result, not a penetration test or interactive certification.

| Finding | Severity | Disposition in audit build |
| --- | --- | --- |
| Global low-level hook buffers without secure-field evidence | Critical | BLOCKED: production registers no global hooks and creates no input engine/buffer; secure-control adapter acceptance is outstanding |
| Backspace replacement uses recorded key count, not validated current document span | Critical | Removed unsafe replacement; exact-context/atomic-span adapter contract introduced and negative tests added; real replacement remains BLOCKED |
| Tab consumed on any pending run even with stale/hidden popup | High | Engine requires visible popup, known safe unchanged context, replacement capability and exact span; no production interception while blocked |
| Unchecked SendInput, partial deletion and premature layout switch | High | Generic injection path removed; failed/partial results never switch, partial uncertainty latches fault, no retry; no shipping injection |
| Async UIA caret task can fault, hang or revive stale popup | High | Removed asynchronous UIA work and stale continuations; retained popup uses native caret geometry on owning UI thread |
| Key/detection/UI work inside hook callback | High | Original callbacks removed from shipping process; future adapter must provide bounded eligible-event queue and fail closed on backlog |
| Unbounded word growth and incomplete timer/form cleanup | Medium | 512-key aggregate limit, 12-word run limit, injected/shortcut/Backspace cancellation, timer disposal contract and immediate pause clearing |
| Hook registration result unchecked, partial setup leaks | Medium | Transactional/idempotent HookLifetime with reverse-order rollback and tests; no production hooks installed |
| First installed EN/FA layout chosen silently; Caps ignored | Medium | Keep variants, refuse ambiguous opposite selection, map physical scan in destination, retain per-key Shift/Caps, reject invalid mapping/focus |
| Missing frequency data silently yields empty detection | Medium | Required list absence fails explicitly; package verifies all three datasets |
| Third-party dictionary license not included in portable build | Medium | Verbatim MIT/notice import, separate CC BY-SA text, attribution/share-alike/change notes and all notices packaged |

The approval of an upstream keyboard component is not evidence that any focused control is safe. Current exclusion is deliberately universal: all controls are unsupported until an independently reviewed adapter can establish password/sensitivity/editability, default desktop, matching integrity and certain context. `SafetyPolicy` expresses these necessary facts, but it is not a Windows detector. No secure-field metadata probe or production span editor is represented as implemented. The disabled shipping state is the mitigation.

The tested engine accepts an `ICorrectionBoundary` only after it provides known context and independent eligibility. Before acceptance, a visible popup, replacement capability, exact expected span/UTF-16 length, nonnegative range start, matching revision ticket and unchanged identity/layout/generation are required. An adapter must atomically validate its revision/span at the actual mutation, not rely on this preflight alone. No real adapter currently satisfies this contract. Fakes prove coordinator behavior, not Windows editor correctness.

There is no SendInput call in the shipping application path, no clipboard API, cloud model, runtime network client or telemetry. Native interop declarations are retained for provenance/integration work; their existence does not enable them. Test-mode output contains only an explicit caller-supplied sample. The standard runtime emits no input/content diagnostics. Source changes never read window titles, clipboard data or private document contents.

Managed text may remain until garbage collection after references are cleared; memory zeroization is not claimed. Crash-dump policy, unsupported-provider timeouts, native event races and secure desktop/session change handling need review before live observation. This audit artifact is not suitable for normal correction use.

## Remaining Phase 1 blockers

- Implement an actual conservative Windows secure-control detector and an approved editor adapter. Reject browser/Electron/IME/terminal/elevated/remote/secure/unknown contexts unless separately proven safe.
- Integrate bounded event processing with generation/focus/layout/caret cancellation and exact-span validation. Do not enable a process-wide hook behind a preference/CLI bypass.
- Verify atomic replacement and readback, partial-failure behavior, undo, grapheme boundaries and layout-switch target/readback in a disposable editor.
- Record all interactive acceptance on real Windows 10/11, including password exclusions, normal Tab/Esc navigation and shutdown/leak behavior. **NOT RUN**.
- Complete product-owner licensing decision for original LAYFIX additions before a production release; preserve all upstream grants and dataset obligations meanwhile.

No production readiness, automatic merge, release, signing bypass, installer or Phase 2 is approved.
