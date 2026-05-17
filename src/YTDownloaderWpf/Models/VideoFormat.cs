using System.Text.Json.Serialization;

namespace YtDownloaderWpf.Models;

public class VideoFormat
{
    [JsonPropertyName("format_id")]
    public string? FormatId { get; set; }

    [JsonPropertyName("ext")]
    public string? Extension { get; set; }

    [JsonPropertyName("format")]
    public string? Format { get; set; }

    [JsonPropertyName("resolution")]
    public string? Resolution { get; set; }

    [JsonPropertyName("vcodec")]
    public string? VideoCodec { get; set; }

    [JsonPropertyName("acodec")]
    public string? AudioCodec { get; set; }

    [JsonPropertyName("filesize")]
    public long? FileSize { get; set; }
}