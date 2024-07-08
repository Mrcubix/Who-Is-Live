using System;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WhoIsLive.UX.ViewModels.Controls;

#nullable enable

public partial class NotificationViewModel : ViewModelBase
{
    #region Fields

    private object _sync = new object();

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private bool _isInfo = false;

    [ObservableProperty]
    private bool _isSuccess = false;

    [ObservableProperty]
    private bool _isWarning = false;

    [ObservableProperty]
    private bool _isError = false;

    private bool _isHidden = true;

    #endregion

    #region Constructors

    public NotificationViewModel()
    {
    }

    public NotificationViewModel(string title, string message, NotificationType type = NotificationType.Information)
    {
        _title = title;
        _message = message;

        ToggleType(type);
        Dispatcher.UIThread.InvokeAsync(() => IsHidden = false);

        // Make the notification disappear after 3 seconds.
        Task.Run(() => ScheduleHide());
    }

    #endregion

    #region Events

    public event EventHandler? RemovalRequested;

    #endregion

    #region Properties

    public bool IsHidden
    {
        get => _isHidden;
        set => SetProperty(ref _isHidden, value);
    }

    public TimeSpan TransitionDuration { get; set; } = TimeSpan.FromMilliseconds(300);
    public TimeSpan DelayBeforeHide { get; set; } = TimeSpan.FromSeconds(3);

    #endregion

    #region Methods

    private void ToggleType(NotificationType type)
    {
        IsInfo = type == NotificationType.Information;
        IsSuccess = type == NotificationType.Success;
        IsWarning = type == NotificationType.Warning;
        IsError = type == NotificationType.Error;
    }

    [RelayCommand]
    public async Task ScheduleHide(int delay = -1)
    {
        await Task.Delay(0 > delay ? DelayBeforeHide : TimeSpan.FromMilliseconds(delay));

        lock (_sync)
        {
            Dispatcher.UIThread.Invoke(() => 
            {
                if (IsHidden == false)
                    IsHidden = true;
            });

            // Wait for the animation to finish.
            Task.Delay(0 > delay ? TransitionDuration : TimeSpan.FromMilliseconds(delay)).Wait();

            RemovalRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    #endregion
}
