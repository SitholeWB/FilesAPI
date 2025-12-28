namespace Services;

/// <summary>
/// SQL implementation for download analytics repository
/// </summary>
public class SqlDbDownloadAnalyticsRepository : IDownloadAnalyticsRepository
{
    private readonly FilesDbContext _filesDbContext;

    public SqlDbDownloadAnalyticsRepository(FilesDbContext filesDbContext)
    {
        _filesDbContext = filesDbContext;
    }

    public async Task<DownloadAnalytics> AddDownloadRecordAsync(DownloadAnalytics analytics, CancellationToken token)
    {
        if (string.IsNullOrEmpty(analytics.Id))
        {
            analytics.Id = ObjectId.NewObjectId().ToString();
        }
        await _filesDbContext.AddAsync(analytics, token);
        await _filesDbContext.SaveChangesAsync(token);

        return analytics;
    }

    public async Task<IEnumerable<DownloadAnalytics>> GetDownloadHistoryAsync(string fileId, CancellationToken token)
    {
        return await _filesDbContext.DownloadAnalytics.Where(x => x.FileId == fileId).OrderByDescending(x => x.DownloadDate).ToListAsync(token);
    }

    public async Task<IEnumerable<DownloadAnalytics>> GetDownloadHistoryAsync(DateTime fromDate, DateTime toDate, CancellationToken token)
    {
        return await _filesDbContext.DownloadAnalytics.Where(x => x.DownloadDate >= fromDate && x.DownloadDate <= toDate).OrderByDescending(x => x.DownloadDate).ToListAsync(token);
    }

    public async Task<DownloadStatistics> GetDownloadStatisticsAsync(CancellationToken token)
    {
        return await GetDownloadStatisticsAsync(DateTime.MinValue, DateTime.MaxValue, token);
    }

    public async Task<DownloadStatistics> GetDownloadStatisticsAsync(DateTime fromDate, DateTime toDate, CancellationToken token)
    {
        var downloads = await GetDownloadHistoryAsync(fromDate, toDate, token);

        if (!downloads.Any())
        {
            return new DownloadStatistics
            {
                MostPopularFiles = new List<FilePopularityInfo>(),
                DailyStats = new List<DailyDownloadStats>()
            };
        }

        var totalDownloads = downloads.Count();
        var totalBytes = downloads.Sum(d => d.FileSize);
        var firstDownload = downloads.Min(d => d.DownloadDate);
        var lastDownload = downloads.Max(d => d.DownloadDate);
        var daysDiff = Math.Max(1, (lastDownload - firstDownload).TotalDays);

        // Most popular files
        var popularFiles = downloads
            .GroupBy(d => d.FileId)
            .Select(g => new FilePopularityInfo
            {
                FileId = g.Key,
                FileName = g.First().FileName,
                DownloadCount = g.Count(),
                FileSize = g.First().FileSize,
                LastDownloaded = g.Max(x => x.DownloadDate),
                ContentType = "application/octet-stream"
            })
            .OrderByDescending(f => f.DownloadCount)
            .Take(10)
            .ToList();

        // Daily stats
        var dailyStats = downloads
            .GroupBy(d => d.DownloadDate.Date)
            .Select(g => new DailyDownloadStats
            {
                Date = g.Key,
                DownloadCount = g.Count(),
                BytesDownloaded = g.Sum(x => x.FileSize),
                UniqueFiles = g.Select(x => x.FileId).Distinct().Count()
            })
            .OrderBy(s => s.Date)
            .ToList();

        return new DownloadStatistics
        {
            TotalDownloads = totalDownloads,
            TotalFiles = downloads.Select(d => d.FileId).Distinct().Count(),
            TotalBytesDownloaded = totalBytes,
            FirstDownloadDate = firstDownload,
            LastDownloadDate = lastDownload,
            AverageDownloadsPerDay = totalDownloads / daysDiff,
            MostPopularFiles = popularFiles,
            DailyStats = dailyStats
        };
    }

    public async Task<IEnumerable<FilePopularityInfo>> GetMostPopularFilesAsync(int count = 10, CancellationToken token = default)
    {
        var downloads = await _filesDbContext.DownloadAnalytics.ToListAsync(token);
        return downloads
            .GroupBy(d => d.FileId)
            .Select(g => new FilePopularityInfo
            {
                FileId = g.Key,
                FileName = g.First().FileName,
                DownloadCount = g.Count(),
                FileSize = g.First().FileSize,
                LastDownloaded = g.Max(x => x.DownloadDate)
            })
            .OrderByDescending(f => f.DownloadCount)
            .Take(count)
            .ToList();
    }

    public async Task<IEnumerable<DailyDownloadStats>> GetDailyStatsAsync(DateTime fromDate, DateTime toDate, CancellationToken token)
    {
        var downloads = await _filesDbContext.DownloadAnalytics.Where(x => x.DownloadDate >= fromDate && x.DownloadDate <= toDate).ToListAsync(token);
        return downloads
            .GroupBy(d => d.DownloadDate.Date)
            .Select(g => new DailyDownloadStats
            {
                Date = g.Key,
                DownloadCount = g.Count(),
                BytesDownloaded = g.Sum(x => x.FileSize),
                UniqueFiles = g.Select(x => x.FileId).Distinct().Count()
            })
            .OrderBy(s => s.Date)
            .ToList();
    }

    public async Task<long> GetTotalDownloadsAsync(CancellationToken token)
    {
        return await _filesDbContext.DownloadAnalytics.CountAsync(token);
    }

    public async Task<long> GetTotalBytesDownloadedAsync(CancellationToken token)
    {
        var downloads = await _filesDbContext.DownloadAnalytics.ToListAsync(token);
        return downloads.Sum(d => d.FileSize);
    }

    public async Task DeleteOldAnalyticsAsync(DateTime olderThan, CancellationToken token)
    {
        var downloads = await _filesDbContext.DownloadAnalytics.Where(x => x.DownloadDate < olderThan).ToListAsync(token);
        _filesDbContext.RemoveRange(downloads);
        await _filesDbContext.SaveChangesAsync(token);
    }
}