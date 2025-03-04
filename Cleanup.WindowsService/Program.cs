using Cleanup.WindowsService;
using Cleanup.WindowsService.Factories;
using Cleanup.WindowsService.Helpers;
using Cleanup.WindowsService.Interfaces;
using Cleanup.WindowsService.Logging;
using Cleanup.WindowsService.Services;
using Cleanup.WindowsService.Tasks;
using CliWrap;
using CliWrap.Buffered;
using static Cleanup.WindowsService.Const.CleanupConstants;

if (PrivilegeManager.EnsureAdminPrivileges(true, ServiceName) == false)
{
    Console.WriteLine("This application requires administrator privileges to run.");
    Environment.Exit(1);
    return;
}

if (args is { Length: 1 })
{
    try
    {
        string executablePath = Path.Combine(AppContext.BaseDirectory, "Cleanup.WindowsService.exe");
        string command = args[0].ToLowerInvariant();

        if (command == "/install" || command == "-install" || command == "--install")
        {
            var checkResult = await Cli.Wrap("sc")
                .WithArguments(new[] { "query", ServiceName })
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();

            bool serviceExists = checkResult.ExitCode == 0;

            if (serviceExists)
            {
                Console.WriteLine($"Service '{ServiceName}' is already installed.");
                return;
            }

            Console.WriteLine($"Installing {ServiceName} service...");

            var createResult = await Cli.Wrap("sc")
                .WithArguments(new[]
                {
                    "create", ServiceName, $"binPath=\"{executablePath}\"", "start=auto",
                    "displayname=\"Windows System Cleanup Service\""
                })
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();

            if (createResult.ExitCode != 0)
            {
                Console.WriteLine($"Failed to create service. Exit code: {createResult.ExitCode}");
                Console.WriteLine($"Error: {createResult.StandardError}");
                return;
            }

            var descResult = await Cli.Wrap("sc")
                .WithArguments(new[] { "description", ServiceName, ServiceDescription })
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();

            if (descResult.ExitCode != 0)
            {
                Console.WriteLine($"Warning: Failed to set service description. Exit code: {descResult.ExitCode}");
            }

            var recoveryResult = await Cli.Wrap("sc")
                .WithArguments(new[]
                    { "failure", ServiceName, "reset=86400", "actions=restart/60000/restart/120000/restart/0" })
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();

            if (recoveryResult.ExitCode != 0)
            {
                Console.WriteLine(
                    $"Warning: Failed to set service recovery options. Exit code: {recoveryResult.ExitCode}");
            }

            var regLogResult = await Cli.Wrap("reg")
                .WithArguments(new[]
                {
                    "add", $@"HKLM\SYSTEM\CurrentControlSet\Services\{ServiceName}\Parameters", "/v",
                    "Logging:LogLevel:Default", "/t", "REG_SZ", "/d", "Information", "/f"
                })
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();

            if (regLogResult.ExitCode != 0)
            {
                Console.WriteLine($"Warning: Failed to set logging registry key. Exit code: {regLogResult.ExitCode}");
            }

            var regEventResult = await Cli.Wrap("reg")
                .WithArguments(new[]
                {
                    "add", $@"HKLM\SYSTEM\CurrentControlSet\Services\{ServiceName}\Parameters", "/v",
                    "EventLog:SourceName", "/t", "REG_SZ", "/d", "The Cleanup Windows Service", "/f"
                })
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();

            if (regEventResult.ExitCode != 0)
            {
                Console.WriteLine(
                    $"Warning: Failed to set event log registry key. Exit code: {regEventResult.ExitCode}");
            }

            Console.WriteLine($"Service '{ServiceName}' installed successfully.");
        }
        else if (command == "/uninstall" || command == "-uninstall" || command == "--uninstall")
        {
            Console.WriteLine($"Uninstalling {ServiceName} service...");

            var checkResult = await Cli.Wrap("sc")
                .WithArguments(new[] { "query", ServiceName })
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();

            if (checkResult.ExitCode != 0)
            {
                Console.WriteLine($"Service '{ServiceName}' is not installed.");
                return;
            }

            Console.WriteLine("Stopping service...");
            var stopResult = await Cli.Wrap("sc")
                .WithArguments(new[] { "stop", ServiceName })
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();

            if (stopResult.ExitCode != 0)
            {
                Console.WriteLine("Warning: Service may not have been stopped properly. Continuing with uninstall...");
            }

            await Task.Delay(2000);

            var deleteResult = await Cli.Wrap("sc")
                .WithArguments(new[] { "delete", ServiceName })
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();

            if (deleteResult.ExitCode != 0)
            {
                Console.WriteLine($"Failed to uninstall service. Exit code: {deleteResult.ExitCode}");
                Console.WriteLine($"Error: {deleteResult.StandardError}");
                return;
            }

            Console.WriteLine($"Service '{ServiceName}' uninstalled successfully.");
        }
        else
        {
            Console.WriteLine("Invalid command. Use /install or /uninstall.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        Console.WriteLine(ex.StackTrace);
    }

    return;
}

try
{
    string logFilePath = WindowsServicePathHelperForLogs.GenerateWindowsServiceFilePath();

    IHost host = Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        })
        .ConfigureServices((context, services) =>
        {
            // Register core services
            services.AddSingleton<IFileOperations, FileOperations>();
            services.AddSingleton<IProcessRunner, ProcessRunner>();

            // Register task factories
            services.AddSingleton<ICleanupTaskFactory, SystemCleanupTaskFactory>();
            services.AddSingleton<ICleanupTaskFactory, BrowserCleanupTaskFactory>();

            // Register system-related tasks
            services.AddTransient<RecycleBinCleanupTask>();
            services.AddTransient<EventLogCleanupTask>();
            services.AddTransient<DnsResolverCacheTask>();
            services.AddTransient<DismOperationsTask>();
            services.AddTransient<SystemRestorePointsTask>();
            services.AddTransient<WindowsUpdateCacheTask>();
            services.AddTransient<ThumbnailCacheCleanupTask>();
            services.AddTransient<MemoryDumpCleanupTask>();
            services.AddTransient<SystemFileCheckerTask>();

            // Register browser-related tasks
            services.AddTransient<ChromeCleanupTask>();
            services.AddTransient<FirefoxCleanupTask>();
            services.AddTransient<InternetExplorerCleanupTask>();

            // Register orchestrator and background service
            services.AddSingleton<CleanupOrchestrator>();
            services.AddHostedService<WindowsBackgroundService>();
        })
        .UseWindowsService(options => { options.ServiceName = ServiceName; })
        .ConfigureLogging((context, logging) =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.AddEventLog();
            logging.AddProvider(new FileLoggerProvider(logFilePath));
        })
        .Build();

    await host.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Critical error during service startup: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    Environment.Exit(1);
}