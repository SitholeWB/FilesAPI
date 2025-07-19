# FilesAPI Deployment Guide

## Self-Contained Docker Deployment (Recommended)

The FilesAPI now supports **fully self-contained deployment** with zero external dependencies.

### Quick Start

**Linux/Mac:**
```bash
./run-standalone.sh
```

**Windows:**
```cmd
run-standalone.bat
```

### What You Get

- **Zero Dependencies**: No MongoDB installation required
- **Embedded Database**: Uses LiteDB for data storage
- **Single Container**: Everything runs in one container
- **Portable**: Works on any machine with Docker
- **Persistent Storage**: Data survives container restarts
- **Production Ready**: Optimized for production use

### Access Points

- **API Endpoints**: http://localhost:5100/api/storage
- **📊 Analytics Dashboard**: http://localhost:5100/analytics.html
- **Analytics API**: http://localhost:5100/api/analytics/dashboard
- **Health Check**: http://localhost:5100/health
- **File Upload**: POST to http://localhost:5100/api/storage
- **File Download**: GET http://localhost:5100/api/storage/{id}

### Manual Commands

```bash
# Start the standalone version
docker-compose -f docker-compose.standalone.yml up -d

# Check status
docker ps

# View logs
docker logs filesapi-standalone

# Stop the service
docker-compose -f docker-compose.standalone.yml down

# Reset all data (removes volumes)
docker-compose -f docker-compose.standalone.yml down -v
```

### File Upload Example

```bash
# Upload a file
curl -X POST "http://localhost:5100/api/storage" \
  -H "Content-Type: multipart/form-data" \
  -F "file=@/path/to/your/file.txt" \
  -F "description=Test file"

# List all files
curl http://localhost:5100/api/storage

# Download a file (replace {id} with actual file ID)
curl http://localhost:5100/api/storage/{id}/download
```

### Data Persistence

All data is stored in Docker volumes:

- **Database**: `filesapi_data` - Contains the LiteDB database
- **Uploads**: `filesapi_uploads` - Contains uploaded files
- **Logs**: `filesapi_logs` - Contains application logs

### Backup and Restore

```bash
# Backup data
docker run --rm -v filesapi_data:/data -v $(pwd):/backup alpine tar czf /backup/filesapi-backup.tar.gz /data

# Restore data
docker run --rm -v filesapi_data:/data -v $(pwd):/backup alpine tar xzf /backup/filesapi-backup.tar.gz -C /
```

## Traditional MongoDB Deployment

For environments requiring MongoDB, use the standard docker-compose:

```bash
docker-compose up -d
```

This provides:
- FilesAPI on port 5100
- MongoDB on port 27017
- MongoDB Express on port 8081

## Key Features

### Unlimited File Uploads
- No file size restrictions
- Works with both Kestrel and IIS hosting
- Optimized for large file handling

### Database Flexibility
- **Embedded Mode**: Uses LiteDB (no external database required)
- **MongoDB Mode**: Traditional MongoDB integration with GridFS
- **Auto-Detection**: Switches based on environment variables

### 📊 Analytics & Monitoring
- **Real-time Download Tracking**: Automatic tracking of all file downloads and views
- **Analytics Dashboard**: Beautiful web interface with comprehensive metrics
- **Download Statistics**: Total downloads, popular files, daily trends
- **User Analytics**: Track user agents, IP addresses, referrers
- **REST API**: Full analytics API for integration with external systems
- **Data Retention**: Configurable cleanup of old analytics data
- **Database Agnostic**: Works with both LiteDB and MongoDB backends

#### Analytics Environment Variables
```bash
ANALYTICS_ENABLED=true              # Enable/disable analytics (default: true)
ANALYTICS_RETENTION_DAYS=365        # Days to keep analytics data (default: 365)
```

#### Analytics Endpoints
- `GET /analytics.html` - Interactive dashboard
- `GET /api/analytics/dashboard` - Dashboard data API
- `GET /api/analytics/statistics` - Overall statistics
- `GET /api/analytics/popular?count=10` - Most popular files
- `GET /api/analytics/daily?days=30` - Daily download stats
- `GET /api/analytics/history/{fileId}` - File download history
- `DELETE /api/analytics/cleanup?daysToKeep=365` - Cleanup old data

### Production Ready
- Health checks included
- Proper logging configuration
- Security best practices (non-root user)
- Resource optimization

### Cross-Platform
- Works on Windows, Linux, and macOS
- ARM64 and x86_64 support
- Container-based deployment

## Troubleshooting

### Container Won't Start
```bash
# Check logs
docker logs filesapi-standalone

# Rebuild container
docker-compose -f docker-compose.standalone.yml up -d --build
```

### Port Already in Use
```bash
# Change port in docker-compose.standalone.yml
ports:
  - "5101:8080"  # Change 5100 to 5101
```

### Reset Everything
```bash
# Stop and remove everything
docker-compose -f docker-compose.standalone.yml down -v
docker system prune -f

# Start fresh
./run-standalone.sh
```

## Migration from Previous Versions

The new self-contained version is backward compatible. Existing MongoDB data can be migrated to LiteDB if needed, or you can continue using MongoDB by setting `USE_EMBEDDED_DATABASE=false`.

## Performance Considerations

- **LiteDB**: Best for small to medium workloads (< 100GB)
- **MongoDB**: Better for large-scale deployments
- **File Storage**: Uses efficient streaming for large files
- **Memory Usage**: Optimized for minimal memory footprint

The self-contained deployment makes FilesAPI truly portable and eliminates the complexity of managing external database dependencies!
