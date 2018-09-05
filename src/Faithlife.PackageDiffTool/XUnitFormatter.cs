using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Faithlife.ApiDiffTool;
using NuGet.Frameworks;

namespace Faithlife.PackageDiffTool
{
	public static class XUnitFormatter
	{
		public static XElement Format(IReadOnlyDictionary<NuGetFramework, IReadOnlyList<TypeChanges>> changes)
		{
			var typeCount = changes.Values.SelectMany(x => x).Count();
			var breakingChangeCount = changes.Values.SelectMany(x => x).SelectMany(x => x.Changes).Count(x => x.IsBreaking);
			return new XElement("testsuites", new object[] {
				new XAttribute("tests", typeCount),
				new XAttribute("failures", breakingChangeCount),
				new XAttribute("errors", 0)
			}.Concat(changes.Select(Format)).ToArray());
		}

		static XElement Format(KeyValuePair<NuGetFramework, IReadOnlyList<TypeChanges>> frameworkChangeSet)
		{
			var typeCount = frameworkChangeSet.Value.Count;
			var breakingChangeCount = frameworkChangeSet.Value.SelectMany(x => x.Changes).Count(x => x.IsBreaking);
			return new XElement("testsuite", new object[] {
				new XAttribute("name", frameworkChangeSet.Key.DotNetFrameworkName),
				new XAttribute("tests", typeCount),
				new XAttribute("failures", breakingChangeCount),
				new XAttribute("errors", 0)
			}.Concat(frameworkChangeSet.Value.Select(Format)).ToArray());
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
