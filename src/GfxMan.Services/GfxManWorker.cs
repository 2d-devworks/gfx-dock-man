using GfxMan.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GfxMan.Services;

public class GfxManWorker(ILogger<GfxManWorker> logger, IGraphicsSettingManager graphicsSettingManager)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        await graphicsSettingManager.Scan(stoppingToken);
    }
}