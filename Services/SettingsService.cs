using Contracts;
using Microsoft.Extensions.Options;
using Models.Settings;

namespace Services
{
	public class SettingsService : ISettingsService
	{
		private readonly MongoDBAppSettings _mongoDBAppSettings;
		private readonly LiteDBAppSettings _liteDBAppSettings;

		public SettingsService(IOptions<MongoDBAppSettings> mongoDBAppSettings, IOptions<LiteDBAppSettings> liteDBAppSettings)
		{
			_mongoDBAppSettings = mongoDBAppSettings.Value;
			_liteDBAppSettings = liteDBAppSettings.Value;
		}

		public LiteDBAppSettings GetLiteDBAppSettings()
		{
			return _liteDBAppSettings;
		}

		public MongoDBAppSettings GetMongoDBAppSettings()
		{
			return _mongoDBAppSettings;
		}
	}
}
