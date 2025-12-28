namespace Contracts;

public interface ISettingsService
{
    MongoDBAppSettings GetMongoDBAppSettings();

    LiteDBAppSettings GetLiteDBAppSettings();
}