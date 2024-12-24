using DirectFantasy.Services;
using Microsoft.AspNetCore.Http;

namespace DirectFantasy;

public static class DependenceInjections
{
    public static void AddDirectFantasyServices(this IServiceCollection service)
    {
        service.AddSingleton<IDynamicRepository, DynamicRepository>();
        service.AddTransient<LogService, LogServiceImp>();
        service.AddSingleton<ScaleOutSettleService, ScaleOutSettleServiceImp>();
        service.AddTransient<GeneralService, GeneralServiceImp>();
        service.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        service.AddTransient<DirectFantasyController, DirectFantasyControllerImp>();
        service.AddSingleton<MatchService, MatchServiceImp>();
        service.AddTransient<MatchSettlementHandler, MatchSettlementHandlerImp>();
        service.AddTransient<CalcService, CalcServiceImp>();
        service.AddTransient<RefundService, RefundServiceImp>();
        service.AddTransient<ValidationMatchService, ValidationMatchServiceImp>();
        service.AddTransient<XmlConvertService, XmlConvertServiceImp>();
        service.AddTransient<CriteriaService, CriteriaServiceImp>();
        service.AddTransient<ResultDetailService, ResultDetailServiceImp>();

        service.AddSingleton<OSMatchService, OSMatchServiceImp>();
        service.AddTransient<DirectOSFantasyController, DirectOSFantasyControllerImp>();
        service.AddTransient<OSMatchSettlementHandler, OSMatchSettlementHandlerImp>();
        service.AddTransient<RefundOSService, RefundOSServiceImp>();
        service.AddTransient<CalcOSService, CalcOSServiceImp>();
    }
}