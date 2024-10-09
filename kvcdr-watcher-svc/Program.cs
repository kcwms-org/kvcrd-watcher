using NReco.Logging.File;
using kvcdr_watcher;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using kvcdr_watcher_logic;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddWindowsService(opt=>{
            opt.ServiceName = "kvcdrFileWatcher";
        });

        LoggerProviderOptions.RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>(builder.Services);
        
        builder.Services.AddSingleton<EmailAttacher>();
        builder.Services.AddHostedService<Worker>();
        builder.Services.AddLogging(lb =>
        {
            lb.AddFile(builder.Configuration.GetSection("Logging"));
        });
        

        var host = builder.Build();
        host.Run();
    }
}