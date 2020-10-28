using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using SimpleExec;
using Xunit;

namespace Faithlife.PackageDiffTool.Tool.Tests
{
	public class PackageDiffToolTests
	{
		[Fact]
		public async Task TestTool()
		{
			var packagePath = await DownloadFileAsync("https://globalcdn.nuget.org/packages/faithlife.utility.0.9.0.nupkg");
			var output = await Command.ReadAsync("dotnet", $"run -p ../../../../../src/Faithlife.PackageDiffTool.Tool -- --packageversion 0.8.0 {packagePath}");

			const string expectedOutput = @"0.9.0
.NETFramework,Version=v4.7.2
N Framework support removed: .NETFramework,Version=v4.7.2
.NETStandard,Version=v2.0
B Generic parameter reference-type constraint added: System.Void Faithlife.Utility.CollectionUtility::AddIfNotNull(System.Collections.Generic.ICollection`1<T>,T) T
";
			Assert.Equal(expectedOutput, output);
		}

		private async Task<string> DownloadFileAsync(string url)
		{
			using var httpClient = new HttpClient();
			using var inputStream = await httpClient.GetStreamAsync(url);
			var filePath = Path.Join(Path.GetTempPath(), Path.GetFileName(new Uri(url).LocalPath));
			using var fileStream = File.OpenWrite(filePath);
			await inputStream.CopyToAsync(fileStream);
			return filePath;
		}
	}
}
