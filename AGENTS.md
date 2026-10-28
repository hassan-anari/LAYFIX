# Codex Operating Contract — LAYFIX

This file is authoritative for autonomous coding agents working in this repository. Read the linked specifications before writing code. In case of conflict: security/privacy > correctness/data integrity > acceptance criteria > UX/performance > agent convenience.

## Scope and work authorization

1. Work only on the explicitly requested phase/task. Do not begin subsequent phases or opportunistically add unrelated features.
2. Start from the latest default branch and open/update a **Draft Pull Request** on a separate branch. Never push to main, merge, enable auto-merge, deploy, publish a release, install software on a user's machine, or issue real user input without an explicit instruction.
3. Preserve existing behavior and user settings. Breaking changes require migration, tests, and explicit disclosure.
4. Prefer small, reviewable commits and a final exact commit SHA. Never claim tests, compatibility, performance, or GUI functionality without actual evidence.
5. All source code, comments, docs, commit messages, PR descriptions, and CI logs intended for maintainers must be in English. The product's end-user UI must support English and Persian, including correct RTL layout.

## Technical baseline

- Phase 1 is the approved upstream Windows migration, based on sina-developer/LayoutFixer commit `969814d2dbb6819eaef984525ef11db4c3f48dca`. Reuse its C# detection and correction rules. Do not recreate the algorithm.
- Target Windows 10/11 x64, C# / .NET 10 and upstream Windows Forms tray/popup. Avalonia PR #2 is rejected; never use its implementation or branch as the base.
- `windows/LayoutFixer.Windows.csproj` is the shipping application; `Layfix.slnx` includes the two regression executables in `tests/`. Physical Windows boundaries and safety adapters must be injectable.
- The application includes an approved, privacy-restricted keyboard observation and correction component. Approval of this direction does not certify an editor as safe. The audit build deliberately installs no global hooks and enables no destructive replacement until a reviewed secure-control and atomic-span adapter passes acceptance. Never remove this gate just to demonstrate functionality.
- Preserve the upstream tray and popup. The final language switcher and extensive settings are subsequent stages and are outside this task.
- All core keyboard features work offline. No cloud model, API, telemetry, clipboard fallback, or input-content logging.
- Keep the UI and hook callbacks nonblocking; do not call UIA providers, load dictionaries, or perform edits in a hook callback. Bound volatile buffers and dispose all resources.
- Preserve the verbatim import as its own commit and record changed files separately. Keep MIT attribution and separate CC BY-SA 4.0 dataset notices in every artifact.

## Mandatory safety constraints

- Treat all keyboard events as highly sensitive. Never persist raw keystrokes, typed text, selected text, clipboard contents, credentials or window titles containing private content; no telemetry or networking for core behavior.
- No monitoring or correction in known password/secure fields, lock screen, elevated/secure desktops, remote session edge cases not verified, or an application the user excludes. If secure-context detection is inconclusive, **fail closed**: do not collect text or rewrite it.
- Prefer read-only detection and explicit confirmation. Never silently auto-rewrite by default.
- Verify the focused HWND/thread, input layout, editability, caret/selection and pending context immediately before any destructive edit. If the target has changed or cannot be verified, abort without modifying text.
- Suppress loops from synthesized input with a tagged injection/reentrancy strategy. Never globally consume Tab except when a visible, focused-context-valid suggestion is being accepted.
- Respect Windows integrity boundaries and UIPI. Do not use privileged injection, remote code, input hooks for other users, elevation prompts, accessibility exploits, or hacks to defeat OS input protections.
- Preserve clipboard ownership and contents; no clipboard fallback unless explicitly opted in, documented and exhaustively tested.
- Avoid global shortcuts that conflict with Windows/apps. Every shortcut must be configurable and escapable.

## Engineering discipline

- Design behavior and failure handling before implementation; cite the owning spec section in the PR.
- Add deterministic unit tests for conversion, normalization, boundaries, state machines, profile rules, settings migration, failures and concurrency.
- Add Windows integration tests where feasible. Mark tests requiring interactive desktop/manual input separately; do not report them as CI-passed if not run.
- Test both FA -> EN and EN -> FA including Persian Standard, punctuation, Shift, Caps Lock, physical key mappings, short/ambiguous words and Unicode (surrogate pairs, combining marks, RTL).
- Verify on a fresh Windows machine; publishing a self-contained artifact does not constitute UI/keyboard compatibility testing.
- No placeholder tests, unconditional green CI, ignored failures, unnecessary sleeps, blanket exception catches, or unpinned third-party runtime downloads.
- Logging is diagnostics-only and MUST NOT contain text, keycodes grouped into words, clipboard data, passwords, or window titles.
- No new dependency or copied source/dictionary/media without license review, attribution and a documented reason.

## CI and delivery

- On each PR, run the repository checks. CI MUST perform locked restore, Release build, run both real regression executables (JUnit XML), and publish an unsigned self-contained win-x64 portable folder/ZIP.
- Installer/release signing and publication are independent future gates. Do not represent an unsigned PR artifact as a signed production release.
- Never require .NET or developer tools on the end-user PC for the supported portable/installer outputs.
- Use least-privilege GitHub Actions permissions. Avoid untrusted PR scripts with write tokens, supply-chain bypasses and publishing from fork PRs.

## Required final report for every task

Provide: task scope, files changed, behavior implemented, exact commit SHA, Draft PR URL and merge state, commands executed, test counts and failed/skipped tests, GitHub Actions job URLs and status, downloadable artifact names if actually produced, known limitations, security/privacy implications, manual Windows validation still required and explicit out-of-scope items.

If blocked, give concrete evidence, fix what can be fixed safely and report what remains. Never make an unverified assertion of completion.

Read [product](docs/PRODUCT_SPEC.md), [architecture](docs/ARCHITECTURE.md), [quality gates](docs/QUALITY_GATES.md), [security](docs/SECURITY_PRIVACY.md) and [workflow](docs/DEVELOPMENT_WORKFLOW.md).
