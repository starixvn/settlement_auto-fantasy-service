using DirectFantasy.Services.Services;
using DirectPayout.Fantasy.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DirectFantasy.Services;

public static class DependenceInjections
{
    public static void AddDirectFantasyServices(this IServiceCollection service)
    {
        service.AddSingleton<DirectFantasyController, DirectFantasyControllerImp>();
        service.AddTransient<MatchService, MatchServiceImp>();
        service.AddTransient<MatchSettlementHandler, MatchSettlementHandlerImp>();
        service.AddTransient<CalcService, CalcServiceImp>();
        service.AddTransient<RefundService, RefundServiceImp>();
        service.AddTransient<ValidationMatchService, ValidationMatchServiceImp>();
    }
}