using Models.Settings;

namespace Contracts
{
	public interface ISettingsService
	{
		MongoDBAppSettings GetMongoDBAppSettings();
		LiteDBAppSettings GetLiteDBAppSettings();
	}
}
