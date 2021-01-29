using Contracts;
using Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Services.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Services
{
	public sealed class StorageService : IStorageService
	{
		private readonly string fileInfoDbName = "FileDetails";
		private readonly string fileBucketDbName = "FilesAPI";
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
			var results = await collection.FindAsync(fileInfo => fileInfo.Id.Equals(id));
			var fileDetails = await results.FirstOrDefaultAsync(); await collection.DeleteOneAsync(info => info.Id == id);
			await fsBucket.DeleteAsync(ObjectId.Parse(fileDetails.SharedStorageId));
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

			var filter = Builders<FileDetails>.Filter.Eq("Id", id);
			var update = Builders<FileDetails>.Update
			.Set("NumberOfDownloads", fileDetails.NumberOfDownloads + 1)
			.CurrentDate("LastModified");

			await collection.UpdateOneAsync(filter, update);

			return (await fsBucket.OpenDownloadStreamAsync(ObjectId.Parse(fileDetails.SharedStorageId)), fileDetails);
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
						.Set("NumberOfDownloads", odlFileDetails.NumberOfDownloads)
						.CurrentDate("LastModified");

			await collection.UpdateOneAsync(filter, update);

			results = await collection.FindAsync(fileInfo => fileInfo.Id.Equals(details.Id));
			return await results.FirstOrDefaultAsync();
		}

		public async Task<FileDetails> UploadFileAsync(Stream stream, FileDetails fileDetails)
		{
			using var fileHelper = new FileHelper(stream, fileDetails.Name);
			var hashId = SHA256CheckSum(fileHelper.GetFilePath());
			var collection = fileInfoDB.GetCollection<FileDetails>(fileInfoDbName);
			var results = await collection.FindAsync(fileInfo => fileInfo.HashId.Equals(hashId));
			var existingFile = await results.FirstOrDefaultAsync();

			if (existingFile != default)
			{
				fileDetails.Id = Guid.NewGuid().ToString();
				fileDetails.SharedStorageId = existingFile.SharedStorageId;
				fileDetails.HashId = hashId;
				await collection.InsertOneAsync(fileDetails);
				return fileDetails;
			}
			using var fileStream = File.OpenRead(fileHelper.GetFilePath());
			var id = await fsBucket.UploadFromStreamAsync(fileDetails.Name, fileStream);
			fileDetails.Id = Guid.NewGuid().ToString();
			fileDetails.SharedStorageId = id.ToString();
			fileDetails.HashId = hashId;

			await collection.InsertOneAsync(fileDetails);

			return fileDetails;
		}

		public string SHA256CheckSum(string filePath)
		{
			using var SHA256 = SHA256Managed.Create();
			using var fileStream = File.OpenRead(filePath);
			return Convert.ToBase64String(SHA256.ComputeHash(fileStream));
		}
	}
}