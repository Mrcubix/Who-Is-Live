using System.Text.Json.Serialization;

namespace WhoIsLive.UX.Entities.API;

public class LiveStream
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("user_login")]
    public string UserLogin { get; set; } = string.Empty;

    [JsonPropertyName("user_name")]
    public string UserName { get; set; } = string.Empty;

    [JsonPropertyName("game_id")]
    public string GameId { get; set; } = string.Empty;

    [JsonPropertyName("game_name")]
    public string GameName { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("viewer_count")]
    public int ViewerCount { get; set; }

    [JsonPropertyName("started_at")]
    public string StartedAt { get; set; } = string.Empty;

    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    [JsonPropertyName("thumbnail_url")]
    public string ThumbnailUrl { get; set; } = string.Empty;

    [JsonPropertyName("tag_ids")]
    public string[] TagIds { get; set; } = null!;

    [JsonPropertyName("is_mature")]
    public bool IsMature { get; set; } = false;
}