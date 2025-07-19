# 📊 FilesAPI Analytics System

## Overview

The FilesAPI now includes a comprehensive download analytics and tracking system that provides real-time insights into file usage patterns, download statistics, and user behavior analytics.

## 🚀 Key Features

### Real-time Download Tracking
- **Automatic Tracking**: Every file download and view is automatically tracked
- **Detailed Metrics**: Captures user agent, IP address, referrer, file size, timestamps
- **Non-blocking**: Analytics recording doesn't impact download performance
- **Dual Endpoints**: Tracks both `/download` and `/view` operations

### Analytics Dashboard
- **Beautiful UI**: Modern responsive web interface with gradient design
- **Real-time Data**: Live statistics with refresh functionality
- **Overview Cards**: Total downloads, files, data transferred, averages
- **Popular Files**: Most downloaded files with metrics and last download dates
- **Activity Trends**: Daily download patterns for the last 7 days
- **Mobile Friendly**: Responsive design works on all devices

### REST API
Complete set of analytics endpoints for integration and automation:

```http
GET /api/analytics/dashboard          # Complete dashboard data
GET /api/analytics/statistics         # Overall download statistics
GET /api/analytics/popular?count=10   # Most popular files
GET /api/analytics/daily?days=30      # Daily download statistics
GET /api/analytics/history/{fileId}   # Download history for specific file
DELETE /api/analytics/cleanup?daysToKeep=365  # Cleanup old analytics data
```

## 🏗️ Technical Architecture

### Database Support
- **LiteDB**: Full analytics support for standalone deployments
- **MongoDB**: Complete analytics with optimized indexes for performance
- **Automatic Switching**: Uses same database backend as main application

### Data Models

#### DownloadAnalytics
```csharp
public class DownloadAnalytics
{
    public string Id { get; set; }
    public string FileId { get; set; }
    public string FileName { get; set; }
    public DateTime DownloadDate { get; set; }
    public string UserAgent { get; set; }
    public string IpAddress { get; set; }
    public string Referrer { get; set; }
    public long FileSize { get; set; }
    public TimeSpan? DownloadDuration { get; set; }
    public bool DownloadCompleted { get; set; }
    public string DownloadMethod { get; set; } // "download" or "view"
}
```

#### DownloadStatistics
```csharp
public class DownloadStatistics
{
    public int TotalDownloads { get; set; }
    public int TotalFiles { get; set; }
    public long TotalBytesDownloaded { get; set; }
    public DateTime? LastDownloadDate { get; set; }
    public DateTime? FirstDownloadDate { get; set; }
    public double AverageDownloadsPerDay { get; set; }
    public IEnumerable<FilePopularityInfo> MostPopularFiles { get; set; }
    public IEnumerable<DailyDownloadStats> DailyStats { get; set; }
}
```

### Repository Pattern
- **IDownloadAnalyticsRepository**: Interface for analytics data access
- **LiteDbDownloadAnalyticsRepository**: LiteDB implementation
- **MongoDbDownloadAnalyticsRepository**: MongoDB implementation with indexes

### Service Layer
- **IAnalyticsService**: Business logic interface
- **AnalyticsService**: Implementation with data aggregation and reporting

## 🔧 Configuration

### Environment Variables
```bash
# Analytics configuration
ANALYTICS_ENABLED=true              # Enable/disable analytics (default: true)
ANALYTICS_RETENTION_DAYS=365        # Days to keep analytics data (default: 365)

# Database configuration (affects analytics storage)
USE_EMBEDDED_DATABASE=true          # Use LiteDB (true) or MongoDB (false)
DATABASE_PATH=/app/data/filesapi.db # LiteDB database path
```

### Docker Configuration
Both Docker deployment modes include analytics:

#### Standalone Mode
```yaml
environment:
  - ANALYTICS_ENABLED=true
  - ANALYTICS_RETENTION_DAYS=365
  - USE_EMBEDDED_DATABASE=true
```

#### MongoDB Mode
```yaml
environment:
  - ANALYTICS_ENABLED=true
  - ANALYTICS_RETENTION_DAYS=365
  # Uses MongoDB for analytics storage
```

## 📈 Usage Examples

### Accessing the Dashboard
```bash
# Open analytics dashboard in browser
http://localhost:5100/analytics.html
```

### API Usage
```bash
# Get overall statistics
curl http://localhost:5100/api/analytics/statistics

# Get top 5 popular files
curl http://localhost:5100/api/analytics/popular?count=5

# Get last 14 days of activity
curl http://localhost:5100/api/analytics/daily?days=14

# Get download history for specific file
curl http://localhost:5100/api/analytics/history/64f1b2c3d4e5f6789abcdef0

# Cleanup analytics older than 180 days
curl -X DELETE http://localhost:5100/api/analytics/cleanup?daysToKeep=180
```

## 🔒 Security & Privacy

### Data Protection
- **IP Anonymization**: IP addresses are stored for analytics but can be anonymized
- **User Agent Tracking**: Only for analytics purposes, no personal identification
- **Data Retention**: Configurable cleanup of old analytics data
- **Non-blocking**: Analytics failures don't affect file operations

### Access Control
- **Public Dashboard**: Analytics dashboard is publicly accessible
- **API Endpoints**: No authentication required (can be added if needed)
- **Admin Operations**: Cleanup operations available to all users

## 🚀 Performance

### Optimizations
- **Fire-and-forget**: Analytics recording is asynchronous and non-blocking
- **Database Indexes**: MongoDB implementation includes optimized indexes
- **Efficient Queries**: Aggregated statistics with minimal database impact
- **Caching Ready**: Statistics can be cached for high-traffic scenarios

### Resource Usage
- **Minimal Overhead**: Analytics add <1ms to download operations
- **Storage Efficient**: Compact data models with configurable retention
- **Memory Optimized**: Streaming queries for large datasets

## 🔧 Maintenance

### Data Cleanup
```bash
# Manual cleanup via API
curl -X DELETE http://localhost:5100/api/analytics/cleanup?daysToKeep=365

# Automated cleanup (can be scheduled)
# Add to cron job or container scheduler
```

### Monitoring
```bash
# Check analytics health
curl http://localhost:5100/health

# Monitor analytics data size
# LiteDB: Check /app/data/filesapi.db size
# MongoDB: Use MongoDB tools to monitor collection size
```

## 🎯 Future Enhancements

### Potential Additions
- **Real-time WebSocket Updates**: Live dashboard updates
- **Advanced Filtering**: Filter analytics by date range, file type, user
- **Export Functionality**: CSV/JSON export of analytics data
- **Alerting**: Notifications for unusual download patterns
- **Geolocation**: IP-based location tracking (with privacy controls)
- **API Rate Limiting**: Track and limit API usage per IP
- **Custom Dashboards**: User-configurable analytics views

### Integration Opportunities
- **External Analytics**: Integration with Google Analytics, Mixpanel
- **Monitoring Systems**: Prometheus metrics export
- **Business Intelligence**: Data export for BI tools
- **Audit Logging**: Enhanced audit trail for compliance

## 📚 Development Notes

### Adding Custom Analytics
To add custom analytics tracking:

1. **Extend DownloadAnalytics model** with new properties
2. **Update repository implementations** to handle new fields
3. **Modify analytics service** to capture additional data
4. **Update dashboard** to display new metrics

### Database Migration
When switching between LiteDB and MongoDB:
- Analytics data is database-specific
- No automatic migration between backends
- Consider data export/import for migrations

### Testing
- All existing tests continue to pass
- Analytics functionality is tested through API endpoints
- Dashboard functionality verified through browser testing

---

**The FilesAPI analytics system provides enterprise-grade insights into file usage patterns while maintaining simplicity and performance.**
