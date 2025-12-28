namespace Contracts;

/// <summary>
/// Service for download analytics and reporting
/// </summary>
public interface IAnalyticsService
{
    Task RecordDownloadAsync(string fileId, string userAgent, string ipAddress, string referrer, string method, CancellationToken token);

    Task<DownloadStatistics> GetDownloadStatisticsAsync(CancellationToken token);

    Task<DownloadStatistics> GetDownloadStatisticsAsync(DateTime fromDate, DateTime toDate, CancellationToken token);

    Task<IEnumerable<FilePopularityInfo>> GetMostPopularFilesAsync(int count = 10, CancellationToken token = default);

    Task<IEnumerable<DailyDownloadStats>> GetDailyStatsAsync(int days = 30, CancellationToken token = default);

    Task<IEnumerable<DownloadAnalytics>> GetDownloadHistoryAsync(string fileId, CancellationToken token);

    Task CleanupOldAnalyticsAsync(int daysToKeep = 365, CancellationToken token = default);
}