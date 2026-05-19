using System.IO;

namespace YtDownloaderWpf.Services;

public class CookiesService
{
    private const string AppFolderName = "YtDownloaderWpf";
    private const string CookiesFileName = "cookies.txt";

    public string AppDataFolder =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            AppFolderName);

    public string CookiesPath =>
        Path.Combine(AppDataFolder, CookiesFileName);

    public bool HasCookies =>
        File.Exists(CookiesPath);

    public void ImportCookies(string sourceFilePath)
    {
        if (!File.Exists(sourceFilePath))
            throw new FileNotFoundException("Nie znaleziono pliku cookies.", sourceFilePath);

        Directory.CreateDirectory(AppDataFolder);

        File.Copy(sourceFilePath, CookiesPath, overwrite: true);
    }

    public void DeleteCookies()
    {
        if (File.Exists(CookiesPath))
        {
            File.Delete(CookiesPath);
        }
    }
}