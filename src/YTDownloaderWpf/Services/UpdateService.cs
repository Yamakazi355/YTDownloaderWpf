using Velopack;
using Velopack.Sources;

namespace YtDownloaderWpf.Services;

public class UpdateService
{
    private readonly UpdateManager _updateManager;

    public UpdateService()
    {
        var source = new GithubSource(
            "https://github.com/Yamakazi355/YtDownloaderWpf",
            accessToken: null,
            prerelease: false);

        _updateManager = new UpdateManager(source);
    }

    public async Task CheckForUpdatesAsync()
    {
        try
        {
            var newVersion = await _updateManager.CheckForUpdatesAsync();

            if (newVersion == null)
                return;

            await _updateManager.DownloadUpdatesAsync(newVersion);

            _updateManager.ApplyUpdatesAndRestart(newVersion);
        }
        catch
        {
            // później dodamy logowanie
        }
    }
}