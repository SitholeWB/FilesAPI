namespace Services;

public class SqlFileDetailsRepository : IFileDetailsRepository
{
    private readonly FilesDbContext _filesDbContext;

    public SqlFileDetailsRepository(FilesDbContext filesDbContext)
    {
        _filesDbContext = filesDbContext;
    }

    public async Task<FileDetails> AddFileDetailsAsync(FileDetails details, CancellationToken token)
    {
        details.Id = Guid.NewGuid().ToString();
        await _filesDbContext.AddAsync(details, token);
        await _filesDbContext.SaveChangesAsync(token);
        return details;
    }

    public async Task DeleteFileAsync(string id, CancellationToken token)
    {
        var fileDetails = await _filesDbContext.FileDetails.FindAsync(id, token);
        if (fileDetails == default)
        {
            throw new FilesApiException("No File found for given Id");
        }
        _filesDbContext.Remove(fileDetails);
        await _filesDbContext.SaveChangesAsync(token);
    }

    public async Task<IEnumerable<FileDetails>> GetAllFileDetailsAsync(CancellationToken token)
    {
        return await _filesDbContext.FileDetails.ToListAsync(token);
    }

    public async Task<FileDetails> GetFileDetailsAsync(string id, CancellationToken token)
    {
        return await _filesDbContext.FileDetails.FindAsync(id, token);
    }

    public async Task<FileDetails> GetFileDetailsByHashIdAsync(string hashId, CancellationToken token)
    {
        return await _filesDbContext.FileDetails.Where(x => x.HashId == hashId).FirstOrDefaultAsync(token);
    }

    public async Task<IEnumerable<FileDetails>> GetFileDetailsByTagAsync(string tag, CancellationToken token)
    {
        return await _filesDbContext.FileDetails.Where(x => x.Tags.Contains(tag)).ToListAsync(token);
    }

    public async Task<FileDetails> UpdateFileDetailsAsync(string id, FileDetails details, CancellationToken token)
    {
        var fileDetails = await _filesDbContext.FileDetails.FindAsync(id, token);
        if (fileDetails == default)
        {
            throw new FilesApiException("No File found for given Id");
        }
        fileDetails.Name = details.Name ?? fileDetails.Name;
        fileDetails.Description = details.Description ?? fileDetails.Description;
        fileDetails.AddedBy = details.AddedBy ?? fileDetails.AddedBy;
        fileDetails.Tags = details.Tags ?? fileDetails.Tags;
        fileDetails.LastModified = DateTime.UtcNow;
        fileDetails.NumberOfDownloads = details.NumberOfDownloads;
        await _filesDbContext.SaveChangesAsync(token);
        return fileDetails;
    }
}