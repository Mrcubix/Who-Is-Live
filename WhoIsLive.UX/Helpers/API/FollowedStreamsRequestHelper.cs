using WhoIsLive.UX.Interfaces;

namespace WhoIsLive.UX.Helpers.API;

public class FollowedStreamsRequestHelper : IAPIRequestHelper<string>
{
    public FollowedStreamsRequestHelper(string url, string data)
    {
        Url = url;
        Data = data;
    }

    public string Url { get; set; } = string.Empty;
    public string Data { get; set; }

    public string BuildUrl(string cursor)
    {
        if (string.IsNullOrEmpty(cursor))
        {
            return Url + $"?user_id={Data}";
        }
        else
        {
            return $"{Url}?after={cursor}" + $"&user_id={Data}";
        }
    }
}