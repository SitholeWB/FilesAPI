using Contracts;
using Models;
using MongoDB.Bson;
using NSubstitute;
using NUnit.Framework;
using Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Tests
{
	public class StorageServiceTests
	{
		private IStorageService _storageService;
		private IStorageRepository _storageRepository;
		private IFileDetailsRepository _fileDetailsRepository;
		private EventHandlerContainer _eventContainer;
		private IServiceProvider _serviceProvider;

		private FileDetails _fileDetails;
		private FileDetails _fileDetails2;

		private IEnumerable<FileDetails> _fileDetailsList;

		[SetUp]
		public void Setup()
		{
			_storageRepository = Substitute.For<IStorageRepository>();
			_fileDetailsRepository = Substitute.For<IFileDetailsRepository>();
			_serviceProvider = Substitute.For<IServiceProvider>();
			_eventContainer = new EventHandlerContainer(_serviceProvider);
			_storageService = new StorageService(_eventContainer, _storageRepository, _fileDetailsRepository);

			_fileDetails = new FileDetails
			{
				AddedBy = "Welcome Sithole",

				AddedDate = DateTime.UtcNow,
				ContentType = "any",
				Description = "fake file",
				HashId = "feuiwgfh9843eugvbon",
				Id = ObjectId.GenerateNewId().ToString(),
				LastModified = DateTime.UtcNow,
				Name = "some-file",
				NumberOfDownloads = 0,
				Size = 923929,
				StorageId = ObjectId.GenerateNewId().ToString(),
				Tags = new List<string> { "tag-1", "tag-2" }
			};
			_fileDetails2 = new FileDetails
			{
				AddedBy = "John Vuligate",

				AddedDate = DateTime.UtcNow,
				ContentType = "any-2",
				Description = "fake file-2",
				HashId = "feuiwgfh9843eugvbon-2",
				Id = ObjectId.GenerateNewId().ToString(),
				LastModified = DateTime.UtcNow,
				Name = "some-file-2",
				NumberOfDownloads = 0,
				Size = 923889,
				StorageId = ObjectId.GenerateNewId().ToString(),
				Tags = new List<string> { "tag-1", "tag-3" }
			};
			_fileDetailsList = new List<FileDetails> { _fileDetails, _fileDetails2 };
		}

		[Test]
		public async Task GetAllFileDetailsAsync_GivenTwoFileDetailsExist_ShouldReturnTwoFileDetails()
		{
			//Arragnge
			_fileDetailsRepository.GetAllFileDetailsAsync().Returns(a => _fileDetailsList);

			//Act
			var results = await _storageService.GetAllFileDetailsAsync();

			//Assert
			Assert.AreEqual(2, results.Count());
			Assert.IsTrue(results.Any(a => a.Id == _fileDetails.Id));
			Assert.IsTrue(results.Any(a => a.Name == _fileDetails.Name));
			Assert.IsTrue(results.Any(a => a.Description == _fileDetails.Description));
			Assert.IsTrue(results.Any(a => a.HashId == _fileDetails.HashId));
			Assert.IsTrue(results.Any(a => a.Size == _fileDetails.Size));
			Assert.IsTrue(results.Any(a => a.StorageId == _fileDetails.StorageId));

			Assert.IsTrue(results.Any(a => a.Id == _fileDetails2.Id));
			Assert.IsTrue(results.Any(a => a.Name == _fileDetails2.Name));
			Assert.IsTrue(results.Any(a => a.Description == _fileDetails2.Description));
			Assert.IsTrue(results.Any(a => a.HashId == _fileDetails2.HashId));
			Assert.IsTrue(results.Any(a => a.Size == _fileDetails2.Size));
			Assert.IsTrue(results.Any(a => a.StorageId == _fileDetails2.StorageId));
		}

		[Test]
		public async Task GetFileDetailsAsync_GivenTwoFileDetailsExist_ShouldReturnOneFileDetailsForGivenId()
		{
			//Arragnge
			_fileDetailsRepository.GetFileDetailsAsync(Arg.Any<string>()).Returns(args => _fileDetailsList.FirstOrDefault(a => a.Id == args[0].ToString()));

			//Act
			var results = await _storageService.GetFileDetailsAsync(_fileDetails.Id);

			//Assert
			Assert.AreEqual(results.Id, _fileDetails.Id);
			Assert.AreEqual(results.Name, _fileDetails.Name);
			Assert.AreEqual(results.Description, _fileDetails.Description);
			Assert.AreEqual(results.HashId, _fileDetails.HashId);
			Assert.AreEqual(results.Size, _fileDetails.Size);
			Assert.AreEqual(results.StorageId, _fileDetails.StorageId);
		}
	}
}