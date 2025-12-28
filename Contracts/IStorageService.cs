namespace Contracts;

public interface IStorageService : IDisposable
{
    Task<FileDetails> UploadFileAsync(Stream fileStream, FileDetails fileDetails, CancellationToken token);

    Task<(Stream, FileDetails)> DownloadFileAsync(string id, CancellationToken token);

    Task<FileDetails> UpdateFileDetailsAsync(string id, FileDetails details, CancellationToken token);

    Task<FileDetails> GetFileDetailsAsync(string id, CancellationToken token);

    Task<IEnumerable<FileDetails>> GetAllFileDetailsAsync(CancellationToken token);

    Task<IEnumerable<FileDetails>> GetFileDetailsByTagAsync(string tag, CancellationToken token);

    Task<string> DeleteFileAsync(string id, CancellationToken token);

    Task IncrementDownloadCountAsync(FileDetails fileDetails, CancellationToken token);
}