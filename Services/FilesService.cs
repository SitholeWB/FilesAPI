using Contracts;
using LiteDB;
using Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public sealed class FilesService //: IStorageService
    {
        private readonly ILiteDatabase _liteDatabase;

        public FilesService(ISettingsService settingsService)
        {
            _liteDatabase = new LiteDatabase(settingsService.GetLiteDBAppSettings().ConnectionString);
        }

        public void Dispose()
        {
            _liteDatabase.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task<string> DeleteFileAsync(string id)
        {
            return await Task.Run(() =>
            {
                var collection = _liteDatabase.GetCollection<FileDetails>("FileDetails");
                var success = collection.Delete(id);
                return (success ? id : string.Empty);
            });
        }

        public async Task<(Stream, FileDetails)> DownloadFileAsync(string id)
        {
            return await Task.Run(() =>
            {
                var collection = _liteDatabase.GetCollection<FileDetails>("FileDetails");
                var fileDetails = collection.FindById(id);
                var stream = _liteDatabase.FileStorage.OpenRead(id);
                return (stream, fileDetails);
            });
        }

        public async Task IncrementDownloadCountAsync(string id)
        {
            await Task.Run(() =>
            {
                var collection = _liteDatabase.GetCollection<FileDetails>("FileDetails");
                var fileDetails = collection.FindById(id);
                if (fileDetails != null)
                {
                    // Increment the download count
                    fileDetails.NumberOfDownloads++;
                    // Update the file details in the collection
                    var success = collection.Update(fileDetails);
                    if (!success)
                    {
                        throw new Exception("Error while updating download count");
                    }
                }
                else
                {
                    throw new Exception("File not found");
                }
            });
        }

        public async Task<IEnumerable<FileDetails>> GetAllFileDetailsAsync()
        {
            return await Task.Run(() =>
           {
               var collection = _liteDatabase.GetCollection<FileDetails>("FileDetails");
               return collection.Query().ToList();
           });
        }

        public async Task<FileDetails> GetFileDetailsAsync(string id)
        {
            return await Task.Run(() =>
            {
                var collection = _liteDatabase.GetCollection<FileDetails>("FileDetails");
                return collection.FindById(id);
            });
        }

        public async Task<IEnumerable<FileDetails>> GetFileDetailsByTagAsync(string tag)
        {
            return await Task.Run(() =>
            {
                var collection = _liteDatabase.GetCollection<FileDetails>("FileDetails");
                return collection.Query().Where(a => a.Tags.Contains(tag.Trim())).ToList();
            });
        }

        public async Task<FileDetails> UpdateFileDetailsAsync(FileDetails details)
        {
            return await Task.Run(() =>
            {
                var collection = _liteDatabase.GetCollection<FileDetails>("FileDetails");
                var fileDetails = collection.FindById(details.Id);
                fileDetails.Description = details.Description;
                fileDetails.LastModified = DateTime.UtcNow;
                fileDetails.Name = details.Name;
                fileDetails.Tags = details.Tags;
                var success = collection.Update(fileDetails);
                return (success ? fileDetails : throw new Exception("Error while updating"));
            });
        }

        public async Task<FileDetails> UploadFileAsync(Stream fileStream, FileDetails fileDetails)
        {
            var collection = _liteDatabase.GetCollection<FileDetails>("FileDetails");
            fileDetails.Id = ObjectId.NewObjectId().ToString();
            collection.Insert(fileDetails.Id, fileDetails);
            var obj = _liteDatabase.FileStorage.Upload(fileDetails.Id, fileDetails.Name, fileStream);
            return fileDetails;
        }
    }
}