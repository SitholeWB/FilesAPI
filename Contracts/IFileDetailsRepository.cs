namespace Contracts;

public interface IFileDetailsRepository
{
    Task<FileDetails> AddFileDetailsAsync(FileDetails details, CancellationToken token);

    Task<FileDetails> UpdateFileDetailsAsync(string id, FileDetails details, CancellationToken token);

    Task<FileDetails> GetFileDetailsAsync(string id, CancellationToken token);

    Task<FileDetails> GetFileDetailsByHashIdAsync(string hashId, CancellationToken token);

    Task<IEnumerable<FileDetails>> GetAllFileDetailsAsync(CancellationToken token);

    Task<IEnumerable<FileDetails>> GetFileDetailsByTagAsync(string tag, CancellationToken token);

    Task DeleteFileAsync(string id, CancellationToken token);
}