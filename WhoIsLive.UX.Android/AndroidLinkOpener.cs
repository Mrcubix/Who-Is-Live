using System;
using Android.Content;
using AndroidUri = Android.Net.Uri;
using WhoIsLive.Lib.Interfaces;
using Android.OS;
using Android.App;

namespace WhoIsLive.UX.Android
{
    public class AndroidLinkOpener : IAuthentificationLinkOpener
    {
        public string DisplayName => "Choose App";

        public string Directory { get; set; } = string.Empty;

        public void Open(string url)
        {
            // API Version 30
            if (Build.VERSION.SdkInt > BuildVersionCodes.R)
            {
                var uri = AndroidUri.Parse(url) ?? throw new ArgumentException("Invalid URL");

                // Open choice of browser
                var intent = new Intent(Intent.ActionView, uri);
                intent.SetFlags(ActivityFlags.NewTask);
                Application.Context.StartActivity(intent);
            }
            else // API Version 29 and below
            {
                if (Application.Context.PackageManager == null)
                    throw new NullReferenceException("PackageManager is null");

                var uri = AndroidUri.Parse(url) ?? throw new ArgumentException("Invalid URL");

                // Open choice of browser
                var intent = new Intent(Intent.ActionView);
                intent.SetData(uri);
                
                var picker = Intent.CreateChooser(intent, "Open with") ?? throw new NullReferenceException("Action chooser is null");

                if (intent.ResolveActivity(Application.Context.PackageManager) != null)
                {
                    picker.SetFlags(ActivityFlags.NewTask);
                    Application.Context.StartActivity(picker);
                }
                else
                {
                    throw new NullReferenceException("No activity found to open URL");
                }
            }
        }

        public void Open(Uri uri)
        {
            Open(uri.ToString());
        }

        public bool IsInstalled() => true;
    }
}