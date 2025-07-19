using Contracts;
using FilesAPI.ViewModels;
using FilesAPI.ViewModels.Mapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Models;
using Models.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FilesAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class StorageController : ControllerBase
	{
		private readonly IStorageService _storageService;

		public StorageController(IStorageService storageService)
		{
			_storageService = storageService;
		}

		//Example from https://dottutorials.net/dotnet-core-web-api-multipart-form-data-upload-file/
		[HttpPost]
		[DisableRequestSizeLimit]
		public async Task<ActionResult<FileDetailsViewModels>> UploadFile([FromForm] UploadImageCommand imageCommand)
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

				var fileDetails = await _storageService.UploadFileAsync(file.OpenReadStream(), details);
				return Ok(ViewModelsMapper.ConvertFileDetailsToFileDetailsViewModels(fileDetails));
			}
			else
			{
				return BadRequest("File is required.");
			}
		}

		[HttpGet("{id}/download")]
		public async Task<IActionResult> DownLoadFile(string id)
		{
			var (content, details) = await _storageService.DownloadFileAsync(id);
			
			// Record analytics
			var userAgent = Request.Headers["User-Agent"].ToString();
			var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
			var referrer = Request.Headers["Referer"].ToString();
			
			// Fire and forget analytics recording
			_ = Task.Run(async () =>
			{
				try
				{
					var analyticsService = HttpContext.RequestServices.GetService<IAnalyticsService>();
					if (analyticsService != null)
					{
						await analyticsService.RecordDownloadAsync(id, userAgent, ipAddress, referrer, "download");
					}
				}
				catch
				{
					// Ignore analytics errors to not affect file download
				}
			});
			
			this.Response.ContentLength = details.Size;
			this.Response.Headers["Accept-Ranges"] = "bytes";
			this.Response.Headers["Content-Range"] = "bytes 0-" + details.Size;
			return File(content, details.ContentType, details.Name);
		}

		[HttpGet("{id}/view")]
		public async Task<FileStreamResult> DownloadView(string id)
		{
			var (stream, details) = await _storageService.DownloadFileAsync(id);
			
			// Record analytics for view
			var userAgent = Request.Headers["User-Agent"].ToString();
			var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
			var referrer = Request.Headers["Referer"].ToString();
			
			// Fire and forget analytics recording
			_ = Task.Run(async () =>
			{
				try
				{
					var analyticsService = HttpContext.RequestServices.GetService<IAnalyticsService>();
					if (analyticsService != null)
					{
						await analyticsService.RecordDownloadAsync(id, userAgent, ipAddress, referrer, "view");
					}
				}
				catch
				{
					// Ignore analytics errors to not affect file download
				}
			});
			
			this.Response.ContentLength = details.Size;
			this.Response.Headers["Accept-Ranges"] = "bytes";
			this.Response.Headers["Content-Range"] = "bytes 0-" + details.Size;
			return new FileStreamResult(stream, details.ContentType);
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<FileDetailsViewModels>>> GetAllFileDetails()
		{
			var fileDetailsList = await _storageService.GetAllFileDetailsAsync();
			return Ok(fileDetailsList.Select(a => ViewModelsMapper.ConvertFileDetailsToFileDetailsViewModels(a)));
		}

		[HttpGet("details/{id}")]
		public async Task<ActionResult<FileDetailsViewModels>> GetFileDetails(string id)
		{
			var fileDetails = await _storageService.GetFileDetailsAsync(id);
			return Ok(ViewModelsMapper.ConvertFileDetailsToFileDetailsViewModels(fileDetails));
		}

		[HttpPut("details/{id}")]
		public async Task<ActionResult<FileDetailsViewModels>> UpdateFileDetails(FileDetails details, string id)
		{
			details.Id = id;
			var fileDetails = await _storageService.UpdateFileDetailsAsync(id, details);
			return Ok(ViewModelsMapper.ConvertFileDetailsToFileDetailsViewModels(fileDetails));
		}

		[HttpGet("details/tags/{tag}")]
		public async Task<ActionResult<IEnumerable<FileDetailsViewModels>>> GetFileDetailsByTag(string tag)
		{
			var fileDetailsList = await _storageService.GetFileDetailsByTagAsync(tag);
			return Ok(fileDetailsList.Select(a => ViewModelsMapper.ConvertFileDetailsToFileDetailsViewModels(a)));
		}

		[HttpDelete("{id}")]
		public async Task<ActionResult<string>> DeleteFileAsync(string id)
		{
			string deletedId = await _storageService.DeleteFileAsync(id);
			return Ok($"Deleted '{deletedId}' successfully.");
		}
	}
}