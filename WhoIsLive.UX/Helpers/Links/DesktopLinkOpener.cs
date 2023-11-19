using System;
using System.Diagnostics;
using WhoIsLive.Lib.Interfaces;

namespace WhoIsLive.UX.Lib
{
    public class DesktopLinkOpener : IAuthentificationLinkOpener
    {
        public string DisplayName => "Browser";

        public string Directory { get; set; } = string.Empty;

        public bool IsInstalled() => true;

        public void Open(string url)
        {
            if (OperatingSystem.IsWindows())
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            else if (OperatingSystem.IsMacOS())
                Process.Start("open", $"\"{url}\""); // "open" is the command to open a link in MacOS
            else if (OperatingSystem.IsLinux())
                Process.Start("xdg-open", $"\"{url}\""); // "xdg-open" is the command to open a link in Linux
            else
                throw new PlatformNotSupportedException("This platform is not supported");
        }

        public void Open(Uri uri)
        {
            Open(uri.ToString());
        }
    }
}