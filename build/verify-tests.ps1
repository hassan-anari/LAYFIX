$ErrorActionPreference = 'Stop'
$files = @(Get-ChildItem 'tests' -Recurse -Filter '*.trx')
if ($files.Count -ne 2) { throw "Expected one TRX result for each test project, found $($files.Count)." }
$total = 0
foreach ($file in $files) {
    [xml]$result = Get-Content $file.FullName
    $counters = $result.TestRun.ResultSummary.Counters
    if ([int]$counters.total -le 0 -or [int]$counters.executed -ne [int]$counters.total -or [int]$counters.passed -ne [int]$counters.total) {
        throw "Tests missing, skipped, or failing: $($file.FullName)"
    }
    $total += [int]$counters.total
    Write-Host "$($file.Directory.Parent.Name): $($counters.passed)/$($counters.total) passed"
}
"Automated tests: $total passed; no failures or skips. Interactive desktop validation is NOT RUN." | Out-File -FilePath $env:GITHUB_STEP_SUMMARY -Append -Encoding utf8
