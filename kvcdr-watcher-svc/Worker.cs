using kvcdr_watcher_logic;

namespace kvcdr_watcher;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _config;
    private readonly FileSystemWatcher _watcher;

    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {
        _logger = logger;
        _config = configuration;
        _watcher = new FileSystemWatcher();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                var folderToWatch = Path.Combine(_config["FolderToWatch"]);

                if (_watcher.EnableRaisingEvents == false || (!_watcher.Path.Equals(folderToWatch, StringComparison.OrdinalIgnoreCase) && Directory.Exists(folderToWatch)))
                {
                    _logger.LogInformation($"watching {folderToWatch}");

                    _watcher.EnableRaisingEvents = false;
                    _watcher.Created -= OnFileCreated;

                    _watcher.Path = folderToWatch;
                    _watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
                    _watcher.IncludeSubdirectories = false;
                    _watcher.Created += OnFileCreated;

                    _watcher.EnableRaisingEvents = true;
                }

            }
            await Task.Delay(1000, stoppingToken);
        }
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation($"New file dropped: '{e.FullPath}'");

        var sourceFolder = Path.GetDirectoryName(e.FullPath);
        var archiveFolder = Path.Combine(sourceFolder, DateTime.Now.ToString("yyyy-MM-dd"));
        var archivedFileName = Path.Combine(archiveFolder, Path.GetFileName(e.FullPath));
        if (!Directory.Exists(archiveFolder))
        {
            Directory.CreateDirectory(archiveFolder);
        }
        File.Move(e.FullPath, archivedFileName, true);

        var configuration = new Dictionary<string, string>();
        configuration.Add("DROPPED_FILE_FULL_PATH", archivedFileName);
        var id = Guid.NewGuid().ToString();

        _logger.LogInformation($"Started EmailAttacher with ID='{id}' and DROPPED_FILE_FULL_PATH='{archiveFolder}'.");
        new EmailAttacher().Start(id, configuration);
        _logger.LogInformation($"Ended EmailAttacher with ID='{id}'");

    }
}
