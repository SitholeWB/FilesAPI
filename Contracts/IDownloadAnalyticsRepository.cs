namespace Contracts;

/// <summary>
/// Repository for download analytics and statistics
/// </summary>
public interface IDownloadAnalyticsRepository
{
    Task<DownloadAnalytics> AddDownloadRecordAsync(DownloadAnalytics analytics, CancellationToken token);

    Task<IEnumerable<DownloadAnalytics>> GetDownloadHistoryAsync(string fileId, CancellationToken token);

    Task<IEnumerable<DownloadAnalytics>> GetDownloadHistoryAsync(DateTime fromDate, DateTime toDate, CancellationToken token);

    Task<DownloadStatistics> GetDownloadStatisticsAsync(CancellationToken token);

    Task<DownloadStatistics> GetDownloadStatisticsAsync(DateTime fromDate, DateTime toDate, CancellationToken token);

    Task<IEnumerable<FilePopularityInfo>> GetMostPopularFilesAsync(int count = 10, CancellationToken token = default);

    Task<IEnumerable<DailyDownloadStats>> GetDailyStatsAsync(DateTime fromDate, DateTime toDate, CancellationToken token);

    Task<long> GetTotalDownloadsAsync(CancellationToken token);

    Task<long> GetTotalBytesDownloadedAsync(CancellationToken token);

    Task DeleteOldAnalyticsAsync(DateTime olderThan, CancellationToken token);
}