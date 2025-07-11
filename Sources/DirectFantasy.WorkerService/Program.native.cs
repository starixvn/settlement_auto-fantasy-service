using DirectFantasy;
using DirectFantasy.Models;
using DirectFantasy.WorkerService;
using Fanex.Data.SqlServer;
using Fanex.Logging.Sentry.RavenClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ONEbook.UM.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddOneBookUmForService(builder.Configuration);
services.AddFanexLogSentry(builder.Configuration);
services.AddFanexSql(builder.Configuration);
services.Configure<AppSettingsKey>(builder.Configuration);
services.AddDirectFantasyServices();
services.AutoPayoutConfigServices(builder.Configuration);

services.AddHostedService<DirectFantasyHostedService>();
services.AddHostedService<BasketBallFantasyWorker>();
services.AddHealthChecks();

var app = builder.Build();
app.MapHealthChecks("/healthz");

await app.RunAsync();