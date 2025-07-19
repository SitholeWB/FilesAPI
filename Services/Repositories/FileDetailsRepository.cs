using Contracts;
using Models;
using Models.Exceptions;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Repositories
{
	public class FileDetailsRepository : IFileDetailsRepository
	{
		private const string _databaseName = "FilesAPI";
		private const string _collectionName = "FileDetails";
		private readonly IMongoCollection<FileDetails> _collection;

		public FileDetailsRepository(ISettingsService settingsService)
		{
			var client = new MongoClient(settingsService.GetMongoDBAppSettings().ConnectionString);
			var database = client.GetDatabase(_databaseName);
			_collection = database.GetCollection<FileDetails>(_collectionName);
		}

		public async Task<FileDetails> AddFileDetailsAsync(FileDetails details)
		{
			details.Id = ObjectId.GenerateNewId().ToString();
			await _collection.InsertOneAsync(details);
			await CreateIndexesAsync();
			return details;
		}

		public async Task DeleteFileAsync(string id)
		{
			var results = await _collection.FindAsync(fileInfo => fileInfo.Id.Equals(id));
			var fileDetails = await results.FirstOrDefaultAsync();
			if (fileDetails == default)
			{
				throw new FilesApiException("No File found for given Id");
			}
			await _collection.DeleteOneAsync(info => info.Id == id);
		}

		public async Task<IEnumerable<FileDetails>> GetAllFileDetailsAsync()
		{
			var results = await _collection.FindAsync(_ => true);
			return await results.ToListAsync();
		}

		public async Task<FileDetails> GetFileDetailsAsync(string id)
		{
			var results = await _collection.FindAsync(fileInfo => fileInfo.Id.Equals(id));
			return await results.FirstOrDefaultAsync();
		}

		public async Task<FileDetails> GetFileDetailsByHashIdAsync(string hashId)
		{
			var results = await _collection.FindAsync(fileInfo => fileInfo.HashId.Equals(hashId));
			return await results.FirstOrDefaultAsync();
		}

		public async Task<IEnumerable<FileDetails>> GetFileDetailsByTagAsync(string tag)
		{
			var tcBuilder = Builders<FileDetails>.Filter;
			var tcFilter = tcBuilder.Eq("Tags", tag);
			var results = await _collection.FindAsync(tcFilter);
			return await results.ToListAsync<FileDetails>();
		}

		public async Task<FileDetails> UpdateFileDetailsAsync(string id, FileDetails details)
		{
			var results = await _collection.FindAsync(fileInfo => fileInfo.Id.Equals(details.Id));
			var odlFileDetails = await results.FirstOrDefaultAsync();
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
			await _collection.UpdateOneAsync(filter, update);
			results = await _collection.FindAsync(fileInfo => fileInfo.Id.Equals(details.Id));
			return await results.FirstOrDefaultAsync();
		}

		private async Task CreateIndexesAsync()
		{
			var indexKeysDefinition1 = Builders<FileDetails>.IndexKeys.Hashed(fileDetails => fileDetails.HashId);
			var indexKeysDefinition2 = Builders<FileDetails>.IndexKeys.Hashed(fileDetails => fileDetails.StorageId);
			var indexes = new List<CreateIndexModel<FileDetails>> { new CreateIndexModel<FileDetails>(indexKeysDefinition1), new CreateIndexModel<FileDetails>(indexKeysDefinition2) };
			await _collection.Indexes.CreateManyAsync(indexes);
		}
	}
}