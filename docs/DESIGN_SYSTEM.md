# Visual Design and Customization Contract

## Design intent

LAYFIX should feel integrated with Windows, not like a generic web wrapper. Use a calm, translucent Fluent-inspired aesthetic with clear keyboard state and fast tactile feedback. Filee informs technical patterns, but do not clone its assets, icons or identity.

## Theme tokens

- Theme mode: System (default), Light, Dark.
- Accent: follow Windows system accent (default), or user custom accent selected with an accessible color picker and reset.
- Surface: Mica where supported (primarily Windows 11 settings), acrylic/translucent for suitable popups and the floating widget; opaque Fluent-like fallback for Windows 10, high contrast, disabled transparency, weak composition or remote sessions.
- Tokens include accent, accent foreground, surface, elevated surface, outline, primary/secondary text, success/warning/error, shadow, radii, spacing, typography, opacity, animation.
- Do not hard-code default branding purple: sample accent values may be used only in mockups. React to Windows theme/accent changes at runtime.
- Respect per-monitor scaling and accessibility font/text scaling; do not force glass if contrast becomes insufficient.

## Floating widget

- Compact FA / EN pill, clearly active segment; optional single-language monogram mode.
- Draggable with a bounded on-screen position; screen-edge snap optional; remembers per-monitor or normalized position safely.
- Pointer click requests target layout change without stealing the active text field's keyboard focus.
- Unavailable/unknown language has a distinct neutral state; failed switch should show non-invasive feedback.
- Options: visibility, opacity, size, always-on-top, mouse hover, lock position, hide in fullscreen, show on change, click behavior and per-app exclusion.
- A tray icon always permits restore if the widget becomes hidden or inaccessible.

## Suggestion popup

- Visually separate from permanent widget; tiny, non-activating popover near caret.
- Shows preview in the target script (proper font shaping and RTL), target language and confirmation hint.
- Respect app boundaries, taskbar edges and monitor bounds. If caret geometry unavailable, use unobtrusive fallback, not a random overlay obscuring input.
- Never steal focus or intercept Tab when no eligible suggestion is active.
- Support reduced animation and accessibility; avoid constantly shifting positions while user types.

## Settings

- Single window, intuitive sections: General, Widget, Appearance, Correction, Keyboard, Applications, Privacy & Diagnostics, About.
- User-facing copy available in English and Persian; consistent RTL mirroring for Persian settings UI, with keyboard names and code samples treated deliberately.
- Live previews for accent, opacity, radius, scale and switcher mode. Changes persist atomically and reset per section.
- Keyboard shortcuts display real registered combinations with conflict/error feedback.
- No settings UX may imply that accessibility/OS switching APIs guarantee universal app coverage.

## Quality requirements

- AA-like contrast target for legibility, with conservative opaque fallback as needed.
- Large enough hit targets for pointer/touch, keyboard navigation and visible focus indicators in settings.
- No focus theft, unexpected overlays or animations that interfere with text typing.
- Validate on Win10/Win11, default and custom accent, dark/light/high contrast, scale 100%/150%/200%, multi-monitor and remote desktop (as supported).
