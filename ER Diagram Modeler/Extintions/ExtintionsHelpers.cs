using System;
using System.Collections.Generic;
using System.Linq;

namespace ER_Diagram_Modeler.Extintions
{
	public static class ExtintionsHelpers
	{
		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> items, Random random)
		{
			T[] arr = items.ToArray();

			for (var i = arr.Length - 1; i >= 0; i--)
			{
				int swapIndex = random.Next(i + 1);
				yield return arr[swapIndex];
				arr[swapIndex] = arr[i];
			}
		}
	}
}