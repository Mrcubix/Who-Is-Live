using System;
using System.ComponentModel;
using System.Diagnostics;
using IODirectory = System.IO.Directory;
using WhoIsLive.Lib.Interfaces;
using WhoIsLive.UX.Entities;

namespace WhoIsLive.UX.Lib
{
    public class VLCLinkOpener : ILinkOpener, ISettingsDependent
    {
        #region Constants

        private const string COMMAND = "vlc";
        private const string TEST_ARGS = "--version";
        private const string STREAMLINK_COMMAND = "streamlink";
        private const string VLC_ARGUMENTS = "--file-caching 10000 --network-caching 10000";

        #endregion

        private readonly string _extension = OperatingSystem.IsWindows() ? ".exe" : string.Empty;

        private string _command = string.Empty;

        #region Constructors

        public VLCLinkOpener(Settings settings)
        {
            Settings = settings;
        }

        #endregion

        #region Properties

        public Settings Settings { get; set; }
        
        public string DisplayName => "VLC";

        public string Directory
        {
            get => _command;
            set
            {
                _command = value;

                if (value != "")
                    if (!value.EndsWith('/') && !value.EndsWith('\\'))
                        _command += "/";
            }
        }

        #endregion

        #region Methods

        public bool IsInstalled()
        {
            if (!string.IsNullOrEmpty(Directory) && IODirectory.Exists(Directory) == false)
                throw new ArgumentException("The specified VLC directory does not exist");

            Process? process = null;

            var command = $"{Directory}{COMMAND}{_extension}";

            try
            {
                process = OpenCore(command, TEST_ARGS);
            }
            catch (PlatformNotSupportedException)
            {
                return false;
            }
            catch (Win32Exception)
            {
                return false;
            }

            if (process is null)
                return false;

            process.WaitForExit(50);

            if (!process.HasExited)
                process.Kill();

            process.WaitForExit();

            if (OperatingSystem.IsWindows())
                return process.ExitCode != 9009 && process.ExitCode != 1;
            else if (OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
                return process.ExitCode != 127 && process.ExitCode != 1;
            else
                return false;
        }

        public void Open(string url)
        {
            if (!string.IsNullOrEmpty(Directory) && IODirectory.Exists(Directory) == false)
                throw new ArgumentException("The specified VLC directory does not exist");

            var command = $"{Directory}{COMMAND}{_extension}";
            var args = $"-p \"{command}\" -a \"{VLC_ARGUMENTS}\" --url {url} --default-stream {Settings.Quality} --twitch-disable-ads";

            var process = OpenCore(STREAMLINK_COMMAND, args);

            process?.WaitForExit();
                
            Console.WriteLine(process?.StandardError.ReadToEnd());
        }

        public void Open(Uri uri)
        {
            Open(uri.ToString());
        }

        private Process? OpenCore(string command, string args = "")
        {
            if (OperatingSystem.IsWindows())
                return Process.Start(new ProcessStartInfo(command) { Arguments = args, UseShellExecute = false, WindowStyle = ProcessWindowStyle.Hidden, RedirectStandardError = true });
            else if (OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
                return Process.Start(command, args);
            else
                throw new PlatformNotSupportedException("This platform is not supported");
        }

        #endregion
    }
}