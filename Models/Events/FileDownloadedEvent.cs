namespace Models.Events
{
	public class FileDownloadedEvent : EventBase
	{
		public FileDetails FileDetails { get; set; }
	}
}