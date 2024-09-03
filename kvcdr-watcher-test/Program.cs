using kvcdr_watcher_logic;
namespace kvcdr_watcher_test
{
    public class Program
    {
        static public void Main(string[] args)
        {
            var destPath = Path.GetTempFileName();
            using (var f = new StreamWriter(destPath))
            {
                f.WriteLine("this is an attachment");
            }
            var configuration = new Dictionary<string, string>();
            configuration.Add("DROPPED_FILE_FULL_PATH", destPath);

            new EmailAttacher().Start(Guid.NewGuid().ToString(), configuration);

        }
    }
}