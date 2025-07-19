using Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contracts
{
    /// <summary>
    /// Repository for download analytics and statistics
    /// </summary>
    public interface IDownloadAnalyticsRepository
    {
        Task<DownloadAnalytics> AddDownloadRecordAsync(DownloadAnalytics analytics);
        Task<IEnumerable<DownloadAnalytics>> GetDownloadHistoryAsync(string fileId);
        Task<IEnumerable<DownloadAnalytics>> GetDownloadHistoryAsync(DateTime fromDate, DateTime toDate);
        Task<DownloadStatistics> GetDownloadStatisticsAsync();
        Task<DownloadStatistics> GetDownloadStatisticsAsync(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<FilePopularityInfo>> GetMostPopularFilesAsync(int count = 10);
        Task<IEnumerable<DailyDownloadStats>> GetDailyStatsAsync(DateTime fromDate, DateTime toDate);
        Task<long> GetTotalDownloadsAsync();
        Task<long> GetTotalBytesDownloadedAsync();
        Task DeleteOldAnalyticsAsync(DateTime olderThan);
    }
}
