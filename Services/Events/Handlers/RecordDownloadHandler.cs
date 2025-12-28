using Microsoft.Extensions.DependencyInjection;

namespace Services;

public class RecordDownloadHandler : IEventHandler<FileDownloadedEvent>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public RecordDownloadHandler(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public Task RunAsync(FileDownloadedEvent obj, CancellationToken token)
    {
        //fire‑and‑forget
        _ = Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var storageService = scope.ServiceProvider.GetRequiredService<IStorageService>();
            await storageService.IncrementDownloadCountAsync(obj.FileDetails, CancellationToken.None);
        }, CancellationToken.None);

        return Task.CompletedTask;
    }
}