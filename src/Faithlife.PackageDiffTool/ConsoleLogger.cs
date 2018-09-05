using System;
using System.Threading.Tasks;
using NuGet.Common;

namespace Faithlife.PackageDiffTool
{
	class ConsoleLogger : LoggerBase
	{
		public static readonly ConsoleLogger Instance = new ConsoleLogger();

		public override void Log(ILogMessage message)
		{
			Console.WriteLine(message.Message);
		}

		public override Task LogAsync(ILogMessage message)
		{
			Console.WriteLine(message.Message);
			return Task.CompletedTask;
		}

		private ConsoleLogger()
		{
		}
	}
}
