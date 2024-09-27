using DirectFantasy.Services;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using LogServiceSentry = Fanex.Logging.LogService;

namespace DirectFantasy.WorkerService;

public class BasketBallFantasyWorker(DirectOSFantasyController directFantasyController,
        IConfiguration configuration,
        IMaintenanceService maintenanceService)
    : BackgroundService
{
    private int counter;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(configuration.GetValue<int>("ServiceInterval", 3000)));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await HandleAutoAsync();
        }
    }

    private async Task HandleAutoAsync()
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
        if (counter >= configuration.GetValue<int>("GarbageCollectorInterval"))
        {
            counter = 0;
            GC.WaitForPendingFinalizers();
        }

        counter += configuration.GetValue<int>("ServiceInterval");
    }
}