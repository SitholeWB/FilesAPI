using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Contracts;
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

		[HttpPost, DisableRequestSizeLimit]
		public async Task<IActionResult> Upload()
		{

			var file = Request.Form.Files[0];

			if (file.Length > 0)
			{
				var details = new FileDetails
				{
					Size = file.Length,
					Name = file.Name,
					AddedDate = DateTime.UtcNow,

				};

				return Ok(await _storageService.UploadFileAsync(file.OpenReadStream(), details));
			}
			else
			{
				return BadRequest();
			}

		}

		[HttpGet]
		public async Task<IActionResult> DownLoad(string id)
		{
			return Ok(await _storageService.DownloadFileAsync(id));
		}

	}
}
