using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Faithlife.ApiDiffTool;
using Faithlife.FacadeGenerator;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Versioning;

namespace Faithlife.PackageDiffTool
{
	public static class PackageDiff
	{
		public static IReadOnlyDictionary<NuGetFramework, IReadOnlyList<TypeChanges>> ComparePackageTypes(PackageReaderBase package1, PackageReaderBase package2, out NuGetVersion suggestedVersion)
		{
			if (package1 == null)
				throw new ArgumentNullException(nameof(package1));
			if (package2 == null)
				throw new ArgumentNullException(nameof(package2));

			var frameworkChanges = new Dictionary<NuGetFramework, IReadOnlyList<TypeChanges>>();
			foreach (var targetFramework in package1.GetSupportedFrameworks())
			{
				var typeChanges = new List<TypeChanges>();
				frameworkChanges.Add(targetFramework, typeChanges.AsReadOnly());
				var dlls1 = package1.GetAssemblyReferences(targetFramework).ToList();
				var dlls2 = package2.GetAssemblyReferences(targetFramework).ToList();
				if (dlls1.Count != 0 && dlls2.Count == 0)
				{
					typeChanges.Add(new TypeChanges(null, new[] { Change.NonBreaking("Framework support removed: {0}", targetFramework) }.ToList().AsReadOnly()));
					continue;
				}
				var changes = new List<Change>();
				foreach (var file1 in dlls1)
				{
					if (dlls2.Contains(file1))
					{
						typeChanges.AddRange(FindChanges(package1.GetStream(file1), package2.GetStream(file1)));
					}
					else
					{
						changes.Add(Change.Breaking("Assembly removed: {0}", file1));
					}
				}
				if (changes.Count != 0)
					typeChanges.Add(new TypeChanges(null, changes.AsReadOnly()));
			}

			suggestedVersion = SuggestVersion(package1.GetIdentity().Version, frameworkChanges.SelectMany(x => x.Value.SelectMany(y => y.Changes)).ToList());

			return frameworkChanges;
		}

		public static NuGetVersion SuggestVersion(NuGetVersion startingVersion, IReadOnlyCollection<Change> changes)
		{
			var major = startingVersion.Major;
			var minor = startingVersion.Minor;
			var patch = startingVersion.Patch;
			var isPrerelease = !string.IsNullOrEmpty(startingVersion.Release);

			if (changes.Count == 0)
			{
				if (!isPrerelease)
					patch++;
			}
			else if (changes.All(x => !x.IsBreaking) || major == 0)
			{
				if (!(isPrerelease && patch == 0) || (major == 0 && changes.Any(x => x.IsBreaking)))
				{
					minor++;
					patch = 0;
				}
			}
			else
			{
				if (!(isPrerelease && patch == 0 && minor == 0))
				{
					major++;
					minor = 0;
					patch = 0;
				}
			}

			return new NuGetVersion(major, minor, patch, null);
		}

		static IReadOnlyList<TypeChanges> FindChanges(Stream stream1, Stream stream2)
		{
			var module1 = CecilUtility.ReadModule(stream1);
			var module2 = CecilUtility.ReadModule(stream2);

			FacadeModuleProcessor.MakePublicFacade(module1, keepInternalTypes: false);
			FacadeModuleProcessor.MakePublicFacade(module2, keepInternalTypes: false);

			return ApiDiff.FindTypeChanges(module1, module2);
		}
	}

	static class PackageUtility
	{
		public static IEnumerable<string> GetAssemblyReferences(this PackageReaderBase package, NuGetFramework targetFramework)
		{
			return package.GetReferenceItems().Where(x => x.TargetFramework == targetFramework).SelectMany(x => x.Items);
		}
	}
}
