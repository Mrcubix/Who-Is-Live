using System;
using CommunityToolkit.Mvvm.ComponentModel;
using WhoIsLive.UX.Entities.API;

namespace WhoIsLive.UX.ViewModels.Controls;

#nullable enable

public partial class LiveStreamViewModel : ViewModelBase, IEquatable<LiveStreamViewModel>
{
    #region Observable Fields

    [ObservableProperty]
    private string _profilePictureUrl = string.Empty;

    [ObservableProperty]
    private string _userId = string.Empty;

    [ObservableProperty]
    private string _login = string.Empty;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _game = "Just Chatting";

    private int _viewers = 0;

    [ObservableProperty]
    private string _viewersCountText = "0";

    #endregion

    public LiveStreamViewModel()
    {
    }

    public LiveStreamViewModel(string profilePictureUrl, LiveStream stream)
    {
        ProfilePictureUrl = profilePictureUrl;
        UserId = stream.UserId;
        Login = stream.UserLogin;
        Username = stream.UserName;
        Title = stream.Title;
        Game = stream.GameName;
        Viewers = stream.ViewerCount;
    }

    public LiveStreamViewModel(string login, string username, string title, string game, int viewers)
    {
        Login = login;
        Username = username;
        Title = title;
        Game = game;
        Viewers = viewers;
    }

    public LiveStreamViewModel(string profilePictureUrl, string login, string username, string title, string game, int viewers)
    {
        ProfilePictureUrl = profilePictureUrl;
        Login = login;
        Username = username;
        Title = title;
        Game = game;
        Viewers = viewers;
    }

    #region Events

    public event EventHandler<string>? CopyStreamURLToClipboardRequested;

    public event EventHandler<string>? OpenRequested;

    public event EventHandler<string>? BlockRequested;

    #endregion

    #region Properties

    public int Viewers
    {
        get => _viewers;
        set
        {
            _viewers = value;
            OnPropertyChanged();

            int divisor = value > 1000 ? value > 1000000 ? 1000000 : 1000 : 1;
            string unitText = divisor == 1000 ? "K" : divisor == 1000000 ? "M" : "";

            string initial;

            if (divisor > 1)
            {
                initial = $"{(double)value / divisor:F1} {unitText}";
                if (initial.EndsWith('0')) // Remove trailing zeros
                    ViewersCountText = initial[..^2] + unitText;
            }
            else
                initial = $"{value} {unitText}";
            
            ViewersCountText = initial;
        }
    }

    #endregion

    #region Methods

    public void Open()
    {
        OpenRequested?.Invoke(this, $"https://twitch.tv/{Login}");
    }

    public void CopyStreamURL()
        => CopyStreamURLToClipboardRequested?.Invoke(this, $"https://twitch.tv/{Login}");
    
    public void Block()
        => BlockRequested?.Invoke(this, UserId);

    public bool Equals(LiveStreamViewModel? other)
    {
        if (other is null)
            return false;

        return UserId == other.UserId;
    }

    #endregion
}