using DirectFantasy.Core.Services;
using DirectFantasy.Services.Services;
using LogService = Fanex.Logging.LogService;

namespace DirectFantasy.WorkerService;

public class DirectFantasyHostedService : IHostedService
{
    private readonly DirectFantasyController directFantasyController;

    private readonly IConfiguration configuration;

    private readonly IMaintenanceService maintenanceService;

    private bool running;

    private int counter;

    public DirectFantasyHostedService(
        DirectFantasyController directFantasyController,
        IConfiguration configuration,
        IMaintenanceService maintenanceService)
    {
        this.directFantasyController = directFantasyController;
        this.configuration = configuration;
        this.maintenanceService = maintenanceService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        running = true;
        var autoThread = new Thread(ThreadProcess);
        Console.WriteLine($"Auto Direct Fantasy Starting at {DateTime.Now}");
        autoThread.Start();
        return Task.CompletedTask;
    }

    private void ThreadProcess()
    {
        while (running)
        {
            if (maintenanceService.IsMaintenance)
            {
                continue;
            }

            MainProcess();

            var value = configuration.GetSection("ServiceInterval").Value;
            if (value != null)
            {
                Thread.Sleep(int.Parse(value));
            }
        }
    }

    private void MainProcess()
    {
        try
        {
            directFantasyController.ProcessAsync();
            HandleGarbageCollector();
        }
        catch (Exception exception)
        {
            LogService.Error("Error from Auto Direct Fantasy Service: ", exception);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Auto Direct Fantasy Service is stopping.");

        return Task.CompletedTask;
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
}