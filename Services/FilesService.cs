using Contracts;
using LiteDB;
using Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
	public class FilesService : IStorageService
	{
		private readonly ISettingsService _settingsService;

		public FilesService(ISettingsService settingsService)
		{
			_settingsService = settingsService;
		}
		public async Task<string> DeleteFileAsync(string id)
		{
			using (var db = new LiteDatabase(_settingsService.GetLiteDBAppSettings().ConnectionString))
			{
				var collection = db.GetCollection<FileDetails>("FileDetails");
				var success = collection.Delete(id);
				return await Task.FromResult((success ? id : string.Empty));
			}
		}

		public async Task<(Stream, FileDetails)> DownloadFileAsync(string id)
		{
			using (var db = new LiteDatabase(_settingsService.GetLiteDBAppSettings().ConnectionString))
			{
				var collection = db.GetCollection<FileDetails>("FileDetails");
				var fileDetails = collection.FindById(id);
				var stream = db.FileStorage.OpenRead(id);
				return await Task.FromResult((stream, fileDetails));
			}
		}

		public async Task<IEnumerable<FileDetails>> GetAllFileDetailsAsync()
		{
			using (var db = new LiteDatabase(_settingsService.GetLiteDBAppSettings().ConnectionString))
			{
				var collection = db.GetCollection<FileDetails>("FileDetails");
				return await Task.FromResult(collection.Query().ToList());
			}
		}

		public async Task<FileDetails> GetFileDetailsAsync(string id)
		{
			using (var db = new LiteDatabase(_settingsService.GetLiteDBAppSettings().ConnectionString))
			{
				var collection = db.GetCollection<FileDetails>("FileDetails");
				return await Task.FromResult(collection.FindById(id));
			}
		}

		public async Task<IEnumerable<FileDetails>> GetFileDetailsByTagAsync(string tag)
		{
			using (var db = new LiteDatabase(_settingsService.GetLiteDBAppSettings().ConnectionString))
			{
				var collection = db.GetCollection<FileDetails>("FileDetails");
				var result = collection.Query().Where(a => a.Tags.Contains(tag.Trim())).ToList();
				return await Task.FromResult(result);
			}
		}

		public async Task<FileDetails> UpdateFileDetailsAsync(FileDetails details)
		{
			using (var db = new LiteDatabase(_settingsService.GetLiteDBAppSettings().ConnectionString))
			{
				var collection = db.GetCollection<FileDetails>("FileDetails");
				var fileDetails = collection.FindById(details.Id);
				fileDetails.Description = details.Description;
				fileDetails.LastModified = DateTime.UtcNow;
				fileDetails.Name = details.Name;
				fileDetails.Tags = details.Tags;
				var success = collection.Update(fileDetails);
				return await Task.FromResult((success ? fileDetails : throw new Exception("Error while updating")));
			}
		}

		public async Task<FileDetails> UploadFileAsync(Stream fileStream, FileDetails fileDetails)
		{
			using (var db = new LiteDatabase(_settingsService.GetLiteDBAppSettings().ConnectionString))
			{
				var collection = db.GetCollection<FileDetails>("FileDetails");
				fileDetails.Id = ObjectId.NewObjectId().ToString();
				collection.Insert(fileDetails.Id, fileDetails);
				var obj = db.FileStorage.Upload(fileDetails.Id, fileDetails.Name, fileStream);
			}
			return await Task.FromResult(fileDetails);
		}
	}
}
