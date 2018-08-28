using System;
using System.Collections.Generic;
using System.Linq;

namespace FacadeGenerator
{
	static class CollectionUtility
	{
		public static void RemoveAll<T>(this ICollection<T> collection, Func<T, bool> condition)
		{
			var toRemove = collection.Where(condition).ToList();
			foreach (var x in toRemove)
				collection.Remove(x);
		}
	}
}
