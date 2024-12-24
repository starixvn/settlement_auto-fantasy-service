using DirectFantasy;
using DirectFantasy.Models;
using DirectFantasy.WorkerService;
using Fanex.Data.SqlServer;
using Fanex.Logging.Sentry.RavenClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ONEbook.UM.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOneBookUm(builder.Configuration);
builder.Services.AddFanexLogSentry(builder.Configuration);
builder.Services.AddFanexSql(builder.Configuration);
builder.Services.Configure<AppSettingsKey>(builder.Configuration);
builder.Services.AddDirectFantasyServices();
builder.Services.AutoPayoutConfigServices(builder.Configuration);

builder.Services.AddHostedService<DirectFantasyHostedService>();
builder.Services.AddHostedService<BasketBallFantasyWorker>();

var app = builder.Build();

app.Run(async context =>
{
    await context.Response.WriteAsync("OK");
});

await app.RunAsync();