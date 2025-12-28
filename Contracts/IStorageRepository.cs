namespace Contracts;

public interface IStorageRepository
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, CancellationToken token);

    Task<Stream> DownloadFileAsync(string id, CancellationToken token);

    Task DeleteFileAsync(string id, CancellationToken token);
}