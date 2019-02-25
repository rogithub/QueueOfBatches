using Message;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace DataBase
{
	public static class DbFeedProvider
	{
		public static IEnumerable<IAssemblyData> GetNextBatch(int batchSize)
		{
			SqlCommand cmd = Db.GetCommand("SELECT TOP @batchSize * FROM T_MESSAGES WHERE F_EXECUTED=? ORDER BY F_DATE_CREATED DESC", CommandType.Text);
			cmd.Parameters.Add(Db.GetParam("@batchSize", SqlDbType.Int, batchSize));
			List<AssemblyData> list = new List<AssemblyData>();

			Db.ExecuteDataReader(cmd, dr =>
			{
				list.Add(new AssemblyData()
				{
					Assembly = GetCastedByteArray<Assembly>(dr["F_ASSEMBLY"]),
					ConstructorParameters = GetCastedByteArray<object[]>(dr["F_CONSTRUCTOR_PARAMETERS"])
				});
			});

			return list;
		}

		private static T GetCastedByteArray<T>(object v)
		{
			throw new NotImplementedException();
		}
	}
}
