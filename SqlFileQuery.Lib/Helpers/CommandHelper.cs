using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SqlFileQueryLib.Helpers
{
	internal class CommandHelper
	{
		internal string GetCommandText(string namespaceName, string scriptsPath, string fileName)
		{
			if (string.IsNullOrEmpty(namespaceName))
			{
				var currentAssembly = Assembly.GetExecutingAssembly();
				var callerAssemblies = new StackTrace().GetFrames();
				var fff = callerAssemblies.Select(x => x.GetMethod());
				var rrs = fff.Where(a => a.ReflectedType != null && a.ReflectedType.Assembly != null).Select(a => a.ReflectedType.Assembly).Distinct();
				var rrrrr = rrs.Where(x => x.GetReferencedAssemblies().Any(y => y.FullName == currentAssembly.FullName));
				var initialAssembly = rrrrr.Last();
				
			}


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
