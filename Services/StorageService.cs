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
	public class StorageService : IStorageService
	{
		private readonly string fileInfoDbName = "FileInfo";
		private readonly string fileBucketDbName = "FileBucket";
		private readonly string bucket = "DEFAULT_BUCKET";
		private readonly GridFSBucket fsBucket = null;

		private readonly IMongoDatabase fileInfoDB = null;
		public StorageService(string connStr = "mongodb://localhost:27017")
		{
			var client = new MongoClient(connStr);
			fileInfoDB = client.GetDatabase(fileInfoDbName);
			var db = client.GetDatabase(fileBucketDbName);
			fsBucket = new GridFSBucket(db, new GridFSBucketOptions { BucketName = bucket });
		}

		public async Task<Stream> DownloadFileAsync(string id)
		{
			return await fsBucket.OpenDownloadStreamAsync(BsonValue.Create(id));
		}

		public async Task<IEnumerable<FileDetails>> GetAllFileDetails()
		{
			var collection = fileInfoDB.GetCollection<FileDetails>(fileInfoDbName);
			return await collection.AsQueryable().ToListAsync();
		}

		public async Task<FileDetails> GetFileDetails(string id)
		{
			var collection = fileInfoDB.GetCollection<FileDetails>(fileInfoDbName);
			var results = await collection.FindAsync(fileInfo => fileInfo.Id.Equals(id));
			return await results.FirstOrDefaultAsync();
		}

		public async Task<FileDetails> UpdateFileDetails(FileDetails details)
		{
			var collection = fileInfoDB.GetCollection<FileDetails>(fileInfoDbName);
			var results = await collection.FindAsync(fileInfo => fileInfo.Id.Equals(details.Id));

			if(await results.FirstOrDefaultAsync() == null)
			{
				throw new GridFSFileNotFoundException(details.Id);
			}

			var filter = Builders<FileDetails>.Filter.Eq("Id", details.Id);

			var update = Builders<FileDetails>.Update
						.Set("Name", details.Name)
						.Set("Description", details.Description)
						.Set("AddedBy", details.AddedBy)
						.Set("Tags", details.Tags)
						.CurrentDate("lastModified");

			await collection.UpdateOneAsync(filter, update);
			return details;
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
