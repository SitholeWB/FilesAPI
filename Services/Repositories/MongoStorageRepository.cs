using MongoDB.Driver.GridFS;
using ObjectId = MongoDB.Bson.ObjectId;

namespace Services;

public class MongoStorageRepository : IStorageRepository
{
    private const string _databaseName = "FilesAPI";
    private const string bucket = "Storage";
    private readonly GridFSBucket fsBucket;

    public MongoStorageRepository(ISettingsService settingsService)
    {
        var client = new MongoClient(settingsService.GetMongoDBAppSettings().ConnectionString);
        var database = client.GetDatabase(_databaseName);
        fsBucket = new GridFSBucket(database, new GridFSBucketOptions { BucketName = bucket });
    }

    public async Task DeleteFileAsync(string id, CancellationToken token)
    {
        await fsBucket.DeleteAsync(ObjectId.Parse(id), token);
    }

    public async Task<Stream> DownloadFileAsync(string id, CancellationToken token)
    {
        return await fsBucket.OpenDownloadStreamAsync(ObjectId.Parse(id), cancellationToken: token);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, CancellationToken token)
    {
        var id = await fsBucket.UploadFromStreamAsync(fileName, fileStream, cancellationToken: token);
        return id.ToString();
    }
}