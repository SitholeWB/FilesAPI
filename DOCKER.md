# Docker Support for FilesAPI

This document provides comprehensive instructions for running FilesAPI using Docker containers.

## Quick Start

### Prerequisites
- Docker Desktop or Docker Engine installed
- Docker Compose installed

### Run with Docker Compose (Recommended)

1. **Clone the repository and navigate to the project directory**
   ```bash
   git clone <repository-url>
   cd FilesAPI_9-master
   ```

2. **Start all services**
   ```bash
   docker-compose up -d
   ```

3. **Access the application**
   - **FilesAPI**: http://localhost:5100
   - **Swagger UI**: http://localhost:5100/swagger
   - **MongoDB Express** (Database UI): http://localhost:8081

4. **Stop all services**
   ```bash
   docker-compose down
   ```

## Services Overview

### FilesAPI Web Service
- **Container**: `filesapi-web`
- **Port**: 5100 (mapped to container port 8080)
- **Environment**: Production
- **File Storage**: Persistent volume mounted at `/app/uploads`

### MongoDB Database
- **Container**: `filesapi-mongodb`
- **Port**: 27017
- **Username**: `admin`
- **Password**: `password123`
- **Database**: `filesapi`
- **Data Persistence**: Named volume `mongodb_data`

### MongoDB Express (Optional)
- **Container**: `filesapi-mongo-express`
- **Port**: 8081
- **Purpose**: Web-based MongoDB administration interface

## Manual Docker Commands

### Build the FilesAPI Image
```bash
docker build -t filesapi:latest .
```

### Run MongoDB Container
```bash
docker run -d \
  --name filesapi-mongodb \
  -p 27017:27017 \
  -e MONGO_INITDB_ROOT_USERNAME=admin \
  -e MONGO_INITDB_ROOT_PASSWORD=password123 \
  -e MONGO_INITDB_DATABASE=filesapi \
  -v mongodb_data:/data/db \
  mongo:7.0
```

### Run FilesAPI Container
```bash
docker run -d \
  --name filesapi-web \
  -p 5100:8080 \
  -e ConnectionStrings__DefaultConnection="mongodb://admin:password123@mongodb:27017/filesapi?authSource=admin" \
  -v filesapi_uploads:/app/uploads \
  --link filesapi-mongodb:mongodb \
  filesapi:latest
```

## Configuration

### Environment Variables
- `ASPNETCORE_ENVIRONMENT`: Set to `Production` for Docker deployment
- `ASPNETCORE_URLS`: Application URLs (default: http://+:8080)
- `ConnectionStrings__DefaultConnection`: MongoDB connection string
- `MongoDB__ConnectionString`: Alternative MongoDB connection string
- `MongoDB__DatabaseName`: MongoDB database name

### Volume Mounts
- **File Uploads**: `/app/uploads` - Stores uploaded files
- **MongoDB Data**: `/data/db` - Stores MongoDB database files

### Security Considerations
- The application runs as a non-root user (`appuser`) for security
- MongoDB is configured with authentication enabled
- Default credentials should be changed for production use

## Development vs Production

### Development
```bash
# Use development docker-compose override
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d
```

### Production
```bash
# Use production environment variables
docker-compose --env-file .env.production up -d
```

## Troubleshooting

### Check Container Logs
```bash
# FilesAPI logs
docker logs filesapi-web

# MongoDB logs
docker logs filesapi-mongodb

# All services logs
docker-compose logs -f
```

### Access Container Shell
```bash
# FilesAPI container
docker exec -it filesapi-web /bin/bash

# MongoDB container
docker exec -it filesapi-mongodb mongosh
```

### Reset Everything
```bash
# Stop and remove all containers and volumes
docker-compose down -v
docker system prune -f

# Rebuild and restart
docker-compose up -d --build
```

## File Upload Testing

Once the containers are running, you can test file uploads:

1. **Via Swagger UI**: Navigate to http://localhost:5100/swagger
2. **Via curl**:
   ```bash
   curl -X POST "http://localhost:5100/api/storage" \
     -H "Content-Type: multipart/form-data" \
     -F "file=@/path/to/your/file.txt" \
     -F "description=Test file upload"
   ```

## Performance Tuning

### For Large File Uploads
- Increase Docker container memory limits
- Adjust MongoDB WiredTiger cache size
- Configure appropriate disk space for volumes

### Example with Resource Limits
```yaml
services:
  filesapi:
    deploy:
      resources:
        limits:
          memory: 2G
        reservations:
          memory: 1G
```

## Backup and Restore

### Backup MongoDB Data
```bash
docker exec filesapi-mongodb mongodump --authenticationDatabase admin -u admin -p password123 --out /backup
docker cp filesapi-mongodb:/backup ./mongodb-backup
```

### Restore MongoDB Data
```bash
docker cp ./mongodb-backup filesapi-mongodb:/backup
docker exec filesapi-mongodb mongorestore --authenticationDatabase admin -u admin -p password123 /backup
```
