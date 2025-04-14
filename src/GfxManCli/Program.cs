using GfxMan.Services;
using GfxMan.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

var builder = Host.CreateApplicationBuilder();

builder.Configuration.Sources.Clear();

var env = builder.Environment;

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

builder.Services.AddGfxManServices();
builder.Services.AddHostedService<GfxManWorker>();

using var host = builder.Build();
var gfxMan = host.Services.GetService<IGraphicsSettingManager>();

const string configArg = "-config";
var configArgIndex = Array.IndexOf(args, configArg);

if (gfxMan != null && configArgIndex > -1 && args.Length > configArgIndex + 2)
{
    var config = args[Array.IndexOf(args, configArg) + 1];
    if (gfxMan.Configuration.ConfigurationOptions.All(n => n.Name != configArg))
    {
        Console.WriteLine($"A configuration named {config} was not found. The active configuration is {gfxMan.Configuration.ActiveConfiguration}.");
    }
    gfxMan.ChangeActiveConfiguration(config);
}
else
{
    host.Run();
}
