using System;
using System.IO;
using Avalonia.Platform;

namespace WhoIsLive.UX.Extensions;

public static class AssetLoaderExtensions
{
    #region Methods

    public static string ReadHTML(string path)
    {
        var stream = AssetLoader.Open(new Uri(path));

        using var reader = new StreamReader(stream);
        
        return reader.ReadToEnd();
    }

    #endregion
}