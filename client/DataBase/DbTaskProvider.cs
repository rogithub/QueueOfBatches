
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Tasks;

namespace DataBase
{
	public static class DbTaskProvider
	{
		public static int Save(IEnumerable<IAssemblyData> batch)
		{
			/*
			 * https://docs.microsoft.com/en-us/sql/sql-server/maximum-capacity-specifications-for-sql-server?view=sql-server-2017
				Parameters per user-defined function 		2,100
				So 2100/8 = 262.5
			 */
			int batchSize = 262;
			if (batch.Count() > batchSize)
			{
				int count = 0;
				foreach (var rows in batch.Batch(batchSize))
				{
					count += AddJobs(rows.ToArray());
				}

				return count;
			}
			else
			{
				return batch.Count() <= 0 ? 0 : AddJobs(batch.ToArray());
			}
		}

		private static int AddJobs(IAssemblyData[] rows)
		{
			StringBuilder sb = new StringBuilder();
			List<SqlParameter> allParams = new List<SqlParameter>();
			string text = @"INSERT INTO T_FEED_QUEUE 
				(F_GUID, F_STATUS, F_DATE_CREATED, F_TIMEOUT_MILLISECONDS, F_ASSEMBLY, F_METHOD_PARAM_TYPES, F_CONSTRUCTOR_PARAMETERS, F_METHOD_PARAMETERS, F_FULLY_QUALIFIED_CLASS_NAME, F_METHOD_NAME) 
				VALUES 
				(@guid{0},0,GETDATE(),@timeoutms{0},@assembly{0},@paramTypes{0},@consParams{0},@methodParams{0},@fullyQName{0},@method{0});";
			for (int i = 0; i < rows.Length; i++)
			{
				sb.AppendFormat(text, i);
				allParams.Add(Db.GetParam(string.Format("@guid{0}", i), SqlDbType.UniqueIdentifier, rows[i].Id));
				allParams.Add(Db.GetParam(string.Format("@timeoutms{0}", i), SqlDbType.Int, rows[i].TimeoutMilliseconds <= 0 ? -1 : rows[i].TimeoutMilliseconds));
				allParams.Add(Db.GetParam(string.Format("@assembly{0}", i), SqlDbType.Binary, Serializer.Serialize(rows[i].Assembly)));
				allParams.Add(Db.GetParam(string.Format("@paramTypes{0}", i), SqlDbType.Binary, Serializer.Serialize(rows[i].MethodParametersTypes)));
				allParams.Add(Db.GetParam(string.Format("@consParams{0}", i), SqlDbType.Xml, Serializer.XmlSerialize(rows[i].ConstructorParameters)));
				allParams.Add(Db.GetParam(string.Format("@methodParams{0}", i), SqlDbType.Xml, Serializer.XmlSerialize(rows[i].MethodParameters)));
				allParams.Add(Db.GetParam(string.Format("@fullyQName{0}", i), SqlDbType.VarChar, rows[i].FullyQualifiedName));
				allParams.Add(Db.GetParam(string.Format("@method{0}", i), SqlDbType.VarChar, rows[i].MethodToRun));
			}

			SqlCommand cmd = Db.GetCommand(sb.ToString(), CommandType.Text);
			cmd.Parameters.AddRange(allParams.ToArray());
			cmd.Connection = Db.GetConnection();
			return Db.ExecuteNonQuery(cmd);
		}
	}
}
