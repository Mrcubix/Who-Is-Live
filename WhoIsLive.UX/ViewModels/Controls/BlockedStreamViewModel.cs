using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WhoIsLive.UX.ViewModels.Controls;

#nullable enable

public partial class BlockedStreamViewModel : ObservableObject
{
    #region Observable Fields

    [ObservableProperty]
    private string _username = null!;

    #endregion

    #region Constructors

    public BlockedStreamViewModel(string username)
    {
        Username = username;
    }

    #endregion

    #region Events

    public event EventHandler? UnblockRequested;

    #endregion

    #region Commands

    [RelayCommand]
    public void Unblock() => UnblockRequested?.Invoke(this, EventArgs.Empty);

    #endregion
}