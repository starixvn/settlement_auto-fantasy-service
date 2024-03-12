using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectFantasy.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace DirectFantasy.WorkerService;

public class BasketBallFantasyHostedService : IHostedService
{
    private readonly DirectOSFantasyController directFantasyController;

    public BasketBallFantasyHostedService(
    DirectOSFantasyController directFantasyController,
    IConfiguration configuration,
    IMaintenanceService maintenanceService)
    {
        this.directFantasyController = directFantasyController;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await directFantasyController.ProcessAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Auto Direct Fantasy Service is stopping.");

        return Task.CompletedTask;
    }
}