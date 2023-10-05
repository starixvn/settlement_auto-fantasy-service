using DirectFantasy.Core;
using DirectFantasy.Core.Models;
using DirectFantasy.Services;
using DirectFantasy.WorkerService;
using Fanex.Data.SqlServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFanexSql(builder.Configuration);
builder.Services.Configure<AppSettingsKey>(builder.Configuration);
builder.Services.AddCoreServices();
builder.Services.AddDirectFantasyServices();
builder.Services.AutoPayoutConfigServices(builder.Configuration);

builder.Services.AddHostedService<DirectFantasyHostedService>();

var app = builder.Build();

app.MapGet("/", () => "Fanex.DirectFantasy.Service");

#if DEBUG
app.Run();
#else
await app.RunAsync("http://0.0.0.0:5000");
#endif