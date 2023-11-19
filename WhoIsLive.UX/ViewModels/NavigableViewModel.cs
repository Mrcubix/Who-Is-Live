using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WhoIsLive.UX.ViewModels;

#nullable enable

public abstract partial class NavigableViewModel : ViewModelBase
{
    public abstract event EventHandler? BackRequested;

    public bool CanGoBack { get; protected set; }

    [ObservableProperty]
    protected NavigableViewModel? _nextViewModel = null;

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    protected abstract void GoBack();
}