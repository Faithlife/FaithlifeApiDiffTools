using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Faithlife.ApiDiffTool
{
	public static class ApiDiff
	{
		public static ReadOnlyCollection<TypeChanges> FindTypeChanges(ModuleDefinition module1, ModuleDefinition module2)
		{
			var typeChanges = new List<TypeChanges>();
			foreach (var type1 in module1.Types)
			{
				var changes = new List<Change>();
				var type2 = module2.Types.FirstOrDefault(x => x.FullName == type1.FullName);
				if (type2 == null)
					changes.Add(Change.Breaking("Type removed: {0}", type1.FullName));
				else
					changes.AddRange(FindChanges(type1.Resolve(), type2.Resolve()));
				typeChanges.Add(new TypeChanges(type1, changes.AsReadOnly()));
			}

			foreach (var type2 in module2.Types)
			{
				var type1 = module1.Types.FirstOrDefault(x => x.FullName == type2.FullName);
				if (type1 == null)
				{
					var changes = new List<Change>();
					changes.Add(Change.NonBreaking("Type added: {0}", type2));
					typeChanges.Add(new TypeChanges(type2, changes.AsReadOnly()));
				}
			}

			return typeChanges.AsReadOnly();
		}

		public static ReadOnlyCollection<Change> FindChanges(ModuleDefinition module1, ModuleDefinition module2)
		{
			return FindTypeChanges(module1, module2).SelectMany(x => x.Changes).ToList().AsReadOnly();
		}

		public static ReadOnlyCollection<Change> FindChanges(TypeDefinition type1, TypeDefinition type2)
		{
			var changes = new List<Change>();

			if (type1.Attributes != type2.Attributes)
			{
				if (!type1.IsInterface && type2.IsInterface)
					changes.Add(Change.Breaking("Type made interface: {0}", type1));
				else if (type1.IsInterface && !type2.IsInterface)
					changes.Add(Change.Breaking("Type made non-interface: {0}", type1));

				if (!type1.IsEnum && type2.IsEnum)
					changes.Add(Change.Breaking("Type made enum: {0}", type1));
				else if (type1.IsEnum && !type2.IsEnum)
					changes.Add(Change.Breaking("Type made non-enum: {0}", type1));

				if (!type1.IsValueType && type2.IsValueType)
					changes.Add(Change.Breaking("Type made value-type: {0}", type1));
				else if (type1.IsValueType && !type2.IsValueType)
					changes.Add(Change.Breaking("Type made non-value-type: {0}", type1));

				if (!type1.IsAbstract && type2.IsAbstract)
					changes.Add(Change.Breaking("Type made abstract: {0}", type1));
				else if (type1.IsAbstract && !type2.IsAbstract)
					changes.Add(Change.NonBreaking("Type made non-abstract: {0}", type1));

				if (!type1.IsSealed && type2.IsSealed)
					changes.Add(Change.Breaking("Type made sealed: {0}", type1));
				else if (type1.IsSealed && !type2.IsSealed)
					changes.Add(Change.NonBreaking("Type made non-sealed: {0}", type1));

				if (type1.IsPublic && !type2.IsPublic)
					changes.Add(Change.Breaking("Type made non-public: {0}", type1));
				else if (!type1.IsPublic && type2.IsPublic)
					changes.Add(Change.NonBreaking("Type made public: {0}", type1));

				if (type1.IsNestedPublic && !type2.IsNestedPublic)
					changes.Add(Change.Breaking("Type made non-public: {0}", type1));
				else if (!type1.IsNestedPublic && type2.IsNestedPublic)
					changes.Add(Change.NonBreaking("Type made public: {0}", type1));
			}

			foreach (var iface1 in type1.Interfaces)
			{
				var iface2 = type2.Interfaces.FirstOrDefault(x => x.InterfaceType.FullName == iface1.InterfaceType.FullName);
				if (iface2 == null)
					changes.Add(Change.Breaking("Interface removed from type: {0} : {1}", type1, iface1));
			}

			foreach (var iface2 in type2.Interfaces)
			{
				var iface1 = type1.Interfaces.FirstOrDefault(x => x.InterfaceType.FullName == iface2.InterfaceType.FullName);
				if (iface1 == null)
					changes.Add(Change.NonBreaking("Interface added to type: {0} : {1}", type1, iface2));
			}

			changes.AddRange(FindGenericParameterChanges(type1, type1.GenericParameters, type2.GenericParameters));

			foreach (var property1 in type1.Properties)
			{
				var property2 = FindMatchingProperty(type2.Properties, property1);
				if (property2 == null)
					changes.Add(Change.Breaking("Property removed: {0}", property1.FullName));
				else
					changes.AddRange(FindChanges(property1, property2));
			}

			foreach (var property2 in type2.Properties)
			{
				var property1 = FindMatchingProperty(type1.Properties, property2);
				if (property1 == null)
					changes.Add(Change.NonBreaking("Property added: {0}", property2.FullName));
			}

			foreach (var method1 in type1.Methods.Where(x => !x.IsGetter && !x.IsSetter))
			{
				var method2 = FindMatchingMethod(type2.Methods, method1);
				if (method2 == null)
				{
					changes.Add(Change.Breaking("Method removed: {0}", method1.FullName));
				}
				else
				{
					changes.AddRange(FindChanges(method1, method2));

					if (method1.Parameters.Any(x => x.HasDefault))
					{
						var method2WithoutDefaultParameters = FindMatchingMethod(type2.Methods, method1, withoutDefaultParameters: true);
						if (method2WithoutDefaultParameters == null)
						{
							changes.Add(Change.Breaking("Method default parameters removed: {0}", method1.FullName));
						}
					}
				}
			}

			foreach (var method2 in type2.Methods.Where(x => !x.IsGetter && !x.IsSetter))
			{
				var method1 = FindMatchingMethod(type1.Methods, method2);
				if (method1 == null)
					changes.Add(Change.NonBreaking("Method added: {0}", method2.FullName));
			}

			foreach (var nestedType1 in type1.NestedTypes)
			{
				var nestedType2 = type2.NestedTypes.FirstOrDefault(x => x.FullName == nestedType1.FullName);
				if (nestedType2 == null)
					changes.Add(Change.Breaking("Type removed: {0}", nestedType1.FullName));
				else
					changes.AddRange(FindChanges(nestedType1.Resolve(), nestedType2.Resolve()));
			}

			foreach (var nestedType2 in type2.NestedTypes)
			{
				var nestedType1 = type1.NestedTypes.FirstOrDefault(x => x.FullName == nestedType2.FullName);
				if (nestedType1 == null)
					changes.Add(Change.NonBreaking("Type added: {0}", nestedType2.FullName));
			}

			if (type1.IsEnum && type2.IsEnum)
			{
				if (!type1.Fields.SequenceEqual(type2.Fields.Take(type1.Fields.Count), EnumFieldComparer.Instance))
					changes.Add(Change.Breaking("Enum values changed: {0}", type1));
				else if (type1.Fields.Count != type2.Fields.Count)
					changes.Add(Change.NonBreaking("Enum values added: {0}", type1));
			}
			else
			{
				foreach (var field1 in type1.Fields)
				{
					var field2 = type2.Fields.FirstOrDefault(x => x.Name == field1.Name);
					if (field2 == null)
						changes.Add(Change.Breaking("Field removed: {0}", field1.FullName));
					else
						changes.AddRange(FindChanges(type1, field1.Resolve(), field2.Resolve()));
				}
			}

			return changes.AsReadOnly();
		}

		static PropertyDefinition FindMatchingProperty(IList<PropertyDefinition> properties, PropertyDefinition property1)
		{
			return properties.FirstOrDefault(x =>
			{
				var namesAreEqual = x.Name == property1.Name;
				var parameterTypesAreEqual = x.Parameters.Select(param => param.ParameterType).SequenceEqual(property1.Parameters.Select(param => param.ParameterType), TypeReferenceComparer.Instance);
				return namesAreEqual && parameterTypesAreEqual;
			});
		}

		static MethodDefinition FindMatchingMethod(IList<MethodDefinition> methods, MethodDefinition method1, bool withoutDefaultParameters = false)
		{
			var methodName = method1.GetNormalizedFullName(includeReturnType: false, withoutDefaultParameters: withoutDefaultParameters);
			// TODO: find best matching generic parameters
			var matchingMethods = methods.Where(x =>
			{
				var namesAreEqual = x.GetNormalizedFullName(includeReturnType: false, withoutDefaultParameters: withoutDefaultParameters) == methodName;
				var parametersAreEqual = x.GenericParameters.SequenceEqual(method1.GenericParameters, GenericParameterComparer.Instance);
				return namesAreEqual && parametersAreEqual;
			});
			return matchingMethods.FirstOrDefault(x => x.ReturnType.GetNormalizedName() == method1.ReturnType.GetNormalizedName()) ?? matchingMethods.FirstOrDefault();
		}

		public static ReadOnlyCollection<Change> FindChanges(PropertyDefinition property1, PropertyDefinition property2)
		{
			var changes = new List<Change>();

			if (!AreEqual(property1.PropertyType, property2.PropertyType))
			{
				changes.Add(Change.Breaking("Property type changed: {0}", property1));
			}
			else
			{
				if (property1.GetMethod != null && property2.GetMethod != null)
					changes.AddRange(FindChanges(property1.GetMethod, property2.GetMethod));
				else if (property1.GetMethod != null && property2.GetMethod == null)
					changes.Add(Change.Breaking("Property getter removed: {0}", property1));
				else if (property1.GetMethod == null && property2.GetMethod != null)
					changes.Add(Change.NonBreaking("Property getter added: {0}", property1));

				if (property1.SetMethod != null && property2.SetMethod != null)
					changes.AddRange(FindChanges(property1.SetMethod, property2.SetMethod));
				else if (property1.SetMethod != null && property2.SetMethod == null)
					changes.Add(Change.Breaking("Property setter removed: {0}", property1));
				else if (property1.SetMethod == null && property2.SetMethod != null)
					changes.Add(Change.NonBreaking("Property setter added: {0}", property1));
			}

			return changes.AsReadOnly();
		}

		public static ReadOnlyCollection<Change> FindChanges(MethodDefinition method1, MethodDefinition method2)
		{
			var changes = new List<Change>();

			if (method1.Attributes != method2.Attributes)
			{
				if (!method1.IsAbstract && method2.IsAbstract)
					changes.Add(Change.Breaking("Method made abstract: {0}", method1));
				else if (method1.IsAbstract && !method2.IsAbstract && !method2.IsVirtual)
					changes.Add(Change.Breaking("Method made non-abstract: {0}", method1));
				else if (method1.IsAbstract && !method2.IsAbstract && method2.IsVirtual)
					changes.Add(Change.NonBreaking("Method made non-abstract but virtual: {0}", method1));

				if (!method1.IsVirtual && method2.IsVirtual)
					changes.Add(Change.Breaking("Method made virtual: {0}", method1));
				else if (method1.IsVirtual && !method2.IsVirtual)
					changes.Add(Change.Breaking("Method made non-virtual: {0}", method1));
				
				if (!method1.IsFinal && method2.IsFinal)
					changes.Add(Change.Breaking("Method made sealed: {0}", method1));
				else if (method1.IsFinal && !method2.IsFinal)
					changes.Add(Change.NonBreaking("Method made non-sealed: {0}", method1));

				if (!method1.IsStatic && method2.IsStatic)
					changes.Add(Change.Breaking("Method made static: {0}", method1));
				else if (method1.IsStatic && !method2.IsStatic)
					changes.Add(Change.Breaking("Method made non-static: {0}", method1));

				if (method1.IsPublic && !method2.IsPublic)
					changes.Add(Change.Breaking("Method made non-public: {0}", method1));
				else if (!method1.IsPublic && method2.IsPublic)
					changes.Add(Change.NonBreaking("Method made public: {0}", method1));
			}

			if (!AreEqual(method1.ReturnType, method2.ReturnType))
				changes.Add(Change.Breaking("Method return type changed: {0}", method1));

			changes.AddRange(FindParameterAttributeChanges(method1, method1.Parameters, method2.Parameters));
			changes.AddRange(FindGenericParameterChanges(method1, method1.GenericParameters, method2.GenericParameters));

			return changes.AsReadOnly();
		}

		public static ReadOnlyCollection<Change> FindParameterAttributeChanges(object obj, IList<ParameterDefinition> parameters1, IList<ParameterDefinition> parameters2)
		{
			var changes = new List<Change>();

			if (parameters1.Count != parameters2.Count)
			{
				// Probably the wrong method was selected.
				changes.Add(Change.Breaking("Parameter count changed: {0}", obj));
			}
			else
			{
				for (var i = 0; i < parameters1.Count; i++)
				{
					var param1 = parameters1[i];
					var param2 = parameters2[i];

					if (param1.Name != param2.Name)
						changes.Add(Change.Breaking("Parameter name changed: {0} {1}", obj, param1));

					if (!AreEqual(param1.ParameterType, param2.ParameterType) && !param1.ParameterType.IsGenericParameter)
						changes.Add(Change.Breaking("Parameter type changed: {0} {1}", obj, param1));

					// TODO: test attributes for non-breaking changes
					if ((param1.Attributes & ~OptionalWithDefault) != (param2.Attributes & ~OptionalWithDefault))
						changes.Add(Change.Breaking("Parameter attributes changed: {0} {1}", obj, param1));
					
					if (param1.HasDefault && !param2.HasDefault)
						changes.Add(Change.NonBreaking("Default parameter value removed: {0} {1}", obj, param1)); // maybe breaking
					else if (!param1.HasDefault && param2.HasDefault)
						changes.Add(Change.NonBreaking("Default parameter value added: {0} {1}", obj, param2));

					if (!object.Equals(param1.Constant, param2.Constant))
						changes.Add(Change.NonBreaking("Default parameter value changed: {0} {1}", obj, param1));
				}
			}

			return changes.AsReadOnly();
		}

		public static ReadOnlyCollection<Change> FindGenericParameterChanges(object obj, IList<GenericParameter> parameters1, IList<GenericParameter> parameters2)
		{
			var changes = new List<Change>();

			if (parameters1.Count != parameters2.Count)
			{
				// Probably the wrong method was selected.
				changes.Add(Change.Breaking("Generic parameter count changed: {0}", obj));
			}
			else
			{
				for (var i = 0; i < parameters1.Count; i++)
				{
					var param1 = parameters1[i];
					var param2 = parameters2[i];

					if (param1.Name != param2.Name)
						changes.Add(Change.NonBreaking("Generic parameter name changed: {0} {1}", obj, param1));

					if (!param1.HasConstraints && param2.HasConstraints)
						changes.Add(Change.Breaking("Generic parameter constraints added: {0} {1}", obj, param1));
					else if (param1.HasConstraints && !param2.HasConstraints)
						changes.Add(Change.NonBreaking("Generic parameter constraints removed: {0} {1}", obj, param1));

					if (!param1.HasDefaultConstructorConstraint && param2.HasDefaultConstructorConstraint)
						changes.Add(Change.Breaking("Generic parameter default constructor constraint added: {0} {1}", obj, param1));
					else if (param1.HasDefaultConstructorConstraint && !param2.HasDefaultConstructorConstraint)
						changes.Add(Change.NonBreaking("Generic parameter default constructor constraint removed: {0} {1}", obj, param1));

					if (!param1.HasNotNullableValueTypeConstraint && param2.HasNotNullableValueTypeConstraint)
						changes.Add(Change.Breaking("Generic parameter non-null value-type constraint added: {0} {1}", obj, param1));
					else if (param1.HasNotNullableValueTypeConstraint && !param2.HasNotNullableValueTypeConstraint)
						changes.Add(Change.NonBreaking("Generic parameter non-null value-type constraint removed: {0} {1}", obj, param1));

					if (!param1.HasReferenceTypeConstraint && param2.HasReferenceTypeConstraint)
						changes.Add(Change.Breaking("Generic parameter reference-type constraint added: {0} {1}", obj, param1));
					else if (param1.HasReferenceTypeConstraint && !param2.HasReferenceTypeConstraint)
						changes.Add(Change.NonBreaking("Generic parameter reference-type constraint removed: {0} {1}", obj, param1));
				}
			}

			return changes.AsReadOnly();
		}

		public static ReadOnlyCollection<Change> FindChanges(object obj, FieldDefinition field1, FieldDefinition field2)
		{
			var changes = new List<Change>();

			if (!AreEqual(field1.FieldType, field2.FieldType))
				changes.Add(Change.Breaking("Field type changed: {0} {1}", obj, field1));

			if (field1.IsPublic && !field2.IsPublic)
				changes.Add(Change.Breaking("Field made non-public: {0} {1}", obj, field1));
			else if (!field1.IsPublic && field2.IsPublic)
				changes.Add(Change.NonBreaking("Field made public: {0} {1}", obj, field1));

			if (!field1.IsStatic && field2.IsStatic)
				changes.Add(Change.Breaking("Field made static: {0} {1}", obj, field1));
			else if (field1.IsStatic && !field2.IsStatic)
				changes.Add(Change.Breaking("Field made non-static: {0} {1}", obj, field1));
			
			if (!object.Equals(field1.Constant, field2.Constant))
				changes.Add(Change.NonBreaking("Field value changed: {0} {1}", obj, field1));

			return changes.AsReadOnly();
		}

		private static bool AreEqual(TypeReference type1, TypeReference type2)
		{
			// TODO: handle the case where type1 is a portable facade and type2 is a system framework, e.g. System.Runtime and mscorlib
			return TypeReferenceComparer.Instance.Equals(type1, type2);
		}

		const ParameterAttributes OptionalWithDefault = ParameterAttributes.Optional | ParameterAttributes.HasDefault;
	}

	public class Change
	{
		public static Change Breaking(string messageFormat, params object[] args)
		{
			return new Change
			{
				IsBreaking = true,
				Message = string.Format(messageFormat, args)
			};
		}

		public static Change NonBreaking(string messageFormat, params object[] args)
		{
			return new Change
			{
				IsBreaking = false,
				Message = string.Format(messageFormat, args)
			};
		}

		public bool IsBreaking { get; private set; }

		public string Message { get; private set; }

		private Change()
		{
		}
	}

	public class TypeChanges
	{
		public TypeChanges(TypeDefinition type, ReadOnlyCollection<Change> changes)
		{
			Type = type;
			Changes = changes;
		}

		public TypeDefinition Type { get; private set; }

		public ReadOnlyCollection<Change> Changes { get; private set; }
	}

	internal sealed class GenericParameterComparer : IEqualityComparer<GenericParameter>
	{
		public static readonly GenericParameterComparer Instance = new GenericParameterComparer();

		public bool Equals(GenericParameter x, GenericParameter y)
		{
			var typesAreEqual = x.Type == y.Type;
			var constraintsAreEqual = x.Constraints.SequenceEqual(y.Constraints, TypeReferenceComparer.Instance);
			return typesAreEqual && constraintsAreEqual;
		}

		public int GetHashCode(GenericParameter obj)
		{
			return obj.Type.GetHashCode() + obj.Constraints.GetHashCode();
		}

		private GenericParameterComparer()
		{
		}
	}

	internal sealed class TypeReferenceComparer : IEqualityComparer<TypeReference>
	{
		public static readonly TypeReferenceComparer Instance = new TypeReferenceComparer();

		public bool Equals(TypeReference x, TypeReference y)
		{
			var namesAreEqual = x.GetNormalizedName() == y.GetNormalizedName();
			var assembliesAreEqual = x.Scope.Name == y.Scope.Name;
			return namesAreEqual && assembliesAreEqual;
		}

		public int GetHashCode(TypeReference obj)
		{
			return obj.FullName.GetHashCode() + obj.Scope.Name.GetHashCode();
		}

		private TypeReferenceComparer()
		{
		}
	}

	internal sealed class EnumFieldComparer : IEqualityComparer<FieldDefinition>
	{
		public static readonly EnumFieldComparer Instance = new EnumFieldComparer();

		public bool Equals(FieldDefinition x, FieldDefinition y)
		{
			var namesAreEqual = x.Name == y.Name;
			var constantsAreEqual = object.Equals(x.Constant, y.Constant);
			return namesAreEqual && constantsAreEqual;
		}

		public int GetHashCode(FieldDefinition obj)
		{
			return obj.Name.GetHashCode() + obj.Constant.GetHashCode();
		}

		private EnumFieldComparer()
		{
		}
	}

	internal static class MemberUtility
	{
		public static string GetNormalizedFullName(this MethodReference method, bool includeReturnType = true, bool withoutDefaultParameters = false)
		{
			var stringBuilder = new StringBuilder();
			if (includeReturnType)
				stringBuilder.Append(GetNormalizedName(method.ReturnType)).Append(" ");
			stringBuilder.Append(GetMemberFullName(method));
			stringBuilder.Append(method.GetMethodSignatureFullName(withoutDefaultParameters));
			return stringBuilder.ToString();
		}

		static string GetMemberFullName(MemberReference member)
		{
			if (member.DeclaringType == null)
			{
				return member.Name;
			}
			return member.DeclaringType.FullName + "::" + member.Name;
		}

		static string GetMethodSignatureFullName(this MethodReference method, bool withoutDefaultParameters = false)
		{
			var builder = new StringBuilder();
			builder.Append("(");
			if (method.HasParameters)
			{
				IList<ParameterDefinition> parameters = method.Parameters;
				if (withoutDefaultParameters)
					parameters = parameters.Where(x => !x.HasDefault).ToList();
				for (var i = 0; i < parameters.Count; i++)
				{
					var parameterDefinition = parameters[i];
					if (i > 0)
					{
						builder.Append(",");
					}
					if (parameterDefinition.ParameterType.IsSentinel)
					{
						builder.Append("...,");
					}
					builder.Append(GetNormalizedName(parameterDefinition.ParameterType));
				}
			}
			builder.Append(")");
			return builder.ToString();
		}

		public static string GetNormalizedName(this TypeReference typeReference)
		{
			var genericParam = typeReference as GenericParameter;
			var genericInstance = typeReference as GenericInstanceType;
			if (genericParam != null)
			{
				return (genericParam.Type == GenericParameterType.Method ? "!!" : "!") + genericParam.Position;
			}
			else if (genericInstance != null)
			{
				var stringBuilder = new StringBuilder();
				stringBuilder.Append(GetNormalizedName(genericInstance.GetElementType()));
				genericInstance.GenericInstanceFullName(stringBuilder);
				return stringBuilder.ToString();
			}
			return typeReference.FullName;
		}

		static void GenericInstanceFullName(this IGenericInstance genericInstance, StringBuilder builder)
		{
			builder.Append("<");
			var genericArguments = genericInstance.GenericArguments;
			for (var i = 0; i < genericArguments.Count; i++)
			{
				if (i > 0)
				{
					builder.Append(",");
				}
				builder.Append(GetNormalizedName(genericArguments[i]));
			}
			builder.Append(">");
		}
	}

	static class ListUtility
	{
		public static int IndexOf<T>(this IList<T> list, Func<T, bool> condition)
		{
			for (var i = 0; i < list.Count; i++)
			{
				if (condition(list[i]))
					return i;
			}
			return -1;
		}
	}
}
