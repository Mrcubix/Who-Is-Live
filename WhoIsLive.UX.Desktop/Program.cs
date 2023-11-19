using System;
using Avalonia;
using Splat;
using WhoIsLive.Lib.Interfaces;
using WhoIsLive.UX.Entities;
using WhoIsLive.UX.Lib;

namespace WhoIsLive.UX.Desktop;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var authLinkOpener = new DesktopLinkOpener();

        Locator.CurrentMutable.RegisterConstant<IAuthentificationLinkOpener>(authLinkOpener);
        
        Locator.CurrentMutable.RegisterConstant<ILinkOpener>(authLinkOpener);
        Locator.CurrentMutable.RegisterConstant<ILinkOpener>(new VLCLinkOpener(new Settings()));
        Locator.CurrentMutable.RegisterConstant<ILinkOpener>(new MPVLinkOpener(new Settings()));

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
