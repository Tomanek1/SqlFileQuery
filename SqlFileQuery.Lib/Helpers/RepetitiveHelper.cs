using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlFileQueryLib.Helpers
{
	internal class RepetitiveHelper
	{
		private ParametrHelper ph = new ParametrHelper();

		/// <summary>
		/// Zopakuje <paramref name="repetitive"/> část <paramref name="counter"/> xkrát
		/// </summary>
		/// <param name="repetitive">Ta část sql scriptu která má být duplikována</param>
		/// <param name="counter">Počet zopakování opakovací části</param>
		/// <returns></returns>
		internal string DuplicateRepetitivePart(string repetitive, int counter)
		{
			//Odeberu bílé znaky
			repetitive = Regex.Replace(repetitive, @"\s+", "");
			StringBuilder sb = new StringBuilder(repetitive.Length * (counter - 1));

			var parameters = ph.GetSqlParametersInString(repetitive);

			//Počítám od 1 do N
			for (int i = 1; i <= counter; i++)
			{
				if (i != 1)
				{
					sb.Append(Environment.NewLine + ",");
				}

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
		/// Vrátí text části který má být duplikován bez komentářů o duplikování
		/// </summary>
		/// <param name="commandText"></param>
		/// <returns></returns>
		internal string GetRepeatingPart(string commandText)
		{
			string pattern = @"\/\*Repeating start\*\/[\s\S]*Repeating end\*\/";
			var str = Regex.Match(commandText, pattern).Value;
			//Regex regex = new Regex(pattern);

			//Odeberu začáteční a koncový komentář
			str = str.Remove(str.LastIndexOf(Environment.NewLine));
			str = str.Substring(str.IndexOf(Environment.NewLine) + Environment.NewLine.Length);
			return str.Trim();
		}

	}
}
