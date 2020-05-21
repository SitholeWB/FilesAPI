using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Commands
{
	public class UploadImageCommand
	{
		public IFormFile File { get; set; }
		public string Description { get; set; }
		public IEnumerable<string> Tags { get; set; }
	}
}
