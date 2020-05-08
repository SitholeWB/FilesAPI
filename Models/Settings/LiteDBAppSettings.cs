using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Settings
{
	public class LiteDBAppSettings
	{
		public LiteDBAppSettings()
		{
			ConnectionString = @"Filename=D:\NoSqlDBs\LiteBD\filesAPI_database.db;";
		}
		public string ConnectionString { get; set; }
	}
}
