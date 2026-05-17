using System.Windows;
using YtDownloaderWpf.ViewModels;

namespace YtDownloaderWpf;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        DataContext = new MainViewModel();
    }
}