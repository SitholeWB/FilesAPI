// MongoDB initialization script for FilesAPI
// This script runs when the MongoDB container starts for the first time

// Switch to the filesapi database
db = db.getSiblingDB('filesapi');

// Create collections with proper indexes
db.createCollection('filedetails');
db.createCollection('fs.files');
db.createCollection('fs.chunks');

// Create indexes for better performance
db.filedetails.createIndex({ "id": 1 }, { unique: true });
db.filedetails.createIndex({ "name": 1 });
db.filedetails.createIndex({ "contentType": 1 });
db.filedetails.createIndex({ "addedDate": 1 });

// GridFS indexes (MongoDB creates these automatically, but we can ensure they exist)
db.fs.files.createIndex({ "filename": 1 });
db.fs.files.createIndex({ "uploadDate": 1 });
db.fs.chunks.createIndex({ "files_id": 1, "n": 1 }, { unique: true });

print('FilesAPI database initialized successfully!');
print('Collections created: filedetails, fs.files, fs.chunks');
print('Indexes created for optimal performance');
