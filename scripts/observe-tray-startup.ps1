[CmdletBinding()]
param(
    [string]$Executable = '.artifacts/PLAN-015/windows-x64/SiteDesktop.exe',
    [Parameter(Mandatory)][string]$EvidenceDirectory,
    [string]$MonitorId = 'GSM5BDE',
    [string]$SettingsFile,
    [string[]]$ExpectedActors = @('SCP-999','SCP-049','SCP-173')
)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path -Parent $PSScriptRoot
$output = [IO.Path]::GetFullPath($EvidenceDirectory, $taskRoot)
if (Test-Path -LiteralPath $output) { throw 'Use a new evidence directory.' }
$null = New-Item -ItemType Directory -Path $output
# Read-only Win32 observation: no focus changes, input injection, or external window capture.
Add-Type -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
public static class TrayStartupObserver {
    public delegate bool Visitor(IntPtr h, IntPtr p);
    [StructLayout(LayoutKind.Sequential)] public struct Rect { public int L,T,R,B; }
    [DllImport("user32.dll")] static extern bool EnumWindows(Visitor visitor, IntPtr parameter);
    [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);
    [DllImport("user32.dll", CharSet=CharSet.Unicode)] static extern int GetWindowText(IntPtr h, StringBuilder text, int size);
    [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr h);
    [DllImport("user32.dll")] static extern bool IsIconic(IntPtr h);
    [DllImport("user32.dll")] static extern int GetSystemMetrics(int index);
    [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr h, out Rect rect);
    [DllImport("user32.dll", EntryPoint="GetWindowLongPtrW")] static extern IntPtr GetWindowLongPtr(IntPtr h, int index);
    [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
    public sealed record Sample(string Title, bool Visible, bool Minimized, bool OnScreen, int X, int Y, int Width, int Height, long Style, bool Foreground);
    public static Sample[] Read(int target) {
        var result=new List<Sample>();
        EnumWindows((h,_)=>{
            GetWindowThreadProcessId(h,out var pid); if(pid!=target)return true;
            var title=new StringBuilder(256); GetWindowText(h,title,256);
            if(!title.ToString().StartsWith("SCP"))return true;
            GetWindowRect(h,out var r);
            int x=GetSystemMetrics(76),y=GetSystemMetrics(77),w=GetSystemMetrics(78),height=GetSystemMetrics(79);
            bool onScreen=r.R>x&&r.B>y&&r.L<x+w&&r.T<y+height;
            result.Add(new(title.ToString(),IsWindowVisible(h),IsIconic(h),onScreen,r.L,r.T,r.R-r.L,r.B-r.T,GetWindowLongPtr(h,-20).ToInt64(),h==GetForegroundWindow()));
            return true;
        },IntPtr.Zero);
        return result.ToArray();
    }
}
'@
$start = [Diagnostics.ProcessStartInfo]::new([IO.Path]::GetFullPath($Executable, $taskRoot))
$start.WorkingDirectory = Split-Path -Parent $start.FileName
$start.UseShellExecute = $false
$start.CreateNoWindow = $true
foreach ($arg in @('--max-fps','60','--quit-after','180','--log-file',(Join-Path $output 'execution.log'),'--','--monitor-id',$MonitorId)) { $start.ArgumentList.Add($arg) }
if ($SettingsFile) { $start.ArgumentList.Add('--scp-settings-file'); $start.ArgumentList.Add([IO.Path]::GetFullPath($SettingsFile,$taskRoot)) }
$process = [Diagnostics.Process]::Start($start)
$timer = [Diagnostics.Stopwatch]::StartNew()
$observations = [Collections.Generic.List[object]]::new()
try {
    while (-not $process.HasExited -and $timer.Elapsed.TotalSeconds -lt 30) {
        $observations.Add(@{ milliseconds=$timer.ElapsedMilliseconds; windows=[TrayStartupObserver]::Read($process.Id) })
        Start-Sleep -Milliseconds 10
    }
    if (-not $process.HasExited) { $process.Kill($true); $process.WaitForExit(); throw 'Observed product exceeded 30 seconds.' }
    $windows = @($observations | ForEach-Object { $_.windows })
    $controls = @($windows | Where-Object { $_.Title -like 'SCP: Site-Desktop*' })
    $visibleControls = @($controls | Where-Object Visible)
    # Godot/Windows clamps the empty transparent 1px boot surface to 64px before
    # C# starts. This is not the control panel. Keep the full-panel and focus
    # checks, and record every bootstrap observation rather than claiming no HWND.
    $bad = @($visibleControls | Where-Object { ($_.OnScreen -and -not $_.Minimized -and ($_.Width -gt 64 -or $_.Height -gt 64)) -or ($_.Style -band 0x08000000) -eq 0 -or $_.Foreground })
    $actors = @($windows | Where-Object { $_.Title -in @('SCP-999','SCP-049','SCP-173') -and $_.Visible })
    $log = Get-Content -LiteralPath (Join-Path $output 'execution.log') -Raw
    $observedActors = @($actors.Title | Sort-Object -Unique)
    $actorMatch = @($observedActors | Where-Object { $_ -notin $ExpectedActors }).Count -eq 0 -and @($ExpectedActors | Where-Object { $_ -notin $observedActors }).Count -eq 0
    $passed = $process.ExitCode -eq 0 -and $controls.Count -gt 0 -and $actorMatch -and $bad.Count -eq 0 -and $log.Contains('SITE_DESKTOP_READY') -and $log.Contains('SITE_DESKTOP_STOPPED') -and $log -notmatch '(?m)^(ERROR:|SCRIPT ERROR:)'
    @{passed=$passed;pid=$process.Id;exitCode=$process.ExitCode;durationMs=$timer.ElapsedMilliseconds;samples=$observations.Count;controlSamples=$controls.Count;visibleControlSamples=$visibleControls.Count;unexpectedVisibleControls=$bad.Count;visibleActorSamples=$actors.Count;observations=$observations} | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $output 'results.json')
    if (-not $passed) { throw 'Tray startup observation failed; inspect results.json.' }
    Write-Output "TRAY_STARTUP_PASS samples=$($observations.Count) controls=$($controls.Count) actors=$($actors.Count) unexpected=$($bad.Count) exit=$($process.ExitCode)"
} finally { $process.Dispose() }
