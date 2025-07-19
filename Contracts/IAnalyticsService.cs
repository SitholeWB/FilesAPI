using Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contracts
{
    /// <summary>
    /// Service for download analytics and reporting
    /// </summary>
    public interface IAnalyticsService
    {
        Task RecordDownloadAsync(string fileId, string userAgent, string ipAddress, string referrer, string method);
        Task<DownloadStatistics> GetDownloadStatisticsAsync();
        Task<DownloadStatistics> GetDownloadStatisticsAsync(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<FilePopularityInfo>> GetMostPopularFilesAsync(int count = 10);
        Task<IEnumerable<DailyDownloadStats>> GetDailyStatsAsync(int days = 30);
        Task<IEnumerable<DownloadAnalytics>> GetDownloadHistoryAsync(string fileId);
        Task CleanupOldAnalyticsAsync(int daysToKeep = 365);
    }
}
