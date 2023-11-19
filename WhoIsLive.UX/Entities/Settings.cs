using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WhoIsLive.UX.Entities;

#nullable enable

public partial class Settings : ObservableObject
{
    #region Observable Fields

    [ObservableProperty]
    private string _openWith = string.Empty;

    [ObservableProperty]
    private int _elementsPerPageIndex = 0;

    [ObservableProperty]
    private string _quality = "best";

    [ObservableProperty]
    private string _vLCDirectory = string.Empty;

    [ObservableProperty]
    private string _mPVDirectory = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _blockedStreams = new();

    #endregion

    #region Constructors

    [JsonConstructor]
    public Settings()
    {
    }

    public Settings(string path)
    {
        Path = path;
    }

    #endregion

    #region Properties

    [JsonIgnore]
    public string Path { get; set; } = string.Empty;

    #endregion

    #region Static Methods

    public static Settings? Load(string path)
    {
        if (!File.Exists(path))
            return null;

        var settingsJson = File.ReadAllText(path);

        var settings = JsonSerializer.Deserialize<Settings>(settingsJson) ?? throw new Exception("A Serialization error occurred.");
        settings.Path = path;

        return settings;
    }

    public void Save() => Save(Path);

    public void Save(string path)
    {
        var settingsJson = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });

        File.WriteAllText(path, settingsJson);
    }

    #endregion
}