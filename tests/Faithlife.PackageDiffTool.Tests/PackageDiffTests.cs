using System.Collections.Generic;
using System.Threading.Tasks;
using Faithlife.ApiDiffTool;
using NuGet.Versioning;
using Xunit;

namespace Faithlife.PackageDiffTool.Tests
{
	public class PackageDiffTests
	{
		[Theory]
		[InlineData("1.0.0", false, false, "1.0.1")]
		[InlineData("1.0.0", false, true, "1.1.0")]
		[InlineData("1.0.0", true, false, "2.0.0")]
		[InlineData("1.0.0", true, true, "2.0.0")]
		[InlineData("0.1.0", false, false, "0.1.1")]
		[InlineData("0.1.0", false, true, "0.2.0")]
		[InlineData("0.1.0", true, false, "0.2.0")]
		[InlineData("0.1.0", true, true, "0.2.0")]
		[InlineData("0.1.0-beta1", false, false, "0.1.0")]
		[InlineData("0.1.0-beta1", false, true, "0.1.0")]
		[InlineData("0.1.0-beta1", true, false, "0.2.0")]
		[InlineData("0.1.0-beta1", true, true, "0.2.0")]
		[InlineData("0.1.1-beta1", false, false, "0.1.1")]
		[InlineData("0.1.1-beta1", false, true, "0.2.0")]
		[InlineData("0.1.1-beta1", true, false, "0.2.0")]
		[InlineData("0.1.1-beta1", true, true, "0.2.0")]
		[InlineData("1.0.0-beta1", false, false, "1.0.0")]
		[InlineData("1.0.0-beta1", false, true, "1.0.0")]
		[InlineData("1.0.0-beta1", true, false, "1.0.0")]
		[InlineData("1.0.0-beta1", true, true, "1.0.0")]
		[InlineData("1.0.1-beta1", false, false, "1.0.1")]
		[InlineData("1.0.1-beta1", false, true, "1.1.0")]
		[InlineData("1.0.1-beta1", true, false, "2.0.0")]
		[InlineData("1.0.1-beta1", true, true, "2.0.0")]
		[InlineData("1.1.0-beta1", false, false, "1.1.0")]
		[InlineData("1.1.0-beta1", false, true, "1.1.0")]
		[InlineData("1.1.0-beta1", true, false, "2.0.0")]
		[InlineData("1.1.0-beta1", true, true, "2.0.0")]
		[InlineData("1.1.1-beta1", false, false, "1.1.1")]
		[InlineData("1.1.1-beta1", false, true, "1.2.0")]
		[InlineData("1.1.1-beta1", true, false, "2.0.0")]
		[InlineData("1.1.1-beta1", true, true, "2.0.0")]
		public void TestSuggestVersion(string startingVersion, bool hasBreakingChanges, bool hasNonBreakingChanges, string expectedVersion)
		{
			var changes = new List<Change>();
			if (hasBreakingChanges)
				changes.Add(Change.Breaking("breaking"));
			if (hasNonBreakingChanges)
				changes.Add(Change.NonBreaking("non-breaking"));

			var suggestedVersion = PackageDiff.SuggestVersion(new NuGetVersion(startingVersion), changes);
			Assert.Equal(expectedVersion, suggestedVersion.ToString());
		}

		[Fact]
		public async Task TestComparePackageTypes()
		{
			using var packageHelper = new LocalPackageHelper();

			var package1 = await packageHelper.GetLocalPackageAsync("TestLibrary.1.0.0.nupkg").ConfigureAwait(false);
			var package2 = await packageHelper.GetLocalPackageAsync("TestLibrary.2.0.0.nupkg").ConfigureAwait(false);
			PackageDiff.ComparePackageTypes(package1, package2, out var suggestedVersion);

			Assert.Equal(new NuGetVersion("2.0.0"), suggestedVersion);
		}
	}
}
