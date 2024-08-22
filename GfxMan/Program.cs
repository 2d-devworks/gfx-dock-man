using GfxMan.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder();

// See https://aka.ms/new-console-template for more information
var tokenSource = new CancellationTokenSource();
var token = tokenSource.Token;

builder.Configuration.Sources.Clear();

IHostEnvironment env = builder.Environment;

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);


using var host = builder.Build();
var gfxMan = new GraphicsSettingManager(builder.Configuration, token);
if (args.Contains("-docked"))
{
    gfxMan.SetToDocked();
    tokenSource.Cancel();
}
else if(args.Contains("-undocked"))
{
    gfxMan.SetToUndocked();
    tokenSource.Cancel();
}
else
{
    await gfxMan.Scan(token);
}
await host.RunAsync(token);
