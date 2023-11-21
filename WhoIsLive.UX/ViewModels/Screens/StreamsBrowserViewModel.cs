using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WhoIsLive.Lib.Events;
using WhoIsLive.Lib.Interfaces;
using WhoIsLive.UX.Entities;
using WhoIsLive.UX.Entities.API;
using WhoIsLive.UX.Entities.API.Arrays;
using WhoIsLive.UX.Helpers.API;
using WhoIsLive.UX.Interfaces;
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

    private int _elementsPerPage;

    #endregion

    #region Observable Fields

    [ObservableProperty]
    private SettingsScreenViewModel _settingsScreenViewModel;

    [ObservableProperty]
    private ObservableCollection<LiveStreamViewModel> _currentPageLiveStreams;

    [ObservableProperty]
    private int _page;

    [ObservableProperty]
    private int _pageCount;

    [ObservableProperty]
    private ObservableCollection<string> _blockedStreams;

    [ObservableProperty]
    private bool _hasStreams;

    [ObservableProperty]
    private bool _canGoNext;

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

    [RelayCommand]
    public void OpenSettings()
    {
        NextViewModel = SettingsScreenViewModel;
    }

    private void StartOver() => ChangePage(1);

    public override void GoBack() => ChangePage(Page - 1);

    [RelayCommand]
    public void GoNext() => ChangePage(Page + 1);

    private void ChangePage(int page, bool doRecalculation = true)
    {
        Page = page;

        UnsubscribeFromEvents();

        CurrentPageLiveStreams.Clear();

        if (doRecalculation)
            RecalculatePageCount();
            
        FillPage((Page - 1) * _elementsPerPage);
    }

    public void UnsubscribeFromEvents()
    {
        foreach (var stream in CurrentPageLiveStreams)
        {
            stream.BlockRequested -= OnBlockRequested;
            stream.OpenRequested -= OnOpenRequested;
        }
    }

    public void RecalculatePageCount()
    {
        PageCount = (int)Math.Ceiling((double)_liveStreams.Count / _elementsPerPage);
        Page = Math.Clamp(Page, 1, PageCount);

        CanGoBack = Page > 1;
        CanGoNext = Page < PageCount;
    }

    private void FillPage(int fromIndex)
    {
        // We need to make sure that we don't add more elements than we need.
        if (CurrentPageLiveStreams.Count == _elementsPerPage)
            return;

        // We need to make sure that we don't start out of bounds.
        if (fromIndex >= _liveStreams.Count)
            return;

        // calculate how many elements to fill
        var fillCount = (Page * _elementsPerPage) - fromIndex;

        if (fillCount <= 0)
            return;

        // Clamp the end index to make sure we don't go out of bounds.
        var endIndex = Math.Clamp(fromIndex + fillCount, 0, _liveStreams.Count);

        for (int i = fromIndex; i < endIndex; i++)
        {
            var stream = _liveStreams[i]; 

            CurrentPageLiveStreams.Add(stream);

            stream.BlockRequested += OnBlockRequested;
            stream.OpenRequested += OnOpenRequested;
        }

        HasStreams = CurrentPageLiveStreams.Count > 0;
    }

    public void Run() => _ = Task.Run(Refresh);

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

        var oldPageCount = PageCount;
 
        // We start filling from the last index of the page post removal.
        RecalculatePageCount();

        if (oldPageCount == PageCount)
            FillPage(Page * _elementsPerPage - 1);
        else
            ChangePage(Page, false);
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