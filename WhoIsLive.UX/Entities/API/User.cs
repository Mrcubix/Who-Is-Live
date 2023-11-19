using System.Text.Json.Serialization;

namespace WhoIsLive.UX.Entities;

public class User
{
    [JsonPropertyName("id")]
    public string ID { get; set; } = string.Empty;

    [JsonPropertyName("login")]
    public string Login { get; set; } = string.Empty;

    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("broadcaster_type")]
    public string BroadcasterType { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("profile_image_url")]
    public string ProfileImageURL { get; set; } = string.Empty;

    [JsonPropertyName("offline_image_url")]
    public string OfflineImageURL { get; set; } = string.Empty;

    [JsonPropertyName("view_count")]
    public int ViewCount { get; set; } = 0;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = string.Empty;
}