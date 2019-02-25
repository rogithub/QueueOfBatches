using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase
{
	public class Db
	{
		public SqlConnection GetConnection()
		{
			return new SqlConnection(ConfigurationManager.ConnectionStrings["Default"].ConnectionString);
		}
	}
}
