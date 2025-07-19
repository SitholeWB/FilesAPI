using System;

namespace Models
{
    /// <summary>
    /// Detailed analytics for file downloads
    /// </summary>
    public class DownloadAnalytics
    {
        public string Id { get; set; }
        public string FileId { get; set; }
        public string FileName { get; set; }
        public DateTime DownloadDate { get; set; }
        public string UserAgent { get; set; }
        public string IpAddress { get; set; }
        public string Referrer { get; set; }
        public long FileSize { get; set; }
        public TimeSpan? DownloadDuration { get; set; }
        public bool DownloadCompleted { get; set; } = true;
        public string DownloadMethod { get; set; } // "download" or "view"
    }
}
