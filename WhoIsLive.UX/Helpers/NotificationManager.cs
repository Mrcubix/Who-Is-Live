using System;
using System.Collections.ObjectModel;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using WhoIsLive.UX.ViewModels.Controls;

namespace WhoIsLive.UX.Helpers;

public partial class NotificationManager : ObservableObject
{
    #region Fields

    private object _sync = new();

    [ObservableProperty]
    private ObservableCollection<NotificationViewModel> _notifications = new();

    #endregion

    #region Methods

    /// <summary>
    ///   Add a new notification to the list.
    /// </summary>
    /// <param name="title">The title or header of the notification.</param>
    /// <param name="message">The message of the notification.</param>
    /// <param name="type">The type of the notification, which determines its background color.</param>
    /// <param name="delay">The time in seconds before the notification should be hidden.</param>
    public void Add(string title, string message, NotificationType type, int delay = 3)
    {
        var notification = new NotificationViewModel(title, message, type)
        {
            DelayBeforeHide = TimeSpan.FromSeconds(delay)
        };

        Add(notification);
    }

    public void Add(NotificationViewModel notification)
    {
        notification.RemovalRequested += OnNotificationRemovalRequested;

        lock (_sync)
            Notifications.Add(notification);
    }

    public void Remove(NotificationViewModel notification)
    {
        notification.RemovalRequested -= OnNotificationRemovalRequested;

        lock (_sync)
            Notifications.Remove(notification);
    }

    private void OnNotificationRemovalRequested(object? sender, EventArgs e)
    {
        if (sender is NotificationViewModel notification)
            Remove(notification);
    }

    #endregion
}