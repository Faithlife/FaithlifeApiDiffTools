using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Versioning;
using ApiDiffTool;
using FacadeGenerator;
using NuGet;

namespace PackageDiffTool
{
	public static class PackageDiff
	{
		public static Dictionary<FrameworkName, ReadOnlyCollection<Change>> ComparePackages(IPackage package1, IPackage package2, out SemanticVersion suggestedVersion)
		{
			return ComparePackageTypes(package1, package2, out suggestedVersion).ToDictionary(x => x.Key, x => x.Value.SelectMany(y => y.Changes).ToList().AsReadOnly());
		}

		public static Dictionary<FrameworkName, ReadOnlyCollection<TypeChanges>> ComparePackageTypes(IPackage package1, IPackage package2, out SemanticVersion suggestedVersion)
		{
			if (package1 == null)
				throw new ArgumentNullException("package1");
			if (package2 == null)
				throw new ArgumentNullException("package2");
			
			var frameworkChanges = new Dictionary<FrameworkName, ReadOnlyCollection<TypeChanges>>();
			foreach (var targetFramework in package1.GetSupportedFrameworks())
			{
				var typeChanges = new List<TypeChanges>();
				frameworkChanges.Add(targetFramework, typeChanges.AsReadOnly());
				var dlls1 = package1.GetAssemblyReferences(targetFramework).Cast<PhysicalPackageFile>().ToList();
				var dlls2 = package2.GetAssemblyReferences(targetFramework).Cast<PhysicalPackageFile>().ToList();
				if (dlls1.Count != 0 && dlls2.Count == 0)
				{
					
					typeChanges.Add(new TypeChanges(null, new[] { Change.NonBreaking("Framework support removed: {0}", targetFramework) }.ToList().AsReadOnly()));
					continue;
				}
				var changes = new List<Change>();
				foreach (var file1 in dlls1)
				{
					var file2 = dlls2.FirstOrDefault(x => x.EffectivePath == file1.EffectivePath);
					if (file2 != null)
					{
						typeChanges.AddRange(FindChanges(file1.SourcePath, file2.SourcePath));
					}
					else
					{
						changes.Add(Change.Breaking("Assembly removed: {0}", file1.EffectivePath));
					}
				}
				if (!changes.IsEmpty())
					typeChanges.Add(new TypeChanges(null, changes.AsReadOnly()));
			}

			suggestedVersion = SuggestVersion(package1.Version, frameworkChanges.SelectMany(x => x.Value.SelectMany(y => y.Changes)).ToList());

			return frameworkChanges;
		}

		public static SemanticVersion SuggestVersion(SemanticVersion startingVersion, IReadOnlyCollection<Change> changes)
		{
			var major = startingVersion.Version.Major;
			var minor = startingVersion.Version.Minor;
			var build = startingVersion.Version.Build;

			if (changes.Count == 0)
			{
				build++;
			}
			else if (changes.All(x => !x.IsBreaking))
			{
				minor++;
				build = 0;
			}
			else
			{
				major++;
				minor = 0;
				build = 0;
			}

			return new SemanticVersion(major, minor, build, null);
		}

		static ReadOnlyCollection<TypeChanges> FindChanges(string path1, string path2)
		{
			var module1 = CecilUtility.ReadModule(path1);
			var module2 = CecilUtility.ReadModule(path2);

			FacadeModuleProcessor.MakePublicFacade(module1, keepInternalTypes: false);
			FacadeModuleProcessor.MakePublicFacade(module2, keepInternalTypes: false);

			return ApiDiff.FindTypeChanges(module1, module2);
		}
	}

	static class PackageUtility
	{
		public static IEnumerable<IPackageAssemblyReference> GetAssemblyReferences(this IPackage package, FrameworkName targetFramework)
		{
			IEnumerable<IPackageAssemblyReference> compatibleItems;
			if (VersionUtility.TryGetCompatibleItems(targetFramework, package.AssemblyReferences, out compatibleItems))
				return compatibleItems;
			return Enumerable.Empty<IPackageAssemblyReference>();
		}
	}
}
