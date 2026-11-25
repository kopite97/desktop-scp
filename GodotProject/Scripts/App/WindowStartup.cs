using Godot;
using SiteDesktop.Platform.Windows;

namespace SiteDesktop.App;

// The engine starts with a transparent, unfocusable tiny window. Windows/Godot
// can clamp the requested 1px surface to 64px before this autoload runs.
// Keep its startup geometry until controls are explicitly opened: changing flags
// can make Godot show a root window that Win32 previously hid.
public partial class WindowStartup : Node
{
    public static bool KeepHidden { get; set; }
    private static bool _prepared;

    public override void _EnterTree()
    {
        KeepHidden = false;
        _prepared = false;
        if (DisplayServer.GetName() == "headless") return;
        WindowsDesktop.SetControlVisible(GetWindow(), false);
    }

    public override void _Ready() => Callable.From(() =>
    {
        // Standalone diagnostic scenes still need their ordinary root window.
        if (!KeepHidden && DisplayServer.GetName() != "headless")
        {
            PrepareControls(GetWindow(), new(640,600));
            WindowsDesktop.SetControlVisible(GetWindow(), true);
        }
    }).CallDeferred();

    public static void PrepareControls(Window window, Vector2I size)
    {
        if (!_prepared)
        {
            _prepared = true;
            window.Transparent = false;
            window.TransparentBg = false;
            window.Borderless = false;
            window.Unresizable = false;
            window.Size = size;
            window.Position = WindowsDesktop.ToGodotPosition(SiteDesktop.Desktop.DesktopPlacement.Select().Usable.Position + new Vector2I(30,50));
        }
        window.Unfocusable = false;
    }
}
