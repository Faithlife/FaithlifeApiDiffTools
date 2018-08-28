using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using Faithlife.FacadeGenerator;
using Mono.Cecil;
using NuGet;

namespace Faithlife.PackageDiffTool
{
	class ConsoleLogger : ILogger
	{
		public static readonly ConsoleLogger Instance = new ConsoleLogger();

		public void Log(MessageLevel level, string message, params object[] args)
		{
			Console.WriteLine(message, args);
		}

		public FileConflictResolution ResolveFileConflict(string message)
		{
			return FileConflictResolution.IgnoreAll;
		}

		private ConsoleLogger()
		{
		}
	}
}
