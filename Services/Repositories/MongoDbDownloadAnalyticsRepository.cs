using Microsoft.Extensions.Options;

namespace Services;

/// <summary>
/// MongoDB implementation for download analytics repository
/// </summary>
public class MongoDbDownloadAnalyticsRepository : IDownloadAnalyticsRepository
{
    private const string _databaseName = "FilesAPI";
    private readonly IMongoCollection<DownloadAnalytics> _collection;

    public MongoDbDownloadAnalyticsRepository(IOptions<MongoDBAppSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(_databaseName);
        _collection = database.GetCollection<DownloadAnalytics>("downloadanalytics");

        // Create indexes for better performance
        CreateIndexes();
    }

    private void CreateIndexes(CancellationToken token = default)
    {
        var indexKeysDefinition = Builders<DownloadAnalytics>.IndexKeys
            .Ascending(x => x.FileId)
            .Ascending(x => x.DownloadDate);
        _collection.Indexes.CreateOne(new CreateIndexModel<DownloadAnalytics>(indexKeysDefinition));

        var dateIndexKeysDefinition = Builders<DownloadAnalytics>.IndexKeys.Ascending(x => x.DownloadDate);
        _collection.Indexes.CreateOne(new CreateIndexModel<DownloadAnalytics>(dateIndexKeysDefinition));
    }

    public async Task<DownloadAnalytics> AddDownloadRecordAsync(DownloadAnalytics analytics, CancellationToken token)
    {
        await _collection.InsertOneAsync(analytics, options: default, token);
        return analytics;
    }

    public async Task<IEnumerable<DownloadAnalytics>> GetDownloadHistoryAsync(string fileId, CancellationToken token)
    {
        var filter = Builders<DownloadAnalytics>.Filter.Eq(x => x.FileId, fileId);
        var sort = Builders<DownloadAnalytics>.Sort.Descending(x => x.DownloadDate);

        var cursor = await _collection.FindAsync(filter, new FindOptions<DownloadAnalytics>
        {
            Sort = sort
        }, token);

        return await cursor.ToListAsync(token);
    }

    public async Task<IEnumerable<DownloadAnalytics>> GetDownloadHistoryAsync(DateTime fromDate, DateTime toDate, CancellationToken token)
    {
        var filter = Builders<DownloadAnalytics>.Filter.And(
            Builders<DownloadAnalytics>.Filter.Gte(x => x.DownloadDate, fromDate),
            Builders<DownloadAnalytics>.Filter.Lte(x => x.DownloadDate, toDate)
        );
        var sort = Builders<DownloadAnalytics>.Sort.Descending(x => x.DownloadDate);

        var cursor = await _collection.FindAsync(filter, new FindOptions<DownloadAnalytics>
        {
            Sort = sort
        }, token);

        return await cursor.ToListAsync(token);
    }

    public async Task<DownloadStatistics> GetDownloadStatisticsAsync(CancellationToken token)
    {
        return await GetDownloadStatisticsAsync(DateTime.MinValue, DateTime.MaxValue, token);
    }

    public async Task<DownloadStatistics> GetDownloadStatisticsAsync(DateTime fromDate, DateTime toDate, CancellationToken token)
    {
        var filter = Builders<DownloadAnalytics>.Filter.And(
            Builders<DownloadAnalytics>.Filter.Gte(x => x.DownloadDate, fromDate),
            Builders<DownloadAnalytics>.Filter.Lte(x => x.DownloadDate, toDate)
        );

        var downloads = await (await _collection.FindAsync(filter, cancellationToken: token)).ToListAsync(token);

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
        var downloads = await (await _collection.FindAsync(_ => true, cancellationToken: token)).ToListAsync(token);

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
        var filter = Builders<DownloadAnalytics>.Filter.And(
            Builders<DownloadAnalytics>.Filter.Gte(x => x.DownloadDate, fromDate),
            Builders<DownloadAnalytics>.Filter.Lte(x => x.DownloadDate, toDate)
        );

        var downloads = await (await _collection.FindAsync(filter)).ToListAsync(token);

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
        return await _collection.CountDocumentsAsync(_ => true, cancellationToken: token);
    }

    public async Task<long> GetTotalBytesDownloadedAsync(CancellationToken token)
    {
        var downloads = await (await _collection.FindAsync(_ => true, cancellationToken: token)).ToListAsync(token);
        return downloads.Sum(d => d.FileSize);
    }

    public async Task DeleteOldAnalyticsAsync(DateTime olderThan, CancellationToken token)
    {
        var filter = Builders<DownloadAnalytics>.Filter.Lt(x => x.DownloadDate, olderThan);
        await _collection.DeleteManyAsync(filter, token);
    }
}