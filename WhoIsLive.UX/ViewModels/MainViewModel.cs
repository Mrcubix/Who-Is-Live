using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using WhoIsLive.Lib.Events;
using WhoIsLive.Lib.Events.Authentication;
using WhoIsLive.Lib.Interfaces;
using WhoIsLive.UX.Cryptography;
using WhoIsLive.UX.Entities;
using WhoIsLive.UX.ViewModels.Screens;
using Resources = WhoIsLive.UX.Assets.Localizations.Resources;

namespace WhoIsLive.UX.ViewModels;

#nullable enable

public class MainViewModel : NavigableViewModel, IRunner
{
    #region Constants

    private static readonly string APPDATA_FOLDER_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WhoIsLive");

    private static readonly string SETTINGS_FILE_PATH = Path.Combine(APPDATA_FOLDER_PATH, "settings.json");

    // <summary>
    //   The client ID for the Twitch API.
    // </summary>
    // <remarks>
    //   This is a public value and is not a secret.
    // </remarks>
    private const string CLIENT_ID = "a678fvjrbd60fh06l9i8fo3t994n6c";

    // <summary>
    //   The ports used to redirect the user to after the authentication.
    // </summary>
    private readonly int[] REDIRECT_PORTS = { 7272, 8080, 17272, 27272, 37272, 47272, 57272, 62727 };
    
    // <summary>
    //   The scope of the authentication.
    // </summary>
    private const string SCOPE = "user:read:follows user:read:email";

    #endregion

    #region Fields

    private Settings _settings = null!;

    private readonly IObfuscator _obfuscator;

    private ErrorScreenViewModel _errorScreenViewModel = null!;

    private AuthenticationScreenViewModel _authenticationScreenViewModel = null!;

    private StreamsBrowserViewModel _streamsBrowserViewModel = null!;

    private string _accessToken = string.Empty;
    private string _userID = string.Empty;

    #endregion

    #region Constructors

    public MainViewModel(IObfuscator obfuscator = null!)
    {
        _obfuscator = obfuscator ?? new DefaultObfuscator();

        LoadSettings();

        _errorScreenViewModel = new ErrorScreenViewModel();

        _errorScreenViewModel.RetryRequested += OnRetryRequested;

        _authenticationScreenViewModel = new AuthenticationScreenViewModel(CLIENT_ID, REDIRECT_PORTS, SCOPE);

        _authenticationScreenViewModel.AuthenticationCompleted += OnAuthenticationCompleted;
        _authenticationScreenViewModel.AuthenticationFailed += OnAuthenticationFailed;

        BackRequested = null!;

        _ = Task.Run(Run);
    }

    #endregion

    #region Events

    #pragma warning disable CS0414

    public override event EventHandler? BackRequested;

    #pragma warning restore CS0414

    #endregion

    #region Methods

    private void LoadSettings()
    {
        try
        {
            _settings = Settings.Load(SETTINGS_FILE_PATH) ?? new Settings(SETTINGS_FILE_PATH);
        }
        catch (Exception ex)
        {
            ShowErrorScreen(null!, "Main Page", $"{Resources.SettingLoadErrorLabel}: {ex.Message}");
            _settings = new Settings(SETTINGS_FILE_PATH);
            return;
        }
    }

    public override void GoBack()
    {
        throw new NotImplementedException();
    }

    public void Run()
    {
        ReadToken(); 

        // Check if the user is authenticated.
        if (string.IsNullOrEmpty(_accessToken))
        {
            RetryAuthentication();
            return;
        }

        if (!ValidateToken())
            return;

        _streamsBrowserViewModel = new StreamsBrowserViewModel(CLIENT_ID, _accessToken, _userID, _settings);

        _streamsBrowserViewModel.ErrorOccurred += OnErrorOccurred;
        //_streamsBrowserViewModel.PropertyChanged += OnBrowserPropertyChanged;

        // Start fetching all followed channels that are live.
        _ = Task.Run(() => _streamsBrowserViewModel.Run());

        NextViewModel = _streamsBrowserViewModel;
    }

    private void ReadToken()
    {
        var directory = new DirectoryInfo(APPDATA_FOLDER_PATH);

        if (!directory.Exists)
            return;

        var tokenFilePath = Path.Combine(APPDATA_FOLDER_PATH, "token.dat");

        if (!File.Exists(tokenFilePath))
            return;

        var encryptedToken = File.ReadAllBytes(tokenFilePath);

        try
        {
            _accessToken = Encoding.UTF8.GetString(_obfuscator.DeObfuscate(encryptedToken));
        }
        catch (Exception)
        {
            ShowErrorScreen(null!, "Main Page", $"{Resources.TokenLoadErrorLabel}");
        }
        
    }

    private bool ValidateToken()
    {
        try
        {
            _userID = _authenticationScreenViewModel.ValidateToken(_accessToken);

            if (string.IsNullOrEmpty(_userID))
            {
                ShowErrorScreen(_authenticationScreenViewModel, "Authentication", Resources.TokenExpiredLabel);
                return false;
            }
        }
        catch (Exception ex)
        {
            ShowErrorScreen(null!, "Authentication", $"{Resources.AuthenticationErrorLabel}: {ex.Message}");
            return false;
        }

        Console.WriteLine("The user is authenticated.");

        return true;
    }

    private void RetryAuthentication()
    {
        _authenticationScreenViewModel.RequestingAuthentication = true;
        _authenticationScreenViewModel.IsAuthenticated = false;

        NextViewModel = _authenticationScreenViewModel;

        _authenticationScreenViewModel.Run();
    }

    private void ShowErrorScreen(NavigableViewModel? sourceVM, string source, string message)
    {
        _errorScreenViewModel.SourceViewModel = sourceVM;
        _errorScreenViewModel.Source = source;
        _errorScreenViewModel.Message = message;

        NextViewModel = _errorScreenViewModel;
    }

    #endregion

    #region Events Handlers

    private void OnAuthenticationCompleted(object? sender, OAuth2AuthenticationEventArgs e)
    {
        ShowErrorScreen(null!, "Authentication", "Authentication completed.");

        _accessToken = e.AccessToken;

        // Encrypt the access token and store it in a file.
        var encryptedToken = _obfuscator.Obfuscate(Encoding.UTF8.GetBytes(_accessToken));

        // Save the encrypted token to a file in the local app data folder.
        var localAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        var tokenFilePath = Path.Combine(localAppDataFolder, "WhoIsLive", "token.dat");

        try
        {
            var directory = new DirectoryInfo(APPDATA_FOLDER_PATH);

            if (!directory.Exists)
                directory.Create();

            File.WriteAllBytes(tokenFilePath, encryptedToken);
        }
        catch (Exception ex)
        {
            ShowErrorScreen(null!, "Main Page", $"{Resources.FileCreationErrorLabel}: {ex.Message}");
            return;
        }

        Run();
    }

    private void OnAuthenticationFailed(object? sender, ProcessErrorEventArgs e)
        => ShowErrorScreen(sender as NavigableViewModel, e.Source, e.Message);

    private void OnErrorOccurred(object? sender, ProcessErrorEventArgs e)
        => ShowErrorScreen(sender as NavigableViewModel, e.Source, e.Message);

    private void OnRetryRequested(object? sender, NavigableViewModel? e)
    {
        if (e is StreamsBrowserViewModel streamsBrowserViewModel && streamsBrowserViewModel.HasAuthenticationFailed)
        {
            Dispatcher.UIThread.Post(RetryAuthentication);
            return;
        }

        if (e is IRunner runner)
            runner.Run();

        if (e != null)
            NextViewModel = e;
    }

    #endregion
}
