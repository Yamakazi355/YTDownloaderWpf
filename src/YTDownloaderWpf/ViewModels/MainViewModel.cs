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
        DownloadFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads");
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
        SelectDownloadFolderCommand = new RelayCommand(SelectDownloadFolder);
        CancelDownloadCommand = new RelayCommand(CancelDownload);
    }

    private string formatUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return string.Empty;
        var index = url.IndexOf('&');
        return index > 0 ? url.Substring(0, index) : url;
    }
    public string? Url
    {
        get => _url;
        set
        {
            SetProperty(ref _url, formatUrl(value));
            if (string.IsNullOrEmpty(value))
                return;
            if (value.Length > 30 && value.Contains("watch?"))
            {
                VideoTitle = string.Empty;
                Uploader = string.Empty;
                ThumbnailUrl = ThumbnailPlaceholder;
                LogText = string.Empty;
                Progress = 0;
                StatusText = string.Empty;
                Task.Run(LoadVideoInfoAsync);
            }
            
        }
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
        set
        {
            SetProperty(ref _isLoading, value);
            OnPropertyChanged(nameof(LockUrlInput));
        }
    }

    public bool LockUrlInput => _isLoading != true && _isDownloading != true;

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
    private string? _statusText;
    private bool _isDownloading;
    private string? _logText;
    public string? StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public bool IsDownloading
    {
        get => _isDownloading;
        set
        {
            SetProperty(ref _isDownloading, value);
            OnPropertyChanged(nameof(LockUrlInput));
        }
    }

    public string? LogText
    {
        get => _logText;
        set => SetProperty(ref _logText, value);
    }
    private string? _downloadFolder;
    public string? DownloadFolder
    {
        get => _downloadFolder;
        set => SetProperty(ref _downloadFolder, value);
    }
    private const string ThumbnailPlaceholder = "pack://application:,,,/Resources/Images/thumbnail-placeholder.jpg";
    private string? _thumbnailUrl = ThumbnailPlaceholder;
    public string? ThumbnailUrl
    {
        get => _thumbnailUrl;
        set => SetProperty(ref _thumbnailUrl, value);
    }

    private CancellationTokenSource? _downloadCancellationTokenSource;
    public IAsyncRelayCommand LoadVideoInfoCommand { get; }
    public IAsyncRelayCommand DownloadCommand { get; }
    public IRelayCommand SelectDownloadFolderCommand { get; }
    public IRelayCommand CancelDownloadCommand { get; }
    private async Task LoadVideoInfoAsync()
    {
        if (string.IsNullOrWhiteSpace(Url))
            return;

        try
        {
            IsLoading = true;
            StatusText = "Ładowanie informacji o wideo...";
            var info = await _ytDlpService.GetVideoInfoAsync(Url);

            if (info == null)
                return;

            CurrentVideoInfo = info;

            VideoTitle = info.Title;
            Uploader = info.Uploader;
            ThumbnailUrl = string.IsNullOrWhiteSpace(info.Thumbnail)
                ? ThumbnailPlaceholder
                : info.Thumbnail;
        }
        finally
        {
            StatusText = string.Empty;
            IsLoading = false;
        }
    }

    private void CancelDownload()
    {
        _downloadCancellationTokenSource?.Cancel();
    }
    private void SelectDownloadFolder()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "Wybierz folder zapisu"
        };

        if (dialog.ShowDialog() == true)
        {
            DownloadFolder = dialog.FolderName;
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
        if (IsDownloading)
            return;
        await LoadVideoInfoAsync();
        try
        {
            IsDownloading = true;
            StatusText = "Przygotowywanie pobierania...";
            LogText = "";
            Progress = 0;
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

            var downloadsFolder = DownloadFolder;

            if (string.IsNullOrWhiteSpace(downloadsFolder))
                return;

            Directory.CreateDirectory(downloadsFolder);

            var outputPath = Path.Combine(
                downloadsFolder,
                "%(title)s.%(ext)s");

            var arguments = _argumentBuilder.Build(request, outputPath);

            StatusText = "Pobieranie / konwersja...";
            _downloadCancellationTokenSource = new CancellationTokenSource();
            await _ytDlpService.RunCustomCommandAsync(
                arguments,
                HandleProgress,
                _downloadCancellationTokenSource.Token);

            StatusText = "Gotowe.";
            Progress = 100;
        }
        catch (OperationCanceledException)
        {
            StatusText = "Pobieranie anulowane.";
        }
        catch (Exception ex)
        {
            StatusText = "Wystąpił błąd.";
            LogText += Environment.NewLine + ex.Message;
        }
        finally
        {
            _downloadCancellationTokenSource?.Dispose();
            _downloadCancellationTokenSource = null;
            IsDownloading = false;
        }
    }


    private void HandleProgress(string line)
    {
        LogText += line + Environment.NewLine;

        if (line.Contains("[ExtractAudio]"))
            StatusText = "Konwersja audio...";

        if (line.Contains("[Merger]"))
            StatusText = "Łączenie audio i wideo...";

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