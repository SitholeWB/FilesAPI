using System;
using System.Collections.Generic;

namespace Models
{
    /// <summary>
    /// Comprehensive download statistics
    /// </summary>
    public class DownloadStatistics
    {
        public int TotalDownloads { get; set; }
        public int TotalFiles { get; set; }
        public long TotalBytesDownloaded { get; set; }
        public DateTime? LastDownloadDate { get; set; }
        public DateTime? FirstDownloadDate { get; set; }
        public double AverageDownloadsPerDay { get; set; }
        public IEnumerable<FilePopularityInfo> MostPopularFiles { get; set; }
        public IEnumerable<DailyDownloadStats> DailyStats { get; set; }
    }

    public class FilePopularityInfo
    {
        public string FileId { get; set; }
        public string FileName { get; set; }
        public int DownloadCount { get; set; }
        public long FileSize { get; set; }
        public DateTime LastDownloaded { get; set; }
        public string ContentType { get; set; }
    }

    public class DailyDownloadStats
    {
        public DateTime Date { get; set; }
        public int DownloadCount { get; set; }
        public long BytesDownloaded { get; set; }
        public int UniqueFiles { get; set; }
    }
}
