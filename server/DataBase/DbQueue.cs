using Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;

namespace DataBase
{
	public class DbQueue : ITaskQueue<IAssemblyData, FinishResult>
	{
		public IEnumerable<IAssemblyData> Dequeue(int batchSize)
		{
			string text = string.Format("SELECT TOP {0} * FROM T_FEED_QUEUE WHERE F_DATE_STARTED IS NULL ORDER BY F_DATE_CREATED", batchSize);

			SqlCommand cmd = Db.GetCommand(text, CommandType.Text);

			List<AssemblyData> list = new List<AssemblyData>();

			Db.ExecuteDataReader(cmd, dr =>
			{
				while (dr.Read())
				{
					list.Add(new AssemblyData()
					{
						Assembly = GetAssemblyFromByteArray(dr["F_ASSEMBLY"]),
						MethodParametersTypes = GetCastedByteArray<Type[]>(dr["F_METHOD_PARAM_TYPES"]),
						ConstructorParameters = GetCastedXml<object[]>(dr["F_CONSTRUCTOR_PARAMETERS"]),
						MethodParameters = GetCastedXml<object[]>(dr["F_METHOD_PARAMETERS"]),
						FullyQualifiedName = Conversions.GetString(dr["F_FULLY_QUALIFIED_CLASS_NAME"]),
						Id = Conversions.GetCastValue<Guid>(dr["F_GUID"]),
						MethodToRun = Conversions.GetString(dr["F_METHOD_NAME"]),
						TimeoutMilliseconds = Conversions.GetCastValue<int>(dr["F_TIMEOUT_MILLISECONDS"])
					});
				}
			});

			return list;
		}

		public int Start(IEnumerable<Guid> rows, string machineName, Guid instanceId)
		{
			if (rows.Count() == 0) return 0;

			string inlist = string.Join(",", from p in rows select string.Format("'{0}'", p.ToString()));
			string inPart = string.Format(" ({0}) ", inlist);

			string text = string.Format(@"UPDATE T_FEED_QUEUE SET F_INSTANCE_ID = @instanceId, F_DATE_STARTED=GETDATE(), F_MACHINE_NAME = @machineName WHERE F_GUID in {0}", inPart);
			SqlCommand cmd = Db.GetCommand(text, CommandType.Text);
			cmd.Parameters.Add(Db.GetParam("@instanceId", SqlDbType.UniqueIdentifier, instanceId));
			cmd.Parameters.Add(Db.GetParam("@machineName", SqlDbType.VarChar, machineName));

			cmd.Connection = Db.GetConnection();
			return Db.ExecuteNonQuery(cmd);
		}

		public int Enqueue(IEnumerable<IAssemblyData> batch)
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
					count += Enqueue(rows.ToArray());
				}

				return count;
			}
			else
			{
				return batch.Count() <= 0 ? 0 : Enqueue(batch.ToArray());
			}
		}
		private int Enqueue(IAssemblyData[] rows)
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

		public int CompleteTask(FinishResult result)
		{
			List<SqlParameter> allParams = new List<SqlParameter>();
			string text = @"UPDATE T_FEED_QUEUE SET F_DATE_COMPLETED=GETDATE(), F_STATUS=@finishStatus, F_RESULT=@result, F_EXCEPTION=@exception WHERE F_GUID=@id";
			SqlCommand cmd = Db.GetCommand(text, CommandType.Text);

			cmd.Parameters.Add(Db.GetParam("@finishStatus", SqlDbType.Int, result.Status));

			if (result.Result == null)
			{
				cmd.Parameters.Add(Db.GetParam("@result", SqlDbType.Xml, DBNull.Value));
			}
			else
			{
				cmd.Parameters.Add(Db.GetParam("@result", SqlDbType.Xml, Serializer.XmlSerialize(result.Result)));
			}

			cmd.Parameters.Add(Db.GetParam("@exception", SqlDbType.VarChar, result.Exception == null ? string.Empty : result.Exception.ToString()));
			cmd.Parameters.Add(Db.GetParam("@id", SqlDbType.UniqueIdentifier, result.Id));

			cmd.Connection = Db.GetConnection();
			return Db.ExecuteNonQuery(cmd);
		}

		private static T GetCastedXml<T>(object v)
		{
			string xml = Conversions.GetCastValue<string>(v);
			return Serializer.XmlDeserialize<T>(xml);
		}

		private static T GetCastedByteArray<T>(object v)
		{
			byte[] arr = Conversions.GetCastValue<byte[]>(v);
			return Serializer.Deserialize<T>(arr);
		}

		private static Assembly GetAssemblyFromByteArray(object v)
		{
			byte[] arr = Conversions.GetCastValue<byte[]>(v);
			return Assembly.Load(arr);
		}
	}
}
