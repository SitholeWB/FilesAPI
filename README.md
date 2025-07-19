<div align="center">

# 🗂️ FilesAPI

**Enterprise-grade file storage and management API with comprehensive analytics**

[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![Docker](https://img.shields.io/badge/Docker-Supported-blue.svg)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Build Status](https://img.shields.io/badge/Build-Passing-brightgreen.svg)](#)

*A modern, scalable file storage microservice with real-time analytics, unlimited file uploads, and dual database support*

[🚀 Quick Start](#-quick-start) • [📊 Analytics](#-analytics--insights) • [🐳 Docker](#-docker-deployment) • [📚 Documentation](#-documentation)

</div>

---

## ✨ Key Features

### 🎯 **Core Functionality**
- **Unlimited File Uploads** - No size restrictions, optimized for large files
- **Universal File Support** - Store any file type with metadata
- **RESTful API** - Clean, intuitive endpoints with OpenAPI/Swagger documentation
- **File Management** - Upload, download, view, update, and delete operations
- **Tagging System** - Organize files with custom tags and descriptions

### 📊 **Advanced Analytics**
- **Real-time Download Tracking** - Automatic monitoring of all file operations
- **Interactive Dashboard** - Beautiful web interface with live statistics
- **Usage Insights** - Popular files, download trends, user behavior analytics
- **Comprehensive Metrics** - Total downloads, data transfer, daily patterns
- **Historical Data** - Detailed download history with user agent and IP tracking

### 🏗️ **Architecture & Deployment**
- **Dual Database Support** - MongoDB for scale, LiteDB for simplicity
- **Docker Ready** - Self-contained deployment or traditional MongoDB setup
- **Production Optimized** - Health checks, logging, security best practices
- **Microservice Architecture** - Clean separation of concerns, testable codebase

---

## 🚀 Quick Start

### Option 1: Docker Standalone ⭐ *Recommended*

**Zero configuration, fully self-contained deployment:**

```bash
# Linux/Mac
./run-standalone.sh

# Windows
run-standalone.bat
```

**✅ What you get:**
- 🎯 **Zero Dependencies** - No MongoDB installation required
- 📦 **Single Container** - Everything runs in one container
- 💾 **Embedded Database** - Uses LiteDB for data storage
- 🔄 **Persistent Storage** - Data survives container restarts
- 🌐 **Instant Access** - API available at `http://localhost:5100`

### Option 2: Docker with MongoDB

**Traditional setup with MongoDB and MongoDB Express:**

```bash
docker-compose up -d
```

**Access Points:**
- 🌐 **API**: `http://localhost:5100`
- 📊 **Analytics**: `http://localhost:5100/analytics.html`
- 🗄️ **MongoDB Express**: `http://localhost:8081`

### Option 3: Local Development

```bash
# Clone and setup
git clone <repository-url>
cd FilesAPI_9-master

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run with LiteDB (recommended for development)
USE_EMBEDDED_DATABASE=true dotnet run --project FilesAPI

# Or run with MongoDB (requires MongoDB running)
dotnet run --project FilesAPI
```

---

## 📊 Analytics & Insights

### 🎨 Interactive Dashboard

Access the **beautiful analytics dashboard** at: [`http://localhost:5100/analytics.html`](http://localhost:5100/analytics.html)

**Dashboard Features:**
- 📈 **Overview Cards** - Total downloads, files, data transferred
- 🔥 **Popular Files** - Most downloaded files with metrics
- 📅 **Activity Trends** - Daily download patterns and insights
- 🔄 **Real-time Updates** - Live data with refresh functionality
- 📱 **Responsive Design** - Works perfectly on all devices

### 🔌 Analytics API Endpoints

```http
GET  /api/analytics/dashboard           # Complete dashboard data
GET  /api/analytics/statistics          # Overall download statistics  
GET  /api/analytics/popular?count=10    # Most popular files
GET  /api/analytics/daily?days=30       # Daily download statistics
GET  /api/analytics/history/{fileId}    # Download history for file
DELETE /api/analytics/cleanup?daysToKeep=365  # Cleanup old data
```

### 📊 What Gets Tracked

- **Download Metrics** - Total downloads, file popularity, data transfer
- **User Behavior** - User agents, IP addresses, referrer tracking
- **Temporal Patterns** - Daily, weekly, monthly download trends
- **File Analytics** - Most popular files, download frequency
- **Performance Data** - Download completion rates, timing data

---

## 🐳 Docker Deployment

### 🎯 Self-Contained Deployment

**Perfect for production with zero external dependencies:**

```yaml
# docker-compose.standalone.yml
version: '3.8'
services:
  filesapi:
    build: .
    ports:
      - "5100:8080"
    environment:
      - USE_EMBEDDED_DATABASE=true
      - ANALYTICS_ENABLED=true
    volumes:
      - filesapi_data:/app/data
      - filesapi_uploads:/app/uploads
```

### 🏢 Enterprise Deployment

**Scalable setup with MongoDB:**

```yaml
# docker-compose.yml
version: '3.8'
services:
  filesapi:
    build: .
    ports:
      - "5100:8080"
    environment:
      - ANALYTICS_ENABLED=true
    depends_on:
      - mongodb
  
  mongodb:
    image: mongo:7.0
    ports:
      - "27017:27017"
```

---

## 🛠️ Configuration

### Environment Variables

```bash
# Database Configuration
USE_EMBEDDED_DATABASE=true              # Use LiteDB (true) or MongoDB (false)
DATABASE_PATH=/app/data/filesapi.db     # LiteDB database path
UPLOADS_PATH=/app/uploads               # File storage path

# Analytics Configuration  
ANALYTICS_ENABLED=true                  # Enable/disable analytics
ANALYTICS_RETENTION_DAYS=365            # Days to keep analytics data

# Application Configuration
ASPNETCORE_ENVIRONMENT=Production       # Environment mode
ASPNETCORE_URLS=http://+:8080          # Binding URLs
```

### Database Options

| Database | Use Case | Pros | Cons |
|----------|----------|------|------|
| **LiteDB** | Development, Small-Medium Scale | Zero setup, Self-contained, Fast | Single file, Limited concurrency |
| **MongoDB** | Production, Enterprise Scale | High performance, Scalable, GridFS | Requires setup, External dependency |

---

## 📚 API Documentation

### 🔗 Core Endpoints

```http
# File Operations
POST   /api/storage                    # Upload file
GET    /api/storage/{id}/download      # Download file
GET    /api/storage/{id}/view          # View file in browser
GET    /api/storage                    # List all files
GET    /api/storage/details/{id}       # Get file details
PUT    /api/storage/details/{id}       # Update file details
DELETE /api/storage/{id}               # Delete file
GET    /api/storage/details/tags/{tag} # Get files by tag

# System Endpoints
GET    /health                         # Health check
GET    /swagger                        # API documentation
GET    /analytics.html                 # Analytics dashboard
```

### 📋 Upload Example

```bash
# Upload file with metadata
curl -X POST \
  -F "File=@document.pdf" \
  -F "Description=Important document" \
  -F "Tags=document,important" \
  http://localhost:5100/api/storage
```

### 📥 Download Example

```bash
# Download file
curl "http://localhost:5100/api/storage/{fileId}/download" \
  -o downloaded-file.pdf

# View file in browser
curl "http://localhost:5100/api/storage/{fileId}/view"
```

---

## 🏗️ Architecture

### 📁 Project Structure

```
FilesAPI_9-master/
├── 📁 FilesAPI/              # Main web API project
│   ├── Controllers/          # API controllers
│   ├── wwwroot/             # Static files (analytics dashboard)
│   └── Program.cs           # Application entry point
├── 📁 Services/             # Business logic layer
│   ├── Repositories/        # Data access implementations
│   └── Events/              # Event handling system
├── 📁 Models/               # Data models and DTOs
├── 📁 Contracts/            # Interfaces and contracts
├── 📁 Services.Tests/       # Unit tests (NUnit)
├── 🐳 Dockerfile           # Container configuration
├── 🐳 docker-compose.yml   # Multi-container setup
└── 📚 Documentation/        # Additional docs
```

### 🔧 Technology Stack

- **Framework**: .NET 9.0
- **Databases**: MongoDB 7.0, LiteDB 5.0.21
- **API Documentation**: Swashbuckle.AspNetCore 9.0.3
- **Testing**: NUnit 4.2.2
- **Containerization**: Docker & Docker Compose
- **Frontend**: Vanilla JavaScript, CSS3, HTML5

---

## ✅ Production Ready

### 🔒 Security Features
- **Non-root Container** - Runs as dedicated `appuser`
- **Input Validation** - Comprehensive request validation
- **Error Handling** - Graceful error responses
- **CORS Support** - Configurable cross-origin requests

### 📊 Monitoring & Observability
- **Health Checks** - `/health` endpoint with database status
- **Structured Logging** - Comprehensive application logging
- **Metrics Ready** - Analytics data for monitoring systems
- **Docker Health Checks** - Container health monitoring

### 🚀 Performance Optimizations
- **Unlimited File Uploads** - No size restrictions
- **Streaming Support** - Efficient large file handling
- **Database Indexes** - Optimized queries for MongoDB
- **Non-blocking Analytics** - Zero impact on file operations

---

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Build verification
dotnet build --configuration Release
```

**Test Coverage:**
- ✅ Unit Tests: 6/6 passing
- ✅ Integration Tests: API endpoints
- ✅ Repository Tests: Database operations
- ✅ Service Tests: Business logic

---

## 📈 Changelog & Resolved Issues

### 🎯 Major Enhancements

| Feature | Status | Description |
|---------|--------|--------------|
| 📊 **Analytics System** | ✅ Complete | Real-time download tracking with interactive dashboard |
| 🚫 **Unlimited Uploads** | ✅ Complete | Removed all file size limitations |
| 🐳 **Docker Support** | ✅ Complete | Self-contained and traditional deployments |
| 💾 **LiteDB Integration** | ✅ Complete | Embedded database with full feature parity |
| 🏥 **Health Monitoring** | ✅ Complete | Comprehensive health checks |
| 🔧 **Environment Config** | ✅ Complete | Flexible database backend switching |

### 🐛 Resolved Issues

- ✅ **[Issue #1]** File size limitation on upload
- ✅ **[Issue #3]** Docker support implementation  
- ✅ **[PR #8]** Incomplete LiteDB implementation
- ✅ **MongoDB 3.4.1** Compatibility and GridFS integration
- ✅ **NUnit 4.x** Migration and test framework updates
- ✅ **.NET 9.0** Framework upgrade and optimization

---

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

### 🔧 Development Setup

1. **Fork** the repository
2. **Clone** your fork: `git clone <your-fork-url>`
3. **Create** a feature branch: `git checkout -b feature/amazing-feature`
4. **Make** your changes and add tests
5. **Test** your changes: `dotnet test`
6. **Commit** your changes: `git commit -m 'Add amazing feature'`
7. **Push** to your branch: `git push origin feature/amazing-feature`
8. **Create** a Pull Request

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🆘 Support

- 📚 **Documentation**: [DEPLOYMENT.md](DEPLOYMENT.md) | [ANALYTICS.md](ANALYTICS.md)
- 🐛 **Issues**: [GitHub Issues](https://github.com/SitholeWB/FilesAPI/issues)
- 💬 **Discussions**: [GitHub Discussions](https://github.com/SitholeWB/FilesAPI/discussions)

---

<div align="center">

**⭐ Star this repository if you find it useful!**

*Built with ❤️ using .NET 9.0*

</div>
