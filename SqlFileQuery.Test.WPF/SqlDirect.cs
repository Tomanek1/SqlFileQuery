using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SqlFileQuery.Lib;

namespace SqlFileQuery.Test.WPF
{
	public class SqlDirect
	{
		SqlFileQuery.Lib.SqlFileQuery sqlFileQuery = null;

		public SqlDirect()
		{
			string con = ConfigurationManager.ConnectionStrings[1].ConnectionString;
			this.sqlFileQuery = new SqlFileQuery.Lib.SqlFileQuery(new SqlConnection(con), "SqlScripts");

		}

		public IEnumerable<object> GetActivePublishers()
		{
			SqlCommand cmd = sqlFileQuery.CreateCommand("GetActivePublishers.sql");
			foreach (var n in sqlFileQuery.Execute(cmd))
			{
				yield return new 
				{
					ID = Convert.ToInt32(n[0]),
					DisplayOnWeb = Convert.ToBoolean(n[1]),
					Name = n[2].ToString(),
					Code = n[3].ToString(),
					KvadosKey = n[4].ToString(),
					//CreatedOn = n[5].(),
				};
			}
		}

		internal object Get2()
		{
			SqlCommand cmd = sqlFileQuery.CreateCommand("GetActivePublishers.sql");
			throw new NotImplementedException();
		}
	}
}
