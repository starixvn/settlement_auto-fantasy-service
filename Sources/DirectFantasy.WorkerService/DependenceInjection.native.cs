using AutoMapper;
using DirectFantasy.WorkerService.Profiles;
using Fanex.Caching;
using Fanex.Caching.Redis;
using Fanex.Settlement.ScaleOut;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DirectFantasy.WorkerService
{
    public static class DependenceInjection
    {
        public static void AutoPayoutConfigServices(this IServiceCollection service, IConfiguration configuration)
        {
            service.AddAutoMapperConfig();
            service.AddRedis(configuration);
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
    }
}