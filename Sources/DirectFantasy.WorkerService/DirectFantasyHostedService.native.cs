using DirectFantasy.Services;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using LogServiceSentry = Fanex.Logging.LogService;

namespace DirectFantasy.WorkerService;

public class DirectFantasyHostedService(
    DirectFantasyController directFantasyController,
    IConfiguration configuration,
    IMaintenanceService maintenanceService)
    : BackgroundService
{
    private int counter;

    private async Task MainProcess()
    {
        try
        {
            if (!maintenanceService.IsMaintenance)
            {
                await directFantasyController.ProcessAsync();
                HandleGarbageCollector();
            }
        }
        catch (Exception exception)
        {
            var logMessage = new LogMessage(exception.Message)
              .AddSection("Database Info", DatabaseInfoGetters.GetDbExceptionInfo(exception))
              .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
              .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(exception))
              .ToString();
            await LogServiceSentry.ErrorAsync(logMessage);
        }
    }

    private void HandleGarbageCollector()
    {
        var value = configuration.GetSection("GarbageCollectorInterval").Value;
        if (value != null && counter >= int.Parse(value))
        {
            counter = 0;
            GC.WaitForPendingFinalizers();
        }

        var interval = configuration.GetSection("ServiceInterval").Value;
        if (interval != null)
        {
            counter += int.Parse(interval);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(configuration.GetValue("ServiceInterval", 3000)));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await MainProcess();
        }
    }
}