using System.IO;
using System.Text.Json;
using YtDownloaderWpf.Models;

namespace YtDownloaderWpf.Services;

public class YtDlpService
{
    private readonly ProcessService _processService;

    public YtDlpService()
    {
        _processService = new ProcessService();
    }

    private string YtDlpPath =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ThirdParty", "yt-dlp.exe");

    public async Task<string> GetVideoInfoRawAsync(string url)
    {
        var output = new List<string>();

        var arguments = $"-J \"{url}\"";

        await _processService.RunAsync(
            YtDlpPath,
            arguments,
            onOutput: line => output.Add(line));

        return string.Join(Environment.NewLine, output);
    }

    public async Task<VideoInfo?> GetVideoInfoAsync(string url)
    {
        var json = await GetVideoInfoRawAsync(url);

        return JsonSerializer.Deserialize<VideoInfo>(json);
    }
    public async Task RunCustomCommandAsync(
    string arguments,
    Action<string>? onOutput = null)
    {
        await _processService.RunAsync(
            YtDlpPath,
            arguments,
            onOutput);
    }
    public async Task DownloadFormatAsync(
    string url,
    string formatId,
    string outputPath,
    Action<string>? onOutput = null,
    Action<string>? onError = null)
    {
        var arguments =
            $"-f {formatId} " +
            $"--newline " +
            $"--progress " +
            $"-o \"{outputPath}\" " +
            $"\"{url}\"";

        await _processService.RunAsync(
            YtDlpPath,
            arguments,
            onOutput,
            onError);
    }
}