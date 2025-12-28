using Files.EntityFrameworkCore.Extensions;

namespace Services;

public class SqlStorageRepository : IStorageRepository
{
    private readonly FilesDbContext _filesDbContext;

    public SqlStorageRepository(FilesDbContext filesDbContext)
    {
        _filesDbContext = filesDbContext;
    }

    public async Task DeleteFileAsync(string id, CancellationToken token)
    {
        await _filesDbContext.DeleteFileAsync<FileEntity>(Guid.Parse(id), token);
    }

    public async Task<Stream> DownloadFileAsync(string id, CancellationToken token)
    {
        var stream = new MemoryStream();
        await _filesDbContext.DownloadFileToStreamAsync<FileEntity>(Guid.Parse(id), stream, token);
        return stream;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, CancellationToken token)
    {
        var fileDetails = await _filesDbContext.SaveFileAsync<FileEntity>(fileStream, fileName, cancellationToken: token);
        return fileDetails.Id.ToString();
    }
}