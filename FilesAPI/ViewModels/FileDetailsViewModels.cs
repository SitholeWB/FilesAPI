using System;
using System.Collections.Generic;
using System.Linq;

namespace FilesAPI.ViewModels
{
	public class FileDetailsViewModels
	{
		public string Id { get; set; }
		public string HashId { get; set; }
		public string Name { get; set; }
		public long Size { get; set; }
		public string AddedBy { get; set; }
		public string Description { get; set; }
		public IEnumerable<string> Tags { get; set; } = Enumerable.Empty<string>();
		public string ContentType { get; set; }
		public int NumberOfDownloads { get; set; } = 0;
		public DateTime AddedDate { get; set; }
		public DateTime LastModified { get; set; }
	}
}