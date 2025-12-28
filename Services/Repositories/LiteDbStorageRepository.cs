namespace Services;

/// <summary>
/// LiteDB-based storage repository for self-contained file storage Uses LiteDB FileStorage for
/// GridFS-like functionality
/// </summary>
public class LiteDbStorageRepository : IStorageRepository
{
    private readonly string _connectionString;
    private readonly string _uploadsPath;

    public LiteDbStorageRepository(string connectionString, string uploadsPath)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _uploadsPath = uploadsPath ?? throw new ArgumentNullException(nameof(uploadsPath));

        // Ensure uploads directory exists
        Directory.CreateDirectory(_uploadsPath);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, CancellationToken token)
    {
        return await Task.Run(() =>
        {
            using var db = new LiteDatabase(_connectionString);
            var fs = db.FileStorage;

            // Generate unique file ID
            var fileId = $"$/files/{ObjectId.NewObjectId()}";

            // Store file in LiteDB FileStorage
            var liteFileInfo = fs.Upload(fileId, fileName, fileStream);

            return liteFileInfo.Id;
        }, token);
    }

    public async Task<Stream> DownloadFileAsync(string fileId, CancellationToken token)
    {
        return await Task.Run(() =>
        {
            using var db = new LiteDatabase(_connectionString);
            var fs = db.FileStorage;

            // Find file
            var fileInfo = fs.FindById(fileId);
            if (fileInfo == null)
            {
                throw new FileNotFoundException($"File with ID {fileId} not found");
            }

            // Download file to memory stream
            var memoryStream = new MemoryStream();
            fs.Download(fileId, memoryStream);
            memoryStream.Position = 0;

            return memoryStream as Stream;
        }, token);
    }

    public async Task DeleteFileAsync(string fileId, CancellationToken token)
    {
        await Task.Run(() =>
        {
            using var db = new LiteDatabase(_connectionString);
            var fs = db.FileStorage;
            fs.Delete(fileId);
        }, token);
    }

    public async Task<bool> FileExistsAsync(string fileId, CancellationToken token)
    {
        return await Task.Run(() =>
        {
            using var db = new LiteDatabase(_connectionString);
            var fs = db.FileStorage;
            return fs.Exists(fileId);
        }, token);
    }

    public async Task<long> GetFileSizeAsync(string fileId, CancellationToken token)
    {
        return await Task.Run(() =>
        {
            using var db = new LiteDatabase(_connectionString);
            var fs = db.FileStorage;

            var fileInfo = fs.FindById(fileId);
            return fileInfo?.Length ?? 0;
        }, token);
    }
}