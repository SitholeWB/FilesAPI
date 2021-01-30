using System;
using System.Runtime.Serialization;

namespace Models.Exceptions
{
	[Serializable]
	public sealed class FilesApiException : Exception
	{
		public FilesApiException(string message) : base(message)
		{
		}

		public FilesApiException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public FilesApiException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}