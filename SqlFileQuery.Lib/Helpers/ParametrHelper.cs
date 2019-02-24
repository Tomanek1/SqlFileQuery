using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlFileQueryLib.Helpers
{
	internal class ParametrHelper
	{

		internal bool ParameterNamesAreCorrect(Type type, string cmd)
		{
			return GetMissingParameter(type, cmd).Count() == 0;
		}


		internal IEnumerable<string> GetMissingParameter(Type type, string cmd)
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


		internal IEnumerable<string> GetFiltredSqlParametersInString(string source)
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

		/// <summary>
		/// Vrátí všechny parametry ve vstupním stringu včetně Konstant a @@ proměných. Pro @ volejte <seealso cref="GetFiltredSqlParametersInString"/>
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>        
		internal IEnumerable<string> GetSqlParametersInString(string source)
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

		internal void ValidateFileForMultipleParameters(string commandText)
		{
			throw new NotImplementedException();
		}

	}
}
