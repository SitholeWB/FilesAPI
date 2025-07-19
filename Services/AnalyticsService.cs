using Contracts;
using Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services
{
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

        public async Task RecordDownloadAsync(string fileId, string userAgent, string ipAddress, string referrer, string method)
        {
            var fileDetails = await _fileDetailsRepository.GetFileDetailsAsync(fileId);
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

            await _analyticsRepository.AddDownloadRecordAsync(analytics);
        }

        public async Task<DownloadStatistics> GetDownloadStatisticsAsync()
        {
            return await _analyticsRepository.GetDownloadStatisticsAsync();
        }

        public async Task<DownloadStatistics> GetDownloadStatisticsAsync(DateTime fromDate, DateTime toDate)
        {
            return await _analyticsRepository.GetDownloadStatisticsAsync(fromDate, toDate);
        }

        public async Task<IEnumerable<FilePopularityInfo>> GetMostPopularFilesAsync(int count = 10)
        {
            return await _analyticsRepository.GetMostPopularFilesAsync(count);
        }

        public async Task<IEnumerable<DailyDownloadStats>> GetDailyStatsAsync(int days = 30)
        {
            var fromDate = DateTime.UtcNow.AddDays(-days).Date;
            var toDate = DateTime.UtcNow.Date;
            return await _analyticsRepository.GetDailyStatsAsync(fromDate, toDate);
        }

        public async Task<IEnumerable<DownloadAnalytics>> GetDownloadHistoryAsync(string fileId)
        {
            return await _analyticsRepository.GetDownloadHistoryAsync(fileId);
        }

        public async Task CleanupOldAnalyticsAsync(int daysToKeep = 365)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            await _analyticsRepository.DeleteOldAnalyticsAsync(cutoffDate);
        }
    }
}
