using System;
using System.Collections.Generic;

namespace DataBase
{
	internal static class Conversions
	{
		//https://stackoverflow.com/questions/13709626/split-an-ienumerablet-into-fixed-sized-chunks-return-an-ienumerableienumerab
		public static IEnumerable<IEnumerable<T>> Batch<T>(
				this IEnumerable<T> source, int batchSize)
		{
			using (var enumerator = source.GetEnumerator())
				while (enumerator.MoveNext())
					yield return YieldBatchElements(enumerator, batchSize - 1);
		}

		private static IEnumerable<T> YieldBatchElements<T>(IEnumerator<T> source, int batchSize)
		{
			yield return source.Current;
			for (int i = 0; i < batchSize && source.MoveNext(); i++)
				yield return source.Current;
		}

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
