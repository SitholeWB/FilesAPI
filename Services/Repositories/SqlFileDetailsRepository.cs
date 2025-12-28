using Contracts;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Exceptions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Repositories;

public class SqlFileDetailsRepository : IFileDetailsRepository
{
    private readonly FilesDbContext _filesDbContext;

    public SqlFileDetailsRepository(FilesDbContext filesDbContext)
    {
        _filesDbContext = filesDbContext;
    }

    public async Task<FileDetails> AddFileDetailsAsync(FileDetails details)
    {
        details.Id = Guid.NewGuid().ToString();
        await _filesDbContext.AddAsync(details);
        await _filesDbContext.SaveChangesAsync();
        return details;
    }

    public async Task DeleteFileAsync(string id)
    {
        var fileDetails = await _filesDbContext.FileDetails.FindAsync(id);
        if (fileDetails == default)
        {
            throw new FilesApiException("No File found for given Id");
        }
        _filesDbContext.Remove(fileDetails);
        await _filesDbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<FileDetails>> GetAllFileDetailsAsync()
    {
        return await _filesDbContext.FileDetails.ToListAsync();
    }

    public async Task<FileDetails> GetFileDetailsAsync(string id)
    {
        return await _filesDbContext.FileDetails.FindAsync(id);
    }

    public async Task<FileDetails> GetFileDetailsByHashIdAsync(string hashId)
    {
        return await _filesDbContext.FileDetails.Where(x => x.HashId == hashId).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<FileDetails>> GetFileDetailsByTagAsync(string tag)
    {
        return await _filesDbContext.FileDetails.Where(x => x.Tags.Contains(tag)).ToListAsync();
    }

    public async Task<FileDetails> UpdateFileDetailsAsync(string id, FileDetails details)
    {
        var fileDetails = await _filesDbContext.FileDetails.FindAsync(id);
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
        await _filesDbContext.SaveChangesAsync();
        return fileDetails;
    }
}