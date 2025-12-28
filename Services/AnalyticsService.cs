using Contracts;
using Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Services;

/// <summary>
/// Service for download analytics and reporting
/// </summary>
public class AnalyticsService : IAnalyticsService
{
    private readonly IDownloadAnalyticsRepository _analyticsRepository;
    private readonly IFileDetailsRepository _fileDetailsRepository;

    public AnalyticsService(
        IDownloadAnalyticsRepository analyticsRepository,
        IFileDetailsRepository fileDetailsRepository)
    {
        _analyticsRepository = analyticsRepository ?? throw new ArgumentNullException(nameof(analyticsRepository));
        _fileDetailsRepository = fileDetailsRepository ?? throw new ArgumentNullException(nameof(fileDetailsRepository));
    }

    public async Task RecordDownloadAsync(string fileId, string userAgent, string ipAddress, string referrer, string method, CancellationToken token)
    {
        var fileDetails = await _fileDetailsRepository.GetFileDetailsAsync(fileId, token);
        if (fileDetails == null) return;

        var analytics = new DownloadAnalytics
        {
            FileId = fileId,
            FileName = fileDetails.Name,
            DownloadDate = DateTime.UtcNow,
            UserAgent = userAgent ?? "Unknown",
            IpAddress = ipAddress ?? "Unknown",
            Referrer = referrer ?? "Direct",
            FileSize = fileDetails.Size,
            DownloadMethod = method ?? "download",
            DownloadCompleted = true
        };

        await _analyticsRepository.AddDownloadRecordAsync(analytics, token);
    }

    public async Task<DownloadStatistics> GetDownloadStatisticsAsync(CancellationToken token)
    {
        return await _analyticsRepository.GetDownloadStatisticsAsync(token);
    }

    public async Task<DownloadStatistics> GetDownloadStatisticsAsync(DateTime fromDate, DateTime toDate, CancellationToken token)
    {
        return await _analyticsRepository.GetDownloadStatisticsAsync(fromDate, toDate, token);
    }

    public async Task<IEnumerable<FilePopularityInfo>> GetMostPopularFilesAsync(int count = 10, CancellationToken token = default)
    {
        return await _analyticsRepository.GetMostPopularFilesAsync(count, token);
    }

    public async Task<IEnumerable<DailyDownloadStats>> GetDailyStatsAsync(int days = 30, CancellationToken token = default)
    {
        var fromDate = DateTime.UtcNow.AddDays(-days).Date;
        var toDate = DateTime.UtcNow.Date;
        return await _analyticsRepository.GetDailyStatsAsync(fromDate, toDate, token);
    }

    public async Task<IEnumerable<DownloadAnalytics>> GetDownloadHistoryAsync(string fileId, CancellationToken token)
    {
        return await _analyticsRepository.GetDownloadHistoryAsync(fileId, token);
    }

    public async Task CleanupOldAnalyticsAsync(int daysToKeep = 365, CancellationToken token = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
        await _analyticsRepository.DeleteOldAnalyticsAsync(cutoffDate, token);
    }
}