﻿using Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Contracts
{
	public interface IStorageService : IDisposable
	{
		Task<FileDetails> UploadFileAsync(Stream fileStream, FileDetails fileDetails);

		Task<(Stream, FileDetails)> DownloadFileAsync(string id);

		Task<FileDetails> UpdateFileDetailsAsync(string id, FileDetails details);

		Task<FileDetails> GetFileDetailsAsync(string id);

		Task<IEnumerable<FileDetails>> GetAllFileDetailsAsync();

		Task<IEnumerable<FileDetails>> GetFileDetailsByTagAsync(string tag);

		Task<string> DeleteFileAsync(string id);

		Task IncrementDownloadCountAsync(FileDetails fileDetails);
	}
}