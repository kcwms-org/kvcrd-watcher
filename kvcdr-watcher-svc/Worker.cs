using kvcdr_watcher_logic;

namespace kvcdr_watcher;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _config;
    private readonly FileSystemWatcher _watcher;
    private readonly EmailAttacher _emailAttacher1;

    public Worker(ILogger<Worker> logger, IConfiguration configuration, EmailAttacher emailAttacher)
    {
        _logger = logger;
        _config = configuration;
        _watcher = new FileSystemWatcher();
        _emailAttacher1 = emailAttacher;

        _logger.LogInformation($"kvcdrFileWatcher has been initialized at {DateTimeOffset.Now}");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                var folderToWatch = Path.Combine(_config["FolderToWatch"]);

                if (_watcher.EnableRaisingEvents == false || (!_watcher.Path.Equals(folderToWatch, StringComparison.OrdinalIgnoreCase) && Directory.Exists(folderToWatch)))
                {
                    _logger.LogInformation($"watching {folderToWatch}");

                    _watcher.EnableRaisingEvents = false;
                    _watcher.Created -= OnFileCreated;
                    _watcher.Disposed -= _watcher_Disposed;

                    _watcher.Path = folderToWatch;
                    //_watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
                    _watcher.IncludeSubdirectories = false;

                    _watcher.Created += OnFileCreated;
                    _watcher.Disposed += _watcher_Disposed;

                    _watcher.EnableRaisingEvents = true;
                }

                //handle any files already in the watch folder
                var existingFiles = Directory.GetFiles(folderToWatch, "*.*", SearchOption.TopDirectoryOnly);
                if (existingFiles.Length > 0)
                {
                    foreach (var file in existingFiles)
                    {
                        var args = new FileSystemEventArgs(WatcherChangeTypes.Created, folderToWatch, Path.GetFileName(file));
                        this.OnFileCreated(this, args);
                    }

                }
            }
            await Task.Delay(1000, stoppingToken);
        }
    }


    private void _watcher_Disposed(object? sender, EventArgs e)
    {
        _logger.LogInformation($"kvcdrFileWatcher disposed {DateTimeOffset.Now}");
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        _logger.LogWarning($"New file dropped: '{e.FullPath}'");

        var sourceFolder = Path.GetDirectoryName(e.FullPath);
        var archiveFolder = Path.Combine(sourceFolder, DateTime.Now.ToString("yyyy-MM-dd"));
        var archivedFileName = Path.Combine(archiveFolder, Path.GetFileName(e.FullPath));
        if (!Directory.Exists(archiveFolder))
        {
            Directory.CreateDirectory(archiveFolder);
        }
        File.Move(e.FullPath, archivedFileName, true);

        var configuration = new Dictionary<string, string>();
        configuration.Add(EmailAttacher.KEY_DROPPED_FILE_FULL_PATH, archivedFileName);
        var id = Guid.NewGuid().ToString();

        _logger.LogWarning($"Started EmailAttacher with ID='{id}' and DROPPED_FILE_FULL_PATH='{archivedFileName}'.");
        _emailAttacher1.Start(id, configuration);
        _logger.LogWarning($"Ended EmailAttacher with ID='{id}'");

    }
}
