using System.IO.Abstractions;
using GfxMan.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GfxMan.Services;

public static class DependencyResolution
{
    public static IServiceCollection AddGfxManServices(this IServiceCollection services)
    {
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<ISettingsFileService, SettingsFileService>();
        services.AddSingleton<IDisplayAdapterService, DisplayAdapterService>();
        services.AddSingleton<IGraphicsSettingManager, GraphicsSettingManager>();
        
        return services;
    }
}