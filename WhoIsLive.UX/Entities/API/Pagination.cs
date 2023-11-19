using System.Text.Json.Serialization;

namespace WhoIsLive.UX.Entities.API;

public class Pagination
{
    [JsonPropertyName("cursor")]
    public string Cursor { get; set; } = string.Empty;
}
