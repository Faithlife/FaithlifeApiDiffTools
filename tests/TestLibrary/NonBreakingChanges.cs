using System;
using System.Collections.Generic;

namespace TestLibrary.NonBreakingChanges
{
	#if V1
	internal
	#else
	public
	#endif
	class ClassMadePublic
	{
		#if V1
		private
		#else
		public
		#endif
		void MethodMadePublic()
		{
		}

		#if !V1
		public void MethodAdded(string foo) {}
		#endif

		public string PropertySetterAdded
		{
			get;
			#if !V1
			set;
			#endif
		}

	}

	#if V1
	public class GenericClass<TClass>
	#else
	public class GenericClass<TClass1>
	#endif
	{
		#if V1
		public TClass Echo(TClass message)
		#else
		public TClass1 Echo(TClass1 message)
		#endif
		{
			return message;
		}
	}

	public class GenericMethodsClass
	{
		#if V1
		public T GenericMethodTypeAliasChanged1<T>(T param)
		#else
		public T1 GenericMethodTypeAliasChanged1<T1>(T1 param)
		#endif
		{
			return param;
		}

		#if V1
		public IEnumerable<KeyValuePair<T1, T2>> GenericMethodTypeAliasChanged2<T1, T2>(IEnumerable<T1> param1, IEnumerable<T2> param2)
		#else
		public IEnumerable<KeyValuePair<TKey, TValue>> GenericMethodTypeAliasChanged2<TKey, TValue>(IEnumerable<TKey> param1, IEnumerable<TValue> param2)
		#endif
		{
			return null;
		}

		public void GenericMethodOverloadAdded<T1>() {}

		#if !V1
		public void GenericMethodOverloadAdded<T1, T2>() {}
		#endif
	}

	public class Overloads
	{
		#if V1
		public Overloads(string a, string b = null) {}
		#else
		public Overloads(string a, string b) {}
		public Overloads(string a, string b = null, string c = null) {}
		#endif

		#if V1
		public void Method(string a, string b = null) {}
		#else
		public void Method(string a, string b) {}
		public void Method(string a, string b = null, string c = null) {}
		#endif
	}

	public class Fields
	{
		#if V1
		protected
		#else
		public
		#endif
		string FieldMadePublic;

		#if !V1
		public string FieldAdded;
		#endif
	}

	public enum Enum
	{
		A,
		B,
		#if !V1
		C,
		#endif
	}

	public class Convertible
	{
		#if V1

		public static explicit operator int(Convertible c)
		{
			throw new NotImplementedException();
		}

		public static explicit operator long(Convertible c)
		{
			throw new NotImplementedException();
		}

		public static explicit operator float(Convertible c)
		{
			throw new NotImplementedException();
		}

		public static explicit operator double(Convertible c)
		{
			throw new NotImplementedException();
		}

		public static implicit operator Convertible(int n)
		{
			throw new NotImplementedException();
		}

		public static implicit operator Convertible(long n)
		{
			throw new NotImplementedException();
		}

		public static implicit operator Convertible(float n)
		{
			throw new NotImplementedException();
		}

		public static implicit operator Convertible(double n)
		{
			throw new NotImplementedException();
		}

		#else

		public static implicit operator Convertible(float n)
		{
			throw new NotImplementedException();
		}

		public static implicit operator Convertible(double n)
		{
			throw new NotImplementedException();
		}

		public static implicit operator Convertible(int n)
		{
			throw new NotImplementedException();
		}

		public static implicit operator Convertible(long n)
		{
			throw new NotImplementedException();
		}

		public static explicit operator int(Convertible c)
		{
			throw new NotImplementedException();
		}

		public static explicit operator long(Convertible c)
		{
			throw new NotImplementedException();
		}

		public static explicit operator float(Convertible c)
		{
			throw new NotImplementedException();
		}

		public static explicit operator double(Convertible c)
		{
			throw new NotImplementedException();
		}

		#endif
	}
}
