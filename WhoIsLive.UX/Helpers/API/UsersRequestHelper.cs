using System;
using System.Text;
using WhoIsLive.UX.Interfaces;

namespace WhoIsLive.UX.Helpers.API;

public class UsersRequestHelper : IAPIRequestHelper<string[]>
{
    private int _currentIndex = 0;

    public UsersRequestHelper(string url, string[] data)
    {
        Url = url;
        Data = data;
    }

    public string Url { get; set; } = string.Empty;
    public string[] Data { get; set; }

    public string BuildUrl(string cursor)
    {
        StringBuilder builder = new();

        builder.Append(Url);
        builder.Append("?id=");

        int nextTargetIndex = Math.Min(_currentIndex + 100, Data.Length);

        for (int i = _currentIndex; i < nextTargetIndex; i++)
        {
            if (_currentIndex++ > 0)
                builder.Append("&id=");

            builder.Append(Data[i]);
        }

        return builder.ToString();
    }
}