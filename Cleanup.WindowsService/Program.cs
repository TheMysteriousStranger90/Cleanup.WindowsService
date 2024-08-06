using Cleanup.WindowsService;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using CliWrap;
using static Cleanup.WindowsService.CleanupConstants;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

builder.Services.AddWindowsService(options => { options.ServiceName = ServiceName; });

LoggerProviderOptions.RegisterProviderOptions<
    EventLogSettings, EventLogLoggerProvider>(builder.Services);

builder.Services.AddSingleton<CleanupService>();
builder.Services.AddHostedService<WindowsBackgroundService>();

builder.Logging.AddConfiguration(
    builder.Configuration.GetSection("Logging"));

builder.Logging.AddEventLog(settings =>
{
    settings.SourceName = ServiceName;
});

string logFilePath = WindowsServicePathHelperForLogs.GenerateWindowsServiceFilePath();
builder.Logging.AddProvider(new FileLoggerProvider(logFilePath));

if (args is { Length: 1 })
{
    try
    {
        string executablePath =
            Path.Combine(AppContext.BaseDirectory, "Cleanup.WindowsService.exe");

        if (args[0] is "/Install")
        {
            await Cli.Wrap("sc")
                .WithArguments(new[] { "create", ServiceName, $"binPath={executablePath}", "start=auto", "obj=LocalSystem" })
                .ExecuteAsync();
            
            await Cli.Wrap("sc")
                .WithArguments(new[] { "description", ServiceName, ServiceDescription })
                .ExecuteAsync();
            
            await Cli.Wrap("reg")
                .WithArguments(new[] { "add", $@"HKLM\SYSTEM\CurrentControlSet\Services\{ServiceName}\Parameters", "/v", "Logging:LogLevel:Default", "/t", "REG_SZ", "/d", "Information", "/f" })
                .ExecuteAsync();

            await Cli.Wrap("reg")
                .WithArguments(new[] { "add", $@"HKLM\SYSTEM\CurrentControlSet\Services\{ServiceName}\Parameters", "/v", "EventLog:SourceName", "/t", "REG_SZ", "/d", "The Cleanup Windows Service", "/f" })
                .ExecuteAsync();
        }
        else if (args[0] is "/Uninstall")
        {
            await Cli.Wrap("sc")
                .WithArguments(new[] { "stop", ServiceName })
                .ExecuteAsync();

            await Cli.Wrap("sc")
                .WithArguments(new[] { "delete", ServiceName })
                .ExecuteAsync();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }

    return;
}

IHost host = builder.Build();
host.Run();