namespace Services;

/// <summary>
/// LiteDB implementation for download analytics repository
/// </summary>
public class LiteDbDownloadAnalyticsRepository : IDownloadAnalyticsRepository
{
    private readonly string _connectionString;
    private readonly string _analyticsCollection = "downloadanalytics";

    public LiteDbDownloadAnalyticsRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<DownloadAnalytics> AddDownloadRecordAsync(DownloadAnalytics analytics, CancellationToken token)
    {
        return await Task.Run(() =>
        {
            using var db = new LiteDatabase(_connectionString);
            var collection = db.GetCollection<DownloadAnalytics>(_analyticsCollection);

            if (string.IsNullOrEmpty(analytics.Id))
            {
                analytics.Id = ObjectId.NewObjectId().ToString();
            }

            collection.Insert(analytics);
            return analytics;
        }, token);
    }

    public async Task<IEnumerable<DownloadAnalytics>> GetDownloadHistoryAsync(string fileId, CancellationToken token)
    {
        return await Task.Run(() =>
        {
            using var db = new LiteDatabase(_connectionString);
            var collection = db.GetCollection<DownloadAnalytics>(_analyticsCollection);
            return collection.Find(Query.EQ("FileId", fileId))
                            .OrderByDescending(x => x.DownloadDate)
                            .ToList();
        }, token);
    }

    public async Task<IEnumerable<DownloadAnalytics>> GetDownloadHistoryAsync(DateTime fromDate, DateTime toDate, CancellationToken token)
    {
        return await Task.Run(() =>
        {
            using var db = new LiteDatabase(_connectionString);
            var collection = db.GetCollection<DownloadAnalytics>(_analyticsCollection);
            return collection.Find(Query.And(
                Query.GTE("DownloadDate", fromDate),
                Query.LTE("DownloadDate", toDate)
            )).OrderByDescending(x => x.DownloadDate).ToList();
        }, token);
    }

    public async Task<DownloadStatistics> GetDownloadStatisticsAsync(CancellationToken token)
    {
        return await GetDownloadStatisticsAsync(DateTime.MinValue, DateTime.MaxValue, token);
    }

    public async Task<DownloadStatistics> GetDownloadStatisticsAsync(DateTime fromDate, DateTime toDate, CancellationToken token)
    {
        return await Task.Run(() =>
        {
            using var db = new LiteDatabase(_connectionString);
            var collection = db.GetCollection<DownloadAnalytics>(_analyticsCollection);

            var downloads = collection.Find(Query.And(
                Query.GTE("DownloadDate", fromDate),
                Query.LTE("DownloadDate", toDate)
            )).ToList();

            if (!downloads.Any())
            {
                return new DownloadStatistics
                {
                    MostPopularFiles = new List<FilePopularityInfo>(),
                    DailyStats = new List<DailyDownloadStats>()
                };
            }

            var totalDownloads = downloads.Count;
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
                    ContentType = "application/octet-stream" // Default, could be enhanced
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
        }, token);
    }

    public async Task<IEnumerable<FilePopularityInfo>> GetMostPopularFilesAsync(int count = 10, CancellationToken token = default)
    {
        return await Task.Run(() =>
        {
            using var db = new LiteDatabase(_connectionString);
            var collection = db.GetCollection<DownloadAnalytics>(_analyticsCollection);

            return collection.FindAll()
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
        }, token);
    }

    public async Task<IEnumerable<DailyDownloadStats>> GetDailyStatsAsync(DateTime fromDate, DateTime toDate, CancellationToken token)
    {
        return await Task.Run(() =>
        {
            using var db = new LiteDatabase(_connectionString);
            var collection = db.GetCollection<DownloadAnalytics>(_analyticsCollection);

            return collection.Find(Query.And(
                Query.GTE("DownloadDate", fromDate),
                Query.LTE("DownloadDate", toDate)
            ))
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
        }, token);
    }

    public async Task<long> GetTotalDownloadsAsync(CancellationToken token)
    {
        return await Task.Run(() =>
        {
            using var db = new LiteDatabase(_connectionString);
            var collection = db.GetCollection<DownloadAnalytics>(_analyticsCollection);
            return collection.Count();
        }, token);
    }

    public async Task<long> GetTotalBytesDownloadedAsync(CancellationToken token)
    {
        return await Task.Run(() =>
        {
            using var db = new LiteDatabase(_connectionString);
            var collection = db.GetCollection<DownloadAnalytics>(_analyticsCollection);
            return collection.FindAll().Sum(d => d.FileSize);
        }, token);
    }

    public async Task DeleteOldAnalyticsAsync(DateTime olderThan, CancellationToken token)
    {
        await Task.Run(() =>
        {
            using var db = new LiteDatabase(_connectionString);
            var collection = db.GetCollection<DownloadAnalytics>(_analyticsCollection);
            collection.DeleteMany(Query.LT("DownloadDate", olderThan));
        }, token);
    }
}