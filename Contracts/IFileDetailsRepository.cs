using Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contracts
{
	public interface IFileDetailsRepository
	{
		Task<FileDetails> AddFileDetailsAsync(FileDetails details);

		Task<FileDetails> UpdateFileDetailsAsync(string id, FileDetails details);

		Task<FileDetails> GetFileDetailsAsync(string id);

		Task<FileDetails> GetFileDetailsByHashIdAsync(string hashId);

		Task<IEnumerable<FileDetails>> GetAllFileDetailsAsync();

		Task<IEnumerable<FileDetails>> GetFileDetailsByTagAsync(string tag);

		Task DeleteFileAsync(string id);
	}
}