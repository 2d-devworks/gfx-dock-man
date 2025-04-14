using System.IO.Abstractions;
using GfxMan.Services.Interfaces;
using GfxMan.Services.Model;
using Microsoft.Extensions.DependencyInjection;

namespace GfxMan.Services;

public static class DependencyResolution
{
    public static void AddGfxManServices(this IServiceCollection services)
    {
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<ISystemDeviceInformationService<MonitorInfo>, SystemMonitorInfoService>();
        services.AddSingleton<ISystemDeviceInformationService<DisplayAdapterInfo>, SystemDisplayAdapterInfoService>();
        services.AddSingleton<ISettingsFileService, SettingsFileService>();
        services.AddSingleton<IPrimaryDisplayAdapterService, PrimaryDisplayAdapterService>();
        services.AddSingleton<IPrimaryMonitorService, PrimaryMonitorService>();
        services.AddSingleton<IGraphicsSettingManager, GraphicsSettingManager>();
    }
}