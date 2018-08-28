using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace FacadeGenerator
{
	public static class FacadeModuleProcessor
	{
		public static void MakePublicFacade(ModuleDefinition module)
		{
			var keepInternalTypes = module.Assembly.CustomAttributes.Any(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.InternalsVisibleToAttribute");
			MakePublicFacade(module, keepInternalTypes);
		}

		public static void MakePublicFacade(ModuleDefinition module, bool keepInternalTypes)
		{
			module.Types.RemoveAll(x => !(x.IsPublic || (keepInternalTypes && x.IsNotPublic)));
			foreach (var type in module.Types)
				ProcessType(type, keepInternalTypes);
		}

		static void ProcessType(TypeDefinition type, bool keepInternalTypes)
		{
			type.Interfaces.RemoveAll(x =>
			{
				try
				{
					var t = x.InterfaceType.Resolve();
					return t != null && !(t.IsPublic || (keepInternalTypes && t.IsNotPublic));
				}
				catch (AssemblyResolutionException)
				{
					// assume that if the type can't be resolved, it isn't private
					return false;
				}
				catch (BadImageFormatException)
				{
					// assume that if the type can't be resolved, it isn't private
					return false;
				}
			});
			type.Fields.RemoveAll(x => !(x.IsPublic || x.IsFamilyOrAssembly || x.IsFamily || (keepInternalTypes && (x.IsAssembly || x.IsFamilyAndAssembly))));
			type.Methods.RemoveAll(x => !(x.IsPublic || x.IsFamilyOrAssembly || x.IsFamily || (keepInternalTypes && (x.IsAssembly || x.IsFamilyAndAssembly))));
			type.Properties.RemoveAll(x => x.GetMethod == null && x.SetMethod == null);

			foreach (var method in type.Methods)
				ProcessMethod(method);

			type.NestedTypes.RemoveAll(x => !(x.IsNestedPublic || x.IsNestedFamilyOrAssembly || x.IsNestedFamily || (keepInternalTypes && (x.IsNestedAssembly || x.IsNestedFamilyAndAssembly))));
			foreach (var nestedType in type.NestedTypes)
				ProcessType(nestedType, keepInternalTypes);
		}

		static void ProcessMethod(MethodDefinition method)
		{
			if (!method.HasBody)
				return;

			method.Body = new MethodBody(method);

			var exceptionCtor = typeof(InvalidOperationException).GetConstructor(new Type[]{});
			var constructorReference = method.Module.ImportReference(exceptionCtor);

			var ilProcessor = method.Body.GetILProcessor();
			ilProcessor.Append(ilProcessor.Create(OpCodes.Newobj, constructorReference));
			ilProcessor.Append(ilProcessor.Create(OpCodes.Throw));
		}
	}
}
