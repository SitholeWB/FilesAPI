using Contracts;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Services.Middlewares
{
	public class VideoDownloadsCountMiddleware
	{
		private readonly RequestDelegate next;

		public VideoDownloadsCountMiddleware(RequestDelegate next)
		{
			this.next = next;
		}

		public async Task Invoke(HttpContext context, IStorageService storageService /* other dependencies */)
		{
			//TODO: Identify unique views
			try
			{
				var header = context.Request.Headers["Sec-Fetch-Dest"];
				if(header.ToString().ToLower() == "video")
				{
					var details = await storageService.GetFileDetailsAsync(context.Request.Path.Value.Split("/")[3]);
					if (details.NumberOfDownloads.HasValue)
					{
						details.NumberOfDownloads++;
					}
					else
					{
						details.NumberOfDownloads = 1;
					}
					await storageService.UpdateFileDetailsAsync(details);
				}
			}
			finally
			{
				await next(context);
			}
		}
	}
}
