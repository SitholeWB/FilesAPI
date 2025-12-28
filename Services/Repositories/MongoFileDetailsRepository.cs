using ObjectId = MongoDB.Bson.ObjectId;

namespace Services;

public class MongoFileDetailsRepository : IFileDetailsRepository
{
    private const string _databaseName = "FilesAPI";
    private const string _collectionName = "FileDetails";
    private readonly IMongoCollection<FileDetails> _collection;

    public MongoFileDetailsRepository(ISettingsService settingsService)
    {
        var client = new MongoClient(settingsService.GetMongoDBAppSettings().ConnectionString);
        var database = client.GetDatabase(_databaseName);
        _collection = database.GetCollection<FileDetails>(_collectionName);
    }

    public async Task<FileDetails> AddFileDetailsAsync(FileDetails details, CancellationToken token)
    {
        details.Id = ObjectId.GenerateNewId().ToString();
        await _collection.InsertOneAsync(details, options: default, token);
        await CreateIndexesAsync(token);
        return details;
    }

    public async Task DeleteFileAsync(string id, CancellationToken token)
    {
        var results = await _collection.FindAsync(fileInfo => fileInfo.Id.Equals(id), cancellationToken: token);
        var fileDetails = await results.FirstOrDefaultAsync(token);
        if (fileDetails == default)
        {
            throw new FilesApiException("No File found for given Id");
        }
        await _collection.DeleteOneAsync(info => info.Id == id, token);
    }

    public async Task<IEnumerable<FileDetails>> GetAllFileDetailsAsync(CancellationToken token)
    {
        var results = await _collection.FindAsync(_ => true, cancellationToken: token);
        return await results.ToListAsync(token);
    }

    public async Task<FileDetails> GetFileDetailsAsync(string id, CancellationToken token)
    {
        var results = await _collection.FindAsync(fileInfo => fileInfo.Id.Equals(id), cancellationToken: token);
        return await results.FirstOrDefaultAsync(token);
    }

    public async Task<FileDetails> GetFileDetailsByHashIdAsync(string hashId, CancellationToken token)
    {
        var results = await _collection.FindAsync(fileInfo => fileInfo.HashId.Equals(hashId), cancellationToken: token);
        return await results.FirstOrDefaultAsync(token);
    }

    public async Task<IEnumerable<FileDetails>> GetFileDetailsByTagAsync(string tag, CancellationToken token)
    {
        var tcBuilder = Builders<FileDetails>.Filter;
        var tcFilter = tcBuilder.Eq("Tags", tag);
        var results = await _collection.FindAsync(tcFilter, cancellationToken: token);
        return await results.ToListAsync<FileDetails>(token);
    }

    public async Task<FileDetails> UpdateFileDetailsAsync(string id, FileDetails details, CancellationToken token)
    {
        var results = await _collection.FindAsync(fileInfo => fileInfo.Id.Equals(details.Id), cancellationToken: token);
        var odlFileDetails = await results.FirstOrDefaultAsync(token);
        if (odlFileDetails == default)
        {
            throw new FilesApiException("No File found for given Id");
        }
        var filter = Builders<FileDetails>.Filter.Eq("Id", details.Id);
        var update = Builders<FileDetails>.Update
                    .Set("Name", details.Name ?? odlFileDetails.Name)
                    .Set("Description", details.Description ?? odlFileDetails.Description)
                    .Set("AddedBy", details.AddedBy ?? odlFileDetails.AddedBy)
                    .Set("Tags", details.Tags ?? odlFileDetails.Tags)
                    .Set("NumberOfDownloads", odlFileDetails.NumberOfDownloads)
                    .CurrentDate("LastModified");
        await _collection.UpdateOneAsync(filter, update, cancellationToken: token);
        results = await _collection.FindAsync(fileInfo => fileInfo.Id.Equals(details.Id), cancellationToken: token);
        return await results.FirstOrDefaultAsync(token);
    }

    private async Task CreateIndexesAsync(CancellationToken token)
    {
        var indexKeysDefinition1 = Builders<FileDetails>.IndexKeys.Hashed(fileDetails => fileDetails.HashId);
        var indexKeysDefinition2 = Builders<FileDetails>.IndexKeys.Hashed(fileDetails => fileDetails.StorageId);
        var indexes = new List<CreateIndexModel<FileDetails>> { new CreateIndexModel<FileDetails>(indexKeysDefinition1), new CreateIndexModel<FileDetails>(indexKeysDefinition2) };
        await _collection.Indexes.CreateManyAsync(indexes, token);
    }
}