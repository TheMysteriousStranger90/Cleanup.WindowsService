using Cleanup.WindowsService;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using CliWrap;
using static Cleanup.WindowsService.CleanupConstants;

if (PrivilegeManager.EnsureAdminPrivileges(true, ServiceName) == false)
{
    Environment.Exit(1);
    return;
}

if (args is { Length: 1 })
{
    try
    {
        string executablePath = Path.Combine(AppContext.BaseDirectory, "Cleanup.WindowsService.exe");

        if (args[0] is "/Install")
        {
            await Cli.Wrap("sc")
                .WithArguments(new[] { "create", ServiceName, $"binPath={executablePath}", "start=auto" })
                .ExecuteAsync();

            await Cli.Wrap("sc")
                .WithArguments(new[] { "description", ServiceName, ServiceDescription })
                .ExecuteAsync();

            await Cli.Wrap("reg")
                .WithArguments(new[]
                {
                    "add", $@"HKLM\SYSTEM\CurrentControlSet\Services\{ServiceName}\Parameters", "/v",
                    "Logging:LogLevel:Default", "/t", "REG_SZ", "/d", "Information", "/f"
                })
                .ExecuteAsync();

            await Cli.Wrap("reg")
                .WithArguments(new[]
                {
                    "add", $@"HKLM\SYSTEM\CurrentControlSet\Services\{ServiceName}\Parameters", "/v",
                    "EventLog:SourceName", "/t", "REG_SZ", "/d", "The Cleanup Windows Service", "/f"
                })
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

string logFilePath = WindowsServicePathHelperForLogs.GenerateWindowsServiceFilePath();

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<CleanupService>();
        services.AddHostedService<WindowsBackgroundService>();
    })
    .UseWindowsService(options =>
    {
        options.ServiceName = ServiceName;
    })
    .ConfigureLogging((context, logging) =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.AddEventLog();
        logging.AddProvider(new FileLoggerProvider(logFilePath));
    })
    .Build();

await host.RunAsync();