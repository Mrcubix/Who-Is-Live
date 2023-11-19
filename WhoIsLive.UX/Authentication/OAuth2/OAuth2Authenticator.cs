using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using WhoIsLive.Lib.Events;
using WhoIsLive.Lib.Events.Authentication;
using WhoIsLive.Lib.Interfaces;
using WhoIsLive.UX.Entities.API.Arrays;

namespace WhoIsLive.UX.Authentication.OAuth;

using static WhoIsLive.UX.Extensions.AssetLoaderExtensions;

#nullable enable

/// <summary>
///   OAuth authenticator for 
/// </summary>
public class OAuth2Authenticator : IDisposable
{
    #region Constants
    private static readonly string FAILURE_RESPONSE = ReadHTML("avares://WhoIsLive.UX/Assets/html/failure_response.html");

    private static readonly string SUCCESS_RESPONSE = ReadHTML("avares://WhoIsLive.UX/Assets/html/success_response.html");

    private static readonly string NORMAL_RESPONSE = ReadHTML("avares://WhoIsLive.UX/Assets/html/normal_response.html");

    private const string AUTHORIZATION_URL_PATTERN = @"https://id.twitch.tv/oauth2/authorize?response_type=token&client_id={0}&redirect_uri=http://localhost:{1}&scope={2}&state={3}";
    
    private const string TEST_URL = "https://api.twitch.tv/helix/users";

    #endregion

    #region Fields

    /// <summary>
    ///   The HTTP listener used to listen for the OAuth2 redirect.
    /// </summary>
    private HttpListener _listener = null!;

    /// <summary>
    ///   The client used to test the validity of the access token.
    /// </summary>
    private HttpClient _client = null!;

    /// <summary>
    ///   The link opener used to open the OAuth2 authentication page on different platforms.
    /// </summary>
    private ILinkOpener _linkOpener = null!;

    /// <summary>
    ///   A random string used to prevent CSRF attacks on the OAuth2 flow.
    /// </summary>
    private string _state = string.Empty;


    private readonly string _clientID;
    private readonly int[] _redirectPorts;
    private readonly string _scope;

    #endregion

    #region Initialization

    // We use an Implicit grant flow app token to authenticate with the Twitch API. (No access token is required)
    public OAuth2Authenticator(string clientID, int[] redirectPorts, string scope, ILinkOpener linkOpener)
    {
        _clientID = clientID;
        _redirectPorts = redirectPorts;
        _scope = scope;

        _linkOpener = linkOpener;

        Port = GetFirstAvailablePort() ?? throw new Exception("No available ports to use for OAuth2 authentication.");

        Initialize();
    }

    private int? GetFirstAvailablePort()
    {
        TcpClient tcpClient = new();

        foreach (int port in _redirectPorts)
        {
            try
            {
                tcpClient.Connect("localhost", port);
            }
            catch (Exception)
            {
                return port;
            }
        }

        return null;
    }

    private void Initialize()
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://localhost:{Port}/");

        _client = new HttpClient();

        AuthenticationCompleted += (_, _) => _listener.Stop();
        AuthenticationFailed += (_, _) => _listener.Stop();
    }

    #endregion

    #region Events

    public event EventHandler<OAuth2AuthenticationEventArgs>? AuthenticationCompleted;

    public event EventHandler<ProcessErrorEventArgs>? AuthenticationFailed;

    #endregion

    #region Properties

    public int Port { get; private set; }

    #endregion

    #region Methods

    public void Authenticate()
    {
        _state = Guid.NewGuid().ToString();

        // Start the HTTP listener.
        _listener.Start();
        _ = ListenForConnectionsAsync();

        // Open the browser to the Twitch OAuth2 authentication page.
        var url = string.Format(AUTHORIZATION_URL_PATTERN, _clientID, Port, _scope, _state);

        // we open the authorization page and wait for the redirection
        _linkOpener.Open(url);
    }

    public string CheckTokenValidity(string accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, TEST_URL);

        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        request.Headers.Add("Client-Id", _clientID);

        var response = _client.Send(request);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            var users = JsonSerializer.Deserialize<Users>(responseContent);

            if (users != null && users.Data.Length == 1)
            {
                return users.Data[0].ID;
            }
        }

        return string.Empty;
    }

    #endregion

    #region HTTP Server

    private async Task ListenForConnectionsAsync()
    {
        while (_listener.IsListening)
        {
            var context = await _listener.GetContextAsync();
            AnswerHTTPRequest(context);
        }
    }

    private void AnswerHTTPRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        if (request.Url == null)
        {
            SendResponse(response, FAILURE_RESPONSE, 400);
            AuthenticationFailed?.Invoke(this, new ProcessErrorEventArgs("Authenticator", "The received request's URL is unexpectedly null."));
        }
        else
        {
            var url = request.Url;

            if (url.AbsolutePath != "/" || url.Query == "")
            {
                SendResponse(response, NORMAL_RESPONSE);
                return;
            }

            // we receive a fragment with the access token
            var fragmentParams = HttpUtility.ParseQueryString(request.Url.Query);

            var hasErrored = fragmentParams["error"] != null;
            var state = fragmentParams["state"];
            var accessToken = fragmentParams["access_token"];

            if (state == null || state != _state)
            {
                SendResponse(response, FAILURE_RESPONSE, 400);
                AuthenticationFailed?.Invoke(this, new ProcessErrorEventArgs("Authenticator", @"The received request's state is not the same as the one we sent, 
                                                                                                    The initial request might have been tampered with."));
                return;
            }

            if (hasErrored || accessToken == null || state == null)
            {
                SendResponse(response, FAILURE_RESPONSE, 400);

                string? description = fragmentParams["error_description"];

                string message = description != null ? $"Twitch provided the following error: {description}"
                                                     : "An error occured but Twitch provided no descriptions.";

                AuthenticationFailed?.Invoke(this, new ProcessErrorEventArgs("Authenticator", $"Twitch provided the following error: {description}"));
            }
            else
            {
                SendResponse(response, SUCCESS_RESPONSE);
                AuthenticationCompleted?.Invoke(this, new OAuth2AuthenticationEventArgs(_clientID,
                                                                                        accessToken,
                                                                                        "Bearer"));
            }
        }
    }

    private static void SendResponse(HttpListenerResponse response, string responseString, int code = 200)
    {
        var buffer = Encoding.UTF8.GetBytes(responseString);

        response.StatusCode = code;
        response.ContentLength64 = buffer.Length;
        response.ContentType = "text/html";

        response.Close(buffer, false);
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        _listener.Close();
        _client.Dispose();
    }

    #endregion
}
