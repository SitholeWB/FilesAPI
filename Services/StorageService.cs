using Contracts;
using Models;
using Models.Events;
using Models.Exceptions;
using MongoDB.Bson;
using Services.Events;
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
		private readonly IStorageRepository _storageRepository;
		private readonly IFileDetailsRepository _fileDetailsRepository;
		private readonly EventHandlerContainer _eventContainer;

		public StorageService(EventHandlerContainer eventContainer, IStorageRepository storageRepository, IFileDetailsRepository fileDetailsRepository)
		{
			_eventContainer = eventContainer;
			_storageRepository = storageRepository;
			_fileDetailsRepository = fileDetailsRepository;
		}

		public async Task<string> DeleteFileAsync(string id)
		{
			var fileDetails = await _fileDetailsRepository.GetFileDetailsAsync(id);
			if (fileDetails == default)
			{
				throw new FilesApiException("No File found for given Id");
			}

			await _fileDetailsRepository.DeleteFileAsync(id);
			var storageId = fileDetails.StorageId;
			var fileName = fileDetails.Name;
			fileDetails = await _fileDetailsRepository.GetFileDetailsAsync(storageId);
			if (fileDetails == default)
			{
				await _storageRepository.DeleteFileAsync(storageId);
			}
			return fileName;
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		public async Task<(Stream, FileDetails)> DownloadFileAsync(string id)
		{
			var fileDetails = await _fileDetailsRepository.GetFileDetailsAsync(id);
			if (fileDetails == default)
			{
				throw new FilesApiException("No File found for given Id");
			}
			await _eventContainer.PublishAsync(new FileDownloadedEvent { FileDetails = fileDetails });
			return (await _storageRepository.DownloadFileAsync(fileDetails.StorageId), fileDetails);
		}

		public async Task IncrementDownloadCountAsync(FileDetails fileDetails)
		{
			fileDetails.NumberOfDownloads++;
			await _fileDetailsRepository.UpdateFileDetailsAsync(fileDetails.Id, fileDetails);
		}

		public async Task<IEnumerable<FileDetails>> GetAllFileDetailsAsync()
		{
			return await _fileDetailsRepository.GetAllFileDetailsAsync();
		}

		public async Task<FileDetails> GetFileDetailsAsync(string id)
		{
			return await _fileDetailsRepository.GetFileDetailsAsync(id);
		}

		public async Task<IEnumerable<FileDetails>> GetFileDetailsByTagAsync(string tag)
		{
			return await _fileDetailsRepository.GetFileDetailsByTagAsync(tag);
		}

		public async Task<FileDetails> UpdateFileDetailsAsync(string id, FileDetails details)
		{
			return await _fileDetailsRepository.UpdateFileDetailsAsync(id, details);
		}

		public async Task<FileDetails> UploadFileAsync(Stream stream, FileDetails fileDetails)
		{
			if (fileDetails == null)
			{
				throw new FilesApiException("FileDetails no provided to upload function.");
			}

			using var fileHelper = new FileHelper(stream, fileDetails.Name);
			var hashId = SHA256CheckSum(fileHelper.GetFilePath());

			var existingFile = await _fileDetailsRepository.GetFileDetailsByHashIdAsync(hashId);

			if (existingFile != default)
			{
				fileDetails.Id = ObjectId.GenerateNewId().ToString();
				fileDetails.StorageId = existingFile.StorageId;
				fileDetails.HashId = hashId;
				await _fileDetailsRepository.AddFileDetailsAsync(fileDetails);
				return fileDetails;
			}
			using var fileStream = File.OpenRead(fileHelper.GetFilePath());
			var id = await _storageRepository.UploadFileAsync(fileStream, fileDetails.Name);
			fileDetails.Id = ObjectId.GenerateNewId().ToString();
			fileDetails.StorageId = id.ToString();
			fileDetails.HashId = hashId;

			await _fileDetailsRepository.AddFileDetailsAsync(fileDetails);
			return fileDetails;
		}

		public string SHA256CheckSum(string filePath)
		{
			using var sha256 = SHA256.Create();
			using var fileStream = File.OpenRead(filePath);
			return Convert.ToBase64String(sha256.ComputeHash(fileStream));
		}
	}
}