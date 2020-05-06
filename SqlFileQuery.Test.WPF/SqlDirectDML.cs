using System;
using System.Configuration;
using System.Data.SqlClient;

namespace SqlFileQueryLib.Test.WPF
{
	public class SqlDirectDML
	{
		SqlFileQuery sqlFileQuery = null;

		internal SqlDirectDML()
		{
			string con = ConfigurationManager.ConnectionStrings[1].ConnectionString;
			this.sqlFileQuery = new SqlFileQuery(new SqlConnection(con), "SqlScripts.BasicQueries");
		}

		internal object SelectScalar()
		{
			var cmd = sqlFileQuery.CreateCommand("SelectTable.sql");
			sqlFileQuery.AddMultipleParameters(cmd, new[] { new { a = 1 }, new { a = 2 } });
			var o = cmd.ExecuteReader(cmd);
			foreach (var item in o)
			{

			}
			return o;
		}

		internal object SelectTable()
		{
			var cmd = sqlFileQuery.CreateCommand("SelectTable.sql");
			sqlFileQuery.AddMultipleParameters(cmd, new[] { new { @id =4 } });
			var o = cmd.ExecuteReader(cmd);
			foreach (var item in o)
			{

			}
			return o;
		}

		internal object SelectGrid()
		{
			throw new NotImplementedException();
		}

		internal object Delete()
		{
			throw new NotImplementedException();
		}

		internal object Update()
		{
			throw new NotImplementedException();
		}

		internal object InsertMultiple()
		{
			throw new NotImplementedException();
		}

		internal object InsertSingle()
		{
			throw new NotImplementedException();
		}
	}
}
