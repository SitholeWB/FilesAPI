using Contracts;
using FilesAPI.ViewModels;
using FilesAPI.ViewModels.Mapper;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FilesAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StorageController : ControllerBase
{
    private readonly IStorageService _storageService;
    private readonly EventHandlerContainer _eventContainer;

    public StorageController(IStorageService storageService, EventHandlerContainer eventContainer)
    {
        _storageService = storageService;
        _eventContainer = eventContainer;
    }

    //Example from https://dottutorials.net/dotnet-core-web-api-multipart-form-data-upload-file/
    [HttpPost]
    [DisableRequestSizeLimit]
    public async Task<ActionResult<FileDetailsViewModels>> UploadFile([FromForm] UploadImageCommand imageCommand, CancellationToken token = default)
    {
        var file = imageCommand.File;
        if (file.Length > 0)
        {
            var details = new FileDetails
            {
                Size = file.Length,
                Name = file.FileName,
                AddedDate = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                ContentType = file.ContentType,
                Description = imageCommand.Description,
                Tags = imageCommand.Tags
            };

            var fileDetails = await _storageService.UploadFileAsync(file.OpenReadStream(), details, token);
            return Ok(ViewModelsMapper.ConvertFileDetailsToFileDetailsViewModels(fileDetails));
        }
        else
        {
            return BadRequest("File is required.");
        }
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownLoadFile(string id, CancellationToken token = default)
    {
        var (content, details) = await _storageService.DownloadFileAsync(id, token);

        // Record analytics for view
        await RecordAnalyticsAsync(details, token);

        this.Response.ContentLength = details.Size;
        this.Response.Headers["Accept-Ranges"] = "bytes";
        this.Response.Headers["Content-Range"] = "bytes 0-" + details.Size;
        return File(content, details.ContentType, details.Name);
    }

    [HttpGet("{id}/view")]
    public async Task<FileStreamResult> DownloadView(string id, CancellationToken token = default)
    {
        var (stream, details) = await _storageService.DownloadFileAsync(id, token);

        // Record analytics for view
        await RecordAnalyticsAsync(details, token);

        this.Response.ContentLength = details.Size;
        this.Response.Headers["Accept-Ranges"] = "bytes";
        this.Response.Headers["Content-Range"] = "bytes 0-" + details.Size;
        return new FileStreamResult(stream, details.ContentType);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FileDetailsViewModels>>> GetAllFileDetails(CancellationToken token = default)
    {
        var fileDetailsList = await _storageService.GetAllFileDetailsAsync(token);
        return Ok(fileDetailsList.Select(a => ViewModelsMapper.ConvertFileDetailsToFileDetailsViewModels(a)));
    }

    [HttpGet("details/{id}")]
    public async Task<ActionResult<FileDetailsViewModels>> GetFileDetails(string id, CancellationToken token = default)
    {
        var fileDetails = await _storageService.GetFileDetailsAsync(id, token);
        return Ok(ViewModelsMapper.ConvertFileDetailsToFileDetailsViewModels(fileDetails));
    }

    [HttpPut("details/{id}")]
    public async Task<ActionResult<FileDetailsViewModels>> UpdateFileDetails(FileDetails details, string id, CancellationToken token = default)
    {
        details.Id = id;
        var fileDetails = await _storageService.UpdateFileDetailsAsync(id, details, token);
        return Ok(ViewModelsMapper.ConvertFileDetailsToFileDetailsViewModels(fileDetails));
    }

    [HttpGet("details/tags/{tag}")]
    public async Task<ActionResult<IEnumerable<FileDetailsViewModels>>> GetFileDetailsByTag(string tag, CancellationToken token = default)
    {
        var fileDetailsList = await _storageService.GetFileDetailsByTagAsync(tag, token);
        return Ok(fileDetailsList.Select(a => ViewModelsMapper.ConvertFileDetailsToFileDetailsViewModels(a)));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<string>> DeleteFileAsync(string id, CancellationToken token = default)
    {
        string deletedId = await _storageService.DeleteFileAsync(id, token);
        return Ok($"Deleted '{deletedId}' successfully.");
    }

    private async Task RecordAnalyticsAsync(FileDetails details, CancellationToken token)
    {
        var userAgent = Request.Headers["User-Agent"].ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var referrer = Request.Headers["Referer"].ToString();
        var enhancedFileDownloadedEvent = new EnhancedFileDownloadedEvent
        {
            DownloadMethod = "view",
            DownloadStartTime = DateTime.UtcNow,
            FileDetails = details,
            UserAgent = userAgent,
            IpAddress = ipAddress,
            Referrer = referrer,
            RequestId = HttpContext.TraceIdentifier
        };
        await _eventContainer.PublishAsync(enhancedFileDownloadedEvent, token);
    }
}