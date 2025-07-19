#!/bin/bash

# FilesAPI Standalone Runner
# Self-contained deployment with no external dependencies

echo "🚀 Starting FilesAPI in standalone mode..."
echo "📦 This version uses embedded LiteDB database"
echo "🔧 No external MongoDB required!"
echo ""

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker and try again."
    exit 1
fi

# Build and run the standalone version
echo "🔨 Building FilesAPI container..."
docker-compose -f docker-compose.standalone.yml build

echo "🏃 Starting FilesAPI..."
docker-compose -f docker-compose.standalone.yml up -d

# Wait for the service to be ready
echo "⏳ Waiting for FilesAPI to be ready..."
sleep 10

# Check if the service is healthy
if curl -f http://localhost:5100/health > /dev/null 2>&1; then
    echo ""
    echo "✅ FilesAPI is running successfully!"
    echo ""
    echo "🌐 Access points:"
    echo "   • API: http://localhost:5100"
    echo "   • Swagger UI: http://localhost:5100/swagger"
    echo "   • Health Check: http://localhost:5100/health"
    echo ""
    echo "📁 Data is persisted in Docker volumes:"
    echo "   • Database: filesapi_data"
    echo "   • Uploads: filesapi_uploads"
    echo "   • Logs: filesapi_logs"
    echo ""
    echo "🛑 To stop: docker-compose -f docker-compose.standalone.yml down"
    echo "🗑️  To reset: docker-compose -f docker-compose.standalone.yml down -v"
else
    echo "❌ FilesAPI failed to start properly. Check logs:"
    echo "   docker-compose -f docker-compose.standalone.yml logs"
fi
