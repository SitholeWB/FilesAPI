using Models.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts
{
	public interface ISettingsService
	{
		MongoDBAppSettings GetMongoDBAppSettings();
	}
}
