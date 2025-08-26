using System.IO.Abstractions;

namespace GfxMan.Services.Interfaces;

public interface IAppFileSystem : IFileSystem
{
    string GetAppDataPath();
}