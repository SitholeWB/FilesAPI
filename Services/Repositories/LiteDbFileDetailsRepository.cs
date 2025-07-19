using Contracts;
using LiteDB;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Repositories
{
    /// <summary>
    /// LiteDB implementation for self-contained file storage without external dependencies
    /// </summary>
    public class LiteDbFileDetailsRepository : IFileDetailsRepository
    {
        private readonly string _connectionString;
        private readonly string _collectionName = "filedetails";

        public LiteDbFileDetailsRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<FileDetails> AddFileDetailsAsync(FileDetails fileDetails)
        {
            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(_connectionString);
                var collection = db.GetCollection<FileDetails>(_collectionName);
                
                // Generate new ID if not provided
                if (string.IsNullOrEmpty(fileDetails.Id))
                {
                    fileDetails.Id = ObjectId.NewObjectId().ToString();
                }

                collection.Insert(fileDetails);
                return fileDetails;
            });
        }

        public async Task<FileDetails> GetFileDetailsAsync(string id)
        {
            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(_connectionString);
                var collection = db.GetCollection<FileDetails>(_collectionName);
                return collection.FindById(id);
            });
        }

        public async Task<IEnumerable<FileDetails>> GetAllFileDetailsAsync()
        {
            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(_connectionString);
                var collection = db.GetCollection<FileDetails>(_collectionName);
                return collection.FindAll().ToList();
            });
        }

        public async Task<FileDetails> UpdateFileDetailsAsync(string id, FileDetails fileDetails)
        {
            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(_connectionString);
                var collection = db.GetCollection<FileDetails>(_collectionName);
                
                // Ensure the ID matches
                fileDetails.Id = id;
                collection.Update(fileDetails);
                return fileDetails;
            });
        }

        public async Task DeleteFileAsync(string id)
        {
            await Task.Run(() =>
            {
                using var db = new LiteDatabase(_connectionString);
                var collection = db.GetCollection<FileDetails>(_collectionName);
                collection.Delete(id);
            });
        }

        public async Task<FileDetails> GetFileDetailsByHashIdAsync(string hashId)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var db = new LiteDatabase(_connectionString);
                    var collection = db.GetCollection<FileDetails>(_collectionName);
                    return collection.FindOne(x => x.HashId == hashId);
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it appropriately
                    // For now, return null if there's an issue
                    System.Console.WriteLine($"Error in GetFileDetailsByHashIdAsync: {ex.Message}");
                    return null;
                }
            });
        }

        public async Task<IEnumerable<FileDetails>> GetFileDetailsByTagAsync(string tag)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var db = new LiteDatabase(_connectionString);
                    var collection = db.GetCollection<FileDetails>(_collectionName);
                    return collection.Find(x => x.Tags.Contains(tag)).ToList();
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"Error in GetFileDetailsByTagAsync: {ex.Message}");
                    return new List<FileDetails>();
                }
            });
        }

        public async Task<bool> FileDetailsExistsAsync(string id)
        {
            return await Task.Run(() =>
            {
                using var db = new LiteDatabase(_connectionString);
                var collection = db.GetCollection<FileDetails>(_collectionName);
                return collection.Exists(Query.EQ("_id", id));
            });
        }
    }
}
