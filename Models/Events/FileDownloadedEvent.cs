using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Events
{
	public class FileDownloadedEvent : EventBase
	{
		public FileDetails FileDetails { get; set; }
	}
}