using Contracts;
using Microsoft.AspNetCore.Mvc;
using Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FilesAPI.Controllers;

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
    public async Task<ActionResult<DownloadStatistics>> GetDownloadStatistics(CancellationToken token = default)
    {
        var stats = await _analyticsService.GetDownloadStatisticsAsync(token);
        return Ok(stats);
    }

    /// <summary>
    /// Get download statistics for a specific date range
    /// </summary>
    [HttpGet("statistics/{fromDate}/{toDate}")]
    public async Task<ActionResult<DownloadStatistics>> GetDownloadStatistics(DateTime fromDate, DateTime toDate, CancellationToken token = default)
    {
        var stats = await _analyticsService.GetDownloadStatisticsAsync(fromDate, toDate, token);
        return Ok(stats);
    }

    /// <summary>
    /// Get most popular files
    /// </summary>
    [HttpGet("popular")]
    public async Task<ActionResult<IEnumerable<FilePopularityInfo>>> GetMostPopularFiles([FromQuery] int count = 10, CancellationToken token = default)
    {
        var popularFiles = await _analyticsService.GetMostPopularFilesAsync(count, token);
        return Ok(popularFiles);
    }

    /// <summary>
    /// Get daily download statistics
    /// </summary>
    [HttpGet("daily")]
    public async Task<ActionResult<IEnumerable<DailyDownloadStats>>> GetDailyStats([FromQuery] int days = 30, CancellationToken token = default)
    {
        var dailyStats = await _analyticsService.GetDailyStatsAsync(days, token);
        return Ok(dailyStats);
    }

    /// <summary>
    /// Get download history for a specific file
    /// </summary>
    [HttpGet("history/{fileId}")]
    public async Task<ActionResult<IEnumerable<DownloadAnalytics>>> GetDownloadHistory(string fileId, CancellationToken token = default)
    {
        var history = await _analyticsService.GetDownloadHistoryAsync(fileId, token);
        return Ok(history);
    }

    /// <summary>
    /// Cleanup old analytics data
    /// </summary>
    [HttpDelete("cleanup")]
    public async Task<IActionResult> CleanupOldAnalytics([FromQuery] int daysToKeep = 365, CancellationToken token = default)
    {
        await _analyticsService.CleanupOldAnalyticsAsync(daysToKeep, token);
        return Ok(new { message = $"Analytics data older than {daysToKeep} days has been cleaned up." });
    }

    /// <summary>
    /// Get analytics dashboard summary
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<object>> GetDashboard(CancellationToken token = default)
    {
        var stats = await _analyticsService.GetDownloadStatisticsAsync(token);
        var popularFiles = await _analyticsService.GetMostPopularFilesAsync(5, token);
        var recentStats = await _analyticsService.GetDailyStatsAsync(7, token);

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