$ErrorActionPreference = 'Stop'
$directory = 'dist/win-x64'
dotnet publish src/Layfix.App/Layfix.App.csproj --configuration Release --runtime win-x64 --self-contained true --no-restore -p:PublishTrimmed=false -p:PublishSingleFile=false --output $directory
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
foreach ($name in @('Layfix.App.exe', 'Layfix.App.dll', 'Layfix.App.deps.json', 'Layfix.App.runtimeconfig.json', 'coreclr.dll', 'hostfxr.dll', 'hostpolicy.dll', 'System.Private.CoreLib.dll', 'libSkiaSharp.dll', 'libHarfBuzzSharp.dll', 'av_libglesv2.dll', 'THIRD_PARTY_NOTICES.md', 'PREVIEW.txt')) {
    if (-not (Test-Path "$directory/$name")) { throw "Portable package is missing $name" }
}
if (-not (Test-Path "$directory/ThirdPartyLicenses")) { throw 'Portable package is missing dependency licenses.' }
$runtime = Get-Content "$directory/Layfix.App.runtimeconfig.json" | ConvertFrom-Json
if ($null -ne $runtime.runtimeOptions.framework -or $null -ne $runtime.runtimeOptions.frameworks) { throw 'Package unexpectedly requires an installed shared runtime.' }
Compress-Archive -Path "$directory/*" -DestinationPath 'dist/LAYFIX-win-x64-portable.zip' -Force
(Get-FileHash 'dist/LAYFIX-win-x64-portable.zip' -Algorithm SHA256).Hash | Out-File 'dist/LAYFIX-win-x64-portable.sha256' -Encoding ascii
