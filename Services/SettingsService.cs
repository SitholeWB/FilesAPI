using Contracts;
using Microsoft.Extensions.Options;
using Models.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services
{
	public class SettingsService : ISettingsService
	{
		private readonly MongoDBAppSettings _mongoDBAppSettings;

		public SettingsService(IOptions<MongoDBAppSettings> mongoDBAppSettings)
		{
			_mongoDBAppSettings = mongoDBAppSettings.Value;
		}
		public MongoDBAppSettings GetMongoDBAppSettings()
		{
			return _mongoDBAppSettings;
		}
	}
}
