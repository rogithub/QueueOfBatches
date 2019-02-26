using System;
using System.Configuration;
using System.Globalization;

namespace DataBase
{
	/// <summary>
	/// App Settings
	/// </summary>
	internal static class AppSettings
	{
		/// <summary>
		/// Number of items to process per round.
		/// </summary>
		public static int BatchSize => Setting<int>("BATCH_SIZE") == 0 ? 10 : Setting<int>("BATCH_SIZE");

		/// <summary>
		/// Time to waite if there are no records on the database to process.
		/// </summary>
		public static int MillisecondsToBeIdle => Setting<int>("MILLISECONDS_TO_BE_IDLE") == 0 ? 1000 : Setting<int>("MILLISECONDS_TO_BE_IDLE");

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
