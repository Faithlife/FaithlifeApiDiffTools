using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ApiDiffTool;
using CommandLine;
using NuGet;

namespace PackageDiffTool
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
			var packageHelper = new LocalPackageHelper();
			if (options.Verbose)
				packageHelper.Logger = ConsoleLogger.Instance;

			var package = packageHelper.GetLocalPackage(options.Path);

			var packageId = options.PackageId ?? package.Id;
			var version = options.Version != null ? new SemanticVersion(options.Version) : null;
			var basePackage = packageHelper.GetPackage(packageId, version);

			if (basePackage == null)
			{
				Console.Error.WriteLine("Package not found {0} {1}", packageId, version);
				return 1;
			}

			if (options.Verbose)
				Console.WriteLine("Comparing {0} with base version {1}", package, basePackage.Version);

			SemanticVersion suggestedVersion;
			var changes = PackageDiff.ComparePackageTypes(basePackage, package, out suggestedVersion);

			if (options.Verbose)
			{
				Console.WriteLine("Suggested version: {0}", suggestedVersion);
				Console.WriteLine();
				foreach (var pair in changes)
				{
					Console.WriteLine("Framework: {0}", pair.Key);
					WriteVerboseChanges(pair.Value.SelectMany(x => x.Changes).ToList(), Console.Out);
				}
			}
			else
			{
				Console.WriteLine(suggestedVersion);
				if (!options.Quiet)
				{
					foreach (var pair in changes.Where(x => x.Value.SelectMany(y => y.Changes).Any()))
					{
						Console.WriteLine(pair.Key);
						foreach (var change in pair.Value.SelectMany(x => x.Changes))
							Console.WriteLine("{0} {1}", change.IsBreaking ? "B" : "N", change.Message);
					}
				}
			}

			if (options.XUnit)
			{
				var root = XUnitFormatter.Format(changes);
				var doc = new XDocument(root);
				var resultsFileName = packageId + "-changes.xml";
				doc.Save(resultsFileName);
				if (options.Verbose)
					Console.WriteLine("xUnit results saved in {0}", resultsFileName);
			}

			if (options.VerifyVersion && package.Version < suggestedVersion)
				return 2;
			return 0;
		}

		static void WriteVerboseChanges(IReadOnlyCollection<Change> changes, TextWriter writer)
		{
			if (changes.Count == 0)
				writer.WriteLine("No changes");
			foreach (var changeGroup in changes.GroupBy(x => x.IsBreaking))
			{
				writer.WriteLine("{0} changes:", changeGroup.Key ? "Breaking" : "Non-breaking");
				foreach (var change in changeGroup)
					writer.WriteLine(change.Message);
				writer.WriteLine();
			}
		}

		class Options
		{
			[Value(0, Required = true)]
			public string Path { get; set; }

			[Option(HelpText = "Package ID to compare with")]
			public string PackageId { get; set; }

			[Option(HelpText = "Package version to compare with")]
			public string Version { get; set; }

			[Option(HelpText = "Verbose output")]
			public bool Verbose { get; set; }

			[Option(HelpText = "Only output suggested version")]
			public bool Quiet { get; set; }

			[Option(HelpText = "Generate xUnit results")]
			public bool XUnit { get; set; }

			[Option(HelpText = "Fail if version is less than suggested")]
			public bool VerifyVersion { get; set; }
		}
	}
}
