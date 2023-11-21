using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WhoIsLive.UX.ViewModels.Screens;

#nullable enable

public partial class ErrorScreenViewModel : NavigableViewModel
{
    #region Observable Fields

    [ObservableProperty]
    public string _source = "An error occurred in the application.";

    [ObservableProperty]
    public string _message = "The error message is not available.";

    #endregion

    #region Constructors

    public ErrorScreenViewModel()
    {
        BackRequested = null!;
        CanGoBack = true;
    }

    #endregion

    #region Events

    public event EventHandler<NavigableViewModel?>? RetryRequested;

    #pragma warning disable CS0414

    public override event EventHandler? BackRequested;

    #pragma warning restore CS0414

    #endregion

    #region Properties

    public NavigableViewModel? SourceViewModel { get; set; }

    #endregion

    #region Methods

    public override void GoBack() => RetryRequested?.Invoke(this, SourceViewModel);

    #endregion
}
