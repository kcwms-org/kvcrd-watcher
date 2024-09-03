
using System.Diagnostics;

namespace kvcdr_watcher_logic;

public class EmailAttacher : IKvcdrWorker
{
    static string KEY_DROPPED_FILE_FULL_PATH = "DROPPED_FILE_FULL_PATH";

    public void Start(string Id, IDictionary<string, string> ConfigurationSettings)
    {
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
            Process.Start(new ProcessStartInfo("thunderbird.exe", $"--compose \"attachment='{dropppedFile.Value}'\""));
        }

    }
}
