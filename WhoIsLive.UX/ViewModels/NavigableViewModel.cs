using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WhoIsLive.UX.ViewModels;

#nullable enable

public abstract partial class NavigableViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _canGoBack = false;

    public abstract event EventHandler? BackRequested;

    [ObservableProperty]
    protected NavigableViewModel? _nextViewModel = null;

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    public abstract void GoBack();
}