using System;

namespace Models
{
	public class FileDetails
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public long Size { get; set; }
		public DateTime AddedDate { get; set; }
		public string AddedBy { get; set; }
		public string Description { get; set; }
		public string Tags { get; set; }

	}
}
