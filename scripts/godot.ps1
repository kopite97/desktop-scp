[CmdletBinding()]
param(
    [ValidateSet('Info', 'Build', 'Import', 'Run', 'Editor', 'Smoke', 'OverlaySmoke', 'InputProbe', 'LightingProbe')]
    [string]$Action = 'Info',
    [switch]$Headless,
    [string]$LogFile,
    [ValidateRange(1, 300)][int]$TimeoutSeconds = 60,
    [ValidateSet('r002','r003','r004','r005-sample','r005')][string]$Revision = 'r002',
    [string]$EvidenceDirectory,
    [switch]$Preview,
    [switch]$Measure,
    [switch]$MeasureOnly,
    [switch]$PlaybackOnly
)

$ErrorActionPreference = 'Stop'
if ($Action -eq 'LightingProbe' -and ($Headless -or -not $EvidenceDirectory -or -not $LogFile)) { throw 'LightingProbe requires a Windows display, -EvidenceDirectory and -LogFile.' }
if ($Action -in @('Smoke', 'OverlaySmoke') -and -not $LogFile) { throw 'Smoke requires -LogFile to verify lifecycle markers.' }
$taskRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $taskRoot 'GodotProject'
$configFile = Join-Path $PSScriptRoot 'tool-paths.local.json'
if (-not (Test-Path -LiteralPath $configFile)) {
    throw 'Create scripts/tool-paths.local.json from tool-paths.example.json with verified tool paths.'
}
$config = Get-Content -LiteralPath $configFile -Raw | ConvertFrom-Json
function Resolve-Tool([string]$Value) {
    if ([string]::IsNullOrWhiteSpace($Value)) { throw 'Tool path is empty.' }
    $resolved = if ([IO.Path]::IsPathRooted($Value)) { $Value } else { Join-Path $taskRoot $Value }
    if (-not (Test-Path -LiteralPath $resolved -PathType Leaf)) { throw "Tool not found: $resolved" }
    return [IO.Path]::GetFullPath($resolved)
}
$godotExe = Resolve-Tool $config.godot
$dotnetExe = Resolve-Tool $config.dotnet
$oldPath = $env:PATH
$oldDotnetRoot = $env:DOTNET_ROOT
$oldTelemetry = $env:DOTNET_CLI_TELEMETRY_OPTOUT
try {
    $env:DOTNET_ROOT = Split-Path -Parent $dotnetExe
    $env:PATH = "$env:DOTNET_ROOT;$oldPath"
    $env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
    Push-Location $projectPath
    try {
        if ($Action -eq 'Info') {
            & $godotExe --version
            if ($LASTEXITCODE -ne 0) { throw "Godot version failed: $LASTEXITCODE" }
            & $dotnetExe --info
            if ($LASTEXITCODE -ne 0) { throw "dotnet info failed: $LASTEXITCODE" }
            return
        }
        if ($Action -eq 'Build') {
            & $dotnetExe build 'SiteDesktop.csproj' --nologo
            if ($LASTEXITCODE -ne 0) { throw "C# build failed: $LASTEXITCODE" }
            return
        }

        $godotArgs = @('--path', $projectPath)
        if ($Headless -or $Action -eq 'Import') { $godotArgs += '--headless' }
        switch ($Action) {
            'Import' { $godotArgs += @('--editor', '--import') }
            'Editor' { $godotArgs += '--editor' }
            'Smoke' { $godotArgs += @('--scene', 'res://Scenes/Diagnostics/EnvironmentSmoke.tscn', '--max-fps', '60') }
            'OverlaySmoke' { $godotArgs += @('--scene', 'res://Scenes/Diagnostics/OverlaySmoke.tscn', '--max-fps', '60') }
            'InputProbe' { $godotArgs += @('--scene', 'res://Scenes/Diagnostics/InputProbe.tscn', '--max-fps', '60') }
            'LightingProbe' { $godotArgs += @('--scene', 'res://Scenes/Diagnostics/Scp999LightingProbe.tscn', '--max-fps', '60') }
        }
        if ($LogFile) {
            $resolvedLog = [IO.Path]::GetFullPath($LogFile, $taskRoot)
            if (Test-Path -LiteralPath $resolvedLog) { throw "Refusing to overwrite a log: $resolvedLog" }
            $null = New-Item -ItemType Directory -Path (Split-Path -Parent $resolvedLog) -Force
            $godotArgs += @('--log-file', $resolvedLog)
        }
        if ($Action -eq 'LightingProbe') {
            $evidencePath = [IO.Path]::GetFullPath($EvidenceDirectory, $taskRoot)
            $godotArgs += @('--', '--revision', $Revision, '--output', $evidencePath)
            if ($Preview) { $godotArgs += '--preview' }
            if ($Measure) { $godotArgs += '--measure' }
            if ($MeasureOnly) { $godotArgs += '--measure-only' }
            if ($PlaybackOnly) { $godotArgs += '--playback-only' }
        }
        if ($Action -in @('Run', 'Editor', 'InputProbe') -or ($Action -eq 'LightingProbe' -and $Preview)) {
            & $godotExe @godotArgs
            if ($LASTEXITCODE -ne 0) { throw "Godot $Action failed: $LASTEXITCODE" }
        } else {
            # Use ArgumentList to preserve spaces. Bound unattended verification time.
            $startInfo = [Diagnostics.ProcessStartInfo]::new($godotExe)
            $startInfo.UseShellExecute = $false
            $startInfo.CreateNoWindow = $true
            foreach ($argument in $godotArgs) { $startInfo.ArgumentList.Add($argument) }
            $process = [Diagnostics.Process]::Start($startInfo)
            try {
                if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
                    $process.Kill($true)
                    $process.WaitForExit()
                    throw "Godot $Action timed out after $TimeoutSeconds seconds."
                }
                if ($process.ExitCode -ne 0) { throw "Godot $Action failed: $($process.ExitCode)" }
            } finally { $process.Dispose() }
            if ($Action -in @('Smoke', 'OverlaySmoke')) {
                $log = Get-Content -LiteralPath $resolvedLog -Raw
                $passMarker = if ($Action -eq 'OverlaySmoke') { 'OVERLAY_SMOKE_PASS' } else { 'ENVIRONMENT_SMOKE_PASS' }
                foreach ($marker in @('SITE_DESKTOP_READY', $passMarker, 'SITE_DESKTOP_EXIT_REQUESTED', 'SITE_DESKTOP_STOPPED')) {
                    if (-not $log.Contains($marker)) { throw "Missing lifecycle marker: $marker" }
                }
                if ($log -match '(?m)^(ERROR:|SCRIPT ERROR:|.*(?:ENVIRONMENT|OVERLAY)_SMOKE_FAILED)') {
                    throw 'Godot smoke log contains an error.'
                }
            }
            if ($Action -eq 'LightingProbe') {
                $log = Get-Content -LiteralPath $resolvedLog -Raw
                if (-not $log.Contains('LIGHTING_CAPTURE_COMPLETE') -or $log -match '(?m)^(ERROR:|SCRIPT ERROR:)') { throw 'Lighting probe failed or its log contains errors.' }
            }
        }
    } finally { Pop-Location }
} finally {
    $env:PATH = $oldPath
    $env:DOTNET_ROOT = $oldDotnetRoot
    $env:DOTNET_CLI_TELEMETRY_OPTOUT = $oldTelemetry
}
