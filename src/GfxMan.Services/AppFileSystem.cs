using System.IO.Abstractions;
using GfxMan.Services.Interfaces;

namespace GfxMan.Services;

public class AppFileSystem : FileSystem, IAppFileSystem
{
    private const string AppDataFolder = "2d-devworks\\GfxMan";
    
    public string GetAppDataPath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appDataPath, AppDataFolder);
    }
}