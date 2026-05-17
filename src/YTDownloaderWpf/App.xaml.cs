using System.Windows;
using Velopack;
using YtDownloaderWpf.Services;

namespace YTDownloaderWpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        VelopackApp.Build().Run();

        base.OnStartup(e);

        var updateService = new UpdateService();

        await updateService.CheckForUpdatesAsync();
    }
}

