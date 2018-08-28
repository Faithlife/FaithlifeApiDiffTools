using System;
using System.IO;
using CommandLine;
using Mono.Cecil;

namespace Faithlife.FacadeGenerator
{
	class MainClass
	{
		public static int Main(string[] args)
		{
			return Parser.Default.ParseArguments<Options>(args)
				.MapResult(Run, errors => 1);
		}

		static int Run(Options options)
		{
			var module = CecilUtility.ReadModule(options.InputFile);

			FacadeModuleProcessor.MakePublicFacade(module);

			if (options.TargetFramework != null)
			{
				var attrType = typeof(System.Runtime.Versioning.TargetFrameworkAttribute);
				module.Assembly.CustomAttributes.RemoveAll(x => x.AttributeType.FullName == attrType.FullName);

				var attributeConstructor = module.ImportReference(attrType.GetConstructor(new[] { typeof(string) }));
				var attribute = new CustomAttribute(attributeConstructor);
				attribute.ConstructorArguments.Add(new CustomAttributeArgument(module.TypeSystem.String, options.TargetFramework));
				var frameworkDisplayName = options.TargetFramework.StartsWith(".NETPortable", StringComparison.Ordinal) ? ".NET Portable Subset" : "";
				attribute.Properties.Add(new CustomAttributeNamedArgument("FrameworkDisplayName", new CustomAttributeArgument(module.TypeSystem.String, frameworkDisplayName)));
				module.Assembly.CustomAttributes.Add(attribute);
			}

			var outputFile = options.OutputFile ?? Path.GetFileNameWithoutExtension(options.InputFile) + ".facade.dll";
			Console.WriteLine("Writing {0}", outputFile);
			module.Write(outputFile);

			return 0;
		}

		class Options
		{
			[Value(0)]
			public string InputFile { get; set; }

			[Option('o', "outputFile", HelpText = "Output file.")]
			public string OutputFile { get; set; }

			[Option('t', "targetFramework")]
			public string TargetFramework { get; set; }
		}
	}
}
