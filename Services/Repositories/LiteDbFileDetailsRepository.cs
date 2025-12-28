namespace Services;

/// <summary>
/// LiteDB implementation for self-contained file storage without external dependencies
/// </summary>
public class LiteDbFileDetailsRepository : IFileDetailsRepository
{
    private readonly string _connectionString;
    private readonly string _collectionName = "filedetails";

    public LiteDbFileDetailsRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<FileDetails> AddFileDetailsAsync(FileDetails fileDetails, CancellationToken token)
    {
        return await Task.Run(() =>
        {
            using var db = new LiteDatabase(_connectionString);
            var collection = db.GetCollection<FileDetails>(_collectionName);

            // Generate new ID if not provided
            if (string.IsNullOrEmpty(fileDetails.Id))
            {
                fileDetails.Id = ObjectId.NewObjectId().ToString();
            }

            collection.Insert(fileDetails);
            return fileDetails;
        }, token);
    }

    public async Task<FileDetails> GetFileDetailsAsync(string id, CancellationToken token)
    {
        return await Task.Run(() =>
        {
            using var db = new LiteDatabase(_connectionString);
            var collection = db.GetCollection<FileDetails>(_collectionName);
            return collection.FindById(id);
        }, token);
    }

    public async Task<IEnumerable<FileDetails>> GetAllFileDetailsAsync(CancellationToken token)
    {
        return await Task.Run(() =>
        {
            using var db = new LiteDatabase(_connectionString);
            var collection = db.GetCollection<FileDetails>(_collectionName);
            return collection.FindAll().ToList();
        }, token);
    }

    public async Task<FileDetails> UpdateFileDetailsAsync(string id, FileDetails fileDetails, CancellationToken token)
    {
        return await Task.Run(() =>
        {
            using var db = new LiteDatabase(_connectionString);
            var collection = db.GetCollection<FileDetails>(_collectionName);

            // Ensure the ID matches
            fileDetails.Id = id;
            collection.Update(fileDetails);
            return fileDetails;
        }, token);
    }

    public async Task DeleteFileAsync(string id, CancellationToken token)
    {
        await Task.Run(() =>
        {
            using var db = new LiteDatabase(_connectionString);
            var collection = db.GetCollection<FileDetails>(_collectionName);
            collection.Delete(id);
        }, token);
    }

    public async Task<FileDetails> GetFileDetailsByHashIdAsync(string hashId, CancellationToken token)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var db = new LiteDatabase(_connectionString);
                var collection = db.GetCollection<FileDetails>(_collectionName);
                return collection.FindOne(x => x.HashId == hashId);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately For now, return null if there's an issue
                Trace.TraceError($"Error in GetFileDetailsByHashIdAsync: {ex.Message}");
                return null;
            }
        }, token);
    }

    public async Task<IEnumerable<FileDetails>> GetFileDetailsByTagAsync(string tag, CancellationToken token)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var db = new LiteDatabase(_connectionString);
                var collection = db.GetCollection<FileDetails>(_collectionName);
                return collection.Find(x => x.Tags.Contains(tag)).ToList();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error in GetFileDetailsByTagAsync: {ex.Message}");
                return new List<FileDetails>();
            }
        }, token);
    }

    public async Task<bool> FileDetailsExistsAsync(string id, CancellationToken token)
    {
        return await Task.Run(() =>
        {
            using var db = new LiteDatabase(_connectionString);
            var collection = db.GetCollection<FileDetails>(_collectionName);
            return collection.Exists(Query.EQ("_id", id));
        }, token);
    }
}