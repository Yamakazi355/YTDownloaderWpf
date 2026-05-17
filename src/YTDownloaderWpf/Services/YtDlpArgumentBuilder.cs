using System.IO;
using YtDownloaderWpf.Models;

namespace YtDownloaderWpf.Services;

public class YtDlpArgumentBuilder
{
    public string Build(DownloadRequest request, string outputPath)
    {
        return request.Mode switch
        {
            DownloadMode.AudioOnly =>
                BuildAudioArguments(request, outputPath),

            DownloadMode.VideoOnly =>
                BuildVideoOnlyArguments(request, outputPath),

            DownloadMode.VideoWithAudio =>
                BuildVideoWithAudioArguments(request, outputPath),

            _ => throw new NotSupportedException()
        };
    }

    private string BuildAudioArguments(
        DownloadRequest request,
        string outputPath)
    {

        var ffmpegPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "ThirdParty", "bin");

        return
            $"-x " +
            $"--audio-format {request.OutputFormat} " +
            $"--ffmpeg-location \"{ffmpegPath}\" " +
            $"--newline " +
            $"--progress " +
            $"-o \"{outputPath}\" " +
            $"\"{request.Url}\"";
    }

    private string BuildVideoOnlyArguments(
        DownloadRequest request,
        string outputPath)
    {
        return
            $"-f bestvideo " +
            $"--recode-video {request.OutputFormat} " +
            $"--newline " +
            $"--progress " +
            $"-o \"{outputPath}\" " +
            $"\"{request.Url}\"";
    }

    private string BuildVideoWithAudioArguments(
        DownloadRequest request,
        string outputPath)
    {
        

        var qualityFilter = request.Quality switch
        {
            "1080p" => "[height<=1080]",
            "720p" => "[height<=720]",
            "480p" => "[height<=480]",
            _ => ""
        };

        return
            $"-f \"bv*{qualityFilter}+ba/b\" " +
            $"--merge-output-format {request.OutputFormat} " +
            $"--newline " +
            $"--progress " +
            $"-o \"{outputPath}\" " +
            $"\"{request.Url}\"";
    }
}