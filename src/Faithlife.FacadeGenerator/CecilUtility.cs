using System.IO;
using Mono.Cecil;
using System;

namespace Faithlife.FacadeGenerator
{
	public static class CecilUtility
	{
		public static DefaultAssemblyResolver CreateDefaultAssemblyResolver()
		{
			var resolver = new DefaultAssemblyResolver();
			var platform = Environment.OSVersion.Platform;
			if (platform == PlatformID.MacOSX || platform == PlatformID.Unix)
			{
				resolver.AddSearchDirectory("/Library/Frameworks/Xamarin.Mac.framework/Versions/Current/lib/mono");
				resolver.AddSearchDirectory("/Library/Frameworks/Xamarin.iOS.framework/Versions/Current/lib/mono/2.1");
				resolver.AddSearchDirectory("/Library/Frameworks/Xamarin.iOS.framework/Versions/Current/lib/mono/2.1/Facades");
				resolver.AddSearchDirectory("/Library/Frameworks/Xamarin.iOS.framework/Versions/Current/lib/mono/Xamarin.iOS");
				resolver.AddSearchDirectory("/Library/Frameworks/Xamarin.Android.framework/Versions/Current/lib/mono");
				resolver.AddSearchDirectory("/Library/Frameworks/Xamarin.Android.framework/Versions/Current/lib/mono/2.1");
				resolver.AddSearchDirectory("/Library/Frameworks/Xamarin.Android.framework/Versions/Current/lib/mandroid");
			}
			return resolver;
		}

		public static ModuleDefinition ReadModule(string path)
		{
			var resolver = CreateDefaultAssemblyResolver();
			resolver.AddSearchDirectory(Path.GetDirectoryName(path));

			var readerParameters = new ReaderParameters(ReadingMode.Deferred);
			readerParameters.AssemblyResolver = resolver;

			return ModuleDefinition.ReadModule(path, readerParameters);
		}
	}
}
