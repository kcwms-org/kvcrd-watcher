
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace kvcdr_watcher_logic;

public class EmailAttacher : IKvcdrWorker
{
    public static string KEY_DROPPED_FILE_FULL_PATH = "DROPPED_FILE_FULL_PATH";
    private readonly ILogger<EmailAttacher> _logger;

    public EmailAttacher(ILogger<EmailAttacher> logger)
    {
        _logger = logger;
    }

    public void Start(string Id, IDictionary<string, string> ConfigurationSettings)
    {
        _logger.LogInformation($"Start({Id}, {JsonSerializer.Serialize<IDictionary<string, string>>(ConfigurationSettings)}");
        if (string.IsNullOrWhiteSpace(Id))
        {
            throw new ArgumentNullException(nameof(Id));
        }
        if (ConfigurationSettings == null || !ConfigurationSettings.Any())
        {
            throw new ArgumentNullException(nameof(ConfigurationSettings));
        }

        var dropppedFile = ConfigurationSettings.FirstOrDefault(c => c.Key.Equals(KEY_DROPPED_FILE_FULL_PATH, StringComparison.InvariantCultureIgnoreCase));
        if (string.IsNullOrWhiteSpace(dropppedFile.Value))
        {
            throw new ArgumentException($"parameter {nameof(ConfigurationSettings)} doest not contain a non-null value for key {KEY_DROPPED_FILE_FULL_PATH}");
        }

        if (File.Exists(dropppedFile.Value))
        {
            _logger.LogInformation($"Start(): starting with '{dropppedFile.Value}'");
            var psi = new ProcessStartInfo("thunderbird.exe", $"--compose \"attachment='{dropppedFile.Value}'\"");
            //psi.WorkingDirectory = Path.GetTempPath();
            psi.UseShellExecute = true;
            psi.WorkingDirectory = Path.GetDirectoryName(path: Path.GetDirectoryName(dropppedFile.Value));
            
            using (var x = new Process())
            {
                x.Exited += X_Exited;
                x.ErrorDataReceived += X_ErrorDataReceived;

                x.StartInfo = psi;
                var isStarted = x.Start();

                _logger.LogInformation("p.Start() returned {isStarted} : '{processName}' '{processId}' '{workingDirectory}' '{userName}'"
                    , new string[] {
                        isStarted.ToString()
                        ,x.ProcessName
                        , x.Id.ToString()
                        , x.StartInfo.WorkingDirectory
                        , x.StartInfo.UserName
                    });

                x.WaitForExit();
                if (x != null)
                {
                    x.Kill(true);                    
                }
            }
            _logger.LogInformation($"Start() ended");
        }

    }

    private void X_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        _logger.LogError($"Start: thunderbird.exe errored : '{e.Data ?? "<noting to errorstream>"}'");
    }

    private void X_Exited(object? sender, EventArgs e)
    {
        _logger.LogInformation($"Start: Exiting {sender?.GetType().Name}: {DateTimeOffset.Now}");
    }
}
