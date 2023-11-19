using System;
using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Splat;
using WhoIsLive.Lib.Events;
using WhoIsLive.Lib.Events.Authentication;
using WhoIsLive.Lib.Interfaces;
using WhoIsLive.UX.Authentication.OAuth;
using WhoIsLive.UX.Lib;

namespace WhoIsLive.UX.ViewModels.Screens;

#nullable enable

public partial class AuthenticationScreenViewModel : NavigableViewModel, IRunner
{
    private OAuth2Authenticator _authenticator;

    #region Observable Fields

    [ObservableProperty]
    private bool _requestingAuthentication = false;

    [ObservableProperty]
    private bool _isAuthenticated = false;

    #endregion

    #region Constructors

    public AuthenticationScreenViewModel(string clientID, int[] redirectPorts, string scope)
    {
        var linkOpener = Locator.Current.GetService<IAuthentificationLinkOpener>() 
                             ?? throw new NullReferenceException("A Link Opener is not available for this platform.");

        _authenticator = new OAuth2Authenticator(clientID, redirectPorts, scope, linkOpener);

        _authenticator.AuthenticationCompleted += OnAuthenticationCompleted;
        _authenticator.AuthenticationFailed += OnAuthenticationFailed;

        BackRequested = null!;
    }

    #endregion

    #region Events

    public event EventHandler<OAuth2AuthenticationEventArgs>? AuthenticationCompleted;

    public event EventHandler<ProcessErrorEventArgs>? AuthenticationFailed;

    #pragma warning disable CS0414

    public override event EventHandler? BackRequested;

    #pragma warning restore CS0414

    #endregion

    #region Methods

    [RelayCommand(CanExecute = nameof(RequestingAuthentication))]
    public void Authenticate()
    {
        _authenticator.Authenticate();
    }

    public string ValidateToken(string accessToken)
        => _authenticator.CheckTokenValidity(accessToken);

    public void Run()
    {
        RequestingAuthentication = true;
        IsAuthenticated = false;
    }

    protected override void GoBack()
    {
        throw new NotImplementedException();
    }

    [RelayCommand(CanExecute = nameof(IsAuthenticated))]
    protected void GoNext()
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Event Handlers

    private void OnAuthenticationCompleted(object? sender, OAuth2AuthenticationEventArgs e) => AuthenticationCompleted?.Invoke(sender, e);

    private void OnAuthenticationFailed(object? sender, ProcessErrorEventArgs e) => AuthenticationFailed?.Invoke(sender, e);

    #endregion
}