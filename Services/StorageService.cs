using Contracts;
using Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Services
{
	public sealed class StorageService : IStorageService
	{
		private readonly string fileInfoDbName = "FileInfo";
		private readonly string fileBucketDbName = "FileBucket";
		private readonly string bucket = "DEFAULT_BUCKET";
		private readonly GridFSBucket fsBucket = null;

		private readonly IMongoDatabase fileInfoDB = null;
		public StorageService(ISettingsService settingsService)
		{
			var client = new MongoClient(settingsService.GetMongoDBAppSettings().ConnectionString);
			fileInfoDB = client.GetDatabase(fileInfoDbName);
			var db = client.GetDatabase(fileBucketDbName);
			fsBucket = new GridFSBucket(db, new GridFSBucketOptions { BucketName = bucket });
		}

		public async Task<string> DeleteFileAsync(string id)
		{
			var collection = fileInfoDB.GetCollection<FileDetails>(fileInfoDbName);
			await collection.DeleteOneAsync(info => info.Id == id);
			await fsBucket.DeleteAsync(ObjectId.Parse(id));
			return id;
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		public async Task<(Stream, FileDetails)> DownloadFileAsync(string id)
		{
			var collection = fileInfoDB.GetCollection<FileDetails>(fileInfoDbName);
			var results = await collection.FindAsync(fileInfo => fileInfo.Id.Equals(id));
			var fileDetails = await results.FirstOrDefaultAsync();
			return (await fsBucket.OpenDownloadStreamAsync(ObjectId.Parse(id)), fileDetails);
		}

		public async Task<IEnumerable<FileDetails>> GetAllFileDetailsAsync()
		{
			var collection = fileInfoDB.GetCollection<FileDetails>(fileInfoDbName);
			return await collection.AsQueryable().ToListAsync();
		}

		public async Task<FileDetails> GetFileDetailsAsync(string id)
		{
			var collection = fileInfoDB.GetCollection<FileDetails>(fileInfoDbName);
			var results = await collection.FindAsync(fileInfo => fileInfo.Id.Equals(id));
			return await results.FirstOrDefaultAsync();
		}

		public async Task<IEnumerable<FileDetails>> GetFileDetailsByTagAsync(string tag)
		{
			var collection = fileInfoDB.GetCollection<FileDetails>(fileInfoDbName);
			FilterDefinitionBuilder<FileDetails> tcBuilder = Builders<FileDetails>.Filter;
			FilterDefinition<FileDetails> tcFilter = tcBuilder.Eq("Tags", tag);
			var results = await collection.FindAsync(tcFilter);
			return await results.ToListAsync<FileDetails>();
		}

		public async Task<FileDetails> UpdateFileDetailsAsync(FileDetails details)
		{
			var collection = fileInfoDB.GetCollection<FileDetails>(fileInfoDbName);
			var results = await collection.FindAsync(fileInfo => fileInfo.Id.Equals(details.Id));

			FileDetails odlFileDetails = await results.FirstOrDefaultAsync();
			if (odlFileDetails == null)
			{
				throw new GridFSFileNotFoundException(details.Id);
			}
			var filter = Builders<FileDetails>.Filter.Eq("Id", details.Id);

			var update = Builders<FileDetails>.Update
						.Set("Name", details.Name ?? odlFileDetails.Name)
						.Set("Description", details.Description ?? odlFileDetails.Description)
						.Set("AddedBy", details.AddedBy ?? odlFileDetails.AddedBy)
						.Set("Tags", details.Tags ?? odlFileDetails.Tags)
						.Set("NumberOfDownloads", details.NumberOfDownloads ?? odlFileDetails.NumberOfDownloads)
						.CurrentDate("LastModified");

			await collection.UpdateOneAsync(filter, update);

			results = await collection.FindAsync(fileInfo => fileInfo.Id.Equals(details.Id));
			return await results.FirstOrDefaultAsync();
		}

		public async Task<FileDetails> UploadFileAsync(Stream inputStream, FileDetails fileDetails)
		{
			var id = await fsBucket.UploadFromStreamAsync(fileDetails.Name, inputStream);
			fileDetails.Id = id.ToString();

			var collection = fileInfoDB.GetCollection<FileDetails>(fileInfoDbName);
			await collection.InsertOneAsync(fileDetails);

			return fileDetails;
		}
	}
}
