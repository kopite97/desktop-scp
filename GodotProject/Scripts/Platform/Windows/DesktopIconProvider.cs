using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SiteDesktop.Platform.Windows;
public sealed record DesktopIconImage(string Id,int Width,int Height,string Kind,byte[] Rgba);
public static class DesktopIconProvider
{
    public static async Task<DesktopIconImage[]> ReadAsync(string workerPath,CancellationToken cancellation)
    {
        using var timeout=CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        timeout.CancelAfter(TimeSpan.FromSeconds(10));
        cancellation.ThrowIfCancellationRequested();
        var info=new ProcessStartInfo(workerPath){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
        using var process=Process.Start(info)??throw new InvalidOperationException("Icon worker unavailable");
        try
        {
            using var job=new WorkerJob(process.Handle);
            var output=process.StandardOutput.ReadToEndAsync(timeout.Token);
            var error=process.StandardError.ReadToEndAsync(timeout.Token);
            await process.WaitForExitAsync(timeout.Token).ConfigureAwait(false);
            if(process.ExitCode!=0)throw new InvalidOperationException("Icon worker failed: "+await error.ConfigureAwait(false));
            string json=await output.ConfigureAwait(false);
            if(json.Length>600000)throw new InvalidDataException("Icon response exceeds bound");
            var icons=JsonSerializer.Deserialize<DesktopIconImage[]>(json)??Array.Empty<DesktopIconImage>();
            if(icons.Length>32)throw new InvalidDataException("Too many icons");
            foreach(var image in icons)
                if(image.Width<1||image.Width>48||image.Height<1||image.Height>48||image.Rgba.Length!=image.Width*image.Height*4||image.Id!=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(image.Rgba)))
                    throw new InvalidDataException("Invalid icon pixels");
            return icons;
        }
        finally
        {
            if(!process.HasExited){process.Kill(entireProcessTree:true);await process.WaitForExitAsync().ConfigureAwait(false);}
        }
    }
}
