# Packaging, Signing and Releases

## Status

Initial repo contains only specifications and CI bootstrap. No executable, installer, signing key or GitHub Release is produced yet.

## Phase 1 distribution (unsigned preview)

Windows GitHub Actions builds a .NET 10 self-contained, non-trimmed win-x64 portable package from **src/Layfix.App/Layfix.App.csproj**, after restore/build/test; packaging must include native Avalonia dependencies. Upload ZIP as a PR/run artifact. CI runners need SDK; end-user machine does not.

Do not assert a single physical EXE is always viable until Avalonia/native dependency packaging is validated. Use self-contained folder + ZIP as the reliable baseline; single-file publishing may be evaluated as an optimization.

## Phase 4 distribution (release gate)

- Approved semantic version tag, immutable SHA, release notes/changelog, lockfile/manifest and verified dependencies.
- Produce win-x64 portable archive and Setup.exe with uninstall support. Add Windows ARM64 only after explicit support/test sign-off.
- Generate SHA-256 checksums and, if approved, SBOM and provenance.
- Review signing strategy; use protected release secrets only from trusted workflows. Never expose certificates or private keys to PR jobs.
- Validate Windows SmartScreen/signature status honestly. Unsigned test artifacts must be marked unsigned.
- Test clean install/upgrade/uninstall without deleting user data unexpectedly; system startup remains opt-in.
- User approval required for publishing release, auto-update configuration and installer distribution.

## Release invariants

No automatic merge, no tag/release from unreviewed PR, no tests silently skipped, and no installer until native end-to-end validation. A successful build is not proof of usability or safe edit behavior.
