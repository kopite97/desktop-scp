using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace SiteDesktop.Platform.Windows;
// Closing the parent process also closes this handle, so a Shell worker cannot survive it.
internal sealed class WorkerJob : SafeHandleZeroOrMinusOneIsInvalid
{
    [StructLayout(LayoutKind.Sequential)] private struct Basic
    { public long ProcessTime,JobTime;public uint Flags;public nuint Minimum,Maximum;public uint Active;public nuint Affinity;public uint Priority,Scheduling; }
    [StructLayout(LayoutKind.Sequential)] private struct Io
    { public ulong ReadOps,WriteOps,OtherOps,ReadBytes,WriteBytes,OtherBytes; }
    [StructLayout(LayoutKind.Sequential)] private struct Limits
    { public Basic Basic;public Io Io;public nuint ProcessMemory,JobMemory,PeakProcess,PeakJob; }
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] private static extern nint CreateJobObject(nint attributes,string? name);
    [DllImport("kernel32.dll",SetLastError=true)] private static extern bool SetInformationJobObject(nint job,int info,ref Limits value,uint length);
    [DllImport("kernel32.dll",SetLastError=true)] private static extern bool AssignProcessToJobObject(nint job,nint process);
    [DllImport("kernel32.dll")] private static extern bool CloseHandle(nint value);
    public WorkerJob(nint process):base(true)
    {
        SetHandle(CreateJobObject(0,null));
        var limits=new Limits{Basic=new Basic{Flags=0x2000}};
        if(IsInvalid||!SetInformationJobObject(handle,9,ref limits,(uint)Marshal.SizeOf<Limits>())||!AssignProcessToJobObject(handle,process))
        {int error=Marshal.GetLastWin32Error();Dispose();throw new Win32Exception(error);}
    }
    protected override bool ReleaseHandle()=>CloseHandle(handle);
}
