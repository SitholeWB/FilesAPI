namespace Services;

public class RecordDownloadHandler : IEventHandler<FileDownloadedEvent>
{
    private readonly IStorageService _storageService;

    public RecordDownloadHandler(IStorageService storageService)
    {
        _storageService = storageService;
    }

    public async Task RunAsync(FileDownloadedEvent obj, CancellationToken token)
    {
        await _storageService.IncrementDownloadCountAsync(obj.FileDetails, token);
    }
}