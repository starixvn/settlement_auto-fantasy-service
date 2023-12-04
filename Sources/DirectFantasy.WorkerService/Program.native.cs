using DirectFantasy;
using DirectFantasy.Models;
using DirectFantasy.WorkerService;
using Fanex.Data.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFanexSql(builder.Configuration);
builder.Services.Configure<AppSettingsKey>(builder.Configuration);
builder.Services.AddDirectFantasyServices();
builder.Services.AutoPayoutConfigServices(builder.Configuration);

builder.Services.AddHostedService<DirectFantasyHostedService>();

var app = builder.Build();

app.Run(async context =>
{
    await context.Response.WriteAsync("OK");
});

app.Run();