using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using YtDownloaderWpf.Models;
using YtDownloaderWpf.Services;

namespace YtDownloaderWpf.ViewModels;

public class MainViewModel : ObservableObject
{
    private readonly YtDlpService _ytDlpService;

    private string? _url;
    private string? _videoTitle;
    private string? _uploader;
    private double _progress;
    private bool _isLoading;
    private string? _selectedDownloadMode;
    private string? _selectedOutputFormat;

    public MainViewModel()
    {
        _ytDlpService = new YtDlpService();

        DownloadModes =
        [
            "Video + Audio",
            "Audio Only",
            "Video Only"
        ];

        OutputFormats = [];

        UpdateOutputFormats();

        SelectedDownloadMode = DownloadModes[0];
        Qualities =
        [
            "Best",
            "1080p",
            "720p",
            "480p"
        ];
        _argumentBuilder = new YtDlpArgumentBuilder();
        SelectedQuality = Qualities[0];
        LoadVideoInfoCommand = new AsyncRelayCommand(LoadVideoInfoAsync);
        DownloadCommand = new AsyncRelayCommand(DownloadAsync);
    }

    public string? Url
    {
        get => _url;
        set => SetProperty(ref _url, value);
    }

    public string? VideoTitle
    {
        get => _videoTitle;
        set => SetProperty(ref _videoTitle, value);
    }

    public string? Uploader
    {
        get => _uploader;
        set => SetProperty(ref _uploader, value);
    }

    public double Progress
    {
        get => _progress;
        set => SetProperty(ref _progress, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string? SelectedDownloadMode
    {
        get => _selectedDownloadMode;
        set
        {
            if (SetProperty(ref _selectedDownloadMode, value))
            {
                UpdateOutputFormats();
            }
        }
    }

    public string? SelectedOutputFormat
    {
        get => _selectedOutputFormat;
        set => SetProperty(ref _selectedOutputFormat, value);
    }


    private string? _selectedQuality;

    public string? SelectedQuality
    {
        get => _selectedQuality;
        set => SetProperty(ref _selectedQuality, value);
    }
    public List<string> Qualities { get; }
    private readonly YtDlpArgumentBuilder _argumentBuilder;
    public List<string> DownloadModes { get; }

    public ObservableCollection<string> OutputFormats { get; }

    public VideoInfo? CurrentVideoInfo { get; set; }

    public IAsyncRelayCommand LoadVideoInfoCommand { get; }
    public IAsyncRelayCommand DownloadCommand { get; }
    private async Task LoadVideoInfoAsync()
    {
        if (string.IsNullOrWhiteSpace(Url))
            return;

        try
        {
            IsLoading = true;

            var info = await _ytDlpService.GetVideoInfoAsync(Url);

            if (info == null)
                return;

            CurrentVideoInfo = info;

            VideoTitle = info.Title;
            Uploader = info.Uploader;
        }
        finally
        {
            IsLoading = false;
        }
    }
    private void UpdateOutputFormats()
    {
        OutputFormats.Clear();

        switch (SelectedDownloadMode)
        {
            case "Audio Only":

                OutputFormats.Add("mp3");
                OutputFormats.Add("wav");
                OutputFormats.Add("m4a");
                OutputFormats.Add("flac");
                OutputFormats.Add("opus");

                break;

            case "Video Only":

                OutputFormats.Add("mp4");
                OutputFormats.Add("webm");
                OutputFormats.Add("mkv");

                break;

            default:

                OutputFormats.Add("mp4");
                OutputFormats.Add("webm");
                OutputFormats.Add("mkv");

                break;
        }

        SelectedOutputFormat = OutputFormats.FirstOrDefault();
    }
    private async Task DownloadAsync()
    {
        if (string.IsNullOrWhiteSpace(Url))
            return;

        var request = new DownloadRequest
        {
            Url = Url,
            OutputFormat = SelectedOutputFormat ?? "mp4",
            Quality = SelectedQuality ?? "Best",
            Mode = SelectedDownloadMode switch
            {
                "Audio Only" => DownloadMode.AudioOnly,
                "Video Only" => DownloadMode.VideoOnly,
                _ => DownloadMode.VideoWithAudio
            }
        };

        var downloadsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads");

        Directory.CreateDirectory(downloadsFolder);

        var outputPath = Path.Combine(
            downloadsFolder,
            "%(title)s.%(ext)s");

        var arguments = _argumentBuilder.Build(request, outputPath);

        Progress = 0;

        await _ytDlpService.RunCustomCommandAsync(
            arguments,
            HandleProgress);
    }


    private void HandleProgress(string line)
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            line,
            @"(\d{1,3}\.\d)%");

        if (!match.Success)
            return;

        if (double.TryParse(
            match.Groups[1].Value,
            System.Globalization.CultureInfo.InvariantCulture,
            out var progress))
        {
            Progress = progress;
        }
    }
}