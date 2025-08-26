using GfxMan.Services.Interfaces;
using GfxMan.Services.Model;
using Microsoft.Extensions.DependencyInjection;

namespace GfxMan.Services;

public static class DependencyResolution
{
    public static void AddGfxManServices(this IServiceCollection services)
    {
        services.AddSingleton<IAppFileSystem, AppFileSystem>();
        services.AddSingleton<ISystemDeviceInformationService<MonitorInfo>, SystemMonitorInfoService>();
        services.AddSingleton<ISystemDeviceInformationService<DisplayAdapterInfo>, SystemDisplayAdapterInfoService>();
        services.AddSingleton<ISettingsFileService, SettingsFileService>();
        services.AddSingleton<IBackupFilesService, BackupFilesService>();
        services.AddSingleton<IPrimaryDisplayAdapterService, PrimaryDisplayAdapterService>();
        services.AddSingleton<IPrimaryMonitorService, PrimaryMonitorService>();
        services.AddSingleton<IGraphicsSettingManager, GraphicsSettingManager>();
    }
}