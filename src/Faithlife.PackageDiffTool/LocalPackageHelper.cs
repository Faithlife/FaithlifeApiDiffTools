using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.Signing;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Repositories;
using NuGet.Versioning;
using LocalPackageInfo = NuGet.Repositories.LocalPackageInfo;

namespace Faithlife.PackageDiffTool
{
	sealed class LocalPackageHelper : IDisposable
	{
		public LocalPackageHelper()
		{
			m_settings = Settings.LoadDefaultSettings(Directory.GetCurrentDirectory());

			m_globalPackagesFolder = SettingsUtility.GetGlobalPackagesFolder(m_settings);
			m_localRepository = new NuGetv3LocalRepository(m_globalPackagesFolder);

			var psp = new PackageSourceProvider(m_settings);
			var sources = psp.LoadPackageSources();
			m_repositories = sources.Select(x => Repository.Factory.GetCoreV3(x.Source));

			m_tempDirectories = new List<string>();
		}

		public ILogger Logger { get; set; } = NullLogger.Instance;

		public async Task<PackageReaderBase> GetPackageAsync(string packageId, NuGetVersion version, bool includePrerelease, CancellationToken cancellationToken = default)
		{
			// if a version is specified, check locally, otherwise check all sources for the latest
			LocalPackageInfo package = null;
			if (version != null)
				package = m_localRepository.FindPackage(packageId, version);

			if (package != null)
				return await GetLocalPackageAsync(package.ZipPath, cancellationToken).ConfigureAwait(false);

			using (var context = new SourceCacheContext())
			{
				var repoVersions = await Task.WhenAll(m_repositories.Select(async repo =>
				{
					var metadata = await repo.GetResourceAsync<MetadataResource>(cancellationToken).ConfigureAwait(false);
					var ver = await metadata.GetLatestVersion(packageId, includePrerelease, includeUnlisted: false, context, Logger, cancellationToken).ConfigureAwait(false);
					return (repository: repo, version: ver);
				})).ConfigureAwait(false);
				var (repository, latestVersion) = repoVersions.OrderByDescending(x => x.version).FirstOrDefault();

				if (latestVersion != null)
				{
					var packageIdentity = new PackageIdentity(packageId, latestVersion);

					var downloadResource = await repository.GetResourceAsync<DownloadResource>(cancellationToken).ConfigureAwait(false);
					var downloadResult = await downloadResource.GetDownloadResourceResultAsync(
						packageIdentity,
						new PackageDownloadContext(context),
						m_globalPackagesFolder,
						Logger,
						cancellationToken).ConfigureAwait(false);
					return downloadResult.PackageReader;
				}
			}

			return null;
		}

		public async Task<PackageReaderBase> GetLocalPackageAsync(string packagePath, CancellationToken cancellationToken = default)
		{
			if (packagePath.StartsWith("~/", StringComparison.Ordinal))
				packagePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), packagePath.Substring(2));
			else
				packagePath = Path.GetFullPath(packagePath);

			var reader = new PackageArchiveReader(File.OpenRead(packagePath));
			var identity = reader.GetIdentity();
			var rootDirectory = Path.Combine(Path.GetTempPath(), "apidiffpackages", Path.GetRandomFileName());
			m_tempDirectories.Add(rootDirectory);

			var context = new PackageExtractionContext(
				PackageSaveMode.Files | PackageSaveMode.Nuspec,
				XmlDocFileSaveMode.None,
				ClientPolicyContext.GetClientPolicy(m_settings, Logger),
				Logger);

			var resolver = new PackagePathResolver(rootDirectory);

			await PackageExtractor.ExtractPackageAsync(null, reader, resolver, context, cancellationToken).ConfigureAwait(false);

			var packageFolder = resolver.GetInstallPath(identity);
			return new PackageFolderReader(packageFolder);
		}

		public void Dispose()
		{
			foreach (var directory in m_tempDirectories)
			{
				try
				{
					Directory.Delete(directory, true);
				}
				catch (Exception)
				{
				}
			}
		}

		readonly ISettings m_settings;
		readonly string m_globalPackagesFolder;
		readonly NuGetv3LocalRepository m_localRepository;
		readonly IEnumerable<SourceRepository> m_repositories;
		readonly List<string> m_tempDirectories;
	}
}
