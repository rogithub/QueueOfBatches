using System;
using System.Configuration;
using System.Globalization;

namespace DataBase
{
	/// <summary>
	/// App Settings
	/// </summary>
	public static class AppSettings
	{
		public static int BatchSize => Setting<int>("BATCH_SIZE") == 0 ? 10 : Setting<int>("BATCH_SIZE");



		private static T Setting<T>(string name)
		{
			string value = ConfigurationManager.AppSettings[name];

			if (value == null)
			{
				return default(T);
			}

			return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
		}
	}
}
