using System;

#pragma warning disable 0169
#pragma warning disable 0649

namespace TestLibrary
{
	public interface IPublicInterface
	{
		void PublicInterfaceMethod1();
	}

	internal interface IInternalInterface
	{
		void InternalInterfaceMethod1();
	}

	public class PublicClass : IPublicInterface, IInternalInterface
	{
		public void PublicInterfaceMethod1() {}

		public void InternalInterfaceMethod1() {}

		internal void InternalMethod1() {}

		protected void ProtectedMethod1() {}

		protected internal void ProtectedInternalMethod1() {}

//		private protected void PrivateProtectedMethod1() {}

		private void PrivateMethod1() {}

		protected class ProtectedNestedClass1
		{
		}

		internal class InternalNestedClass1
		{
		}

		protected internal class ProtectedInternalNestedClass1
		{
		}

//		private protected class PrivateProtectedNestedClass1
//		{
//		}

		private class PrivateNestedClass1
		{
		}

		internal int m_internalField1;
		private int m_privateField1;
	}

	internal class InternalClass : IPublicInterface, IInternalInterface
	{
		public void PublicInterfaceMethod1() {}
		public void InternalInterfaceMethod1() {}

		internal int m_internalField1;
		private int m_privateField1;
	}

	public class PublicGenericClass<T>
	{
		public T DoSomething() { return default(T); }
	}

	public class PublicGenericClass<T1, T2>
	{
		public T1 DoSomething(T2 t2) { return default(T1); }
	}
}
