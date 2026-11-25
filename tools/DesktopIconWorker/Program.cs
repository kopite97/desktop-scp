using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]

// Short-lived STA isolation: the parent can terminate a stuck Shell extension.
internal static class Program
{
    [STAThread] private static int Main()
    {
        try { Console.Write(JsonSerializer.Serialize(DesktopIcons.Read())); return 0; }
        catch (Exception ex) { Console.Error.Write($"shell-error:0x{ex.HResult:X8}"); return 1; }
    }
}

internal record IconImage(string Id, int Width, int Height, string Kind, byte[] Rgba);
internal static class DesktopIcons
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int QueryService(nint self, ref Guid service, ref Guid iid, out nint result);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int GetPointer(nint self, out nint result);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int Items(nint self, uint flags, ref Guid iid, out nint result);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int GetCount(nint self, out uint count);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int GetItem(nint self, uint index, out nint item);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int GetImage(nint self, Size size, uint flags, out nint bitmap);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int Attributes(nint self, uint mask, out uint attributes);
    [StructLayout(LayoutKind.Sequential)] private struct Size { public int X,Y; }
    [StructLayout(LayoutKind.Sequential)] private struct BitmapHeader
    { public uint Size; public int Width,Height; public ushort Planes,Bits; public uint Compression,ImageSize; public int X,Y; public uint Used,Important; }
    [StructLayout(LayoutKind.Sequential)] private struct Bitmap
    { public int Type,Width,Height,WidthBytes; public ushort Planes,BitsPixel; public nint Bits; }
    [DllImport("gdi32.dll")] private static extern bool DeleteObject(nint obj);
    [DllImport("gdi32.dll",EntryPoint="GetObjectW")] private static extern int GetObject(nint obj,int size,out Bitmap bitmap);
    [DllImport("gdi32.dll")] private static extern int GetDIBits(nint dc,nint bitmap,uint start,uint lines,[Out]byte[] bits,ref BitmapHeader info,uint usage);
    [DllImport("user32.dll")] private static extern nint GetDC(nint window);
    [DllImport("user32.dll")] private static extern int ReleaseDC(nint window,nint dc);
    private static T Method<T>(nint instance,int slot) where T:Delegate => Marshal.GetDelegateForFunctionPointer<T>(Marshal.ReadIntPtr(Marshal.ReadIntPtr(instance),slot*nint.Size));
    private static nint Interface(nint value,string id) { var iid=new Guid(id);Marshal.ThrowExceptionForHR(Marshal.QueryInterface(value,ref iid,out var result));return result; }
    private static void Release(ref nint value) { if(value!=0){Marshal.Release(value);value=0;} }
    public static List<IconImage> Read()
    {
        object? windows=null,desktop=null;
        nint unknown=0,provider=0,browser=0,view=0,folderView=0,array=0;
        var images=new List<IconImage>();
        try
        {
            windows=Activator.CreateInstance(Type.GetTypeFromCLSID(new("9BA05972-F6A8-11CF-A442-00A0C90A8F39"),true)!);
            object location=0,unused=Type.Missing;
            desktop=((dynamic)windows!).FindWindowSW(ref location,ref unused,8,out int hwnd,1);
            unknown=Marshal.GetIUnknownForObject(desktop!);
            provider=Interface(unknown,"6D5140C1-7436-11CE-8034-00AA006009FA");
            var service=new Guid("4C96BE40-915C-11CF-99D3-00AA004AE837");var browserId=new Guid("000214E2-0000-0000-C000-000000000046");
            Marshal.ThrowExceptionForHR(Method<QueryService>(provider,3)(provider,ref service,ref browserId,out browser));
            Marshal.ThrowExceptionForHR(Method<GetPointer>(browser,15)(browser,out view));
            folderView=Interface(view,"CDE725B0-CCC9-4519-917E-325D72FAB4CE");
            var arrayId=new Guid("B63EA76D-1F85-456F-A19C-48159EFA858B");
            Marshal.ThrowExceptionForHR(Method<Items>(folderView,8)(folderView,2,ref arrayId,out array));
            Marshal.ThrowExceptionForHR(Method<GetCount>(array,7)(array,out uint count));
            for(uint i=0;i<Math.Min(count,64)&&images.Count<32;i++)
            {
                nint item=0,factory=0,bitmap=0;
                try
                {
                    Marshal.ThrowExceptionForHR(Method<GetItem>(array,8)(array,i,out item));
                    factory=Interface(item,"BCC18B79-BA16-442F-80C4-8A59C30C463B");
                    Marshal.ThrowExceptionForHR(Method<GetImage>(factory,3)(factory,new Size{X=48,Y=48},4,out bitmap));
                    var image=Pixels(bitmap);
                    Method<Attributes>(item,6)(item,0x20010000,out uint attributes);
                    string kind=(attributes&0x10000)!=0?"shortcut":(attributes&0x20000000)!=0?"folder":"item";
                    var id=Convert.ToHexString(SHA256.HashData(image.Bytes));
                    if(!images.Any(x=>x.Id==id))images.Add(new(id,image.Width,image.Height,kind,image.Bytes));
                }
                catch(COMException) { /* An unavailable item does not invalidate other icons. */ }
                finally { if(bitmap!=0)DeleteObject(bitmap);Release(ref factory);Release(ref item); }
            }
            return images;
        }
        finally
        {
            Release(ref array);Release(ref folderView);Release(ref view);Release(ref browser);Release(ref provider);Release(ref unknown);
            if(desktop!=null&&Marshal.IsComObject(desktop))Marshal.FinalReleaseComObject(desktop);
            if(windows!=null&&Marshal.IsComObject(windows))Marshal.FinalReleaseComObject(windows);
        }
    }
    private static (int Width,int Height,byte[] Bytes) Pixels(nint bitmap)
    {
        if(GetObject(bitmap,Marshal.SizeOf<Bitmap>(),out var b)==0||b.Width<1||b.Width>48||b.Height<1||b.Height>48)throw new COMException("Invalid icon bitmap");
        var data=new byte[b.Width*b.Height*4];var header=new BitmapHeader{Size=40,Width=b.Width,Height=-b.Height,Planes=1,Bits=32};
        nint dc=GetDC(0);
        try{if(GetDIBits(dc,bitmap,0,(uint)b.Height,data,ref header,0)!=b.Height)throw new COMException("GetDIBits failed");}
        finally{ReleaseDC(0,dc);}
        // Shell ARGB DIB is premultiplied; Image.Format.Rgba8 expects straight RGBA.
        for(int p=0;p<data.Length;p+=4)
        {
            byte blue=data[p],red=data[p+2],alpha=data[p+3];
            if(alpha==0){data[p]=data[p+1]=data[p+2]=0;continue;}
            data[p]=(byte)Math.Min(255,(red*255+alpha/2)/alpha);
            data[p+1]=(byte)Math.Min(255,(data[p+1]*255+alpha/2)/alpha);
            data[p+2]=(byte)Math.Min(255,(blue*255+alpha/2)/alpha);
        }
        if(!Enumerable.Range(0,data.Length/4).Any(i=>data[i*4+3]>0))throw new COMException("Empty icon alpha");
        return(b.Width,b.Height,data);
    }
}
