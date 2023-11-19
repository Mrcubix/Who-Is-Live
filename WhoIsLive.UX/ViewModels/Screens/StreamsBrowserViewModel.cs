using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Splat;
using WhoIsLive.Lib.Events;
using WhoIsLive.Lib.Interfaces;
using WhoIsLive.UX.Entities;
using WhoIsLive.UX.Entities.API;
using WhoIsLive.UX.Entities.API.Arrays;
using WhoIsLive.UX.Helpers.API;
using WhoIsLive.UX.Interfaces;
using WhoIsLive.UX.Lib;
using WhoIsLive.UX.ViewModels.Controls;

namespace WhoIsLive.UX.ViewModels.Screens;

#nullable enable

public partial class StreamsBrowserViewModel : NavigableViewModel, IRunner, IDisposable
{
    #region Constants

    private const string SOURCE = "Livestreams Browser";

    private const string FOLLOWED_STREAMS_URL = "https://api.twitch.tv/helix/streams/followed";
    private const string GET_USERS_URL = "https://api.twitch.tv/helix/users";

    #endregion

    #region Fields

    #region API Fields

    private HttpClient _client;

    private string _clientID;

    private string _accessToken;

    private string _userID;

    #endregion

    private List<LiveStreamViewModel> _liveStreams;

    private string _cursor;

    private bool _canGoNext;

    private int _elementsPerPage;

    private int _previousEndIndex;

    #endregion

    #region Observable Fields

    [ObservableProperty]
    private SettingsScreenViewModel _settingsScreenViewModel;

    [ObservableProperty]
    private ObservableCollection<LiveStreamViewModel> _currentPageLiveStreams;

    private int _page;

    [ObservableProperty]
    private int _pageCount;

    [ObservableProperty]
    private ObservableCollection<string> _blockedStreams;

    [ObservableProperty]
    private bool _hasStreams;

    #endregion

    #region Constructors

    public StreamsBrowserViewModel(string clientID, string accessToken, string userID, Settings settings)
    {
        SettingsScreenViewModel = new SettingsScreenViewModel(settings);
        SettingsScreenViewModel.PropertyChanged += OnSettingsChanged;
        SettingsScreenViewModel.BlockedStreamsChanged += OnBlockedStreamsChanged;
        SettingsScreenViewModel.BackRequested += OnSettingsBackRequested;

        _clientID = clientID;
        _accessToken = accessToken;
        _userID = userID;

        _client = new HttpClient();
        _liveStreams = new List<LiveStreamViewModel>();

        CurrentPageLiveStreams = new ObservableCollection<LiveStreamViewModel>();

        Page = 1;

        // Elements per page

        _elementsPerPage = SettingsScreenViewModel.ElementsPerPageChoices[settings.ElementsPerPageIndex];

        // Blocked streams

        _blockedStreams = new ObservableCollection<string>(settings.BlockedStreams);

        // The rest

        _cursor = string.Empty;

        BackRequested = null!;

        NextViewModel = this;
    }

    #endregion

    #region Events

    public event EventHandler<ProcessErrorEventArgs>? ErrorOccurred;

#pragma warning disable CS0414

    public override event EventHandler? BackRequested;

#pragma warning restore CS0414

    #endregion

    #region Properties

    public int Page
    {
        get => _page;
        set
        {
            SetProperty(ref _page, value);

            CanGoBack = Page > 1;
            CanGoNext = Page < PageCount;
        }
    }

    public bool CanGoNext
    {
        get => _canGoNext;
        set => SetProperty(ref _canGoNext, value);
    }

    public bool HasAuthenticationFailed { get; set; } = false;

    #endregion

    #region Methods

    #region API Methods

    [RelayCommand]
    public async Task Refresh()
    {
        if (string.IsNullOrEmpty(_clientID) ||
            string.IsNullOrEmpty(_accessToken) ||
            string.IsNullOrEmpty(_userID))
        {
            return;
        }

        var followedStreams = await GetFollowedStreams();

        if (followedStreams == null)
            return;

        // removed blocked streams
        foreach (var blockedStream in BlockedStreams)
        {
            var stream = followedStreams.FirstOrDefault(s => s.UserName == blockedStream);

            if (stream != null)
                followedStreams.Remove(stream);
        }

        var users = await GetUsers(followedStreams);

        if (users == null)
            return;

        _liveStreams.Clear();

        for (int i = 0; i < followedStreams.Count; i++)
        {
            var user = users.FirstOrDefault(u => u.ID == followedStreams[i].UserId);

            if (user != null)
                _liveStreams.Add(new LiveStreamViewModel(user.ProfileImageURL, followedStreams[i]));
        }

        if (followedStreams != null)
            Dispatcher.UIThread.Invoke(StartOver);
    }

    [RelayCommand]
    public void OpenSettings()
    {
        NextViewModel = SettingsScreenViewModel;
    }

    private void StartOver()
    {
        Page = 1;
        PageCount = (int)Math.Ceiling((double)_liveStreams.Count / _elementsPerPage);

        UpdatePageLiveStreams();
    }

    private async Task<List<LiveStream>?> GetFollowedStreams()
    {
        FollowedStreamsRequestHelper helper = new(FOLLOWED_STREAMS_URL, _userID);

        return await FetchItems<LiveStream, LiveStreams, string>(helper, true);
    }

    private async Task<List<User>?> GetUsers(List<LiveStream> streams)
    {
        UsersRequestHelper helper = new(GET_USERS_URL, streams.Select(stream => stream.UserId).ToArray());

        return await FetchItems<User, Users, string[]>(helper, false);
    }

    private async Task<List<T>?> FetchItems<T, TArray, THelper>(IAPIRequestHelper<THelper> helper, bool requestHasCursor) where TArray : TwitchAPIArrayResponse<T>
    {
        List<T> list = new();

        bool isSuccessful = false;

        Range range = new(0, 0);

        do
        {
            string url = helper.BuildUrl(_cursor);

            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.Add("Authorization", $"Bearer {_accessToken}");
            request.Headers.Add("Client-ID", _clientID);

            var response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsStringAsync().Result;

                try
                {
                    var objects = JsonSerializer.Deserialize<TArray>(content);

                    if (objects != null)
                    {
                        list.AddRange(objects.Data);

                        if (requestHasCursor)
                            _cursor = objects.Pagination.Cursor;

                        isSuccessful = true;
                    }
                }
                catch (Exception)
                {
                    ErrorOccurred?.Invoke(this, new ProcessErrorEventArgs(SOURCE, "An error occurred while parsing the response."));
                    isSuccessful = false;
                }
            }
            else
            {
                ParseNonSuccessfulResponse(response);
                isSuccessful = false;
            }
        } while (!string.IsNullOrEmpty(_cursor) && isSuccessful);

        if (isSuccessful)
            return list;
        else
            return null;
    }

    private void ParseNonSuccessfulResponse(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            HasAuthenticationFailed = true;

        var content = response.Content.ReadAsStringAsync().Result;

        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content) ?? new ErrorResponse("", 500, "Unknown error.");

        ErrorOccurred?.Invoke(this, new ProcessErrorEventArgs(SOURCE, errorResponse.ToString()));
    }

    #endregion

    private void UpdatePageLiveStreams()
    {
        var startIndex = (Page - 1) * _elementsPerPage;
        var endIndex = Math.Clamp(startIndex + _elementsPerPage, 0, _liveStreams.Count);

        if (endIndex == _previousEndIndex)
            return;

        _previousEndIndex = endIndex;

        CurrentPageLiveStreams.Clear();

        if (startIndex >= _liveStreams.Count)
            return;

        foreach (var stream in _liveStreams.GetRange(startIndex, endIndex - startIndex))
        {
            CurrentPageLiveStreams.Add(stream);
            stream.BlockRequested += OnBlockRequested;
            stream.OpenRequested += OnOpenRequested;
        }

        HasStreams = CurrentPageLiveStreams.Count > 0;
    }

    public void Run()
    {
        _ = Refresh();
    }

    protected override void GoBack() => ChangePage(-1);

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    public void GoNext() => ChangePage(1);

    private void ChangePage(int increment)
    {
        Page = Math.Clamp(Page + increment, 1, PageCount);

        UpdatePageLiveStreams();
    }

    public void Dispose()
    {
        foreach (var stream in _liveStreams)
        {
            stream.BlockRequested -= OnBlockRequested;
            stream.OpenRequested -= OnOpenRequested;
        }

        _client.Dispose();
    }

    #endregion

    #region Event Handlers

    private void OnBlockRequested(object? sender, string userId)
    {
        if (sender is not LiveStreamViewModel stream)
            return;

        var index = _liveStreams.IndexOf(stream);

        if (index == -1)
            return;

        _liveStreams.RemoveAt(index);

        CurrentPageLiveStreams.Remove(stream);

        SettingsScreenViewModel.BlockedStreams.Add(new BlockedStreamViewModel(stream.Username));
    }

    private void OnOpenRequested(object? sender, string url)
    {
        if (sender is not LiveStreamViewModel stream)
            return;

        var index = _liveStreams.IndexOf(stream);

        if (index == -1)
            return;

        SettingsScreenViewModel.OpenWith.Open(url);
    }

    private void OnSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SettingsScreenViewModel.ElementsPerPageIndex))
        {
            _elementsPerPage = SettingsScreenViewModel.ElementsPerPage;
            Dispatcher.UIThread.Invoke(StartOver);
        }
    }

    private void OnBlockedStreamsChanged(object? sender, EventArgs e)
    {
        var names = SettingsScreenViewModel.BlockedStreams.Select(stream => stream.Username).ToList();

        BlockedStreams = new ObservableCollection<string>(names);
    }

    private void OnSettingsBackRequested(object? sender, EventArgs e)
    {
        NextViewModel = this;
    }

    #endregion
}