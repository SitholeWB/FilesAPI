using Contracts;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System.IO;
using System.Threading.Tasks;

namespace Services.Repositories
{
	public class StorageRepository : IStorageRepository
	{
		private const string _databaseName = "FilesAPI";
		private const string bucket = "Storage";
		private readonly GridFSBucket fsBucket;

		public StorageRepository(ISettingsService settingsService)
		{
			var client = new MongoClient(settingsService.GetMongoDBAppSettings().ConnectionString);
			var database = client.GetDatabase(_databaseName);
			fsBucket = new GridFSBucket(database, new GridFSBucketOptions { BucketName = bucket });
		}

		public async Task DeleteFileAsync(string id)
		{
			await fsBucket.DeleteAsync(ObjectId.Parse(id));
		}

		public async Task<Stream> DownloadFileAsync(string id)
		{
			return await fsBucket.OpenDownloadStreamAsync(ObjectId.Parse(id));
		}

		public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
		{
			var id = await fsBucket.UploadFromStreamAsync(fileName, fileStream);
			return id.ToString();
		}
	}
}