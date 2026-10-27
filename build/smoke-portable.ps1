$ErrorActionPreference = 'Stop'
# Start the published app with its real runtime and native dependencies, and exercise startup/teardown.
# No keyboard events, layout requests, or text input are generated. This does NOT prove interactive compatibility.
$directory = (Resolve-Path 'dist/win-x64').Path
$env:LAYFIX_SMOKE_OUTPUT = [System.IO.Path]::GetFullPath("dist/smoke")
$process = Start-Process -FilePath "$directory/Layfix.App.exe" -WorkingDirectory $directory -ArgumentList '--smoke-test' -PassThru
try {
    if (-not $process.WaitForExit(30000)) { throw 'Published app failed to finish its lifecycle smoke test within 30 seconds.' }
    if ($process.ExitCode -ne 0) { throw "Published app smoke test failed with exit code $($process.ExitCode)." }
} finally {
    if (-not $process.HasExited) { Stop-Process -Id $process.Id -Force }
    $process.Dispose()
    Remove-Item Env:LAYFIX_SMOKE_OUTPUT
}
Write-Host 'Self-contained app startup, widget HWND focus protection, native registrations and shutdown passed. Editor compatibility NOT RUN.'

$captures = @(Get-ChildItem 'dist/smoke/settings-*.png')
if ($captures.Count -ne 10) { throw "Expected 10 rendered Settings views, found $($captures.Count)." }
