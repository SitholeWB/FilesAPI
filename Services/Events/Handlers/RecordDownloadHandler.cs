using Contracts;
using Models.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Services.Events.Handlers
{
	public class RecordDownloadHandler : IEventHandler<FileDownloadedEvent>
	{
		private readonly IStorageService _storageService;

		public RecordDownloadHandler(IStorageService storageService)
		{
			_storageService = storageService;
		}

		public async Task RunAsync(FileDownloadedEvent obj)
		{
			await _storageService.IncrementDownloadCountAsync(obj.FileDetails.Id);
		}
	}
}