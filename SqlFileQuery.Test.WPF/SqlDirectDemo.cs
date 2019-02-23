using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlFileQueryLib.Test.WPF
{
	class SqlDirectDemo
	{
		SqlFileQuery sqlFileQuery = null;

		public SqlDirectDemo()
		{
			string con = ConfigurationManager.ConnectionStrings[1].ConnectionString;
			this.sqlFileQuery = new SqlFileQuery(new SqlConnection(con), "SqlScripts");

		}


		public IEnumerable<object> GetDemo()
		{
			SqlCommand cmd = sqlFileQuery.CreateCommand("Demo.sql");
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

		public IEnumerable<object> GetDemoInRoot()
		{
			//TODO: Dodělat
			SqlCommand cmd = sqlFileQuery.CreateCommand("Demo.sql");
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

	}
}
