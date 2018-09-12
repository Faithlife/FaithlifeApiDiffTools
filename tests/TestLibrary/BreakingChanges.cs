using System;

namespace TestLibrary.BreakingChanges
{
	#if V1
	public
	#else
	internal
	#endif
	class Class1
	#if V1
		: IPublicInterface
	#endif
	{
		public void Method1(
			#if V2
			string arg
			#endif
		)
		{
		}

		#if V1
		public void MethodRemoved() {}
		#endif

		public void PublicInterfaceMethod1()
		{
		}

		protected
		#if !V1
		virtual
		#endif
		void MethodMadeVirtual() {}

		protected
		#if V1
		virtual
		#endif
		void MethodMadeNonVirtual() {}

		#if V1
		protected
		#else
		internal
		#endif
		void MethodChangedProtectedToInternal() {}

		public string PropertySetterRemoved
		{
			get;
			#if V1
			set;
			#endif
		}

		public string PropertySetterMadePrivate
		{
			get;
			#if !V1
			private
			#endif
			set;
		}
	}

	#if V2
	sealed
	#endif
	public class ClassMadeSealed
	{
	}

	#if V2
	abstract
	#endif
	public class ClassMadeAbstract
	{
	}

	public abstract class BaseClass
	{
		public virtual void Method1() {}

		public virtual void Method2() {}
	}

	public class Class4 : BaseClass
	{
		#if V2
		new
		#else
		override
		#endif
		public void Method1() {}

		#if V2
		sealed
		#endif
		public override void Method2() {}
	}

	#if !V1
	static
	#endif
	public class StaticClass
	{
		public static void Method1() {}
	}

	public class StaticMethods
	{
		#if !V1
		static
		#endif
		public void Method1() {}

		#if V1
		static
		#endif
		public void Method2() {}
	}

	public static class ExtensionMethods
	{
		public static void Method1(
			#if !V1
			this
			#endif
			object obj
		) {}

		public static void Method2(
			#if V1
			this
			#endif
			object obj
		) {}
	}

	public class ConstructorVisibility
	{
		#if V1
		public
		#else
		protected
		#endif
		ConstructorVisibility() {}
	}

	public class ClassGenericConstraintAdded<T>
	#if !V1
		where T : new()
	#endif
	{
		public static void MethodGenericConstraintAdded<T1>()
		#if !V1
			where T1 : IPublicInterface
		#endif
		{
		}

		public static void MethodGenericConstraintAdded<T1, T2>()
		#if !V1
			where T1 : class
			where T2 : struct
		#endif
		{
		}
	}

	public class ClassInterfaceImplementation
		: IPublicInterface
	{
		#if V1
		public void PublicInterfaceMethod1()
		#else
		void IPublicInterface.PublicInterfaceMethod1()
		#endif
		{
		}
	}

	public class Parameters
	{
		public void MethodParametersChanged1(
			#if !V1
			out
			#endif
			string foo
		)
		{
			foo = null;
		}

		public void MethodParametersChanged2(
			#if V1
			out
			#else
			ref
			#endif
			string foo
		)
		{
			foo = null;
		}

		public void MethodParametersTypeChanged(
			#if V1
			string
			#else
			int
			#endif
			foo
		)
		{
		}

		public void MethodParameterNameChanged(
			#if V1
			string first,
			string second
			#else
			string one,
			string two
			#endif
		)
		{
		}

		public void MethodDefaultParameterValueAdded(
			string foo
			#if !V1
			= null
			#endif
		)
		{
		}

		public void MethodDefaultParameterValueRemoved(
			string foo
			#if V1
			= null
			#endif
		)
		{
		}

		public void MethodDefaultParameterValueChanged(
			string foo
			#if V1
			= null
			#else
			= "foo"
			#endif
		)
		{
		}
	}

	public class Fields
	{
		#if V1
		public
		#else
		protected
		#endif
		string Field1;

		#if V1
		static
		#endif
		public string Field2;

		#if !V1
		static
		#endif
		public string Field3;
	}

	public enum Enum
	{
		#if V1
		A,
		B,
		#else
		B,
		A,
		#endif
	}

	public interface IInterface1
	{
		#if V1
		string PropertyRemoved { get; set; }
		void MethodRemoved();
		#else
		string PropertyAdded { get; set; }
		void MethodAdded();
		#endif

		string PropertyChanged
		{
			get;
			#if !V1
			set;
			#endif
		}
	}
}
