using System;
using System.Collections.Generic;
using System.Linq;

namespace Models
{
	public class FileDetails
	{
		public string Id { get; set; }
		public string SharedStorageId { get; set; }
		public string HashId { get; set; }
		public string Name { get; set; }
		public long Size { get; set; }
		public DateTime AddedDate { get; set; }
		public string AddedBy { get; set; }
		public string Description { get; set; }
		public IEnumerable<string> Tags { get; set; } = Enumerable.Empty<string>();
		public DateTime LastModified { get; set; }
		public string ContentType { get; set; }
		public int NumberOfDownloads { get; set; } = 0;
	}
}