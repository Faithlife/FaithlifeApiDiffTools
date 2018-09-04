using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Faithlife.FacadeGenerator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Faithlife.ApiDiffTool.Tests
{
	[TestClass]
	public class ApiDiffTest
	{
		[TestMethod]
		public void TestFindChanges()
		{
			var directory = Path.GetDirectoryName(typeof(ApiDiffTest).Assembly.Location);

			var module1 = CecilUtility.ReadModule(Path.Combine(directory, "TestLibrary.V1.dll"));
			var module2 = CecilUtility.ReadModule(Path.Combine(directory, "TestLibrary.V2.dll"));

			FacadeModuleProcessor.MakePublicFacade(module1);
			FacadeModuleProcessor.MakePublicFacade(module2);

			var changes = ApiDiff.FindChanges(module1, module2);

			var diff = NormalizeDiff(changes.Select(change => $"{(change.IsBreaking ? "B" : "N")} {change.Message}"));
			var expectedDiff = NormalizeDiff(File.ReadAllLines(Path.Join(directory, "expected-diff.txt")));

			CollectionAssert.AreEqual(expectedDiff, diff);
		}

		private static List<string> NormalizeDiff(IEnumerable<string> lines)
		{
			return lines.Where(x => x.StartsWith("B ", StringComparison.Ordinal) || x.StartsWith("N ", StringComparison.Ordinal))
				.OrderBy(x => x, StringComparer.Ordinal)
				.ToList();
		}
	}
}
