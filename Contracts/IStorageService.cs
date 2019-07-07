using Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Contracts
{
	public interface IStorageService
	{
		Task<FileDetails> UploadFileAsync(Stream fileStream, FileDetails fileDetails);
		Task<(Stream, FileDetails)> DownloadFileAsync(string id);
		Task<FileDetails> UpdateFileDetails(FileDetails details);
		Task<FileDetails> GetFileDetails(string id);
		Task<IEnumerable<FileDetails>> GetAllFileDetails();
		Task<IEnumerable<FileDetails>> GetFileDetailsByTag(string tag);
	}
}
