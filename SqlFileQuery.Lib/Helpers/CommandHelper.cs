using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace SqlFileQueryLib.Helpers
{
	internal class CommandHelper
	{
		internal string GetCommandText(string namespaceName, string scriptsPath, string fileName)
		{
			string name = scriptsPath + "." + fileName;
			Assembly assembly = null;
			if (string.IsNullOrEmpty(namespaceName))
				assembly = Assembly.Load(scriptsPath);


			Stream vaf = assembly.GetManifestResourceStream(name);
			if (vaf == null)
				throw new ArgumentException("Požadovaný script nebyl nalezen.");

			using (Stream strm = vaf)
			using (StreamReader queryText = new StreamReader(strm))
			{
				return queryText.ReadToEnd();
			}
		}


		internal void ValidateFileContent(string fileContent)
		{
			throw new NotImplementedException();
		}

	}
}
