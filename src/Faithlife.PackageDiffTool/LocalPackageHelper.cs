using System;
using System.IO;
using NuGet;

namespace Faithlife.PackageDiffTool
{
	class LocalPackageHelper
	{
		public LocalPackageHelper()
			: this(Path.Combine(Path.GetTempPath(), "apidiffpackages"))
		{
		}

		public LocalPackageHelper(string path)
		{
			m_repository = CreateRepository();
			m_packageManager = CreatePackageManager(new PhysicalFileSystem(path), useSideBySidePaths: true);
		}

		public ILogger Logger { get; set; }

		public IPackage GetPackage(string packageId, SemanticVersion version)
		{
			// if a version is specified, check locally, otherwise check all sources for the latest
			IPackage package = null;
			if (version != null)
				package = m_packageManager.LocalRepository.FindPackage(packageId, version);
			if (package == null)
			{
				package = m_repository.FindPackage(packageId, version);
				if (package != null)
					m_packageManager.InstallPackage(package, ignoreDependencies: true, allowPrereleaseVersions: true);
			}
			return package;
		}

		public IPackage GetLocalPackage(string packagePath)
		{
			if (packagePath.StartsWith("~/", StringComparison.Ordinal))
				packagePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), packagePath.Substring(2));
			else
				packagePath = Path.GetFullPath(packagePath);
			return new OptimizedZipPackage(packagePath);
		}

		IPackageRepository CreateRepository()
		{
			var settings = Settings.LoadDefaultSettings(null, null, null);
			var packageSourceProvider = new PackageSourceProvider(settings);
			var repository = packageSourceProvider.CreateAggregateRepository(PackageRepositoryFactory.Default, true);
			if (Logger != null)
				repository.Logger = Logger;
			return repository;
		}

		IPackageManager CreatePackageManager(IFileSystem packagesFolderFileSystem, bool useSideBySidePaths)
		{
			var repository = CreateRepository();
			var pathResolver = new DefaultPackagePathResolver(packagesFolderFileSystem, useSideBySidePaths);
			var localRepository = new SharedPackageRepository(
				pathResolver, 
				packagesFolderFileSystem, 
				configSettingsFileSystem: NullFileSystem.Instance);

			var packageManager = new PackageManager(repository, pathResolver, packagesFolderFileSystem, localRepository)
			{
				CheckDowngrade = false
			};
			if (Logger != null)
				packageManager.Logger = Logger;

			return packageManager;
		}

		readonly IPackageRepository m_repository;
		readonly IPackageManager m_packageManager;
	}
}
