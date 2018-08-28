using System;
using System.Linq;
using CommandLine;
using FacadeGenerator;

namespace ApiDiffTool
{
	class MainClass
	{
		public static int Main(string[] args)
		{
			return Parser.Default.ParseArguments<Options>(args)
				.MapResult(Run, errors => 1);
		}

		static int Run(Options options)
		{
			var module1 = CecilUtility.ReadModule(options.File1);
			var module2 = CecilUtility.ReadModule(options.File2);

			FacadeModuleProcessor.MakePublicFacade(module1);
			FacadeModuleProcessor.MakePublicFacade(module2);

			var changes = ApiDiff.FindChanges(module1, module2);

			if (changes.Count == 0)
				Console.WriteLine("No changes");
			foreach (var changeGroup in changes.GroupBy(x => x.IsBreaking))
			{
				Console.WriteLine("{0} changes:", changeGroup.Key ? "Breaking" : "Non-breaking");
				foreach (var change in changeGroup)
					Console.WriteLine(change.Message);
				Console.WriteLine();
			}

			return 0;
		}

		class Options
		{
			[Value(0, Required = true)]
			public string File1 { get; set; }

			[Value(1, Required = true)]
			public string File2 { get; set; }
		}
	}
}
