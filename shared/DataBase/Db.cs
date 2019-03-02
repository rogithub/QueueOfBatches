using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace DataBase
{
	internal static class Db
	{
		public static SqlConnection GetConnection()
		{
			return new SqlConnection(ConfigurationManager.ConnectionStrings["Default"].ConnectionString);
		}

		private static void CloseConnection(SqlConnection conn)
		{
			if (conn.State != System.Data.ConnectionState.Closed)
				conn.Close();
		}

		private static void OpenConnection(SqlConnection conn)
		{
			if (conn.State != System.Data.ConnectionState.Open)
				conn.Open();
		}

		public static int ExecuteNonQuery(SqlCommand cmd)
		{
			return Execute<int>(cmd, delegate ()
			{
				return cmd.ExecuteNonQuery();
			});
		}

		public static object ExecuteScalar(SqlCommand cmd)
		{
			return Execute<object>(cmd, delegate ()
			{
				return cmd.ExecuteScalar();
			});
		}

		public static void ExecuteDataReader(SqlCommand cmd, Action<SqlDataReader> action)
		{
			if (cmd.Connection != null)
				throw new ArgumentException("Command must have null Connection property");

			cmd.Connection = GetConnection();
			try
			{
				OpenConnection(cmd.Connection);
				using (cmd)
				{
					action(cmd.ExecuteReader(CommandBehavior.CloseConnection));
				}
			}
			catch (Exception e)
			{
				throw e;
			}
			finally
			{
				CloseConnection(cmd.Connection);
				cmd.Connection.Dispose();
				cmd.Dispose();
			}
		}

		private static T Execute<T>(SqlCommand cmd, Func<T> execute)
		{
			try
			{
				OpenConnection(cmd.Connection);
				using (cmd)
				{
					return execute();
				}
			}
			catch (Exception e)
			{
				throw e;
			}
			finally
			{
				CloseConnection(cmd.Connection);
			}
		}

		public static SqlCommand GetCommand(string sqlText, CommandType type)
		{
			return new SqlCommand(sqlText) { CommandType = type };
		}

		public static SqlParameter GetParam(string name, SqlDbType type, object value)
		{
			SqlParameter param = new SqlParameter(name, value);
			param.SqlDbType = type;

			return param;
		}

		public static SqlParameter GetParamOut(string name, SqlDbType type)
		{
			SqlParameter param = new SqlParameter(name, type);
			param.Direction = ParameterDirection.Output;

			return param;
		}
	}
}
