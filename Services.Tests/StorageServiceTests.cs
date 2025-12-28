using Contracts;
using Models;
using MongoDB.Bson;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Tests;

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
        //Arrange
        _fileDetailsRepository.GetAllFileDetailsAsync(TestContext.CurrentContext.CancellationToken).Returns(a => _fileDetailsList);

        //Act
        var results = await _storageService.GetAllFileDetailsAsync(TestContext.CurrentContext.CancellationToken);

        //Assert
        ClassicAssert.AreEqual(2, results.Count());
        ClassicAssert.IsTrue(results.Any(a => a.Id == _fileDetails.Id));
        ClassicAssert.IsTrue(results.Any(a => a.Name == _fileDetails.Name));
        ClassicAssert.IsTrue(results.Any(a => a.Description == _fileDetails.Description));
        ClassicAssert.IsTrue(results.Any(a => a.HashId == _fileDetails.HashId));
        ClassicAssert.IsTrue(results.Any(a => a.Size == _fileDetails.Size));
        ClassicAssert.IsTrue(results.Any(a => a.StorageId == _fileDetails.StorageId));

        ClassicAssert.IsTrue(results.Any(a => a.Id == _fileDetails2.Id));
        ClassicAssert.IsTrue(results.Any(a => a.Name == _fileDetails2.Name));
        ClassicAssert.IsTrue(results.Any(a => a.Description == _fileDetails2.Description));
        ClassicAssert.IsTrue(results.Any(a => a.HashId == _fileDetails2.HashId));
        ClassicAssert.IsTrue(results.Any(a => a.Size == _fileDetails2.Size));
        ClassicAssert.IsTrue(results.Any(a => a.StorageId == _fileDetails2.StorageId));
    }

    public async Task GetAllFileDetailsAsync_GivenNoFileDetailsExist_ShouldReturnEmptyFileDetailsList()
    {
        //Arrange
        _fileDetailsRepository.GetAllFileDetailsAsync(TestContext.CurrentContext.CancellationToken).Returns(a => Enumerable.Empty<FileDetails>());

        //Act
        var results = await _storageService.GetAllFileDetailsAsync(TestContext.CurrentContext.CancellationToken);

        //Assert
        ClassicAssert.AreEqual(0, results.Count());
    }

    [Test]
    public async Task GetFileDetailsAsync_GivenTwoFileDetailsExist_ShouldReturnOneFileDetailsForGivenId()
    {
        //Arrange
        _fileDetailsRepository.GetFileDetailsAsync(Arg.Any<string>(), TestContext.CurrentContext.CancellationToken).Returns(args => _fileDetailsList.FirstOrDefault(a => a.Id == args[0].ToString()));

        //Act
        var results = await _storageService.GetFileDetailsAsync(_fileDetails.Id, TestContext.CurrentContext.CancellationToken);

        //Assert
        ClassicAssert.AreEqual(results.Id, _fileDetails.Id);
        ClassicAssert.AreEqual(results.Name, _fileDetails.Name);
        ClassicAssert.AreEqual(results.Description, _fileDetails.Description);
        ClassicAssert.AreEqual(results.HashId, _fileDetails.HashId);
        ClassicAssert.AreEqual(results.Size, _fileDetails.Size);
        ClassicAssert.AreEqual(results.StorageId, _fileDetails.StorageId);
    }

    [Test]
    public async Task GetFileDetailsAsync_GivenIdDonnotExist_ShouldReturnNull()
    {
        //Arrange
        _fileDetailsRepository.GetFileDetailsAsync(Arg.Any<string>(), TestContext.CurrentContext.CancellationToken).Returns(args => _fileDetailsList.FirstOrDefault(a => a.Id == args[0].ToString()));

        //Act
        var results = await _storageService.GetFileDetailsAsync(Guid.NewGuid().ToString(), TestContext.CurrentContext.CancellationToken);

        //Assert
        ClassicAssert.IsNull(results);
    }

    [TestCase(0)]
    [TestCase(1)]
    [Test]
    public async Task GetFileDetailsByTagAsync_GivenTagExist_ShouldReturnFileDetailsForTag(int tagIndex)
    {
        //Arrange
        _fileDetailsRepository.GetFileDetailsByTagAsync(Arg.Any<string>(), TestContext.CurrentContext.CancellationToken).Returns(args => _fileDetailsList.Where(a => a.Tags.Contains(args[0].ToString())));
        var tag = _fileDetails.Tags.ElementAt(tagIndex);
        //Act

        var results = await _storageService.GetFileDetailsByTagAsync(tag, TestContext.CurrentContext.CancellationToken);

        //Assert
        ClassicAssert.IsTrue(results.Any(a => a.Tags.Contains(tag)));
    }

    [Test]
    public async Task GetFileDetailsByTagAsync_GivenTagDonnotExist_ShouldReturnEmptyFileDetailsList()
    {
        //Arrange
        _fileDetailsRepository.GetFileDetailsByTagAsync(Arg.Any<string>(), TestContext.CurrentContext.CancellationToken).Returns(args => _fileDetailsList.Where(a => a.Tags.Contains(args[0].ToString())));
        var tag = Guid.NewGuid().ToString();

        //Act

        var results = await _storageService.GetFileDetailsByTagAsync(tag, TestContext.CurrentContext.CancellationToken);

        //Assert
        ClassicAssert.IsFalse(results.Any(a => a.Tags.Contains(tag)));
    }
}