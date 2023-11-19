using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Splat;
using WhoIsLive.Lib.Interfaces;
using WhoIsLive.UX.Entities;
using WhoIsLive.UX.Lib;
using WhoIsLive.UX.ViewModels.Controls;

namespace WhoIsLive.UX.ViewModels.Screens;

#nullable enable

public partial class SettingsScreenViewModel : NavigableViewModel
{
    #region Fields

    private Settings _settings = null!;

    #endregion

    #region Observable Fields

    [ObservableProperty]
    private ILinkOpener _openWith = null!;

    [ObservableProperty]
    private string _quality;

    [ObservableProperty]
    private int _elementsPerPageIndex;

    private string _vlcDirectory = string.Empty;

    private string _mPVDirectory = string.Empty;

    [ObservableProperty]
    private ObservableCollection<BlockedStreamViewModel> _blockedStreams = null!;

    #endregion

    #region Initialization

    public SettingsScreenViewModel(Settings settings)
    {
        _settings = settings;

        // Quality

        Quality = settings.Quality;

        // Elements Per page

        ElementsPerPageIndex = Math.Clamp(settings.ElementsPerPageIndex, 
                                          0, ElementsPerPageChoices.Length - 1);

        // Open With

        OpenWithChoices = new List<ILinkOpener>();

        var linkOpeners = Locator.Current.GetServices<ILinkOpener>().ToList();

        foreach (var linkOpener in linkOpeners)
        {
            if (linkOpener.IsInstalled())
            {
                if (linkOpener is ISettingsDependent settingsDependent)
                    settingsDependent.Settings = settings;

                SetDirectory(linkOpener, settings);

                OpenWithChoices.Add(linkOpener);
            }
        }

        OpenWith = OpenWithChoices.FirstOrDefault(linkOpener => linkOpener.DisplayName == settings.OpenWith) ?? OpenWithChoices[0];

        // Directories

        VLCDirectory = settings.VLCDirectory;
        MPVDirectory = settings.MPVDirectory;

        // Blocked Streams

        BlockedStreams = new ObservableCollection<BlockedStreamViewModel>();

        foreach (var blockedStream in settings.BlockedStreams)
        {
            var stream = new BlockedStreamViewModel(blockedStream);
            stream.UnblockRequested += OnUnblockRquested;

            BlockedStreams.Add(stream);
        }

        BlockedStreams.CollectionChanged += OnBlockedUsersChanged;

        PropertyChanged += OnPropertyChanged;

        CanGoBack = true;
        BackRequested = null!;
    }

    private void SetDirectory(ILinkOpener linkOpener, Settings settings)
    {
        if (linkOpener is VLCLinkOpener vlcLinkOpener)
            vlcLinkOpener.Directory = settings.VLCDirectory;

        if (linkOpener is MPVLinkOpener mpvLinkOpener)
            mpvLinkOpener.Directory = settings.MPVDirectory;
    }

    #endregion

    #region Events

    public event EventHandler? BlockedStreamsChanged;

    public override event EventHandler? BackRequested;

    #endregion

    #region Properties

    public List<ILinkOpener> OpenWithChoices { get; set; }

    public string[] QualityChoices { get; set; } = { "best", "720p60", "720p", "480p", "360p", "worst" };

    public int[] ElementsPerPageChoices { get; set; } = { 10, 25, 50, 100 };

    public int ElementsPerPage => ElementsPerPageChoices[ElementsPerPageIndex];

    public string VLCDirectory
    {
        get => _vlcDirectory;
        set
        {
            if (value == _vlcDirectory)
                return;

            var vlcLinkOpener = OpenWithChoices.OfType<VLCLinkOpener>().FirstOrDefault();

            if (vlcLinkOpener is not null)
                vlcLinkOpener.Directory = value;

            SetProperty(ref _vlcDirectory, value);
        }
    }

    public string MPVDirectory
    {
        get => _mPVDirectory;
        set
        {
            if (value == _mPVDirectory)
                return;

            var mpvLinkOpener = OpenWithChoices.OfType<MPVLinkOpener>().FirstOrDefault();

            if (mpvLinkOpener is not null)
                mpvLinkOpener.Directory = value;

            SetProperty(ref _mPVDirectory, value);
        }
    }

    #endregion

    #region Methods

    protected override void GoBack() => BackRequested?.Invoke(this, EventArgs.Empty);

    public void OnUnblockRquested(object? sender, EventArgs e)
    {
        if (sender is not BlockedStreamViewModel blockedStreamViewModel)
            return;

        BlockedStreams.Remove(blockedStreamViewModel);
    }

    #endregion

    #region Event Handlers

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OpenWith))
            _settings.OpenWith = OpenWith.DisplayName;
        else if (e.PropertyName == nameof(Quality))
            _settings.Quality = Quality;
        else if (e.PropertyName == nameof(ElementsPerPageIndex))
            _settings.ElementsPerPageIndex = ElementsPerPageIndex;
        else if (e.PropertyName == nameof(VLCDirectory))
            _settings.VLCDirectory = VLCDirectory;
        else if (e.PropertyName == nameof(MPVDirectory))
            _settings.MPVDirectory = MPVDirectory;

        _settings.Save();
    }

    private void OnBlockedUsersChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            if (e.NewItems == null)
                return;

            foreach (var item in e.NewItems)
            {
                if (item is BlockedStreamViewModel stream)
                {
                    _settings.BlockedStreams.Add(stream.Username);
                    stream.UnblockRequested += OnUnblockRquested;
                }
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            if (e.OldItems == null)
                return;

            foreach (var item in e.OldItems)
            {
                if (item is BlockedStreamViewModel stream)
                {
                    _settings.BlockedStreams.Remove(stream.Username);
                    stream.UnblockRequested -= OnUnblockRquested;
                }
            }
        }

        _settings.Save();
        BlockedStreamsChanged?.Invoke(this, EventArgs.Empty);
    }

    #endregion
}