using GfxMan.Services;
using GfxMan.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

var builder = Host.CreateApplicationBuilder();

// See https://aka.ms/new-console-template for more information
builder.Configuration.Sources.Clear();

IHostEnvironment env = builder.Environment;

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);

builder.Services.AddLogging(config =>
{
    config.AddConfiguration(builder.Configuration.GetSection("Logging"));
    config.AddConsole();
    
    if (System.Diagnostics.Debugger.IsAttached)
    {
        config.AddDebug();    
    }
    
    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        return;
    }
    
    config.AddEventLog(settings =>
    {
#pragma warning disable CA1416
        settings.LogName = "Application";
        settings.SourceName = "GfxMan";
#pragma warning restore CA1416
    });
});

builder.Services.AddSingleton<IGraphicsSettingManager, GraphicsSettingManager>();
builder.Services.AddHostedService<GfxManWorker>();

using var host = builder.Build();
var gfxMan = host.Services.GetService<IGraphicsSettingManager>();
if (gfxMan != null && args.Length != 0)
{
    if (args.Contains("-docked"))
    {
        gfxMan.SetToDocked();
    }
    else if(args.Contains("-undocked"))
    {
        gfxMan.SetToUndocked();
    }
    
    gfxMan.SaveConfig();
}
else
{
    host.Run();
}
