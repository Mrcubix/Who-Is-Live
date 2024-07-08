using System;
using System.ComponentModel;
using System.Diagnostics;
using IODirectory = System.IO.Directory;
using WhoIsLive.Lib.Interfaces;
using WhoIsLive.UX.Entities;
using System.IO;
using WhoIsLive.UX.Helpers;

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

        private string _command = string.Empty;

        #region Constructors

        public VLCLinkOpener(Settings settings)
        {
            Settings = settings;
        }

        #endregion

        #region Properties

        public Settings Settings { get; set; }

        public string Directory
        {
            get => _command;
            set
            {
                _command = value;
            
                if (value != "")
                    if (!value.EndsWith('/') || !value.EndsWith('\\'))
                        _command += "/";
            }
        }

        public string DisplayName => "VLC";

        #endregion

        #region Methods

        public bool IsInstalled()
        {
            if (!string.IsNullOrEmpty(Directory) && IODirectory.Exists(Directory) == false)
                throw new ArgumentException("The specified VLC directory does not exist");
                
            Process? process = null;

            var command = $"{Directory}{COMMAND}";

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

            var command = $"{Directory}{COMMAND}";
            var args = $"-p {command} -a \"{VLC_ARGUMENTS}\" {url} {Settings.Quality} --twitch-disable-ads";
            
            OpenCore(STREAMLINK_COMMAND, args);
        }

        public void Open(Uri uri)
        {
            Open(uri.ToString());
        }

        private Process? OpenCore(string command, string args = "")
        {
            if (OperatingSystem.IsWindows())
                return Process.Start(new ProcessStartInfo(command) { Arguments = args, UseShellExecute = true, WindowStyle = ProcessWindowStyle.Hidden });
            else if (OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
                return Process.Start(command, args);
            else
                throw new PlatformNotSupportedException("This platform is not supported");
        }

        #endregion
    }
}