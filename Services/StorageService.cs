using System.Security.Cryptography;
using ObjectId = MongoDB.Bson.ObjectId;

namespace Services;

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

    public async Task<string> DeleteFileAsync(string id, CancellationToken token)
    {
        var fileDetails = await _fileDetailsRepository.GetFileDetailsAsync(id, token);
        if (fileDetails == default)
        {
            throw new FilesApiException("No File found for given Id");
        }

        await _fileDetailsRepository.DeleteFileAsync(id, token);
        var storageId = fileDetails.StorageId;
        var fileName = fileDetails.Name;
        fileDetails = await _fileDetailsRepository.GetFileDetailsAsync(storageId, token);
        if (fileDetails == default)
        {
            await _storageRepository.DeleteFileAsync(storageId, token);
        }
        return fileName;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public async Task<(Stream, FileDetails)> DownloadFileAsync(string id, CancellationToken token)
    {
        var fileDetails = await _fileDetailsRepository.GetFileDetailsAsync(id, token);
        if (fileDetails == default)
        {
            throw new FilesApiException("No File found for given Id");
        }
        await _eventContainer.PublishAsync(new FileDownloadedEvent { FileDetails = fileDetails }, token);
        return (await _storageRepository.DownloadFileAsync(fileDetails.StorageId, token), fileDetails);
    }

    public async Task IncrementDownloadCountAsync(FileDetails fileDetails, CancellationToken token)
    {
        fileDetails.NumberOfDownloads++;
        await _fileDetailsRepository.UpdateFileDetailsAsync(fileDetails.Id, fileDetails, token);
    }

    public async Task<IEnumerable<FileDetails>> GetAllFileDetailsAsync(CancellationToken token)
    {
        return await _fileDetailsRepository.GetAllFileDetailsAsync(token);
    }

    public async Task<FileDetails> GetFileDetailsAsync(string id, CancellationToken token)
    {
        return await _fileDetailsRepository.GetFileDetailsAsync(id, token);
    }

    public async Task<IEnumerable<FileDetails>> GetFileDetailsByTagAsync(string tag, CancellationToken token)
    {
        return await _fileDetailsRepository.GetFileDetailsByTagAsync(tag, token);
    }

    public async Task<FileDetails> UpdateFileDetailsAsync(string id, FileDetails details, CancellationToken token)
    {
        return await _fileDetailsRepository.UpdateFileDetailsAsync(id, details, token);
    }

    public async Task<FileDetails> UploadFileAsync(Stream stream, FileDetails fileDetails, CancellationToken token)
    {
        if (fileDetails == null)
        {
            throw new FilesApiException("FileDetails not provided.");
        }

        using var fileHelper = new FileHelper(stream, fileDetails.Name);
        var hashId = SHA256CheckSum(fileHelper.GetFilePath());

        var existingFile = await _fileDetailsRepository.GetFileDetailsByHashIdAsync(hashId, token);

        if (existingFile != default)
        {
            fileDetails.Id = ObjectId.GenerateNewId().ToString();
            fileDetails.StorageId = existingFile.StorageId;
            fileDetails.HashId = hashId;
            await _fileDetailsRepository.AddFileDetailsAsync(fileDetails, token);
            return fileDetails;
        }
        using var fileStream = File.OpenRead(fileHelper.GetFilePath());
        var id = await _storageRepository.UploadFileAsync(fileStream, fileDetails.Name, token);
        fileDetails.Id = ObjectId.GenerateNewId().ToString();
        fileDetails.StorageId = id.ToString();
        fileDetails.HashId = hashId;

        await _fileDetailsRepository.AddFileDetailsAsync(fileDetails, token);
        return fileDetails;
    }

    public static string SHA256CheckSum(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var fileStream = File.OpenRead(filePath);
        return Convert.ToBase64String(sha256.ComputeHash(fileStream));
    }
}