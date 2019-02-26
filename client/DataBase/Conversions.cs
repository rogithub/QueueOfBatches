using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace DataBase
{
	public static class Conversions
	{

		public static T GetCastValue<T>(object obj)
		{
			if (obj is DBNull)
				return default(T);

			return (T)obj;
		}

		public static string GetString(object obj)
		{
			if (obj is DBNull)
				return string.Empty;

			return Convert.ToString(obj);
		}
	}
}
