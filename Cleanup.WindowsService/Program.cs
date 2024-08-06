using System;
using System.IO;
using Cleanup.WindowsService;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using CliWrap;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options => { options.ServiceName = "Cleanup Windows Service"; });

LoggerProviderOptions.RegisterProviderOptions<
    EventLogSettings, EventLogLoggerProvider>(builder.Services);

builder.Services.AddSingleton<CleanupService>();
builder.Services.AddHostedService<WindowsBackgroundService>();

builder.Logging.AddConfiguration(
    builder.Configuration.GetSection("Logging"));

const string ServiceName = "Cleanup Windows Service";
const string ServiceDescription = "Cleanup.WindowsService is a Windows service designed to perform various system cleanup tasks, such as emptying the recycle bin, cleaning temporary folders, and removing old log files.";

if (args is { Length: 1 })
{
    try
    {
        string executablePath =
            Path.Combine(AppContext.BaseDirectory, "Cleanup.WindowsService.exe");

        if (args[0] is "/Install")
        {
            await Cli.Wrap("sc")
                .WithArguments(new[] { "create", ServiceName, $"binPath={executablePath}", "start=auto" })
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