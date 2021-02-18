using System.IO;
using System.Threading.Tasks;

namespace Contracts
{
	public interface IStorageRepository
	{
		Task<string> UploadFileAsync(Stream fileStream, string fileName);

		Task<Stream> DownloadFileAsync(string id);

		Task DeleteFileAsync(string id);
	}
}