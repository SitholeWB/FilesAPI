namespace Models;

public class FileDownloadedEvent : EventBase
{
    public FileDetails FileDetails { get; set; }
}