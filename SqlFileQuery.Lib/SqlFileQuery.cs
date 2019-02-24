using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using SqlFileQueryLib.Helpers;
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

		private readonly ParametrHelper ph = new ParametrHelper();
		private readonly CommandHelper ch = new CommandHelper();
		private readonly RepetitiveHelper rh = new RepetitiveHelper();


		#region Public Properties

		/// <summary>
		/// Cesta ze které složky v projektu bude tahat sql scripty
		/// </summary>
		protected string ScriptsPath { get; private set; }

		/// <summary>
		/// Jméno Namespaceu projektu ve kterém se nachází SQL Scripty
		/// </summary>
		public string NamespaceName { get; set; }

		#endregion

		#region Public Execution Methods

		/// <summary>
		/// Načte text SQL dotazu z uvedeného umístění <see cref="SqlFileQuery.ScriptsPath"/> a vytvoří objekt typu <see cref="SqlCommand"/>
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public SqlCommand CreateCommand(string fileName)
		{
			//TODO: Půjde se této metody zbavit a zaimplementovat do Execute metod
			SqlCommand cmd = sqlConnection.CreateCommand();

			if (!scriptBuffer.ContainsKey(fileName))
			{
				scriptBuffer.Add(fileName, ch.GetCommandText(NamespaceName, ScriptsPath, fileName));
			}

			cmd.CommandText = scriptBuffer[fileName];
			return cmd;
		}

		public virtual IEnumerable<object[]> ExecuteReader(IDbCommand cmd)
		{
			return ExecuteReader(cmd, false);
		}

		/// <summary>
		/// Slouží k provedení SELECT dotazu
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="manualClose">Pokud true tak po provedení příkazu neukončí spojení. Je potřeba zavoalt <see cref="CloseConnection"/></param>
		/// <returns></returns>
		protected virtual IEnumerable<object[]> ExecuteReader(IDbCommand cmd, bool manualClose)
		{
			if (sqlConnection.State != ConnectionState.Open)
			{
				sqlConnection.Open();
			}

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
			{
				sqlConnection.Close();
			}
		}

		/// <summary>
		/// Slouží k provedení SELECT dotazu, kde výsledkem je jediný sloupec hodnot typu <typeparamref name="T"/>
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="manualClose">Pokud true tak po provedení příkazu neukončí spojení. Je potřeba zavoalt <see cref="CloseConnection"/></param>
		/// <returns></returns>
		protected virtual IEnumerable<T> ExecuteReader<T>(IDbCommand cmd, bool manualClose) where T : struct
		{
			if (sqlConnection.State != ConnectionState.Open)
				sqlConnection.Open();
			using (var reader = cmd.ExecuteReader())
			{
				if (reader.FieldCount != 1)
					throw new TargetParameterCountException("Počet sloupců výstupu je > 1");

				while (reader.Read())
				{
					object o = reader.GetValue(1);
					yield return (T)o;
				}
			}
			if (!manualClose)
				sqlConnection.Close();
		}

		public virtual int ExecuteNonQuery(IDbCommand cmd)
		{
			return ExecuteNonQuery(cmd, false);

		}

		public virtual int ExecuteNonQuery(IDbCommand cmd, bool manualClose)
		{
			//this.ValidateFileContent(this.GetCommandText(cmd.CommandText));
			if (sqlConnection.State != ConnectionState.Open)
			{
				sqlConnection.Open();
			}

			int affected = cmd.ExecuteNonQuery();

			if (!manualClose)
			{
				sqlConnection.Close();
			}

			return affected;
		}

		#endregion

		#region Public Parameter methods

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
			if (cmd == null || string.IsNullOrEmpty(cmd.CommandText))
				throw new ArgumentException("Nenalezen text příkazu");

			if (!ph.ParameterNamesAreCorrect(radky.GetType().GetGenericArguments()[0], rh.GetRepeatingPart(cmd.CommandText)))
			{
				var exception = new ArgumentException("Nazvy propert kolekce se neschoduní s požadovanými. Zobraz si data pro více informací");
				foreach (var item in ph.GetMissingParameter(radky.GetType().GetGenericArguments()[0], cmd.CommandText))
				{
					exception.Data.Add(item, "Missing");
				}
				throw exception;
			}

			int counter = 0;
			string repetitive = rh.GetRepeatingPart(cmd.CommandText);
			var parameters = ph.GetFiltredSqlParametersInString(cmd.CommandText).Select(a => a.Substring(1));
			string extensionPart = rh.DuplicateRepetitivePart(repetitive, radky.Count());
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

		#endregion

		#region Public Extension Functionality Methods

		public void CloseConnection()
		{
			sqlConnection.Close();
		}

		#endregion


		/// <param name="scriptsPath">Cesta složek v projekt k SQL scriptům. Složky oddělené tečkami</param>
		public SqlFileQuery(string sqlConnection, string scriptsPath)
		{
			this.sqlConnection = new SqlConnection(sqlConnection);
			this.ScriptsPath = scriptsPath;
		}

		/// <param name="scriptsPath">Cesta složek v projekt k SQL scriptům. Složky oddělené tečkami</param>
		public SqlFileQuery(SqlConnection sqlConnection, string scriptsPath)
		{
			this.sqlConnection = sqlConnection;
			this.ScriptsPath = scriptsPath;
		}

		public void Dispose()
		{
			sqlConnection.Dispose();
		}
	}
}
