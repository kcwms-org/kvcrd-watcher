using System.Diagnostics;
using kvcdr_watcher_logic;
using Microsoft.Win32.SafeHandles;

namespace kvcdr_watcher;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return base.StartAsync(cancellationToken);

        var folderToWatch = @"C:\Users\Quickemu\Desktop\Scans";
        using (var watcher = new FileSystemWatcher(folderToWatch))
        {
            watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite;
            watcher.IncludeSubdirectories = false;

            watcher.Created += OnFileCreated;
        }

        return Task.CompletedTask;
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        if (File.Exists(e.FullPath))
        {
            var process = new ProcessStartInfo("thunderbird.exe", $"--compose \"attachment='{e.FullPath}'\"");
        }
        var destFolder = Path.GetDirectoryName(e.FullPath) ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var destPath = Path.Combine(destFolder, DateTime.Now.ToString("u"), Path.GetFileName(e.FullPath));
        if (!Directory.Exists(destFolder))
        {
            Directory.CreateDirectory(destFolder);
        }
        File.Move(e.FullPath, destPath, true);

        var configuration = new Dictionary<string, string>();
        configuration.Add("DROPPED_FILE_FULL_PATH", destPath);

        new EmailAttacher().Start(Guid.NewGuid().ToString(), configuration);

    }
}
