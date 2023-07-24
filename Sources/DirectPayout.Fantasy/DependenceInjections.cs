using DirectFantasy.Core.Services;
using Fanex.Data.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace DirectFantasy.Core;

public static class DependenceInjections
{
    public static void AddCoreServices(this IServiceCollection service)
    {
        service.AddSingleton<IDynamicRepository, DynamicRepository>();
        service.AddTransient<LogService, LogServiceImp>();
        service.AddTransient<IMaintenanceService, MaintenanceServiceImp>();
    }
}