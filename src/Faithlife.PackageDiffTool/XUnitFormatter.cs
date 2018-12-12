using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Faithlife.ApiDiffTool;
using NuGet.Frameworks;
using NuGet.Versioning;

namespace Faithlife.PackageDiffTool
{
	public static class XUnitFormatter
	{
		public static XElement Format(IReadOnlyDictionary<NuGetFramework, IReadOnlyList<TypeChanges>> changes, string packageId, NuGetVersion packageVersion, NuGetVersion suggestedVersion)
		{
			var typeCount = changes.Values.SelectMany(x => x).Count();
			var breakingChangeCount = changes.Values.SelectMany(x => x).SelectMany(x => x.Changes).Count(x => x.IsBreaking);
			return new XElement("testsuites", new object[]
				{
					new XAttribute("tests", typeCount),
					new XAttribute("failures", breakingChangeCount),
					new XAttribute("errors", 0)
				}
				.Concat(changes.Select(x => Format(packageId, x))
				.Concat(new[] { Format(packageId, packageVersion, suggestedVersion) }))
				.Where(x => x != null)
				.ToArray());
		}

		static XElement Format(string packageId, NuGetVersion packageVersion, NuGetVersion suggestedVersion)
		{
			var fail = packageVersion < suggestedVersion;

			return new XElement("testsuite", new object[]
				{
					new XAttribute("name", $"{packageId}.PackageDiff"),
					new XAttribute("tests", 1),
					new XAttribute("failures", fail ? 1 : 0),
					new XAttribute("errors", 0),
					new XElement("testcase", new object[]
						{
							new XAttribute("name", "PackageVersion"),
							fail ? new XElement("failure", new XAttribute("type", "IncorrectVersion"), new XAttribute("message", $"Expected version {suggestedVersion}, got {packageVersion}")) : null
						}.Where(x => x != null))
				});
		}

		static XElement Format(string packageId, KeyValuePair<NuGetFramework, IReadOnlyList<TypeChanges>> frameworkChangeSet)
		{
			var typeCount = frameworkChangeSet.Value.Count;
			var breakingChangeCount = frameworkChangeSet.Value.SelectMany(x => x.Changes).Count(x => x.IsBreaking);
			var frameworkName = frameworkChangeSet.Key.DotNetFrameworkName;
			return new XElement("testsuite", (new object[]
				{
					new XAttribute("name", $"{packageId}.ApiDiff.{frameworkName.Replace('.', '_')}"),
					new XAttribute("tests", typeCount),
					new XAttribute("failures", breakingChangeCount),
					new XAttribute("errors", 0)
				})
				.Concat(frameworkChangeSet.Value.Select(Format))
				.ToArray());
		}

		static XElement Format(TypeChanges typeChangeSet)
		{
			var testCaseElement = new XElement("testcase", new XAttribute("name", typeChangeSet.Type != null ? typeChangeSet.Type.FullName : "<framework changes>"));
			var nonBreakingChanges = new List<Change>();
			foreach (var change in typeChangeSet.Changes)
			{
				if (change.IsBreaking)
					testCaseElement.Add(new XElement("failure", new XAttribute("type", "BreakingChange"), new XAttribute("message", change.Message)));
				else
					nonBreakingChanges.Add(change);
			}
			if (nonBreakingChanges.Any())
			{
				testCaseElement.Add(new XElement("system-out", string.Join("\n", nonBreakingChanges.Select(x => x.Message))));
			}
			return testCaseElement;
		}
	}
}
