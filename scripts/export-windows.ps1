[CmdletBinding()]
param(
    [string]$OutputDirectory = '.artifacts/PLAN-015/windows-x64',
    [Parameter(Mandatory)][string]$LogFile,
    [ValidateRange(1,1200)][int]$TimeoutSeconds = 1200
)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path -Parent $PSScriptRoot
$config = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'tool-paths.local.json') -Raw | ConvertFrom-Json
$godotPath = [IO.Path]::GetFullPath($config.godot, $taskRoot)
$dotnetPath = [IO.Path]::GetFullPath($config.dotnet, $taskRoot)
$outputPath = [IO.Path]::GetFullPath($OutputDirectory, $taskRoot)
$logPath = [IO.Path]::GetFullPath($LogFile, $taskRoot)
if (Test-Path -LiteralPath $logPath) { throw "Refusing to overwrite log: $logPath" }
$null = New-Item -ItemType Directory -Path $outputPath -Force
$null = New-Item -ItemType Directory -Path (Split-Path -Parent $logPath) -Force
$oldPath = $env:PATH
$oldDotnetRoot = $env:DOTNET_ROOT
$deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds)
function Invoke-Bounded([string]$Executable, [string[]]$Arguments) {
    $start = [Diagnostics.ProcessStartInfo]::new($Executable)
    $start.UseShellExecute = $false
    $start.CreateNoWindow = $true
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    $start.WorkingDirectory = $taskRoot
    foreach ($arg in $Arguments) { $start.ArgumentList.Add($arg) }
    $process = [Diagnostics.Process]::Start($start)
    try {
        $stdout = $process.StandardOutput.ReadToEndAsync()
        $stderr = $process.StandardError.ReadToEndAsync()
        $remaining = [Math]::Max(1, [int]($deadline - [DateTime]::UtcNow).TotalMilliseconds)
        if (-not $process.WaitForExit($remaining)) {
            $process.Kill($true)
            $process.WaitForExit()
            throw "Export exceeded $TimeoutSeconds seconds."
        }
        $result = $stdout.GetAwaiter().GetResult() + $stderr.GetAwaiter().GetResult()
        Add-Content -LiteralPath $logPath -Value $result
        if ($process.ExitCode -ne 0 -or $result -match '(?m)^(ERROR:|SCRIPT ERROR:)') {
            throw "Export command failed ($($process.ExitCode)); see $logPath"
        }
    } finally { $process.Dispose() }
}
try {
    $env:DOTNET_ROOT = Split-Path -Parent $dotnetPath
    $env:PATH = "$env:DOTNET_ROOT;$oldPath"
    # Publish the isolated Shell helper with its own runtime; never require a user's SDK.
    Invoke-Bounded $dotnetPath @('publish', (Join-Path $taskRoot 'tools/DesktopIconWorker/DesktopIconWorker.csproj'), '-c', 'Release', '-r', 'win-x64', '--self-contained', 'true', '-o', (Join-Path $outputPath 'IconWorker'))
    Invoke-Bounded $godotPath @('--headless', '--path', (Join-Path $taskRoot 'GodotProject'), '--export-release', 'Windows Desktop', (Join-Path $outputPath 'SiteDesktop.exe'))
    foreach ($file in @('SiteDesktop.exe', 'SiteDesktop.pck', 'IconWorker/DesktopIconWorker.exe', 'IconWorker/hostfxr.dll')) {
        if (-not (Test-Path -LiteralPath (Join-Path $outputPath $file))) { throw "Missing export output: $file" }
    }
    Add-Content -LiteralPath $logPath -Value "WINDOWS_EXPORT_PASS $outputPath"
    Write-Output "WINDOWS_EXPORT_PASS $outputPath"
} finally {
    $env:PATH = $oldPath
    $env:DOTNET_ROOT = $oldDotnetRoot
}
