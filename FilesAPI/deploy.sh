dotnet build --configuration Release
sudo systemctl stop files-api.service
dotnet publish -c Release --output /var/www/files-api FilesAPI.csproj
sudo systemctl start files-api.service
sudo systemctl reload nginx