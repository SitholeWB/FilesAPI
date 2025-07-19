using Contracts;
using Microsoft.AspNetCore.Mvc;
using Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FilesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
        }

        /// <summary>
        /// Get overall download statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<DownloadStatistics>> GetDownloadStatistics()
        {
            var stats = await _analyticsService.GetDownloadStatisticsAsync();
            return Ok(stats);
        }

        /// <summary>
        /// Get download statistics for a specific date range
        /// </summary>
        [HttpGet("statistics/{fromDate}/{toDate}")]
        public async Task<ActionResult<DownloadStatistics>> GetDownloadStatistics(DateTime fromDate, DateTime toDate)
        {
            var stats = await _analyticsService.GetDownloadStatisticsAsync(fromDate, toDate);
            return Ok(stats);
        }

        /// <summary>
        /// Get most popular files
        /// </summary>
        [HttpGet("popular")]
        public async Task<ActionResult<IEnumerable<FilePopularityInfo>>> GetMostPopularFiles([FromQuery] int count = 10)
        {
            var popularFiles = await _analyticsService.GetMostPopularFilesAsync(count);
            return Ok(popularFiles);
        }

        /// <summary>
        /// Get daily download statistics
        /// </summary>
        [HttpGet("daily")]
        public async Task<ActionResult<IEnumerable<DailyDownloadStats>>> GetDailyStats([FromQuery] int days = 30)
        {
            var dailyStats = await _analyticsService.GetDailyStatsAsync(days);
            return Ok(dailyStats);
        }

        /// <summary>
        /// Get download history for a specific file
        /// </summary>
        [HttpGet("history/{fileId}")]
        public async Task<ActionResult<IEnumerable<DownloadAnalytics>>> GetDownloadHistory(string fileId)
        {
            var history = await _analyticsService.GetDownloadHistoryAsync(fileId);
            return Ok(history);
        }

        /// <summary>
        /// Cleanup old analytics data
        /// </summary>
        [HttpDelete("cleanup")]
        public async Task<IActionResult> CleanupOldAnalytics([FromQuery] int daysToKeep = 365)
        {
            await _analyticsService.CleanupOldAnalyticsAsync(daysToKeep);
            return Ok(new { message = $"Analytics data older than {daysToKeep} days has been cleaned up." });
        }

        /// <summary>
        /// Get analytics dashboard summary
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<ActionResult<object>> GetDashboard()
        {
            var stats = await _analyticsService.GetDownloadStatisticsAsync();
            var popularFiles = await _analyticsService.GetMostPopularFilesAsync(5);
            var recentStats = await _analyticsService.GetDailyStatsAsync(7);

            return Ok(new
            {
                overview = new
                {
                    totalDownloads = stats.TotalDownloads,
                    totalFiles = stats.TotalFiles,
                    totalBytesDownloaded = stats.TotalBytesDownloaded,
                    averageDownloadsPerDay = stats.AverageDownloadsPerDay,
                    lastDownloadDate = stats.LastDownloadDate
                },
                popularFiles,
                recentActivity = recentStats
            });
        }
    }
}
