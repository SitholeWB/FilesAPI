using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Contracts;
using FilesAPI.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;

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

		//Example from https://janaks.com.np/file-upload-asp-net-core-web-api/
		[HttpPost]
		public async Task<IActionResult> UploadFile(IFormFile file)
		{
			if (file.Length > 0)
			{
				var details = new FileDetails
				{
					Size = file.Length,
					Name = file.FileName,
					AddedDate = DateTime.UtcNow,
					LastModified = DateTime.UtcNow,
					ContentType = file.ContentType
				};

				return Ok(await _storageService.UploadFileAsync(file.OpenReadStream(), details));
			}
			else
			{
				return BadRequest();
			}
		}

		/*
		[HttpGet("{id}")]
		public async Task<IActionResult> DownLoadFile(string id)
		{
			var (content, details) = await _storageService.DownloadFileAsync(id);
			return File(content, details.ContentType, details.Name);
		}
		*/

		[HttpGet("{id}/view")]
		public async Task<FileStreamResult> DownloadView(string id)
		{
			var (stream, details) = await _storageService.DownloadFileAsync(id);
			return new FileStreamResult(stream, details.ContentType);
		}

		[HttpGet]
		public async Task<IActionResult> GetAllFileDetails()
		{
			return Ok(await _storageService.GetAllFileDetailsAsync());
		}

		[HttpGet("details/{id}")]
		public async Task<IActionResult> GetFileDetails(string id)
		{
			return Ok(await _storageService.GetFileDetailsAsync(id));
		}

		[HttpPut("details/{id}")]
		public async Task<IActionResult> UpdateFileDetails(FileDetails details, string id)
		{
			details.Id = id;
			return Ok(await _storageService.UpdateFileDetailsAsync(details));
		}

		[HttpGet("details/tags/{tag}")]
		public async Task<IActionResult> GetFileDetailsByTag(string tag)
		{
			return Ok(await _storageService.GetFileDetailsByTagAsync(tag));
		}


		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteFileAsync(string id)
		{
			string deletedId = await _storageService.DeleteFileAsync(id);
			return base.Ok($"Deleted {deletedId} successfully");
		}

	}
}
