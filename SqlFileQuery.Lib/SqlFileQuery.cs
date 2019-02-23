using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using SqlFileQueryLib.Tools;

namespace SqlFileQueryLib
{
	/// <summary>
	/// Třída pro přímý přístup k SQL Databázi.
	/// Pokud z nějakého důvodu nelze provést SQL dotaz pomocí ORM Framewoku využiju tuto třídu
	/// Veškterá DB spojení budou vždy okamžitě ukončena. Pokud uživatel neuvede jinak v parametru metod
	/// </summary>
	[DebuggerDisplay("Path={ScriptsPath}, Database={sqlConnection.Database}")]
	public class SqlFileQuery : IDisposable
	{

		private SqlConnection sqlConnection;
		private Dictionary<string, string> scriptBuffer = new Dictionary<string, string>();

		/// <summary>
		/// Cesta ze které složky v projektu bude tahat sql scripty
		/// </summary>
		protected string ScriptsPath { get; private set; }

		/// <summary>
		/// Jméno Namespaceu projektu ve kterém se nachází SQL Scripty
		/// </summary>
		public string NamespaceName { get; set; }

		/// <param name="sqlConnection"></param>
		/// <param name="scriptsPath">Cesta složek v projekt k SQL scriptům. Složky oddělené tečkami</param>
		public SqlFileQuery(SqlConnection sqlConnection, string scriptsPath)
		{
			this.sqlConnection = sqlConnection;
			this.ScriptsPath = scriptsPath;
		}

		/// <summary>
		/// Načte text SQL dotazu z uvedeného umístění <see cref="SqlFileQuery.ScriptsPath"/> a vytvoří objekt typu <see cref="SqlCommand"/>
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public SqlCommand CreateCommand(string fileName)
		{
			SqlCommand cmd = sqlConnection.CreateCommand();

			if (!scriptBuffer.ContainsKey(fileName))
				scriptBuffer.Add(fileName, this.GetCommandText(fileName));

			cmd.CommandText = scriptBuffer[fileName];
			return cmd;
		}


		//[DataObjectMethod(DataObjectMethodType.Select, true)]
		public virtual IEnumerable<object[]> Execute(IDbCommand cmd)
		{
			return Execute(cmd, false);
		}

		/// <summary>
		/// Slouží k provedení SELECT dotazu
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="manualClose">Pokud true tak po provedení příkazu neukončí spojení. Je potřeba zavoalt <see cref="CloseConnection"/></param>
		/// <returns></returns>
		//[DataObjectMethod(DataObjectMethodType.Select)]
		protected virtual IEnumerable<object[]> Execute(IDbCommand cmd, bool manualClose)
		{
			if (sqlConnection.State != ConnectionState.Open)
				sqlConnection.Open();
			object[] o;
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					o = new object[reader.FieldCount];
					reader.GetValues(o);
					yield return o;
				}
			}
			if (!manualClose)
				sqlConnection.Close();
		}

		/// <summary>
		/// Slouží k provedení SELECT dotazu
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="manualClose">Pokud true tak po provedení příkazu neukončí spojení. Je potřeba zavoalt <see cref="CloseConnection"/></param>
		/// <returns></returns>
		//[DataObjectMethod(DataObjectMethodType.Select)]
		protected virtual IEnumerable<T> Execute<T>(IDbCommand cmd, bool manualClose)
		{
			//TODO: provede SQL příkaz kde výstupem metody budo objekty generického typu
			//if (sqlConnection.State != ConnectionState.Open)
			//    sqlConnection.Open();
			//object[] o;
			//using (var reader = cmd.ExecuteReader())
			//{
			//    while (reader.Read())
			//    {
			//        o = new object[reader.FieldCount];
			//        reader.GetValues(o);
			//        yield return o;
			//    }
			//}
			//if (!manualClose)
			//    sqlConnection.Close();
			throw new NotImplementedException();
		}

		//[DataObjectMethod(DataObjectMethodType.Insert | DataObjectMethodType.Update | DataObjectMethodType.Delete)]
		public virtual int ExecuteNonQuery(IDbCommand cmd)
		{
			return ExecuteNonQuery(cmd, false);

		}

		//[DataObjectMethod(DataObjectMethodType.Insert | DataObjectMethodType.Update | DataObjectMethodType.Delete)]
		public virtual int ExecuteNonQuery(IDbCommand cmd, bool manualClose)
		{
			//this.ValidateFileContent(this.GetCommandText(cmd.CommandText));
			if (sqlConnection.State != ConnectionState.Open)
				sqlConnection.Open();


			int affected = cmd.ExecuteNonQuery();

			if (!manualClose)
				sqlConnection.Close();
			return affected;
		}

		#region Public Helper methods

		/// <summary>
		/// Přidá do <paramref name="cmd"/>  položky kolekce <paramref name="radky"/> kde jméno parametru bude její název
		/// </summary>
		/// <typeparam name="T">Anonymní třída</typeparam>
		/// <param name="cmd">Existující příkaz do kterého budou přidány parametry</param>
		/// <param name="radky"></param>
		public void AddMultipleParameters<T>(IDbCommand cmd, IEnumerable<T> radky)
		{
			if (radky.Count() > 999)
				throw new ArgumentOutOfRangeException("Nelze vložit víc než 999 záznamů najednou");
			if (!this.ParameterNamesAreCorrect(radky.GetType().GetGenericArguments()[0], this.GetRepeatingPart(cmd.CommandText)))
			{
				var exception = new ArgumentException("Nazvy propert kolekce se neschoduní s požadovanými. Zobraz si data pro více informací.");
				foreach (var item in GetMissingParameter(radky.GetType().GetGenericArguments()[0], cmd.CommandText))
				{
					exception.Data.Add(item, "Missing");
				}
				throw exception;
			}

			int counter = 0;
			string repetitive = this.GetRepeatingPart(cmd.CommandText);
			var parameters = GetFiltredSqlParametersInString(cmd.CommandText).Select(a => a.Substring(1));
			string extensionPart = this.DuplicateRepetitivePart(repetitive, radky.Count());
			cmd.CommandText = cmd.CommandText.Replace(repetitive, extensionPart);

			//Načtu si property z první položky
			//Smyčka obsahuje kontrolu jestli parametr je v pokolekci
			PropertyInfo[] properties = radky.First().GetType().GetTypeInfo().DeclaredProperties.Where(a => parameters.Any(b => b.Equals(a.Name, StringComparison.OrdinalIgnoreCase))).ToArray();
			foreach (T item in radky)
			{
				counter++;
				foreach (PropertyInfo item2 in properties)
				{
					object value = item2.GetValue(item);
					SqlDbType type = SqlDataTypeMapper.GetDbType(item2.PropertyType);
					string name = "@" + item2.Name + counter.ToString();
					AddParameter(cmd, type, name, value);
				}
			}
		}

		public void AddParameter(IDbCommand cmd, SqlDbType dbType, string paramName, object value, int length = 0)
		{
			SqlParameter myParam;
			if (length != 0)
			{
				myParam = new SqlParameter(paramName, dbType, length);
			}
			else
			{
				myParam = new SqlParameter(paramName, dbType);
			}
			myParam.Value = value != null ? value : DBNull.Value;
			cmd.Parameters.Add(myParam);
		}

		public void CloseConnection()
		{
			sqlConnection.Close();
		}
		#endregion


		#region PrivateMethods

		private bool ParameterNamesAreCorrect(Type type, string cmd)
		{
			return GetMissingParameter(type, cmd).Count() == 0;
		}

		private IEnumerable<string> GetMissingParameter(Type type, string cmd)
		{
			List<string> names = new List<string>();
			//TODO: Předělat
			foreach (string item in GetFiltredSqlParametersInString(cmd))
			{
				string name = item.Substring(1);//Odeberu @

				foreach (PropertyInfo item2 in type.GetTypeInfo().DeclaredProperties)
				{
					if (item2.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
						goto yes;

				}
				names.Add(name);

				yes:;
			}
			return names;
		}

		private void ValidateFileForMultipleParameters(string commandText)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Zopakuje <paramref name="repetitive"/> část <paramref name="counter"/> xkrát
		/// </summary>
		/// <param name="repetitive">Ta část sql scriptu která má být duplikována</param>
		/// <param name="counter">Počet zopakování opakovací části</param>
		/// <returns></returns>
		private string DuplicateRepetitivePart(string repetitive, int counter)
		{
			//Odeberu bílé znaky
			repetitive = Regex.Replace(repetitive, @"\s+", "");
			StringBuilder sb = new StringBuilder(repetitive.Length * (counter - 1));

			var parameters = GetSqlParametersInString(repetitive);

			//Počítám od 1 do N
			for (int i = 1; i <= counter; i++)
			{
				if (i != 1)
					sb.Append(Environment.NewLine + ",");

				string noBracet = "(";

				foreach (var item in parameters)
				{
					if (item.Length >= 2 && item.StartsWith("@") && !item.StartsWith("@@"))
					{
						noBracet += item + i + ",";
					}
					else
					{
						noBracet += item + ",";
					}
				}
				noBracet = noBracet.Remove(noBracet.Length - 1);
				noBracet += ")";
				sb.Append(noBracet);
			}

			return sb.ToString();
		}


		/// <summary>
		/// Vrátí všechny parametry ve vstupním stringu včetně Konstant a @@ proměných. Pro @ volejte <seealso cref="GetFiltredSqlParametersInString"/>
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>        
		private IEnumerable<string> GetSqlParametersInString(string source)
		{
			//Najdu všechny proměné (začínající @) 
			//TODO: šel by udělat lepší regex
			//Nejprve odeberu závorky, pak rozdělím
			//TODO: špatný parser. Půjde udělat líp;
			source = source.Trim();
			var par = source.Substring(1, source.Length - 2).Split(',');
			for (int i = 0; i < par.Length; i++)
			{
				par[i] = par[i].Trim();
			}
			return par;

		}

		private IEnumerable<string> GetFiltredSqlParametersInString(string source)
		{
			var matches = Regex.Matches(source, @"\@[^=<>\s\'\(\)\,]+");
			foreach (var item in matches)
			{
				//ignoruju systémové proměné sql serveru jako @@IDENTITY
				if (item.ToString().StartsWith("@@"))
					continue;
				yield return item.ToString();
			}
		}

		private string GetCommandText(string fileName)
		{
			string name = ScriptsPath + "." + fileName;
			Assembly assembly = null;
			if (string.IsNullOrEmpty(NamespaceName))
				assembly = Assembly.Load(ScriptsPath);


			var vaf = assembly.GetManifestResourceStream(name);
			if (vaf == null)
				throw new ArgumentException("Požadovaný script nebyl nalezen.");

			using (Stream strm = vaf)
			using (StreamReader queryText = new StreamReader(strm))
			{
				return queryText.ReadToEnd();
			}
		}

		/// <summary>
		/// Vrátí text části který má být duplikován bez komentářů o duplikování
		/// </summary>
		/// <param name="commandText"></param>
		/// <returns></returns>
		private string GetRepeatingPart(string commandText)
		{
			string pattern = @"\/\*Repeating start\*\/[\s\S]*Repeating end\*\/";
			var str = Regex.Match(commandText, pattern).Value;
			//Regex regex = new Regex(pattern);

			//Odeberu začáteční a koncový komentář
			str = str.Remove(str.LastIndexOf(Environment.NewLine));
			str = str.Substring(str.IndexOf(Environment.NewLine) + Environment.NewLine.Length);
			return str.Trim();
		}

		private void ValidateFileContent(string fileContent)
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			sqlConnection.Dispose();
		}
		#endregion
	}
}
