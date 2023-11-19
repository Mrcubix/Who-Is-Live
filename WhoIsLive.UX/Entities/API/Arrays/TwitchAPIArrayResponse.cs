using System.Text.Json.Serialization;

namespace WhoIsLive.UX.Entities.API.Arrays;

public class TwitchAPIArrayResponse<T>
{
    [JsonPropertyName("data")]
    public T[] Data { get; set; } = null!;
    
    [JsonPropertyName("pagination")]
    public Pagination Pagination { get; set; } = null!;
}
