using System.Text.Json.Serialization;

namespace YtDownloaderWpf.Models;

public class VideoInfo
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("uploader")]
    public string? Uploader { get; set; }

    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    [JsonPropertyName("formats")]
    public List<VideoFormat>? Formats { get; set; }
}