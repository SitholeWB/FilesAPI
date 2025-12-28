using Contracts;
using Files.EntityFrameworkCore.Extensions;
using Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Services.Repositories;

public class SqlStorageRepository : IStorageRepository
{
    private readonly FilesDbContext _filesDbContext;

    public SqlStorageRepository(FilesDbContext filesDbContext)
    {
        _filesDbContext = filesDbContext;
    }

    public async Task DeleteFileAsync(string id)
    {
        await _filesDbContext.DeleteFileAsync<FileEntity>(Guid.Parse(id));
    }

    public async Task<Stream> DownloadFileAsync(string id)
    {
        var stream = new MemoryStream();
        await _filesDbContext.DownloadFileToStreamAsync<FileEntity>(Guid.Parse(id), stream);
        return stream;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
    {
        var fileDetails = await _filesDbContext.SaveFileAsync<FileEntity>(fileStream, fileName);
        return fileDetails.Id.ToString();
    }
}