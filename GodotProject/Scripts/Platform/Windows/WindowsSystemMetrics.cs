using System;
using System.Runtime.InteropServices;
using SiteDesktop.Simulation;

namespace SiteDesktop.Platform.Windows;

// Called sequentially on a worker task. Reads aggregate counters and the default output setting only.
public sealed class WindowsSystemMetrics
{
    [StructLayout(LayoutKind.Sequential)] private struct Memory
    { public uint Length, Load; public ulong TotalPhysical, AvailablePhysical, TotalPage, AvailablePage, TotalVirtual, AvailableVirtual, Extended; }
    [DllImport("kernel32.dll")] private static extern bool GetSystemTimes(out ulong idle, out ulong kernel, out ulong user);
    [DllImport("kernel32.dll")] private static extern bool GlobalMemoryStatusEx(ref Memory value);
    [DllImport("ole32.dll")] private static extern int CoInitializeEx(nint reserved, uint flags);
    [DllImport("ole32.dll")] private static extern void CoUninitialize();
    [DllImport("ole32.dll")] private static extern int CoCreateInstance(ref Guid clsid, nint outer, uint context, ref Guid iid, out nint result);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int DefaultDevice(nint self, int flow, int role, out nint device);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int Activate(nint self, ref Guid iid, uint context, nint parameters, out nint result);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int Scalar(nint self, out float value);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int Mute(nint self, out int value);
    private static T Method<T>(nint p, int slot) where T : Delegate => Marshal.GetDelegateForFunctionPointer<T>(Marshal.ReadIntPtr(Marshal.ReadIntPtr(p), slot * nint.Size));
    private ulong _idle, _total;
    private bool _primed;
    public SystemSample Read()
    {
        if (!OperatingSystem.IsWindows()) return default;
        float? cpu = null;
        if (GetSystemTimes(out var idle, out var kernel, out var user))
        {
            ulong total = kernel + user;
            if (_primed && total > _total && idle >= _idle) cpu = Math.Clamp(100f * (1f - (idle - _idle) / (float)(total - _total)), 0, 100);
            _idle = idle; _total = total; _primed = true;
        }
        var memory = new Memory { Length = (uint)Marshal.SizeOf<Memory>() };
        float? ram = GlobalMemoryStatusEx(ref memory) ? memory.Load : null;
        var (volume, muted) = Audio();
        return new(cpu, ram, volume, muted);
    }
    private static (float?, bool) Audio()
    {
        int initialized = CoInitializeEx(0, 0);
        if (initialized < 0) return (null, true);
        nint enumerator = 0, device = 0, endpoint = 0;
        try
        {
            var clsid = new Guid("BCDE0395-E52F-467C-8E3D-C4579291692E");
            var iid = new Guid("A95664D2-9614-4F35-A746-DE8DB63617E6");
            Marshal.ThrowExceptionForHR(CoCreateInstance(ref clsid, 0, 1, ref iid, out enumerator));
            Marshal.ThrowExceptionForHR(Method<DefaultDevice>(enumerator, 4)(enumerator, 0, 1, out device));
            iid = new("5CDF2C82-841E-4546-9722-0CF74078229A");
            Marshal.ThrowExceptionForHR(Method<Activate>(device, 3)(device, ref iid, 23, 0, out endpoint));
            Marshal.ThrowExceptionForHR(Method<Scalar>(endpoint, 9)(endpoint, out float level));
            Marshal.ThrowExceptionForHR(Method<Mute>(endpoint, 15)(endpoint, out int muted));
            return (level * 100, muted != 0);
        }
        catch (COMException) { return (null, true); }
        finally
        {
            if (endpoint != 0) Marshal.Release(endpoint);
            if (device != 0) Marshal.Release(device);
            if (enumerator != 0) Marshal.Release(enumerator);
            CoUninitialize();
        }
    }
}
