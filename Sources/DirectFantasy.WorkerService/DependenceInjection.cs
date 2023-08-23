using AutoMapper;
using DirectFantasy.WorkerService.Profiles;
using Fanex.Caching;
using Fanex.Caching.Redis;
using Fanex.Data;
using Fanex.Data.SqlServer;
using Fanex.Logging;
using Fanex.Logging.Sentry.RavenClient;
using Fanex.Settlement.ScaleOut;
using ONEbook.UM;
using ONEbook.UM.AspNetCore;
using ONEbook.UM.Configurations;
using ONEbook.UM.Helpers;
using ONEbook.UM.Transport;
using SharpRaven;

namespace DirectFantasy.WorkerService
{
    public static class DependenceInjection
    {
        public static void AutoPayoutConfigServices(this IServiceCollection service, IConfiguration configuration)
        {
            service.AddAutoMapperConfig();
            service.AddUm(configuration);
            service.AddRedis(configuration);

            StartUmSetting(configuration);
            StartDataBaseSetting(configuration);
            StartLogSetting(configuration);
        }

        private static void AddUm(this IServiceCollection service, IConfiguration configuration)
        {
            service.AddSingleton(new UMClientConfiguration().Initialize(configuration));
            service.AddSingleton<IUMCacheService, UMCacheService>();
            service.AddSingleton<IUMRestClient, UMRestClient>();
            service.AddSingleton<ONEbook.UM.Services.IMaintenanceService, ONEbook.UM.Services.MaintenanceService>();
        }

        private static void AddRedis(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetValue<string>("Redis:ConnectionString");
            var applicationName = configuration.GetValue<string>("Redis:ApplicationName");

            services.AddSingleton<ICache>(new RedisCache(new RedisCacheOptions
            {
                ConnectionString = connectionString,
                ApplicationName = applicationName,
            }));
            services.AddSingleton<ICacheService>(new CacheService());
        }

        private static void AddAutoMapperConfig(this IServiceCollection service)
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(typeof(AutoMapperProfile));
                mc.AddProfile(typeof(ScaleOutMappingProfile));
            });

            var mapper = mapperConfig.CreateMapper();

            service.AddSingleton(mapper);
        }

        private static void StartUmSetting(IConfiguration configuration)
        {
            var umConfig = new UMClientConfiguration().Initialize(configuration);

            UMClientManager
                .UseConfig(umConfig)
                .UseCacheService(new UMCacheService(new CacheService()))
                .UseRestClient(new UMRestClient(umConfig));
        }

        private static void StartDataBaseSetting(IConfiguration configuration)
        {
            var connections = configuration.GetSection("ConnectionStrings")
                .GetChildren()
                .AsEnumerable()
                .ToDictionary(
                    x => x.GetValue<string>("Name") ?? string.Empty,
                    y => new ConnectionConfiguration(y.GetValue<string>("Name"), y.GetValue<string>("ConnectionString"), y.GetValue<string>("ProviderName"))
                );
            DbSettingProviderManager
                .StartNewSession()
                .Use(connections)
                .WithSqlServer()
                .UseDefaultDbSettingProvider(resourcePath: configuration.GetSection("StoredProcedurePath").Value)
                .Run();
        }

        private static void StartLogSetting(IConfiguration configuration)
        {
            var logEngine = new List<ILogEngine>();

            var sentryEngineOptions = new SentryEngineOptions
            {
                Dsn = new Dsn(configuration.GetSection("SentryToken").Value)
            };

            logEngine.Add(new SentryLogging(sentryEngineOptions));

            LogManager.SetDefaultLogCategory(configuration.GetSection("SiteCategory").Value).Use(logEngine);
        }
    }
}