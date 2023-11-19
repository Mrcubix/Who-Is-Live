namespace WhoIsLive.UX.Interfaces;

public interface IAPIRequestHelper<T>
{
    string Url { get; }
    T Data { get; }

    string BuildUrl(string cursor);
}