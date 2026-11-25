[CmdletBinding()]
param(
    [ValidateSet('Info', 'Build', 'Import', 'Run', 'Editor', 'Smoke', 'OverlaySmoke', 'InputProbe', 'LightingProbe', 'Scp999Smoke', 'PlatformSmoke', 'PlatformPreview', 'Scp999Color', 'Scp049Smoke','OverlayStackSmoke','DesktopIconSmoke','StatueSmoke','SystemSmoke','DoctorArtSmoke','ZombieArtSmoke','ScpActivationSmoke','BehaviorSmoke','MonitorPortalSmoke')]
    [string]$Action = 'Info',
    [switch]$Headless,
    [string]$LogFile,
    [ValidateRange(1, 300)][int]$TimeoutSeconds = 60,
    [ValidateSet('r002','r003','r004','r005-sample','r005')][string]$Revision = 'r002',
    [string]$EvidenceDirectory,
    [switch]$Preview,
    [switch]$Measure,
    [switch]$Baseline,
    [switch]$MeasureOnly,
    [switch]$PlaybackOnly,
    [switch]$NativeInput,
    [string]$MonitorId
)

$ErrorActionPreference = 'Stop'
if ($Action -eq 'OverlayStackSmoke' -and $Headless) { throw 'OverlayStackSmoke requires a Windows display.' }
if ($NativeInput -and ($Action -notin @('Scp999Smoke','Scp049Smoke','OverlayStackSmoke','DesktopIconSmoke','StatueSmoke','SystemSmoke','DoctorArtSmoke','ZombieArtSmoke','ScpActivationSmoke','MonitorPortalSmoke') -or $Headless)) { throw 'NativeInput requires Scp999Smoke, Scp049Smoke or OverlayStackSmoke on a Windows display.' }
if ($Action -eq 'LightingProbe' -and ($Headless -or -not $EvidenceDirectory -or -not $LogFile)) { throw 'LightingProbe requires a Windows display, -EvidenceDirectory and -LogFile.' }
if ($Action -in @('Smoke', 'OverlaySmoke') -and -not $LogFile) { throw 'Smoke requires -LogFile to verify lifecycle markers.' }
if ($Action -in @('Scp999Smoke','Scp049Smoke','OverlayStackSmoke','DesktopIconSmoke','StatueSmoke','SystemSmoke','DoctorArtSmoke','ZombieArtSmoke','ScpActivationSmoke','MonitorPortalSmoke') -and (-not $EvidenceDirectory -or -not $LogFile)) { throw 'Companion diagnostics require -EvidenceDirectory and -LogFile.' }
if ($Action -eq 'PlatformSmoke' -and (-not $EvidenceDirectory -or -not $LogFile)) { throw 'PlatformSmoke requires -EvidenceDirectory and -LogFile.' }
if ($Action -eq 'Scp999Color' -and ($Headless -or -not $EvidenceDirectory -or -not $LogFile)) { throw 'Scp999Color requires display, evidence directory and log.' }
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
# A task preview is another process: its actor windows can intercept synthetic input.
# Do not silently close it or let a diagnostic click a different instance.
if (-not $Headless -and $Action -in @('Scp049Smoke','Scp999Smoke','PlatformSmoke','OverlayStackSmoke')) {
    $taskPreviews = @(Get-CimInstance Win32_Process | Where-Object {
        $_.Name -like 'Godot*' -and $_.CommandLine -like "*$projectPath*" -and $_.CommandLine -like '*live-preview.log*'
    })
    if ($taskPreviews.Count -gt 0) { throw 'A task preview is still running. Close that preview before native diagnostics.' }
}
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
            'MonitorPortalSmoke' { $godotArgs += @('--scene', 'res://Scenes/Diagnostics/MonitorPortalSmoke.tscn', '--max-fps', '60') }
            'BehaviorSmoke' { $godotArgs += @('--scene', 'res://Scenes/Diagnostics/BehaviorSmoke.tscn', '--max-fps', '60') }
            'ScpActivationSmoke' { $godotArgs += @('--scene', 'res://Scenes/Diagnostics/ScpActivationSmoke.tscn', '--max-fps', '60') }
            'ZombieArtSmoke' { $godotArgs += @('--scene', 'res://Scenes/Diagnostics/ZombieArtSmoke.tscn', '--max-fps', '60') }
            'DoctorArtSmoke' { $godotArgs += @('--scene', 'res://Scenes/Diagnostics/DoctorArtSmoke.tscn', '--max-fps', '60') }
            'SystemSmoke' { $godotArgs += @('--scene', 'res://Scenes/Diagnostics/SystemSmoke.tscn', '--max-fps', '60') }
            'StatueSmoke' { $godotArgs += @('--scene', 'res://Scenes/Diagnostics/StatueSmoke.tscn', '--max-fps', '60') }
            'DesktopIconSmoke' { $godotArgs += @('--scene', 'res://Scenes/Diagnostics/DesktopIconSmoke.tscn', '--max-fps', '60') }
            'OverlayStackSmoke' { $godotArgs += @('--scene', 'res://Scenes/Diagnostics/OverlayStackSmoke.tscn', '--max-fps', '60') }
            'Scp049Smoke' { $godotArgs += @('--scene', 'res://Scenes/Diagnostics/Scp049Smoke.tscn', '--max-fps', '60') }
            'Scp999Smoke' { $godotArgs += @('--scene', 'res://Scenes/Diagnostics/Scp999Smoke.tscn', '--max-fps', '60') }
            'PlatformSmoke' { $godotArgs += @('--scene', 'res://Scenes/Diagnostics/PlatformSmoke.tscn', '--max-fps', '60') }
            'PlatformPreview' { $godotArgs += @('--scene', 'res://Scenes/App/PlatformDesktop.tscn', '--max-fps', '60') }
            'Scp999Color' { $godotArgs += @('--scene', 'res://Scenes/Diagnostics/Scp999ColorProbe.tscn', '--max-fps', '60') }
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
        if ($Action -in @('Scp999Smoke','Scp049Smoke','OverlayStackSmoke','DesktopIconSmoke','StatueSmoke','SystemSmoke','DoctorArtSmoke','ZombieArtSmoke','ScpActivationSmoke')) {
            $godotArgs += @('--', '--output', [IO.Path]::GetFullPath($EvidenceDirectory, $taskRoot))
            if ($NativeInput) { $godotArgs += '--native-input' }
        }
        if ($Action -in @('PlatformSmoke','Scp999Color')) { $godotArgs += @('--', '--output', [IO.Path]::GetFullPath($EvidenceDirectory, $taskRoot)) }
        if ($Action -in @('BehaviorSmoke','MonitorPortalSmoke')) {
            if (-not $EvidenceDirectory -or -not $LogFile) { throw 'BehaviorSmoke requires evidence and log.' }
            $godotArgs += @('--', '--output', [IO.Path]::GetFullPath($EvidenceDirectory, $taskRoot))
            if ($Baseline) { $godotArgs += '--baseline' }
            if ($NativeInput -and $Action -eq 'MonitorPortalSmoke') { $godotArgs += '--native-input' }
            if ($Measure) { $godotArgs += '--measure' }
        }
        if ($MonitorId) {
            if ($godotArgs -notcontains '--') { $godotArgs += '--' }
            $godotArgs += @('--monitor-id', $MonitorId)
        }
        if ($Action -in @('Run', 'Editor', 'InputProbe', 'PlatformPreview') -or ($Action -eq 'LightingProbe' -and $Preview)) {
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
            if ($Action -in @('Scp999Smoke','Scp049Smoke')) {
                $log = Get-Content -LiteralPath $resolvedLog -Raw
                $scpMarker = if ($Action -eq 'Scp049Smoke') { 'SCP049' } else { 'SCP999' }
                foreach ($marker in @('SITE_DESKTOP_READY',"${scpMarker}_SMOKE_PASS","${scpMarker}_STOPPED",'SITE_DESKTOP_STOPPED')) { if (-not $log.Contains($marker)) { throw "Missing lifecycle marker: $marker" } }
                if ($log -match '(?m)^(ERROR:|SCRIPT ERROR:)') { throw "$Action smoke log contains errors." }
            }
            if ($Action -in @('BehaviorSmoke','MonitorPortalSmoke')) {
                $log = Get-Content -LiteralPath $resolvedLog -Raw
                if (-not $log.Contains($(if ($Action -eq 'MonitorPortalSmoke') {'PORTAL_PASS'} else {'BEHAVIOR_PASS'})) -or $log -match '(?m)^(ERROR:|SCRIPT ERROR:)') { throw 'Behavior diagnostic failed.' }
            }
            if ($Action -eq 'ZombieArtSmoke') {
                $log = Get-Content -LiteralPath $resolvedLog -Raw
                if (-not $log.Contains('ZOMBIE_ART_PASS') -or $log -match '(?m)^(ERROR:|SCRIPT ERROR:)') { throw 'Zombie art diagnostic failed.' }
            }
            if ($Action -eq 'DoctorArtSmoke') {
                $log = Get-Content -LiteralPath $resolvedLog -Raw
                if (-not $log.Contains('DOCTOR_ART_PASS') -or $log -match '(?m)^(ERROR:|SCRIPT ERROR:)') { throw 'Doctor art diagnostic failed.' }
            }
            if ($Action -eq 'ScpActivationSmoke') {
                $log = Get-Content -LiteralPath $resolvedLog -Raw
                if (-not $log.Contains('ACTIVATION_PASS') -or $log -match '(?m)^(ERROR:|SCRIPT ERROR:)') { throw 'Activation diagnostic failed.' }
            }
            if ($Action -eq 'SystemSmoke') {
                $log = Get-Content -LiteralPath $resolvedLog -Raw
                if (-not $log.Contains('SYSTEM_PASS') -or $log -match '(?m)^(ERROR:|SCRIPT ERROR:)') { throw 'System diagnostic failed.' }
            }
            if ($Action -eq 'StatueSmoke') {
                $log = Get-Content -LiteralPath $resolvedLog -Raw
                if (-not $log.Contains('STATUE_PASS') -or $log -match '(?m)^(ERROR:|SCRIPT ERROR:)') { throw 'Statue diagnostic failed.' }
            }
            if ($Action -eq 'DesktopIconSmoke') {
                $log = Get-Content -LiteralPath $resolvedLog -Raw
                if (-not $log.Contains('DESKTOP_ICON_PASS') -or $log -match '(?m)^(ERROR:|SCRIPT ERROR:)') { throw 'Desktop icon diagnostic failed.' }
            }
            if ($Action -eq 'OverlayStackSmoke') {
                $log = Get-Content -LiteralPath $resolvedLog -Raw
                if (-not $log.Contains('OVERLAY_STACK_PASS') -or $log -match '(?m)^(ERROR:|SCRIPT ERROR:)') { throw 'Overlay stack diagnostic failed.' }
            }
            if ($Action -eq 'PlatformSmoke') {
                $log = Get-Content -LiteralPath $resolvedLog -Raw
                foreach ($marker in @('PLATFORM_SMOKE_PASS','PLATFORM_STOPPED')) { if (-not $log.Contains($marker)) { throw "Missing lifecycle marker: $marker" } }
                if ($log -match '(?m)^(ERROR:|SCRIPT ERROR:)') { throw 'Platform smoke log contains errors.' }
            }
            if ($Action -eq 'Scp999Color') {
                $log = Get-Content -LiteralPath $resolvedLog -Raw
                if (-not $log.Contains('SCP999_COLOR_PASS') -or $log -match '(?m)^(ERROR:|SCRIPT ERROR:)') { throw 'Color comparison failed.' }
            }
        }
    } finally { Pop-Location }
} finally {
    $env:PATH = $oldPath
    $env:DOTNET_ROOT = $oldDotnetRoot
    $env:DOTNET_CLI_TELEMETRY_OPTOUT = $oldTelemetry
}
