using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using Splat;
using WhoIsLive.Lib.Interfaces;

namespace WhoIsLive.UX.Android;

[Activity(
    Label = "WhoIsLive.UX.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        var authLinkOpener = new AndroidLinkOpener();

        Locator.CurrentMutable.RegisterConstant<IAuthentificationLinkOpener>(authLinkOpener);
        Locator.CurrentMutable.RegisterConstant<ILinkOpener>(authLinkOpener);

        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }
}
