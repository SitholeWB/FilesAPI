using Microsoft.Extensions.DependencyInjection;

namespace Services;

public class RecordDownloadAnalyticsHandler : IEventHandler<EnhancedFileDownloadedEvent>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public RecordDownloadAnalyticsHandler(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public Task RunAsync(EnhancedFileDownloadedEvent obj, CancellationToken token)
    {
        _ = Task.Run(async () =>
        {
            //fire‑and‑forget
            using var scope = _scopeFactory.CreateScope();
            var analyticsService = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();
            await analyticsService.RecordDownloadAsync(
                obj.FileDetails.Id,
                obj.UserAgent,
                obj.IpAddress,
                obj.Referrer,
                obj.DownloadMethod,
                CancellationToken.None);
        }, CancellationToken.None);

        return Task.CompletedTask;
    }
}