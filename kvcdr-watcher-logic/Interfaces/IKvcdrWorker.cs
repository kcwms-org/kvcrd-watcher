namespace kvcdr_watcher_logic;

public interface IKvcdrWorker
{
   /// <summary>
   /// Start the worker
   /// </summary>
   /// <param name="Id">a uniqueue id for this instance of the worker</param>
   /// <param name="ConfigurationSettings">a collection of configuration data</param>
   /// <exception cref="ArgumentNullException"> one or both input paraemters are null</exception>
    void Start(string Id, IDictionary<string, string> ConfigurationSettings);
}
