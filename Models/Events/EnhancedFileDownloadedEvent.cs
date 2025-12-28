namespace Models;

/// <summary>
/// Enhanced file download event with additional analytics data
/// </summary>
public class EnhancedFileDownloadedEvent
{
    public FileDetails FileDetails { get; set; }
    public DateTime DownloadStartTime { get; set; }
    public string UserAgent { get; set; }
    public string IpAddress { get; set; }
    public string Referrer { get; set; }
    public string DownloadMethod { get; set; } // "download" or "view"
    public string RequestId { get; set; }
}