$ErrorActionPreference = 'Stop'
# Start the published app with its real runtime and native dependencies, and exercise startup/teardown.
# No keyboard events, layout requests, or text input are generated. This does NOT prove interactive compatibility.
$directory = (Resolve-Path 'dist/win-x64').Path
$process = Start-Process -FilePath "$directory/Layfix.App.exe" -WorkingDirectory $directory -ArgumentList '--smoke-test' -PassThru
try {
    if (-not $process.WaitForExit(30000)) { throw 'Published app failed to finish its lifecycle smoke test within 30 seconds.' }
    if ($process.ExitCode -ne 0) { throw "Published app smoke test failed with exit code $($process.ExitCode)." }
} finally {
    if (-not $process.HasExited) { Stop-Process -Id $process.Id -Force }
    $process.Dispose()
}
Write-Host 'Self-contained app startup, widget HWND focus protection, native registrations and shutdown passed. Editor compatibility NOT RUN.'
